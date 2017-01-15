using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void BusCanRespondToStreamRequestOverloadOnNextAndOnCompleted()
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
        public void BusCanRespondToStreamRequestOverloadOnNextAndOnErrorAndOnCompleted()
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
                sut.StreamRequest<string, string>(string.Empty, s => Assert.AreEqual(s, testData), e => { }, () => listener.Set());

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout), "Failed");
            }
        }

        [TestMethod]
        public void BusCanRespondToStreamRequestOverloadOnNextAndOnError()
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
                    o.OnError(new Exception("Stuff"));
                });

                // Act
                sut.StreamRequest<string, string>(string.Empty, s => Assert.AreEqual(s, testData), e => listener.Set());

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

        [TestMethod]
        public void BusShouldRespondToStreamRequestWithFilter()
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
                }, s => s.Length > 5);

                // Act
                var response = sut.StreamRequest<string, string>("123456");
                response.Subscribe(s =>
                {
                    Assert.AreEqual(s, testData);
                },
                () => listener.Set());

                // Assert
                Assert.IsTrue(listener.Wait(TestConfig.TestTimeout), "Failed");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BusShouldFailRespondToStreamRequestWithFilter()
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
                }, s => s.Length > 5);

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
