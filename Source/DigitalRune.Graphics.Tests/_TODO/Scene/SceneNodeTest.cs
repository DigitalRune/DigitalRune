using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Graphics.Tests
{
  [TestFixture]
  public class SceneNodeTest
  {
    [Test]
    public void GetAncestorsOfRoot()
    {
      DummySceneNode nodeA = new DummySceneNode();
      Assert.AreEqual(0, nodeA.GetAncestors().Count());
    }


    [Test]
    public void GetAncestors()
    {
      DummySceneNode nodeA = new DummySceneNode();
      DummySceneNode nodeB = new DummySceneNode();
      DummySceneNode nodeC = new DummySceneNode();
      DummySceneNode nodeD = new DummySceneNode();
      nodeA.Children.Add(nodeB);
      nodeA.Children.Add(nodeC);
      nodeB.Children.Add(nodeD);

      var ancestors = nodeD.GetAncestors().ToArray();
      Assert.AreEqual(2, ancestors.Length);
      Assert.AreSame(nodeB, ancestors[0]);
      Assert.AreSame(nodeA, ancestors[1]);
    }


    [Test]
    public void GetDescendantsOfLeaf()
    {
      DummySceneNode nodeA = new DummySceneNode();      
      Assert.AreEqual(0, nodeA.GetDescendants().Count());
    }


    [Test]
    public void GetDescendantsDepthFirst()
    {
      DummySceneNode nodeA = new DummySceneNode();
      DummySceneNode nodeB = new DummySceneNode();
      DummySceneNode nodeC = new DummySceneNode();
      DummySceneNode nodeD = new DummySceneNode();
      nodeA.Children.Add(nodeB);
      nodeA.Children.Add(nodeC);
      nodeB.Children.Add(nodeD);

      var descendants = nodeA.GetDescendants().ToArray();
      Assert.AreEqual(3, descendants.Length);
      Assert.AreSame(nodeB, descendants[0]);
      Assert.AreSame(nodeD, descendants[1]);
      Assert.AreSame(nodeC, descendants[2]);
    }


    [Test]
    public void GetDescendantsBreadthFirst()
    {
      DummySceneNode nodeA = new DummySceneNode();
      DummySceneNode nodeB = new DummySceneNode();
      DummySceneNode nodeC = new DummySceneNode();
      DummySceneNode nodeD = new DummySceneNode();
      nodeA.Children.Add(nodeB);
      nodeA.Children.Add(nodeC);
      nodeB.Children.Add(nodeD);

      var descendants = nodeA.GetDescendants(false).ToArray();
      Assert.AreEqual(3, descendants.Length);
      Assert.AreSame(nodeB, descendants[0]);
      Assert.AreSame(nodeC, descendants[1]);
      Assert.AreSame(nodeD, descendants[2]);
    }


    [Test]
    public void ManipulatePoses()
    {
      DummySceneNode nodeA = new DummySceneNode { PoseLocal = new Pose(new Vector3F(1, 1, 1)) };
      DummySceneNode nodeB = new DummySceneNode { PoseLocal = new Pose(new Vector3F(1, 2, -3)) };
      DummySceneNode nodeC = new DummySceneNode { PoseLocal = new Pose(new Vector3F(1, 2, 3)) };
      DummySceneNode nodeD = new DummySceneNode { PoseLocal = new Pose(new Vector3F(0, 0, -1)) };
      nodeA.Children.Add(nodeB);
      nodeA.Children.Add(nodeC);
      nodeB.Children.Add(nodeD);

      Assert.AreEqual(new Pose(new Vector3F(1, 1, 1)), nodeA.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(1, 2, -3)), nodeB.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(1, 2, 3)), nodeC.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(0, 0, -1)), nodeD.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(1, 1, 1)), nodeA.PoseWorld);
      Assert.AreEqual(new Pose(new Vector3F(2, 3, -2)), nodeB.PoseWorld);
      Assert.AreEqual(new Pose(new Vector3F(2, 3, 4)), nodeC.PoseWorld);
      Assert.AreEqual(new Pose(new Vector3F(2, 3, -3)), nodeD.PoseWorld);

      // Update local pose
      nodeA.PoseLocal = new Pose(new Vector3F(2, 0, 0));
      Assert.AreEqual(new Pose(new Vector3F(2, 0, 0)), nodeA.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(1, 2, -3)), nodeB.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(1, 2, 3)), nodeC.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(0, 0, -1)), nodeD.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(2, 0, 0)), nodeA.PoseWorld);
      Assert.AreEqual(new Pose(new Vector3F(3, 2, -3)), nodeB.PoseWorld);
      Assert.AreEqual(new Pose(new Vector3F(3, 2, 3)), nodeC.PoseWorld);
      Assert.AreEqual(new Pose(new Vector3F(3, 2, -4)), nodeD.PoseWorld);

      // Update world pose
      nodeB.PoseWorld = new Pose(new Vector3F(9, 9, 9));
      Assert.AreEqual(new Pose(new Vector3F(2, 0, 0)), nodeA.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(7, 9, 9)), nodeB.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(1, 2, 3)), nodeC.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(0, 0, -1)), nodeD.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(2, 0, 0)), nodeA.PoseWorld);
      Assert.AreEqual(new Pose(new Vector3F(9, 9, 9)), nodeB.PoseWorld);
      Assert.AreEqual(new Pose(new Vector3F(3, 2, 3)), nodeC.PoseWorld);
      Assert.AreEqual(new Pose(new Vector3F(9, 9, 8)), nodeD.PoseWorld);

      // Attach node to different node
      nodeA.Children.Remove(nodeB);
      nodeC.Children.Add(nodeB);
      Assert.AreEqual(new Pose(new Vector3F(2, 0, 0)), nodeA.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(1, 2, 3)), nodeC.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(7, 9, 9)), nodeB.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(0, 0, -1)), nodeD.PoseLocal);
      Assert.AreEqual(new Pose(new Vector3F(2, 0, 0)), nodeA.PoseWorld);
      Assert.AreEqual(new Pose(new Vector3F(3, 2, 3)), nodeC.PoseWorld);
      Assert.AreEqual(new Pose(new Vector3F(10, 11, 12)), nodeB.PoseWorld);
      Assert.AreEqual(new Pose(new Vector3F(10, 11, 11)), nodeD.PoseWorld);
    }
  }
}
