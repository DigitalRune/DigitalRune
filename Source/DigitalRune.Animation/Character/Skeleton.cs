// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if !PORTABLE
using System.ComponentModel;
#endif
#if PORTABLE || WINDOWS
using System.Dynamic;
#endif


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Describes a skeleton for 3D character animation in the bind pose.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="Skeleton"/> describes the hierarchy of bones in the "bind pose" (also called
  /// "rest pose"). Skeletons are immutable: After an instance is created, it is not possible to 
  /// add/remove bones or change the bind pose.
  /// </para>
  /// <para>
  /// The class <see cref="SkeletonPose"/> can be used to animate a skeleton: It defines a new pose 
  /// for an existing skeleton. A single <see cref="Skeleton"/> instance can be shared by multiple 
  /// <see cref="SkeletonPose"/>s. I.e. if multiple characters with the same skeleton are animated
  /// they share the same <see cref="Skeleton"/> instance, but each character has a different 
  /// <see cref="SkeletonPose"/>.
  /// </para>
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public class Skeleton : INamedObject
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // A node in the skeleton tree.
    internal struct Bone
    {
      public int Parent;      // Parent index.
      public int[] Children;  // Child indices.
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    internal readonly ResourcePool<SkeletonPose> SkeletonPosePool;
    internal readonly ResourcePool<SkeletonBoneAccessor> SkeletonBoneAccessorPool;
    internal readonly ResourcePool<AnimatableBoneTransform[]> AnimatableBoneTransformsPool;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    // The bone nodes describing the parent-child relationships.
    internal Bone[] Bones { get; set; }

    // Mapping of bone names to indices.
    internal Dictionary<string, int> BoneNames { get; set; }

    // One SrtTransform for each bone describing the bind pose relative to the parent bone.
    internal SrtTransform[] BindPosesRelative { get; set; }

    // One SrtTransform for each bone describing the bind pose relative to model space.
    internal SrtTransform[] BindPosesAbsoluteInverse { get; set; }


    /// <summary>
    /// Gets or sets the name of the skeleton.
    /// </summary>
    /// <value>The name. The default is <see langword="null"/>.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets the number of bones in this skeleton.
    /// </summary>
    /// <value>The number of bones.</value>
    public int NumberOfBones { get { return Bones.Length; } }


#if PORTABLE || WINDOWS
    /// <exclude/>
#if !PORTABLE
    [Browsable(false)]
#endif
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public /*dynamic*/ object Internals
    {
      // Make internals visible to assemblies that cannot be added with InternalsVisibleTo().
      // (Workaround only necessary because open source libraries are not strong-name signed.)
      get
      {
        int numberOfBones = Bones.Length;
        int[] boneParents = new int[numberOfBones];
        for (int i = 0; i < numberOfBones; i++)
          boneParents[i] = Bones[i].Parent;

        // ----- PCL Profile136 does not support dynamic.
        //dynamic internals = new ExpandoObject();
        //internals.BoneParents = boneParents;
        //internals.BindPosesRelative = BindPosesRelative;
        //return internals;

        IDictionary<string, object> internals = new ExpandoObject();
        internals["BoneParents"] = boneParents;
        internals["BindPosesRelative"] = BindPosesRelative;
        return internals;
      }
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Skeleton"/> class.
    /// </summary>
    /// <param name="boneParents">
    /// The bone parents. This list contains one entry per bone. The list element is the 
    /// parent bone index for each bone. If a bone has no parent, the array should contain -1.
    /// </param>
    /// <param name="boneNames">
    /// The bone names. This list contains one entry per bone. The list element is the name
    /// of the bone or <see langword="null"/> if the bone is unnamed.
    /// </param>
    /// <param name="bindPosesRelative">
    /// The bind poses. This list contains one entry per bone. The list element is the bone
    /// pose transformation relative to the parent bone.
    /// </param>
    /// <remarks>
    /// The bone data must be specified in lists. The index in the list is the bone index. The
    /// bones must be sorted so that parent bones come before their child bones.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="boneParents"/>, <paramref name="boneNames"/> or 
    /// <paramref name="bindPosesRelative"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Either the given lists are empty, have different length, or the 
    /// <paramref name="boneParents"/> are invalid (parent bones must come be before their child 
    /// bones).
    /// </exception>
    public Skeleton(IList<int> boneParents, IList<string> boneNames, IList<SrtTransform> bindPosesRelative)
    {
      Initialize(boneParents, boneNames, bindPosesRelative);

      // If skeleton has been initialized successfully, create resource pools 
      // used by SkeletonPoses.
      SkeletonPosePool = new ResourcePool<SkeletonPose>(
        () => new SkeletonPose(this),
        null,
        null);

      SkeletonBoneAccessorPool = new ResourcePool<SkeletonBoneAccessor>(
        () => new SkeletonBoneAccessor(),
        null,
        null);

      AnimatableBoneTransformsPool = new ResourcePool<AnimatableBoneTransform[]>(
        () =>
        {
          var animatableBoneTransforms = new AnimatableBoneTransform[NumberOfBones];
          for (int i = 0; i < animatableBoneTransforms.Length; i++)
            animatableBoneTransforms[i] = new AnimatableBoneTransform(i);

          return animatableBoneTransforms;
        },
        null,
        null);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes the skeleton.
    /// </summary>
    /// <param name="boneParents">The bone parents.</param>
    /// <param name="boneNames">The bone names.</param>
    /// <param name="bindPosesRelative">The bind poses.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="boneParents"/>, <paramref name="boneNames"/> or 
    /// <paramref name="bindPosesRelative"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Either the given lists are empty, have different length, or the 
    /// <paramref name="boneParents"/> are invalid (parent bones must come be before their child 
    /// bones).
    /// </exception>
    internal void Initialize(IList<int> boneParents, IList<string> boneNames, IList<SrtTransform> bindPosesRelative)
    {
      if (boneParents == null)
        throw new ArgumentNullException("boneParents");
      if (boneNames == null)
        throw new ArgumentNullException("boneNames");
      if (bindPosesRelative == null)
        throw new ArgumentNullException("bindPosesRelative");

      var numberOfBones = boneParents.Count;

      if (numberOfBones == 0)
        throw new ArgumentException("boneParents list must not be empty.");
      if (boneNames.Count == 0)
        throw new ArgumentException("boneNames list must not be empty.");
      if (bindPosesRelative.Count == 0)
        throw new ArgumentException("bindPosesRelative list must not be empty.");
      if (numberOfBones != boneNames.Count || numberOfBones != bindPosesRelative.Count)
        throw new ArgumentException("The lists must have the same number of elements.");

      // Check if bone parents come before children. This ordering also forbids cycles.
      for (int index = 0; index < numberOfBones; index++)
      {
        var parentIndex = boneParents[index];
        if (parentIndex >= index)
          throw new ArgumentException("Invalid boneParents list. Parent bones must have a lower index than child bones.");
      }

      // Build list of bone nodes.
      Bones = new Bone[numberOfBones];
      var children = new List<int>();
      for (int index = 0; index < numberOfBones; index++)
      {
        Bone bone = new Bone();

        // Set parent index.
        int parentIndex = boneParents[index];
        if (parentIndex < -1)
          parentIndex = -1;

        bone.Parent = parentIndex;

        // Create array of child indices.
        children.Clear();
        for (int childIndex = index + 1; childIndex < numberOfBones; childIndex++)
          if (boneParents[childIndex] == index)
            children.Add(childIndex);

        if (children.Count > 0)
          bone.Children = children.ToArray();

        Bones[index] = bone;
      }

      // Initialize bone name/index dictionary.
      if (BoneNames == null)
        BoneNames = new Dictionary<string, int>(numberOfBones);
      else
        BoneNames.Clear();

      for (int index = 0; index < numberOfBones; index++)
      {
        var name = boneNames[index];
        if (name != null)
          BoneNames[name] = index;
      }

      // Copy relative bind poses.
      if (BindPosesRelative == null)
        BindPosesRelative = new SrtTransform[numberOfBones];

      for (int index = 0; index < numberOfBones; index++)
        BindPosesRelative[index] = bindPosesRelative[index];

      // Initialize absolute bind poses (inverse).
      if (BindPosesAbsoluteInverse == null)
        BindPosesAbsoluteInverse = new SrtTransform[numberOfBones];

      // First store the non-inverted BindTransforms in model space.
      BindPosesAbsoluteInverse[0] = BindPosesRelative[0];
      for (int index = 1; index < numberOfBones; index++)
      {
        var parentIndex = Bones[index].Parent;

        //BindPosesAbsoluteInverse[index] = BindPosesAbsoluteInverse[parentIndex] * BindPosesRelative[index];
        SrtTransform.Multiply(ref BindPosesAbsoluteInverse[parentIndex], ref BindPosesRelative[index], out BindPosesAbsoluteInverse[index]);
      }

      // Invert matrices.
      for (int index = 0; index < numberOfBones; index++)
        BindPosesAbsoluteInverse[index].Invert();
    }


    /// <summary>
    /// Gets the index of the parent bone of a given bone.
    /// </summary>
    /// <param name="boneIndex">The bone index.</param>
    /// <returns>
    /// The bone index of the parent bone, or -1 if the bone is a root bone that does not have
    /// a parent bone.
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="boneIndex"/> is out of range.
    /// </exception>
    public int GetParent(int boneIndex)
    {
      return Bones[boneIndex].Parent;
    }


    /// <summary>
    /// Gets the number of child bones of a given bone.
    /// </summary>
    /// <param name="boneIndex">The bone index.</param>
    /// <returns>
    /// The number of child bones that are attached to this bone. (Only direct child bones - not 
    /// children of children.)
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="boneIndex"/> is out of range.
    /// </exception>
    public int GetNumberOfChildren(int boneIndex)
    {
      var bone = Bones[boneIndex];
      return (bone.Children != null) ? bone.Children.Length : 0;
    }


    /// <summary>
    /// Gets the bone index of a child bone of a given bone.
    /// </summary>
    /// <param name="boneIndex">The bone index.</param>
    /// <param name="childIndex">
    /// The child index. 0 is the first child bone, 1 is the second child bone, and so on.
    /// </param>
    /// <returns>
    /// The bone index of the child bone.
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="boneIndex"/> or <paramref name="childIndex"/> is out of range.
    /// </exception>
    public int GetChild(int boneIndex, int childIndex)
    {
      var bone = Bones[boneIndex];

      if (childIndex < 0 || bone.Children == null || childIndex >= bone.Children.Length)
        throw new IndexOutOfRangeException("childIndex");

      return bone.Children[childIndex];
    }


    //public void SetIndex(string boneName, int boneIndex)
    //{
    //  BoneNames[boneName] = boneIndex;
    //}


    /// <summary>
    /// Gets the bone index for a given bone name.
    /// </summary>
    /// <param name="boneName">The name of the bone.</param>
    /// <returns>
    /// The bone index, or -1 if no bone with this name exists.
    /// </returns>
    public int GetIndex(string boneName)
    {
      int boneIndex;
      if (!BoneNames.TryGetValue(boneName, out boneIndex))
      {
        return -1;
      }

      return boneIndex;
    }


    /// <summary>
    /// Gets the name of a given bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <returns>
    /// The name of the bone, or <see langword="null"/> if the bone is unnamed.
    /// </returns>
    public string GetName(int boneIndex)
    {
      foreach (var item in BoneNames)
      {
        if (item.Value == boneIndex)
          return item.Key;
      }

      return null;
    }


    //public void SetName(int boneIndex, string boneName)
    //{
    //  var oldName = GetName(boneIndex);
    //  if (oldName != null)
    //    BoneNames.Remove(oldName);

    //  BoneNames[boneName] = boneIndex;
    //}


    /// <summary>
    /// Gets the bind pose transformation of a given bone relative to the parent bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <returns>
    /// The bind pose transformation relative to the parent bone.
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="boneIndex"/> is out of range.
    /// </exception>
    public SrtTransform GetBindPoseRelative(int boneIndex)
    {
      return BindPosesRelative[boneIndex];
    }


    /// <summary>
    /// Gets the bind pose transformation of a given bone relative to model space.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <returns>
    /// The bind pose transformation relative to model space.
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="boneIndex"/> is out of range.
    /// </exception>
    public SrtTransform GetBindPoseAbsoluteInverse(int boneIndex)
    {
      return BindPosesAbsoluteInverse[boneIndex];
    }
    #endregion
  }
}
