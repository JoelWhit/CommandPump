using CommandPump.Contract;
using System;
using CommandPump.Common;
using CommandPump.Enum;
using CommandPump.Event;
using System.IO;
using RabbitMQ.Client;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace CommandPump.RabbitMQ
{
    public class RabbitMQMessageReceiver : IMessageReceiver
    {
        private IConnectionFactory _factory;
        private IConnection _conn;
        private IModel _client;

        public string QueueName { get; private set; }

        public RabbitMQMessageReceiver(string queueName, ConnectionFactory connectionFactory)
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

        public Func<Envelope<Stream>, MessageReleaseAction> InvokeMessageHandler { get; set; }

        public event EventHandler<MessageProcessingEventArgs> OnMessageProcessing;

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
            //var result = _client.BasicConsume(QueueName, false, _consumer);

            BasicGetResult result = _client.BasicGet(QueueName, false);
            if (result != null)
            {
                ProcessReceivedMessage(result);
            }
        }

        private void ProcessReceivedMessage(BasicGetResult result)
        {
            Envelope<Stream> envelope = RabbitMQMessageConverter.ConstructEnvelope(result);
            Task messageProcess = Task.Run(() =>
            {                
                MessageReleaseAction action = InvokeMessageHandler(envelope);

                CompleteMessage(result.DeliveryTag, action);
            });

            OnMessageProcessing?.Invoke(this, new MessageProcessingEventArgs() { Task = messageProcess, MessageId = envelope.MessageId, CorrelationId = envelope.CorrelationId });
        }

        private void ProcessReceivedMessage(object sender, BasicDeliverEventArgs e)
        {
            Envelope<Stream> envelope = RabbitMQMessageConverter.ConstructEnvelope(e);
            Task messageProcess = Task.Run(() =>
            {
                MessageReleaseAction action = InvokeMessageHandler(envelope);

                CompleteMessage(e.DeliveryTag, action);
            });

            OnMessageProcessing?.Invoke(this, new MessageProcessingEventArgs() { Task = messageProcess, MessageId = envelope.MessageId, CorrelationId = envelope.CorrelationId });

            messageProcess.Wait();
        }

        private void CompleteMessage(ulong DeliveryTag, MessageReleaseAction action)
        {
            _client.BasicAck(DeliveryTag, false);
        }
    }
}
