// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Mathematics;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Represents an instance of an animation timeline.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Animation instances are used to play back animations. They maintain the runtime-state of an 
  /// animation. 
  /// </para>
  /// <para>
  /// Animation instances are automatically created and managed by the animation system when a new 
  /// animation is started using one of the <strong>StartAnimation</strong>-methods or when an 
  /// animation controller is created using one of the <strong>CreateController</strong>-methods 
  /// (see <see cref="IAnimationService"/>). 
  /// </para>
  /// <para>
  /// <strong>Animation Value:</strong> An animation usually produces a value as the output. The
  /// base class <see cref="AnimationInstance"/> manages the timeline of the animation. The derived 
  /// class <see cref="AnimationInstance{T}"/> manages the animation value. The animation system
  /// automatically applies the output values to the properties that are being animated (see 
  /// <see cref="IAnimatableProperty{T}"/>).
  /// </para>
  /// <para>
  /// <strong>Animation State and Timing:</strong> The property <see cref="State"/> returns the 
  /// current state of the animation. The animation can be <see cref="AnimationState.Delayed"/>, 
  /// <see cref="AnimationState.Playing"/>, <see cref="AnimationState.Filling"/> or 
  /// <see cref="AnimationState.Stopped"/>. But the state does not indicate whether the animation 
  /// timing is active. The property <see cref="IsPaused"/> indicates whether the animation timing 
  /// is active or paused.
  /// </para>
  /// <para>
  /// Active animations are managed by the <see cref="AnimationManager"/>. The animation system 
  /// automatically advances and updates the animations.
  /// </para>
  /// <para>
  /// The current state of an animation is computed once per frame (in 
  /// <see cref="AnimationManager.Update"/>) and cached until the next frame. The animation instance
  /// does not monitor the animation for changes. I.e. when the animation is modified the animation 
  /// instance needs to be notified by calling <see cref="Invalidate"/>. Otherwise, it can return an 
  /// invalid state in the current frame. For example, an animation instance plays a certain 
  /// <see cref="TimelineClip"/>. During playback the user changes the 
  /// <see cref="TimelineClip.Delay"/> of the animation. Now, when the user reads 
  /// <see cref="State"/> in the same frame the instance might return the wrong value. The user 
  /// needs to call <see cref="Invalidate"/> to get the correct, up-to-date value. (It is not 
  /// necessary to call <see cref="Invalidate"/> if the animations are updated using 
  /// <see cref="AnimationManager.Update"/>. <see cref="AnimationManager.Update"/>
  /// automatically computes the new values.)
  /// </para>
  /// <para>
  /// <strong>Animation Tree:</strong> Animation instances can have children. For example, when a 
  /// <see cref="TimelineGroup"/> is started it creates a root instance that has several children - 
  /// one animation instance per animation in the timeline group. A timeline group might contain 
  /// other timeline groups. The animation instances are organized in a tree structure.
  /// </para>
  /// <para>
  /// Only the root instance of a tree can be controlled interactively (using an 
  /// <see cref="AnimationController"/>).
  /// </para>
  /// <para>
  /// <strong>Speed Ratio:</strong> The playback speed of the animation tree can be controlled by
  /// changing the property <see cref="Speed"/>. The speed ratio defines the rate at which time 
  /// progresses. The default value is 1. A value of, for example, 2 means that the animation runs 
  /// twice as fast. A value of 0.5 causes the animation to run in slow-motion at half speed.
  /// </para>
  /// <para>
  /// Note that the only the speed ratio of the root instance in the animation tree can be 
  /// controlled. (Changing the speed ratio of other nodes in the animation tree has no effect.)
  /// </para>
  /// <para>
  /// <strong>Animation Weights:</strong> Each animation instance has a weight (see 
  /// <see cref="Weight"/>) that defines the intensity of the animation. It is a factor that is 
  /// applied to the animation output. The animation weight is in particular relevant when multiple
  /// animations should be combined. Each animation combines its output with the output of the
  /// previous stage in the animation composition chain. (If the animation is the first animation of
  /// a composition chain it combines its value with the base value of the property that is being
  /// animated.)
  /// </para>
  /// <para>
  /// The default value is 1, which means that 100% of the animation is returned overriding any 
  /// previous stage in an animation composition chain. A value of 0.75 means that result is
  /// weighted combination of the previous stage (25%) and the output of the current animation
  /// (75%). A value of 0 basically disables the output of the current stage.
  /// </para>
  /// <para>
  /// Changing the animation weight of an instance affects the entire subtree: The current animation
  /// instance and all children. The effective animation weight is the product of all weights from
  /// the root instance to the current animation instance.
  /// </para>
  /// <para>
  /// <strong>Secondary Animations:</strong> An animation instance itself is an 
  /// <see cref="IAnimatableObject"/>, which means that it has properties which can be animated.
  /// The properties that can be animated are <see cref="Speed"/> and <see cref="Weight"/>. 
  /// Secondary animation can be used, for example, to fade-in an animation by letting the animation
  /// weight go from 0 to 1 over time.
  /// </para>
  /// </remarks>
  public class AnimationInstance : IAnimatableObject, IRecyclable
  {
    // Ideas for new methods:
    //   SkipToEnd() ... see SkipToFill() in WPF.
    //   Seek() ... see Seek() in WPF.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static readonly ResourcePool<AnimationInstance> Pool = new ResourcePool<AnimationInstance>(
      () => new AnimationInstance(),  // Create
      null,                           // Initialize
      null);                          // Uninitialize
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value that indicates the version of the animation instance.
    /// </summary>
    /// <value>The version of the animation instance.</value>
    /// <remarks>
    /// This number is automatically incremented every time the animation instance is recycled.
    /// </remarks>
    internal int RunCount
    {
      get { return _runCount; }
    }
    private int _runCount;


    /// <summary>
    /// Gets the parent of this animation instance.
    /// </summary>
    /// <value>
    /// The parent of this animation instance; <see langword="null"/>, if the current instance does 
    /// not have a parent.
    /// </value>
    public AnimationInstance Parent
    {
      get { return _parent; }
      internal set { _parent = value; }
    }
    private AnimationInstance _parent;


    /// <summary>
    /// Gets the children of this animation instance.
    /// </summary>
    /// <value>The children of this animation instance.</value>
    public AnimationInstanceCollection Children
    {
      get { return _children; }
    }
    private readonly AnimationInstanceCollection _children;


    /// <summary>
    /// Gets the animation timeline that is being played back.
    /// </summary>
    /// <value>The animation timeline that is being played back.</value>
    public ITimeline Animation
    {
      get { return _animation; }
    }
    private ITimeline _animation;


    /// <summary>
    /// Gets or sets a value indicating whether this animation instance should be automatically
    /// recycled when it is stopped and removed from the animation system.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the animation instance is recycled automatically; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// See <see cref="AnimationController"/> for more information.
    /// </remarks>
    public bool AutoRecycleEnabled
    {
      get { return _autoRecycleEnabled; }
      set { _autoRecycleEnabled = value; }
    }
    private bool _autoRecycleEnabled;


    /// <summary>
    /// Gets a value indicating whether this animation instance is paused.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this animation instance (or any of its ancestors in the animation
    /// tree) is paused; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsPaused
    {
      get
      {
        // Only the root instance can be paused.
        return (Parent != null) ? Parent.IsPaused : _isPaused;

        // Alternatively, if the animation tree can be paused at any level.
        // (But this is currently not supported.)
        //return _isPaused || (Parent != null) ? Parent.IsPaused : false;
      }
    }
    private bool _isPaused;


    /// <summary>
    /// Gets a value indicating whether this animation instance is the root node in the animation 
    /// tree.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if animation instance is the root node in the animation tree; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    internal bool IsRoot
    {
      get { return _parent == null; }
    }


    /// <summary>
    /// Gets the current state of the animation.
    /// </summary>
    /// <value>The current state of the animation.</value>
    public AnimationState State
    {
      get
      {
        UpdateState();
        return _state;
      }
    }
    private AnimationState _state;
    private bool _isStateValid;


    /// <summary>
    /// Gets or sets the current animation time.
    /// </summary>
    /// <value>
    /// The current animation time. (The animation time is <see langword="null"/> if the instance 
    /// has not been started.)
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Cannot set animation time because this is not the root instance. The animation instance is 
    /// a node in the animation tree, but only the root node of the tree can be controlled directly.
    /// </exception>
    public TimeSpan? Time
    {
      get { return _time; }
      set
      {
        if (!IsRoot)
        {
          throw new InvalidOperationException(
            "Cannot set animation time because it is not the root instance. "
            + "The animation instance is a node in the animation tree, "
            + "but only the root node of the tree can be controlled directly.");
        }

        if (_time != value)
          SetTime(value);
      }
    }
    private TimeSpan? _time;


    /// <summary>
    /// Gets the <see cref="Speed"/> property as an <see cref="IAnimatableProperty{T}"/>.
    /// </summary>
    /// <value>
    /// The <see cref="Speed"/> property as an <see cref="IAnimatableProperty{T}"/>.
    /// </value>
    internal IAnimatableProperty<float> SpeedProperty
    {
      get { return _speedProperty; }
    }
    private readonly ImmediateAnimatableProperty<float> _speedProperty;


    /// <summary>
    /// Gets or sets the rate at which time progresses.
    /// </summary>
    /// <value>The speed ratio. The default value is 1.</value>
    /// <remarks>
    /// <para>
    /// The speed ratio defines the rate at which time progresses on the timeline. The default value
    /// is 1. A value of, for example, 2 means that the animation runs twice as fast. A value of
    /// 0.5 causes the animation to run in slow-motion with half speed.
    /// </para>
    /// <para>
    /// Note that the only the speed ratio of the root instance in the animation tree can be 
    /// controlled. (Changing the speed ratio of other nodes in the animation tree has no effect.)
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is NaN or infinity.
    /// </exception>
    public float Speed
    {
      get { return _speedProperty.Value; }
      set
      {
        if (!Numeric.IsFinite(value))
          throw new ArgumentOutOfRangeException("value", "The speed ratio must be a finite value.");

        _speedProperty.Value = value;
      }
    }


    /// <summary>
    /// Gets the <see cref="Weight"/> property as an <see cref="IAnimatableProperty{T}"/>.
    /// </summary>
    /// <value>
    /// The <see cref="Weight"/> property as an <see cref="IAnimatableProperty{T}"/>.
    /// </value>
    internal IAnimatableProperty<float> WeightProperty
    {
      get { return _weightProperty; }
    }
    private readonly ImmediateAnimatableProperty<float> _weightProperty;


    /// <summary>
    /// Gets or sets the animation weight.
    /// </summary>
    /// <value>
    /// The animation weight. The value lies in the range [0, 1]. The default value is 1.
    /// </value>
    /// <remarks>
    /// <para>
    /// The animation weight defines the intensity of the animation. It is a factor that is applied 
    /// to the animation output. The animation weight is in particular relevant when multiple 
    /// animations should be combined. Each animation combines its output with the output of the 
    /// previous stage in the animation composition chain. (If the animation is the first animation 
    /// of a composition chain it combines its value with the base value of the property that is 
    /// being animated.)
    /// </para>
    /// <para>
    /// The default value is 1 which means that 100% of the animation is returned, overriding any 
    /// previous stage in a animation composition chain. A value of 0.75 means that result is
    /// weighted combination of the previous stage (25%) and the output of the current animation
    /// (75%). A value of 0 basically disables the output of the current animation.
    /// </para>
    /// <para>
    /// Changing the animation weight of an instance affects the entire subtree: the current 
    /// animation instance and all children. The effective animation weight is the product of all 
    /// weights from the root node to the current animation instance.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or greater than 1.
    /// </exception>
    public float Weight
    {
      get { return _weightProperty.Value; }
      set
      {
        // The following check is only applied when the value is set explicitly. The check 
        // is not applied when the animation value is set. (Note: The animation value can be 
        // can be outside of the range [0, 1]. Some easing functions produce values outside 
        // this range.)
        if (value < 0.0f || value > 1.0f || Numeric.IsNaN(value))
          throw new ArgumentOutOfRangeException("value", "The animation weight must be a value in the range [0, 1].");

        _weightProperty.Value = value;
      }
    }


    /// <summary>
    /// Occurs when the animation has completed playing. Use with caution - see remarks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A root instance has completed playing when the root timeline has reached the end of its 
    /// duration (including any repeats). A child instance is considered to have finished playing 
    /// when the root instance has finished playing.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> The completion event does not trigger when animation is 
    /// explicitly stopped or removed before it has reached the end of its duration. (But the 
    /// completion event is triggered when the user sets the <see cref="Time"/> to a value past the 
    /// end of the duration.)
    /// </para>
    /// <para>
    /// The completion event is not raised immediately when the state of the animation changes. 
    /// Instead, the <see cref="AnimationManager"/> records all animations that have finished 
    /// playing in <see cref="AnimationManager.Update"/> and explicitly raises the completion events
    /// in <see cref="AnimationManager.ApplyAnimations"/>.
    /// </para>
    /// <para>
    /// <strong>Use with Caution:</strong> The animation system uses weak references to ensure that
    /// animations do not accidentally keep the animated objects and properties alive. Animations 
    /// are automatically removed if the animated objects are removed (i.e. garbage collected). But 
    /// the <see cref="Completed"/> event stores the event handlers using a strong reference. If an 
    /// event handler keeps the animation objects or properties alive, then the animation system 
    /// will not be able to automatically remove the animation and the referenced resources. To 
    /// ensure that all resources are properly freed, make sure that one of the following conditions 
    /// is true:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// The completion event handlers do not keep the target object or properties that are being
    /// animated alive.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The completion event handlers are removed explicitly if they are no longer required.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The animation is stopped explicitly when it is no longer needed, e.g. by calling 
    /// <see cref="AnimationController.Stop()"/>).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The animation is stopped implicitly, e.g. by starting a new animation which replaces it.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The animation has a limited duration and stops automatically. (The 
    /// <see cref="ITimeline.FillBehavior"/> needs to be set to <see cref="FillBehavior.Stop"/>.)
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public event EventHandler<EventArgs> Completed;


    /// <summary>
    /// Gets a value indicating whether the animation service needs to call 
    /// <see cref="RaiseCompletedEvent"/> when the animation completes.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the animation service needs to call 
    /// <see cref="RaiseCompletedEvent"/>; otherwise, <see langword="false"/>.
    /// </value>
    internal bool RequiresCompletedEvent
    {
      get { return Completed != null; }
    }
    #endregion
    

    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationInstance"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    protected AnimationInstance()
    {
      // Initialize RunCount with 1. (0 is invalid.)
      _runCount = 1;

      _state = AnimationState.Stopped;
      _time = null;
      _speedProperty = new ImmediateAnimatableProperty<float> { Value = 1.0f };
      _weightProperty = new ImmediateAnimatableProperty<float> { Value = 1.0f };

      // ReSharper disable DoNotCallOverridableMethodsInConstructor
      _children = CreateChildCollection();
      // ReSharper restore DoNotCallOverridableMethodsInConstructor
    }


    /// <summary>
    /// Creates an instance of the <see cref="AnimationInstance"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <param name="animation">The animation timeline.</param>
    /// <returns>
    /// A new or reusable instance of the <see cref="AnimationInstance"/> class.
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
    /// <paramref name="animation"/> is <see langword="null"/>.
    /// </exception>
    public static AnimationInstance Create(ITimeline animation)
    {
      var animationInstance = Pool.Obtain();
      animationInstance.Initialize(animation);
      return animationInstance;
    }


    /// <summary>
    /// Recycles this animation instance (including all children).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance (and all children) and returns it to a resource pool if 
    /// resource pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    public virtual void Recycle()
    {
      // Recursively recycle animation tree.
      foreach (var child in Children)
        child.Recycle();

      Reset();

      if (RunCount < int.MaxValue)
        Pool.Recycle(this);
    }


    /// <summary>
    /// Initializes the animation instance.
    /// </summary>
    /// <param name="animation">The animation timeline.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animation"/> is <see langword="null"/>.
    /// </exception>
    internal void Initialize(ITimeline animation)
    {
      Debug.Assert(_animation == null, "Animation instance has not been properly reset.");
      Debug.Assert(_autoRecycleEnabled == false, "Animation instance has not been properly reset.");
      Debug.Assert(_isPaused == false, "Animation instance has not been properly reset.");
      Debug.Assert(_isStateValid == false, "Animation instance has not been properly reset.");
      Debug.Assert(_parent == null, "Animation instance has not been properly reset.");
      Debug.Assert(_state == AnimationState.Stopped, "Animation instance has not been properly reset.");
      Debug.Assert(_time == null, "Animation instance has not been properly reset.");
      Debug.Assert(_speedProperty.Value == 1.0f, "Animation instance has not been properly reset.");
      Debug.Assert(_weightProperty.Value == 1.0f, "Animation instance has not been properly reset.");
      Debug.Assert(_children.Count == 0, "Animation instance has not been properly reset.");
      Debug.Assert(_runCount > 0, "Animation instance has invalid RunCount.");
      Debug.Assert(Completed == null, "Animation instance has invalid Completed.");
      
      if (animation == null)
        throw new ArgumentNullException("animation");

      _animation = animation;
    }


    /// <summary>
    /// Resets this animation instance.
    /// </summary>
    internal void Reset()
    {
      Debug.Assert(SpeedProperty.IsAnimated == false, "The speed ratio is still animated. Make sure that all animations are stopped before recycling an animation instance!");
      Debug.Assert(WeightProperty.IsAnimated == false, "The animation weight is still animated. Make sure that all animations are stopped before recycling an animation instance!");
      Debug.Assert(State == AnimationState.Stopped, "Animation instance is still running.");

      _animation = null;
      _autoRecycleEnabled = false;
      _isPaused = false;
      _isStateValid = false;
      _parent = null;
      _state = AnimationState.Stopped;
      _time = null;
      _speedProperty.Value = 1.0f;
      _weightProperty.Value = 1.0f;
      _children.Clear();
      Completed = null;

      // The animation instance requires a new ID: Increment RunCount.
      _runCount++;
    }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Creates the child collection of this animation instance.
    /// </summary>
    /// <returns>The child collection to be used by this animation instance.</returns>
    internal virtual AnimationInstanceCollection CreateChildCollection()
    {
      return new AnimationInstanceCollection(this);
    }


    /// <summary>
    /// Gets the effective animation weight.
    /// </summary>
    /// <returns>The effective animation weight.</returns>
    /// <remarks>
    /// The effective animation weight is the product of all animation weights (from the root 
    /// instance in the animation tree to current animation instance).
    /// </remarks>
    internal float GetEffectiveWeight()
    {
      float weight = Weight;
      var animationInstance = Parent;
      while (animationInstance != null)
      {
        weight *= animationInstance.Weight;
        animationInstance = animationInstance.Parent;
      }

      return weight;
    }


    /// <summary>
    /// Advances the animation time by the given time.
    /// </summary>
    /// <param name="deltaTime">The elapsed time.</param>
    /// <remarks>
    /// Calling this method has no effect when the animation instance or any of its ancestors in the 
    /// animation tree is paused.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot advance animation because this is not the root instance. The animation instance is a 
    /// node in the animation tree, but only the root node of the tree can be controlled directly.
    /// </exception>
    internal void AdvanceTime(TimeSpan deltaTime)
    {
      if (!IsRoot)
      {
        throw new InvalidOperationException(
          "Cannot advance animation because it is not the root instance. "
          + "The animation instance is a node in the animation tree, "
          + "but only the root node of the tree can be interactively controlled.");
      }

      if (_time == null)
        _time = TimeSpan.Zero;

      if (!IsPaused)
      {
        // Apply speed ratio.
        deltaTime = new TimeSpan((long)(deltaTime.Ticks * Speed));

        SetTime(_time + deltaTime);
      }
    }


    /// <summary>
    /// Sets the animation time of the current animation instance and updates all children.
    /// </summary>
    /// <param name="time">The time value to set.</param>
    internal virtual void SetTime(TimeSpan? time)
    {
      _time = time;

      int numberOfChildren = Children.Count;
      if (numberOfChildren > 0)
      {
        if (time.HasValue)
        {
          // Convert time to local animation time.
          time = Animation.GetAnimationTime(time.Value);
        }

        // Update children.
        for (int i = 0; i < numberOfChildren; i++)
        {
          var child = Children[i];
          child.SetTime(time);
        }
      }

      // Invalidate this instance.
      // (Note: Children are automatically invalidated when their animation time changes.)
      InvalidateState();
    }


    /// <summary>
    /// Invalidates the current state of the animation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method needs to be called manually if the animation data (such as the begin time of an 
    /// animation) is changed and other objects want to read the animation state in the same frame.
    /// If an animation instance has been invalidated, it will automatically recompute its state
    /// when <see cref="State"/> is read.
    /// </para>
    /// <para>
    /// In is not necessary to explicitly call this method if <see cref="AnimationManager.Update"/> 
    /// is called. In this case the state of the animation will be recomputed automatically.
    /// </para>
    /// </remarks>
    public void Invalidate()
    {
      InvalidateState();

      foreach (var animationInstance in Children)
        animationInstance.Invalidate();
    }


    private void InvalidateState()
    {
      _isStateValid = false;
    }


    internal void UpdateState()
    {
      if (!_isStateValid)
      {
        _state = _time.HasValue ? Animation.GetState(_time.Value) : AnimationState.Stopped;
        _isStateValid = true;
      }
    }


    /// <summary>
    /// Stops the animation from progressing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method stops the animation timing. The animation no longer progresses when 
    /// <see cref="AdvanceTime"/> is called. The timing can be resumed by calling 
    /// <see cref="Resume"/>.
    /// </para>
    /// <para>
    /// Pausing an animation instance implicitly pauses all child instances.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot pause animation because this is not the root instance. The animation instance is a 
    /// node in the animation tree, but only the root node of the tree can be controlled directly.
    /// </exception>
    internal void Pause()
    {
      if (!IsRoot)
      {
        throw new InvalidOperationException(
          "Cannot pause animation because it is not the root instance. "
          + "The animation instance is a node in the animation tree, "
          + "but only the root node of the tree can be interactively controlled.");
      }

      _isPaused = true;
    }


    /// <summary>
    /// Resumes an animation that was previously stopped.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resumes the animation timing from where it was stopped. The timing can be 
    /// stopped by calling <see cref="Pause"/>.
    /// </para>
    /// <para>
    /// Resuming an animation instance implicitly resumes all child instances.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot pause animation because this is not the root instance. The animation instance is a
    /// node in the animation tree, but only the root node of the tree can be controlled directly.
    /// </exception>
    internal void Resume()
    {
      if (!IsRoot)
      {
        throw new InvalidOperationException(
          "Cannot resume animation because it is not the root instance. "
          + "The animation instance is a node in the animation tree, "
          + "but only the root node of the tree can be interactively controlled.");
      }

      _isPaused = false;
    }


    /// <overloads>
    /// <summary>
    /// Determines whether the animation tree can be assigned to the given objects or properties.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether this animation tree can be assigned to the specified set of objects.
    /// </summary>
    /// <param name="objects">The set of animatable objects.</param>
    /// <returns>
    /// <see langword="true"/> if this animation instance (or one of its children) can be assigned 
    /// to <paramref name="objects"/>; otherwise, <see langword="false"/>.
    /// </returns>
    internal virtual bool IsAssignableTo(IEnumerable<IAnimatableObject> objects)
    {
      string objectName = Animation.TargetObject;
      if (String.IsNullOrEmpty(objectName))
      {
        // No target object set. All objects are potential targets.
        foreach (var child in Children)
          if (child.IsAssignableTo(objects))
            return true;
      }
      else
      {
        // Find target object.
        IAnimatableObject targetObject = null;
        foreach (var obj in objects)
        {
          if (obj.Name == objectName)
          {
            targetObject = obj;
            break;
          }
        }

        if (targetObject != null)
        {
          // Check whether any child can be assigned to the target object.
          foreach (var child in Children)
            if (child.IsAssignableTo(targetObject))
              return true;
        }
      }

      return false;
    }


    /// <summary>
    /// Determines whether this animation tree can be assigned to the specified object.
    /// </summary>
    /// <param name="obj">The animatable object.</param>
    /// <returns>
    /// <see langword="true"/> if this animation instance (or one of its children) can be assigned 
    /// to <paramref name="obj"/>; otherwise, <see langword="false"/>.
    /// </returns>
    internal virtual bool IsAssignableTo(IAnimatableObject obj)
    {
      // Note: In this case we do not check whether Animation.TargetObject matches obj.Name.
      // This method should only be called to explicitly assign the animation to the given object.

      foreach (var child in Children)
        if (child.IsAssignableTo(obj))
          return true;

      return false;
    }


    /// <summary>
    /// Determines whether this animation tree can be assigned to the specified property.
    /// </summary>
    /// <param name="property">The animatable property.</param>
    /// <returns>
    /// <see langword="true"/> if this animation instance (or one of its children) can be assigned 
    /// to <paramref name="property"/>; otherwise, <see langword="false"/>.
    /// </returns>
    internal virtual bool IsAssignableTo(IAnimatableProperty property)
    {
      foreach (var child in Children)
        if (child.IsAssignableTo(property))
          return true;

      return false;
    }


    /// <summary>
    /// Determines whether this animation tree is assigned to the specified property.
    /// </summary>
    /// <param name="property">The animatable property.</param>
    /// <returns>
    /// <see langword="true"/> if this animation instance (or one of its children) is assigned to 
    /// <paramref name="property"/>; otherwise, <see langword="false"/>.
    /// </returns>
    internal virtual bool IsAssignedTo(IAnimatableProperty property)
    {
      foreach (var child in Children)
        if (child.IsAssignedTo(property))
          return true;

      return false;
    }


    /// <overloads>
    /// <summary>
    /// Assigns this animation tree to objects or properties.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Assigns this animation tree to the specified set of objects.
    /// </summary>
    /// <param name="objects">The collection of animatable object.</param>
    internal virtual void AssignTo(IEnumerable<IAnimatableObject> objects)
    {
      string objectName = Animation.TargetObject;
      if (String.IsNullOrEmpty(objectName))
      {
        // No target object set. All objects are potential targets.
        foreach (var child in Children)
          child.AssignTo(objects);
      }
      else
      {
        // Find target object.
        IAnimatableObject targetObject = null;
        foreach (var obj in objects)
        {
          if (obj.Name == objectName)
          {
            targetObject = obj;
            break;
          }
        }

        if (targetObject != null)
        {
          // Assign children to the selected object.
          foreach (var child in Children)
            child.AssignTo(targetObject);
        }
      }
    }


    /// <summary>
    /// Assigns this animation tree to the specified object.
    /// </summary>
    /// <param name="obj">The animatable object.</param>
    internal virtual void AssignTo(IAnimatableObject obj)
    {
      // Note: In this case we do not check whether Animation.TargetObject matches obj.Name.
      // This method should only be called to explicitly assign the animation to the given object.

      foreach (var child in Children)
        child.AssignTo(obj);
    }


    /// <summary>
    /// Assigns this animation tree to the specified property.
    /// </summary>
    /// <param name="property">The animatable property.</param>
    internal virtual void AssignTo(IAnimatableProperty property)
    {
      foreach (var child in Children)
        child.AssignTo(property);
    }


    ///// <summary>
    ///// Unassigns this animation trees. (Removes all references to the currently assigned
    ///// properties.)
    ///// </summary>
    //internal virtual void Unassign()
    //{
    //  foreach (var child in Children)
    //    child.Unassign();
    //}


    /// <summary>
    /// Checks whether this animation tree is assigned to a certain animated property and returns 
    /// the animation instance.
    /// </summary>
    /// <param name="property">The animatable property.</param>
    /// <returns>
    /// The animation instance that is assigned to <paramref name="property"/>. 
    /// <see langword="null"/> if neither this instance nor its children are assigned to the
    /// property. If multiple children are assigned to the property the last found child is 
    /// returned.
    /// </returns>
    internal virtual AnimationInstance GetAssignedInstance(IAnimatableProperty property)
    {
      for (int i = Children.Count - 1; i >= 0; i--)
      {
        var child = Children[i];
        var assignedInstance = child.GetAssignedInstance(property);
        if (assignedInstance != null)
          return assignedInstance;
      }

      return null;
    }


    /// <summary>
    /// Gets all assigned properties and stores them in the given list.
    /// </summary>
    /// <param name="properties">A list in which the target properties shall be stored.</param>
    internal virtual void GetAssignedProperties(List<IAnimatableProperty> properties)
    {
      foreach (var child in Children)
        child.GetAssignedProperties(properties);
    }


    /// <summary>
    /// Determines whether this animation tree is currently applied to any properties.
    /// </summary>
    /// <param name="animationManager">The <see cref="AnimationManager"/>.</param>
    /// <returns>
    /// <see langword="true"/> if this animation tree is applied to any properties; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    internal virtual bool IsActive(AnimationManager animationManager)
    {
      foreach (var child in Children)
        if (child.IsActive(animationManager))
          return true;

      return false;
    }


    /// <summary>
    /// Applies this animation tree to the assigned properties.
    /// </summary>
    /// <param name="animationManager">The <see cref="AnimationManager"/>.</param>
    /// <param name="handoffBehavior">
    /// A value indicating how the new animations interact with existing ones.
    /// </param>
    /// <param name="previousInstance">
    /// Optional: The animation instance after which this animation instance will be added in the 
    /// animation composition chain. If set to <see langword="null"/> this animation instance will 
    /// be appended at the end of the composition chain. This parameter is only relevant when 
    /// <paramref name="handoffBehavior"/> is <see cref="HandoffBehavior.Compose"/>. 
    /// </param>
    internal virtual void AddToCompositionChains(AnimationManager animationManager, HandoffBehavior handoffBehavior, AnimationInstance previousInstance)
    {
      foreach (var child in Children)
        child.AddToCompositionChains(animationManager, handoffBehavior, previousInstance);
    }


    /// <summary>
    /// Removes this animation tree from the animated properties.
    /// </summary>
    /// <param name="animationManager">The <see cref="AnimationManager"/>.</param>
    internal virtual void RemoveFromCompositionChains(AnimationManager animationManager)
    {
      foreach (var child in Children)
        child.RemoveFromCompositionChains(animationManager);
    }


    /// <summary>
    /// Stops all animations that affect the animation tree (such as animations that control the 
    /// speed ratios or animation weights).
    /// </summary>
    /// <param name="animationManager">The <see cref="AnimationManager"/>.</param>
    internal void StopSecondaryAnimations(AnimationManager animationManager)
    {
      if (((IAnimatableProperty<float>)_speedProperty).IsAnimated)
      {
        animationManager.StopAnimation(_speedProperty);
        animationManager.UpdateAndApplyAnimation(_speedProperty);
      }

      if (((IAnimatableProperty<float>)_weightProperty).IsAnimated)
      {
        animationManager.StopAnimation(_weightProperty);
        animationManager.UpdateAndApplyAnimation(_weightProperty);
      }

      foreach (var child in Children)
        child.StopSecondaryAnimations(animationManager);
    }


    /// <summary>
    /// Immediately evaluates the given animation instance and applies the new animation values.
    /// </summary>
    /// <param name="animationManager">The <see cref="AnimationManager"/>.</param>
    internal virtual void UpdateAndApply(AnimationManager animationManager)
    {
      foreach (var child in Children)
        child.UpdateAndApply(animationManager);
    }


    internal void RaiseCompletedEvent()
    {
      foreach (var child in Children)
        child.RaiseCompletedEvent();

      OnCompleted(EventArgs.Empty);
    }


    /// <summary>
    /// Raises the <see cref="Completed"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnCompleted"/> in a derived
    /// class, be sure to call the base class's <see cref="OnCompleted"/> method so that registered 
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnCompleted(EventArgs eventArgs)
    {
      var handler = Completed;

      if (handler != null)
        handler(this, eventArgs);
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
      if (((IAnimatableProperty<float>)_speedProperty).IsAnimated)
        yield return _speedProperty;

      if (((IAnimatableProperty<float>)_weightProperty).IsAnimated)
        yield return _weightProperty;
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
    /// An animation instance has two animatable properties: <see cref="Speed"/> and 
    /// <see cref="Weight"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IAnimatableProperty<T> IAnimatableObject.GetAnimatableProperty<T>(string name)
    {
      switch (name)
      {
        case "Speed":
          return _speedProperty as IAnimatableProperty<T>;
        case "Weight":
          return _weightProperty as IAnimatableProperty<T>;
        default:
          return null;
      }
    }
    #endregion
  }
}
