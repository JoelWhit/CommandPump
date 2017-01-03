using CommandPump.Contract;
using System;
using System.Threading.Tasks;
using CommandPump.Common;
using CommandPump.Enum;
using CommandPump.Event;
using System.IO;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace CommandPump.AmazonSQS
{
    public class AmazonSQSMessageReceiver : IMessageReceiver
    {
        private AmazonSQSClient _client;
        public Func<Envelope<Stream>, MessageReleaseAction> InvokeMessageHandler { get; set; }

        public string QueueName { get; private set; }

        public event EventHandler<MessageProcessingEventArgs> OnMessageProcessing;

        public AmazonSQSMessageReceiver(string queueName, AWSCredentials credentials, AmazonSQSConfig clientConfig)
        {
            _client = new AmazonSQSClient(credentials, clientConfig);
            QueueName = queueName;
        }

        public void Start()
        {
            return;
        }

        public void Stop()
        {
            return;
        }

        public void TriggerReceive()
        {
            ReceiveMessageRequest request = new ReceiveMessageRequest(QueueName);
            ReceiveMessageResponse responce = _client.ReceiveMessage(request);

            foreach (var message in responce.Messages)
            {
                ProcessRecieveMessage(message);
            }
        }

        public void ProcessRecieveMessage(Message message)
        {
            Envelope<Stream> envelope = AmazonSQSMessageConverter.ConstructEnvelope(message);
            Task messageProcess = Task.Run(() =>
            {
                MessageReleaseAction releaseResult = InvokeMessageHandler(envelope);

                CompleteMessage(message, releaseResult);
            });

            OnMessageProcessing?.Invoke(this, new MessageProcessingEventArgs() { Task = messageProcess, MessageId = envelope.MessageId, CorrelationId = envelope.CorrelationId });
        }

        private void CompleteMessage(Message message, MessageReleaseAction action)
        {
            DeleteMessageRequest request = new DeleteMessageRequest(QueueName, message.ReceiptHandle);

            _client.DeleteMessage(request);
        }
    }
}
