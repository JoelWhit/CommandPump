using CommandPump.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CommandPump
{
    /// <summary>
    /// Provides a means of regulating the number of tasks concurrently running. 
    /// Can handle multiple input threads through wait handles
    /// </summary>
    public class TaskThrottler
    {
        private AutoResetEvent _waitHandle;

        private TaskCache _taskCache;
        public int MaxDegreeOfParallelism;

        /// <summary>
        /// Incremented when looking for Messages and decremented when finished
        /// CurrentlyRunningTasks + _temporaryOffset > MaxDegreeOfParallelism
        /// </summary>
        private int _temporaryOffset = 0;

        public TaskThrottler(int maxDegreeOfParallelism, TaskCache cache)
        {
            _taskCache = cache;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            _waitHandle = new AutoResetEvent(true);
        }

        /// <summary>
        /// Adds a task - non blocking
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(Task task)
        {
            _taskCache.AddTask(task, NotifyCompletion);
        }

        /// <summary>
        /// Used by TaskCache callback when a task is removed from the cache
        /// </summary>
        private void NotifyCompletion()
        {
            _waitHandle.Set();
        }

        /// <summary>
        /// Blocks untill the parallelism limit is no longer exceeded
        /// When block is released the offset is incremented
        /// </summary>
        /// <param name="token"></param>
        public void WaitUntilAllowedParallelism(CancellationToken token)
        {
            while (_taskCache.CurrentlyRunningTasks + _temporaryOffset >= MaxDegreeOfParallelism)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                _waitHandle.WaitOne();
            }
            WorkAttemptStart();
        }

        /// <summary>
        /// Increments a temporary value used to offset message processing attempts 
        /// with currently running tasks
        /// </summary>
        public void WorkAttemptStart()
        {
            Interlocked.Increment(ref _temporaryOffset);
        }

        /// <summary>
        /// Decrements a temporary value used to offset message processing attempts 
        /// with currently running tasks
        /// </summary>
        public void WorkAttemptFinished()
        {
            Interlocked.Decrement(ref _temporaryOffset);
        }
    }
}
