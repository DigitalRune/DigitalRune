using System;
using DigitalRune.Graphics.Scene3D;
using NUnit.Framework;


namespace DigitalRune.Graphics.Tests
{
  [TestFixture]
  public class SceneTest
  {
    [Test]
    public void ConstructorTest()
    {
      DefaultScene scene = new DefaultScene();

      Assert.IsNotNull(scene.CollisionDomain);
      Assert.AreEqual(0, scene.CollisionDomain.CollisionObjects.Count);
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ShouldThrowWhenSceneIsAddedToAsChild()
    {
      DefaultScene scene = new DefaultScene();

      DummySceneNode node = new DummySceneNode();
      node.Children.Add(scene);
    }


    [Test]
    public void AddRemoveSingleNode()
    {
      DefaultScene scene = new DefaultScene();

      DummySceneNode node = new DummySceneNode();
      scene.Children.Add(node);

      Assert.AreEqual(1, scene.CollisionDomain.CollisionObjects.Count);
      Assert.AreSame(node, scene.CollisionDomain.CollisionObjects[0].GeometricObject);

      scene.Children.Remove(node);
      Assert.AreEqual(0, scene.CollisionDomain.CollisionObjects.Count);
    }


    [Test]
    public void AddRemoveTreeOfNodesTest()
    {
      DefaultScene scene = new DefaultScene();

      DummySceneNode nodeA = new DummySceneNode { Name = "A" };
      DummySceneNode nodeB = new DummySceneNode { Name = "B" };
      DummySceneNode nodeC = new DummySceneNode { Name = "C" };
      DummySceneNode nodeD = new DummySceneNode { Name = "D" };
      nodeA.Children.Add(nodeB);
      nodeA.Children.Add(nodeC);
      nodeB.Children.Add(nodeD);

      // Add a tree of nodes.
      scene.Children.Add(nodeA);

      Assert.AreEqual(4, scene.CollisionDomain.CollisionObjects.Count);
      Assert.IsNotNull(scene.CollisionDomain.CollisionObjects.Get(nodeA));
      Assert.IsNotNull(scene.CollisionDomain.CollisionObjects.Get(nodeB));
      Assert.IsNotNull(scene.CollisionDomain.CollisionObjects.Get(nodeC));
      Assert.IsNotNull(scene.CollisionDomain.CollisionObjects.Get(nodeD));

      // Add a single node to tree.
      DummySceneNode nodeE = new DummySceneNode();
      nodeD.Children.Add(nodeE);

      Assert.AreEqual(5, scene.CollisionDomain.CollisionObjects.Count);
      Assert.IsNotNull(scene.CollisionDomain.CollisionObjects.Get(nodeE));

      // Remove a subtree.
      nodeB.Children.Remove(nodeD);

      Assert.AreEqual(3, scene.CollisionDomain.CollisionObjects.Count);
      Assert.IsNotNull(scene.CollisionDomain.CollisionObjects.Get(nodeA));
      Assert.IsNotNull(scene.CollisionDomain.CollisionObjects.Get(nodeB));
      Assert.IsNotNull(scene.CollisionDomain.CollisionObjects.Get(nodeC));

      // Remove tree.
      scene.Children.Remove(nodeA);
      Assert.AreEqual(0, scene.CollisionDomain.CollisionObjects.Count);
    }
  }
}
