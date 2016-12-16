using System.Collections.Generic;
using System.IO;

namespace CommandPump.Contract
{
    public interface IMessageSender 
    {
        string QueueName { get; }

        /// <summary>
        /// Sends a message using the metadata in the envelope if applicable
        /// </summary>
        /// <param name="message"></param>
        void Send(Envelope<Stream> message);

        /// <summary>
        /// Sends a collection of messages inside a batch transaction
        /// </summary>
        /// <param name="messages"></param>
        void SendBatch(IEnumerable<Envelope<Stream>> messages);
    }
}
