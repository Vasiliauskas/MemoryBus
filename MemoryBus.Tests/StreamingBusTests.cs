﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reactive.Subjects;
using System.Threading;

namespace MemoryBus.Tests
{
    [TestClass]
    public class StreamingBusTests
    {

        [TestMethod]
        public void BusCanRespondToStreamRequestOverloadOnNextOnly()
        {
            // Arrange
            using (var sut = GetSut())
            {
                var listener = new ManualResetEventSlim();
                IObservable<string> _stream = new Subject<string>();
                string testData = Guid.NewGuid().ToString();
                sut.StreamRespond<string, string>((s, o) =>
                {
                    o.OnNext(testData);
                    o.OnCompleted();
                });

                // Act
                sut.StreamRequest<string, string>(string.Empty, s =>
                {
                    Assert.AreEqual(s, testData);
                    listener.Set();
                });

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout), "Failed");
            }
        }

        [TestMethod]
        public void BusCanRespondToStreamRequestOverloadOnNextAndOnCompelted()
        {
            // Arrange
            using (var sut = GetSut())
            {
                var listener = new ManualResetEventSlim();
                IObservable<string> _stream = new Subject<string>();
                string testData = Guid.NewGuid().ToString();
                sut.StreamRespond<string, string>((s, o) =>
                {
                    o.OnNext(testData);
                    o.OnCompleted();
                });

                // Act
                sut.StreamRequest<string, string>(string.Empty, s => Assert.AreEqual(s, testData), () => listener.Set());

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout), "Failed");
            }
        }

        [TestMethod]
        public void BusCanRespondToStreamRequestOverloadReturnIObservable()
        {
            // Arrange
            using (var sut = GetSut())
            {
                var listener = new ManualResetEventSlim();
                string testData = Guid.NewGuid().ToString();
                sut.StreamRespond<string, string>((s, o) =>
                {
                    o.OnNext(testData);
                    o.OnCompleted();
                });

                // Act
                var response = sut.StreamRequest<string, string>(string.Empty);
                response.Subscribe(s =>
                {
                    Assert.AreEqual(s, testData);
                },
                () => listener.Set());

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout), "Failed");
            }
        }

        private IBus GetSut() => new MemoryBus(new DefaultConfig());
    }
}
