using System;
using System.Linq;
using DigitalRune.Game.UI.Controls;
using NUnit.Framework;


namespace DigitalRune.Game.UI.Tests
{
  [TestFixture]
  public class UIHelperTest
  {
    [Test]
    public void GetRoot()
    {
      var controlA = new UIControl();
      var controlB = new UIControl();
      var controlC = new UIControl();
      var controlD = new UIControl();
      var controlE = new UIControl();
      var controlF = new UIControl();
      var controlG = new UIControl();
      controlA.VisualChildren.Add(controlB);
      controlA.VisualChildren.Add(controlC);
      controlA.VisualChildren.Add(controlD);
      controlD.VisualChildren.Add(controlE);
      controlD.VisualChildren.Add(controlF);
      controlE.VisualChildren.Add(controlG);

      Assert.That(() => UIHelper.GetRoot(null), Throws.TypeOf<ArgumentNullException>());

      Assert.AreEqual(controlA, controlA.GetRoot());
      Assert.AreEqual(controlA, controlG.GetRoot());
    }


    [Test]
    public void GetAncestors()
    {
      var controlA = new UIControl();
      var controlB = new UIControl();
      var controlC = new UIControl();
      var controlD = new UIControl();
      var controlE = new UIControl();
      var controlF = new UIControl();
      var controlG = new UIControl();
      controlA.VisualChildren.Add(controlB);
      controlA.VisualChildren.Add(controlC);
      controlA.VisualChildren.Add(controlD);
      controlD.VisualChildren.Add(controlE);
      controlD.VisualChildren.Add(controlF);
      controlE.VisualChildren.Add(controlG);

      Assert.That(() => UIHelper.GetAncestors(null), Throws.TypeOf<ArgumentNullException>());

      var ancestors = controlE.GetAncestors().ToArray();
      Assert.AreEqual(2, ancestors.Length);
      Assert.AreEqual(controlD, ancestors[0]);
      Assert.AreEqual(controlA, ancestors[1]);
    }


    [Test]
    public void GetSelfAndAncestors()
    {
      var controlA = new UIControl();
      var controlB = new UIControl();
      var controlC = new UIControl();
      var controlD = new UIControl();
      var controlE = new UIControl();
      var controlF = new UIControl();
      var controlG = new UIControl();
      controlA.VisualChildren.Add(controlB);
      controlA.VisualChildren.Add(controlC);
      controlA.VisualChildren.Add(controlD);
      controlD.VisualChildren.Add(controlE);
      controlD.VisualChildren.Add(controlF);
      controlE.VisualChildren.Add(controlG);

      Assert.That(() => UIHelper.GetSelfAndAncestors(null), Throws.TypeOf<ArgumentNullException>());

      var ancestors = controlE.GetSelfAndAncestors().ToArray();
      Assert.AreEqual(3, ancestors.Length);
      Assert.AreEqual(controlE, ancestors[0]);
      Assert.AreEqual(controlD, ancestors[1]);
      Assert.AreEqual(controlA, ancestors[2]);
    }


    [Test]
    public void GetDescendantsDepthFirst()
    {
      var controlA = new UIControl();
      var controlB = new UIControl();
      var controlC = new UIControl();
      var controlD = new UIControl();
      var controlE = new UIControl();
      var controlF = new UIControl();
      var controlG = new UIControl();
      controlA.VisualChildren.Add(controlB);
      controlA.VisualChildren.Add(controlC);
      controlA.VisualChildren.Add(controlD);
      controlD.VisualChildren.Add(controlE);
      controlD.VisualChildren.Add(controlF);
      controlE.VisualChildren.Add(controlG);

      Assert.That(() => UIHelper.GetDescendants(null), Throws.TypeOf<ArgumentNullException>());

      var descendants = controlA.GetDescendants().ToArray();
      Assert.AreEqual(6, descendants.Length);
      Assert.AreEqual(controlB, descendants[0]);
      Assert.AreEqual(controlC, descendants[1]);
      Assert.AreEqual(controlD, descendants[2]);
      Assert.AreEqual(controlE, descendants[3]);
      Assert.AreEqual(controlG, descendants[4]);
      Assert.AreEqual(controlF, descendants[5]);
    }


