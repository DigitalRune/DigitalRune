using System;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
    [TestFixture]
    public class MergeableNodeCollectionTest
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
        public void Constructor()
        {
            MergeableNodeCollection<NamedObject> nodeCollection = new MergeableNodeCollection<NamedObject>();
            Assert.AreEqual(0, nodeCollection.Count);
        }
    }
}
