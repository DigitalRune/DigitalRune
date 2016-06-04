// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Globalization;
#if ANIMATION
using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Animation.Character;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  partial class DRModelProcessor
  {
    //--------------------------------------------------------------
    #region Properties & Events
    //-------------------------------------------------------------- 

    /*
    /// <summary>
    /// Gets or sets the list of file names for additional animations as a <see cref="string"/>.
    /// </summary>
    /// <value>
    /// The file names for additional animations as a string; for example, "run.fbx;jump.fbx;turn.fbx".
    /// File paths must be relative to the folder of the current model.
    /// </value>
    [DisplayName("Animation Merge Files")]
    [Description("Merge several animations into this file. List the file names relative to the folder of the model file separated by ';'. Example: \"run.fbx;jump.fbx;turn.fbx\"")]
    [DefaultValue(typeof(string), "")]
    public virtual string AnimationMergeFiles
    {
      get { return _animationMergeFiles; }
      set { _animationMergeFiles = value; }
    }
    private string _animationMergeFiles = string.Empty;


    /// <summary>
    /// Gets or sets the file name of the animation split definition XML file.
    /// </summary>
    /// <value>
    /// The file name of the animation split definition XML file. The file path must be relative to 
    /// the folder of the current model.
    /// </value>
    [DisplayName("Animation Split File")]
    [Description("Split animation into separate animations. Specify the file name of the split definition XML file relative to the folder of the model file. Example: \"Dude_AnimationSplits.xml\"")]
    [DefaultValue(typeof(string), "")]
    public virtual string AnimationSplitFile
    {
      get { return _animationSplitFile; }
      set { _animationSplitFile = value; }
    }
    private string _animationSplitFile = string.Empty;


    /// <summary>
    /// Gets or sets the allowed scale error for key frame compression.
    /// </summary>
    /// <value>
    /// The allowed scale error for key frame compression, for example 0.01. Set to -1 to disable 
    /// compression.
    /// </value>
    [DisplayName("Animation Scale Compression")]
    [Description("Define the allowed scale error after key frame compression, for example 0.01. Set to -1 to disable compression.")]
    [DefaultValue(typeof(float), "-1")]
    public virtual float AnimationScaleCompression
    {
      get { return _animationScaleCompression; }
      set { _animationScaleCompression = value; }
    }
    private float _animationScaleCompression = -1;


    /// <summary>
    /// Gets or sets the allowed rotation error for key frame compression.
    /// </summary>
    /// <value>
    /// The allowed rotation error for key frame compression (in degrees), for example 2. Set 
    /// to -1 to disable compression.
    /// </value>
    [DisplayName("Animation Rotation Compression")]
    [Description("Define the allowed rotation error after key frame compression (in degrees), for example 2. Set to -1 to disable compression.")]
    [DefaultValue(typeof(float), "-1")]
    public virtual float AnimationRotationCompression
    {
      get { return _animationRotationCompression; }
      set { _animationRotationCompression = value; }
    }
    private float _animationRotationCompression = -1;


    /// <summary>
    /// Gets or sets the allowed translation error for key frame compression.
    /// </summary>
    /// <value>
    /// The allowed translation error for key frame compression, for example 0.001. Set to -1 to 
    /// disable compression.
    /// </value>
    [DisplayName("Animation Translation Compression")]
    [Description("Define the allowed translation error for key frame compression, for example 0.001. Set to -1 to disable compression.")]
    [DefaultValue(typeof(float), "-1")]
    public virtual float AnimationTranslationCompression
    {
      get { return _animationTranslationCompression; }
      set { _animationTranslationCompression = value; }
    }
    private float _animationTranslationCompression = -1;
    */
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Merge animations defined in other files.
    private void MergeAnimationFiles()
    {
      if (_modelDescription != null)
      {
        var animationDescription = _modelDescription.Animation;
        if (animationDescription != null)
        {
          var animationFiles = animationDescription.MergeFiles;
          AnimationMerger.Merge(animationFiles, _rootBone.Animations, _input.Identity, _context);
        }
      }
    }


    // Initialize _skeleton from BoneContents.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    private void BuildSkeleton()
    {
      // Get an array of all bones in depth-first order.
      // (Same as MeshHelper.FlattenSkeleton(root).)
      var bones = TreeHelper.GetSubtree(_rootBone, n => n.Children.OfType<BoneContent>(), true)
                            .ToList();

      // Create list of parent indices, bind pose transformations and bone names.
      var boneParents = new List<int>();
      var bindTransforms = new List<SrtTransform>();
      var boneNames = new List<string>();
      int numberOfWarnings = 0;
      foreach (var bone in bones)
      {
        int parentIndex = bones.IndexOf(bone.Parent as BoneContent);
        boneParents.Add(parentIndex);

        // Log warning for invalid transform matrices - but not too many warnings.
        if (numberOfWarnings < 2)
        {
          if (!SrtTransform.IsValid((Matrix44F)bone.Transform))
          {
            if (numberOfWarnings < 1)
              _context.Logger.LogWarning(null, _input.Identity, "Bone transform is not supported. Bone transform matrices may only contain scaling, rotation and translation.");
            else
              _context.Logger.LogWarning(null, _input.Identity, "More unsupported bone transform found.");

            numberOfWarnings++;
          }
        }

        bindTransforms.Add(SrtTransform.FromMatrix(bone.Transform));

        if (boneNames.Contains(bone.Name))
        {
          string message = String.Format(CultureInfo.InvariantCulture, "Duplicate bone name (\"{0}\") found.", bone.Name);
          throw new InvalidContentException(message, _input.Identity);
        }

        boneNames.Add(bone.Name);
      }

      // Create and return a new skeleton instance.
      _skeleton = new Skeleton(boneParents, boneNames, bindTransforms);
    }


    // Extracts all animations and stores them in _animations.
    private void BuildAnimations()
    {
      SplitAnimations();

      _animations = new Dictionary<string, SkeletonKeyFrameAnimation>();
      foreach (var item in _rootBone.Animations)
      {
        string animationName = item.Key;
        AnimationContent animationContent = item.Value;

        // Convert the AnimationContent to a SkeletonKeyFrameAnimation.
        var skeletonAnimation = BuildAnimation(animationContent);
        if (skeletonAnimation != null)
          _animations.Add(animationName, skeletonAnimation);
      }
    }


    // Split animation into separate animations based on a split definition defined in XML file.
    private void SplitAnimations()
    {
      if (_modelDescription != null)
      {
        var animationDescription = _modelDescription.Animation;
        if (animationDescription != null)
        {
          var animationsSplits = animationDescription.Splits;
          AnimationSplitter.Split(_rootBone.Animations, animationsSplits, _input.Identity, _context);
        }
      }
    }


    // Converts an AnimationContent to a SkeletonKeyFrameAnimation.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private SkeletonKeyFrameAnimation BuildAnimation(AnimationContent animationContent)
    {
      string name = animationContent.Name;

      // Add loop frame?
      bool addLoopFrame = false;
      if (_modelDescription != null)
      {
        var animationDescription = _modelDescription.Animation;
        if (animationDescription != null)
        {
          addLoopFrame = animationDescription.AddLoopFrame ?? false;

          if (animationDescription.Splits != null)
          {
            foreach (var split in animationDescription.Splits)
            {
              if (split.Name == name)
              {
                if (split.AddLoopFrame.HasValue)
                  addLoopFrame = split.AddLoopFrame.Value;

                break;
              }
            }
          }
        }
      }

      var animation = new SkeletonKeyFrameAnimation { EnableInterpolation = true };

      // Process all animation channels (each channel animates a bone).
      int numberOfKeyFrames = 0;
      foreach (var item in animationContent.Channels)
      {
        string channelName = item.Key;
        AnimationChannel channel = item.Value;

        int boneIndex = _skeleton.GetIndex(channelName);
        if (boneIndex != -1)
        {
          SrtTransform? loopFrame = null;

          var bindPoseRelativeInverse = _skeleton.GetBindPoseRelative(boneIndex).Inverse;
          foreach (AnimationKeyframe keyframe in channel)
          {
            TimeSpan time = keyframe.Time;
            SrtTransform transform = SrtTransform.FromMatrix(keyframe.Transform);

            // The matrix in the key frame is the transformation in the coordinate space of the
            // parent bone. --> Convert it to a transformation relative to the animated bone.
            transform = bindPoseRelativeInverse * transform;

            // To start with minimal numerical errors, we normalize the rotation quaternion.
            transform.Rotation.Normalize();

            if (loopFrame == null)
              loopFrame = transform;

            if (!addLoopFrame || time < animationContent.Duration)
              animation.AddKeyFrame(boneIndex, time, transform);

            numberOfKeyFrames++;
          }

          if (addLoopFrame && loopFrame.HasValue)
            animation.AddKeyFrame(boneIndex, animationContent.Duration, loopFrame.Value);
        }
        else
        {
          _context.Logger.LogWarning(
            null, animationContent.Identity, 
            "Found animation for bone \"{0}\", which is not part of the skeleton.", 
            channelName);
        }
      }

      if (numberOfKeyFrames == 0)
      {
        _context.Logger.LogWarning(null, animationContent.Identity, "Animation is ignored because it has no keyframes.");
        return null;
      }

      // Compress animation to save memory.
      if (_modelDescription != null)
      {
        var animationDescription = _modelDescription.Animation;
        if (animationDescription != null)
        {
          float removedKeyFrames = animation.Compress(
            animationDescription.ScaleCompression,
            animationDescription.RotationCompression,
            animationDescription.TranslationCompression);

          if (removedKeyFrames > 0)
          {
            _context.Logger.LogImportantMessage("{0}: Compression removed {1:P} of all key frames.",
                                                string.IsNullOrEmpty(name) ? "Unnamed" : name,
                                                removedKeyFrames);
          }
        }
      }

      // Finalize the animation. (Optimizes the animation data for fast runtime access.)
      animation.Freeze();

      return animation;
    }
    #endregion
  }
}
#endif
