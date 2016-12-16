using CommandPump.Contract;
using System.Collections.Generic;
using System.Messaging;
using System.IO;

namespace CommandPump.MSMQ
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
                _queue.Send(ConstructMessage(message), trans);
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
                msg.Add(ConstructMessage(message));
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

        /// <summary>
        /// Takes the metadata envelope and translates it to messaging implementation
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private Message ConstructMessage(Envelope<Stream> command)
        {

            var message = new Message();
            message.BodyStream = command.Body;

            //if (!string.IsNullOrWhiteSpace(command.MessageId))
            //{
            //    message.Id = command.MessageId;
            //}

            if (!string.IsNullOrWhiteSpace(command.CorrelationId))
            {
                message.CorrelationId = command.CorrelationId;
            }

            //if (command.Delay > TimeSpan.Zero)
            //{
            //    message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.Add(command.Delay);
            //}

            //if (command.TimeToLive > TimeSpan.Zero)
            //{
            //    message.TimeToLive = command.TimeToLive;
            //}

            return message;
        }
    }
}
