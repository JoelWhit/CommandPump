using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CommandPump.Common
{
    /// <summary>
    /// Collection of Tasks which fire a callback function when the task has completed
    /// </summary>
    public class TaskCache
    {
        public ConcurrentDictionary<int, Task> TaskCollection;

        public TaskCache()
        {
            TaskCollection = new ConcurrentDictionary<int, Task>();
        }

        public int CurrentlyRunningTasks
        {
            get
            {
                return TaskCollection.Count;
            }
        }

        /// <summary>
        /// Blocks untill all the tasks currently cached to complete. This will take 5+ seconds
        /// </summary>
        public void WaitForAllTasksToComplete()
        {
            // loop and sleep to make an attempt to get all the late coming tasks
            while (CurrentlyRunningTasks != 0)
            {
                Task.WaitAll(TaskCollection.Values.ToArray());
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// Adds a task to the internal cache and executes the callback function when the task has completed
        /// </summary>
        /// <param name="task"></param>
        /// <param name="removeCallback"></param>
        public void AddTask(Task task, Action callback)
        {
            TaskCollection.TryAdd(task.Id, task);
            Task.WhenAll(task).ContinueWith(x => RemoveTaskFromCache(task)).ContinueWith(x => callback());
        }

        /// <summary>
        /// Adds a task to the internal cache
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(Task task)
        {
            AddTask(task, () => { });
        }

        /// <summary>
        /// Removes the task from the cache
        /// </summary>
        /// <param name="task"></param>
        private void RemoveTaskFromCache(Task task)
        {
            Task outTask;
            TaskCollection.TryRemove(task.Id, out outTask);
        }
    }
}
