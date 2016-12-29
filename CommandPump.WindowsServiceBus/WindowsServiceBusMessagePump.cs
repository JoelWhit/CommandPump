using CommandPump.Common;
using CommandPump.Contract;
using CommandPump.Enum;
using CommandPump.Event;
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
        private QueueClient _queue;

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

        public IMessageConverter MessageConverter { get; } = new WindowsServiceBusMessageConverter();

        public WindowsServiceBusMessagePump(string queueName, string connectionString, int maxDegreeOfParalism, int preFetch = 10)
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            _messagingFactory = MessagingFactory.Create(_namespaceManager.Address, _namespaceManager.Settings.TokenProvider);
            _queue = QueueClient.CreateFromConnectionString(connectionString, queueName);
            _queue.PrefetchCount = PrefetchCount;

            _messageOptions = new OnMessageOptions();
            _messageOptions.AutoComplete = false;
            _messageOptions.ExceptionReceived += OnExceptionReceived;
            _messageOptions.MaxConcurrentCalls = maxDegreeOfParalism;
        }

        /// <summary>
        /// Starts the message pump
        /// </summary>
        public void Start()
        {
            _queue.OnMessageAsync(ProcessReceivedMessage, _messageOptions);
        }

        /// <summary>
        /// Stops the message pump
        /// </summary>
        public void Stop()
        {
            _queue.Close();
        }

        /// <summary>
        /// Does nothing. The magic starts when the Start() method is called
        /// </summary>
        public void TriggerReceive()
        {
            // do nothing - the message pump main loop is selfcontrolled on OnMessage
            return;
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
        private Task ProcessReceivedMessage(BrokeredMessage message)
        {
            Task messageProcess = Task.Run(() =>
            {
                Envelope<Stream> env = MessageConverter.ConstructEnvelope(message);

                MessageReleaseAction releaseResult = InvokeMessageHandler(env);

                CompleteMessage(message, releaseResult);
            });
            OnMessageProcessing?.Invoke(this, new MessageProcessingEventArgs() { Task = messageProcess, MessageId = message.MessageId, CorrelationId = message.CorrelationId });

            // http://stackoverflow.com/questions/30467896/brokeredmessage-automatically-disposed-after-calling-onmessage
            // "...The received message needs to be processed in the callback function's life time..."
            return messageProcess;
        }

        /// <summary>
        /// Method called on message exceptions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExceptionReceived(object sender, ExceptionReceivedEventArgs e)
        {
            Console.WriteLine("Unhandled Exception: " + e.Exception);
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
