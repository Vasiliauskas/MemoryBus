using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace MemoryBus.Tests
{
    [TestClass]
    public class MemoryBusSynchronousTests
    {
        [TestMethod]
        public void BusCanSubscribeAndReceiveMessages()
        {
            // Arrange
            var sut = GetSut();
            var value = Guid.NewGuid().ToString();
            var listener = new ManualResetEventSlim();
            sut.Subscribe<string>(s =>
            {
                Assert.AreEqual(s, value);
                listener.Set();
            });

            // Act
            sut.Publish(value);

            // Assert
            Assert.IsTrue(listener.Wait(5000));
        }

        [TestMethod]
        public void BusCanSubscribeAndReceiveMessagesWithFilter()
        {
            // Arrange
            var sut = GetSut();
            var value = Guid.NewGuid().ToString();
            var listener = new ManualResetEventSlim();
            sut.Subscribe<string>(s =>
            {
                Assert.AreEqual(s, value);
                listener.Set();
            }, s => s.Length > value.Length);

            // Act
            sut.Publish(value);

            // Assert
            Assert.IsFalse(listener.Wait(1000));
        }

        private IBus GetSut()
        {
            return new MemoryBus(new DefaultConfig());
        }
    }
}
