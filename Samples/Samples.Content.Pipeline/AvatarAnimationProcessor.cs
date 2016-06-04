#if XNA
#region License
// This file extends the CustomAvatarAnimationProcessor.cs from the 
// AppHub Custom Avatar Animation Sample which is licensed under the.
// Microsoft Permissive License (Ms-PL, see http://create.msdn.com/downloads/?id=15) 
//
// Original copyrights:
//-----------------------------------------------------------------------------
// CustomAvatarAnimationProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.GamerServices;


namespace Samples.Content.Pipeline
{
  // This content processor processes a model file containing a custom avatar animation.
  // The processor creates a TimelineGroup that contains an animation for the expression 
  // and the skeleton of an avatar.
  // The animation can be compressed (see properties Compress*Threshold).
  [ContentProcessor(DisplayName = "Avatar Animation Processor (DigitalRune Samples)")]
  public class AvatarAnimation : ContentProcessor<NodeContent, TimelineGroup>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The bone bind poses.
    private readonly List<Matrix> _bindPoses = new List<Matrix>();

    // A mapping between bone names and indices.
    private Dictionary<string, int> _boneNames;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    [DisplayName("Facial expression file")]
    [Description("Specify the file to use for facial animations (relative to the folder of the animation file).")]
    public string ExpressionFile { get; set; }


    [DisplayName("Compression - Translation Threshold")]
    [Description("Define the allowed translation error for key frame compression, for example 0.001. Set to -1 to disable compression.")]
    [DefaultValue(typeof(float), "0.001")]
    public float CompressionTranslationThreshold
    {
      get { return _compressionTranslationThreshold; }
      set { _compressionTranslationThreshold = value; }
    }
    private float _compressionTranslationThreshold = -1;


    [DisplayName("Compression - Rotation Threshold")]
    [Description("Define the allowed rotation error after key frame compression (in degrees), for example 0.01. Set to -1 to disable compression.")]
    [DefaultValue(typeof(float), "0.01")]
    public float CompressionRotationThreshold
    {
      get { return _compressionRotationThreshold; }
      set { _compressionRotationThreshold = value; }
    }
    private float _compressionRotationThreshold = -1;


    [DisplayName("Compression - Scale Threshold")]
    [Description("Define the allowed scale error after key frame compression, for example 0.01. Set to -1 to disable compression.")]
    [DefaultValue(typeof(float), "0.01")]
    public float CompressionScaleThreshold
    {
      get { return _compressionScaleThreshold; }
      set { _compressionScaleThreshold = value; }
    }
    private float _compressionScaleThreshold = -1;
    #endregion


    //--------------------------------------------------------------
    #region General Methods
    //--------------------------------------------------------------

    public override TimelineGroup Process(NodeContent input, ContentProcessorContext context)
    {
      // Uncomment this to attach and launch a debugger.
      //System.Diagnostics.Debugger.Launch();

      // Get the skeleton node.
      NodeContent skeleton = FindSkeleton(input);
      if (skeleton == null)
        throw new InvalidContentException("Avatar skeleton not found.", input.Identity);
      if (skeleton.Animations.Count < 1)
        throw new InvalidContentException("No animation was found in the file.", input.Identity);
      if (skeleton.Animations.Count > 1)
        throw new InvalidContentException("More than one animation was found.", input.Identity);

      // Remove the extra bones that we are not using.
      RemoveEndBonesAndFixBoneNames(skeleton);

      // Create a list of the bones from the skeleton hierarchy.
      IList<NodeContent> bones = FlattenSkeleton(skeleton);

      if (bones.Count != AvatarRenderer.BoneCount)
        throw new InvalidContentException("Invalid number of bones found.", input.Identity);

      // Fill the bind pose array with the transforms from the bones.
      foreach (NodeContent bone in bones)
        _bindPoses.Add(bone.Transform);

      // Build up a table mapping bone names to indices.
      _boneNames = new Dictionary<string, int>();
      for (int i = 0; i < bones.Count; i++)
      {
        string boneName = bones[i].Name;
        if (!string.IsNullOrEmpty(boneName))
          _boneNames.Add(boneName, i);
      }

      // Create the custom animation data.
      // From the error-checking above, we know there will only be one animation.
      AnimationContent animationContent = skeleton.Animations.Values.First();
      SkeletonKeyFrameAnimation skeletonAnimation = ProcessSkeletonAnimation(animationContent, context);
      AvatarExpressionKeyFrameAnimation expressionAnimation = ProcessExpressionAnimation(input, context);

      var timelineGroup = new TimelineGroup();
      if (skeletonAnimation != null)
        timelineGroup.Add(skeletonAnimation);
      if (expressionAnimation != null)
        timelineGroup.Add(expressionAnimation);

      return timelineGroup;
    }
    #endregion


    //--------------------------------------------------------------
    #region Skeleton Processing
    //--------------------------------------------------------------

