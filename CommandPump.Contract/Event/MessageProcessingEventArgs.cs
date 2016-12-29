using System;
using System.Threading.Tasks;

namespace CommandPump.Event
{
    public class MessageProcessingEventArgs : EventArgs
    {
        /// <summary>
        /// The Task processing the message
        /// </summary>
        public Task Task { get; set; }

        public string MessageId { get; set; }

        public string CorrelationId { get; set; }
    }
}
