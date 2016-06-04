// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;


namespace DigitalRune.Animation.Character
{
  partial class SkeletonKeyFrameAnimation
  {
    // See AnimationHelper_Compression.cs

    // TODO: Multithreaded compression using ConcurrentStack<T>.
    private class CompressionContext
    {
      public BoneKeyFrameSRT[] UncompressedKeyFrames; // Original, uncompressed keyframes.
      public List<int> CompressedKeyFrames;           // Indices of the keyframes to keep.
      public Stack<Pair<int>> Segments;               // Segments that need to be processed.
      public float ScaleThreshold;
      public float RotationThreshold;                 // [rad]
      public float TranslationThreshold;
    }


    /// <summary>
    /// Compresses the animation using a simple lossy compression algorithm.
    /// </summary>
    /// <param name="scaleThreshold">The scale threshold.</param>
    /// <param name="rotationThreshold">The rotation threshold in degrees.</param>
    /// <param name="translationThreshold">The translation threshold.</param>
    /// <returns>
    /// The amount of removed key frames in the range [0, 1]. 0 means that no key frames have been
    /// removed. 0.5 means that 50% of the key frames have been removed. Etc.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method compresses the animation by removing key frames that can be computed by
    /// interpolation of nearby key frames. The threshold parameters define the allowed errors. If
    /// the thresholds are 0, this compression is lossless. If the thresholds are greater than 0
    /// (recommended), the compression is lossy. The best way to determine optimal thresholds is to
    /// compare the compressed animation with the uncompressed animation visually.
    /// </para>
    /// <para>
    /// This method does nothing if any threshold is negative.
    /// </para>
    /// </remarks>
    public float Compress(float scaleThreshold, float rotationThreshold, float translationThreshold)
    {
      // Abort if any threshold is negative.
      if (scaleThreshold < 0 || rotationThreshold < 0 || translationThreshold < 0)
        return 0;

      Unfreeze();

      if (_preprocessData == null)
        return 0;

      int totalKeyFrameCount = 0;
      int removedKeyFrameCount = 0;

      var context = new CompressionContext
      {
        CompressedKeyFrames = new List<int>(),
        Segments = new Stack<Pair<int>>(),
        ScaleThreshold = scaleThreshold,
        RotationThreshold = MathHelper.ToRadians(rotationThreshold),
        TranslationThreshold = translationThreshold
      };

      // Compress channels.
      foreach (var keyFrames in _preprocessData.Channels.Values)
      {
        const int startIndex = 0;
        int endIndex = keyFrames.Count - 1;
        totalKeyFrameCount += keyFrames.Count;
        if (endIndex - startIndex > 1)
        {
          Debug.Assert(context.UncompressedKeyFrames == null);
          context.UncompressedKeyFrames = keyFrames.ToArray();

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

          // Rebuild list of keyframes.
          keyFrames.Clear();
          context.CompressedKeyFrames.Sort();
          foreach (int index in context.CompressedKeyFrames)
            keyFrames.Add(context.UncompressedKeyFrames[index]);

          // Not necessary: Collections will be reduced in Freeze().
          //keyFrames.TrimExcess();

          removedKeyFrameCount += context.UncompressedKeyFrames.Length - context.CompressedKeyFrames.Count;

          // Clean up.
          context.UncompressedKeyFrames = null;
          context.CompressedKeyFrames.Clear();
        }
      }

      return (float)removedKeyFrameCount / totalKeyFrameCount;
    }


    private static void CompressSegment(Pair<int> segment, CompressionContext context)
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

      // ----- Rotation
      float maxError = 0;
      int maxErrorIndex = 0;
      for (int i = startIndex + 1; i < endIndex; i++)
      {
        var current = context.UncompressedKeyFrames[i];
        float parameter = (current.Time.Ticks - start.Time.Ticks) / ticks;
        QuaternionF lerpedRotation = InterpolationHelper.Lerp(start.Transform.Rotation, end.Transform.Rotation, parameter);
        float error = QuaternionF.GetAngle(current.Transform.Rotation, lerpedRotation);
        if (error > maxError)
        {
          maxError = error;
          maxErrorIndex = i;
        }
      }

      if (maxError > context.RotationThreshold)
      {
        SplitSegment(startIndex, maxErrorIndex, endIndex, context);
        return;
      }

      // ----- Translation
      maxError = 0;
      maxErrorIndex = 0;
      for (int i = startIndex + 1; i < endIndex; i++)
      {
        var current = context.UncompressedKeyFrames[i];
        float parameter = (current.Time.Ticks - start.Time.Ticks) / ticks;
        Vector3F lerpedTranslation = InterpolationHelper.Lerp(start.Transform.Translation, end.Transform.Translation, parameter);
        float error = (current.Transform.Translation - lerpedTranslation).Length;
        if (error > maxError)
        {
          maxError = error;
          maxErrorIndex = i;
        }
      }

      if (maxError > context.TranslationThreshold)
      {
        SplitSegment(startIndex, maxErrorIndex, endIndex, context);
        return;
      }

      // ----- Scale
      maxError = 0;
      maxErrorIndex = 0;
      for (int i = startIndex + 1; i < endIndex; i++)
      {
        var current = context.UncompressedKeyFrames[i];
        float parameter = (current.Time.Ticks - start.Time.Ticks) / ticks;
        Vector3F lerpedScale = InterpolationHelper.Lerp(start.Transform.Scale, end.Transform.Scale, parameter);
        float error = (current.Transform.Scale - lerpedScale).Length;
        if (error > maxError)
        {
          maxError = error;
          maxErrorIndex = i;
        }
      }

      if (maxError > context.ScaleThreshold)
      {
        SplitSegment(startIndex, maxErrorIndex, endIndex, context);
        return;
      }

      // When we get here: Segment is within tolerance.
    }


    private static void SplitSegment(int startIndex, int splitIndex, int endIndex, CompressionContext context)
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
  }
}