    /// <summary>
    /// Finds node that is the root node of the skeleton.
    /// </summary>
    private static NodeContent FindSkeleton(NodeContent input)
    {
      if (input == null)
        throw new ArgumentNullException("input");

      if (input.Name.Contains("BASE__Skeleton"))
        return input;

      // Search children.
      NodeContent skeleton = null;
      foreach (NodeContent child in input.Children)
      {
        skeleton = FindSkeleton(child);
        if (skeleton != null)
          break;
      }

      return skeleton;
    }


    /// <summary>
    /// Removes each bone node that contains "_END" in the name.
    /// </summary>
    /// <remarks>
    /// These bones are not needed by the AvatarRenderer runtime but
    /// are part of the Avatar rig used in modeling programs
    /// </remarks>
    private static void RemoveEndBonesAndFixBoneNames(NodeContent bone)
    {
      // safety-check the parameter
      if (bone == null)
      {
        throw new ArgumentNullException("bone");
      }

      // Remove unneeded text from the bone name
      bone.Name = CleanBoneName(bone.Name);

      // Remove each child bone that contains "_END" in the name
      for (int i = 0; i < bone.Children.Count; ++i)
      {
        NodeContent child = bone.Children[i];
        if (child.Name.Contains("_END"))
        {
          bone.Children.Remove(child);
          --i;
        }
        else
        {
          // Recursively search through the remaining child bones
          RemoveEndBonesAndFixBoneNames(child);
        }
      }
    }


    /// <summary>
    /// Removes extra text from the bone names.
    /// </summary>
    private static string CleanBoneName(string boneName)
    {
      boneName = boneName.Replace("__Skeleton", "");
      return boneName;
    }


    /// <summary>
    /// Flattens the skeleton into a list. The order in the list is sorted by
    /// depth first and then by name.
    /// </summary>
    private static IList<NodeContent> FlattenSkeleton(NodeContent skeleton)
    {
      // safety check on the parameter
      if (skeleton == null)
      {
        throw new ArgumentNullException("skeleton");
      }

      // Create the destination list of bones
      List<NodeContent> bones = new List<NodeContent>();

      // Create a list to track current items in the level of tree
      List<NodeContent> currentLevel = new List<NodeContent>();

      // Add the root node of the skeleton to the list
      currentLevel.Add(skeleton);

      while (currentLevel.Count > 0)
      {
        // Create a list of bones to track the next level of the tree
        List<NodeContent> nextLevel = new List<NodeContent>();

        // Sort the bones in the current level 
        IEnumerable<NodeContent> sortedBones = currentLevel.OrderBy(item => item.Name);

        // Add the newly sorted items to the output list
        foreach (NodeContent bone in sortedBones)
        {
          bones.Add(bone);
          // Add the bone's children to the next-level list
          foreach (NodeContent child in bone.Children)
            nextLevel.Add(child);
        }

        // the next level is now the current level
        currentLevel = nextLevel;
      }

      // return the flattened array of bones
      return bones;
    }
    #endregion


    //--------------------------------------------------------------
    #region Avatar Expression Animation
    //--------------------------------------------------------------

    /// <summary>
    /// Converts the input expression animation file into expression animation keyframes.
    /// </summary>
    private AvatarExpressionKeyFrameAnimation ProcessExpressionAnimation(NodeContent input, ContentProcessorContext context)
    {
      if (string.IsNullOrEmpty(ExpressionFile))
        return null;

      // Create a AvatarExpression key frame animation that will animate the Expression 
      // property of an AvatarPose.
      var animation = new AvatarExpressionKeyFrameAnimation
      {
        TargetProperty = "Expression",
      };

      // Let the content pipeline know that we depend on this file and we need to rebuild the 
      // content if the file is modified.
      string sourcePath = Path.GetDirectoryName(input.Identity.SourceFilename);
      string filePath = Path.GetFullPath(Path.Combine(sourcePath, ExpressionFile));
      context.AddDependency(filePath);

      FileStream fs = File.OpenRead(filePath);
      StreamReader sr = new StreamReader(fs);
      while (!sr.EndOfStream)
      {
        string currentLine = sr.ReadLine();

        // Skip comment lines
        if (currentLine.StartsWith("#"))
          continue;

        string[] Components = currentLine.Split(',');

        // Check for the correct number of components
        if (Components.Length != 6)
          throw new InvalidContentException("Error processing facial expression file", input.Identity);

        try
        {
          TimeSpan time = TimeSpan.FromMilliseconds(Convert.ToDouble(Components[0]));
          AvatarExpression avatarExpression = new AvatarExpression();
          avatarExpression.LeftEye = (AvatarEye)Convert.ToInt32(Components[1]);
          avatarExpression.LeftEyebrow = (AvatarEyebrow)Convert.ToInt32(Components[2]);
          avatarExpression.Mouth = (AvatarMouth)Convert.ToInt32(Components[3]);
          avatarExpression.RightEye = (AvatarEye)Convert.ToInt32(Components[4]);
          avatarExpression.RightEyebrow = (AvatarEyebrow)Convert.ToInt32(Components[5]);

          // Add key frame.
          var keyFrame = new KeyFrame<AvatarExpression>(time, avatarExpression);
          animation.KeyFrames.Add(keyFrame);
        }
        catch (Exception)
        {
          throw new InvalidContentException("Error processing facial expression file", input.Identity);
        }
      }

      if (animation.KeyFrames.Count == 0)
        return null;

      // Sort the animation frames
      animation.KeyFrames.Sort();

      return animation;
    }
    #endregion


