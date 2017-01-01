using System.Linq;
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

        public TaskCache TaskCache;
        public int MaxDegreeOfParallelism;

        public TaskThrottler(int maxDegreeOfParallelism)
        {
            TaskCache = new TaskCache();
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            _waitHandle = new AutoResetEvent(true);
        }

        /// <summary>
        /// Adds a task - non blocking
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(Task task)
        {
            TaskCache.AddTask(task, NotifyCompletion);
        }

        /// <summary>
        /// Used by TaskCache callback when a task is removed from the cache
        /// </summary>
        private void NotifyCompletion()
        {
            _waitHandle.Set();
        }

        /// <summary>
        /// Blocks untill all the tasks currently cached to complete
        /// </summary>
        public void WaitForAllTasksToComplete()
        {
            Task.WaitAll(TaskCache.TaskCollection.Values.ToArray());
        }

        /// <summary>
        /// Blocks untill the parallelism limit is no longer exceeded
        /// </summary>
        /// <param name="token"></param>
        public void WaitUntilAllowedParallelism(CancellationToken token)
        {
            while(TaskCache.CurrentlyRunningTasks >= MaxDegreeOfParallelism)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                _waitHandle.WaitOne();
            }
        }
    }
}
