using CommandPump.Common;
using CommandPump.Enum;
using CommandPump.Event;
using System;
using System.IO;

namespace CommandPump.Contract
{
    public interface IMessageReceiver
    {
        /// <summary>
        /// Delegate used to process messages
        /// </summary>
        Func<Envelope<Stream>, MessageReleaseAction> InvokeMessageHandler { get; set; }


        /// <summary>
        /// Synchronous method that attempts to receive messages triggering async execution of the message handler
        /// </summary>
        void TriggerReceive();

        /// <summary>
        /// Event fired when a message processing Task has been created
        /// </summary>
        event EventHandler<MessageProcessingEventArgs> OnMessageProcessing;

        /// <summary>
        /// Starts the message pump
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the message pump
        /// </summary>
        void Stop();

        string QueueName { get; }
    }
}
