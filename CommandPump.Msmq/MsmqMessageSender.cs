using CommandPump.Contract;
using CommandPump.Common;
using System.Collections.Generic;
using System.Messaging;
using System.IO;

namespace CommandPump.Msmq
{
    public class MsmqMessageSender : IMessageSender
    {
        private MessageQueue _client;
        public string QueueName
        {
            get
            {
                return _client?.QueueName;
            }
        }

        public MsmqMessageSender(string connectionString)
        {
            if (!MessageQueue.Exists(connectionString))
            {
                _client = MessageQueue.Create(connectionString, true);
            }
            else
            {
                _client = new MessageQueue(connectionString);
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
                _client.Send(MsmqMessageConverter.ConstructMessage(message), trans);
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
                msg.Add(MsmqMessageConverter.ConstructMessage(message));
            }

            using (MessageQueueTransaction tran = new MessageQueueTransaction())
            {
                tran.Begin();
                foreach (var mg in msg)
                {
                    _client.Send(mg, tran);
                }
                tran.Commit();
            }
        }
    }
}
