using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RobotRuntime
{
    public class PriorityStack<T>
    {
        private int total_size;
        private SortedDictionary<int, CustomStack<T>> storage;

        public PriorityStack()
        {
            this.storage = new SortedDictionary<int, CustomStack<T>>();
            this.total_size = 0;
        }

        public bool IsEmpty()
        {
            return (total_size == 0);
        }

        public void Remove(T value)
        {
            foreach (CustomStack<T> stack in storage.Values)
            {
                if (stack.Contains(value))
                    stack.Remove(value);
            }
            return; 
        }

        public void RemoveFirst()
        {
            if (IsEmpty())
                throw new Exception("Please check that priorityList is not empty");

            foreach (CustomStack<T> stack in storage.Values)
            {
                if (stack.Count() > 0)
                {
                    total_size--;
                    stack.Pop();
                    return;
                }
            }

            Debug.Assert(false, "not supposed to reach here. problem with changing total_size");
            return; // not supposed to reach here.
        }

        public T First()
        {
            if (IsEmpty())
                throw new Exception("Please check that priorityList is not empty");

            foreach (CustomStack<T> stack in storage.Values)
            {
                if (stack.Count() > 0)
                    return stack.Peek();
            }

            Debug.Assert(false, "not supposed to reach here. problem with changing total_size");
            return default(T); // not supposed to reach here.
        }

       /*  public void RemoveFirst(int priority)
        {
            total_size--;
            storage[priority].Pop();
        }*/

        public void Add(T item, int priority)
        {
            if (!storage.ContainsKey(priority))
            {
                storage.Add(priority, new CustomStack<T>());
            }
            storage[priority].Push(item);
            total_size++;
        }
    }
}
