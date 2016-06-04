using System;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class AnimationInstanceCollectionTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowWhenOwnerIsNull()
    {
      var collection = new AnimationInstanceCollection(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ShouldThrowWhenOwnerHasChildren()
    {
      var timelineGroup = new TimelineGroup();
      var animationInstance = timelineGroup.CreateInstance();
      var collection = new AnimationInstanceCollection(animationInstance);
    }
  }
}
