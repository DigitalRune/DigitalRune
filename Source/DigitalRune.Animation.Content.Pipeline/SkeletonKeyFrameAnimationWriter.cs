// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Animation.Character;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Animation.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="SkeletonKeyFrameAnimationWriter"/> to binary format.
  /// </summary>
  [ContentTypeWriter]
  public class SkeletonKeyFrameAnimationWriter : ContentTypeWriter<SkeletonKeyFrameAnimation>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(SkeletonKeyFrameAnimation).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(SkeletonKeyFrameAnimationReader).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    protected override void Write(ContentWriter output, SkeletonKeyFrameAnimation value)
    {
      value.Freeze();

      dynamic internals = value.Internals;
      TimeSpan[] times = internals.Times;
      int[] channels = internals.Channels;
      float[] weights = internals.Weights;
      int[] indices = internals.Indices;
      int[] keyFrameTypes = internals.KeyFrameTypes;
      TimeSpan[][] keyFrameTimes = internals.KeyFrameTimes;
      SrtTransform[][] keyFrameTransforms = internals.KeyFrameTransforms;

      // _totalDuration
      output.WriteRawObject(value.GetTotalDuration());

      // _times
      output.Write(times.Length);
      for (int i = 0; i < times.Length; i++)
        output.WriteRawObject(times[i]);

      // _channels
      int numberOfChannels = channels.Length;
      output.Write(numberOfChannels);
      for (int channelIndex = 0; channelIndex < numberOfChannels; channelIndex++)
        output.Write(channels[channelIndex]);

      // _weights
      for (int channelIndex = 0; channelIndex < numberOfChannels; channelIndex++)
        output.Write(weights[channelIndex]);

      // _indices
      Debug.Assert(indices.Length == numberOfChannels * times.Length, "Number of indices expected to be: number of channels * number of times");
      for (int i = 0; i < indices.Length; i++)
        output.Write(indices[i]);

      // _keyFrameTypes, _keyFrames
      for (int channelIndex = 0; channelIndex < numberOfChannels; channelIndex++)
      {
        int boneKeyFrameType = keyFrameTypes[channelIndex];
        output.Write(boneKeyFrameType);

        int numberOfKeyFrames = keyFrameTimes[channelIndex].Length;
        output.Write(numberOfKeyFrames);

        if (boneKeyFrameType == 0 /*BoneKeyFrameType.R*/)
        {
          for (int keyFrameIndex = 0; keyFrameIndex < numberOfKeyFrames; keyFrameIndex++)
          {
            output.WriteRawObject(keyFrameTimes[channelIndex][keyFrameIndex]);
            output.WriteRawObject(keyFrameTransforms[channelIndex][keyFrameIndex].Rotation);
          }
        }
        else if (boneKeyFrameType == 1 /*BoneKeyFrameType.RT*/)
        {
          for (int keyFrameIndex = 0; keyFrameIndex < numberOfKeyFrames; keyFrameIndex++)
          {
            output.WriteRawObject(keyFrameTimes[channelIndex][keyFrameIndex]);
            output.WriteRawObject(keyFrameTransforms[channelIndex][keyFrameIndex].Rotation);
            output.WriteRawObject(keyFrameTransforms[channelIndex][keyFrameIndex].Translation);
          }
        }
        else
        {
          for (int keyFrameIndex = 0; keyFrameIndex < numberOfKeyFrames; keyFrameIndex++)
          {
            output.WriteRawObject(keyFrameTimes[channelIndex][keyFrameIndex]);
            output.WriteRawObject(keyFrameTransforms[channelIndex][keyFrameIndex].Scale);
            output.WriteRawObject(keyFrameTransforms[channelIndex][keyFrameIndex].Rotation);
            output.WriteRawObject(keyFrameTransforms[channelIndex][keyFrameIndex].Translation);
          }
        }
      }

      output.Write(value.EnableInterpolation);
      output.Write((int)value.FillBehavior);
      output.Write(value.IsAdditive);

      output.Write(value.TargetObject != null);
      if (value.TargetObject != null)
        output.Write(value.TargetObject);

      output.Write(value.TargetProperty != null);
      if (value.TargetProperty != null)
        output.Write(value.TargetProperty);
    }
  }
}