    //--------------------------------------------------------------
    #region Avatar Skeleton Animation
    //--------------------------------------------------------------

    /// <summary>
    /// Converts an intermediate-format content pipeline AnimationContent object 
    /// to a SkeletonKeyFrameAnimation.
    /// </summary>
    private SkeletonKeyFrameAnimation ProcessSkeletonAnimation(AnimationContent animationContent, ContentProcessorContext context)
    {
      if (animationContent.Duration <= TimeSpan.Zero)
        throw new InvalidContentException("Animation has a zero duration.", animationContent.Identity);

      // Create a SkeletonPose key frame animation that will animate the SkeletonPose 
      // property of an AvatarPose.
      var animation = new SkeletonKeyFrameAnimation
      {
        TargetProperty = "SkeletonPose",
      };

      // Process each channel in the animation
      int numberOfKeyFrames = 0;
      foreach (KeyValuePair<string, AnimationChannel> item in animationContent.Channels)
      {
        var channelName = item.Key;
        channelName = CleanBoneName(channelName);

        var channel = item.Value;

        // Don't add animation nodes with "_END" in the name
        // -- These bones were removed from the skeleton already
        if (channelName.Contains("_END"))
        {
          continue;
        }

        // Look up what bone this channel is controlling.
        int boneIndex;
        if (!_boneNames.TryGetValue(channelName, out boneIndex))
        {
          var message = string.Format("Found animation for bone '{0}', which is not part of the skeleton.", channelName);
          throw new InvalidContentException(message, animationContent.Identity);
        }

        // Convert and add the key frame data.
        foreach (AnimationKeyframe keyframe in channel)
        {
          var time = keyframe.Time;
          var matrix = CreateKeyFrameMatrix(keyframe, boneIndex);
          var srt = SrtTransform.FromMatrix(matrix);
          animation.AddKeyFrame(boneIndex, time, srt);

          numberOfKeyFrames++;
        }
      }

      if (numberOfKeyFrames == 0)
        throw new InvalidContentException("Animation has no key frames.", animationContent.Identity);

      // Compress animation to safe memory.
      float removedKeyFrames = animation.Compress(
        CompressionScaleThreshold,
        CompressionRotationThreshold,
        CompressionTranslationThreshold);

      if (removedKeyFrames > 0)
        context.Logger.LogImportantMessage("Compression removed {0:P} of all key frames.", removedKeyFrames);

      // Finalize the skeleton key frame animation. This optimizes the internal data structures.
      animation.Freeze();

      return animation;
    }


    /// <summary>
    /// Create an AvatarRenderer-friendly matrix from an animation keyframe.
    /// </summary>
    /// <param name="keyframe">The keyframe to be converted.</param>
    /// <param name="boneIndex">The index of the bone this keyframe is for.</param>
    /// <returns>The converted AvatarRenderer-friendly matrix for this bone and keyframe.</returns>
    private Matrix CreateKeyFrameMatrix(AnimationKeyframe keyframe, int boneIndex)
    {
      // safety-check the parameter
      if (keyframe == null)
        throw new ArgumentNullException("keyframe");

      // Retrieve the transform for this keyframe
      Matrix keyframeMatrix;

      // The root node is transformed by the root of the bind pose
      // We need to make the keyframe relative to the root
      if (boneIndex == 0)
      {
        // When the animation is exported the bind pose can have the 
        // wrong translation of the root node so we hard code it here
        Vector3 bindPoseTranslation = new Vector3(0.000f, 75.5199f, -0.8664f);

        Matrix keyTransform = keyframe.Transform;

        Matrix inverseBindPose = _bindPoses[boneIndex];
        inverseBindPose.Translation -= bindPoseTranslation;
        inverseBindPose = Matrix.Invert(inverseBindPose);

        keyframeMatrix = (keyTransform * inverseBindPose);
        keyframeMatrix.Translation -= bindPoseTranslation;

        // Scale from cm to meters
        keyframeMatrix.Translation *= 0.01f;
      }
      else
      {
        keyframeMatrix = keyframe.Transform;
        // Only the root node can have translation
        keyframeMatrix.Translation = Vector3.Zero;
      }

      return keyframeMatrix;
    }
    #endregion
  }
}
#endif