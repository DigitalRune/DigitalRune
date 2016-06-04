using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Animation.Character;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class AnimationHelperTest
  {
    [Test]
    public void GetRoot()
    {
      var animationA = new TimelineGroup();
      var animationB = new SingleFromToByAnimation();
      var animationC = new SingleFromToByAnimation();
      var animationD = new TimelineGroup();
      var animationE = new TimelineGroup();
      var animationF = new SingleFromToByAnimation();
      var animationG = new TimelineGroup();
      animationA.Add(animationB);
      animationA.Add(animationC);
      animationA.Add(animationD);
      animationD.Add(animationE);
      animationD.Add(animationF);
      animationE.Add(animationG);

      Assert.That(() => AnimationHelper.GetRoot(null), Throws.TypeOf<ArgumentNullException>());

      var animationInstance = animationA.CreateInstance();
      Assert.AreEqual(animationInstance, animationInstance.GetRoot());
      Assert.AreEqual(animationInstance, animationInstance.Children[2].Children[0].GetRoot());
    }


    [Test]
    public void GetAncestors()
    {
      var animationA = new TimelineGroup();
      var animationB = new SingleFromToByAnimation();
      var animationC = new SingleFromToByAnimation();
      var animationD = new TimelineGroup();
      var animationE = new TimelineGroup();
      var animationF = new SingleFromToByAnimation();
      var animationG = new TimelineGroup();
      animationA.Add(animationB);
      animationA.Add(animationC);
      animationA.Add(animationD);
      animationD.Add(animationE);
      animationD.Add(animationF);
      animationE.Add(animationG);

      Assert.That(() => AnimationHelper.GetAncestors(null), Throws.TypeOf<ArgumentNullException>());

      var animationInstance = animationA.CreateInstance();
      var ancestors = animationInstance.Children[2].Children[0].GetAncestors().ToArray();
      Assert.AreEqual(2, ancestors.Length);
      Assert.AreEqual(animationInstance.Children[2], ancestors[0]);
      Assert.AreEqual(animationInstance, ancestors[1]);
    }


    [Test]
    public void GetSelfAndAncestors()
    {
      var animationA = new TimelineGroup();
      var animationB = new SingleFromToByAnimation();
      var animationC = new SingleFromToByAnimation();
      var animationD = new TimelineGroup();
      var animationE = new TimelineGroup();
      var animationF = new SingleFromToByAnimation();
      var animationG = new TimelineGroup();
      animationA.Add(animationB);
      animationA.Add(animationC);
      animationA.Add(animationD);
      animationD.Add(animationE);
      animationD.Add(animationF);
      animationE.Add(animationG);

      Assert.That(() => AnimationHelper.GetSelfAndAncestors(null), Throws.TypeOf<ArgumentNullException>());

      var animationInstance = animationA.CreateInstance();
      var ancestors = animationInstance.Children[2].Children[0].GetSelfAndAncestors().ToArray();
      Assert.AreEqual(3, ancestors.Length);
      Assert.AreEqual(animationInstance.Children[2].Children[0], ancestors[0]);
      Assert.AreEqual(animationInstance.Children[2], ancestors[1]);
      Assert.AreEqual(animationInstance, ancestors[2]);
    }


    [Test]
    public void GetDescendantsDepthFirst()
    {
      var animationA = new TimelineGroup();
      var animationB = new SingleFromToByAnimation();
      var animationC = new SingleFromToByAnimation();
      var animationD = new TimelineGroup();
      var animationE = new TimelineGroup();
      var animationF = new SingleFromToByAnimation();
      var animationG = new TimelineGroup();
      animationA.Add(animationB);
      animationA.Add(animationC);
      animationA.Add(animationD);
      animationD.Add(animationE);
      animationD.Add(animationF);
      animationE.Add(animationG);

      Assert.That(() => AnimationHelper.GetDescendants(null), Throws.TypeOf<ArgumentNullException>());

      var animationInstance = animationA.CreateInstance();
      var descendants = animationInstance.GetDescendants().ToArray();
      Assert.AreEqual(6, descendants.Length);
      Assert.AreEqual(animationInstance.Children[0], descendants[0]);
      Assert.AreEqual(animationInstance.Children[1], descendants[1]);
      Assert.AreEqual(animationInstance.Children[2], descendants[2]);
      Assert.AreEqual(animationInstance.Children[2].Children[0], descendants[3]);
      Assert.AreEqual(animationInstance.Children[2].Children[0].Children[0], descendants[4]);
      Assert.AreEqual(animationInstance.Children[2].Children[1], descendants[5]);
    }


    [Test]
    public void GetDescendantsBreadthFirst()
    {
      var animationA = new TimelineGroup();
      var animationB = new SingleFromToByAnimation();
      var animationC = new SingleFromToByAnimation();
      var animationD = new TimelineGroup();
      var animationE = new TimelineGroup();
      var animationF = new SingleFromToByAnimation();
      var animationG = new TimelineGroup();
      animationA.Add(animationB);
      animationA.Add(animationC);
      animationA.Add(animationD);
      animationD.Add(animationE);
      animationD.Add(animationF);
      animationE.Add(animationG);

      Assert.That(() => AnimationHelper.GetDescendants(null, false), Throws.TypeOf<ArgumentNullException>());

      var animationInstance = animationA.CreateInstance();
      var descendants = animationInstance.GetDescendants(false).ToArray();
      Assert.AreEqual(6, descendants.Length);
      Assert.AreEqual(animationInstance.Children[0], descendants[0]);
      Assert.AreEqual(animationInstance.Children[1], descendants[1]);
      Assert.AreEqual(animationInstance.Children[2], descendants[2]);
      Assert.AreEqual(animationInstance.Children[2].Children[0], descendants[3]);
      Assert.AreEqual(animationInstance.Children[2].Children[1], descendants[4]);
      Assert.AreEqual(animationInstance.Children[2].Children[0].Children[0], descendants[5]);
    }


    [Test]
    public void GetSubtreeDepthFirst()
    {
      var animationA = new TimelineGroup();
      var animationB = new SingleFromToByAnimation();
      var animationC = new SingleFromToByAnimation();
      var animationD = new TimelineGroup();
      var animationE = new TimelineGroup();
      var animationF = new SingleFromToByAnimation();
      var animationG = new TimelineGroup();
      animationA.Add(animationB);
      animationA.Add(animationC);
      animationA.Add(animationD);
      animationD.Add(animationE);
      animationD.Add(animationF);
      animationE.Add(animationG);

      Assert.That(() => AnimationHelper.GetSubtree(null), Throws.TypeOf<ArgumentNullException>());

      var animationInstance = animationA.CreateInstance();
      var descendants = animationInstance.Children[2].GetSubtree().ToArray();
      Assert.AreEqual(4, descendants.Length);
      Assert.AreEqual(animationInstance.Children[2], descendants[0]);
      Assert.AreEqual(animationInstance.Children[2].Children[0], descendants[1]);
      Assert.AreEqual(animationInstance.Children[2].Children[0].Children[0], descendants[2]);
      Assert.AreEqual(animationInstance.Children[2].Children[1], descendants[3]);
    }


    [Test]
    public void GetSubtreeBreadthFirst()
    {
      var animationA = new TimelineGroup();
      var animationB = new SingleFromToByAnimation();
      var animationC = new SingleFromToByAnimation();
      var animationD = new TimelineGroup();
      var animationE = new TimelineGroup();
      var animationF = new SingleFromToByAnimation();
      var animationG = new TimelineGroup();
      animationA.Add(animationB);
      animationA.Add(animationC);
      animationA.Add(animationD);
      animationD.Add(animationE);
      animationD.Add(animationF);
      animationE.Add(animationG);

      Assert.That(() => AnimationHelper.GetSubtree(null, false), Throws.TypeOf<ArgumentNullException>());

      var animationInstance = animationA.CreateInstance();
      var descendants = animationInstance.Children[2].GetSubtree(false).ToArray();
      Assert.AreEqual(4, descendants.Length);
      Assert.AreEqual(animationInstance.Children[2], descendants[0]);
      Assert.AreEqual(animationInstance.Children[2].Children[0], descendants[1]);
      Assert.AreEqual(animationInstance.Children[2].Children[1], descendants[2]);
      Assert.AreEqual(animationInstance.Children[2].Children[0].Children[0], descendants[3]);
    }


    [Test]
    public void GetLeavesTest()
    {
      var animationA = new TimelineGroup();
      var animationB = new SingleFromToByAnimation();
      var animationC = new SingleFromToByAnimation();
      var animationD = new TimelineGroup();
      var animationE = new TimelineGroup();
      var animationF = new SingleFromToByAnimation();
      var animationG = new TimelineGroup();
      animationA.Add(animationB);
      animationA.Add(animationC);
      animationA.Add(animationD);
      animationD.Add(animationE);
      animationD.Add(animationF);
      animationE.Add(animationG);

      Assert.That(() => AnimationHelper.GetLeaves(null), Throws.TypeOf<ArgumentNullException>());

      var animationInstance = animationA.CreateInstance();
      var descendants = animationInstance.GetLeaves().ToArray();
      Assert.AreEqual(4, descendants.Length);
      Assert.AreEqual(animationInstance.Children[0], descendants[0]);
      Assert.AreEqual(animationInstance.Children[1], descendants[1]);
      Assert.AreEqual(animationInstance.Children[2].Children[0].Children[0], descendants[2]);
      Assert.AreEqual(animationInstance.Children[2].Children[1], descendants[3]);
    }


    [Test]
    public void ComputeLinearVelocity()
    {
      var v = new Vector3F(1, 2, 3);
      var p = new Vector3F(-7, 8, 4);
      var dt = 1 / 60f;

      var targetPosition = p + v * dt;

      Assert.IsTrue(Vector3F.AreNumericallyEqual(v, AnimationHelper.ComputeLinearVelocity(p, targetPosition, dt)));
    }


    [Test]
    public void ComputeAngularVelocity()
    {
      var v = new Vector3F(0.1f, 0.2f, 0.3f);
      var o0 = QuaternionF.CreateRotation(new Vector3F(-7, 8, 0.3f).Normalized, 4.2f);
      var dt = 1 / 60f;

      var o1 = (QuaternionF.CreateRotation(v, v.Length * dt) * o0).Normalized;
      
      // Negate quaternion. This is still the same rotation, but now we can test if 
      // the rotation around the shortest arc is used.
      o1 = -o1;

      // We use a big epsilon. Quaternion multiplication seems to create a large error...?
      Assert.IsTrue(Vector3F.AreNumericallyEqual(v, AnimationHelper.ComputeAngularVelocity(o0, o1, dt), 0.01f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(v, AnimationHelper.ComputeAngularVelocity(o0.ToRotationMatrix33(), o1.ToRotationMatrix33(), dt), 0.01f));

      // Zero rotation.
      Assert.AreEqual(Vector3F.Zero, AnimationHelper.ComputeAngularVelocity(o0, -o0, dt));
    }


    [Test]
    public void CompressedAnimationShouldHaveSameProperties()
    {
      var srtKeyFrameAnimation = new SrtKeyFrameAnimation
      {
        FillBehavior = FillBehavior.Hold,
        IsAdditive = false,
        TargetObject = null,
        TargetProperty = null
      };

      srtKeyFrameAnimation.KeyFrames.Add(
        new KeyFrame<SrtTransform>(
          TimeSpan.FromTicks(100000),
          SrtTransform.Identity));

      var srtAnimation = AnimationHelper.Compress(srtKeyFrameAnimation, 0, 0, 0);

      Assert.IsNotNull(srtAnimation);
      Assert.AreEqual(FillBehavior.Hold, srtAnimation.FillBehavior);
      Assert.AreEqual(false, srtAnimation.IsAdditive);
      Assert.AreEqual(null, srtAnimation.TargetObject);
      Assert.AreEqual(null, srtAnimation.TargetProperty);

      srtKeyFrameAnimation.FillBehavior = FillBehavior.Stop;
      srtKeyFrameAnimation.IsAdditive = true;
      srtKeyFrameAnimation.TargetObject = "Object XYZ";
      srtKeyFrameAnimation.TargetProperty = "Property XYZ";

      srtAnimation = AnimationHelper.Compress(srtKeyFrameAnimation, 0, 0, 0);

      Assert.IsNotNull(srtAnimation);
      Assert.AreEqual(FillBehavior.Stop, srtAnimation.FillBehavior);
      Assert.AreEqual(true, srtAnimation.IsAdditive);
      Assert.AreEqual("Object XYZ", srtAnimation.TargetObject);
      Assert.AreEqual("Property XYZ", srtAnimation.TargetProperty);
    }


    [Test]
    public void CompressEmptySrtKeyFrameAnimation0()
    {
      // Animation without keyframes.
      var srtKeyFrameAnimation = new SrtKeyFrameAnimation();
      var srtAnimation = AnimationHelper.Compress(srtKeyFrameAnimation, 0, 0, 0);
      Assert.IsNull(srtAnimation);
    }


    [Test]
    public void CompressEmptySrtKeyFrameAnimation1()
    {
      // Animation with 1 keyframe, which is Identity.
      var srtKeyFrameAnimation = new SrtKeyFrameAnimation();
      var time = TimeSpan.FromTicks(100000);
      srtKeyFrameAnimation.KeyFrames.Add(new KeyFrame<SrtTransform>(time, SrtTransform.Identity));

      var srtAnimation = AnimationHelper.Compress(srtKeyFrameAnimation, 0, 0, 0);

      Assert.IsNotNull(srtAnimation);
      Assert.AreEqual(srtKeyFrameAnimation.GetTotalDuration(), srtAnimation.GetTotalDuration());

      var defaultSource = SrtTransform.Identity;
      var defaultTarget = SrtTransform.Identity;
      var result = new SrtTransform();
      srtAnimation.GetValue(time, ref defaultSource, ref defaultTarget, ref result);
      Assert.AreEqual(srtKeyFrameAnimation.KeyFrames[0].Value, result);

      // Only 1 channel is needed.
      int numberOfChannels = 0;
      if (srtAnimation.Scale != null)
        numberOfChannels++;
      if (srtAnimation.Rotation != null)
        numberOfChannels++;
      if (srtAnimation.Translation != null)
        numberOfChannels++;

      Assert.AreEqual(1, numberOfChannels);
    }


    [Test]
    public void CompressEmptySrtKeyFrameAnimation2()
    {
      var random = new Random(12345);

      // Animation with 1 keyframe, which is not Identity.
      var srtKeyFrameAnimation = new SrtKeyFrameAnimation();
      var time = TimeSpan.FromTicks(100000);
      var value = new SrtTransform(random.NextVector3F(-2, 2), random.NextQuaternionF(), random.NextVector3F(-10, 10));
      srtKeyFrameAnimation.KeyFrames.Add(new KeyFrame<SrtTransform>(time, value));

      var srtAnimation = AnimationHelper.Compress(srtKeyFrameAnimation, 2, 360, 10);

      Assert.IsNotNull(srtAnimation);
      Assert.AreEqual(srtKeyFrameAnimation.GetTotalDuration(), srtAnimation.GetTotalDuration());

      var defaultSource = SrtTransform.Identity;
      var defaultTarget = SrtTransform.Identity;
      var result = new SrtTransform();
      srtAnimation.GetValue(time, ref defaultSource, ref defaultTarget, ref result);
      Assert.AreEqual(srtKeyFrameAnimation.KeyFrames[0].Value, result);
    }


    [Test]
    public void CompressEmptySrtKeyFrameAnimation3()
    {
      // Animation with 2 keyframes, which are Identity.
      var srtKeyFrameAnimation = new SrtKeyFrameAnimation();
      var time0 = TimeSpan.FromTicks(100000);
      var time1 = TimeSpan.FromTicks(200000);
      srtKeyFrameAnimation.KeyFrames.Add(new KeyFrame<SrtTransform>(time0, SrtTransform.Identity));
      srtKeyFrameAnimation.KeyFrames.Add(new KeyFrame<SrtTransform>(time1, SrtTransform.Identity));

      var srtAnimation = AnimationHelper.Compress(srtKeyFrameAnimation, 1, 90, 1);

      Assert.IsNotNull(srtAnimation);
      Assert.AreEqual(srtKeyFrameAnimation.GetTotalDuration(), srtAnimation.GetTotalDuration());

      var defaultSource = SrtTransform.Identity;
      var defaultTarget = SrtTransform.Identity;
      var result = new SrtTransform();
      srtAnimation.GetValue(time0, ref defaultSource, ref defaultTarget, ref result);
      Assert.AreEqual(srtKeyFrameAnimation.KeyFrames[0].Value, result);

      srtAnimation.GetValue(time1, ref defaultSource, ref defaultTarget, ref result);
      Assert.AreEqual(srtKeyFrameAnimation.KeyFrames[1].Value, result);

      // Only 1 channel is needed.
      int numberOfChannels = 0;
      if (srtAnimation.Scale != null)
        numberOfChannels++;
      if (srtAnimation.Rotation != null)
        numberOfChannels++;
      if (srtAnimation.Translation != null)
        numberOfChannels++;

      Assert.AreEqual(1, numberOfChannels);
    }


    [Test]
    public void CompressEmptySrtKeyFrameAnimation4()
    {
      var random = new Random(12345);

      // Animation with 2 keyframe, which are not Identity.
      var srtKeyFrameAnimation = new SrtKeyFrameAnimation();
      var time0 = TimeSpan.FromTicks(100000);
      var value0 = new SrtTransform(random.NextVector3F(-2, 2), random.NextQuaternionF(), random.NextVector3F(-10, 10));

      var time1 = TimeSpan.FromTicks(200000);
      var value1 = new SrtTransform(random.NextVector3F(-2, 2), random.NextQuaternionF(), random.NextVector3F(-10, 10));

      srtKeyFrameAnimation.KeyFrames.Add(new KeyFrame<SrtTransform>(time0, value0));
      srtKeyFrameAnimation.KeyFrames.Add(new KeyFrame<SrtTransform>(time1, value1));

      var srtAnimation = AnimationHelper.Compress(srtKeyFrameAnimation, 2, 360, 10);

      Assert.IsNotNull(srtAnimation);
      Assert.AreEqual(srtKeyFrameAnimation.GetTotalDuration(), srtAnimation.GetTotalDuration());

      var defaultSource = SrtTransform.Identity;
      var defaultTarget = SrtTransform.Identity;
      var result = new SrtTransform();
      srtAnimation.GetValue(time0, ref defaultSource, ref defaultTarget, ref result);
      Assert.AreEqual(srtKeyFrameAnimation.KeyFrames[0].Value, result);

      srtAnimation.GetValue(time1, ref defaultSource, ref defaultTarget, ref result);
      Assert.AreEqual(srtKeyFrameAnimation.KeyFrames[1].Value, result);
    }


    [Test]
    public void CompressSrtKeyFrameAnimation()
    {
      var random = new Random(12345);

      float scaleThreshold = 0.1f;
      float rotationThreshold = 2;  // [°]
      float translationThreshold = 0.2f;

      var srtKeyFrameAnimation = new SrtKeyFrameAnimation();

      // Define a view important keyframes.
      var time0 = TimeSpan.FromTicks(100000);
      var value0 = new SrtTransform(Vector3F.One, QuaternionF.Identity, Vector3F.Zero);

      var time1 = TimeSpan.FromTicks(200000);
      var value1 = new SrtTransform(new Vector3F(2, 2, 2), QuaternionF.CreateRotationX(MathHelper.ToRadians(10)), new Vector3F(1, 1, 1));

      var time2 = TimeSpan.FromTicks(400000);
      var value2 = new SrtTransform(new Vector3F(-1, -1, -1), QuaternionF.CreateRotationX(MathHelper.ToRadians(80)), new Vector3F(10, 10, 10));

      var time3 = TimeSpan.FromTicks(500000);
      var value3 = new SrtTransform(new Vector3F(3, 3, 3), QuaternionF.CreateRotationX(MathHelper.ToRadians(-10)), new Vector3F(-2, -2, -2));

      srtKeyFrameAnimation.KeyFrames.Add(new KeyFrame<SrtTransform>(time0, value0));
      srtKeyFrameAnimation.KeyFrames.Add(new KeyFrame<SrtTransform>(time1, value1));
      srtKeyFrameAnimation.KeyFrames.Add(new KeyFrame<SrtTransform>(time2, value2));
      srtKeyFrameAnimation.KeyFrames.Add(new KeyFrame<SrtTransform>(time3, value3));

      // Add random keyframes within tolerance.
      InsertRandomKeyFrames(random, srtKeyFrameAnimation, time0, time1, scaleThreshold, rotationThreshold, translationThreshold);
      InsertRandomKeyFrames(random, srtKeyFrameAnimation, time1, time2, scaleThreshold, rotationThreshold, translationThreshold);
      InsertRandomKeyFrames(random, srtKeyFrameAnimation, time2, time3, scaleThreshold, rotationThreshold, translationThreshold);

      // ---- Compress animation with tolerance.
      var srtAnimation = AnimationHelper.Compress(srtKeyFrameAnimation, scaleThreshold, rotationThreshold, translationThreshold);

      Assert.IsNotNull(srtAnimation);
      Assert.AreEqual(srtKeyFrameAnimation.GetTotalDuration(), srtAnimation.GetTotalDuration());
      Assert.IsNotNull(srtAnimation.Scale);

      Assert.AreEqual(4, ((KeyFrameAnimation<Vector3F>)srtAnimation.Scale).KeyFrames.Count);
      Assert.AreEqual(4, ((KeyFrameAnimation<QuaternionF>)srtAnimation.Rotation).KeyFrames.Count);
      Assert.AreEqual(4, ((KeyFrameAnimation<Vector3F>)srtAnimation.Translation).KeyFrames.Count);

      var defaultSource = SrtTransform.Identity;
      var defaultTarget = SrtTransform.Identity;
      var result = new SrtTransform();
      srtAnimation.GetValue(time0, ref defaultSource, ref defaultTarget, ref result);
      Assert.AreEqual(value0, result);

      srtAnimation.GetValue(time1, ref defaultSource, ref defaultTarget, ref result);
      Assert.AreEqual(value1, result);

      srtAnimation.GetValue(time2, ref defaultSource, ref defaultTarget, ref result);
      Assert.AreEqual(value2, result);

      srtAnimation.GetValue(time3, ref defaultSource, ref defaultTarget, ref result);
      Assert.AreEqual(value3, result);

      // Take a view samples.
      const int numberOfSamples = 10;
      long tickIncrement = (time3 - time0).Ticks / (numberOfSamples + 1);
      for (int i = 0; i < numberOfSamples; i++)
      {
        var time = TimeSpan.FromTicks(time0.Ticks + (i + 1) * tickIncrement);

        var valueRef = new SrtTransform();
        srtKeyFrameAnimation.GetValue(time, ref defaultSource, ref defaultTarget, ref valueRef);

        var valueNew = new SrtTransform();
        srtAnimation.GetValue(time, ref defaultSource, ref defaultTarget, ref valueNew);

        Assert.IsTrue((valueRef.Scale - valueNew.Scale).Length <= scaleThreshold);
        Assert.IsTrue(QuaternionF.GetAngle(valueRef.Rotation, valueNew.Rotation) <= MathHelper.ToRadians(rotationThreshold));
        Assert.IsTrue((valueRef.Translation - valueNew.Translation).Length <= translationThreshold);
      }

      // ----- Compress animation with zero tolerance. 
      srtAnimation = AnimationHelper.Compress(srtKeyFrameAnimation, 0, 0, 0);

      Assert.IsNotNull(srtAnimation);
      Assert.AreEqual(srtKeyFrameAnimation.GetTotalDuration(), srtAnimation.GetTotalDuration());
      Assert.IsNotNull(srtAnimation.Scale);

      Assert.AreEqual(srtKeyFrameAnimation.KeyFrames.Count, ((KeyFrameAnimation<Vector3F>)srtAnimation.Scale).KeyFrames.Count);
      Assert.AreEqual(srtKeyFrameAnimation.KeyFrames.Count, ((KeyFrameAnimation<QuaternionF>)srtAnimation.Rotation).KeyFrames.Count);
      Assert.AreEqual(srtKeyFrameAnimation.KeyFrames.Count, ((KeyFrameAnimation<Vector3F>)srtAnimation.Translation).KeyFrames.Count);

      // Take a view samples.
      for (int i = 0; i < numberOfSamples; i++)
      {
        var time = TimeSpan.FromTicks(time0.Ticks + (i + 1) * tickIncrement);

        var valueRef = new SrtTransform();
        srtKeyFrameAnimation.GetValue(time, ref defaultSource, ref defaultTarget, ref valueRef);

        var valueNew = new SrtTransform();
        srtAnimation.GetValue(time, ref defaultSource, ref defaultTarget, ref valueNew);

        Assert.IsTrue(SrtTransform.AreNumericallyEqual(valueRef, valueNew));
      }
    }


    private static void InsertRandomKeyFrames(Random random, SrtKeyFrameAnimation animation, TimeSpan time0, TimeSpan time1,
                                              float scaleThreshold, float rotationThreshold, float translationThreshold)
    {
      rotationThreshold = MathHelper.ToRadians(rotationThreshold);
      var defaultSource = SrtTransform.Identity;
      var defaultTarget = SrtTransform.Identity;
      var value = new SrtTransform();

      int insertionIndex = 0;
      for (int i = 0; i < animation.KeyFrames.Count; i++)
      {
        if (animation.KeyFrames[i].Time == time0)
        {
          insertionIndex = i + 1;
          break;
        }
      }

      Debug.Assert(insertionIndex > 0);

      const int numberOfKeyFrames = 2;
      long tickIncrement = (time1 - time0).Ticks / (numberOfKeyFrames + 1);
      for (int i = 0; i < numberOfKeyFrames; i++)
      {
        var time = TimeSpan.FromTicks(time0.Ticks + (i + 1) * tickIncrement);
        Debug.Assert(time0 < time && time < time1);

        // Get interpolated animation value.
        animation.GetValue(time, ref defaultSource, ref defaultTarget, ref value);

        // Apply small variation (within thresholds).
        value.Scale += random.NextVector3F(-1, 1).Normalized * (scaleThreshold / 2);
        value.Rotation = QuaternionF.CreateRotation(random.NextVector3F(-1, 1), rotationThreshold / 2) * value.Rotation;
        value.Translation += random.NextVector3F(-1, 1).Normalized * (translationThreshold / 2);

        animation.KeyFrames.Insert(insertionIndex, new KeyFrame<SrtTransform>(time, value));
        insertionIndex++;
      }
    }
  }
}
