using Amazon.Runtime;
using Amazon.SQS;
using CommandPump.Contract;
using System.Collections.Generic;
using CommandPump.Common;
using System.IO;
using Amazon.SQS.Model;

namespace CommandPump.AmazonSQS
{
    public class AmazonSQSMessageSender : IMessageSender
    {
        AmazonSQSClient _client;

        public string QueueName { get; private set; }

        public AmazonSQSMessageSender(string queueName, AWSCredentials credentials, AmazonSQSConfig clientConfig)
        {
            _client = new AmazonSQSClient(credentials, clientConfig);
            QueueName = queueName;
        }

        public void Send(Envelope<Stream> message)
        {
            SendMessageRequest msgRequest = AmazonSQSMessageConverter.ConstructMessage(message);
            msgRequest.QueueUrl = QueueName;

            _client.SendMessage(msgRequest);
        }

        public void SendBatch(IEnumerable<Envelope<Stream>> messages)
        {
            SendMessageBatchRequest msgRequest = AmazonSQSMessageConverter.ConstructMessage(messages);
            msgRequest.QueueUrl = QueueName;            

            _client.SendMessageBatch(msgRequest);
        }
    }
}
