// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Threading;
using DigitalRune.Collections;
using DigitalRune.Mathematics.Algebra;

#if XNA || MONOGAME
using Microsoft.Xna.Framework;
#endif


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Handles efficient access to derived bone transformations.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class computes transformations that are derived from the bind pose information and the
  /// current (animated) bone transforms: The bone pose transformations relative to parent bone
  /// space, the bone pose transformation relative to model space and the skinning matrices.
  /// </para>
  /// <para>
  /// Dirty flags are managed for the bones. Whenever a bone transform of the 
  /// <see cref="SkeletonPose"/> is modified, <see cref="Invalidate(int)"/> must be called to set 
  /// the dirty flag. When bone pose transformations are accessed, they are recomputed if necessary.
  /// </para>
  /// <para>
  /// Call <see cref="Update"/> to compute all derived bone transformation at once.
  /// </para>
  /// </remarks>
  internal sealed class SkeletonBoneAccessor
  {
    // Thread-safety:
    // The AnimationManager may read bone transforms from multiple threads simultaneously.
    // Locking ensures that this memory access in DigitalRune Animation is safe.
    // But the SkeletonBoneAccessor does not implement a full reader-writer lock!
    // The following can occur in custom multithreaded applications:
    // - Thread A enters GetBonePoseAbsolute() GetBonePoseRelative() and starts
    //   reading the SrtTransform.
    // - Thread B invalidates the bone and immediately updates the SrtTransform.
    // - Thread A reads an invalid SrtTransform.
    // To improve thread-safety and simplify code we could use a ReaderWriterLockSlim
    // instead of partial lock in vNext.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private SkeletonPose _skeletonPose;

    // BitArrays and _isDirty flag:
    // Unfortunately the BitArray does not have a method to check whether any bit
    // is set. Therefore, we use a second flag.
    private bool _isDirty;

    // Bone poses relative to the parents + IsDirty flags.
    private SrtTransform[] _bonePoseRelative;
    private FastBitArray _isBonePoseRelativeDirty;

    // Bone poses relative to the model space + IsDirty flags.
    private SrtTransform[] _bonePoseAbsolute;
    private FastBitArray _isBonePoseAbsoluteDirty;

    private volatile Matrix44F[] _skinningMatrices;
#if XNA || MONOGAME
    private volatile Matrix[] _skinningMatricesXna;
#endif
    private FastBitArray _isSkinningMatrixDirty;

    // Object to lock when updating stuff in reads.
    private readonly object _syncRoot = new object();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    // Array must not be modified by user!
    public Matrix44F[] SkinningMatrices
    {
      get
      {
        if (_skinningMatrices == null)
        {
          lock (_syncRoot)
          {
            if (_skinningMatrices == null)
            {
              _isDirty = true;
              _isSkinningMatrixDirty.SetAll(true);
              _skinningMatrices = new Matrix44F[_bonePoseRelative.Length];
            }
          }
        }

        Update();
        return _skinningMatrices;
      }
    }


#if XNA || MONOGAME
    // Array must not be modified by user!
    public Matrix[] SkinningMatricesXna
    {
      get
      {
        if (_skinningMatricesXna == null)
        {
          lock (_syncRoot)
          {
            if (_skinningMatricesXna == null)
            {
              _isDirty = true;
              _isSkinningMatrixDirty.SetAll(true);
              _skinningMatricesXna = new Matrix[_bonePoseRelative.Length];
            }
          }
        }

        Update();
        return _skinningMatricesXna;
      }
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SkeletonBoneAccessor"/> class.
    /// </summary>
    internal SkeletonBoneAccessor()
    {
      // The constructor is only used by the resource pool.
      // The SkeletonBoneAccessor is initialized in Create().
    }


    /// <summary>
    /// Creates an instance of the <see cref="SkeletonBoneAccessor"/> class. (This method 
    /// reuses a previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <returns>
    /// A new or reusable instance of the <see cref="SkeletonBoneAccessor"/> class.
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
    /// <paramref name="skeletonPose"/> is <see langword="null"/>.
    /// </exception>
    public static SkeletonBoneAccessor Create(SkeletonPose skeletonPose)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      var skeletonBoneAccessor = skeletonPose.Skeleton.SkeletonBoneAccessorPool.Obtain();
      skeletonBoneAccessor.Initialize(skeletonPose);
      return skeletonBoneAccessor;
    }


    private void Initialize(SkeletonPose skeletonPose)
    {
      _skeletonPose = skeletonPose;

      if (_bonePoseRelative == null)
      {
        // The SkeletonPoseAccessor is initialized for the first time.
        int numberOfBones = skeletonPose.Skeleton.NumberOfBones;

        // Create arrays. (Note: SkinningMatrices are created on demand.)
        _bonePoseRelative = new SrtTransform[numberOfBones];
        _bonePoseAbsolute = new SrtTransform[numberOfBones];

        // Set all dirty flags.
        _isDirty = true;
        _isBonePoseRelativeDirty = new FastBitArray(numberOfBones);
        _isBonePoseRelativeDirty.SetAll(true);
        _isBonePoseAbsoluteDirty = new FastBitArray(numberOfBones);
        _isBonePoseAbsoluteDirty.SetAll(true);
        _isSkinningMatrixDirty = new FastBitArray(numberOfBones);
        _isSkinningMatrixDirty.SetAll(true);
      }
      else
      {
        Debug.Assert(_bonePoseRelative != null, "SkeletonBoneAccessor is not properly initialized. Array _bonePoseRelative is not set.");
        Debug.Assert(_bonePoseAbsolute != null, "SkeletonBoneAccessor is not properly initialized. Array _bonePoseAbsolute is not set.");
        Debug.Assert(_isBonePoseRelativeDirty != null, "SkeletonBoneAccessor is not properly initialized. BitArray _isBonePoseRelativeDirty is not set.");
        Debug.Assert(_isBonePoseAbsoluteDirty != null, "SkeletonBoneAccessor is not properly initialized. BitArray _isBonePoseAbsoluteDirty is not set.");
        Debug.Assert(_isSkinningMatrixDirty != null, "SkeletonBoneAccessor is not properly initialized. BitArray _isSkinningMatrixDirty is not set.");
        Debug.Assert(_bonePoseRelative.Length != skeletonPose.Skeleton.NumberOfBones, "SkeletonBoneAccessor is incompatible. Array _bonePoseRelative has wrong length.");
        Debug.Assert(_bonePoseAbsolute.Length != skeletonPose.Skeleton.NumberOfBones, "SkeletonBoneAccessor is incompatible. Array _bonePoseAbsolute has wrong length.");
        Debug.Assert(_isBonePoseRelativeDirty.Length != skeletonPose.Skeleton.NumberOfBones, "SkeletonBoneAccessor is incompatible. BitArray _isBonePoseRelativeDirty has wrong length.");
        Debug.Assert(_isBonePoseAbsoluteDirty.Length != skeletonPose.Skeleton.NumberOfBones, "SkeletonBoneAccessor is incompatible. BitArray _isBonePoseAbsoluteDirty has wrong length.");
        Debug.Assert(_isSkinningMatrixDirty.Length != skeletonPose.Skeleton.NumberOfBones, "SkeletonBoneAccessor is incompatible. BitArray _isSkinningMatrixDirty has wrong length.");

        // Set all dirty flags.
        Invalidate();
      }
    }


    /// <summary>
    /// Recycles this instance of the <see cref="SkeletonBoneAccessor"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    public void Recycle()
    {
      var pool = _skeletonPose.Skeleton.SkeletonBoneAccessorPool;
      _skeletonPose = null;
      pool.Recycle(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the relative bone pose of the specified bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <returns>
    /// The bone pose transformation of the specified bone relative to the parent bone space.
    /// </returns>
    public SrtTransform GetBonePoseRelative(int boneIndex)
    {
      UpdateBonePoseRelative(boneIndex);
      return _bonePoseRelative[boneIndex];
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
      UpdateBonePoseAbsolute(boneIndex);
      return _bonePoseAbsolute[boneIndex];
    }


    /// <summary>
    /// Invalidates all cached information.
    /// </summary>
    public void Invalidate()
    {
      _isDirty = true;
      _isBonePoseRelativeDirty.SetAll(true);
      _isBonePoseAbsoluteDirty.SetAll(true);
      _isSkinningMatrixDirty.SetAll(true);
    }


    /// <summary>
    /// Invalidates cached information for the specified bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    internal void Invalidate(int boneIndex)
    {
      _isDirty = true;
      _isBonePoseRelativeDirty[boneIndex] = true;
      InvalidateAbsoluteMatrices(boneIndex);
    }


    /// <summary>
    /// Recursively invalidates the specified bone and its descendants.
    /// </summary>
    /// <param name="boneIndex">Index of the bone.</param>
    private void InvalidateAbsoluteMatrices(int boneIndex)
    {
      // Abort if already marked as dirty?
      if (_isBonePoseAbsoluteDirty[boneIndex] && _isSkinningMatrixDirty[boneIndex])
        return;

      _isBonePoseAbsoluteDirty[boneIndex] = true;
      _isSkinningMatrixDirty[boneIndex] = true;

      // Invalidate children.
      var skeleton = _skeletonPose.Skeleton;
      int numberOfChildren = skeleton.GetNumberOfChildren(boneIndex);
      for (int i = 0; i < numberOfChildren; i++)
        InvalidateAbsoluteMatrices(skeleton.GetChild(boneIndex, i));
    }


    /// <summary>
    /// Updates all bone transformations.
    /// </summary>
    public void Update()
    {
      // This method updates does not check the individual dirty flags. If something is
      // dirty all transforms are updated. Checking the dirty flags makes the average 
      // case (whole skeleton is animated) a lot slower.
      if (_isDirty)
      {
        lock (_syncRoot)
        {
          if (_isDirty)
          {
            var skeleton = _skeletonPose.Skeleton;
            int numberOfBones = skeleton.NumberOfBones;

            // ----- Update dirty relative bone poses.
            var boneTransforms = _skeletonPose.BoneTransforms;

            var bindPosesRelative = skeleton.BindPosesRelative;

            for (int i = 0; i < numberOfBones; i++)
            {
              //_bonePoseRelative[i] = bindPosesRelative[i] * boneTransforms[i];
              SrtTransform.Multiply(ref bindPosesRelative[i], ref boneTransforms[i], out _bonePoseRelative[i]);
            }

            // ----- Update dirty absolute bone poses.
            _bonePoseAbsolute[0] = _bonePoseRelative[0];
            for (int i = 1; i < numberOfBones; i++)
            {
              int parentIndex = skeleton.GetParent(i);

              //_bonePoseAbsolute[i] = _bonePoseAbsolute[parentIndex] * _bonePoseRelative[i];
              SrtTransform.Multiply(ref _bonePoseAbsolute[parentIndex], ref _bonePoseRelative[i],
                                    out _bonePoseAbsolute[i]);
            }

            // ----- Update skinning matrices (either the Matrix44F or the XNA variant).
            if (_skinningMatrices != null)
            {
              for (int i = 0; i < numberOfBones; i++)
              {
                //_skinningMatrices[i] = _bonePoseAbsolute[i] * skeleton.BindPosesAbsoluteInverse[i];
                SrtTransform.Multiply(ref _bonePoseAbsolute[i], ref skeleton.BindPosesAbsoluteInverse[i],
                                      out _skinningMatrices[i]);
              }
            }

#if XNA || MONOGAME
            if (_skinningMatricesXna != null)
            {
              if (_skinningMatrices != null)
              {
                for (int i = 0; i < numberOfBones; i++)
                  _skinningMatricesXna[i] = (Matrix)_skinningMatrices[i];
              }
              else
              {
                for (int i = 0; i < numberOfBones; i++)
                {
                  //_skinningMatricesXna[i] = _bonePoseAbsolute[i] * skeleton.BindPosesAbsoluteInverse[i];
                  SrtTransform.Multiply(ref _bonePoseAbsolute[i], ref skeleton.BindPosesAbsoluteInverse[i],
                                        out _skinningMatricesXna[i]);
                }
              }
            }
#endif

#if !NETFX_CORE && !NET45
            Thread.MemoryBarrier();
#else
            Interlocked.MemoryBarrier();
#endif

            _isBonePoseRelativeDirty.SetAll(false);
            _isBonePoseAbsoluteDirty.SetAll(false);
            _isSkinningMatrixDirty.SetAll(false);
            _isDirty = false;
          }
        }
      }
    }


    /// <summary>
    /// Updates the relative bone pose for the specified bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    private void UpdateBonePoseRelative(int boneIndex)
    {
      if (_isBonePoseRelativeDirty[boneIndex])
      {
        lock (_syncRoot)
        {
          if (_isBonePoseRelativeDirty[boneIndex])
          {
            // _bonePoseRelative[boneIndex] = _skeletonPose.Skeleton.BindPosesRelative[boneIndex] * _skeletonPose.BoneTransforms[boneIndex];
            SrtTransform.Multiply(ref _skeletonPose.Skeleton.BindPosesRelative[boneIndex],
                                  ref _skeletonPose.BoneTransforms[boneIndex], out _bonePoseRelative[boneIndex]);

#if !NETFX_CORE && !NET45
            Thread.MemoryBarrier();
#else
            Interlocked.MemoryBarrier();
#endif

            _isBonePoseRelativeDirty[boneIndex] = false;
          }
        }
      }
    }


    /// <summary>
    /// Updates the absolute bone pose for the specified bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    private void UpdateBonePoseAbsolute(int boneIndex)
    {
      if (_isBonePoseAbsoluteDirty[boneIndex])
      {
        lock (_syncRoot)
        {
          if (_isBonePoseAbsoluteDirty[boneIndex])
          {
            // Make sure relative bone pose is up-to-date.
            UpdateBonePoseRelative(boneIndex);

            int parentIndex = _skeletonPose.Skeleton.GetParent(boneIndex);
            if (parentIndex < 0)
            {
              // No parent.
              _bonePoseAbsolute[boneIndex] = _bonePoseRelative[boneIndex];
            }
            else
            {
              // Make sure parent is up-to-date. (Recursively update ancestors.)
              UpdateBonePoseAbsolute(parentIndex);

              //_bonePoseAbsolute[boneIndex] = _bonePoseAbsolute[parentIndex] * _bonePoseRelative[boneIndex];
              SrtTransform.Multiply(ref _bonePoseAbsolute[parentIndex], ref _bonePoseRelative[boneIndex],
                                    out _bonePoseAbsolute[boneIndex]);
            }

#if !NETFX_CORE && !NET45
            Thread.MemoryBarrier();
#else
            Interlocked.MemoryBarrier();
#endif

            _isBonePoseAbsoluteDirty[boneIndex] = false;
          }
        }
      }
    }
    #endregion
  }
}
