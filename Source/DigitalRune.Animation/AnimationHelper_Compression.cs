// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;


namespace DigitalRune.Animation
{
  public static partial class AnimationHelper
  {
    /// <summary>
    /// Compresses the specified animation using simple lossy compression algorithm.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="scaleThreshold">The scale threshold.</param>
    /// <param name="rotationThreshold">The rotation threshold in degrees.</param>
    /// <param name="translationThreshold">The translation threshold.</param>
    /// <returns>
    /// The compressed animation. Or <see langword="null"/> if the animation does contain any
    /// key frames.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method takes an <see cref="SrtKeyFrameAnimation"/> and removes not needed scale,
    /// rotation or translation channels. It further removes key frames that can be interpolated
    /// from the neighbor key frames. This a lossy compression and the threshold parameters define
    /// the allowed errors. If the thresholds are 0 or negative, this compression is lossless. If
    /// the thresholds are greater than 0 (recommended), the compression is lossy. The best way to
    /// determine optimal thresholds is to compare the compressed animation with the uncompressed
    /// animation visually.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animation" /> is <see langword="null"/>.
    /// </exception>
    public static SrtAnimation Compress(SrtKeyFrameAnimation animation, float scaleThreshold, float rotationThreshold, float translationThreshold)
    {
      if (animation == null)
        throw new ArgumentNullException("animation");

      var keyFrames = animation.KeyFrames;
      if (keyFrames.Count == 0)
      {
        // Empty animation.
        return null;
      }

      var animationEx = new SrtAnimation
      {
        FillBehavior = animation.FillBehavior,
        IsAdditive = animation.IsAdditive,
        TargetObject = animation.TargetObject,
        TargetProperty = animation.TargetProperty,
      };

      Vector3FKeyFrameAnimation scaleAnimation = null;
      QuaternionFKeyFrameAnimation rotationAnimation = null;
      Vector3FKeyFrameAnimation translationAnimation = null;

      // Create Scale channel if required.
      foreach (var keyFrame in keyFrames)
      {
        if (!Vector3F.AreNumericallyEqual(keyFrame.Value.Scale, Vector3F.One))
        {
          scaleAnimation = new Vector3FKeyFrameAnimation();
          break;
        }
      }

      // Create Rotation channel if required.
      foreach (var keyFrame in keyFrames)
      {
        if (!QuaternionF.AreNumericallyEqual(keyFrame.Value.Rotation, QuaternionF.Identity))
        {
          rotationAnimation = new QuaternionFKeyFrameAnimation();
          break;
        }
      }

      // Create Translation channel if required.
      foreach (var keyFrame in keyFrames)
      {
        if (!keyFrame.Value.Translation.IsNumericallyZero)
        {
          translationAnimation = new Vector3FKeyFrameAnimation();
          break;
        }
      }

      if (scaleAnimation == null && rotationAnimation == null && translationAnimation == null)
      {
        // The animation does not contain any transformations. However, the keyframe times (start
        // and end) may be relevant.
        translationAnimation = new Vector3FKeyFrameAnimation();
      }

      if (keyFrames.Count <= 2)
      {
        // Add first keyframe.
        {
          Debug.Assert(keyFrames.Count > 0);

          var keyFrame = keyFrames[0];
          if (scaleAnimation != null)
            scaleAnimation.KeyFrames.Add(new KeyFrame<Vector3F>(keyFrame.Time, keyFrame.Value.Scale));

          if (rotationAnimation != null)
            rotationAnimation.KeyFrames.Add(new KeyFrame<QuaternionF>(keyFrame.Time, keyFrame.Value.Rotation));

          if (translationAnimation != null)
            translationAnimation.KeyFrames.Add(new KeyFrame<Vector3F>(keyFrame.Time, keyFrame.Value.Translation));
        }

        // Add second (last) keyframe.
        if (keyFrames.Count > 1)
        {
          Debug.Assert(keyFrames.Count == 2);

          var keyFrame = keyFrames[1];
          if (scaleAnimation != null)
            scaleAnimation.KeyFrames.Add(new KeyFrame<Vector3F>(keyFrame.Time, keyFrame.Value.Scale));

          if (rotationAnimation != null)
            rotationAnimation.KeyFrames.Add(new KeyFrame<QuaternionF>(keyFrame.Time, keyFrame.Value.Rotation));

          if (translationAnimation != null)
            translationAnimation.KeyFrames.Add(new KeyFrame<Vector3F>(keyFrame.Time, keyFrame.Value.Translation));
        }
      }
      else
      {
        // Animation has more than 2 keyframes.
        // --> Compress animation.
        if (scaleAnimation != null)
          Compress(animation, scaleAnimation, scaleThreshold, keyFrame => keyFrame.Value.Scale, ComputeError);

        if (rotationAnimation != null)
          Compress(animation, rotationAnimation, MathHelper.ToRadians(rotationThreshold), keyFrame => keyFrame.Value.Rotation, ComputeError);

        if (translationAnimation != null)
          Compress(animation, translationAnimation, translationThreshold, keyFrame => keyFrame.Value.Translation, ComputeError);
      }

      animationEx.Scale = scaleAnimation;
      animationEx.Rotation = rotationAnimation;
      animationEx.Translation = translationAnimation;

      return animationEx;
    }


