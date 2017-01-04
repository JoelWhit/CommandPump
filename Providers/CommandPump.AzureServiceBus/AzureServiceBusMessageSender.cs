using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Collections.Generic;
using System.IO;
using CommandPump.Contract;
using CommandPump.Common;

namespace CommandPump.AzureServiceBus
{
    public class AzureServiceBusMessageSender : IMessageSender
    {
        private QueueClient _client;
        private NamespaceManager _namespaceManager;
        private MessagingFactory _messagingFactory;

        public string QueueName
        {
            get
            {
                return _client?.Path;
            }
        }

        public AzureServiceBusMessageSender(string queueName, string connectionString)
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            _messagingFactory = MessagingFactory.Create(_namespaceManager.Address, _namespaceManager.Settings.TokenProvider);
            _client = QueueClient.CreateFromConnectionString(connectionString, queueName);
        }


        /// <summary>
        /// Sends a message using the metadata in the envelope if applicable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void Send(Envelope<Stream> message)
        {
            _client.Send(AzureServiceBusMessageConverter.ConstructMessage(message));
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
                msg.Add(AzureServiceBusMessageConverter.ConstructMessage(message));
            }

            _client.SendBatch(msg);
        }
    }
}
