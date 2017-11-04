using System;
using System.Collections;
using System.Collections.Generic;

namespace RobotRuntime.Perf
{
    public class LimitedStack<T> : IEnumerable<T>
    {
        public int Limit { get; private set; }

        private int m_First;
        private int m_Last;
        private T[] m_Data;

        public LimitedStack(int limit)
        {
            Limit = limit;

            m_First = -1;
            m_Last = 0;
            m_Data = new T[limit];
        }

        public void Add(T value)
        {
            m_First++;
            if (m_First >= Limit)
            {
                m_First = 0;

                if (m_First == m_Last)
                    m_Last++;
            }
            else
            {
                if (m_First == m_Last && m_First != 0)
                    m_Last++;
            }

            if (m_Last >= Limit)
                m_Last = 0;

            m_Data[m_First] = value;
        }

        public T Pop()
        {
            if (m_First == -1)
                throw new InvalidOperationException("Stack is empty");

            T ret = m_Data[m_First];
            m_Data[m_First] = default(T);
            if (m_First == m_Last)
            {
                m_First = -1;
                m_Last = 0;
            }
            else
            {
                m_First--;

                if (m_First < 0)
                    m_First = Limit - 1;
            }

            return ret;
        }

        public T this[int i]
        {
            get { return m_Data[i]; }
            set { m_Data[i] = value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (m_First == -1)
                yield break;

            if (m_First >= m_Last)
            {
                for (int i = m_First; i >= m_Last; --i)
                    yield return m_Data[i];
            }
            else
            {
                for (int i = m_First; i >= 0; --i)
                    yield return m_Data[i];

                for (int i = Limit - 1; i >= m_Last; --i)
                    yield return m_Data[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
