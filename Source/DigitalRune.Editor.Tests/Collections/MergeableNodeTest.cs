using System;
using System.Linq;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
    [TestFixture]
    public class MergeableNodeTest
    {
        class NamedObject : INamedObject
        {
            public string Name { get; }


            public NamedObject(string name)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException(nameof(name));

                Name = name;
            }
        }


        [Test]
        public void Constructor1()
        {
            MergeableNode<NamedObject> node = new MergeableNode<NamedObject>();
            Assert.IsNull(node.Content);
            Assert.IsNull(node.Parent);
            Assert.That(node.Children, Is.Null.Or.Empty);
            Assert.IsNull(node.Next);
            Assert.IsNull(node.Previous);
            Assert.That(node.MergePoints.Count(), Is.EqualTo(1));
            Assert.That(node.MergePoints.First(), Is.EqualTo(new MergePoint(MergeOperation.Append, null)));
        }


        [Test]
        public void Constructor2()
        {
            NamedObject namedObject = new NamedObject("Name");
            MergeableNode<NamedObject> node = new MergeableNode<NamedObject>(namedObject);
            Assert.AreSame(namedObject, node.Content);
            Assert.IsNull(node.Parent);
            Assert.That(node.Children, Is.Null.Or.Empty);
            Assert.IsNull(node.Next);
            Assert.IsNull(node.Previous);
            Assert.That(node.MergePoints.Count(), Is.EqualTo(1));
            Assert.That(node.MergePoints.First(), Is.EqualTo(new MergePoint(MergeOperation.Append, null)));
        }


        [Test]
        public void Constructor3()
        {
            var namedObject = new NamedObject("Node");
            var children = new[]
            {
                new MergeableNode<NamedObject>(new NamedObject("ChildA")),
                new MergeableNode<NamedObject>(new NamedObject("ChildB")),
                new MergeableNode<NamedObject>(new NamedObject("ChildC")),
            };
            MergeableNode<NamedObject> node = new MergeableNode<NamedObject>(namedObject, children);
            Assert.AreSame(namedObject, node.Content);
            Assert.IsNull(node.Parent);
            Assert.IsNotNull(node.Children);
            Assert.AreEqual(3, node.Children.Count);
            Assert.AreSame(children[0], node.Children[0]);
            Assert.AreSame(children[1], node.Children[1]);
            Assert.AreSame(children[2], node.Children[2]);
            Assert.IsNull(node.Next);
            Assert.IsNull(node.Previous);
            Assert.That(node.MergePoints.Count(), Is.EqualTo(1));
            Assert.That(node.MergePoints.First(), Is.EqualTo(new MergePoint(MergeOperation.Append, null)));
        }


        [Test]
        public void GetRelativeNodeInGroup()
        {
            var namedObject = new NamedObject("Node");
            var children = new[]
            {
                new MergeableNode<NamedObject>(new NamedObject("ChildA")),
                new MergeableNode<NamedObject>(new NamedObject("ChildB")),
                new MergeableNode<NamedObject>(new NamedObject("ChildC")),
            };
            MergeableNode<NamedObject> node = new MergeableNode<NamedObject>(namedObject, children);

            Assert.IsNull(node.Children[0].Previous);
            Assert.AreSame(children[0], node.Children[1].Previous);
            Assert.AreSame(children[1], node.Children[2].Previous);
            Assert.AreSame(children[1], node.Children[0].Next);
            Assert.AreSame(children[2], node.Children[1].Next);
            Assert.IsNull(node.Children[2].Next);
        }


        [Test]
        public void Traversal()
        {
            var nodeCollection = new MergeableNodeCollection<NamedObject>();
            nodeCollection.AddRange(new[]
            {
                new MergeableNode<NamedObject>(new NamedObject("File")),
                new MergeableNode<NamedObject>(new NamedObject("Edit"),
                    new MergeableNode<NamedObject>(new NamedObject("Undo")),
                    new MergeableNode<NamedObject>(new NamedObject("Redo"))),
                new MergeableNode<NamedObject>(new NamedObject("Tools")),
                new MergeableNode<NamedObject>(new NamedObject("Help")),
            });

            // GetRoot()
            Assert.AreSame(nodeCollection[1], nodeCollection[1].Children[1].GetRoot());

            // GetAncestors()
            var ancestors = nodeCollection[1].Children[1].GetAncestors().ToList();
            Assert.AreEqual(1, ancestors.Count);
            Assert.AreSame(nodeCollection[1], ancestors[0]);

            // GetDescendants()
            var descendants = nodeCollection[1].GetDescendants().ToList();
            Assert.AreEqual(2, descendants.Count);
            Assert.AreSame(nodeCollection[1].Children[0], descendants[0]);
            Assert.AreSame(nodeCollection[1].Children[1], descendants[1]);

            descendants = nodeCollection[1].GetDescendants(false).ToList();
            Assert.AreEqual(2, descendants.Count);
            Assert.AreSame(nodeCollection[1].Children[0], descendants[0]);
            Assert.AreSame(nodeCollection[1].Children[1], descendants[1]);

            // GetSubtree()
            var subtree = nodeCollection[1].GetSubtree().ToList();
            Assert.AreEqual(3, subtree.Count);
            Assert.AreSame(nodeCollection[1], subtree[0]);
            Assert.AreSame(nodeCollection[1].Children[0], subtree[1]);
            Assert.AreSame(nodeCollection[1].Children[1], subtree[2]);

            subtree = nodeCollection[1].GetSubtree(false).ToList();
            Assert.AreEqual(3, subtree.Count);
            Assert.AreSame(nodeCollection[1], subtree[0]);
            Assert.AreSame(nodeCollection[1].Children[0], subtree[1]);
            Assert.AreSame(nodeCollection[1].Children[1], subtree[2]);
        }
    }
}
