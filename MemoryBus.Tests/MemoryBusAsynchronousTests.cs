﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;
using System.Reactive.Subjects;

namespace MemoryBus.Tests
{
    [TestClass]
    public class MemoryBusAsynchronousTests
    {

        [TestMethod]
        public async Task MemoryBusCanPublishAndSubscribeAsynchronously()
        {
            // Arrange
            using (var sut = GetSut())
            {
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
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout));
            }
        }

        [TestMethod]
        public async Task MemoryBusCanPublishAndSubscribeAsynchronouslyWithFilter()
        {
            // Arrange
            using (var sut = GetSut())
            {
                var value = Guid.NewGuid().ToString();
                var listener = new ManualResetEventSlim();

                sut.SubscribeAsync<string>(s => Task.Run(() =>
                {
                    Assert.AreEqual(s, value + value);
                    listener.Set();
                }), s => s.Length > value.Length);

                // Act
                await sut.PublishAsync(value);
                Assert.IsFalse(listener.Wait(TestConfig.TestTimeout));
                await sut.PublishAsync(value + value);

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout));
            }
        }

        [TestMethod]
        public async Task BusCanRespondAsync()
        {
            // Assert
            using (var sut = GetSut())
            {
                var value = Guid.NewGuid().ToString();
                var listener = new ManualResetEventSlim();
                sut.RespondAsync<string, string>(s =>
                {
                    Assert.AreEqual(s, value);
                    listener.Set();
                    return Task.FromResult(s);
                });

                // Act
                var result = await sut.RequestAsync<string, string>(value);

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout));
                Assert.AreEqual(result, value);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task BusCannotRespondAsyncIfMoreThanOneResponder()
        {
            // Assert
            using (var sut = GetSut())
            {
                var value = Guid.NewGuid().ToString();
                var listener = new ManualResetEventSlim();
                sut.RespondAsync<string, string>(s =>
                {
                    Assert.AreEqual(s, value);
                    return Task.FromResult(s);
                });
                sut.RespondAsync<string, string>(s =>
                {
                    Assert.AreEqual(s, value);
                    return Task.FromResult(s);
                });

                // Act
                // Assert
                var result = await sut.RequestAsync<string, string>(value);
            }
        }

        [TestMethod]
        public async Task BusCanRespondAsyncIfMoreThanOneResponderButWithFilters()
        {
            // Assert
            using (var sut = GetSut())
            {
                var value = Guid.NewGuid().ToString();
                var listener = new ManualResetEventSlim();
                sut.RespondAsync<string, string>(s =>
                {
                    Assert.AreEqual(s, value);
                    return Task.FromResult(s);
                }, s => s.Length > value.Length);
                sut.RespondAsync<string, string>(s =>
                {
                    Assert.AreEqual(s, value);
                    listener.Set();
                    return Task.FromResult(s); ;
                });

                // Act
                var result = await sut.RequestAsync<string, string>(value);

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout));
            }
        }

        [TestMethod]
        public async Task BusCanPublishAsyncToBothSyncAndAsyncHandlers()
        {
            // Arrange
            using (var sut = GetSut())
            {
                var listener = new ManualResetEventSlim();
                var listener2 = new ManualResetEventSlim();

                sut.Subscribe<string>(s => listener.Set());
                sut.SubscribeAsync<string>(s => Task.Run(() => listener2.Set()));

                // Act
                await sut.PublishAsync(string.Empty);

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout), "First one");
                Assert.IsTrue(listener2.Wait(TestConfig.TestTimeout), "Second one");
            }
        }

        [TestMethod]
        public async Task BusCanRespondAsyncWithSyncResponder()
        {
            // Arrange
            using (var sut = GetSut())
            {
                var value = Guid.NewGuid().ToString();
                sut.Respond<string, string>(_ => value);

                // Act
                var result = await sut.RequestAsync<string, string>(string.Empty);

                // Assert
                Assert.AreEqual(value, result);
            }
        }


        private IBus GetSut() => new MemoryBus(new DefaultConfig());
    }
}
