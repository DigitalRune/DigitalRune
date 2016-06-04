using System;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
  [TestFixture]
  public class ChildCollectionTest
  {
    class Node
    {
      public Node Parent;
      public ChildCollection<Node, Node> Children;
      public Node()
      {
        Children = new MyChildCollection(this);
      }
    }

    class MyChildCollection : ChildCollection<Node, Node>
    {
      public new Node Parent
      {
        get { return base.Parent; }
        set { base.Parent = value; }
      }

      public MyChildCollection(Node parent) : base(parent)
      {
      }

      protected override Node GetParent(Node child)
      {
        return child.Parent;
      }

      protected override void SetParent(Node child, Node parent)
      {
        child.Parent = parent;
      }
    }


    [Test]
    public void Test1()
    {
      var a = new Node();
      var b = new Node();
      var c = new Node();

      a.Children.Add(b);
      Assert.AreEqual(a, b.Parent);
      b.Children.Add(c);
      Assert.AreEqual(b, c.Parent);

      a.Children.Remove(b);
      Assert.AreEqual(null, b.Parent);
    }


    [Test]
    public void Parent()
    {
      var a = new Node();
      var b = new Node();
      var c = new Node();

      var collection = new MyChildCollection(null);
      collection.Add(b);
      collection.Add(c);

      Assert.AreEqual(null, b.Parent);
      Assert.AreEqual(null, c.Parent);

      collection.Parent = a;

      Assert.AreEqual(a, b.Parent);
      Assert.AreEqual(a, c.Parent);

      collection.Parent = null;

      Assert.AreEqual(null, b.Parent);
      Assert.AreEqual(null, c.Parent);
    }


    [Test]
    public void ClearItems()
    {
      var a = new Node();
      var b = new Node();
      var c = new Node();

      a.Children.Add(b);
      a.Children.Add(c);

      Assert.AreEqual(2, a.Children.Count);
      Assert.AreEqual(a, b.Parent);
      Assert.AreEqual(a, c.Parent);

      a.Children.Clear();

      Assert.AreEqual(0, a.Children.Count);
      Assert.AreEqual(null, b.Parent);
      Assert.AreEqual(null, c.Parent);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void InsertItemsShouldThrowArgumentNullException()
    {
      var a = new Node();
      
      a.Children.Add(null);
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void InsertItemsShouldThrowInvalidOperationException1()
    {
      var a = new Node();
      var b = new Node();
      var c = new Node() { Parent = b };

      a.Children.Add(c);
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void InsertItemsShouldThrowInvalidOperationException2()
    {
      var a = new Node();
      var c = new Node() { Parent = a };

      a.Children.Add(c);
    }


    [Test]
    public void RemoveItem()
    {
      var a = new Node();
      var b = new Node();
      var c = new Node();

      a.Children.Add(b);
      a.Children.Add(c);

      Assert.AreEqual(2, a.Children.Count);
      Assert.AreEqual(a, b.Parent);
      Assert.AreEqual(a, c.Parent);

      a.Children.RemoveAt(0);

      Assert.AreEqual(1, a.Children.Count);
      Assert.AreEqual(null, b.Parent);
      Assert.AreEqual(a, c.Parent);
    }


    [Test]
    public void SetItem()
    {
      var a = new Node();
      var b = new Node();
      var c = new Node();

      a.Children.Add(b);

      Assert.AreEqual(1, a.Children.Count);
      Assert.AreEqual(a, b.Parent);
      Assert.AreEqual(null, c.Parent);

      a.Children[0] = c;

      Assert.AreEqual(1, a.Children.Count);
      Assert.AreEqual(null, b.Parent);
      Assert.AreEqual(a, c.Parent);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetItemShouldThrowArgumentNullException()
    {
      var a = new Node();
      a.Children.Add(new Node());

      a.Children[0] = null;
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void SetItemShouldThrowInvalidOperationException1()
    {
      var a = new Node();
      a.Children.Add(new Node());
      var b = new Node();
      var c = new Node() { Parent = b };

      a.Children[0] = c;
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void SetItemShouldThrowInvalidOperationException2()
    {
      var a = new Node();
      a.Children.Add(new Node());
      var b = new Node();
      var c = new Node() { Parent = a };

      a.Children[0] = c;
    }



    [Test]
    public void GetEnumerator()
    {
      var a = new Node();
      var b = new Node();
      var c = new Node();

      a.Children.Add(b);
      a.Children.Add(c);

      int i = 0;
      foreach(var item in a.Children)
      {
        if (i == 0)
          Assert.AreEqual(b, item);
        else
          Assert.AreEqual(c, item);

        i++;
      }
    }


    // ----- For Move() testing.
    class MoveTestNode
    {
      public MoveTestNode Parent;
    }

    class MoveTestCollection : ChildCollection<MoveTestNode, MoveTestNode>
    {
      public bool IsMoving;

      public MoveTestCollection(MoveTestNode parent)
        : base(parent)
      {
      }

      protected override MoveTestNode GetParent(MoveTestNode child)
      {
        return child.Parent;
      }

      protected override void SetParent(MoveTestNode child, MoveTestNode parent)
      {
        Assert.IsFalse(IsMoving);
        child.Parent = parent;
      }
    }


    [Test]
    public void MoveTest()
    {
      var p = new MoveTestNode();
      var a = new MoveTestNode();
      var b = new MoveTestNode();
      var c = new MoveTestNode();

      var coll = new MoveTestCollection(p);
      coll.Add(a);
      coll.Add(b);
      coll.Add(c);

      Assert.AreEqual(p, a.Parent);
      Assert.AreEqual(p, b.Parent);
      Assert.AreEqual(p, c.Parent);

      Assert.Throws<ArgumentOutOfRangeException>(() => coll.Move(-1, 1));
      Assert.Throws<ArgumentOutOfRangeException>(() => coll.Move(5, 1));
      Assert.Throws<ArgumentOutOfRangeException>(() => coll.Move(1, -1));
      Assert.Throws<ArgumentOutOfRangeException>(() => coll.Move(-1, 5));

      coll.IsMoving = true;  // Set flag. SetParent must not be called in collection.
      coll.Move(1, 1);   // Does nothing.
      coll.Move(0, 1);
      coll.IsMoving = false;

      Assert.AreEqual(p, a.Parent);
      Assert.AreEqual(p, b.Parent);
      Assert.AreEqual(p, c.Parent);

      Assert.AreEqual(b, coll[0]);
      Assert.AreEqual(a, coll[1]);
      Assert.AreEqual(c, coll[2]);

      Assert.AreEqual(1, coll.IndexOf(a));
    }    
  }
}


