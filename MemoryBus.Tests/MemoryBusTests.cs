using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace MemoryBus.Tests
{
    [TestClass]
    public class MemoryBusTests
    {
        [TestMethod]
        public void BusCanSubscribeAndReceiveMessages()
        {
            // Arrange
            using (var sut = GetSut())
            {
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
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout));
            }
        }

        [TestMethod]
        public void BusCanSubscribeAndReceiveMessagesWithFilter()
        {
            // Arrange
            using (var sut = GetSut())
            {
                var value = Guid.NewGuid().ToString();
                var listener = new ManualResetEventSlim();
                sut.Subscribe<string>(s =>
                {
                    Assert.AreEqual(s, value + value);
                    listener.Set();
                }, s => s.Length > value.Length);

                // Act
                sut.Publish(value);
                Assert.IsFalse(listener.Wait(TestConfig.TestTimeout));
                sut.Publish(value + value);

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout));
            }
        }

        [TestMethod]
        public void BusCanRespond()
        {
            // Assert
            using (var sut = GetSut())
            {
                var value = Guid.NewGuid().ToString();
                var listener = new ManualResetEventSlim();
                sut.Respond<string, string>(s =>
                {
                    Assert.AreEqual(s, value);
                    listener.Set();
                    return s;
                });

                // Act
                var result = sut.Request<string, string>(value);

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout));
                Assert.AreEqual(result, value);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BusCannotRespondIfMoreThanOneResponder()
        {
            // Assert
            using (var sut = GetSut())
            {
                var value = Guid.NewGuid().ToString();
                sut.Respond<string, string>(s =>
                {
                    Assert.AreEqual(s, value);
                    return s;
                });
                sut.Respond<string, string>(s =>
                {
                    Assert.AreEqual(s, value);
                    return s;
                });

                // Act
                // Assert
                var result = sut.Request<string, string>(value);
            }
        }

        [TestMethod]
        public void BusCanRespondIfMoreThanOneResponderButWithFilters()
        {
            // Assert
            using (var sut = GetSut())
            {
                var value = Guid.NewGuid().ToString();
                var listener = new ManualResetEventSlim();
                sut.Respond<string, string>(s =>
                {
                    Assert.AreEqual(s, value);
                    return s;
                }, s => s.Length > value.Length);
                sut.Respond<string, string>(s =>
                {
                    Assert.AreEqual(s, value);
                    listener.Set();
                    return s;
                });

                // Act
                var result = sut.Request<string, string>(value);

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BusThrowsIfNoResponder()
        {
            // Arrange
            using (var sut = GetSut())
            {
                // Act
                // Arrange
                sut.Request<string, string>(string.Empty);
            }
        }

        [TestMethod]
        public void BusRemovesSubscriptionOnHandleDisposed()
        {
            // Arrange
            using (var sut = GetSut())
            {
                var listener = new ManualResetEventSlim();
                var handle = sut.Subscribe<string>(s =>
                {
                    listener.Set();
                });
                // Act
                handle.Dispose();
                sut.Publish(string.Empty);

                // Assert
                Assert.IsFalse(listener.Wait(TestConfig.TestTimeout));
            }
        }

        private IBus GetSut() => new MemoryBus(new DefaultConfig());
    }
}
