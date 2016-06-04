using System;
using DigitalRune.Graphics.Scene3D;
using NUnit.Framework;


namespace DigitalRune.Graphics.Tests
{
  [TestFixture]
  public class SceneNodeCollectionTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowWhenOwnerIsNull()
    {
      new SceneNodeCollection(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ShouldThrowWhenOwnerHasCollection()
    {
      DummySceneNode node = new DummySceneNode();
      new SceneNodeCollection(node);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowWhenChildIsNull()
    {
      DummySceneNode node = new DummySceneNode();
      node.Children.Add(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowWhenChildIsNull2()
    {
      DummySceneNode node = new DummySceneNode();
      node.Children.Add(new DummySceneNode());
      node.Children[0] = null;
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ShouldThrowWhenChildExists()
    {
      DummySceneNode node = new DummySceneNode();
      DummySceneNode child = new DummySceneNode();
      node.Children.Add(child);
      node.Children.Add(child);
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ShouldThrowWhenChildExists2()
    {
      DummySceneNode node = new DummySceneNode();
      DummySceneNode childA = new DummySceneNode();
      DummySceneNode childB = new DummySceneNode();
      node.Children.Add(childA);
      node.Children.Add(childB);
      node.Children[1] = childA;
    }


    [Test]
    public void ReplaceChild()
    {
      DummySceneNode node = new DummySceneNode();
      DummySceneNode childA = new DummySceneNode();
      DummySceneNode childB = new DummySceneNode();
      node.Children.Add(childA);

      Assert.AreEqual(1, node.Children.Count);
      Assert.AreSame(childA, node.Children[0]);

      node.Children[0] = childB;
      Assert.AreEqual(1, node.Children.Count);
      Assert.AreSame(childB, node.Children[0]);
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ShoudThrowWhenChildHasParent()
    {
      DummySceneNode node = new DummySceneNode();
      DummySceneNode node2 = new DummySceneNode();
      DummySceneNode childA = new DummySceneNode();
      DummySceneNode childB = new DummySceneNode();
      node.Children.Add(childA);
      node2.Children.Add(childB);
      node.Children[0] = childB;
    }


    [Test]
    public void CheckSequence()
    {
      DummySceneNode node = new DummySceneNode();

      Assert.AreEqual(0, node.Children.Count);

      DummySceneNode nodeA = new DummySceneNode();
      DummySceneNode nodeB = new DummySceneNode();
      DummySceneNode nodeC = new DummySceneNode();
      node.Children.Add(nodeA);
      node.Children.Add(nodeB);
      node.Children.Add(nodeC);

      Assert.AreEqual(3, node.Children.Count);
      Assert.AreSame(nodeA, node.Children[0]);
      Assert.AreSame(nodeB, node.Children[1]);
      Assert.AreSame(nodeC, node.Children[2]);
    }


    [Test]
    public void ShouldSetParent()
    {
      DummySceneNode node = new DummySceneNode();

      DummySceneNode nodeA = new DummySceneNode();
      DummySceneNode nodeB = new DummySceneNode();
      DummySceneNode nodeC = new DummySceneNode();
      node.Children.Add(nodeA);
      node.Children.Add(nodeB);
      node.Children.Add(nodeC);

      Assert.AreSame(node, nodeA.Parent);
      Assert.AreSame(node, nodeB.Parent);
      Assert.AreSame(node, nodeC.Parent);
    }


    [Test]
    public void SwitchingCollection()
    {
      DummySceneNode node = new DummySceneNode();
      DummySceneNode node2 = new DummySceneNode();

      DummySceneNode nodeA = new DummySceneNode();
      DummySceneNode nodeB = new DummySceneNode();
      DummySceneNode nodeC = new DummySceneNode();
      node.Children.Add(nodeA);
      node.Children.Add(nodeB);
      node.Children.Add(nodeC);

      Assert.AreSame(node, nodeA.Parent);
      Assert.AreSame(node, nodeB.Parent);
      Assert.AreSame(node, nodeC.Parent);

      node.Children.Remove(nodeB);
      node.Children.Remove(nodeC);
      node2.Children.Add(nodeB);
      node2.Children.Add(nodeC);
      Assert.AreEqual(1, node.Children.Count);
      Assert.AreSame(node, nodeA.Parent);
      Assert.AreEqual(2, node2.Children.Count);
      Assert.AreSame(node2, nodeB.Parent);
      Assert.AreSame(node2, nodeC.Parent);
    }


    [Test]
    public void RemoveShouldResetParents()
    {
      DummySceneNode node = new DummySceneNode();

      DummySceneNode child = new DummySceneNode();
      node.Children.Add(child);

      Assert.AreSame(node, child.Parent);

      node.Children.Remove(child);

      Assert.IsNull(child.Parent);
    }


    [Test]
    public void ClearShouldResetParents()
    {
      DummySceneNode node = new DummySceneNode();

      Assert.AreEqual(0, node.Children.Count);

      node.Children.Clear();  // Test clear on empty collection.

      DummySceneNode nodeA = new DummySceneNode();
      DummySceneNode nodeB = new DummySceneNode();
      DummySceneNode nodeC = new DummySceneNode();
      node.Children.Add(nodeA);
      node.Children.Add(nodeB);
      node.Children.Add(nodeC);

      Assert.AreSame(node, nodeA.Parent);
      Assert.AreSame(node, nodeB.Parent);
      Assert.AreSame(node, nodeC.Parent);

      node.Children.Clear();

      Assert.IsNull(nodeA.Parent);
      Assert.IsNull(nodeB.Parent);
      Assert.IsNull(nodeC.Parent);
    }    
  }
}
