using System;
using System.Threading.Tasks;

namespace TaskSynchronizer
{
    /// <summary>
    /// Ensures multiple parallel invocations to the same async method return the same result.
    /// </summary>
    /// <typeparam name="TTask">The type of task which is being run.</typeparam>
    public abstract class Synchronizer<TTask> where TTask : Task
    {
        protected readonly object Lock = new object();

        protected TTask CurrentTask;

        protected AsyncSynchronization<TTask> CurrentSynchronization;

        /// <summary>
        /// Acquires a synchronization object which should be disposed when the task is completed.
        /// </summary>
        /// <param name="taskFactory">The invocation of the method which should be synchronized.</param>
        /// <param name="task">The task which will return the synchronized result.</param>
        /// <returns>A synchronization object which should be disposed when the task is completed.</returns>
        public AsyncSynchronization<TTask> Acquire(Func<TTask> taskFactory, out TTask task) 
        {
            AsyncSynchronization<TTask> synchronization = new AsyncSynchronization<TTask>(this);
            lock (Lock)
            {
                if (CurrentTask == null)
                {
                    CurrentTask = taskFactory.Invoke();
                    CurrentSynchronization = synchronization;
                }
            }

            task = CurrentTask;

            return synchronization;
        }

        internal void Release(AsyncSynchronization<TTask> synchronization)
        {
            if (CurrentSynchronization == synchronization)
            {
                lock (Lock)
                {
                    CurrentTask = null;
                    CurrentSynchronization = null;
                }
            }
        }
    }

    public class TaskSynchronizer : Synchronizer<Task>
    { }

    public class TaskSynchronizer<T> : Synchronizer<Task<T>>
    { }
}
