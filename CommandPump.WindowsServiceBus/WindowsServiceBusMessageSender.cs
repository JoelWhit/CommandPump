using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using CommandPump.Contract;

namespace CommandPump.WindowsServiceBus
{
    public class WindowsServiceBusMessageSender : IMessageSender
    {
        private QueueClient _queue;
        private NamespaceManager _namespaceManager;
        private MessagingFactory _messagingFactory;

        public string QueueName
        {
            get
            {
                return _queue?.Path;
            }
        } 


        public WindowsServiceBusMessageSender(string queueName, string connectionString)
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            _messagingFactory = MessagingFactory.Create(_namespaceManager.Address, _namespaceManager.Settings.TokenProvider);
            _queue = QueueClient.CreateFromConnectionString(connectionString, queueName);
        }


        /// <summary>
        /// Sends a message using the metadata in the envelope if applicable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void Send(Envelope<Stream> message)
        {
            _queue.Send(ConstructMessage(message));
        }

        /// <summary>
        /// Sends a collection of messages inside a batch transaction
        /// </summary>
        /// <param name="messages"></param>
        public void SendBatch(IEnumerable<Envelope<Stream>> messages)
        {
            List<BrokeredMessage> msg = new List<BrokeredMessage>();
            foreach (var message in messages)
            {
                msg.Add(ConstructMessage(message));
            }

            _queue.SendBatch(msg);
        }

        /// <summary>
        /// Takes the metadata envelope and translates it to messaging implementation
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private BrokeredMessage ConstructMessage(Envelope<Stream> command)
        {

            var message = new BrokeredMessage(command.Body, true);

            if (!string.IsNullOrWhiteSpace(command.MessageId))
            {
                message.MessageId = command.MessageId;
            }

            if (!string.IsNullOrWhiteSpace(command.CorrelationId))
            {
                message.CorrelationId = command.CorrelationId;
            }

            if (command.Delay > TimeSpan.Zero)
            {
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.Add(command.Delay);
            }

            if (command.TimeToLive > TimeSpan.Zero)
            {
                message.TimeToLive = command.TimeToLive;
            }

            return message;
        }
    }
}
