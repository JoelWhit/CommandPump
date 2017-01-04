using CommandPump.Contract;
using System;
using CommandPump.Common;
using CommandPump.Enum;
using System.IO;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace CommandPump.RabbitMQ
{
    public class RabbitMQMessageReceiver : IMessageReceiver
    {
        private IConnectionFactory _factory;
        private IConnection _conn;
        private IModel _client;

        public string QueueName { get; private set; }

        public RabbitMQMessageReceiver(string queueName, ConnectionFactory connectionFactory, ushort preFetch = 10)
        {
            QueueName = queueName;

            _factory = connectionFactory;
            _conn = _factory.CreateConnection();
            _client = _conn.CreateModel();

            _client.BasicQos(0, preFetch, false);

            QueueDeclareOk dec = _client.QueueDeclare(queue: QueueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);
        }

        /// <summary>
        /// Delegate used to process messages
        /// </summary>
        public Func<Envelope<Stream>, MessageReleaseAction> InvokeMessageHandler { get; set; }

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
            //var result = _client.BasicConsume(QueueName, false, _consumer);

            BasicGetResult result = _client.BasicGet(QueueName, false);
            if (result != null)
            {
                return ProcessReceivedMessage(result);
            }
            else
            {
                return null;
            }
        }

        private Task ProcessReceivedMessage(BasicGetResult result)
        {
            Envelope<Stream> envelope = RabbitMQMessageConverter.ConstructEnvelope(result);
            Task messageProcess = Task.Run(() =>
            {
                MessageReleaseAction action = InvokeMessageHandler(envelope);

                CompleteMessage(result.DeliveryTag, action);
            });

            return messageProcess;
        }

        private void CompleteMessage(ulong DeliveryTag, MessageReleaseAction action)
        {
            _client.BasicAck(DeliveryTag, false);
        }
    }
}
