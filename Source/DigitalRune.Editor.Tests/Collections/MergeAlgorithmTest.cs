using System;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
    [TestFixture]
    public class MergeAlgorithmTest
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


        private MergeableNodeCollection<NamedObject> _emptyNodeCollection;
        private MergeableNodeCollection<NamedObject> _nodeCollection;


        [SetUp]
        public void SetUp()
        {
            _emptyNodeCollection = new MergeableNodeCollection<NamedObject>();

            _nodeCollection = new MergeableNodeCollection<NamedObject>();
            _nodeCollection.AddRange(new[]
            {
                new MergeableNode<NamedObject>(new NamedObject("File")),
                new MergeableNode<NamedObject>(new NamedObject("Edit"),
                    new MergeableNode<NamedObject>(new NamedObject("Undo")),
                    new MergeableNode<NamedObject>(new NamedObject("Redo"))),
                new MergeableNode<NamedObject>(new NamedObject("Tools")),
                new MergeableNode<NamedObject>(new NamedObject("Help")),
            });
        }


        [Test]
        public void MergeShouldThrowWhenNull()
        {
            MergeAlgorithm<NamedObject> mergeAlgorithm = new MergeAlgorithm<NamedObject>();
            Assert.Throws<ArgumentNullException>(() => mergeAlgorithm.Merge(null, _emptyNodeCollection));
            Assert.Throws<ArgumentNullException>(() => mergeAlgorithm.Merge(_emptyNodeCollection, null));
        }


        [Test]
        public void AppendToEmptyCollection()
        {
            MergeAlgorithm<NamedObject> mergeAlgorithm = new MergeAlgorithm<NamedObject>();
            mergeAlgorithm.Merge(_emptyNodeCollection, _nodeCollection);

            Assert.AreEqual(4, _emptyNodeCollection.Count);
            Assert.AreEqual("File", _emptyNodeCollection[0].Content.Name);
            Assert.That(_emptyNodeCollection[0].Children, Is.Null.Or.Empty);
            Assert.AreEqual("Edit", _emptyNodeCollection[1].Content.Name);
            Assert.AreEqual(2, _emptyNodeCollection[1].Children.Count);
            Assert.AreEqual("Undo", _emptyNodeCollection[1].Children[0].Content.Name);
            Assert.That(_emptyNodeCollection[1].Children[0].Children, Is.Null.Or.Empty);
            Assert.AreEqual("Redo", _emptyNodeCollection[1].Children[1].Content.Name);
            Assert.That(_emptyNodeCollection[1].Children[1].Children, Is.Null.Or.Empty);
            Assert.AreEqual("Tools", _emptyNodeCollection[2].Content.Name);
            Assert.That(_emptyNodeCollection[2].Children, Is.Null.Or.Empty);
            Assert.AreEqual("Help", _emptyNodeCollection[3].Content.Name);
            Assert.That(_emptyNodeCollection[3].Children, Is.Null.Or.Empty);
        }


        [Test]
        public void MergeShouldThrowWhenContentIsNull()
        {
            var nodeCollection = new MergeableNodeCollection<NamedObject>();
            nodeCollection.AddRange(new[]
            {
                new MergeableNode<NamedObject>(),
            });

            MergeAlgorithm<NamedObject> mergeAlgorithm = new MergeAlgorithm<NamedObject>();
            Assert.Throws<NotSupportedException>(() => mergeAlgorithm.Merge(_emptyNodeCollection, nodeCollection));
        }


        [Test]
        public void ComplexMerge()
        {
            var nodeCollection = new MergeableNodeCollection<NamedObject>();
            nodeCollection.AddRange(new[]
            {
                new MergeableNode<NamedObject>(new NamedObject("Ignore1"), new MergePoint(MergeOperation.Ignore, null)),
                new MergeableNode<NamedObject>(new NamedObject("Ignore2"), new MergePoint(MergeOperation.Match, "Unknown")),
                new MergeableNode<NamedObject>(new NamedObject("Ignore3"), new MergePoint(MergeOperation.InsertBefore, "Unknown")),
                new MergeableNode<NamedObject>(new NamedObject("Ignore4"), new MergePoint(MergeOperation.InsertAfter, "Unknown")),
                new MergeableNode<NamedObject>(new NamedObject("Ignore5"), new MergePoint(MergeOperation.Replace, "Unknown")),
                new MergeableNode<NamedObject>(new NamedObject("Ignore6"), new MergePoint(MergeOperation.Remove, "Unknown")),

                new MergeableNode<NamedObject>(new NamedObject("PostHelp")),
                new MergeableNode<NamedObject>(new NamedObject("PreFile"), new MergePoint(MergeOperation.Prepend, null)),
                new MergeableNode<NamedObject>(new NamedObject("DoNothing"), new MergePoint(MergeOperation.Match, "Help")),
                new MergeableNode<NamedObject>(new NamedObject("View"), new MergePoint(MergeOperation.InsertAfter, "Edit")),
                new MergeableNode<NamedObject>(new NamedObject("Windows"), new MergePoint(MergeOperation.InsertBefore, "Help")),
                new MergeableNode<NamedObject>(new NamedObject("HelpNew"), new MergePoint(MergeOperation.Replace, "Help")),
                new MergeableNode<NamedObject>(new NamedObject("RemoveTools"), new MergePoint(MergeOperation.Remove, "Tools")),

                new MergeableNode<NamedObject>(
                    new NamedObject("Edit"),
                    new MergePoint(MergeOperation.Append),
                        new MergeableNode<NamedObject>(new NamedObject("Cut"), new MergePoint(MergeOperation.InsertAfter, "Unkown"), new MergePoint(MergeOperation.InsertAfter, "Redo")),
                        new MergeableNode<NamedObject>(new NamedObject("Paste"), new MergePoint(MergeOperation.InsertAfter, "Unkown"), new MergePoint(MergeOperation.Append)),
                        new MergeableNode<NamedObject>(new NamedObject("Copy"), new MergePoint(MergeOperation.InsertAfter, "Unkown"), new MergePoint(MergeOperation.InsertBefore, "Paste")),
                        new MergeableNode<NamedObject>(new NamedObject("Format"),
                            new MergeableNode<NamedObject>(new NamedObject("Comment")),
                            new MergeableNode<NamedObject>(new NamedObject("Uncomment"))),
                        new MergeableNode<NamedObject>(new NamedObject("PreUndo"), new MergePoint(MergeOperation.InsertAfter, "Unkown"), new MergePoint(MergeOperation.Prepend)),
                        new MergeableNode<NamedObject>(new NamedObject("Ignore"), new MergePoint(MergeOperation.InsertAfter, "Unkown"), new MergePoint(MergeOperation.Ignore)),
                        new MergeableNode<NamedObject>(new NamedObject("UndoNew"), new MergePoint(MergeOperation.InsertAfter, "Unkown"), new MergePoint(MergeOperation.Replace, "Undo")),
                        new MergeableNode<NamedObject>(new NamedObject("Redo"), new MergePoint(MergeOperation.InsertAfter, "Unkown"), new MergePoint(MergeOperation.Remove, "Redo"))),
            });

            MergeAlgorithm<NamedObject> mergeAlgorithm = new MergeAlgorithm<NamedObject>();
            mergeAlgorithm.Merge(_nodeCollection, nodeCollection);

            Assert.AreEqual(7, _nodeCollection.Count);
            Assert.AreEqual("PreFile", _nodeCollection[0].Content.Name);
            Assert.That(_nodeCollection[0].Children, Is.Null.Or.Empty);
            Assert.AreEqual("File", _nodeCollection[1].Content.Name);
            Assert.That(_nodeCollection[1].Children, Is.Null.Or.Empty);

            Assert.AreEqual("Edit", _nodeCollection[2].Content.Name);
            Assert.AreEqual(6, _nodeCollection[2].Children.Count);
            Assert.AreEqual("PreUndo", _nodeCollection[2].Children[0].Content.Name);
            Assert.That(_nodeCollection[2].Children[0].Children, Is.Null.Or.Empty);
            Assert.AreEqual("UndoNew", _nodeCollection[2].Children[1].Content.Name);
            Assert.That(_nodeCollection[2].Children[1].Children, Is.Null.Or.Empty);
            Assert.AreEqual("Cut", _nodeCollection[2].Children[2].Content.Name);
            Assert.That(_nodeCollection[2].Children[2].Children, Is.Null.Or.Empty);
            Assert.AreEqual("Copy", _nodeCollection[2].Children[3].Content.Name);
            Assert.That(_nodeCollection[2].Children[3].Children, Is.Null.Or.Empty);
            Assert.AreEqual("Paste", _nodeCollection[2].Children[4].Content.Name);
            Assert.That(_nodeCollection[2].Children[4].Children, Is.Null.Or.Empty);
            Assert.AreEqual("Format", _nodeCollection[2].Children[5].Content.Name);
            Assert.AreEqual(2, _nodeCollection[2].Children[5].Children.Count);
            Assert.AreEqual("Comment", _nodeCollection[2].Children[5].Children[0].Content.Name);
            Assert.That(_nodeCollection[2].Children[5].Children[0].Children, Is.Null.Or.Empty);
            Assert.AreEqual("Uncomment", _nodeCollection[2].Children[5].Children[1].Content.Name);
            Assert.That(_nodeCollection[2].Children[5].Children[1].Children, Is.Null.Or.Empty);

            Assert.AreEqual("View", _nodeCollection[3].Content.Name);
            Assert.That(_nodeCollection[3].Children, Is.Null.Or.Empty);
            Assert.AreEqual("Windows", _nodeCollection[4].Content.Name);
            Assert.That(_nodeCollection[4].Children, Is.Null.Or.Empty);
            Assert.AreEqual("HelpNew", _nodeCollection[5].Content.Name);
            Assert.That(_nodeCollection[5].Children, Is.Null.Or.Empty);
            Assert.AreEqual("PostHelp", _nodeCollection[6].Content.Name);
            Assert.That(_nodeCollection[6].Children, Is.Null.Or.Empty);
        }
    }
}
