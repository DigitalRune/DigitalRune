// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Animation.Transitions;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Provides interactive control over an animation.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Animation controllers provide interactive control over animations. They are created when a new
  /// animation is started using one of the <strong>StartAnimation</strong>-methods of the 
  /// <see cref="IAnimationService"/>. Animation controllers can also be created explicitly by 
  /// calling one of the <strong>CreateController</strong>-methods. Using the animation controller 
  /// the user can:
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// Start the animation by calling <see cref="Start()"/>, if the animation is not already running.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Pause the animation by calling <see cref="Pause"/>. (The animation will hold its current
  /// animation value, but won't advance.)
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Continue a previously paused animation by calling <see cref="Resume"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Stop an animation by calling <see cref="Stop()"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Query the state of the animation using the property <see cref="State"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Query the current time of the animation by reading the property <see cref="Time"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Jump to a certain point in time by setting the property <see cref="Time"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Etc.
  /// </description>
  /// </item>
  /// </list>
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
  /// <strong>Memory Management:</strong> The following information is relevant for users who want
  /// to reduce garbage collector (GC) overhead, in particular on the Xbox 360.
  /// </para>
  /// <para>
  /// The type <see cref="AnimationController"/> is a value type (struct). It is a lightweight 
  /// handle that stores a reference to the <see cref="IAnimationService"/> and the 
  /// <see cref="AnimationInstance"/>.
  /// </para>
  /// <para>
  /// Objects of type <see cref="Animation.AnimationInstance"/> are automatically created when a new 
  /// animation controller is created. The animation instances maintain the runtime-state of the
  /// animations. Animation instances can be recycled when they are no longer used. They can then be 
  /// reused by the animation system for future animations.
  /// </para>
  /// <para>
  /// It is recommended to recycle animation instances in order to reduce the number of memory
  /// allocations at runtime. Animation instances can be recycled explicitly by calling 
  /// <see cref="Recycle"/>. Or, they can be recycled automatically by calling 
  /// <see cref="AutoRecycle"/> or setting the property <see cref="AutoRecycleEnabled"/> to 
  /// <see langword="true"/>. If <see cref="AutoRecycleEnabled"/> is set, then the animation system 
  /// will automatically recycle the animation instances once the animation has stopped. The 
  /// property <see cref="AutoRecycleEnabled"/> is <see langword="false"/> by default.
  /// </para>
  /// <para>
  /// The method <see cref="Recycle"/> can be called at any time, regardless of whether 
  /// <see cref="AutoRecycleEnabled"/> is set. The property <see cref="AutoRecycleEnabled"/> can 
  /// also be changed at any time - it will only take effect when the animation stops.
  /// </para>
  /// <para>
  /// Once the animation instance has been recycled the struct <see cref="AnimationController"/> 
  /// becomes invalid. The property <see cref="IsValid"/> indicates whether the animation instance
  /// is still alive or has already been recycled.
  /// </para>
  /// </remarks>
  public struct AnimationController
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly int _id; // 0 indicates an invalid AnimationController.
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the animation service.
    /// </summary>
    /// <value>
    /// The <see cref="IAnimationService"/>.
    /// </value>
    public IAnimationService AnimationService
    {
      get { return _animationManager; }
    }
    private readonly AnimationManager _animationManager;


    /// <summary>
    /// Gets the root animation instance.
    /// </summary>
    /// <value>
    /// The root animation instance. (Or <see langword="null"/> if the animation instance has 
    /// already been recycled and the animation controller is no longer valid.)
    /// </value>
    /// <remarks>
    /// Animation instances maintain the runtime-state of the animations. See 
    /// <see cref="Animation.AnimationInstance"/> for more information.
    /// </remarks>
    public AnimationInstance AnimationInstance
    {
      get
      {
        // The animation instance might have been recycled and reused. Check if the
        // instance is still active.
        return IsValid ? _animationInstance : null;
      }
    }
    private readonly AnimationInstance _animationInstance;


    /// <summary>
    /// Gets a value indicating whether this animation controller is valid.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this animation controller is valid; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property indicates whether the animation controller is still valid. 
    /// <see langword="false"/> means that associated animation instances have been recycled by the 
    /// animation system and the controller can no longer be used.
    /// </para>
    /// <para>
    /// See description of <see cref="AnimationController"/> for more info.
    /// </para>
    /// </remarks>
    public bool IsValid
    {
      get
      {
        // When an animation instance is recycled, its RunCount is incremented. If the 
        // RunCount matches the stored ID, then the animation instance is still active.
        return _animationInstance != null && _animationInstance.RunCount == _id;
      }
    }


    /// <summary>
    /// Gets or sets a value indicating whether the animation instance should be automatically
    /// recycled when the animation is stopped and removed from the animation system.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the animation instance is recycled automatically; otherwise, 
    /// <see langword="false"/>. (The property also returns <see langword="false"/> if the animation
    /// instance has already been recycled and the animation controller is no longer valid.)
    /// </value>
    /// <remarks>
    /// <para>
    /// Objects of type <see cref="Animation.AnimationInstance"/> are automatically created when a 
    /// new animation controller is created. The animation instances maintain the runtime-state of 
    /// the animations. The animation instances can be recycled when they are no longer used. They 
    /// can then be reused by the animation system for future animations.
    /// </para>
    /// <para>
    /// It is recommended to recycle animation instances in order to reduce the number of memory
    /// allocations at runtime. Animation instances can be recycled explicitly by calling 
    /// <see cref="Recycle"/>. Or, they can be recycled automatically by calling 
    /// <see cref="AutoRecycle"/> or setting the property <see cref="AutoRecycleEnabled"/> to 
    /// <see langword="true"/>. If <see cref="AutoRecycleEnabled"/> is set, then the animation 
    /// system will automatically recycle the animation instances once the animation has stopped.
    /// </para>
    /// <para>
    /// The method <see cref="Recycle"/> can be called at any time, regardless of whether 
    /// <see cref="AutoRecycleEnabled"/> is set. The property <see cref="AutoRecycleEnabled"/> can 
    /// also be changed at any time - it will only take effect when the animation stops.
    /// </para>
    /// <para>
    /// Once the animation instance has been recycled the struct <see cref="AnimationController"/> 
    /// becomes invalid. The property <see cref="IsValid"/> indicates whether the animation instance
    /// is still alive or has already been recycled.
    /// </para>
    /// <para>
    /// Note that calling the methods <see cref="Recycle"/>, <see cref="AutoRecycle"/>, or setting 
    /// the property <see cref="AutoRecycleEnabled"/> has no effect if the animation controller is 
    /// invalid.
    /// </para>
    /// </remarks>
    public bool AutoRecycleEnabled
    {
      get { return IsValid && _animationInstance.AutoRecycleEnabled; }
      set
      {
        if (IsValid)
          _animationInstance.AutoRecycleEnabled = value;
      }
    }


    /// <summary>
    /// Gets a value indicating whether the animation is paused.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the animation is paused; otherwise, <see langword="false"/>.
    /// (The property returns <see langword="false"/> if the animation instance has already been
    /// recycled and the animation controller is no longer valid.)
    /// </value>
    /// <seealso cref="Pause"/>
    /// <seealso cref="Resume"/>
    public bool IsPaused
    {
      get { return IsValid && _animationInstance.IsPaused; }
    }


    /// <summary>
    /// Gets or sets the rate at which time progresses.
    /// </summary>
    /// <value>
    /// The speed ratio. The default value is 1. (The property returns <see cref="float.NaN"/> if
    /// the animation instance has already been recycled and the animation controller is no longer
    /// valid.)
    /// </value>
    /// <remarks>
    /// <para>
    /// This property gets or sets the speed ratio of the <see cref="AnimationInstance"/>. See 
    /// <see cref="Animation.AnimationInstance.Speed"/> for more information.
    /// </para>
    /// <para>
    /// Note that setting the property <see cref="Speed"/> has no effect if the animation controller
    /// is invalid.
    /// </para>
    /// </remarks>
    public float Speed
    {
      get { return IsValid ? _animationInstance.Speed : float.NaN; }
      set
      {
        if (IsValid)
          _animationInstance.Speed = value;
      }
    }


    /// <summary>
    /// Gets the current state of the animation.
    /// </summary>
    /// <value>
    /// The current state of the animation. (The property returns 
    /// <see cref="AnimationState.Stopped"/> if the animation instance has already been recycled and
    /// the animation controller is no longer valid.)
    /// </value>
    public AnimationState State
    {
      get { return IsValid ? _animationInstance.State : AnimationState.Stopped; }
    }


    /// <summary>
    /// Gets or sets the current animation time.
    /// </summary>
    /// <value>
    /// The current animation time. (The animation time is <see langword="null"/> if the animation 
    /// has not been started or the animation instance has already been recycled and the animation 
    /// controller is no longer valid.)
    /// </value>
    /// <para>
    /// Note that setting the property <see cref="Time"/> has no effect if the animation controller 
    /// is invalid.
    /// </para>
    public TimeSpan? Time
    {
      get { return IsValid ? _animationInstance.Time : null; }
      set
      {
        if (IsValid)
          _animationInstance.Time = value;
      }
    }


    /// <summary>
    /// Gets or sets the animation weight.
    /// </summary>
    /// <value>
    /// The animation weight. The default value is 1. (The property returns <see cref="float.NaN"/> 
    /// if the animation instance has already been recycled and the animation controller is no 
    /// longer valid.)
    /// </value>
    /// <remarks>
    /// <para>
    /// This property gets or sets the animation weight of the <see cref="AnimationInstance"/>. See 
    /// <see cref="Animation.AnimationInstance.Weight"/> for more information.
    /// </para>
    /// <para>
    /// Note that setting the property <see cref="Weight"/> has no effect if the animation 
    /// controller is invalid.
    /// </para>
    /// </remarks>
    public float Weight
    {
      get { return IsValid ? _animationInstance.Weight : float.NaN; }
      set
      {
        if (IsValid)
          _animationInstance.Weight = value;
      }
    }


    /// <summary>
    /// Occurs when the animation has completed playing. Use with caution - see remarks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This event adds or removes an event handler to or from the completion event of the 
    /// <see cref="AnimationInstance"/>. There are several points to consider when using completion 
    /// events. Be sure to read <see cref="Animation.AnimationInstance.Completed"/> for a detailed
    /// description!
    /// </para>
    /// <para>
    /// Note that adding or removing an event handler has no effect if the animation controller is 
    /// invalid.
    /// </para>
    /// </remarks>
    public event EventHandler<EventArgs> Completed
    {
      add
      {
        if (IsValid)
          _animationInstance.Completed += value;
      }
      remove
      {
        if (IsValid)
          _animationInstance.Completed -= value;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationController"/> struct.
    /// </summary>
    internal AnimationController(AnimationManager animationManager, AnimationInstance animationInstance)
    {
      Debug.Assert(animationManager != null, "The animation system is null.");
      Debug.Assert(animationInstance != null, "The animation instance is null.");
      Debug.Assert(animationInstance.RunCount > 0, "The animation instance has an invalid RunCount.");

      _id = animationInstance.RunCount;
      _animationManager = animationManager;
      _animationInstance = animationInstance;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Automatically recycles the animation instances when the animation is stopped and removed 
    /// from the animation system.
    /// </summary>
    /// <inheritdoc cref="AutoRecycleEnabled"/>
    public void AutoRecycle()
    {
      AutoRecycleEnabled = true;
    }


    /// <summary>
    /// Recycles the animation instances associated with this controller.
    /// </summary>
    /// <inheritdoc cref="AutoRecycleEnabled"/>
    public void Recycle()
    {
      if (IsValid)
        Stop();

      // Check IsValid again - animation instance might have been recycled automatically.
      if (IsValid)
        _animationInstance.Recycle();
    }


    /// <summary>
    /// Stops the animation from progressing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method stops the animation from progressing. The animation will stop at the current
    /// animation time and hold the current animation value. The timing can be resumed by calling 
    /// <see cref="Resume"/>.
    /// </para>
    /// <para>
    /// Note that calling <see cref="Pause"/> or <see cref="Resume"/> has no effect if the animation
    /// controller is invalid.
    /// </para>
    /// </remarks>
    /// <seealso cref="IsPaused"/>
    /// <seealso cref="Resume"/>
    public void Pause()
    {
      if (IsValid)
        _animationInstance.Pause();
    }


    /// <summary>
    /// Resumes an animation that was previously paused.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resumes the animation timing from where it was stopped. The timing can be 
    /// stopped by calling <see cref="Pause"/>.
    /// </para>
    /// <para>
    /// Note that calling <see cref="Pause"/> or <see cref="Resume"/> has no effect if the animation
    /// controller is invalid.
    /// </para>
    /// </remarks>
    /// <seealso cref="IsPaused"/>
    /// <seealso cref="Pause"/>
    public void Resume()
    {
      if (IsValid)
        _animationInstance.Resume();
    }


    /// <overloads>
    /// <summary>
    /// Starts the animation.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Starts the animation immediately.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The animation will be started immediately using <see cref="AnimationTransitions.SnapshotAndReplace"/>. 
    /// Call <see cref="Start(AnimationTransition)"/> if another type of transition (for example, a
    /// fade-in) should be used.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> When animations are started or stopped the animations do not 
    /// take effect immediately. That means the new animation values are not immediately applied to 
    /// the properties that are being animated. The animations are evaluated when the animation 
    /// system is updated (see <see cref="AnimationManager.Update"/>) and new animation values are
    /// written when <see cref="AnimationManager.ApplyAnimations"/> is called.
    /// </para>
    /// <para>
    /// The method <see cref="UpdateAndApply"/> can be called to immediately evaluate and apply the
    /// animation. But in most cases it is not necessary to call this method explicitly.
    /// </para>
    /// </remarks>
    /// <exception cref="AnimationException">
    /// Cannot start animation. The animation instance associated with the current animation 
    /// controller is already running or has already been recycled and the animation controller is
    /// no longer valid.
    /// </exception>
    public void Start()
    {
      Start(null);
    }


    /// <summary>
    /// Starts the animation using the specified transition.
    /// </summary>
    /// <param name="transition">
    /// The transition that determines how the new animation is applied. The class 
    /// <see cref="AnimationTransitions"/> provides a set of predefined animation transitions.
    /// </param>
    /// <inheritdoc cref="Start()"/>
    /// <exception cref="AnimationException">
    /// Cannot start animation. The animation instance associated with the current animation 
    /// controller is already running or has already been recycled and the animation controller is
    /// no longer valid.
    /// </exception>
    public void Start(AnimationTransition transition)
    {
      if (!IsValid)
        throw new AnimationException("Cannot start animation. The animation instance associated with the current animation controller has already been recycled.");

      if (State != AnimationState.Stopped)
        throw new AnimationException("Cannot start animation. The animation instance associated with the current animation controller is already running.");

      _animationManager.StartAnimation(_animationInstance, transition);
    }


    /// <overloads>
    /// <summary>
    /// Stops the animation.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Stops the animation immediately.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Once the animation is stopped, call <see cref="Start()"/> to restart the animation.
    /// </para>
    /// <para>
    /// Note that calling the method <see cref="Stop()"/> (or one of its overloads) has no effect if 
    /// the animation controller is invalid.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> When animations are started or stopped the animations do not 
    /// take effect immediately. That means the new animation values are not immediately applied to 
    /// the properties that are being animated. The animations are evaluated when the animation 
    /// system is updated (see <see cref="AnimationManager.Update"/>) and new animation values are
    /// written when <see cref="AnimationManager.ApplyAnimations"/> is called.
    /// </para>
    /// <para>
    /// The method <see cref="UpdateAndApply"/> can be called to immediately evaluate and apply the
    /// animation. But in most cases it is not necessary to call this method explicitly.
    /// </para>
    /// </remarks>
    public void Stop()
    {
      if (IsValid)
        _animationManager.StopAnimation(_animationInstance);
    }


    /// <summary>
    /// Stops the animation by fading it out over time.
    /// </summary>
    /// <param name="fadeOutDuration">
    /// The duration over which the existing animation fades out.
    /// </param>
    /// <inheritdoc cref="Stop()"/>
    public void Stop(TimeSpan fadeOutDuration)
    {
      if (IsValid)
      {
        if (fadeOutDuration > TimeSpan.Zero)
        {
          // Fade-out animation.
          var transition = new FadeOutTransition(fadeOutDuration);
          transition.AnimationInstance = _animationInstance;
          _animationManager.Add(transition);
        }
        else
        {
          // Stop immediately.
          _animationManager.StopAnimation(_animationInstance);
        }
      }
    }


    /// <summary>
    /// Immediately evaluates the animation and applies the new animation values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When an animation is started or stopped, the values of the animated properties do not change 
    /// immediately. The new animation values will be computed and set when the animation system is 
    /// updated. See <see cref="AnimationManager.Update"/> and 
    /// <see cref="AnimationManager.ApplyAnimations"/>.
    /// </para>
    /// <para>
    /// But in certain cases when an animation is started or stopped the animated properties should 
    /// be updated immediately. In these case the method <see cref="UpdateAndApply"/> needs to be 
    /// called after the animation is started or stopped. This method immediately evaluates the 
    /// animation and applies the new animation values to the properties that are being animated. 
    /// </para>
    /// <para>
    /// The method can also be called if an animation is modified (e.g. key frames are added or 
    /// removed) and the changes should take effect immediately.
    /// </para>
    /// <para>
    /// In most cases it is not necessary to call this method because the animation service updates 
    /// and applies animations automatically. 
    /// </para>
    /// <para>
    /// Note that <see cref="UpdateAndApply"/> does not advance the time of the animation. The 
    /// animation is evaluated at the current time.
    /// </para>
    /// </remarks>
    /// <exception cref="AnimationException">
    /// Cannot update and apply animation. The animation instance associated with the current 
    /// animation controller is already running or has already been recycled and the animation 
    /// controller is no longer valid.
    /// </exception>
    public void UpdateAndApply()
    {
      if (!IsValid)
        throw new AnimationException("Cannot update and apply animation. The animation instance associated with the current animation controller has already been recycled.");

      _animationInstance.UpdateAndApply(_animationManager);
    }
    #endregion
  }
}
