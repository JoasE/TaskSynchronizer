using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TaskSynchronizer.Tests
{
    public class TaskSynchronizerTests
    {
        public TaskSynchronizer<DateTime> Synchronizer { get; set; }

        [SetUp]
        public void SetUp()
        {
            Synchronizer = new TaskSynchronizer<DateTime>();
        }

        [Repeat(10)]
        [Test]
        public async Task Synchronizes_Small()
        {
            // Arrange
            var result1 = DateTime.MaxValue;
            var result2 = DateTime.MinValue;

            // Act
            async Task Func1()
            {
                using (Synchronizer.Acquire(() => GetTimeAfterMilliseconds(1), out var task))
                {
                    result1 = await task;
                }
            }

            async Task Func2()
            {
                using (Synchronizer.Acquire(() => GetTimeAfterMilliseconds(1), out var task))
                {
                    result2 = await task;
                }
            }

            await Task.WhenAll(Func1(), Func2());

            // Assert
            Assert.AreEqual(result1, result2);
        }

        [Test]
        public async Task Synchronizes_Big()
        {
            // Arrange
            var result1 = DateTime.MaxValue;
            var result2 = DateTime.MinValue;

            // Act
            async Task Func1()
            {
                using (Synchronizer.Acquire(() => GetTimeAfterMilliseconds(2000), out var task))
                {
                    result1 = await task;
                }
            }

            async Task Func2()
            {
                using (Synchronizer.Acquire(() => GetTimeAfterMilliseconds(1000), out var task))
                {
                    result2 = await task;
                }
            }

            await Task.WhenAll(Func1(), Func2());

            // Assert
            Assert.AreEqual(result1, result2);
        }

        [Test]
        public async Task Releases()
        {
            // Arrange
            var result1 = DateTime.MaxValue;
            var result2 = DateTime.MinValue;

            // Act
            async Task Func1()
            {
                using (Synchronizer.Acquire(() => GetTimeAfterMilliseconds(100), out var task))
                {
                    result1 = await task;
                }
            }

            async Task Func2()
            {
                using (Synchronizer.Acquire(() => GetTimeAfterMilliseconds(100), out var task))
                {
                    result2 = await task;
                }
            }

#pragma warning disable 4014
            Func1();
            await GetTimeAfterMilliseconds(200);
            Func2();
#pragma warning restore 4014

            await GetTimeAfterMilliseconds(200);

            // Assert
            Assert.AreNotEqual(result1, result2);
        }

        public async Task<DateTime> GetTimeAfterMilliseconds(int milliSeconds)
        {
            await Task.Delay(milliSeconds);
            return DateTime.UtcNow;
        }
    }
}