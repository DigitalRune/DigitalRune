// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DigitalRune.Mathematics.Algebra;
#if XNA || MONOGAME
using Microsoft.Xna.Framework;
#endif


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Defines an animation pose of a <see cref="Character.Skeleton"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="Character.Skeleton"/> class defines a skeleton in bind pose. The 
  /// <see cref="SkeletonPose"/> can be used to animate the bones of a skeleton. There are two ways
  /// to animate a skeleton using the animation system:
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// The skeleton pose implements the interface <c>IAnimatableProperty&lt;SkeletonPose&gt;</c>.
  /// This means that the skeleton pose can be treated as one animation value and can be animated as
  /// a whole. A skeleton pose can, for example, be animated using a 
  /// <see cref="SkeletonKeyFrameAnimation"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// The skeleton pose also implements the interface <c>IAnimatableObject</c>. The bones of the
  /// skeleton pose are animatable properties. This means that the bones can be animated 
  /// individually. For example, a <see cref="SrtKeyFrameAnimation"/> can be applied directly to a 
  /// single bone.
  /// </description>
  /// </item>
  /// </list>
  /// You can find more details below.
  /// </para>
  /// <para>
  /// <strong>Bone Transforms:</strong><br/>
  /// A <i>bone transform</i> is a local transformation that is applied to a bone. Bones are 
  /// animated by changing the bone transforms (see <see cref="GetBoneTransform"/> and 
  /// <see cref="SetBoneTransform"/>). A bone transform is defined using a <see cref="SrtTransform"/> 
  /// given in bone space. If a bone transform is the identity transformation (see 
  /// <see cref="SrtTransform.Identity"/>), then the bone is not animated and rendered in its bind 
  /// pose.
  /// </para>
  /// <para>
  /// <strong>Bone Poses:</strong><br/>
  /// A <i>bone pose transformation matrix</i> (<i>bone pose</i>) defines the resulting pose of bone
  /// after the bone transforms are applied. A bone pose describes the bone's position, orientation 
  /// and scale relative to another coordinate space. A <i>relative bone pose</i> describes the pose
  /// of a bone relative to the parent bone (see <see cref="GetBonePoseRelative"/>). An 
  /// <i>absolute bone pose</i> describes the pose of a bone relative to model space 
  /// (<see cref="GetBonePoseAbsolute"/>).
  /// </para>
  /// <para>
  /// The bone poses are computed and updated automatically: Whenever a bone transform is changed, 
  /// the relative bone pose, absolute bone pose and the skinning matrices of the affected bone 
  /// needs to be updated. Additionally, the absolute bone poses and the skinning matrices of all 
  /// bones attached to this bone need to be updated as well. The skeleton pose automatically keeps 
  /// track of which poses and matrices need to be recomputed. The recomputation is performed 
  /// automatically as soon as one of these values is required.
  /// </para>
  /// <para>
  /// To update all derived transformations at once, <see cref="Update"/> can be called. 
  /// </para>
  /// <para>
  /// <strong>IAnimatableProperty Implementation:</strong><br/>
  /// The class <see cref="SkeletonPose"/> implements the interface 
  /// <c>IAnimatableProperty&lt;SkeletonPose&gt;</c> (see <see cref="IAnimatableProperty{T}"/>).
  /// This means the <see cref="SkeletonPose"/> itself is an animatable property. It can 
  /// be animated using any animation of type <c>IAnimation&lt;SkeletonPose&gt;</c>. Typically a 
  /// <see cref="SkeletonKeyFrameAnimation"/> is used to animate the <see cref="SkeletonPose"/>. 
  /// This is the most efficient way to animate a skeleton.
  /// </para>
  /// <para>
  /// Note: The <see cref="SkeletonPose"/> as a <see cref="IAnimatableProperty"/> does not have
  /// a base value and therefore cannot be used in some from-to-by animations and similar animations
  /// that require a base value.
  /// </para>
  /// <para>
  /// <strong>IAnimatableObject Implementation (For Advanced Uses Only!):</strong><br/>
  /// The class <see cref="SkeletonPose"/> additionally implements the interface 
  /// <see cref="IAnimatableObject"/>. This means that the bones of the skeleton can also be 
  /// animated independently. The animatable properties are the bone transforms given as
  /// <c>IAnimatableProperty&lt;SrtTransform&gt;</c>. The animatable properties can be accessed by
  /// calling the method <see cref="IAnimatableObject.GetAnimatableProperty{T}"/> passing the name 
  /// of the desired bone as the parameter. (Bones need to be named if they should be animated 
  /// independently.)
  /// </para>
  /// <para>
  /// For example, a <see cref="SrtKeyFrameAnimation"/> (or any other animation that implements
  /// <c>IAnimation&lt;SrtTransform&gt;</c>) can be applied directly to a bone.
  /// </para>
  /// <para>
  /// Or, multiple <see cref="SrtKeyFrameAnimation"/>s can be grouped together in a 
  /// <see cref="TimelineGroup"/>. The animations can be assigned to different bones by setting
  /// the animation's <see cref="IAnimation.TargetProperty"/> to the name of the bone. The 
  /// <see cref="TimelineGroup"/> can then be played as one animation.
  /// </para>
  /// <para>
  /// Animating the <see cref="SkeletonPose"/> this way is very flexible - but slower than 
  /// animating it with a single <see cref="SkeletonKeyFrameAnimation"/>.
  /// </para>
  /// <para>
  /// Note: The <see cref="IAnimatableProperty"/>s of the individual bones do not have a base value
  /// and therefore cannot be used in some from-to-by animations or similar animations that require
  /// a base value.
  /// </para>
  /// <para>
  /// <strong>Tip:</strong><br/>
  /// When bone transforms are manipulated regularly, e.g. in an IK solver, numerical errors can 
  /// accumulate. If this happens and the skeleton seems to "explode", try to normalize the rotation
  /// quaternions regularly.
  /// </para>
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public sealed class SkeletonPose : IAnimatableObject, IAnimatableProperty<SkeletonPose>, IRecyclable
  {
    // Notes:
    // In this class we skip the input parameter checking for bone indices. Array access will
    // automatically throw an exception if a bone index is out of range.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    internal readonly SrtTransform[] BoneTransforms;

    // Only allocated when needed.
    private SkeletonBoneAccessor _boneAccessor;

    // The bone transforms as animatable properties. Only allocated when needed.
    private AnimatableBoneTransform[] _animatableBoneTransforms;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the skeleton pose.
    /// </summary>
    /// <value>
    /// The name of this instance. The default value is the name of the <see cref="Skeleton"/>.
    /// </value>
    public string Name { get; set; }  // Needed for IAnimatableObject


    /// <summary>
    /// Gets the skeleton.
    /// </summary>
    /// <value>The skeleton.</value>
    public Skeleton Skeleton { get; private set; }


    /// <summary>
    /// Gets the skinning matrices. 
    /// </summary>
    /// <value>The skinning matrices.</value>
    /// <remarks>
    /// <para>
    /// This array contains one element per bone. The array element is the skinning matrix which
    /// is a matrix that transforms a position from bone space in the bind pose to model space in 
    /// the animated model. This matrices are used for mesh skinning.
    /// </para>
    /// <para>
    /// This property returns an internal array that is allocated when needed. Therefore, it is not
    /// recommended to modify the elements in this array!
    /// </para>
    /// </remarks>
    public Matrix44F[] SkinningMatrices
    {
      get
      {
        EnsureBoneAccessor();
        return _boneAccessor.SkinningMatrices;
      }
    }


#if XNA || MONOGAME
    /// <summary>
    /// Gets the skinning matrices. (Only available in the XNA-compatible build.)
    /// </summary>
    /// <value>The skinning matrices.</value>
    /// <remarks>
    /// <para>
    /// This type is available only in the XNA-compatible build of the DigitalRune.Animation.dll.
    /// </para>
    /// <para>
    /// This array contains one element per bone. The array element is the skinning matrix which
    /// is a matrix that transforms a position from bone space in the bind pose to model space in 
    /// the animated model. This matrices are used for mesh skinning.
    /// </para>
    /// <para>
    /// This property returns an internal array that is allocated when needed. Therefore, it is not
    /// recommended to modify the elements in this array!
    /// </para>
    /// </remarks>
    public Matrix[] SkinningMatricesXna
    {
      get
      {
        EnsureBoneAccessor();
        return _boneAccessor.SkinningMatricesXna;
      }
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SkeletonPose"/> class.
    /// This constructor is used by the resource pool.
    /// </summary>
    /// <param name="skeleton">The skeleton.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeleton" /> is <see langword="null"/>.
    /// </exception>
    internal SkeletonPose(Skeleton skeleton)
    {
      if (skeleton == null)
        throw new ArgumentNullException("skeleton");

      Skeleton = skeleton;
      Name = skeleton.Name;

      BoneTransforms = new SrtTransform[skeleton.NumberOfBones];
      for (int i = 0; i < BoneTransforms.Length; i++)
        BoneTransforms[i] = SrtTransform.Identity;
    }


    /// <summary>
    /// Creates an instance of the <see cref="SkeletonPose"/> class. (This method 
    /// reuses a previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <param name="skeleton">The skeleton.</param>
    /// <returns>
    /// A new or reusable instance of the <see cref="SkeletonPose"/> class.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method tries to obtain a previously recycled instance from a resource pool if resource
    /// pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>). If no
    /// object is available, a new instance is automatically allocated on the heap. 
    /// </para>
    /// <para>
    /// The owner of the object should call <see cref="Recycle"/> when the instance is no longer 
    /// needed.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeleton"/> is <see langword="null"/>.
    /// </exception>
    public static SkeletonPose Create(Skeleton skeleton)
    {
      if (skeleton == null)
        throw new ArgumentNullException("skeleton");

      var skeletonPose = skeleton.SkeletonPosePool.Obtain();
      skeletonPose.Name = skeleton.Name;

      Debug.Assert(skeletonPose.Skeleton == skeleton, "The Skeleton of the SkeletonPose is not set properly.");
      Debug.Assert(skeletonPose.BoneTransforms.Length == skeleton.NumberOfBones, "The BoneTransforms array of the SkeletonPose has the wrong size.");
      Debug.Assert(skeletonPose.BoneTransforms.All(srt => srt == SrtTransform.Identity), "The BoneTransforms array of the SkeletonPose is not initialized properly.");
      Debug.Assert(skeletonPose._boneAccessor == null, "The SkeletonBoneAccessors should have been recycled.");
      Debug.Assert(skeletonPose._animatableBoneTransforms == null, "The AnimatableBoneTransforms should have been recycled.");

      return skeletonPose;
    }


    /// <summary>
    /// Recycles this instance of the <see cref="SkeletonPose"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    public void Recycle()
    {
      // Recycle SkeletonBoneAccessors.
      if (_boneAccessor != null)
      {
        _boneAccessor.Recycle();
        _boneAccessor = null;
      }

      // Recycle AnimatableBoneTransforms.
      if (_animatableBoneTransforms != null)
      {
        foreach (var animatableBoneTransform in _animatableBoneTransforms)
          animatableBoneTransform.SkeletonPose = null;

        Skeleton.AnimatableBoneTransformsPool.Recycle(_animatableBoneTransforms);
        _animatableBoneTransforms = null;
      }

      // Reset name.
      Name = null;

      // Reset bone transforms.
      for (int i = 0; i < BoneTransforms.Length; i++)
        BoneTransforms[i] = SrtTransform.Identity;

      // Recycle self.
      Skeleton.SkeletonPosePool.Recycle(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="SkeletonPose"/> that is a clone (deep copy) of the current 
    /// instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="SkeletonPose"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    public SkeletonPose Clone()
    {
      var clone = Create(Skeleton);
      SkeletonHelper.Copy(this, clone);
      return clone;
    }


    /// <summary>
    /// Makes sure that the <see cref="AnimatableBoneTransform"/>s are initialized.
    /// </summary>
    private void EnsureAnimatableProperties()
    {
      if (_animatableBoneTransforms == null)
      {
        var animatableBoneTransforms = Skeleton.AnimatableBoneTransformsPool.Obtain();
        foreach (var animatableBoneTransform in animatableBoneTransforms)
          animatableBoneTransform.SkeletonPose = this;

        Interlocked.CompareExchange(ref _animatableBoneTransforms, animatableBoneTransforms, null);
      }
    }


    /// <summary>
    /// Makes sure that the <see cref="SkeletonBoneAccessor"/> is initialized.
    /// </summary>
    private void EnsureBoneAccessor()
    {
      if (_boneAccessor == null)
        Interlocked.CompareExchange(ref _boneAccessor, SkeletonBoneAccessor.Create(this), null);
    }


    /// <summary>
    /// Gets the relative bone pose of the specified bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <returns>
    /// The bone pose transformation of the specified bone relative to the parent bone space.
    /// </returns>
    public SrtTransform GetBonePoseRelative(int boneIndex)
    {
      EnsureBoneAccessor();
      return _boneAccessor.GetBonePoseRelative(boneIndex);
    }


    /// <summary>
    /// Gets the absolute bone pose of the specified bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <returns>
    /// The bone pose transformation of the specified bone relative to model space.
    /// </returns>
    public SrtTransform GetBonePoseAbsolute(int boneIndex)
    {
      EnsureBoneAccessor();
      return _boneAccessor.GetBonePoseAbsolute(boneIndex);
    }


    /// <summary>
    /// Gets the bone transform of the specified bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <returns>
    /// The bone transform.
    /// </returns>
    public SrtTransform GetBoneTransform(int boneIndex)
    {
      return BoneTransforms[boneIndex];
    }


    /// <summary>
    /// Sets the bone transform of the specified bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <param name="boneTransform">The bone transform.</param>
    public void SetBoneTransform(int boneIndex, SrtTransform boneTransform)
    {
      BoneTransforms[boneIndex] = boneTransform;
      Invalidate(boneIndex);
    }


    /// <overloads>
    /// <summary>
    /// Resets bone transforms.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Resets the bone transforms of all bones in the skeleton.
    /// </summary>
    /// <remarks>
    /// If a bone transform is reset, it is set to the <see cref="SrtTransform.Identity"/>
    /// transform. If all bone transforms of a skeleton are reset, then the skeleton is in its
    /// bind pose.
    /// </remarks>
    public void ResetBoneTransforms()
    {
      // Set bone transforms to identity using loop.
      for (int i = 0; i < BoneTransforms.Length; i++)
        BoneTransforms[i] = SrtTransform.Identity;

      Invalidate();
    }


    /// <summary>
    /// Resets the bone transforms of the specified bone.
    /// </summary>
    /// <param name="boneIndex">The bone index.</param>
    /// <remarks>
    /// If a bone transform is reset, it is set to the <see cref="SrtTransform.Identity"/>
    /// transform. If all bone transforms of a skeleton are reset, then the skeleton is in its
    /// bind pose.
    /// </remarks>
    public void ResetBoneTransform(int boneIndex)
    {
      BoneTransforms[boneIndex] = SrtTransform.Identity;
      Invalidate(boneIndex);
    }


    /// <summary>
    /// Invalidates all cached information.
    /// </summary>
    internal void Invalidate()
    {
      if (_boneAccessor != null)
        _boneAccessor.Invalidate();
    }


    /// <summary>
    /// Invalidates cached information for the specified bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    internal void Invalidate(int boneIndex)      // Internal because it is used in AnimatableBoneTransform.
    {
      if (_boneAccessor != null)
        _boneAccessor.Invalidate(boneIndex);
    }


    /// <summary>
    /// Updates all bone transformations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method recomputes all bone transformations (the relative/absolute bone poses and 
    /// skinning matrices). It is generally not necessary to call this method explicitly because the 
    /// transformations are recomputed automatically in <see cref="GetBonePoseRelative"/> and 
    /// <see cref="GetBonePoseAbsolute"/> if they are invalid. 
    /// </para>
    /// <para>
    /// In certain cases in can be helpful to call <see cref="Update"/> explicitly: The method 
    /// forces the skeleton pose to update the transformations immediately. This is helpful to 
    /// perform all the computations at once and to avoid any stalls when calling 
    /// <see cref="GetBonePoseRelative"/> or <see cref="GetBonePoseAbsolute"/>.
    /// </para>
    /// </remarks>
    public void Update()
    {
      EnsureBoneAccessor();
      _boneAccessor.Update();
    }
    #endregion


    //--------------------------------------------------------------
    #region IAnimatableObject
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IEnumerable<IAnimatableProperty> IAnimatableObject.GetAnimatedProperties()
    {
      if (_animatableBoneTransforms != null)
      {
        foreach (var property in _animatableBoneTransforms)
        {
          if (((IAnimatableProperty)property).IsAnimated)
            yield return property;
        }
      }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IAnimatableProperty<T> IAnimatableObject.GetAnimatableProperty<T>(string name)
    {
      var index = Skeleton.GetIndex(name);
      if (index >= 0)
      {
        EnsureAnimatableProperties();
        return _animatableBoneTransforms[index] as IAnimatableProperty<T>;
      }

      return null;
    }
    #endregion


    //--------------------------------------------------------------
    #region IAnimatableProperty<SkeletonPose>
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IAnimatableProperty.HasBaseValue
    {
      get { return false; }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    SkeletonPose IAnimatableProperty<SkeletonPose>.BaseValue
    {
      get
      {
        throw new NotImplementedException(
          "The current IAnimatableProperty<SkeletonPose> does not have a BaseValue. "
          + "Check HasBaseValue before accessing BaseValue!");
      }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    SkeletonPose IAnimatableProperty<SkeletonPose>.AnimationValue
    {
      get { return this; }
      set
      {
        throw new InvalidOperationException(
          "The setter of IAnimatableProperty<SkeletonPose>.AnimationValue should never be called");
      }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    object IAnimatableProperty.BaseValue
    {
      get
      {
        throw new NotImplementedException(
          "The current IAnimatableProperty<SkeletonPose> does not have a BaseValue. "
          + "Check HasBaseValue before accessing BaseValue!");
      }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IAnimatableProperty.IsAnimated { get; set; }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    object IAnimatableProperty.AnimationValue { get { return this; } }
    #endregion
  }
}
