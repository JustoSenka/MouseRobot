﻿using NUnit.Framework;
using RobotRuntime;
using System.Collections.Generic;

namespace Tests.Utils
{
    [TestFixture]
    public class ListExtensionTests
    {
        IList<int> list;

        [Test]
        public void MoveFirstAfter_Middle_Works()
        {
            list = new List<int> { 0, 1, 2, 3 };
            list.MoveAfter(0, 2);
            Assert.AreEqual(0, list[2]);
            Assert.AreEqual(4, list.Count);
        }

        [Test]
        public void MoveFirstBefore_Middle_Works()
        {
            list = new List<int> { 0, 1, 2, 3 };
            list.MoveBefore(0, 2);
            Assert.AreEqual(0, list[1]);
            Assert.AreEqual(4, list.Count);
        }

        [Test]
        public void MoveLastAfter_Middle_Works()
        {
            list = new List<int> { 0, 1, 2, 3 };
            list.MoveAfter(3, 1);
            Assert.AreEqual(3, list[2]);
            Assert.AreEqual(4, list.Count);
        }

        [Test]
        public void MoveLastBefore_Middle_Works()
        {
            list = new List<int> { 0, 1, 2, 3 };
            list.MoveBefore(3, 1);
            Assert.AreEqual(3, list[1]);
            Assert.AreEqual(4, list.Count);
        }



        [Test]
        public void MoveAfter_First_PutsToSecondPosition()
        {
            list = new List<int> { 0, 1, 2, 3 };
            list.MoveAfter(2, 0);
            Assert.AreEqual(2, list[1]);
        }

        [Test]
        public void MoveBefore_First_PutsToFirstPosition()
        {
            list = new List<int> { 0, 1, 2, 3 };
            list.MoveBefore(2, 0);
            Assert.AreEqual(2, list[0]);
        }

        [Test]
        public void MoveAfter_Last_PutsToLastPosition()
        {
            list = new List<int> { 0, 1, 2, 3 };
            list.MoveAfter(1, 3);
            Assert.AreEqual(1, list[3]);
        }

        [Test]
        public void MoveBefore_Last_PutsToSecondFromEndPosition()
        {
            list = new List<int> { 0, 1, 2, 3 };
            list.MoveBefore(1, 3);
            Assert.AreEqual(1, list[2]);
        }



        [Test]
        public void MoveSelfAfter_First_PutsToSamePosition()
        {
            list = new List<int> { 0, 1, 2, 3 };
            list.MoveAfter(0, 0);
            Assert.AreEqual(0, list[0]);
        }

        [Test]
        public void MoveSelfBefore_First_PutsToSamePosition()
        {
            list = new List<int> { 0, 1, 2, 3 };
            list.MoveBefore(0, 0);
            Assert.AreEqual(0, list[0]);
        }

        [Test]
        public void MoveSelfAfter_Last_PutsToLastPosition()
        {
            list = new List<int> { 0, 1, 2, 3 };
            list.MoveAfter(3, 3);
            Assert.AreEqual(3, list[3]);
        }

        [Test]
        public void MoveSelfBefore_Last_PutsToLastPosition()
        {
            list = new List<int> { 0, 1, 2, 3 };
            list.MoveBefore(3, 3);
            Assert.AreEqual(3, list[3]);
        }

    }
}
