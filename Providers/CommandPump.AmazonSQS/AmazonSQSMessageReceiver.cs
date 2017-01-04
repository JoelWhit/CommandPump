using CommandPump.Contract;
using System;
using System.Threading.Tasks;
using CommandPump.Common;
using CommandPump.Enum;
using System.IO;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace CommandPump.AmazonSQS
{
    public class AmazonSQSMessageReceiver : IMessageReceiver
    {
        private AmazonSQSClient _client;

        /// <summary>
        /// Delegate used to process messages
        /// </summary>
        public Func<Envelope<Stream>, MessageReleaseAction> InvokeMessageHandler { get; set; }

        public string QueueName { get; private set; }

        public AmazonSQSMessageReceiver(string queueName, AWSCredentials credentials, AmazonSQSConfig clientConfig)
        {
            _client = new AmazonSQSClient(credentials, clientConfig);
            QueueName = queueName;
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
        /// Synchronous method that attempts to receive messages triggering async execution of the message handler
        /// </summary>
        public Task TriggerReceive()
        {
            ReceiveMessageRequest request = new ReceiveMessageRequest(QueueName);
            request.MaxNumberOfMessages = 1;

            ReceiveMessageResponse responce = _client.ReceiveMessage(request);

            foreach (var message in responce.Messages)
            {
                return ProcessRecieveMessage(message);
            }

            return null;
        }

        public Task ProcessRecieveMessage(Message message)
        {
            Envelope<Stream> envelope = AmazonSQSMessageConverter.ConstructEnvelope(message);
            Task messageProcess = Task.Run(() =>
            {
                MessageReleaseAction releaseResult = InvokeMessageHandler(envelope);

                CompleteMessage(message, releaseResult);
            });

            return messageProcess;
        }

        private void CompleteMessage(Message message, MessageReleaseAction action)
        {
            DeleteMessageRequest request = new DeleteMessageRequest(QueueName, message.ReceiptHandle);

            _client.DeleteMessage(request);
        }
    }
}
