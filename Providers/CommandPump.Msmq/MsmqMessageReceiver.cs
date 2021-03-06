﻿using CommandPump.Contract;
using System;
using System.IO;
using System.Messaging;
using System.Threading.Tasks;
using CommandPump.Enum;
using CommandPump.Common;

namespace CommandPump.Msmq
{
    public class MsmqMessageReceiver : IMessageReceiver
    {
        private MessageQueue _client { get; set; }
        public string QueueName
        {
            get
            {
                return _client?.QueueName;
            }
        }


        public MsmqMessageReceiver(string connectionString)
        {
            if (!MessageQueue.Exists(connectionString))
            {
                _client = MessageQueue.Create(connectionString, true);
            }
            else
            {
                _client = new MessageQueue(connectionString);
            }
        }

        /// <summary>
        /// Starts the message pump
        /// </summary>
        public void Start()
        {
            return;
        }

        /// <summary>
        /// Stops the message pump
        /// </summary>
        public void Stop()
        {
            return;
        }

        /// <summary>
        /// Attempts to recieve a message, triggering the processing of the message on another thread
        /// </summary>
        public Task TriggerReceive()
        {
            MessageQueueTransaction trans = new MessageQueueTransaction();
            Message message = null;
            trans.Begin();
            try
            {
                message = _client.Receive(trans);

                if (message != null)
                {
                    // transaction will now be commited / disposed in another thread
                    return ProcessReceivedMessage(trans, message); // process on another thread                
                }
                else
                {
                    trans.Abort();
                    trans.Dispose();
                    return null;
                }
            }
            catch (TimeoutException) // expecting a timeout error here
            {
                return null;
            }
        }

        /// <summary>
        /// Delegate used to process messages
        /// </summary>
        public Func<Envelope<Stream>, MessageReleaseAction> InvokeMessageHandler { get; set; }



        /// <summary>
        /// Called by the message receiver to start processing a message
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="message"></param>
        private Task ProcessReceivedMessage(MessageQueueTransaction trans, Message message)
        {
            Envelope<Stream> envelope = MsmqMessageConverter.ConstructEnvelope(message);
            Task messageProcess = Task.Run(() =>
            {
                MessageReleaseAction action = InvokeMessageHandler(envelope);

                CompleteMessage(message, trans, action);
            });

            return messageProcess;
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
    }
}
