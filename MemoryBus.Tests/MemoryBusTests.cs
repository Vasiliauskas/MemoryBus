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
                Assert.IsTrue(listener.Wait(5000));
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
                Assert.IsFalse(listener.Wait(1000));
                sut.Publish(value + value);

                // Assert
                Assert.IsTrue(listener.Wait(1000));
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
                Assert.IsTrue(listener.Wait(1000));
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
                var listener = new ManualResetEventSlim();
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
                Assert.IsTrue(listener.Wait(1000));
            }
        }

        private IBus GetSut()
        {
            return new MemoryBus(new DefaultConfig());
        }
    }
}
