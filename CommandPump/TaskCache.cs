using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CommandPump
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
                return TaskCollection.Keys.Count;
            }
        }

        /// <summary>
        /// Adds a task to the internal cache and executes the callback function when the task has completed
        /// </summary>
        /// <param name="task"></param>
        /// <param name="removeCallback"></param>
        public void AddTask(Task task, Action removeCallback)
        {
            TaskCollection.TryAdd(task.Id, task);
            Task.WhenAll(task).ContinueWith(x => RemoveTaskFromCache(task)).ContinueWith(x => removeCallback());
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
