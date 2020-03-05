using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrboPortal.TrboNet;

namespace TrboPortal.Tests.Controllers
{
    [TestClass]
    public class QueueTest
    {
        Queue<SimpleObject> queue;

        [TestInitialize]
        public void TestInitialize()
        {
            queue = new Queue<SimpleObject>();
        }

        [TestMethod]
        public void Queue_WhenAddingItem_ShouldBeAdded()
        {
            SimpleObject simpleObject = new SimpleObject
            {
                identifier = 15,
                text = "hello"
            };
            queue.Add(simpleObject);
            SimpleObject fromQueue = queue.Peek();
            Assert.AreEqual(simpleObject, fromQueue);
        }

        [TestMethod]
        public void Queue_WhenQueueingItem_ShouldBeLast()
        {
            SimpleObject firstObject = new SimpleObject
            {
                identifier = 15,
                text = "hello"
            };
            SimpleObject secondObject = new SimpleObject
            {
                identifier = 18,
                text = "hai"
            };
            queue.Add(firstObject);
            queue.Add(secondObject);
            SimpleObject a = queue.Peek();
            Assert.AreEqual(firstObject, a);

        }


        [TestMethod]
        public void Queue_WhenJumpingItem_ShouldBeFirst()
        {
            SimpleObject firstObject = new SimpleObject
            {
                identifier = 15,
                text = "hello"
            };
            SimpleObject secondObject = new SimpleObject
            {
                identifier = 18,
                text = "hai"
            };
            SimpleObject skipObject = new SimpleObject
            {
                identifier = 18,
                text = "hai"
            };
            queue.Add(firstObject);
            queue.Add(secondObject);
            queue.Jump(skipObject);
            SimpleObject a = queue.Peek();
            Assert.AreEqual(skipObject, a);

        }

        [TestMethod]
        public void Queue_WhenPopItem_ShouldBeRemoved()
        {
            SimpleObject firstObject = new SimpleObject
            {
                identifier = 15,
                text = "hello"
            };
            SimpleObject secondObject = new SimpleObject
            {
                identifier = 18,
                text = "hai"
            };
            queue.Add(firstObject);
            queue.Add(secondObject);
            SimpleObject a = queue.Pop();
            SimpleObject b = queue.Peek();
            Assert.AreEqual(firstObject, a);
            Assert.AreEqual(secondObject, b);
            Assert.IsTrue(queue.Count == 1);
        }

        [TestMethod]
        public void Queue_WhenRemoveItem_ShouldBeRemoved()
        {
            SimpleObject firstObject = new SimpleObject
            {
                identifier = 15,
                text = "hello"
            };
            SimpleObject secondObject = new SimpleObject
            {
                identifier = 18,
                text = "hai"
            };
            SimpleObject thirdObject = new SimpleObject
            {
                identifier = 21,
                text = "hi"
            };
            queue.Add(firstObject);
            queue.Add(secondObject);
            queue.Add(secondObject);
            queue.Add(thirdObject);
            queue.Remove(secondObject);

            Assert.IsTrue(queue.Count == 2);
            var a = queue.Pop();
            var b = queue.Pop();
            Assert.AreEqual(firstObject, a);
            Assert.AreEqual(thirdObject, b);
        }

        [TestMethod]
        public void Queue_WhenRemoveItemWithComperator_ShouldNotRemoveNonMatching()
        {
            SimpleObject firstObject = new SimpleObject
            {
                identifier = 15,
                text = "hello"
            };
            SimpleObject secondObject = new SimpleObject
            {
                identifier = 18,
                text = "hai"
            };
            SimpleObject thirdObject = new SimpleObject
            {
                identifier = 21,
                text = "hi"
            };
            SimpleObject objectToRemove = new SimpleObject
            {
                identifier = 180,
                text = "different"
            };

            queue.Add(firstObject);
            queue.Add(secondObject);
            queue.Add(secondObject);
            queue.Add(thirdObject);
            queue.Remove(objectToRemove, (object1, object2) => { return (object1.identifier == object2.identifier); });

            Assert.IsTrue(queue.Count == 4);
            var a = queue.Pop();
            var b = queue.Pop();
            Assert.AreEqual(firstObject, a);
            Assert.AreEqual(secondObject, b);
        }

        [TestMethod]
        public void Queue_WhenRemoveItemWithComperator_ShouldNotRemoveMatching()
        {
            SimpleObject firstObject = new SimpleObject
            {
                identifier = 15,
                text = "hello"
            };
            SimpleObject secondObject = new SimpleObject
            {
                identifier = 18,
                text = "hai"
            };
            SimpleObject thirdObject = new SimpleObject
            {
                identifier = 21,
                text = "hi"
            };
            SimpleObject objectToRemove = new SimpleObject
            {
                identifier = 18,
                text = "different"
            };

            queue.Add(firstObject);
            queue.Add(secondObject);
            queue.Add(secondObject);
            queue.Add(thirdObject);
            queue.Remove(objectToRemove, (object1, object2) => { return (object1.identifier == object2.identifier); });

            Assert.IsTrue(queue.Count == 2);
            var a = queue.Pop();
            var b = queue.Pop();
            Assert.AreEqual(firstObject, a);
            Assert.AreEqual(thirdObject, b);
        }
    }



    class SimpleObject
    {
        public int identifier;
        public string text;
    }

}
