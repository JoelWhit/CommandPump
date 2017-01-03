using System;
using System.IO;
using Microsoft.ServiceBus;
using System.Threading.Tasks;
using CommandPump.Contract;
using Microsoft.ServiceBus.Messaging;
using CommandPump.Event;
using CommandPump.Enum;
using CommandPump.Common;

namespace CommandPump.AzureServiceBus
{
    public class AzureServiceBusMessageReceiver : IMessageReceiver
    {
        private QueueClient _client;
        private NamespaceManager _namespaceManager;
        private MessagingFactory _messagingFactory;

        public int PrefetchCount
        {
            get
            {
                return _client.PrefetchCount;
            }
            set
            {
                _client.PrefetchCount = value;
            }
        }
        public string QueueName
        {
            get
            {
                return _client?.Path;
            }
        }

        public AzureServiceBusMessageReceiver(string queueName, string connectionString, int preFetch = 10)
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            _messagingFactory = MessagingFactory.Create(_namespaceManager.Address, _namespaceManager.Settings.TokenProvider);
            _client = QueueClient.CreateFromConnectionString(connectionString, queueName);
            PrefetchCount = preFetch;
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
        public void TriggerReceive()
        {
            BrokeredMessage message = null;

            try
            {
                message = _client.Receive();
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
            Envelope<Stream> envelope = AzureServiceBusMessageConverter.ConstructEnvelope(message);
            Task messageProcess = Task.Run(() =>
            {
                MessageReleaseAction releaseResult = InvokeMessageHandler(envelope);

                CompleteMessage(message, releaseResult);
            });

            OnMessageProcessing?.Invoke(this, new MessageProcessingEventArgs() { Task = messageProcess, MessageId = envelope.MessageId, CorrelationId = envelope.CorrelationId });
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
