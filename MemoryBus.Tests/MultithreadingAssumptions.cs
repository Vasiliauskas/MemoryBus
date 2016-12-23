using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemoryBus.Tests
{
    [TestClass]
    // This is a non deterministic approach to prove thread safety
    public class MultithreadingAssumptions
    {
        [TestMethod]
        public async Task BusPubSubCanHandleMultiThreadedEnvironment()
        {
            // Arrange
            using (var sut = GetSut())
            {
                var tasks = new List<Task>();
                var listener = new ManualResetEventSlim();

                for (int i = 0; i < 5; i++)
                {
                    tasks.Add(new Task(() => sut.Subscribe<object>(s => { })));
                    tasks.Add(new Task(() => sut.Subscribe<object>(s => { }, s => s != null)));
                }

                for (int i = 0; i < 5; i++)
                {
                    tasks.Add(new Task(() => sut.Publish<object>(new object())));
                    tasks.Add(new Task(() => sut.Publish<object>(null)));
                }

                for (int i = 0; i < 10; i++)
                {
                    tasks.Add(new Task(() =>
                    {
                        var handle = sut.Subscribe<object>(s => { });
                        Thread.Sleep(i * 10);
                        sut.Publish<object>(new object());
                        handle.Dispose();
                    }));
                }
                // Act
                tasks.ForEach(t => t.Start());

                // Assert
                await Task.WhenAll(tasks).ContinueWith(c =>
                {
                    Assert.AreEqual(c.IsFaulted, false,c.Exception?.ToString());
                    Assert.AreEqual(c.IsCompleted, true);
                    Assert.AreEqual(c.Exception, null);
                    listener.Set();
                });

                Assert.AreEqual(listener.Wait(TestConfig.TestTimeout), true);
            }
        }


        private IBus GetSut() =>  new MemoryBus(new DefaultConfig());
    }
}
