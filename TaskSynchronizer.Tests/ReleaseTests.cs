using NUnit.Framework;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TaskSynchronizer.Tests
{
    public class ReleaseTests
    {
        public TaskSynchronizer Synchronizer { get; set; }

        [SetUp]
        public void SetUp()
        {
            Synchronizer = new TaskSynchronizer();
        }

        [Test]
        public void SlowLockRelease_DoesNotInterfereWithNextCall()
        {
            var receivedTasks = new ConcurrentQueue<Task>();
            for (var i = 0; i < 4; i++)
            {
                var iNow = i;
                Task.Run(async () =>
                {
                    using (Synchronizer.Acquire(() => Task.Delay(100), out var task))
                    {
                        receivedTasks.Enqueue(task);
                        await task;

                        if (iNow == 1)
                        {
                            // Be slow on releasing the synchronization
                            await Task.Delay(100);
                        }
                    }
                });

                Thread.Sleep(75);
            }

            while (receivedTasks.Count != 4)
            {
                Thread.Sleep(1);
            }

            var tasks = receivedTasks.ToArray();

            Assert.AreEqual(tasks[0], tasks[1], "0,1");
            Assert.AreEqual(tasks[2], tasks[3], "2,3");
        }
    }
}
