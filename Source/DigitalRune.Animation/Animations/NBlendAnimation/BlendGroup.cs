// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Mathematics;
using DigitalRune.Text;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Blends two or more animations.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A blend group is a collection of animations (timelines). It automatically blends the
  /// animations based on a blend weight, which can be set using 
  /// <see cref="SetWeight(ITimeline,float)"/>. The blend weight indicates the importance of the
  /// corresponding animation. For example, if one animation has a blend weight of 1 and another
  /// animation has a blend weight of 0.5 then the second has less influence on the animation
  /// result. A blend weight can be set to 0 to disable the animation.
  /// </para>
  /// <para>
  /// The blend weights are internally normalized, meaning that the blend weights do not need be in 
  /// the range [0, 1] when they are set. Blend weights can have any value equal to or greater than
  /// 0. The sum of all blend weights should be greater than 0 - i.e. at least one animation should
  /// to be active!
  /// </para>
  /// <para>
  /// A blend group can contain <see cref="TimelineGroup"/>s. In this case the animations within one
  /// <see cref="TimelineGroup"/> are blended with the matching animations in the other 
  /// <see cref="TimelineGroup"/>s. The animations are matched by comparing the 
  /// <see cref="IAnimation.TargetProperty"/> properties.
  /// </para>
  /// <para>
  /// <strong>Synchronization:</strong> Most animations have different durations. When blending 
  /// cyclic animations, for example, a "Walk" cycle and a "Run" cycle of a character, then it is
  /// important to synchronize the durations of the animations. When a "Walk" cycle and a "Run" 
  /// cycle are blended with equal blend weights then the result should be the average of "Walk" and
  /// "Run". The duration of the blended animation should be the average both cycles. A key frame of
  /// the "Walk" cycle needs to be interpolated with the matching key frame of the "Run" cycle. In 
  /// order to synchronize the animations the method <see cref="SynchronizeDurations"/> needs to be 
  /// called when all animations have been set.
  /// </para>
  /// <para>
  /// Note that synchronization is optional. The methods <see cref="SynchronizeDurations"/> does not
  /// need to be called if the contained animations do not need to be synchronized.
  /// </para>
  /// <para>
  /// <strong>Limitations:</strong> A blend group has certain limitations.
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// The timelines within a blend group must be of type <see cref="TimelineGroup"/> or implement
  /// <see cref="IAnimation{T}"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// If a <see cref="TimelineGroup"/> is added to a blend group, it should only contain animations 
  /// (<see cref="IAnimation{T}"/>). Nested timelines, such as <see cref="TimelineGroup"/> within a
  /// <see cref="TimelineGroup"/>, are not supported and will be ignored.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// If a <see cref="TimelineGroup"/>s is added to a blend group, the 
  /// <see cref="IAnimation.TargetProperty"/> must be set for all animations in the
  /// <see cref="TimelineGroup"/>. This is necessary because the blend group matches the animations
  /// of one <see cref="TimelineGroup"/> with those of the other <see cref="TimelineGroup"/>s by 
  /// comparing the <see cref="IAnimation.TargetProperty"/>. A <see cref="TimelineGroup"/> should 
  /// not contain multiple animations that target the same property.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// The <see cref="TargetObject"/> of the animations within a blend group will be ignored. Only
  /// the property <see cref="TargetObject"/> of the blend group itself will be read.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Animations must not be added to or removed from the blend group if the animation is already 
  /// playing. (However, if all playing animation instances of the blend group are stopped and the 
  /// associated <see cref="AnimationController"/>s are recycled, then animations can be added or 
  /// removed, and the blend group can be restarted.)
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// A blend group does not support additive animations. The property 
  /// <see cref="Animation{T}.IsAdditive"/> of contained animations will be ignored.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// <strong>Animating Blend Weights:</strong> The blend weights in a blend group can be animated.
  /// The method <see cref="GetWeightAsAnimatable(ITimeline)"/> returns an 
  /// <see cref="IAnimatableProperty{T}"/> that represent the blend weight of the specified timeline
  /// and can be animated. 
  /// </para>
  /// <para>
  /// A blend group also implements the interface <see cref="IAnimatableObject"/>. The blend weights
  /// can also be retrieved using <see cref="IAnimatableObject.GetAnimatableProperty{T}"/> where the
  /// blend weights are identified using the strings "Weight0", Weight1", etc. The suffix is the 
  /// index of the associated timeline.
  /// </para>
  /// </remarks>
  public class BlendGroup : ITimeline, IList<ITimeline>, IAnimatableObject
  {
    // Thread-safety:
    // The AnimationManager may read blend animations from multiple threads simultaneously.
    // Locking ensures that this memory access in DigitalRune Animation is safe.
    // But the BlendGroup does not implement a full reader-writer lock!
    // The following can occur in custom multithreaded applications:
    // - Thread A enters GetBonePoseAbsolute() GetBonePoseRelative() and starts
    //   reading the SrtTransform.
    // - Thread B invalidates the bone and immediately updates the SrtTransform.
    // - Thread A reads an invalid SrtTransform.
    // To improve thread-safety and simplify code:
    // To improve thread-safety and simplify code we could use a ReaderWriterLockSlim
    // instead of partial lock in vNext.
    //
    // Internals:
    // - CachedDurations, TimeNormalizationFactors, _synchronizedDuration are only computed
    //   when _synchronizeDurations is set.
    // - CachedDurations are always updated immediately, the rest is updated when needed.
    // - The internal update requires locking because the AnimationManager is multithreaded.
    //   BlendGroups are usually not shared, therefore it is unlikely that one worker
    //   thread blocks another worker thread.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // Data per timeline.
    private struct Entry
    {
      public ITimeline Timeline;
      public AnimatableBlendWeight Weight;
      public float NormalizedWeight;
      public TimeSpan CachedDuration;
      public double TimeNormalizationFactor;
    }


    /// <summary>
    /// Enumerates the elements of a <see cref="BlendGroup"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<ITimeline>
    {
      private readonly ArrayList<Entry> _entries;
      private int _index;
      private ITimeline _current;


      /// <summary>
      /// Gets the element in the collection at the current position of the enumerator.
      /// </summary>
      /// <value>The element in the collection at the current position of the enumerator.</value>
      public ITimeline Current
      {
        get { return _current; }
      }


      /// <summary>
      /// Gets the element in the collection at the current position of the enumerator.
      /// </summary>
      /// <value>The element in the collection at the current position of the enumerator.</value>
      /// <exception cref="InvalidOperationException">
      /// The enumerator is positioned before the first element of the collection or after the last
      /// element.
      /// </exception>
      object IEnumerator.Current
      {
        get
        {
          if (_index < 0)
          {
            if (_index == -1)
              throw new InvalidOperationException("The enumerator is positioned before the first element of the collection.");

            throw new InvalidOperationException("The enumerator is positioned after the last element of the collection.");
          }

          return _current;
        }
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="BlendGroup.Enumerator"/> struct.
      /// </summary>
      /// <param name="blendGroup">The <see cref="BlendGroup"/> to be enumerated.</param>
      internal Enumerator(BlendGroup blendGroup)
      {
        _entries = blendGroup._entries;
        _index = -1;
        _current = null;
      }


      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting 
      /// unmanaged resources.
      /// </summary>
      public void Dispose()
      {
        _index = -2;
        _current = null;
      }


      /// <summary>
      /// Advances the enumerator to the next element of the collection.
      /// </summary>
      /// <returns>
      /// <see langword="true"/> if the enumerator was successfully advanced to the next element; 
      /// <see langword="false"/> if the enumerator has passed the end of the collection.
      /// </returns>
      /// <exception cref="InvalidOperationException">
      /// The collection was modified after the enumerator was created.
      /// </exception>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Deque")]
      public bool MoveNext()
      {
        //if (_version != _collection._version)
        //  throw new InvalidOperationException("The collection was modified after the enumerator was created.");

        if (_index == -2)
          return false;

        _index++;
        if (_index < _entries.Count)
        {
          _current = _entries.Array[_index].Timeline;
          return true;
        }

        _index = -2;
        _current = null;
        return false;
      }


      /// <summary>
      /// Sets the enumerator to its initial position, which is before the first element in the 
      /// <see cref="BlendGroup"/>.
      /// </summary>
      /// <exception cref="InvalidOperationException">
      /// The <see cref="BlendGroup"/> was modified after the enumerator was created.
      /// </exception>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Deque")]
      public void Reset()
      {
        //if (_version != _collection._version)
        //  throw new InvalidOperationException("The collection was modified after the enumerator was created.");

        _index = -1;
        _current = null;
      }
    }
    #endregion
    

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly ArrayList<Entry> _entries;
    private readonly Dictionary<string, BlendAnimation> _blendAnimations;

    private volatile bool _isDirty;         // true, if normalized weights or synchronized durations need to be updated.
    private bool _synchronizeDurations;     // true, if timelines should be synchronized.
    private bool _synchronized;             // true, if TimeNormalizationFactors and _synchronizeDuration are up-to-date.
    private TimeSpan _synchronizedDuration; // The current duration when synchronized.

    // The internal update requires locking.
    private readonly object _syncRoot = new object();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value that specifies how the animation behaves when it reaches the end of its 
    /// duration.
    /// </summary>
    /// <value>
    /// A value that specifies how the animation behaves when it reaches the end of its duration.
    /// The default value is <see cref="Animation.FillBehavior.Hold"/>.
    /// </value>
    /// <inheritdoc cref="ITimeline.FillBehavior"/>
    public FillBehavior FillBehavior { get; set; }


    /// <summary>
    /// Gets or sets the object to which the animation is applied by default.
    /// </summary>
    /// <value>
    /// The object to which the animation is applied by default. The default value is 
    /// <see langword="null"/>.
    /// </value>
    /// <inheritdoc cref="ITimeline.TargetObject"/>
    public string TargetObject { get; set; }


    /// <summary>
    /// Gets the number of timelines contained in the <see cref="BlendGroup"/>.
    /// </summary>
    /// <value>The number of timelines contained in the <see cref="BlendGroup"/>.</value>
    public int Count
    {
      get { return _entries.Count; }
    }


    /// <summary>
    /// Gets a value indicating whether this collection is read-only.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this collection is read-only; otherwise, <see langword="false"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool ICollection<ITimeline>.IsReadOnly
    {
      get { return false; }
    }


    /// <summary>
    /// Gets or sets the timeline at the specified index.
    /// </summary>
    /// <value>The timeline at the specified index.</value>
    /// <param name="index">The zero-based index of the timeline to get or set.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or equal to or greater than <see cref="Count"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>. The <see cref="BlendGroup"/> does not
    /// allow <see langword="null"/> values.
    /// </exception>
    public ITimeline this[int index]
    {
      get { return _entries.Array[index].Timeline; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _entries.Array[index].Timeline = value;
        OnTimelineChanged(index);
      }
    }


    /// <summary>
    /// Gets or sets the time at which the animation begins.
    /// </summary>
    /// <value>
    /// The time at which the animation should begin. The default value is 0.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property can be used to delay the start of an animation. The delay time marks the time
    /// on the timeline when the animation starts. The <see cref="Speed"/> does not affect the 
    /// delay. For example, an animation with a delay of 3 seconds, a duration of 10 seconds 
    /// and a speed ratio of 2 will start after 3 seconds and run for 5 seconds with double speed.
    /// </para>
    /// <para>
    /// Note: The delay time can also be negative. For example, an animation with a delay time of 
    /// -2.5 seconds and a duration of 5 seconds will start right in the middle of the animation.
    /// </para>
    /// </remarks>
    public TimeSpan Delay { get; set; }


    /// <summary>
    /// Gets or sets the duration for which the animation is played.
    /// </summary>
    /// <value>
    /// The duration for which the animation is played. The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The property <see cref="Duration"/> can be set to override the duration of the animation. If
    /// <see cref="Duration"/> is greater than the length of the animations in the blend group, the 
    /// blend group will be repeated using the defined loop behavior (see 
    /// <see cref="LoopBehavior"/>).
    /// </para>
    /// <para>
    /// The effective duration depends on the <see cref="Speed"/>: For example, an animation with a 
    /// delay of 3 seconds, a duration of 10 seconds and a speed ratio of 2 will start after 3 
    /// seconds and run for 5 seconds with double speed.
    /// </para>
    /// <para>
    /// The default value is <see langword="null"/>, which indicates that the duration is 
    /// 'automatic' or 'unknown'. In this case the blend group plays exactly once. A duration of
    /// <see cref="TimeSpan.MaxValue"/> can be set to repeat the animation forever. 
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public TimeSpan? Duration
    {
      get { return _duration; }
      set
      {
        if (value.HasValue && value.Value < TimeSpan.Zero)
          throw new ArgumentOutOfRangeException("value", "The duration of an animation must not be negative.");

        _duration = value;
      }
    }
    private TimeSpan? _duration;


    /// <summary>
    /// Gets or sets the speed ratio at which the animation is played.
    /// </summary>
    /// <value>
    /// The rate at which time progresses for the blend group. The value must be a finite number 
    /// greater than or equal to 0. The default value is 1.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or not a finite value.
    /// </exception>
    public float Speed
    {
      get { return _speed; }
      set
      {
        if (!Numeric.IsZeroOrPositiveFinite(value))
          throw new ArgumentOutOfRangeException("value", "The speed must be a finite number greater than or equal to 0.");

        _speed = value;
      }
    }
    private float _speed;


    /// <summary>
    /// Gets or sets the behavior of the animations past the end of the duration.
    /// </summary>
    /// <value>
    /// The behavior of the animations past the end of the duration. The default value is 
    /// <see cref="Animation.LoopBehavior.Cycle"/>.
    /// </value>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is set to <see cref="Animation.LoopBehavior.CycleOffset"/>. This 
    /// loop behavior is not supported by a <see cref="BlendGroup"/>.
    /// </exception>
    public LoopBehavior LoopBehavior
    {
      get { return _loopBehavior; }
      set
      {
        if (value == LoopBehavior.CycleOffset)
          throw new ArgumentException("BlendGroups do not support the loop behavior 'CycleOffset'.");

        _loopBehavior = value;
      }
    }
    private LoopBehavior _loopBehavior;


    /// <summary>
    /// Gets the duration of a single cycle of the blended animations.
    /// </summary>
    /// <value>The duration of a single cycle of the blended animations.</value>
    /// <remarks>
    /// This property is only valid after <see cref="SynchronizeDurations"/>. If the animations are
    /// not synchronized, the property returns the duration of the longest animation in the blend
    /// group.
    /// </remarks>
    public TimeSpan SynchronizedDuration
    {
      get { return GetDefaultDuration(); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="BlendGroup"/> class.
    /// </summary>
    public BlendGroup()
    {
      FillBehavior = FillBehavior.Hold;
      TargetObject = null;

      Delay = TimeSpan.Zero;
      Duration = null;
      Speed = 1.0f;
      LoopBehavior = LoopBehavior.Constant;

      _entries = new ArrayList<Entry>(4);
      _blendAnimations = new Dictionary<string, BlendAnimation>();

      _isDirty = false;
      _synchronizeDurations = false;
      _synchronized = false;
      _synchronizedDuration = TimeSpan.Zero;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public AnimationInstance CreateInstance()
    {
      var animationInstance = BlendGroupInstance.Create(this);
      if (_blendAnimations.Count == 0)
      {
        // The blend group has been invalidated, or this is the first time that the
        // animation is played. --> BlendAnimations need to be created.
        var entries = _entries.Array;
        int numberOfEntries = _entries.Count;
        for (int i = 0; i < numberOfEntries; i++)
        {
          var timeline = entries[i].Timeline;
          if (timeline is IAnimation)
          {
            // The timeline in the blend group is an IAnimation<T>.
            // Add animation to a BlendAnimation.
            CreateBlendAnimations(i, (IAnimation)timeline);
          }
          else if (timeline is TimelineGroup)
          {
            // The timeline in the blend group is a timeline group.
            // Add the children of timeline group to BlendAnimations.
            var timelineGroup = (TimelineGroup)timeline;
            foreach (var child in timelineGroup)
              CreateBlendAnimations(i, child as IAnimation);
          }
        }
      }

      // Add BlendAnimations to animation tree.
      foreach (var blendAnimation in _blendAnimations.Values)
        animationInstance.Children.Add(blendAnimation.CreateInstance());

      return animationInstance;
    }


    /// <summary>
    /// Creates the required <see cref="BlendAnimation"/>s and adds the given timeline to the 
    /// <see cref="BlendAnimation"/>.
    /// </summary>
    /// <param name="index">The index of <paramref name="animation"/> in the blend group.</param>
    /// <param name="animation">
    /// The animation to add to a <see cref="BlendAnimation"/>. Can be <see langword="null"/>.
    /// </param>
    private void CreateBlendAnimations(int index, IAnimation animation)
    {
      if (animation == null)
      {
        // Only timelines that implement IAnimation are supported.
        // The current timeline is not of the correct type.
        return;
      }

      BlendAnimation blendAnimation;
      var targetProperty = animation.TargetProperty ?? String.Empty;
      if (!_blendAnimations.TryGetValue(targetProperty, out blendAnimation))
      {
        // Create new BlendAnimation<T>.
        blendAnimation = animation.CreateBlendAnimation();
        blendAnimation.Initialize(this, targetProperty);
        _blendAnimations.Add(targetProperty, blendAnimation);
      }

      // Add animation to the BlendAnimation<T>.
      blendAnimation.AddAnimation(index, animation);
    }


    /// <summary>
    /// Synchronizes the durations of the animations in the blend group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Many animations that need to be blended have different durations. For example, a "Walk"
    /// animation has a different duration than a "Run" animation. In order to blend the matching
    /// frames the durations of the animations in the blend group need to be synchronized. 
    /// Synchronization of durations is typically required for cyclic animations that should be 
    /// mixed.
    /// </para>
    /// <para>
    /// The method <see cref="SynchronizeDurations"/> needs to be called once after all animations 
    /// have been added to the blend group to synchronize them. (It does not need be called again if
    /// blend weights are changed or when animations are added or removed. However, if the duration
    /// of one animation is changed manually - for example, by appending key frames to a key frame
    /// animation - then <see cref="SynchronizeDurations"/> needs to be called a second time to
    /// update the blend group.)
    /// </para>
    /// <para>
    /// If the durations are synchronized the total animation duration of the blend group is the
    /// weighted average of all animations. If the durations are not synchronized then the total 
    /// durations is determined by the longest animation in the blend group.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidAnimationException">
    /// The sum of the blend weights in the blend group is 0.
    /// </exception>
    public void SynchronizeDurations()
    {
      _synchronizeDurations = true;

      // Cache the durations of all animations in the blend group.
      int numberOfEntries = _entries.Count;
      for (int i = 0; i < numberOfEntries; i++)
        CacheDuration(i);

      // Update the rest when needed.
      _isDirty = true;
    }


    /// <summary>
    /// Called when a timeline was added or removed.
    /// </summary>
    /// <param name="index">
    /// The index of the new timeline; -1 if a timeline was removed.
    /// </param>
    private void OnTimelineChanged(int index)
    {
      // Clear the cached BlendAnimations.
      _blendAnimations.Clear();

      // Cached the new duration immediately.
      CacheDuration(index);

      // Update the rest when needed.
      _isDirty = true;
    }


    /// <summary>
    /// Called when one or more blend weights were changed.
    /// (For use by <see cref="AnimatableBlendWeight"/>.)
    /// </summary>
    internal void OnWeightChanged()
    {
      _isDirty = true;
    }


    /// <summary>
    /// Caches the total duration of the timeline with the given index.
    /// </summary>
    /// <param name="index">The index of the timeline.</param>
    private void CacheDuration(int index)
    {
      if (_synchronizeDurations && index >= 0)
      {
        var entries = _entries.Array;
        entries[index].CachedDuration = entries[index].Timeline.GetTotalDuration();
      }
    }


    /// <summary>
    /// Updates this instance.
    /// </summary>
    /// <remarks>
    /// Call <see cref="Update"/> before calling <see cref="GetTimeNormalizationFactor"/> to ensure
    /// that the animations are synchronized.
    /// </remarks>
    internal void Update()
    {
      if (_isDirty)
      {
        lock (_syncRoot)
        {
          if (_isDirty)
          {
            NormalizeWeights();
            UpdateDuration();

            _isDirty = false;
          }
        }
      }
    }


    /// <summary>
    /// Normalizes the blend weights and caches the results.
    /// </summary>
    private void NormalizeWeights()
    {
      // Get weights and calculate sum.
      var entries = _entries.Array;
      int numberOfEntries = _entries.Count;
      float sum = 0.0f;
      for (int i = 0; i < numberOfEntries; i++)
      {
        float weight = entries[i].Weight.Value;
        entries[i].NormalizedWeight = weight;
        sum += weight;
      }

      if (sum <= 0.0f)
      {
        // Invalid blend weights. The sum of the blend weights should be greater than 0.
        // Disable blend group by setting all blend weights to 0.
        for (int i = 0; i < numberOfEntries; i++)
          entries[i].NormalizedWeight = 0.0f;
      }
      else
      {
        // Normalize weights.
        float oneOverSum = 1.0f / sum;
        for (int i = 0; i < numberOfEntries; i++)
          entries[i].NormalizedWeight *= oneOverSum;
      }
    }


    /// <summary>
    /// Synchronizes the durations if necessary and computes factors which are required to normalize 
    /// animation times.
    /// </summary>
    private void UpdateDuration()
    {
      if (_synchronizeDurations)
      {
        // Reset fields in case something goes wrong.
        _synchronized = false;
        _synchronizedDuration = TimeSpan.Zero;

        // Determine the total duration as the weighted average of the animations in the
        // blend group. The durations are cached to avoid expensive recomputation.
        var array = _entries.Array;
        int numberOfEntries = _entries.Count;
        for (int i = 0; i < numberOfEntries; i++)
          _synchronizedDuration += new TimeSpan((long)(array[i].CachedDuration.Ticks * (double)array[i].NormalizedWeight));

        // Compute factors required to synchronize the animation times.
        // The total duration needs to be greater than 0, otherwise there is
        // no need to synchronize anything.
        if (_synchronizedDuration > TimeSpan.Zero)
        {
          double oneOverTotalDuration = 1.0 / _synchronizedDuration.Ticks;
          for (int i = 0; i < numberOfEntries; i++)
            array[i].TimeNormalizationFactor = array[i].CachedDuration.Ticks * oneOverTotalDuration;

          _synchronized = true;
        }
      }
    }


    /// <summary>
    /// Gets the normalized blend weight.
    /// </summary>
    /// <param name="index">The index of the timeline in the <see cref="BlendGroup"/>.</param>
    /// <returns>The normalized blend weight.</returns>
    /// <remarks>
    /// Call <see cref="Update"/> before calling <see cref="GetNormalizedWeight"/> to ensure
    /// that the normalized weights are up-to-date.
    /// </remarks>
    internal float GetNormalizedWeight(int index)
    {
      Debug.Assert(!_isDirty);
      return _entries.Array[index].NormalizedWeight;
    }


    /// <summary>
    /// Gets the factor that needs to be multiplied with the animation time to synchronize the
    /// animation with the specified index.
    /// </summary>
    /// <param name="index">The index of the animation in the blend group.</param>
    /// <returns>The time normalization factor.</returns>
    /// <remarks>
    /// Call <see cref="Update"/> before calling <see cref="GetTimeNormalizationFactor"/> to ensure
    /// that the durations are synchronized.
    /// </remarks>
    internal double GetTimeNormalizationFactor(int index)
    {
      Debug.Assert(!_isDirty);
      return _synchronized ? _entries.Array[index].TimeNormalizationFactor : 1.0;
    }


    private TimeSpan GetDefaultDuration()
    {
      Update();

      if (_synchronized)
      {
        // The total duration has been cached.
        return _synchronizedDuration;
      }

      // Get max duration of the animations in the blend group.
      TimeSpan duration = TimeSpan.Zero;
      var entries = _entries.Array;
      int numberOfEntries = _entries.Count;
      for (int i = 0; i < numberOfEntries; i++)
        duration = AnimationHelper.Max(duration, entries[i].Timeline.GetTotalDuration());

      return duration;
    }


    /// <inheritdoc/>
    public AnimationState GetState(TimeSpan time)
    {
      // ----- Delay
      time -= Delay;
      if (time < TimeSpan.Zero)
      {
        // The animation has not started.
        return AnimationState.Delayed;
      }

      // ----- Speed
      time = new TimeSpan((long)(time.Ticks * (double)Speed));

      // ----- Duration
      TimeSpan duration = Duration ?? GetDefaultDuration();

      // ----- FillBehavior
      if (time > duration)
      {
        if (FillBehavior == FillBehavior.Stop)
        {
          // The animation has stopped.
          return AnimationState.Stopped;
        }

        // The animation holds the final value.
        Debug.Assert(FillBehavior == FillBehavior.Hold);
        return AnimationState.Filling;
      }

      return AnimationState.Playing;
    }


    /// <inheritdoc/>
    public virtual TimeSpan? GetAnimationTime(TimeSpan time)
    {
      // ----- Delay
      time -= Delay;
      if (time < TimeSpan.Zero)
      {
        // The animation has not started.
        return null;
      }

      // ----- Speed
      time = new TimeSpan((long)(time.Ticks * (double)Speed));

      // ----- Duration
      TimeSpan defaultDuration = GetDefaultDuration();
      TimeSpan duration = Duration ?? defaultDuration;

      // ----- FillBehavior
      if (time > duration)
      {
        if (FillBehavior == FillBehavior.Stop)
        {
          // The animation has stopped.
          return null;
        }

        // The animation holds the last value.
        Debug.Assert(FillBehavior == FillBehavior.Hold);
        time = duration;
      }

      // ----- Loop animation time.
      AnimationHelper.LoopParameter(time, TimeSpan.Zero, defaultDuration, LoopBehavior, out time);

      return time;
    }


    /// <summary>
    /// Adjusts the time on the timeline in case the blend weights have changed. (This is necessary
    /// to keep the cycles in sync.)
    /// </summary>
    /// <param name="time">The time on the timeline.</param>
    /// <param name="duration">
    /// In/Out: The synchronized duration. <see cref="TimeSpan.Zero"/> if the animations are not
    /// synchronized.
    /// </param>
    /// <returns>The adjusted time on the timeline.</returns>
    internal TimeSpan AdjustTimeline(TimeSpan time, ref TimeSpan duration)
    {
      Update();

      if (_synchronized
          && duration != _synchronizedDuration
          && duration > TimeSpan.Zero
          && (LoopBehavior == LoopBehavior.Cycle || LoopBehavior == LoopBehavior.Oscillate))
      {
        double cycles = (double)time.Ticks / duration.Ticks;
        time = new TimeSpan((long)(_synchronizedDuration.Ticks * cycles));
      }

      duration = _synchronizedDuration;
      return time;
    }


    /// <inheritdoc/>
    public TimeSpan GetTotalDuration()
    {
      float speed = Speed;
      if (Numeric.IsZero(speed))
        return TimeSpan.MaxValue;

      TimeSpan delay = Delay;

      TimeSpan duration = Duration ?? GetDefaultDuration();
      if (duration == TimeSpan.MaxValue)
        return TimeSpan.MaxValue;

      return new TimeSpan(delay.Ticks + (long)(duration.Ticks / (double)speed));
    }


    /// <overloads>
    /// <summary>
    /// Gets the weight of a timeline.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the blend weight of the specified timeline.
    /// </summary>
    /// <param name="timeline">The timeline.</param>
    /// <returns>The blend weight of the timeline.</returns>
    /// <exception cref="ArgumentException">
    /// The <see cref="BlendGroup"/> does not contain the specified timeline.
    /// </exception>
    public float GetWeight(ITimeline timeline)
    {
      int index = IndexOf(timeline);
      if (index == -1)
        throw new ArgumentException("The BlendGroup does not contain the specified timeline.", "timeline");

      return _entries.Array[index].Weight.Value;
    }


    /// <summary>
    /// Gets the blend weight of the timeline with the specified index.
    /// </summary>
    /// <param name="index">The index of the timeline in the <see cref="BlendGroup"/>.</param>
    /// <returns>The blend weight of the timeline.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or equal to or greater than <see cref="Count"/>.
    /// </exception>
    public float GetWeight(int index)
    {
      if ((uint)index >= (uint)_entries.Count)
        throw new ArgumentOutOfRangeException("index");

      return _entries.Array[index].Weight.Value;
    }


    /// <overloads>
    /// <summary>
    /// Gets the blend weight of a timeline as an <see cref="IAnimatableProperty{T}"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the blend weight of the specified timeline as an <see cref="IAnimatableProperty{T}"/>.
    /// </summary>
    /// <param name="timeline">The timeline.</param>
    /// <returns>
    /// The blend weight of the timeline as an <see cref="IAnimatableProperty{T}"/>.
    /// </returns>
    /// <remarks>
    /// The returned <see cref="IAnimatableProperty{T}"/> can be used to animate the blend weight.
    /// It can also be used to directly read and write the blend weight.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// The <see cref="BlendGroup"/> does not contain the specified timeline.
    /// </exception>
    public IAnimatableProperty<float> GetWeightAsAnimatable(ITimeline timeline)
    {
      int index = IndexOf(timeline);
      if (index == -1)
        throw new ArgumentException("The BlendGroup does not contain the specified timeline.", "timeline");

      return _entries.Array[index].Weight;
    }


    /// <summary>
    /// Gets the blend weight of the timeline with the specified index as an 
    /// <see cref="IAnimatableProperty{T}"/>.
    /// </summary>
    /// <param name="index">The index of the timeline in the <see cref="BlendGroup"/>.</param>
    /// <returns>
    /// The blend weight of the timeline as an <see cref="IAnimatableProperty{T}"/>.
    /// </returns>
    /// <remarks>
    /// The returned <see cref="IAnimatableProperty{T}"/> can be used to animate the blend weight.
    /// It can also be used to directly read and write the blend weight.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or equal to or greater than <see cref="Count"/>.
    /// </exception>
    public IAnimatableProperty<float> GetWeightAsAnimatable(int index)
    {
      if ((uint)index >= (uint)_entries.Count)
        throw new ArgumentOutOfRangeException("index");

      return _entries.Array[index].Weight;
    }


    /// <overloads>
    /// <summary>
    /// Sets the blend weight of a timeline.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets the blend weight of the specified timeline.
    /// </summary>
    /// <param name="timeline">The timeline.</param>
    /// <param name="weight">The blend weight of the timeline.</param>
    /// <exception cref="ArgumentException">
    /// The <see cref="BlendGroup"/> does not contain the specified timeline.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="weight"/> is negative.
    /// </exception>
    public void SetWeight(ITimeline timeline, float weight)
    {
      int index = IndexOf(timeline);
      if (index == -1)
        throw new ArgumentException("The BlendGroup does not contain the specified timeline.", "timeline");

      SetWeight(index, weight);
    }


    /// <summary>
    /// Sets the blend weight of the timeline at the specified index.
    /// </summary>
    /// <param name="index">The index of the timeline in the <see cref="BlendGroup"/>.</param>
    /// <param name="weight">The blend weight of the timeline.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or equal to or greater than <see cref="Count"/>. Or,
    /// <paramref name="weight"/> is negative.
    /// </exception>
    public void SetWeight(int index, float weight)
    {
      if ((uint)index >= (uint)_entries.Count)
        throw new ArgumentOutOfRangeException("index");
      if (weight < 0.0f)
        throw new ArgumentOutOfRangeException("weight", "The weight of a timeline must not be negative.");

      _entries.Array[index].Weight.Value = weight;

      // Note: AnimatableBlendWeight automatically calls OnWeightChanged().
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<ITimeline> IEnumerable<ITimeline>.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="BlendGroup"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="BlendGroup"/>.
    /// </returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }


    /// <overloads>
    /// <summary>
    /// Adds a timeline to the <see cref="BlendGroup"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Adds a timeline with a blend weight of 1 to the <see cref="BlendGroup"/>.
    /// </summary>
    /// <param name="timeline">The timeline to add to the <see cref="BlendGroup"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="timeline"/> is <see langword="null"/>. The <see cref="BlendGroup"/> does 
    /// not allow <see langword="null"/> values.
    /// </exception>
    public void Add(ITimeline timeline)
    {
      if (timeline == null)
        throw new ArgumentNullException("timeline");

      int index = _entries.Count;
      var entry = new Entry
      {
        Timeline = timeline,
        Weight = new AnimatableBlendWeight(this, 1.0f),
        NormalizedWeight = 1.0f,
        CachedDuration = TimeSpan.Zero,
        TimeNormalizationFactor = 1.0f
      };
      _entries.Add(ref entry);

      OnTimelineChanged(index);
    }


    /// <summary>
    /// Adds a timeline with the specified blend weight to the <see cref="BlendGroup"/>.
    /// </summary>
    /// <param name="timeline">The timeline to add to the <see cref="BlendGroup"/>.</param>
    /// <param name="weight">The blend weight of the timeline.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="timeline"/> is <see langword="null"/>. The <see cref="BlendGroup"/> does 
    /// not allow <see langword="null"/> values.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="weight"/> is negative.
    /// </exception>
    public void Add(ITimeline timeline, float weight)
    {
      if (timeline == null)
        throw new ArgumentNullException("timeline");
      if (weight < 0.0f)
        throw new ArgumentOutOfRangeException("weight", "The weight of a timeline must not be negative.");

      int index = _entries.Count;
      var entry = new Entry
      {
        Timeline = timeline,
        Weight = new AnimatableBlendWeight(this, weight),
        NormalizedWeight = weight,
        CachedDuration = TimeSpan.Zero,
        TimeNormalizationFactor = 1.0f
      };
      _entries.Add(ref entry);

      OnTimelineChanged(index);
    }


    /// <summary>
    /// Removes all timelines from the <see cref="BlendGroup"/>.
    /// </summary>
    public void Clear()
    {
      _entries.Clear();
      OnTimelineChanged(-1);
    }


    /// <summary>
    /// Determines whether the <see cref="BlendGroup"/> contains a specific timeline.
    /// </summary>
    /// <param name="timeline">The timeline to locate in the <see cref="BlendGroup"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="timeline"/> is found in the 
    /// <see cref="BlendGroup"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(ITimeline timeline)
    {
      return IndexOf(timeline) >= 0;
    }


    /// <summary>
    /// Copies the elements of the <see cref="BlendGroup"/> to an <see cref="Array"/>, starting 
    /// at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="BlendGroup"/>. The <see cref="Array"/> must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="array"/> is multidimensional. Or <paramref name="arrayIndex"/> is equal to 
    /// or greater than the length of <paramref name="array"/>. Or the number of elements in the 
    /// source <see cref="BlendGroup"/> is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    void ICollection<ITimeline>.CopyTo(ITimeline[] array, int arrayIndex)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (arrayIndex < 0)
        throw new ArgumentOutOfRangeException("arrayIndex", "Array index must be equal to or greater than 0.");

      var entries = _entries.Array;
      int numberOfEntries = _entries.Count;

      if (array.Length - arrayIndex < numberOfEntries)
        throw new ArgumentException("Destination array cannot hold the requested elements!");

      for (int i = 0; i < numberOfEntries; i++, arrayIndex++)
        array[arrayIndex] = entries[i].Timeline;
    }


    /// <summary>
    /// Determines the index of a specific timeline in the <see cref="BlendGroup"/>.
    /// </summary>
    /// <param name="timeline">The timeline to locate in the <see cref="BlendGroup"/>.</param>
    /// <returns>
    /// The index of <paramref name="timeline"/> if found in the <see cref="BlendGroup"/>;
    /// otherwise, -1.
    /// </returns>
    public int IndexOf(ITimeline timeline)
    {
      var entries = _entries.Array;
      int numberOfEntries = _entries.Count;
      for (int i = 0; i < numberOfEntries; i++)
        if (entries[i].Timeline == timeline)
          return i;

      return -1;
    }


    /// <summary>
    /// Inserts a timeline into the <see cref="BlendGroup"/> at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="timeline"/> should be inserted.
    /// </param>
    /// <param name="timeline">
    /// The timeline to insert into the <see cref="BlendGroup"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="BlendGroup"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="timeline"/> is <see langword="null"/>. The <see cref="BlendGroup"/> does 
    /// not allow <see langword="null"/> values.
    /// </exception>
    public void Insert(int index, ITimeline timeline)
    {
      if (timeline == null)
        throw new ArgumentNullException("timeline");

      var entry = new Entry
      {
        Timeline = timeline,
        Weight = new AnimatableBlendWeight(this, 1.0f),
        NormalizedWeight = 1.0f,
        CachedDuration = TimeSpan.Zero,
        TimeNormalizationFactor = 1.0f
      };
      _entries.Insert(index, ref entry);

      OnTimelineChanged(index);
    }


    /// <summary>
    /// Removes the first occurrence of a specific timeline from the <see cref="BlendGroup"/>.
    /// </summary>
    /// <param name="timeline">The timeline to remove from the <see cref="BlendGroup"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="timeline"/> was successfully removed from the 
    /// <see cref="BlendGroup"/>; otherwise, <see langword="false"/>. This method also returns 
    /// <see langword="false"/> if <paramref name="timeline"/> is not found in the original 
    /// <see cref="BlendGroup"/>.
    /// </returns>
    public bool Remove(ITimeline timeline)
    {
      int index = IndexOf(timeline);
      if (index >= 0)
      {
        _entries.RemoveAt(index);
        OnTimelineChanged(-1);
        return true;
      }

      return false;
    }


    /// <summary>
    /// Removes the timeline at the specified index from the <see cref="BlendGroup"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the timeline to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="BlendGroup"/>.
    /// </exception>
    public void RemoveAt(int index)
    {
      _entries.RemoveAt(index);
      OnTimelineChanged(-1);
    }
    #endregion


    //--------------------------------------------------------------
    #region IAnimatableObject
    //--------------------------------------------------------------

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <value>
    /// Not implemented. Always returns <see cref="String.Empty"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    string INamedObject.Name
    {
      get { return string.Empty; }
    }


    /// <summary>
    /// Gets the properties which are currently being animated.
    /// </summary>
    /// <returns>
    /// The properties which are currently being animated.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IEnumerable<IAnimatableProperty> IAnimatableObject.GetAnimatedProperties()
    {
      var entries = _entries.Array;
      int numberOfEntries = _entries.Count;
      for (int i = 0; i < numberOfEntries; i++)
      {
        var property = (IAnimatableProperty<float>)entries[i].Weight;
        if (property.IsAnimated)
          yield return property;
      }
    }


    /// <summary>
    /// Gets the property with given name and type which can be animated.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>
    /// The <see cref="IAnimatableProperty"/> that has the given name and type; otherwise, 
    /// <see langword="null"/> if the object does not have an property with this name or type.
    /// </returns>
    /// <remarks>
    /// The blend weights in a blend group can be animated. The blend weights are identified using 
    /// the strings "Weight0", Weight1", etc., where the suffix is the index of the timeline.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IAnimatableProperty<T> IAnimatableObject.GetAnimatableProperty<T>(string name)
    {
      string propertyName;
      int index;
      name.SplitTextAndNumber(out propertyName, out index);
      if (propertyName == "Weight" && 0 <= index && index < _entries.Count)
      {
        return _entries.Array[index].Weight as IAnimatableProperty<T>;
      }

      return null;
    }
    #endregion
  }
}
