using System;
using NUnit.Framework;


namespace DigitalRune.Game.States.Tests
{
  [TestFixture]
  public class StateCollectionTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void NullNotAllowed()
    {
      var sc = new StateCollection();
      sc.Add(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void DuplicatesNotAllowed()
    {
      var sc = new StateCollection();
      var s = new State();
      sc.Add(s);
      sc.Add(s);
    }
  }
}
