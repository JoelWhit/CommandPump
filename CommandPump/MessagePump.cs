using CommandPump.Common;
using CommandPump.Contract;
using CommandPump.Enum;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CommandPump
{
    /// <summary>
    /// Provides a management layer for controling aspects such as 
    /// Starting, Stoping, CancellationTokens, MaxDegreeOfParallelism and handling of exceptions in messages
    /// </summary>
    public class MessagePump : IMessagePump
    {
        private IMessageReceiver _receiver;
        private object _cancellationTokenLock = new object();
        private TaskThrottler _throttler;
        private TaskCache _cache;
        private int _maxDegreeOfParallelism;

        private CancellationTokenSource cancellationSource;

        public MessagePump(IMessageReceiver receiver, int maxDegreeOfParallelism)
        {
            _receiver = receiver;
            _receiver.InvokeMessageHandler = OnMessageReceived;
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _cache = new TaskCache();
            _throttler = new TaskThrottler(maxDegreeOfParallelism, _cache);
        }


        /// <summary>
        /// Start the message pump
        /// </summary>
        public void Start()
        {
            lock (_cancellationTokenLock)
            {
                cancellationSource = new CancellationTokenSource();
                _receiver.Start();
                Task.Run(() =>
                    ReceiveMessage(cancellationSource.Token),
                    cancellationSource.Token);
            }
        }

        /// <summary>
        /// Stop the message pump
        /// </summary>
        public void Stop()
        {
            lock (_cancellationTokenLock)
            {
                using (cancellationSource)
                {
                    if (cancellationSource != null)
                    {
                        cancellationSource.Cancel();
                        _receiver.Stop();
                        cancellationSource = null;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to receive a message after a parallelism check
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void ReceiveMessage(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _throttler.WaitUntilAllowedParallelism(cancellationToken);

                    Task messageProcess = _receiver.TriggerReceive();

                    if (messageProcess != null)
                    {
                        _throttler.AddTask(messageProcess);
                    }
                }
                finally
                {
                    _throttler.WorkAttemptFinished();
                }
            }
        }

        /// <summary>
        /// Message handler set in the MessageReceiver.
        /// Handles processing of the message and exceptions
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private MessageReleaseAction OnMessageReceived(Envelope<Stream> message)
        {
            try
            {
                ProcessReceivedMessage(message.Body, message.MessageId, message.CorrelationId);
                return MessageReleaseAction.Complete;
            }
            catch (Exception ex)
            {
                return MessageReleaseAction.DeadLetter;
            }
        }

        /// <summary>
        /// Provides an entry point for message processing with metadata
        /// </summary>
        /// <param name="Payload"></param>
        /// <param name="MessageId"></param>
        /// <param name="CorrelationId"></param>
        public virtual void ProcessReceivedMessage(object Payload, string MessageId, string CorrelationId)
        {

        }

        /// <summary>
        /// Blocks untill all the tasks currently cached to complete. This will take 5+ seconds
        /// </summary>
        public void WaitForCompletion()
        {
            _cache.WaitForAllTasksToComplete();
        }
    }
}
