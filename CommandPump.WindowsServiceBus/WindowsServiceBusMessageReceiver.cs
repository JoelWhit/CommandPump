using System;
using System.IO;
using Microsoft.ServiceBus;
using System.Threading.Tasks;
using CommandPump.Contract;
using Microsoft.ServiceBus.Messaging;
using CommandPump.Event;

namespace CommandPump.WindowsServiceBus
{
    public class WindowsServiceBusMessageReceiver : IMessageReceiver
    {
        private QueueClient _queue;
        private NamespaceManager _namespaceManager;
        private MessagingFactory _messagingFactory;


        public int PrefetchCount
        {
            get
            {
                return _queue.PrefetchCount;
            }
            set
            {
                _queue.PrefetchCount = value;
            }
        }
        public string QueueName
        {
            get
            {
                return _queue?.Path;
            }
        }

        public WindowsServiceBusMessageReceiver(string queueName, string connectionString, int preFetch = 10)
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            _messagingFactory = MessagingFactory.Create(_namespaceManager.Address, _namespaceManager.Settings.TokenProvider);
            _queue = QueueClient.CreateFromConnectionString(connectionString, queueName);
            PrefetchCount = preFetch;
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
            BrokeredMessage message = null;

            try
            {
                message = _queue.Receive();
            }
            catch (TimeoutException) // expecting timeout exception
            {
                return;
            }

            if (message != null)
            {
                ProcessReceivedMessage(message); // process on another thread
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

        /// <summary>
        /// Called by the message receiver to start processing a message
        /// </summary>
        /// <param name="message"></param>
        private void ProcessReceivedMessage(BrokeredMessage message)
        {
            Task messageProcess = Task.Run(() =>
            {
                Envelope<Stream> env = CreateEnvelope(message);
                MessageReleaseAction releaseResult = InvokeMessageHandler(env);

                CompleteMessage(message, releaseResult);
            });

            OnMessageProcessing?.Invoke(this, new MessageProcessingEventArgs() { Task = messageProcess, MessageId = message.MessageId, CorrelationId = message.CorrelationId });
        }

        /// <summary>
        /// Takes the metadata envelope and translates it to messaging implementation
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private Envelope<Stream> CreateEnvelope(BrokeredMessage command)
        {

            Envelope<Stream> message = Envelope.Create(command.GetBody<Stream>());

            if (!string.IsNullOrWhiteSpace(command.MessageId))
            {
                message.MessageId = command.MessageId;
            }

            if (!string.IsNullOrWhiteSpace(command.CorrelationId))
            {
                message.CorrelationId = command.CorrelationId;
            }

            if (command.TimeToLive > TimeSpan.Zero)
            {
                message.TimeToLive = command.TimeToLive;
            }

            return message;
        }


        private void CompleteMessage(BrokeredMessage message, MessageReleaseAction action)
        {
            switch (action)
            {
                case MessageReleaseAction.Abandon:
                    message.Abandon();
                    break;
                case MessageReleaseAction.Complete:
                    message.Complete();
                    break;
                case MessageReleaseAction.DeadLetter:
                    message.DeadLetter();
                    break;
            }
        }
    }
}
