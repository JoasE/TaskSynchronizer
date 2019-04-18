using System;
using System.Threading.Tasks;

namespace TaskSynchronizer
{
    public class AsyncSynchronization<TTask> : IDisposable where TTask : Task
    {
        private readonly Synchronizer<TTask> _synchronizer;

        internal AsyncSynchronization(Synchronizer<TTask> synchronizer)
        {
            _synchronizer = synchronizer;
        }

        public void Dispose()
        {
            _synchronizer.CurrentTask = null;
        }
    }
}