    [Test]
    public void GetDescendantsBreadthFirst()
    {
      var controlA = new UIControl();
      var controlB = new UIControl();
      var controlC = new UIControl();
      var controlD = new UIControl();
      var controlE = new UIControl();
      var controlF = new UIControl();
      var controlG = new UIControl();
      controlA.VisualChildren.Add(controlB);
      controlA.VisualChildren.Add(controlC);
      controlA.VisualChildren.Add(controlD);
      controlD.VisualChildren.Add(controlE);
      controlD.VisualChildren.Add(controlF);
      controlE.VisualChildren.Add(controlG);

      Assert.That(() => UIHelper.GetDescendants(null, false), Throws.TypeOf<ArgumentNullException>());

      var descendants = controlA.GetDescendants(false).ToArray();
      Assert.AreEqual(6, descendants.Length);
      Assert.AreEqual(controlB, descendants[0]);
      Assert.AreEqual(controlC, descendants[1]);
      Assert.AreEqual(controlD, descendants[2]);
      Assert.AreEqual(controlE, descendants[3]);
      Assert.AreEqual(controlF, descendants[4]);
      Assert.AreEqual(controlG, descendants[5]);
    }


    [Test]
    public void GetSubtreeDepthFirst()
    {
      var controlA = new UIControl();
      var controlB = new UIControl();
      var controlC = new UIControl();
      var controlD = new UIControl();
      var controlE = new UIControl();
      var controlF = new UIControl();
      var controlG = new UIControl();
      controlA.VisualChildren.Add(controlB);
      controlA.VisualChildren.Add(controlC);
      controlA.VisualChildren.Add(controlD);
      controlD.VisualChildren.Add(controlE);
      controlD.VisualChildren.Add(controlF);
      controlE.VisualChildren.Add(controlG);

      Assert.That(() => UIHelper.GetSubtree(null), Throws.TypeOf<ArgumentNullException>());

      var descendants = controlD.GetSubtree().ToArray();
      Assert.AreEqual(4, descendants.Length);
      Assert.AreEqual(controlD, descendants[0]);
      Assert.AreEqual(controlE, descendants[1]);
      Assert.AreEqual(controlG, descendants[2]);
      Assert.AreEqual(controlF, descendants[3]);
    }


    [Test]
    public void GetSubtreeBreadthFirst()
    {
      var controlA = new UIControl();
      var controlB = new UIControl();
      var controlC = new UIControl();
      var controlD = new UIControl();
      var controlE = new UIControl();
      var controlF = new UIControl();
      var controlG = new UIControl();
      controlA.VisualChildren.Add(controlB);
      controlA.VisualChildren.Add(controlC);
      controlA.VisualChildren.Add(controlD);
      controlD.VisualChildren.Add(controlE);
      controlD.VisualChildren.Add(controlF);
      controlE.VisualChildren.Add(controlG);

      Assert.That(() => UIHelper.GetSubtree(null, false), Throws.TypeOf<ArgumentNullException>());

      var descendants = controlD.GetSubtree(false).ToArray();
      Assert.AreEqual(4, descendants.Length);
      Assert.AreEqual(controlD, descendants[0]);
      Assert.AreEqual(controlE, descendants[1]);
      Assert.AreEqual(controlF, descendants[2]);
      Assert.AreEqual(controlG, descendants[3]);
    }


    [Test]
    public void GetLeavesTest()
    {
      var controlA = new UIControl();
      var controlB = new UIControl();
      var controlC = new UIControl();
      var controlD = new UIControl();
      var controlE = new UIControl();
      var controlF = new UIControl();
      var controlG = new UIControl();
      controlA.VisualChildren.Add(controlB);
      controlA.VisualChildren.Add(controlC);
      controlA.VisualChildren.Add(controlD);
      controlD.VisualChildren.Add(controlE);
      controlD.VisualChildren.Add(controlF);
      controlE.VisualChildren.Add(controlG);

      Assert.That(() => UIHelper.GetLeaves(null), Throws.TypeOf<ArgumentNullException>());

      var descendants = controlA.GetLeaves().ToArray();
      Assert.AreEqual(4, descendants.Length);
      Assert.AreEqual(controlB, descendants[0]);
      Assert.AreEqual(controlC, descendants[1]);
      Assert.AreEqual(controlG, descendants[2]);
      Assert.AreEqual(controlF, descendants[3]);
    }
  }
}
