using NUnit.Framework;
using RobotRuntime;

namespace Tests.Utils
{
    [TestFixture]
    public class LinkedTreeTests
    {
        [Test]
        public void LinkedTree_GetChild_ReturnsCorrectChild()
        {
            var tree = new TreeNode<int>();

            tree.AddChild(0);
            tree.AddChild(1);
            tree.AddChild(2);

            Assert.AreEqual(0, tree.GetChild(0).value);
            Assert.AreEqual(1, tree.GetChild(1).value);
            Assert.AreEqual(2, tree.GetChild(2).value);
        }

        [Test]
        public void LinkedTree_InsertChild_InsertsInCorrectPositions()
        {
            var tree = new TreeNode<int>();

            tree.AddChild(1);
            tree.AddChild(3);
            tree.AddChild(4);

            tree.Insert(0, 0);
            tree.Insert(2, 2);
            tree.Insert(5, 5);

            Assert.AreEqual(0, tree.GetChild(0).value);
            Assert.AreEqual(1, tree.GetChild(1).value);
            Assert.AreEqual(2, tree.GetChild(2).value);
            Assert.AreEqual(3, tree.GetChild(3).value);
            Assert.AreEqual(4, tree.GetChild(4).value);
            Assert.AreEqual(5, tree.GetChild(5).value);
        }

        [Test]
        public void LinkedTree_IndexOf_InsertsInCorrectPositions()
        {
            var tree = new TreeNode<int>();

            tree.AddChild(0);
            tree.AddChild(1);
            tree.AddChild(2);

            Assert.AreEqual(0, tree.IndexOf(0));
            Assert.AreEqual(1, tree.IndexOf(1));
            Assert.AreEqual(2, tree.IndexOf(2));
        }

        [Test]
        public void LinkedTree_MoveAfter_MiddleWorksFine()
        {
            var tree = new TreeNode<int>();

            tree.AddChild(0);
            tree.AddChild(1);
            tree.AddChild(2);

            tree.MoveAfter(0, 1);

            Assert.AreEqual(1, tree.GetChild(0).value);
            Assert.AreEqual(0, tree.GetChild(1).value);
            Assert.AreEqual(2, tree.GetChild(2).value);
        }

        [Test]
        public void LinkedTree_MoveAfter_LastWorksFine()
        {
            var tree = new TreeNode<int>();

            tree.AddChild(0);
            tree.AddChild(1);
            tree.AddChild(2);

            tree.MoveAfter(0, 2);

            Assert.AreEqual(1, tree.GetChild(0).value);
            Assert.AreEqual(2, tree.GetChild(1).value);
            Assert.AreEqual(0, tree.GetChild(2).value);
        }

        [Test]
        public void LinkedTree_MoveAfter_FirstWorksFine()
        {
            var tree = new TreeNode<int>();

            tree.AddChild(0);
            tree.AddChild(1);
            tree.AddChild(2);
            tree.AddChild(3);

            tree.MoveAfter(2, 0);

            Assert.AreEqual(0, tree.GetChild(0).value);
            Assert.AreEqual(2, tree.GetChild(1).value);
            Assert.AreEqual(1, tree.GetChild(2).value);
            Assert.AreEqual(3, tree.GetChild(3).value);
        }

        [Test]
        public void LinkedTree_MoveBefore_MiddleWorksFine()
        {
            var tree = new TreeNode<int>();

            tree.AddChild(0);
            tree.AddChild(1);
            tree.AddChild(2);

            tree.MoveBefore(2, 1);

            Assert.AreEqual(0, tree.GetChild(0).value);
            Assert.AreEqual(2, tree.GetChild(1).value);
            Assert.AreEqual(1, tree.GetChild(2).value);
        }

        [Test]
        public void LinkedTree_MoveBefore_LastWorksFine()
        {
            var tree = new TreeNode<int>();

            tree.AddChild(0);
            tree.AddChild(1);
            tree.AddChild(2);

            tree.MoveBefore(0, 2);

            Assert.AreEqual(1, tree.GetChild(0).value);
            Assert.AreEqual(0, tree.GetChild(1).value);
            Assert.AreEqual(2, tree.GetChild(2).value);
        }

