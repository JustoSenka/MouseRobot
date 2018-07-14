using RobotRuntime;
using RobotRuntime.Scripts;
using RobotRuntime.Tests;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RobotEditor.Hierarchy
{
    public class TestNode : IEnumerable<TestNode>
    {
        public int Level { get; private set; }

        public object Value { get; private set; }

        public TestFixture TestFixture { get; private set; }
        public Script Script { get; private set; }

        public List<TestNode> Children { get; private set; }
        public TestNode Parent { get; set; }

        public TestNode(Script script, TestNode parent)
        {
            Value = script;
            Script = script;
            Parent = parent;
            Level = Parent.Level + 1;

            Children = new List<TestNode>();
        }

        public TestNode(object value)
        {
            Value = value;
            Children = new List<TestNode>();
        }

        public TestNode(TestFixture testFixture, int overrideLevel = 0)
        {
            Value = testFixture;
            TestFixture = testFixture;
            Level = overrideLevel;

            Children = new List<TestNode>();

            foreach (var script in testFixture.Tests)
                Children.Add(new TestNode(script, this));
        }

        public void AddTestNode(TestNode node)
        {
            node.Parent = this;
            node.Level = Level + 1;
            Children.Add(node);
        }

        public TestNode TopLevelNode
        {
            get
            {
                var node = this;
                while (node.Level != 0)
                    node = node.Parent;

                return node;
            }
        }

        public void Update(Script script)
        {
            Script = script;
            Value = script;
        }

        public void Update(TestFixture testFixture)
        {
            TestFixture = testFixture;
            Value = testFixture;

            Children = new List<TestNode>();

            foreach (var script in testFixture.Tests)
                Children.Add(new TestNode(script, this));
        }

        public TestNode GetNodeFromValue(Command command)
        {
            return GetAllNodes().FirstOrDefault(n => n.Value.Equals(command));
        }

        /// <summary>
        /// Returns all nodes in the tree hierarchy recursively, including all child and grandchild nodes
        /// </summary>
        public IEnumerable<TestNode> GetAllNodes(bool includeSelf = true)
        {
            if (includeSelf)
                yield return this;

            foreach (var c in Children)
            {
                yield return c;
                foreach (var child in c.GetAllNodes(false))
                    yield return child;
            }
        }

        public IEnumerator<TestNode> GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
