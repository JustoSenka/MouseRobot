using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotRuntime.Perf;
using System;
using System.Linq;

namespace Tests.Utils
{
    [TestClass]
    public class LimitedStackTests
    {
        [TestMethod]
        public void LimitedStack_HasCorrectCount_InNonOverflowScenario()
        {
            var stack = new LimitedStack<int>(5);

            stack.Add(1);
            stack.Add(2);
            stack.Add(3);

            Assert.AreEqual(3, stack.Count());
        }

        [TestMethod]
        public void LimitedStack_HasCorrectCount_InOverflowScenario()
        {
            var stack = new LimitedStack<int>(3);

            stack.Add(1);
            stack.Add(2);
            stack.Add(3);
            stack.Add(4);

            Assert.AreEqual(3, stack.Count());
        }

        [TestMethod]
        public void LimitedStack_HasCorrectValues_InOrder_NoOverflow()
        {
            var stack = new LimitedStack<int>(3);

            stack.Add(1);
            stack.Add(2);

            var arr = stack.ToArray();

            Assert.AreEqual(2, arr[0]);
            Assert.AreEqual(1, arr[1]);
        }

        [TestMethod]
        public void LimitedStack_HasCorrectValues_InOrder_AfterOverflow()
        {
            var stack = new LimitedStack<int>(3);

            stack.Add(1);
            stack.Add(2);
            stack.Add(3);
            stack.Add(4);
            stack.Add(5);

            var arr = stack.ToArray();

            Assert.AreEqual(5, arr[0]);
            Assert.AreEqual(4, arr[1]);
            Assert.AreEqual(3, arr[2]);
        }

        [TestMethod]
        public void LimitedStack_PopGivesBackCorrectValues_NoOverflow()
        {
            var stack = new LimitedStack<int>(4);

            stack.Add(1);
            stack.Add(2);
            stack.Add(3);

            Assert.AreEqual(3, stack.Pop());
            Assert.AreEqual(2, stack.Pop());
            Assert.AreEqual(1, stack.Count());

            Assert.AreEqual(1, stack.Pop());
            Assert.AreEqual(0, stack.Count());
        }

        [TestMethod]
        public void LimitedStack_PopGivesBackCorrectValues_AfterOverflow()
        {
            var stack = new LimitedStack<int>(4);

            stack.Add(1);
            stack.Add(2);
            stack.Add(3);
            stack.Add(4);
            stack.Add(5);
            stack.Add(6);
            stack.Add(7);

            Assert.AreEqual(7, stack.Pop());
            Assert.AreEqual(6, stack.Pop());
            Assert.AreEqual(2, stack.Count());

            Assert.AreEqual(5, stack.Pop());
            Assert.AreEqual(4, stack.Pop());
            Assert.AreEqual(0, stack.Count());
        }

        [TestMethod]
        public void LimitedStack_AddingValuesAfterPopping_WithOverflow_StillGivesCorrectOrder()
        {
            var stack = new LimitedStack<int>(4);

            stack.Add(1);
            stack.Add(2);
            stack.Pop();

            stack.Add(3);
            stack.Add(4);
            stack.Pop();

            stack.Add(5);
            stack.Add(6);
            stack.Pop();

            stack.Add(7);
            stack.Add(8);
            stack.Pop();
            stack.Add(9);

            var arr = stack.ToArray();

            Assert.AreEqual(9, arr[0]);
            Assert.AreEqual(7, arr[1]);
            Assert.AreEqual(5, arr[2]);
            Assert.AreEqual(3, arr[3]);
            Assert.AreEqual(4, arr.Length);
        }

        [TestMethod]
        public void LimitedStack_PopingEmptyStack_Throws()
        {
            var stack = new LimitedStack<int>(4);

            stack.Add(1);
            stack.Pop();

            bool gotException = false;
            try
            {
                stack.Pop();
            }
            catch (Exception)
            {
                gotException = true;
            }

            Assert.IsTrue(gotException, "No exception if poping empty stack");
        }
    }
}
