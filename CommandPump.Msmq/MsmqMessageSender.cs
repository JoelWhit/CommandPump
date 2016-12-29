using CommandPump.Contract;
using CommandPump.Common;
using System.Collections.Generic;
using System.Messaging;
using System.IO;

namespace CommandPump.Msmq
{
    public class MsmqMessageSender : IMessageSender
    {
        private MessageQueue _queue;
        public string QueueName
        {
            get
            {
                return _queue?.QueueName;
            }
        }

        public IMessageConverter MessageConverter { get; } = new MsmqMessageConverter();

        public MsmqMessageSender(string connectionString)
        {
            if (!MessageQueue.Exists(connectionString))
            {
                _queue = MessageQueue.Create(connectionString, true);
            }
            else
            {
                _queue = new MessageQueue(connectionString);
            }
        }

        /// <summary>
        /// Sends a message using the metadata in the envelope if applicable
        /// </summary>
        /// <param name="message"></param>
        public void Send(Envelope<Stream> message)
        {
            using (MessageQueueTransaction trans = new MessageQueueTransaction())
            {
                trans.Begin();
                _queue.Send(MessageConverter.ConstructMessage(message), trans);
                trans.Commit();
            }
        }

        /// <summary>
        /// Sends a collection of messages inside a batch transaction
        /// </summary>
        /// <param name="messages"></param>
        public void SendBatch(IEnumerable<Envelope<Stream>> messages)
        {
            List<Message> msg = new List<Message>();
            foreach (var message in messages)
            {
                msg.Add((Message)MessageConverter.ConstructMessage(message));
            }

            using (MessageQueueTransaction tran = new MessageQueueTransaction())
            {
                tran.Begin();
                foreach (var mg in msg)
                {
                    _queue.Send(mg, tran);
                }
                tran.Commit();
            }
        }
    }
}
