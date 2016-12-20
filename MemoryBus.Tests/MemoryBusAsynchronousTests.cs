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

        private IBus GetSut()
        {
            return new MemoryBus(new DefaultConfig());
        }
    }
}
