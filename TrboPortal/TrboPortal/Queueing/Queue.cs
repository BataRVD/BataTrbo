using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrboPortal.TrboNet
{
    public class Queue<T>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly LinkedList<T> queue;

        public int Count { get { return queue.Count; } }

        public Queue()
        {
            queue = new LinkedList<T>();
        }

        /// <summary>
        /// Remove all occurences of device from the queue
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool Remove(T device)
        {
            return Remove(device, (a, b) => { return EqualityComparer<T>.Default.Equals(a, b); });
        }


        /// <summary>
        /// Remove all occurences of device from the queue, matched by the compare function
        /// </summary>
        /// <param name="device"></param>
        /// <param name="comperator"></param>
        /// <returns></returns>
        public bool Remove(T device, Func<T, T, bool> comperator)
        {
            bool removed = false;
            lock (queue)
            {
                var node = queue.First;
                while (node != null)
                {
                    var nextNode = node.Next;
                    if (comperator(node.Value, device))
                    {
                        queue.Remove(node);
                        logger.Debug($"Removed from queue {node.Value}");
                        removed = true;
                    }
                    node = nextNode;
                }
            }
            return removed;
        }


        /// <summary>
        /// See the first item of the queue
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            lock (queue)
            {
                if (!IsEmpty())
                {
                    T peeked = queue.First();
                    logger.Trace($"Peeked, returning {peeked}");
                    return peeked;
                }
            }

            logger.Trace($"Queue is empty, nothing to peek");
            return default(T);
        }

        /// <summary>
        /// Check if the queue is empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            logger.Trace($"Queue count: {Count}");
            return (Count == 0);
        }

        /// <summary>
        /// Takes and returns the first value from the queue
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            lock (queue)
            {
                if (!IsEmpty())
                {
                    T popped = queue.First();
                    queue.RemoveFirst();
                    logger.Debug($"Popped queue {popped}");
                    return popped;
                }
            }

            logger.Trace($"Nothing to pop");
            return default(T);
        }

        /// <summary>
        /// Returns a copy of the queue
        /// </summary>
        /// <returns></returns>
        public List<T> GetQueue()
        {
            lock (queue)
            {
                logger.Trace("Returning copy of the queue");
                return new List<T>(queue);
            }
        }

        /// <summary>
        /// Jump the queue, and add the entries to the beginning
        /// </summary>
        /// <param name="entries"></param>
        public void Jump(params T[] entries)
        {
            lock (queue)
            {
                foreach (T entry in entries)
                {
                    queue.AddFirst(entry);
                    logger.Debug($"Added to the front of the queue {entry}");
                }
            }
        }

        /// <summary>
        /// Add the entries to the end of the queue
        /// </summary>
        /// <param name="entries"></param>
        public void Add(params T[] entries)
        {
            lock (queue)
            {
                foreach (T entry in entries)
                {
                    queue.AddLast(entry);
                    logger.Debug($"Added to the end of the queue {entry}");
                }
            }
        }
    }
}