    // Notes:
    // This animation compression algorithm is a version of the Ramer–Douglas–Peucker algorithm that
    // was developed independently by DigitalRune. As distance metric, we use the distance between
    // the keyframe value and the interpolated keyframe value at the keyframe time.
    // Another paper by Önder et al. uses the same technique. The only difference is that paper uses
    // the distance to the line as the distance metric.
    //
    // References:
    // - Ramer–Douglas–Peucker algorithm,
    //   https://en.wikipedia.org/wiki/Ramer%E2%80%93Douglas%E2%80%93Peucker_algorithm
    // - Keyframe Reduction Techniques for Motion Capture Data, 
    //   http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.331.204&rep=rep1&type=pdf


    // It is possible to use a recursive method for compressing segments. (There is
    // no danger of a stack overflow. The worst-case size of the stack is O(log n).)
    // But we use an explicit Stack<T> instead.
    // TODO: Multithreaded compression using ConcurrentStack<T>.
    private class CompressionContext<T0, T1>
    {
      public IList<IKeyFrame<T0>> UncompressedKeyFrames;   // Original, uncompressed keyframes.
      public List<int> CompressedKeyFrames;               // Indices of the keyframes to keep.
      public Stack<Pair<int>> Segments;                   // Segments that need to be processed.
      public Func<IKeyFrame<T0>, T1> GetValue;
      public Func<T1, T1, T1, float, float> ComputeError;
      public float Threshold;
    }


    private static void Compress<T0, T1>(KeyFrameAnimation<T0> sourceAnimation, KeyFrameAnimation<T1> targetAnimation, float threshold, Func<IKeyFrame<T0>, T1> getValue, Func<T1, T1, T1, float, float> computeError)
    {
      var keyFrames = sourceAnimation.KeyFrames;

      var context = new CompressionContext<T0, T1>
      {
        UncompressedKeyFrames = keyFrames,
        CompressedKeyFrames = new List<int>(),
        Segments = new Stack<Pair<int>>(),
        GetValue = getValue,
        ComputeError = computeError,
        Threshold = threshold
      };

      const int startIndex = 0;
      int endIndex = keyFrames.Count - 1;

      Debug.Assert(context.CompressedKeyFrames.Count == 0);
      context.CompressedKeyFrames.Add(startIndex);
      context.CompressedKeyFrames.Add(endIndex);

      Debug.Assert(context.Segments.Count == 0);
      context.Segments.Push(new Pair<int>(startIndex, endIndex));

      do
      {
        CompressSegment(context.Segments.Pop(), context);
      } while (context.Segments.Count > 0);

      Debug.Assert(
        context.CompressedKeyFrames.Distinct().Count() == context.CompressedKeyFrames.Count,
        "CompressedKeyFrames should not contain duplicates.");

      // Build compressed animation.
      context.CompressedKeyFrames.Sort();
      foreach (int index in context.CompressedKeyFrames)
      {
        var keyFrame = keyFrames[index];
        targetAnimation.KeyFrames.Add(new KeyFrame<T1>(keyFrame.Time, getValue(keyFrame)));
      }
    }


    private static void CompressSegment<T0, T1>(Pair<int> segment, CompressionContext<T0, T1> context)
    {
      int startIndex = segment.First;
      int endIndex = segment.Second;

      Debug.Assert(endIndex - startIndex > 1, "Empty segment.");
      Debug.Assert(context.CompressedKeyFrames.Contains(startIndex), "Start index of segment needs to be in CompressedKeyFrames.");
      Debug.Assert(context.CompressedKeyFrames.Contains(endIndex), "End index of segment needs to be in CompressedKeyFrames.");

      // Check whether all keyframes are within tolerance.
      var start = context.UncompressedKeyFrames[startIndex];
      var end = context.UncompressedKeyFrames[endIndex];
      float ticks = end.Time.Ticks - start.Time.Ticks;
      float maxError = 0;
      int maxErrorIndex = 0;
      for (int i = startIndex + 1; i < endIndex; i++)
      {
        var current = context.UncompressedKeyFrames[i];
        float parameter = (current.Time.Ticks - start.Time.Ticks) / ticks;
        float error = context.ComputeError(context.GetValue(current), context.GetValue(start), context.GetValue(end), parameter);
        if (error > maxError)
        {
          maxError = error;
          maxErrorIndex = i;
        }
      }

      if (maxError > context.Threshold)
        SplitSegment(startIndex, maxErrorIndex, endIndex, context);
    }


    private static void SplitSegment<T0, T1>(int startIndex, int splitIndex, int endIndex, CompressionContext<T0, T1> context)
    {
      Debug.Assert(startIndex < splitIndex && splitIndex < endIndex, "Keyframe indices need to be sorted.");
      Debug.Assert(context.CompressedKeyFrames.Contains(startIndex), "Start index of segment needs to be in CompressedKeyFrames.");
      Debug.Assert(context.CompressedKeyFrames.Contains(endIndex), "End index of segment needs to be in CompressedKeyFrames.");

      context.CompressedKeyFrames.Add(splitIndex);

      // Split if necessary. (The first segment should be on top of the stack.)
      if (endIndex - splitIndex > 1)
        context.Segments.Push(new Pair<int>(splitIndex, endIndex));

      if (splitIndex - startIndex > 1)
        context.Segments.Push(new Pair<int>(startIndex, splitIndex));
    }


    private static float ComputeError(Vector3F current, Vector3F start, Vector3F end, float parameter)
    {
      Vector3F lerpedValue = InterpolationHelper.Lerp(start, end, parameter);
      return (current - lerpedValue).Length;
    }


    private static float ComputeError(QuaternionF current, QuaternionF start, QuaternionF end, float parameter)
    {
      QuaternionF lerpedValue = InterpolationHelper.Lerp(start, end, parameter);
      return QuaternionF.GetAngle(current, lerpedValue);
    }
  }
}
