using System.Collections;
using System.Collections.Generic;

namespace RobotRuntime
{
    public class CustomStack<T> : IEnumerable<T>
    {
        private List<T> items = new List<T>();

        public void Push(T item)
        {
            items.Add(item);
        }

        public T Pop()
        {
            if (items.Count > 0)
            {
                T temp = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                return temp;
            }
            else
                return default(T);
        }

        public T Peek()
        {
            if (items.Count > 0)
            {
                return items[items.Count - 1];
            }
            else
                return default(T);
        }

        public void Remove(T item)
        {
            items.Remove(item);
        }

        public IEnumerator GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}