        [Test]
        public void LinkedTree_MoveBefore_FirstWorksFine()
        {
            var tree = new TreeNode<int>();

            tree.AddChild(0);
            tree.AddChild(1);
            tree.AddChild(2);
            tree.AddChild(3);

            tree.MoveBefore(2, 0);

            Assert.AreEqual(2, tree.GetChild(0).value);
            Assert.AreEqual(0, tree.GetChild(1).value);
            Assert.AreEqual(1, tree.GetChild(2).value);
            Assert.AreEqual(3, tree.GetChild(3).value);
        }

        [Test]
        public void LinkedTree_Parent_GivesCorrectParent()
        {
            var tree = new TreeNode<string>("recording");

            var node_0 = tree.AddChild("0");
            var node_1 = tree.AddChild("1");

            node_0.AddChild("00");
            node_0.AddChild("01");

            node_1.AddChild("10");
            var node_11 = node_1.AddChild("11");

            Assert.AreEqual("11", node_11.value);
            Assert.AreEqual("1", node_11.parent.value);
            Assert.AreEqual("recording", node_11.parent.parent.value);
        }

        [Test]
        public void LinkedTree_GetNodeFromValue_GivesCorrectNode()
        {
            var tree = new TreeNode<string>("recording");

            var node_0 = tree.AddChild("0");
            var node_1 = tree.AddChild("1");

            node_0.AddChild("00");
            node_0.AddChild("01");

            node_1.AddChild("10");
            var node_11 = node_1.AddChild("11");

            Assert.AreEqual("0", tree.GetNodeFromValue("0").value);
            Assert.AreEqual("1", tree.GetNodeFromValue("1").value);
            Assert.AreEqual("00", tree.GetNodeFromValue("00").value);
            Assert.AreEqual("01", tree.GetNodeFromValue("01").value);
            Assert.AreEqual("10", tree.GetNodeFromValue("10").value);
            Assert.AreEqual("11", tree.GetNodeFromValue("11").value);
        }

        [Test]
        public void LinkedTree_MoveAfter_CanWorkWithDifferentNodeLevels()
        {
            var tree = new TreeNode<string>("recording");

            var node_0 = tree.AddChild("0");
            var node_1 = tree.AddChild("1");

            node_0.AddChild("00");
            node_0.AddChild("01");

            node_1.AddChild("10");
            node_1.AddChild("11");

            tree.MoveAfter("00", "11");

            Assert.AreEqual("01", node_0.GetChild(0).value);
            Assert.AreEqual("10", node_1.GetChild(0).value);
            Assert.AreEqual("11", node_1.GetChild(1).value);
            Assert.AreEqual("00", node_1.GetChild(2).value);
        }

        [Test]
        public void LinkedTree_MoveBefore_CanWorkWithMultipleObjects()
        {
            var tree = new TreeNode<string>("recording");

            var node_0 = tree.AddChild("0");
            var node_1 = tree.AddChild("1");

            node_0.AddChild("00");
            node_0.AddChild("01");

            node_1.AddChild("10");
            node_1.AddChild("11");

            tree.MoveBefore("0", "11");

            Assert.AreEqual("1", tree.GetChild(0).value);
            Assert.AreEqual("10", node_1.GetChild(0).value);
            Assert.AreEqual("0", node_1.GetChild(1).value);
            Assert.AreEqual("11", node_1.GetChild(2).value);
        }

        [Test]
        public void LinkedTree_Clone_MakesDeepCopy()
        {
            var tree = new TreeNode<int>();

            tree.AddChild(0);
            tree.AddChild(1);
            tree.AddChild(2);

            var newTree = (TreeNode<int>)tree.Clone();

            tree.GetChild(0).value = 5;
            tree.RemoveAt(1);

            Assert.AreEqual(0, newTree.GetChild(0).value);
            Assert.AreEqual(1, newTree.GetChild(1).value);
            Assert.AreEqual(2, newTree.GetChild(2).value);
        }
    }
}
