using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;

namespace MemoryBus.Tests
{
    [TestClass]
    public class MemoryBusAsynchronousTests
    {

        [TestMethod]
        public async Task MemoryBusCanPublishAndSubscribeAsynchronously()
        {
            // Arrange
            var sut = GetSut();
            var value = Guid.NewGuid().ToString();
            var listener = new ManualResetEventSlim();
            sut.SubscribeAsync<string>(s =>
            {
                Assert.AreEqual(s, value);
                listener.Set();

                return Task.FromResult<object>(null);
            });

            // Act
            await sut.PublishAsync(value);

            // Assert
            Assert.IsTrue(listener.Wait(5000));
        }

        [TestMethod]
        public async Task MemoryBusCanPublishAndSubscribeAsynchronouslyWithFilter()
        {
            // Arrange
            var sut = GetSut();
            var value = Guid.NewGuid().ToString();
            var listener = new ManualResetEventSlim();

            sut.SubscribeAsync<string>(s => Task.Run(() =>
            {
                Assert.AreEqual(s, value + value);
                listener.Set();
            }), s => s.Length > value.Length);

            // Act
            await sut.PublishAsync(value);
            Assert.IsFalse(listener.Wait(1000));
            await sut.PublishAsync(value + value);

            // Assert
            Assert.IsTrue(listener.Wait(1000));
        }

        private IBus GetSut()
        {
            return new MemoryBus(new DefaultConfig());
        }
    }
}
