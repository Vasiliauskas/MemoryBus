using MemoryBus.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryBus.Tests
{
    [TestClass]
    public class TopicTests
    {
        [TestMethod]
        public void SameTopicsReturnSameHashCodeSingleParam()
        {
            // Arrange
            // Act
            var sut = Topic.CreateTopic<ContractA>();

            // Assert
            Assert.AreEqual(sut, Topic.CreateTopic<ContractA>());
            Assert.AreEqual(sut, Topic.CreateTopic<ContractA>());
            Assert.AreEqual(sut, Topic.CreateTopic<ContractA>());
            Assert.AreEqual(sut, Topic.CreateTopic<ContractA>());
        }

        [TestMethod]
        public void SameTopicsReturnSameHashCodeSingleParam2()
        {
            // Arrange
            // Act
            var sut = Topic.CreateTopic<ContractB>();

            // Assert
            Assert.AreEqual(sut, Topic.CreateTopic<ContractB>());
            Assert.AreEqual(sut, Topic.CreateTopic<ContractB>());
            Assert.AreEqual(sut, Topic.CreateTopic<ContractB>());
            Assert.AreEqual(sut, Topic.CreateTopic<ContractB>());
        }

        [TestMethod]
        public void DifferentTopicsAreNotEqual()
        {
            // Arrange
            // Act
            var sut = Topic.CreateTopic<ContractA>();

            // Assert
            Assert.AreNotEqual(sut, Topic.CreateTopic<ContractB>());
        }

        [TestMethod]
        public void DifferentTopicsAreNotEqual2()
        {
            // Arrange
            // Act
            var sut = Topic.CreateTopic<ContractB>();

            // Assert
            Assert.AreNotEqual(sut, Topic.CreateTopic<ContractA>());
        }

        [TestMethod]
        public void TopicsAreEqualIfCreatedWithFactory()
        {
            // Arrange
            // Act
            var sut = Topic.CreateTopic<ContractA, ContractB>();

            // Assert
            Assert.AreEqual(sut, Topic.CreateTopic<ContractA, ContractB>());
            Assert.AreEqual(sut, Topic.CreateTopic<ContractA, ContractB>());
            Assert.AreEqual(sut, Topic.CreateTopic<ContractA, ContractB>());
            Assert.AreEqual(sut, Topic.CreateTopic<ContractA, ContractB>());
        }

        [TestMethod]
        public void TopicsAreNotEqualIfCreatedWithFactory()
        {
            // Arrange
            // Act
            var sut = Topic.CreateTopic<ContractA, ContractB>();

            // Assert
            Assert.AreNotEqual(sut, Topic.CreateTopic<ContractB, ContractA>());
            Assert.AreNotEqual(sut, Topic.CreateTopic<ContractB, ContractA>());
            Assert.AreNotEqual(sut, Topic.CreateTopic<ContractB, ContractA>());
            Assert.AreNotEqual(sut, Topic.CreateTopic<ContractB, ContractA>());
        }
    }
}
