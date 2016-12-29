using CommandPump.Contract;
using System;
using System.IO;
using System.Messaging;
using System.Threading.Tasks;
using CommandPump.Event;
using CommandPump.Enum;
using CommandPump.Common;

namespace CommandPump.Msmq
{
    public class MsmqMessageReceiver : IMessageReceiver
    {
        private MessageQueue _queue { get; set; }
        public string QueueName
        {
            get
            {
                return _queue?.QueueName;
            }
        }


        public MsmqMessageReceiver(string connectionString)
        {
            if (!MessageQueue.Exists(connectionString))
            {
                _queue = MessageQueue.Create(connectionString, true);
            }
            else
            {
                _queue = new MessageQueue(connectionString);
            }
        }

        /// <summary>
        /// Starts the message pump
        /// </summary>
        public void Start()
        {

        }

        /// <summary>
        /// Stops the message pump
        /// </summary>
        public void Stop()
        {

        }

        /// <summary>
        /// Attempts to recieve a message, triggering the processing of the message on another thread
        /// </summary>
        public void TriggerReceive()
        {
            MessageQueueTransaction trans = new MessageQueueTransaction();

            Message message = null;
            trans.Begin();
            try
            {
                message = _queue.Receive(trans);
            }
            catch (TimeoutException) // expecting a timeout error here
            {
                return;
            }

            if (message != null)
            {
                // transaction will now be commited / disposed in another thread
                ProcessReceivedMessage(trans, message); // process on another thread                
            }
            else
            {
                trans.Abort();
                trans.Dispose();
            }
        }

        /// <summary>
        /// Event fired when a message processing Task has been created
        /// </summary>
        public event EventHandler<MessageProcessingEventArgs> OnMessageProcessing;

        /// <summary>
        /// Delegate used to process messages
        /// </summary>
        public Func<Envelope<Stream>, MessageReleaseAction> InvokeMessageHandler { get; set; }

        public IMessageConverter MessageConverter { get; } = new MsmqMessageConverter();

        /// <summary>
        /// Called by the message receiver to start processing a message
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="message"></param>
        private void ProcessReceivedMessage(MessageQueueTransaction trans, Message message)
        {
            Task messageProcess = Task.Run(() =>
           {
               Envelope<Stream> envelope = MessageConverter.ConstructEnvelope(message);
               MessageReleaseAction action = InvokeMessageHandler(envelope);

               CompleteMessage(message, trans, action);
           });
            MessageProcessingEventArgs args = new MessageProcessingEventArgs() { Task = messageProcess, MessageId = message.Id };

            // the CorrelationId throws an exception when you attempt to get the value 
            try
            {
                args.CorrelationId = message.CorrelationId;
            }
            catch //will throw an exception if the correlationid is null
            {
                //args.CorrelationId = string.Empty;
            }

            OnMessageProcessing?.Invoke(this, args);
        }

        private void CompleteMessage(Message message, MessageQueueTransaction trans, MessageReleaseAction releaseResult)
        {
            switch (releaseResult)
            {
                case MessageReleaseAction.Abandon:
                case MessageReleaseAction.Complete:
                    break;
                case MessageReleaseAction.DeadLetter:
                    message.UseDeadLetterQueue = true;
                    break;
            }
            trans.Commit();
            trans.Dispose();
        }

        /// <summary>
        /// Takes the metadata envelope and translates it to messaging implementation
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
    }
}
