using System.Collections.Generic;
using System.Linq;

namespace TrboPortal.TrboNet
{
    public class Queue<T>
    {
        private LinkedList<T> queue;

        public Queue()
        {
            queue = new LinkedList<T>();
        }

        /// <summary>
        /// Remove all occurences of device from the queue
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private bool Remove(T device)
        {
            bool removed = false;
            lock (queue)
            {
                var node = queue.First;
                while (node != null)
                {
                    var nextNode = node.Next;
                    if (EqualityComparer<T>.Default.Equals(node.Value, device))
                    {
                        queue.Remove(node);
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
        private T Peek()
        {
            lock (queue)
            {
                if (!IsEmpty())
                {
                    return queue.First();
                }
            }

            return default(T);
        }

        /// <summary>
        /// Check if the queue is empty
        /// </summary>
        /// <returns></returns>
        private bool IsEmpty()
        {
            return (queue.Count == 0);
        }

        /// <summary>
        /// Takes and returns the first value from the queue
        /// </summary>
        /// <returns></returns>
        private T Pop()
        {
            lock (queue)
            {
                if (!IsEmpty())
                {
                    T entry = queue.First();
                    queue.RemoveFirst();
                    return entry;
                }
            }
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
                return new List<T>(queue);
            }
        }

        /// <summary>
        /// Jump the queue, and add the entries to the beginning
        /// </summary>
        /// <param name="entries"></param>
        private void Jump(params T[] entries)
        {
            lock (queue)
            {
                foreach (T entry in entries)
                {
                    queue.AddFirst(entry);
                }
            }
        }

        /// <summary>
        /// Add the entries to the end of the queue
        /// </summary>
        /// <param name="entries"></param>
        private void Add(params T[] entries)
        {
            lock (queue)
            {
                foreach (T entry in entries)
                {
                    queue.AddLast(entry);
                }
            }
        }
    }
}
