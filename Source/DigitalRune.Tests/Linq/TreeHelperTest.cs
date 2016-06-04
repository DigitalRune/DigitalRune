using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;


namespace DigitalRune.Linq.Tests
{
  internal class TreeNode
  {
    public TreeNode Parent;
    public List<TreeNode> Children = new List<TreeNode>();
  }


  [TestFixture]
  public class TreeHelperTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetAncestorsShouldThrowWhenReferenceIsNull()
    {
      TreeHelper.GetAncestors<TreeNode>(null, t => t.Parent);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetAncestorsShouldThrowWhenDelegateIsNull()
    {
      TreeHelper.GetAncestors(new TreeNode(), null);
    }


    [Test]
    public void GetAncestorsOfARootNode()
    {
      TreeNode root = new TreeNode();
      Assert.AreEqual(0, TreeHelper.GetAncestors(root, t => t.Parent).Count());
    }


    [Test]
    public void GetAncestors()
    {
      TreeNode root = new TreeNode();

      // Children of root
      TreeNode a = new TreeNode { Parent = root };
      TreeNode b = new TreeNode { Parent = root };
      TreeNode c = new TreeNode { Parent = root };
      root.Children.Add(a);
      root.Children.Add(b);
      root.Children.Add(c);

      // Children of a
      TreeNode d = new TreeNode { Parent = a };
      TreeNode e = new TreeNode { Parent = a };
      a.Children.Add(d);
      a.Children.Add(e);

      // b has no children

      // Children of c
      TreeNode f = new TreeNode { Parent = c };
      TreeNode g = new TreeNode { Parent = c };
      c.Children.Add(f);
      c.Children.Add(g);

      // Children of f
      TreeNode h = new TreeNode { Parent = f };
      f.Children.Add(h);

      Assert.AreEqual(0, TreeHelper.GetAncestors(root, t => t.Parent).Count());

      var ancestorsOfA = TreeHelper.GetAncestors(a, t => t.Parent).ToArray();
      Assert.AreEqual(1, ancestorsOfA.Length);
      Assert.AreSame(root, ancestorsOfA[0]);

      var ancestorsOfH = TreeHelper.GetAncestors(h, t => t.Parent).ToArray();
      Assert.AreEqual(3, ancestorsOfH.Length);
      Assert.AreSame(f, ancestorsOfH[0]);
      Assert.AreSame(c, ancestorsOfH[1]);
      Assert.AreSame(root, ancestorsOfH[2]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetSelfAndAncestorsShouldThrowWhenReferenceIsNull()
    {
      TreeHelper.GetSelfAndAncestors<TreeNode>(null, t => t.Parent);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetSelfAndAncestorsShouldThrowWhenDelegateIsNull()
    {
      TreeHelper.GetSelfAndAncestors(new TreeNode(), null);
    }


    [Test]
    public void GetSelfAndAncestors()
    {
      TreeNode root = new TreeNode();

      // Children of root
      TreeNode a = new TreeNode { Parent = root };
      TreeNode b = new TreeNode { Parent = root };
      TreeNode c = new TreeNode { Parent = root };
      root.Children.Add(a);
      root.Children.Add(b);
      root.Children.Add(c);

      // Children of a
      TreeNode d = new TreeNode { Parent = a };
      TreeNode e = new TreeNode { Parent = a };
      a.Children.Add(d);
      a.Children.Add(e);

      // b has no children

      // Children of c
      TreeNode f = new TreeNode { Parent = c };
      TreeNode g = new TreeNode { Parent = c };
      c.Children.Add(f);
      c.Children.Add(g);

      // Children of f
      TreeNode h = new TreeNode { Parent = f };
      f.Children.Add(h);

      Assert.AreEqual(1, TreeHelper.GetSelfAndAncestors(root, t => t.Parent).Count());

      var aAndAncestors = TreeHelper.GetSelfAndAncestors(a, t => t.Parent).ToArray();
      Assert.AreEqual(2, aAndAncestors.Length);
      Assert.AreSame(a, aAndAncestors[0]);
      Assert.AreSame(root, aAndAncestors[1]);

      var hAndAncestors = TreeHelper.GetSelfAndAncestors(h, t => t.Parent).ToArray();
      Assert.AreEqual(4, hAndAncestors.Length);
      Assert.AreSame(h, hAndAncestors[0]);
      Assert.AreSame(f, hAndAncestors[1]);
      Assert.AreSame(c, hAndAncestors[2]);
      Assert.AreSame(root, hAndAncestors[3]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetRootShouldThrowWhenReferenceIsNull()
    {
      TreeHelper.GetRoot<TreeNode>(null, t => t.Parent);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetRootShouldThrowWhenDelegateIsNull()
    {
      TreeHelper.GetRoot(new TreeNode(), null);
    }


    [Test]
    public void GetRootOfARootNode()
    {
      TreeNode root = new TreeNode();
      Assert.AreEqual(root, TreeHelper.GetRoot(root, t => t.Parent));
    }


    [Test]
    public void GetRoot()
    {
      TreeNode root = new TreeNode();

      // Children of root
      TreeNode a = new TreeNode { Parent = root };
      TreeNode b = new TreeNode { Parent = root };
      TreeNode c = new TreeNode { Parent = root };
      root.Children.Add(a);
      root.Children.Add(b);
      root.Children.Add(c);

      // Children of a
      TreeNode d = new TreeNode { Parent = a };
      TreeNode e = new TreeNode { Parent = a };
      a.Children.Add(d);
      a.Children.Add(e);

      // b has no children

      // Children of c
      TreeNode f = new TreeNode { Parent = c };
      TreeNode g = new TreeNode { Parent = c };
      c.Children.Add(f);
      c.Children.Add(g);

      // Children of f
      TreeNode h = new TreeNode { Parent = f };
      f.Children.Add(h);

      Assert.AreEqual(root, TreeHelper.GetRoot(root, t => t.Parent));
      Assert.AreEqual(root, TreeHelper.GetRoot(a, t => t.Parent));
      Assert.AreEqual(root, TreeHelper.GetRoot(h, t => t.Parent));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetDescendantsShouldThrowWhenReferenceIsNull()
    {
      TreeHelper.GetDescendants<TreeNode>(null, t => t.Children);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetDescendantsShouldThrowWhenDelegateIsNull()
    {
      TreeHelper.GetDescendants(new TreeNode(), null);
    }    


    [Test]
    public void GetDescendants()
    {
      TreeNode root = new TreeNode();

      // Children of root
      TreeNode a = new TreeNode { Parent = root };
      TreeNode b = new TreeNode { Parent = root };
      TreeNode c = new TreeNode { Parent = root };
      root.Children.Add(a);
      root.Children.Add(b);
      root.Children.Add(c);

      // Children of a
      TreeNode d = new TreeNode { Parent = a };
      TreeNode e = new TreeNode { Parent = a };
      a.Children.Add(d);
      a.Children.Add(e);

      // b has no children

      // Children of c
      TreeNode f = new TreeNode { Parent = c };
      TreeNode g = new TreeNode { Parent = c };
      c.Children.Add(f);
      c.Children.Add(g);

      // Children of f
      TreeNode h = new TreeNode { Parent = f };
      f.Children.Add(h);

      Assert.AreEqual(0, TreeHelper.GetDescendants(b, t => t.Children).Count());

      // Depth-first
      var descendantsOfRoot = TreeHelper.GetDescendants(root, t => t.Children).ToArray();
      Assert.AreEqual(8, descendantsOfRoot.Length);
      Assert.AreSame(a, descendantsOfRoot[0]);
      Assert.AreSame(d, descendantsOfRoot[1]);
      Assert.AreSame(e, descendantsOfRoot[2]);
      Assert.AreSame(b, descendantsOfRoot[3]);
      Assert.AreSame(c, descendantsOfRoot[4]);
      Assert.AreSame(f, descendantsOfRoot[5]);
      Assert.AreSame(h, descendantsOfRoot[6]);
      Assert.AreSame(g, descendantsOfRoot[7]);

      // Breadth-first
      descendantsOfRoot = TreeHelper.GetDescendants(root, t => t.Children, false).ToArray();
      Assert.AreEqual(8, descendantsOfRoot.Length);
      Assert.AreSame(a, descendantsOfRoot[0]);
      Assert.AreSame(b, descendantsOfRoot[1]);
      Assert.AreSame(c, descendantsOfRoot[2]);
      Assert.AreSame(d, descendantsOfRoot[3]);
      Assert.AreSame(e, descendantsOfRoot[4]);
      Assert.AreSame(f, descendantsOfRoot[5]);
      Assert.AreSame(g, descendantsOfRoot[6]);
      Assert.AreSame(h, descendantsOfRoot[7]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetSubtreeShouldThrowWhenNodeIsNull()
    {
      TreeHelper.GetSubtree<TreeNode>(null, t => t.Children);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetSubtreeShouldThrowWhenDelegateIsNull()
    {
      TreeHelper.GetSubtree(new TreeNode(), null);
    }


    [Test]
    public void GetSubtree()
    {
      TreeNode root = new TreeNode();

      // Children of root
      TreeNode a = new TreeNode { Parent = root };
      TreeNode b = new TreeNode { Parent = root };
      TreeNode c = new TreeNode { Parent = root };
      root.Children.Add(a);
      root.Children.Add(b);
      root.Children.Add(c);

      // Children of a
      TreeNode d = new TreeNode { Parent = a };
      TreeNode e = new TreeNode { Parent = a };
      a.Children.Add(d);
      a.Children.Add(e);

      // b has no children

      // Children of c
      TreeNode f = new TreeNode { Parent = c };
      TreeNode g = new TreeNode { Parent = c };
      c.Children.Add(f);
      c.Children.Add(g);

      // Children of f
      TreeNode h = new TreeNode { Parent = f };
      f.Children.Add(h);

      Assert.AreEqual(1, TreeHelper.GetSubtree(b, t => t.Children).Count());

      // Depth-first
      var subtreeOfRoot = TreeHelper.GetSubtree(root, t => t.Children).ToArray();
      Assert.AreEqual(9, subtreeOfRoot.Length);
      Assert.AreSame(root, subtreeOfRoot[0]);
      Assert.AreSame(a, subtreeOfRoot[1]);
      Assert.AreSame(d, subtreeOfRoot[2]);
      Assert.AreSame(e, subtreeOfRoot[3]);
      Assert.AreSame(b, subtreeOfRoot[4]);
      Assert.AreSame(c, subtreeOfRoot[5]);
      Assert.AreSame(f, subtreeOfRoot[6]);
      Assert.AreSame(h, subtreeOfRoot[7]);
      Assert.AreSame(g, subtreeOfRoot[8]);

      // Breadth-first
      subtreeOfRoot = TreeHelper.GetSubtree(root, t => t.Children, false).ToArray();
      Assert.AreEqual(9, subtreeOfRoot.Length);
      Assert.AreSame(root, subtreeOfRoot[0]);
      Assert.AreSame(a, subtreeOfRoot[1]);
      Assert.AreSame(b, subtreeOfRoot[2]);
      Assert.AreSame(c, subtreeOfRoot[3]);
      Assert.AreSame(d, subtreeOfRoot[4]);
      Assert.AreSame(e, subtreeOfRoot[5]);
      Assert.AreSame(f, subtreeOfRoot[6]);
      Assert.AreSame(g, subtreeOfRoot[7]);
      Assert.AreSame(h, subtreeOfRoot[8]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetLeavesShouldThrowWhenNodeIsNull()
    {
      TreeHelper.GetLeaves<TreeNode>(null, t => t.Children);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetLeavesShouldThrowWhenGetChildrenIsNull()
    {
      TreeHelper.GetLeaves(new TreeNode(), null);
    }


    [Test]
    public void GetLeaves()
    {
      TreeNode root = new TreeNode();

      // Children of root
      TreeNode a = new TreeNode { Parent = root };
      TreeNode b = new TreeNode { Parent = root };
      TreeNode c = new TreeNode { Parent = root };
      root.Children.Add(a);
      root.Children.Add(b);
      root.Children.Add(c);

      // Children of a
      TreeNode d = new TreeNode { Parent = a };
      TreeNode e = new TreeNode { Parent = a };
      a.Children.Add(d);
      a.Children.Add(e);

      // b has no children

      // Children of c
      TreeNode f = new TreeNode { Parent = c };
      TreeNode g = new TreeNode { Parent = c };
      c.Children.Add(f);
      c.Children.Add(g);

      // Children of f
      TreeNode h = new TreeNode { Parent = f };
      f.Children.Add(h);

      var leavesOfB = TreeHelper.GetLeaves(b, t => t.Children).ToArray();
      Assert.AreEqual(1, leavesOfB.Length);
      Assert.AreSame(b, leavesOfB[0]);

      var leavesOfRoot = TreeHelper.GetLeaves(root, t => t.Children).ToArray();
      Assert.AreEqual(5, leavesOfRoot.Length);
      Assert.AreSame(d, leavesOfRoot[0]);
      Assert.AreSame(e, leavesOfRoot[1]);
      Assert.AreSame(b, leavesOfRoot[2]);
      Assert.AreSame(h, leavesOfRoot[3]);
      Assert.AreSame(g, leavesOfRoot[4]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetDepthShouldThrowWhenNodeIsNull()
    {
      TreeHelper.GetDepth<TreeNode>(null, t => t.Parent);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetDepthShouldThrowWhenGetParentIsNull()
    {
      TreeHelper.GetDepth(new TreeNode(), null);
    }


    [Test]
    public void GetDepth()
    {
      TreeNode root = new TreeNode();

      // Children of root
      TreeNode a = new TreeNode { Parent = root };
      TreeNode b = new TreeNode { Parent = root };
      TreeNode c = new TreeNode { Parent = root };
      root.Children.Add(a);
      root.Children.Add(b);
      root.Children.Add(c);

      // Children of a
      TreeNode d = new TreeNode { Parent = a };
      TreeNode e = new TreeNode { Parent = a };
      a.Children.Add(d);
      a.Children.Add(e);

      // b has no children

      // Children of c
      TreeNode f = new TreeNode { Parent = c };
      TreeNode g = new TreeNode { Parent = c };
      c.Children.Add(f);
      c.Children.Add(g);

      // Children of f
      TreeNode h = new TreeNode { Parent = f };
      f.Children.Add(h);

      Assert.AreEqual(0, TreeHelper.GetDepth(root, t => t.Parent));
      Assert.AreEqual(1, TreeHelper.GetDepth(a, t => t.Parent));
      Assert.AreEqual(1, TreeHelper.GetDepth(b, t => t.Parent));
      Assert.AreEqual(1, TreeHelper.GetDepth(c, t => t.Parent));
      Assert.AreEqual(2, TreeHelper.GetDepth(d, t => t.Parent));
      Assert.AreEqual(2, TreeHelper.GetDepth(e, t => t.Parent));
      Assert.AreEqual(2, TreeHelper.GetDepth(f, t => t.Parent));
      Assert.AreEqual(2, TreeHelper.GetDepth(g, t => t.Parent));
      Assert.AreEqual(3, TreeHelper.GetDepth(h, t => t.Parent));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetHeightShouldThrowWhenNodeIsNull()
    {
      TreeHelper.GetHeight<TreeNode>(null, t => t.Children);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetHeightShouldThrowWhenGetChildrenIsNull()
    {
      TreeHelper.GetHeight(new TreeNode(), null);
    }


    [Test]
    public void GetHeight()
    {
      TreeNode root = new TreeNode();

      // Children of root
      TreeNode a = new TreeNode { Parent = root };
      TreeNode b = new TreeNode { Parent = root };
      TreeNode c = new TreeNode { Parent = root };
      root.Children.Add(a);
      root.Children.Add(b);
      root.Children.Add(c);

      // Children of a
      TreeNode d = new TreeNode { Parent = a };
      TreeNode e = new TreeNode { Parent = a };
      a.Children.Add(d);
      a.Children.Add(e);

      // b has no children

      // Children of c
      TreeNode f = new TreeNode { Parent = c };
      TreeNode g = new TreeNode { Parent = c };
      c.Children.Add(f);
      c.Children.Add(g);

      // Children of f
      TreeNode h = new TreeNode { Parent = f };
      f.Children.Add(h);

      Assert.AreEqual(3, TreeHelper.GetHeight(root, t => t.Children));
      Assert.AreEqual(1, TreeHelper.GetHeight(a, t => t.Children));
      Assert.AreEqual(0, TreeHelper.GetHeight(b, t => t.Children));
      Assert.AreEqual(2, TreeHelper.GetHeight(c, t => t.Children));
      Assert.AreEqual(0, TreeHelper.GetHeight(d, t => t.Children));
      Assert.AreEqual(0, TreeHelper.GetHeight(e, t => t.Children));
      Assert.AreEqual(1, TreeHelper.GetHeight(f, t => t.Children));
      Assert.AreEqual(0, TreeHelper.GetHeight(g, t => t.Children));
      Assert.AreEqual(0, TreeHelper.GetHeight(h, t => t.Children));
    }
  }
}
