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

        /// <summary>
        /// Sends a message using the metadata in the envelope if applicable
        /// </summary>
        /// <param name="message"></param>
        public void Send(Envelope<Stream> message)
        {
            SendMessageRequest msgRequest = AmazonSQSMessageConverter.ConstructMessage(message);
            msgRequest.QueueUrl = QueueName;

            _client.SendMessage(msgRequest);
        }

        /// <summary>
        /// Sends a collection of messages inside a batch transaction
        /// </summary>
        /// <param name="messages"></param>
        public void SendBatch(IEnumerable<Envelope<Stream>> messages)
        {
            SendMessageBatchRequest msgRequest = AmazonSQSMessageConverter.ConstructMessage(messages);
            msgRequest.QueueUrl = QueueName;            

            _client.SendMessageBatch(msgRequest);
        }
    }
}
