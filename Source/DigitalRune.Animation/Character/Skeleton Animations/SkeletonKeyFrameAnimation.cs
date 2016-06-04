// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Animation.Traits;
using DigitalRune.Mathematics.Algebra;
#if !PORTABLE
using System.ComponentModel;
#endif
#if PORTABLE || WINDOWS
using System.Dynamic;
#endif


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Animates a <see cref="SkeletonPose"/> based on predefined key frames.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="SkeletonKeyFrameAnimation"/> is an optimized <see cref="KeyFrameAnimation{T}"/>
  /// for character animation. A <see cref="SkeletonKeyFrameAnimation"/> instance animates the bone 
  /// transforms of a <see cref="SkeletonPose"/>. It can animate a single bone (e.g. only the head),
  /// several bones (e.g. an upper body animation) or all bones of skeleton at once.
  /// </para>
  /// <para>
  /// A <see cref="SkeletonKeyFrameAnimation"/> is defined by adding key frames calling the method
  /// <see cref="AddKeyFrame"/> for the animated bones. The animation controls a bone if at least
  /// one key frame for the bone has been added (see also <see cref="IsBoneAnimated"/>). After
  /// adding all key frames, <see cref="Freeze"/> must be called. <see cref="Freeze"/> optimizes the
  /// animation data for fast access at runtime.
  /// </para>
  /// <para>
  /// When key frames are added after calling <see cref="Freeze"/>, the animation is automatically
  /// reset into an editable state. All internal optimizations will be discarded!
  /// <see cref="Freeze"/> needs to be called again after all key frames are added.
  /// </para>
  /// <para>
  /// <strong>Bone Channel Weights:</strong><br/>
  /// A weight can be set for each animated bone (see <see cref="SetWeight"/>). If the weight is 0,
  /// the bone is not animated. If the weight is 1, the bone is animated and replaces all preceding 
  /// animations in the animation composition chain. If the weight is less than 1, the bone 
  /// animation is blended with preceding animations.
  /// </para>
  /// <para>
  /// Weights can be modified before and after <see cref="Freeze"/>. For example, weights can be
  /// modified while the animation is running.
  /// </para>
  /// <para>
  /// <strong>Key Frame Interpolation:</strong><br/>
  /// A key frame animation contains a list of key frames that define the animation values at
  /// certain points in time. When the animation is played the class automatically looks up the
  /// animation value in the list of key frames. The property <see cref="EnableInterpolation"/>
  /// defines whether interpolation between key frames is enabled. When the property is set
  /// (default), the values between two key frames are interpolated using linear interpolation.
  /// </para>
  /// <para>
  /// <strong>Cyclic Animations:</strong><br/>
  /// A skeleton key frame animation, by default, runs until the last key frame is reached. The 
  /// types <see cref="TimelineClip"/> and <see cref="AnimationClip{T}"/> can be used to repeat the 
  /// entire key frame animation (or a certain clip) for a number of times using a certain 
  /// loop-behavior (see <see cref="TimelineClip.LoopBehavior"/>).
  /// </para>
  /// <para>
  /// The first and the last key frame need to be identical to achieve a smooth cyclic
  /// interpolation.
  /// </para>
  /// </remarks>
  public partial class SkeletonKeyFrameAnimation : IAnimation<SkeletonPose>
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    private class PreprocessingData
    {
      // A list of key frames per bone.
      //   bone index --> key frames
      public readonly Dictionary<int, List<BoneKeyFrameSRT>> Channels = new Dictionary<int, List<BoneKeyFrameSRT>>();

      // A weight for each bone.
      //   bone index --> weight
      public readonly Dictionary<int, float> Weights = new Dictionary<int, float>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // ----- Preprocessing data ("not yet frozen")
    private PreprocessingData _preprocessData;

    // ----- Runtime data ("frozen")

    // The total duration of the animation. Cached to make GetTotalDuration() faster.
    internal TimeSpan _totalDuration = TimeSpan.MinValue;

    // Acceleration structures to avoid binary search per channel:
    // A sorted list of all key frame times.
    //  time = _times[timeIndex]
    internal TimeSpan[] _times;

    // The sorted bone indices of the animated bones.
    //   boneIndex = _channels[channelIndex]
    internal int[] _channels;

    // Weights of the animated bones. Same order as in _channels.
    //   weight = _weight[channelIndex]
    internal float[] _weights;

    // The key frame indices for the _times. The first _channel.Length entries are
    // the key frame indices of the key frames where the time is less than or equal
    // to the first key frame time. Etc.
    // Length of array is: _channels.Length * _times.Length.
    //   keyFrameIndex = _indices[timeIndex * _channels.Length + channelIndex]
    // keyFrameIndex is the index of the key frame where time ≤ _times[timeIndex].
    internal int[] _indices;

    // The key frame type (R, RT, SRT, ...) of each channel.
    //   boneKeyFrameType = _keyFrameTypes[channelIndex]
    internal BoneKeyFrameType[] _keyFrameTypes;

    // One array of key frames for each channel.
    //   keyFrames = _keyFrames[channelIndex];
    //   keyFrame = keyFrames[keyFrameIndex]
    // keyFrames is either BoneKeyFrameR[], BoneKeyFrameRT[] or BoneKeyFrameSRT[].
    internal object[] _keyFrames;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether values between key frames are interpolated.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if interpolation of key frames is enabled; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    public bool EnableInterpolation { get; set; }


    /// <summary>
    /// Gets or sets a value that specifies how the animation behaves when it reaches the end of its
    /// duration.
    /// </summary>
    /// <value>
    /// A value that specifies how the animation behaves when it reaches the end of its duration.
    /// </value>
    public FillBehavior FillBehavior { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the output of the animation is added to the current
    /// value of the property that is being animated.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this animation is additive; otherwise, <see langword="false"/>.
    /// The default value is <see langword="false"/>.
    /// </value>
    public bool IsAdditive { get; set; }


    /// <summary>
    /// Gets a value indicating whether this animation is frozen.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this animation is frozen; otherwise, <see langword="false"/>.
    /// </value>
    /// <seealso cref="Freeze"/>
    public bool IsFrozen
    {
      get { return _channels != null; }
    }


    /// <summary>
    /// Gets or sets the property to which the animation is applied by default.
    /// </summary>
    /// <value>The property to which the animation is applied by default.</value>
    /// <inheritdoc cref="ITimeline.TargetObject"/>
    public string TargetObject { get; set; }


    /// <summary>
    /// Gets or sets the property to which the animation is applied by default.
    /// </summary>
    /// <value>The property to which the animation is applied by default.</value>
    /// <inheritdoc cref="IAnimation.TargetProperty"/>
    public string TargetProperty { get; set; }


    /// <inheritdoc/>
    public IAnimationValueTraits<SkeletonPose> Traits
    {
      get { return SkeletonPoseTraits.Instance; }
    }


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
        // BoneKeyFrameType, BoneKeyFrameR, BoneKeyFrameRT, BoneKeyFrameSRT are internal.
        // --> Use int, TimeSpan and SrtTransform instead.
        int numberOfChannels = _channels.Length;
        int[] keyFrameTypes = new int[numberOfChannels];
        TimeSpan[][] keyFrameTimes = new TimeSpan[numberOfChannels][];
        SrtTransform[][] keyFrameTransforms = new SrtTransform[numberOfChannels][];
        for (int channelIndex = 0; channelIndex < numberOfChannels; channelIndex++)
        {
          keyFrameTypes[channelIndex] = (int)_keyFrameTypes[channelIndex];

          int numberOfKeyFrames = ((Array)_keyFrames[channelIndex]).Length;
          keyFrameTimes[channelIndex] = new TimeSpan[numberOfKeyFrames];
          keyFrameTransforms[channelIndex] = new SrtTransform[numberOfKeyFrames];
          for (int keyFrameIndex = 0; keyFrameIndex < numberOfKeyFrames; keyFrameIndex++)
          {
            TimeSpan time;
            SrtTransform transform;
            GetBoneKeyFrame(channelIndex, keyFrameIndex, out time, out transform);
            keyFrameTimes[channelIndex][keyFrameIndex] = time;
            keyFrameTransforms[channelIndex][keyFrameIndex] = transform;
          }
        }

        // ----- PCL Profile136 does not support dynamic.
        //dynamic internals = new ExpandoObject();
        //internals.Times = _times;
        //internals.Channels = _channels;
        //internals.Weights = _weights;
        //internals.Indices = _indices;
        //internals.KeyFrameTypes = keyFrameTypes;
        //internals.KeyFrameTimes = keyFrameTimes;
        //internals.KeyFrameTransforms = keyFrameTransforms;
        //return internals;

        IDictionary<string, object> internals = new ExpandoObject();
        internals["Times"] = _times;
        internals["Channels"] = _channels;
        internals["Weights"] = _weights;
        internals["Indices"] = _indices;
        internals["KeyFrameTypes"] = keyFrameTypes;
        internals["KeyFrameTimes"] = keyFrameTimes;
        internals["KeyFrameTransforms"] = keyFrameTransforms;
        return internals;
      }
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SkeletonKeyFrameAnimation"/> class.
    /// </summary>
    public SkeletonKeyFrameAnimation()
    {
      EnableInterpolation = true;
      FillBehavior = FillBehavior.Hold;
      IsAdditive = false;
      TargetObject = null;
      TargetProperty = null;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void EnsurePreprocessingData()
    {
      if (_preprocessData == null)
        _preprocessData = new PreprocessingData();
    }


    /// <summary>
    /// Adds a key frame for the specified bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <param name="time">The time of the key frame.</param>
    /// <param name="boneTransform">The bone transform.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="boneIndex"/> is out of range.
    /// </exception>
    public void AddKeyFrame(int boneIndex, TimeSpan time, SrtTransform boneTransform)
    {
      if (boneIndex < 0)
        throw new ArgumentOutOfRangeException("boneIndex", "boneIndex must not be negative.");

      Unfreeze();
      EnsurePreprocessingData();

      // Get channel with key frames.
      List<BoneKeyFrameSRT> channel;
      if (!_preprocessData.Channels.TryGetValue(boneIndex, out channel))
      {
        // Create a new key frame list.
        channel = new List<BoneKeyFrameSRT>();
        _preprocessData.Channels.Add(boneIndex, channel);
      }

      // Add key frame to bone channel.
      channel.Add(new BoneKeyFrameSRT { Time = time, Transform = boneTransform });
    }


    //public bool RemoveKeyFrame(int boneIndex, float time)
    //{
    //}


    /// <summary>
    /// Removes all key frames and bone weights.
    /// </summary>
    public void Clear()
    {
      Unfreeze();

      if (_preprocessData != null)
      {
        _preprocessData.Channels.Clear();
        _preprocessData.Weights.Clear();
      }
    }


    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>A copy of this key frame animation.</returns>
    public SkeletonKeyFrameAnimation Clone()
    {
      var clone = new SkeletonKeyFrameAnimation
      {
        EnableInterpolation = EnableInterpolation,
        FillBehavior = FillBehavior,
        IsAdditive = IsAdditive,
        TargetObject = TargetObject,
        TargetProperty = TargetProperty
      };

      if (IsFrozen)
      {
        // Already frozen.
        for (int i = 0; i < _channels.Length; i++)
        {
          clone._totalDuration = _totalDuration;
          clone._times = _times;
          clone._channels = _channels;
          clone._weights = new float[_weights.Length];
          Array.Copy(_weights, clone._weights, _weights.Length);
          clone._indices = _indices;
          clone._keyFrameTypes = _keyFrameTypes;
          clone._keyFrames = _keyFrames;
        }
      }
      else
      {
        // Not frozen.

        // Copy key frames.
        if (_preprocessData != null)
        {
          foreach (var item in _preprocessData.Channels)
          {
            int boneIndex = item.Key;
            var keyFrames = item.Value;
            foreach (var keyFrame in keyFrames)
              clone.AddKeyFrame(boneIndex, keyFrame.Time, keyFrame.Transform);
          }

          // Copy weights.
          foreach (var item in _preprocessData.Weights)
            clone.SetWeight(item.Key, item.Value);
        }
      }

      return clone;
    }


    /// <summary>
    /// Prepares this animation for runtime usage. (Must be called after all key frames have been
    /// added!)
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Freeze"/> must be called after all key frames have been added (see method
    /// <see cref="AddKeyFrame"/>) and before the animation can be used in the animation system.
    /// <see cref="Freeze"/> optimizes the internal data for fast access at runtime.
    /// </para>
    /// <para>
    /// When key frames are added or removed after calling <see cref="Freeze"/>, the animation is
    /// automatically reset into an editable state. All internal optimizations will be discarded!
    /// <see cref="Freeze"/> needs to be called again after all key frames are added/removed.
    /// </para>
    /// <para>
    /// Weights can be modified before and after <see cref="Freeze"/>. For example, weights can be
    /// modified while the animation is running.
    /// </para>
    /// </remarks>
    /// <seealso cref="IsFrozen"/>
    public void Freeze()
    {
      if (IsFrozen)
        return;

      // Desired memory layout to maximize read performance at runtime:
      //   _times
      //   _channels
      //   _weights
      //   _indices
      //   _keyFrameTypes
      //   _keyFrames

      // ----- Process channels and initialize array of key frame times (_times) and bone indices (_channels).
      var times = new List<TimeSpan>();
      var channels = new List<int>();
      if (_preprocessData != null)
      {
        foreach (var item in _preprocessData.Channels)
        {
          int boneIndex = item.Key;
          var keyFrames = item.Value;

          // Ignore empty channels.
          if (keyFrames == null || keyFrames.Count <= 0)
            continue;

          // Sort key frames.
          keyFrames.Sort(CompareKeyFrameTime);

          // Create a list of all key frame times.
          foreach (var keyFrame in keyFrames)
            times.Add(keyFrame.Time);

          // Update total duration.
          _totalDuration = AnimationHelper.Max(_totalDuration, keyFrames[keyFrames.Count - 1].Time);

          // Create a list of all bone indices.
          channels.Add(boneIndex);
        }
      }

      // Sort key frame times and remove duplicates.
      times.Sort();
      var timesCompacted = new List<TimeSpan>();
      var lastTime = TimeSpan.MinValue;
      foreach (var time in times)
      {
        if (time > lastTime)
        {
          timesCompacted.Add(time);
          lastTime = time;
        }
      }

      _times = timesCompacted.ToArray();

      // Sort bone indices.
      channels.Sort();
      _channels = channels.ToArray();
      int numberOfChannels = channels.Count;

      // ----- Initialize array of bone weights (_weights).
      _weights = new float[numberOfChannels];
      for (int channelIndex = 0; channelIndex < numberOfChannels; channelIndex++)
      {
        Debug.Assert(_preprocessData != null, "If _preprocessData is null, the number of channels should be 0.");

        int boneIndex = _channels[channelIndex];

        float weight;
        if (!_preprocessData.Weights.TryGetValue(boneIndex, out weight))
          weight = 1;

        _weights[channelIndex] = weight;
      }

      // ----- Create indices for fast lookup (_indices).
      _indices = new int[numberOfChannels * _times.Length];
      for (int channelIndex = 0; channelIndex < numberOfChannels; channelIndex++)
      {
        Debug.Assert(_preprocessData != null, "If _preprocessData is null, the number of channels should be 0.");

        int boneIndex = _channels[channelIndex];
        var keyFrames = _preprocessData.Channels[boneIndex];
        int keyFrameIndex = 0;
        for (int timeIndex = 0; timeIndex < _times.Length; timeIndex++)
        {
          TimeSpan time = _times[timeIndex];

          int nextIndex = keyFrameIndex + 1;
          while (nextIndex < keyFrames.Count && keyFrames[nextIndex].Time <= time)
            nextIndex++;

          // Now nextIndex points to the first key frame with a larger time. We must use
          // the previous key frame.
          keyFrameIndex = nextIndex - 1;
          _indices[timeIndex * numberOfChannels + channelIndex] = keyFrameIndex;
        }
      }

      // ----- Initialize the key frame lists (_keyFrameTypes, _keyFrames).
      _keyFrameTypes = new BoneKeyFrameType[numberOfChannels];
      _keyFrames = new object[numberOfChannels];

      // One iteration for each bone channel.
      for (int channelIndex = 0; channelIndex < numberOfChannels; channelIndex++)
      {
        Debug.Assert(_preprocessData != null, "If _preprocessData is null, the number of channels should be 0.");

        int boneIndex = _channels[channelIndex];
        var keyFrames = _preprocessData.Channels[boneIndex];

        // Check if we need Scale.
        bool isScaleUsed = false;
        foreach (var keyFrame in keyFrames)
        {
          if (keyFrame.Transform.HasScale)
          {
            isScaleUsed = true;
            break;
          }
        }

        // Check if we need Translation.
        bool isTranslationUsed = false;
        foreach (var keyFrame in keyFrames)
        {
          if (keyFrame.Transform.HasTranslation)
          {
            isTranslationUsed = true;
            break;
          }
        }

        // Add key frames depending on the type.
        if (!isScaleUsed && !isTranslationUsed)
        {
          // R
          _keyFrameTypes[channelIndex] = BoneKeyFrameType.R;
          var compactKeyFrames = new BoneKeyFrameR[keyFrames.Count];
          for (int keyFrameIndex = 0; keyFrameIndex < compactKeyFrames.Length; keyFrameIndex++)
          {
            compactKeyFrames[keyFrameIndex] = new BoneKeyFrameR
            {
              Time = keyFrames[keyFrameIndex].Time,
              Rotation = keyFrames[keyFrameIndex].Transform.Rotation,
            };
          }

          _keyFrames[channelIndex] = compactKeyFrames;
        }
        else if (isScaleUsed)
        {
          // SRT
          _keyFrameTypes[channelIndex] = BoneKeyFrameType.SRT;

          // (Cache optimization: The preprocessing data already contains SRT key frames,
          // but we still copy the key frames because we want all data to be aligned in
          // memory.)
          _keyFrames[channelIndex] = keyFrames.ToArray();
        }
        else
        {
          // RT
          _keyFrameTypes[channelIndex] = BoneKeyFrameType.RT;
          var compactKeyFrames = new BoneKeyFrameRT[keyFrames.Count];
          for (int keyFrameIndex = 0; keyFrameIndex < compactKeyFrames.Length; keyFrameIndex++)
          {
            compactKeyFrames[keyFrameIndex] = new BoneKeyFrameRT
            {
              Time = keyFrames[keyFrameIndex].Time,
              Rotation = keyFrames[keyFrameIndex].Transform.Rotation,
              Translation = keyFrames[keyFrameIndex].Transform.Translation,
            };
          }

          _keyFrames[channelIndex] = compactKeyFrames;
        }
      }

      // Free memory of preprocessing structures.
      _preprocessData = null;
    }


    private void Unfreeze()
    {
      if (!IsFrozen)
        return;

      // Copy runtime data to preprocessing data.
      EnsurePreprocessingData();
      for (int channelIndex = 0; channelIndex < _channels.Length; channelIndex++)
      {
        int boneIndex = _channels[channelIndex];

        // Copy key frames.
        var keyFrames = new List<BoneKeyFrameSRT>();
        _preprocessData.Channels.Add(boneIndex, keyFrames);

        int numberOfKeyFrames = ((Array)_keyFrames[channelIndex]).Length;
        for (int keyFrameIndex = 0; keyFrameIndex < numberOfKeyFrames; keyFrameIndex++)
        {
          TimeSpan time;
          SrtTransform boneTransform;
          GetBoneKeyFrame(channelIndex, keyFrameIndex, out time, out boneTransform);
          keyFrames.Add(new BoneKeyFrameSRT { Time = time, Transform = boneTransform });
        }

        // Copy weights.
        _preprocessData.Weights[boneIndex] = _weights[channelIndex];
      }

      // Clear runtime data.
      _totalDuration = TimeSpan.MinValue;
      _times = null;
      _channels = null;
      _weights = null;
      _indices = null;
      _keyFrameTypes = null;
      _keyFrames = null;
    }


    private static int CompareKeyFrameTime(BoneKeyFrameSRT keyFrameA, BoneKeyFrameSRT keyFrameB)
    {
      return TimeSpan.Compare(keyFrameA.Time, keyFrameB.Time);
    }


    /// <summary>
    /// Determines whether this animation animates the specified bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <returns>
    /// <see langword="true"/> if the bone is animated; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsBoneAnimated(int boneIndex)
    {
      if (IsFrozen)
      {
        // Already frozen.
        return GetChannelIndex(boneIndex) >= 0;
      }

      // Not yet frozen.
      return (_preprocessData != null) ? _preprocessData.Channels.ContainsKey(boneIndex) : false;
    }


    /// <summary>
    /// Gets the index in <see cref="_channels"/> for the given bone.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <returns>
    /// The index in <see cref="_channels"/> or a negative value (see 
    /// <see cref="Array.BinarySearch{T}(T[],T)"/> description).
    /// </returns>
    private int GetChannelIndex(int boneIndex)
    {
      Debug.Assert(IsFrozen, "Cannot use GetIndex before the SkeletonKeyFrameAnimation was frozen.");

      return Array.BinarySearch(_channels, boneIndex);
    }


    /// <summary>
    /// Gets the weight for a specific bone animation channel.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <returns>
    /// The weight. A value of 0 means that the channel is disabled. A value of 1 means that the
    /// channel is fully enabled. If the value is less than 1, then animation of the bone is mixed
    /// with preceding animations. If the animation does contain a bone animation channel for the
    /// given bone then 1 (default value) is returned.
    /// </returns>
    /// <remarks>
    /// Call <see cref="IsBoneAnimated"/> to check whether this animation contains a bone animation
    /// channel for the specified bone.
    /// </remarks>
    public float GetWeight(int boneIndex)
    {
      if (IsFrozen)
      {
        // Already frozen.
        int channelIndex = GetChannelIndex(boneIndex);
        if (channelIndex >= 0)
          return _weights[channelIndex];
      }
      else
      {
        // Not yet frozen.
        float weight;
        if (_preprocessData != null && _preprocessData.Weights.TryGetValue(boneIndex, out weight))
          return weight;
      }

      return 1;
    }


    /// <summary>
    /// Sets the weight for a specific bone animation channel.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <param name="weight">
    /// The weight. Use 0 to disable the channel. Use 1 to fully enable the channel. If the weight
    /// is less than 1, the animation of the bone is mixed with preceding animations. The default
    /// weight of all bone animation channels is 1.
    /// </param>
    public void SetWeight(int boneIndex, float weight)
    {
      if (IsFrozen)
      {
        // Already frozen.
        int channelIndex = GetChannelIndex(boneIndex);
        if (channelIndex < 0)
          return;

        _weights[channelIndex] = weight;
      }
      else
      {
        // Not yet frozen.
        EnsurePreprocessingData();
        _preprocessData.Weights[boneIndex] = weight;
      }
    }


    /// <inheritdoc/>
    /// <exception cref="AnimationException">
    /// This animation is not frozen. <see cref="Freeze"/> must be called before the animation can
    /// be used.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    AnimationInstance ITimeline.CreateInstance()
    {
      if (!IsFrozen)
        throw new AnimationException("This animation is not frozen. Freeze() must be called before the animation can be used.");

      return AnimationInstance<SkeletonPose>.Create(this);
    }


    /// <inheritdoc/>
    /// <exception cref="AnimationException">
    /// This animation is not frozen. <see cref="Freeze"/> must be called before the animation can
    /// be used.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    BlendAnimation IAnimation.CreateBlendAnimation()
    {
      if (!IsFrozen)
        throw new AnimationException("This animation is not frozen. Freeze() must be called before the animation can be used.");

      return new BlendAnimation<SkeletonPose>();
    }


    /// <inheritdoc/>
    public TimeSpan? GetAnimationTime(TimeSpan time)
    {
      return AnimationHelper.GetAnimationTime(this, time);
    }


    /// <inheritdoc/>
    public AnimationState GetState(TimeSpan time)
    {
      return AnimationHelper.GetState(this, time);
    }


    /// <inheritdoc/>
    public TimeSpan GetTotalDuration()
    {
      if (IsFrozen)
      {
        // Already frozen.
        return _totalDuration;
      }

      // Not yet frozen.

      // This method can be called when the key frames are not sorted. We simply
      // check all key frames that we have.
      if (_preprocessData == null)
        return TimeSpan.Zero;

      TimeSpan duration = TimeSpan.Zero;
      foreach (var keyFrames in _preprocessData.Channels.Values)
        foreach (var keyFrame in keyFrames)
          duration = AnimationHelper.Max(duration, keyFrame.Time);

      return duration;
    }


    /// <exception cref="AnimationException">
    /// This animation is not frozen. <see cref="Freeze"/> must be called before the animation can
    /// be used.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="defaultSource"/>, <paramref name="defaultTarget"/> or
    /// <paramref name="result"/> is <see langword="null"/>.
    /// </exception>
    /// <inheritdoc/>
    public void GetValue(TimeSpan time, ref SkeletonPose defaultSource, ref SkeletonPose defaultTarget, ref SkeletonPose result)
    {
      if (!IsFrozen)
        throw new AnimationException("This animation is not frozen. Freeze() must be called before the animation can be used.");
      if (defaultSource == null)
        throw new ArgumentNullException("defaultSource");
      if (defaultTarget == null)
        throw new ArgumentNullException("defaultTarget");
      if (result == null)
        throw new ArgumentNullException("result");

      TimeSpan? animationTime = GetAnimationTime(time);
      if (animationTime == null)
      {
        // Animation is inactive and does not produce any output.
        if (defaultSource == result)
          return;

        // Copy bone transforms of defaultSource to result for the animated channels.
        for (int i = 0; i < _channels.Length; i++)
        {
          int boneIndex = _channels[i];
          if (boneIndex >= result.BoneTransforms.Length)
            break;

          result.BoneTransforms[boneIndex] = defaultSource.BoneTransforms[boneIndex];
        }

        return;
      }

      time = animationTime.Value;

      // Clamp time to allowed range.
      var startTime = _times[0];
      var endTime = _totalDuration;
      if (time < startTime)
        time = startTime;
      if (time > endTime)
        time = endTime;

      int timeIndex = GetTimeIndex(time);

      if (!IsAdditive)
      {
        // Evaluate animation.
        for (int channelIndex = 0; channelIndex < _channels.Length; channelIndex++)
        {
          int boneIndex = _channels[channelIndex];
          if (boneIndex >= result.BoneTransforms.Length)
            break;

          Debug.Assert(((Array)_keyFrames[channelIndex]).Length > 0, "Each channel must have at least 1 key frame.");

          float weight = _weights[channelIndex];
          if (weight == 0 && defaultSource != result)
          {
            // This channel is inactive.
            result.BoneTransforms[boneIndex] = defaultSource.BoneTransforms[boneIndex];
          }
          else if (weight == 1)
          {
            // Channel is fully active.
            result.BoneTransforms[boneIndex] = GetBoneTransform(channelIndex, timeIndex, time);
          }
          else
          {
            // Mix channel with source.
            SrtTransform boneTransform = GetBoneTransform(channelIndex, timeIndex, time);
            SrtTransform.Interpolate(ref defaultSource.BoneTransforms[boneIndex], ref boneTransform, weight, ref boneTransform);
            result.BoneTransforms[boneIndex] = boneTransform;
          }
        }
      }
      else
      {
        // Additive animation.
        for (int channelIndex = 0; channelIndex < _channels.Length; channelIndex++)
        {
          int boneIndex = _channels[channelIndex];
          if (boneIndex >= result.BoneTransforms.Length)
            break;

          Debug.Assert(((Array)_keyFrames[channelIndex]).Length > 0, "Each channel must have at least 1 key frame.");

          float weight = _weights[channelIndex];
          if (weight == 0 && defaultSource != result)
          {
            // Channel is inactive.
            result.BoneTransforms[boneIndex] = defaultSource.BoneTransforms[boneIndex];
          }
          else if (weight == 1)
          {
            // Channel is fully active.
            result.BoneTransforms[boneIndex] = defaultSource.BoneTransforms[boneIndex] * GetBoneTransform(channelIndex, timeIndex, time);
          }
          else
          {
            // Add only a part of this animation value.
            SrtTransform boneTransform = GetBoneTransform(channelIndex, timeIndex, time);
            SrtTransform identity = SrtTransform.Identity;
            SrtTransform.Interpolate(ref identity, ref boneTransform, weight, ref boneTransform);
            result.BoneTransforms[boneIndex] = defaultSource.BoneTransforms[boneIndex] * boneTransform;
          }
        }
      }
    }


    /// <summary>
    /// Gets the index in <see cref="_times"/> for the given key frame time.
    /// </summary>
    /// <param name="time">The time.</param>
    /// <returns>The index in <see cref="_times"/>.</returns>
    private int GetTimeIndex(TimeSpan time)
    {
      Debug.Assert(_times != null && _times.Length > 0, "_times is empty or invalid.");

      if (_times.Length == 1)
        return 0;

      // Search range.
      int start = 0;
      int end = _times.Length - 1;

      // Binary search.
      while (start <= end)
      {
        int index = start + (end - start >> 1);
        int comparison = TimeSpan.Compare(_times[index], time);
        if (comparison == 0)
        {
          return index;
        }

        if (comparison < 0)
        {
          Debug.Assert(time > _times[index]);
          start = index + 1;
        }
        else
        {
          Debug.Assert(time < _times[index]);
          end = index - 1;
        }
      }

      return start - 1;
    }


    /// <summary>
    /// Gets the bone transform for a certain time considering key frame interpolation.
    /// </summary>
    /// <param name="channelIndex">The index in <see cref="_channels"/>.</param>
    /// <param name="timeIndex">The index in <see cref="_times"/>.</param>
    /// <param name="time">The animation time.</param>
    /// <returns>The animation value.</returns>
    private SrtTransform GetBoneTransform(int channelIndex, int timeIndex, TimeSpan time)
    {
      // Get index in the key frames list using the _indices lookup table.
      int keyFrameIndex = _indices[timeIndex * _channels.Length + channelIndex];

      if (EnableInterpolation && keyFrameIndex + 1 < ((Array)_keyFrames[channelIndex]).Length)
      {
        // ----- Key frame interpolation.
        // Get the key frame before and after the specified time.
        TimeSpan previousTime, nextTime;
        SrtTransform previousTransform, nextTransform;
        GetBoneKeyFrames(channelIndex, keyFrameIndex, out previousTime, out previousTransform, out nextTime, out nextTransform);

        float parameter = (float)(time.Ticks - previousTime.Ticks) / (nextTime - previousTime).Ticks;
        SrtTransform.Interpolate(ref previousTransform, ref nextTransform, parameter, ref previousTransform);
        return previousTransform;
      }

      // ----- No key frame interpolation.
      //TimeSpan time;
      SrtTransform transform;
      GetBoneKeyFrame(channelIndex, keyFrameIndex, out time, out transform);
      return transform;
    }


    /// <summary>
    /// Gets one bone key frame for a given channel and key frame index.
    /// </summary>
    /// <param name="channelIndex">The index in <see cref="_channels"/>.</param>
    /// <param name="keyFrameIndex">
    /// The index in <see cref="_keyFrames"/> for the given channel.
    /// </param>
    /// <param name="time">The key frame time.</param>
    /// <param name="transform">The transform.</param>
    private void GetBoneKeyFrame(int channelIndex, int keyFrameIndex, out TimeSpan time, out SrtTransform transform)
    {
      var boneKeyFrameType = _keyFrameTypes[channelIndex];
      if (boneKeyFrameType == BoneKeyFrameType.R)
      {
        var keyFrames = (BoneKeyFrameR[])_keyFrames[channelIndex];

        var keyFrame = keyFrames[keyFrameIndex];
        time = keyFrame.Time;
        transform.Scale = Vector3F.One;
        transform.Rotation = keyFrame.Rotation;
        transform.Translation = Vector3F.Zero;
      }
      else if (boneKeyFrameType == BoneKeyFrameType.RT)
      {
        var keyFrames = (BoneKeyFrameRT[])_keyFrames[channelIndex];

        var keyFrame = keyFrames[keyFrameIndex];
        time = keyFrame.Time;
        transform.Scale = Vector3F.One;
        transform.Rotation = keyFrame.Rotation;
        transform.Translation = keyFrame.Translation;
      }
      else
      {
        var keyFrames = (BoneKeyFrameSRT[])_keyFrames[channelIndex];

        var keyFrame = keyFrames[keyFrameIndex];
        time = keyFrame.Time;
        transform = keyFrame.Transform;
      }
    }


    /// <summary>
    /// Gets two bone key frame for a given channel and key frame index.
    /// </summary>
    /// <param name="channelIndex">The index in <see cref="_channels"/>.</param>
    /// <param name="keyFrameIndex">The index of the first key frame.</param>
    /// <param name="time0">The time of the first key frame.</param>
    /// <param name="transform0">The transform of the first key frame.</param>
    /// <param name="time1">The time of the second key frame.</param>
    /// <param name="transform1">The transform of the second key frame.</param>
    private void GetBoneKeyFrames(int channelIndex, int keyFrameIndex,
                                  out TimeSpan time0, out SrtTransform transform0,
                                  out TimeSpan time1, out SrtTransform transform1)
    {
      Debug.Assert(keyFrameIndex + 1 < ((Array)_keyFrames[channelIndex]).Length, "Call GetBoneKeyFrame() instead of GetBoneKeyFrames()!");

      var boneKeyFrameType = _keyFrameTypes[channelIndex];
      if (boneKeyFrameType == BoneKeyFrameType.R)
      {
        var keyFrames = (BoneKeyFrameR[])_keyFrames[channelIndex];

        var keyFrame = keyFrames[keyFrameIndex];
        time0 = keyFrame.Time;
        transform0.Scale = Vector3F.One;
        transform0.Rotation = keyFrame.Rotation;
        transform0.Translation = Vector3F.Zero;

        keyFrame = keyFrames[keyFrameIndex + 1];
        time1 = keyFrame.Time;
        transform1.Scale = Vector3F.One;
        transform1.Rotation = keyFrame.Rotation;
        transform1.Translation = Vector3F.Zero;
      }
      else if (boneKeyFrameType == BoneKeyFrameType.RT)
      {
        var keyFrames = (BoneKeyFrameRT[])_keyFrames[channelIndex];

        var keyFrame = keyFrames[keyFrameIndex];
        time0 = keyFrame.Time;
        transform0.Scale = Vector3F.One;
        transform0.Rotation = keyFrame.Rotation;
        transform0.Translation = keyFrame.Translation;

        keyFrame = keyFrames[keyFrameIndex + 1];
        time1 = keyFrame.Time;
        transform1.Scale = Vector3F.One;
        transform1.Rotation = keyFrame.Rotation;
        transform1.Translation = keyFrame.Translation;
      }
      else
      {
        var keyFrames = (BoneKeyFrameSRT[])_keyFrames[channelIndex];

        var keyFrame = keyFrames[keyFrameIndex];
        time0 = keyFrame.Time;
        transform0 = keyFrame.Transform;

        keyFrame = keyFrames[keyFrameIndex + 1];
        time1 = keyFrame.Time;
        transform1 = keyFrame.Transform;
      }
    }
    #endregion
  }
}
