using CommandPump.Contract;
using System;
using System.Collections.Generic;
using CommandPump.Common;
using System.IO;
using RabbitMQ.Client;

namespace CommandPump.RabbitMQ
{
    public class RabbitMQMessageSender : IMessageSender
    {
        private IConnectionFactory _factory;
        private IConnection _conn;
        private IModel _client;

        public string QueueName { get; private set; }

        public RabbitMQMessageSender(string queueName, ConnectionFactory connectionFactory)
        {
            QueueName = queueName;

            _factory = connectionFactory;
            _conn = _factory.CreateConnection();
            _client = _conn.CreateModel();

            QueueDeclareOk dec = _client.QueueDeclare(queue: QueueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);
        }

        /// <summary>
        /// Sends a message using the metadata in the envelope if applicable
        /// </summary>
        /// <param name="message"></param>
        public void Send(Envelope<Stream> message)
        {
            IBasicProperties prop = _client.CreateBasicProperties();

            _client.BasicPublish(exchange: "",
                                 routingKey: QueueName,
                                 basicProperties: prop,
                                 body: RabbitMQMessageConverter.GetBytesFromStream(message.Body));
        }

        /// <summary>
        /// Sends a collection of messages inside a batch transaction
        /// </summary>
        /// <param name="messages"></param>
        public void SendBatch(IEnumerable<Envelope<Stream>> messages)
        {
            foreach (var message in messages)
            {
                Send(message);
            }
        }
    }
}
