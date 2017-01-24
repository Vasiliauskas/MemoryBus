using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

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
                var handle = sut.Subscribe<string>(s => listener.Set());

                // Act
                handle.Dispose();
                sut.Publish(string.Empty);

                // Assert
                Assert.IsFalse(listener.Wait(TestConfig.TestTimeout));
            }
        }

        [TestMethod]
        public void BusCanPublishToBothSyncAndAsyncHandlers()
        {
            // Arrange
            using (var sut = GetSut())
            {
                var listener = new ManualResetEventSlim();
                var listener2 = new ManualResetEventSlim();

                sut.Subscribe<string>(s => listener.Set());
                sut.SubscribeAsync<string>(s => Task.Run(() => listener2.Set()));

                // Act
                sut.Publish(string.Empty);

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout), "First one");
                Assert.IsTrue(listener2.Wait(TestConfig.TestTimeout), "Second one");
            }
        }


        [TestMethod]
        public void BusCanRespondSyncWithAsyncResponder()
        {
            // Arrange
            using (var sut = GetSut())
            {
                var value = Guid.NewGuid().ToString();
                sut.RespondAsync<string, string>(_ => Task.Run(() => value));

                // Act
                var result = sut.Request<string, string>(string.Empty);

                // Assert
                Assert.AreEqual(value, result);
            }
        }


        [TestMethod]
        public void BusCanSubscribeAndRespondWithReferenceTypeContract()
        {
            using (var sut = GetSut())
            {
                var value = new ContractA() { Name = Guid.NewGuid().ToString() };
                var listener = new ManualResetEventSlim();
                sut.Subscribe<ContractA>(s =>
                {
                    Assert.AreEqual(s.Name, value.Name);
                    listener.Set();
                });
                var test = new ContractB();

                // Act
                sut.Publish(value);

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout));
            }
        }

        [TestMethod]
        public void BusCanRequestAndRespondWithReferenceTypeContract()
        {
            // Assert
            using (var sut = GetSut())
            {
                var value = new ContractA() { Name = Guid.NewGuid().ToString() };
                var id = Guid.NewGuid();
                var listener = new ManualResetEventSlim();
                sut.Respond<ContractA, ContractB>(s =>
                {
                    Assert.AreEqual(s.Name, value.Name);
                    listener.Set();
                    return new ContractB() { Id = id };
                });
                var test = new ContractB();

                // Act
                var result = sut.Request<ContractA, ContractB>(value);

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout));
                Assert.AreEqual(result.Id, id);
            }
        }

        private IBus GetSut() => new MemoryBus(new DefaultConfig());
    }
}
