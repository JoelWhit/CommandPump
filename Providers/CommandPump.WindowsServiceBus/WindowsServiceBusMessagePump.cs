using CommandPump.Common;
using CommandPump.Contract;
using CommandPump.Enum;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CommandPump.WindowsServiceBus
{
    /// <summary>
    /// Creates a message pump by providing a wrapper round the existing OnMessage() functionality
    /// </summary>
    public class WindowsServiceBusMessagePump : IMessageReceiver
    {
        private NamespaceManager _namespaceManager;
        private MessagingFactory _messagingFactory;
        private OnMessageOptions _messageOptions;
        private QueueClient _client;

        /// <summary>
        /// internal task cache for service bus - there isn't an inbuilt way of waiting for all processing to finish
        /// </summary>
        private TaskCache _cache;

        public int MaxDegreeOfParalism
        {
            get
            {
                return _messageOptions.MaxConcurrentCalls;
            }
            set
            {
                _messageOptions.MaxConcurrentCalls = value;
            }
        }
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

        public WindowsServiceBusMessagePump(string queueName, string connectionString, int maxDegreeOfParalism, int preFetch = 10)
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            _messagingFactory = MessagingFactory.Create(_namespaceManager.Address, _namespaceManager.Settings.TokenProvider);
            _client = QueueClient.CreateFromConnectionString(connectionString, queueName);
            _client.PrefetchCount = PrefetchCount;

            _messageOptions = new OnMessageOptions();
            _messageOptions.AutoComplete = false;
            _messageOptions.MaxConcurrentCalls = maxDegreeOfParalism;

            _cache = new TaskCache();
        }

        /// <summary>
        /// Starts the message pump
        /// </summary>
        public void Start()
        {
            _client.OnMessageAsync(ProcessReceivedMessage, _messageOptions);
        }

        /// <summary>
        /// Stops the message pump
        /// </summary>
        public void Stop()
        {
            _client.Close();
            _cache.WaitForAllTasksToComplete();
        }

        /// <summary>
        /// Does nothing. The magic starts when the Start() method is called
        /// </summary>
        public Task TriggerReceive()
        {
            // do nothing - the message pump main loop is selfcontrolled on OnMessage
            return null;
        }

        /// <summary>
        /// Delegate used to process messages
        /// </summary>
        public Func<Envelope<Stream>, MessageReleaseAction> InvokeMessageHandler { get; set; }

        /// <summary>
        /// Called by the message receiver to start processing a message
        /// </summary>
        /// <param name="message"></param>
        private Task ProcessReceivedMessage(BrokeredMessage message)
        {
            Envelope<Stream> envelope = WindowsServiceBusMessageConverter.ConstructEnvelope(message);
            Task messageProcess = Task.Run(() =>
            {
                MessageReleaseAction releaseResult = InvokeMessageHandler(envelope);

                CompleteMessage(message, releaseResult);
            });

            // http://stackoverflow.com/questions/30467896/brokeredmessage-automatically-disposed-after-calling-onmessage
            // "...The received message needs to be processed in the callback function's life time..."

            _cache.AddTask(messageProcess);

            return messageProcess;
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
