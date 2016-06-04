// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Animation.Traits;
using DigitalRune.Animation.Transitions;
using DigitalRune.Threading;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Represents the animations system which can be used to play back animations.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class implements the <see cref="IAnimationService"/>. See documentation of
  /// <see cref="IAnimationService"/>.
  /// </para>
  /// <para>
  /// This class has two important methods: <see cref="Update"/> must be called once per frame,
  /// and it updates all managed animations. The new animation values are internally cached but not 
  /// applied until <see cref="ApplyAnimations"/> is called. <see cref="Update"/> can usually be 
  /// called parallel to other game services (graphics, physics, AI, etc.). 
  /// <see cref="ApplyAnimations"/> should be called when it is safe to write the animation results 
  /// to the animated objects and properties.
  /// </para>
  /// </remarks>
  public class AnimationManager : IAnimationService
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly List<AnimationInstance> _rootInstances;
    private readonly List<AnimationInstance> _completedInstances;
    private readonly AnimationCompositionChainCollection _compositionChains;
    private readonly List<AnimationTransition> _transitions;

    // Temporary lists allocated once to avoid memory allocations at runtime.
    private readonly List<AnimationInstance> _tempInstances;
    private readonly List<IAnimatableProperty> _tempProperties;
    private readonly List<AnimationTransition> _tempTransitions;

    // Last index for incremental clean-up.
    private uint _incrementalIndex;

    // Store delegates to avoid memory allocation at runtime.
    private readonly Action<int> _updateAnimationsMethod;
    private readonly Action<int> _updateCompositionChainMethod;

    // The size of the current time step.
    private TimeSpan _deltaTime;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether multithreading is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if multithreading is enabled; otherwise, <see langword="false"/>. The
    /// default value is <see langword="true"/> if the current system has more than one CPU cores.
    /// </value>
    /// <remarks>
    /// <para>
    /// When multithreading is enabled the animation system will distribute the workload across 
    /// multiple processors (CPU cores) to improve the performance. 
    /// </para>
    /// <para>
    /// Multithreading adds an additional overhead, therefore it should only be enabled if the 
    /// current system has more than one CPU core and if the other cores are not fully utilized by
    /// the application. Multithreading should be disabled if the system has only one CPU core or
    /// if all other CPU cores are busy. In some cases it might be necessary to run a benchmark of
    /// the application and compare the performance with and without multithreading to decide
    /// whether multithreading should be enabled or not.
    /// </para>
    /// <para>
    /// The animation system internally uses the class <see cref="Parallel"/> for parallelization.
    /// <see cref="Parallel"/> is a static class that defines how many worker threads are created, 
    /// how the workload is distributed among the worker threads and more. (See 
    /// <see cref="Parallel"/> to find out more on how to configure parallelization.)
    /// </para>
    /// </remarks>
    /// <seealso cref="Parallel"/>
    public bool EnableMultithreading { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationManager"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
    public AnimationManager()
    {
      _rootInstances = new List<AnimationInstance>();
      _completedInstances = new List<AnimationInstance>();
      _compositionChains = new AnimationCompositionChainCollection();
      _transitions = new List<AnimationTransition>();

      // Temporary list used in update.
      _tempInstances = new List<AnimationInstance>();
      _tempProperties = new List<IAnimatableProperty>();
      _tempTransitions = new List<AnimationTransition>();

#if WP7 || UNITY
      // Cannot access Environment.ProcessorCount in phone app. (Security issue.)
      EnableMultithreading = false;
#else
      // Enable multithreading by default if the current system has multiple processors.
      EnableMultithreading = Environment.ProcessorCount > 1;

      // Multithreading works but Parallel.For of Xamarin.Android/iOS is very inefficient.
      if (GlobalSettings.PlatformID == PlatformID.Android || GlobalSettings.PlatformID == PlatformID.iOS)
        EnableMultithreading = false;
#endif

      _updateAnimationsMethod = UpdateAnimation;
      _updateCompositionChainMethod = UpdateCompositionChain;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public bool IsAnimated(IAnimatableObject animatableObject)
    {
      if (animatableObject == null)
        throw new ArgumentNullException("animatableObject");

      foreach (var animatableProperty in animatableObject.GetAnimatedProperties())
        if (IsAnimated(animatableProperty))
          return true;

      return false;
    }


    /// <inheritdoc/>
    public bool IsAnimated(IAnimatableProperty animatableProperty)
    {
      if (animatableProperty == null)
        throw new ArgumentNullException("animatableProperty");

      int index;
      IAnimationCompositionChain compositionChain;
      _compositionChains.Get(animatableProperty, out index, out compositionChain);
      return compositionChain != null;
    }


    /// <overloads>
    /// <summary>
    /// Creates a new animation controller which can be used to apply the given animation on the
    /// specified objects or properties.
    /// </summary>
    /// </overloads>
    /// 
    /// <inheritdoc/>
    public AnimationController CreateController(ITimeline animation, IEnumerable<IAnimatableObject> targetObjects)
    {
      if (targetObjects == null)
        throw new ArgumentNullException("targetObjects");
      if (animation == null)
        throw new ArgumentNullException("animation");

      var animationInstance = animation.CreateInstance();
      animationInstance.AssignTo(targetObjects);
      return new AnimationController(this, animationInstance);
    }


    /// <inheritdoc/>
    public AnimationController CreateController(ITimeline animation, IAnimatableObject targetObject)
    {
      if (targetObject == null)
        throw new ArgumentNullException("targetObject");
      if (animation == null)
        throw new ArgumentNullException("animation");

      var animationInstance = animation.CreateInstance();
      animationInstance.AssignTo(targetObject);
      return new AnimationController(this, animationInstance);
    }


    /// <inheritdoc/>
    public AnimationController CreateController(ITimeline animation, IAnimatableProperty targetProperty)
    {
      if (targetProperty == null)
        throw new ArgumentNullException("targetProperty");
      if (animation == null)
        throw new ArgumentNullException("animation");

      var animationInstance = animation.CreateInstance();
      animationInstance.AssignTo(targetProperty);
      return new AnimationController(this, animationInstance);
    }


    /// <overloads>
    /// <summary>
    /// Starts an animation on the specified objects or properties.
    /// </summary>
    /// </overloads>
    /// 
    /// <inheritdoc/>
    public AnimationController StartAnimation(ITimeline animation, IEnumerable<IAnimatableObject> targetObjects)
    {
      return StartAnimation(animation, targetObjects, null);
    }


    /// <inheritdoc/>
    public AnimationController StartAnimation(ITimeline animation, IEnumerable<IAnimatableObject> targetObjects, AnimationTransition transition)
    {
      if (targetObjects == null)
        throw new ArgumentNullException("targetObjects");
      if (animation == null)
        throw new ArgumentNullException("animation");

      var animationInstance = animation.CreateInstance();
      animationInstance.AssignTo(targetObjects);
      StartAnimation(animationInstance, transition);
      return new AnimationController(this, animationInstance);
    }


    /// <inheritdoc/>
    public AnimationController StartAnimation(ITimeline animation, IAnimatableObject targetObject)
    {
      return StartAnimation(animation, targetObject, null);
    }


    /// <inheritdoc/>
    public AnimationController StartAnimation(ITimeline animation, IAnimatableObject targetObject, AnimationTransition transition)
    {
      if (targetObject == null)
        throw new ArgumentNullException("targetObject");
      if (animation == null)
        throw new ArgumentNullException("animation");

      var animationInstance = animation.CreateInstance();
      animationInstance.AssignTo(targetObject);
      StartAnimation(animationInstance, transition);
      return new AnimationController(this, animationInstance);
    }


    /// <inheritdoc/>
    public AnimationController StartAnimation(ITimeline animation, IAnimatableProperty targetProperty)
    {
      return StartAnimation(animation, targetProperty, null);
    }


    /// <inheritdoc/>
    public AnimationController StartAnimation(ITimeline animation, IAnimatableProperty targetProperty, AnimationTransition transition)
    {
      if (targetProperty == null)
        throw new ArgumentNullException("targetProperty");
      if (animation == null)
        throw new ArgumentNullException("animation");

      var animationInstance = animation.CreateInstance();
      animationInstance.AssignTo(targetProperty);
      StartAnimation(animationInstance, transition);
      return new AnimationController(this, animationInstance);
    }


    /// <summary>
    /// Starts the animations using the specified transition.
    /// </summary>
    /// <param name="animationInstance">The animation instance.</param>
    /// <param name="transition">
    /// The animation transition. (Can be <see langword="null"/>, in which case
    /// <see cref="AnimationTransitions.SnapshotAndReplace()"/> will be used.)
    /// </param>
    internal void StartAnimation(AnimationInstance animationInstance, AnimationTransition transition)
    {
      if (transition == null)
        transition = AnimationTransitions.SnapshotAndReplace();

      transition.AnimationInstance = animationInstance;
      Add(transition);
    }


    /// <overloads>
    /// <summary>
    /// Stops the animation on the specified objects or properties.
    /// </summary>
    /// </overloads>
    /// 
    /// <inheritdoc/>
    public void StopAnimation(IEnumerable<IAnimatableObject> animatedObjects)
    {
      if (animatedObjects == null)
        return;

      foreach (var animatedObject in animatedObjects)
        StopAnimation(animatedObject);
    }


    /// <inheritdoc/>
    public void StopAnimation(IAnimatableObject animatedObject)
    {
      if (animatedObject == null)
        return;

      foreach (var animatedProperty in animatedObject.GetAnimatedProperties())
        StopAnimation(animatedProperty);
    }


    /// <inheritdoc/>
    public void StopAnimation(IAnimatableProperty animatedProperty)
    {
      int index;
      IAnimationCompositionChain compositionChain;
      _compositionChains.Get(animatedProperty, out index, out compositionChain);

      if (compositionChain != null)
      {
        for (int i = compositionChain.Count - 1; i >= 0; i--)
        {
          if (i >= compositionChain.Count)
          {
            // Multiple instances were removed in the previous iteration.
            // --> Skip index.
            continue;
          }

          var animationInstance = compositionChain[i].GetRoot();
          Remove(animationInstance);
        }
      }
    }


    /// <summary>
    /// Stops the specified animations.
    /// </summary>
    /// <param name="animationInstance">The animation instance.</param>
    internal void StopAnimation(AnimationInstance animationInstance)
    {
      if (animationInstance != null)
        Remove(animationInstance);
    }


    /// <summary>
    /// Registers the specified animation instance.
    /// </summary>
    /// <param name="animationInstance">The animation instance.</param>
    /// <param name="handoffBehavior">
    /// A value indicating how the new animations interact with existing ones.
    /// </param>
    /// <param name="previousInstance">
    /// Optional: The animation instance after which <paramref name="animationInstance"/> will be 
    /// added in the animation composition chain. If set to <see langword="null"/> 
    /// <paramref name="animationInstance"/> will be appended at the end of the composition chain. 
    /// This parameter is only relevant when <paramref name="handoffBehavior"/> is 
    /// <see cref="HandoffBehavior.Compose"/>. 
    /// </param>
    /// <remarks>
    /// <para>
    /// This method adds the specified animation tree (<paramref name="animationInstance"/> and all
    /// of its children) to the animation system. The animation system will from now on
    /// automatically advance and update the animations.
    /// </para>
    /// <para>
    /// Adding and removing animation instances is usually controlled by 
    /// <see cref="AnimationTransition"/> instances.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animationInstance" /> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Cannot add <paramref name="animationInstance"/> to animation system. The animation instance 
    /// is already registered, or it is not a root instance.
    /// </exception>
    internal void Add(AnimationInstance animationInstance, HandoffBehavior handoffBehavior, AnimationInstance previousInstance)
    {
      if (animationInstance == null)
        throw new ArgumentNullException("animationInstance");
      if (_rootInstances.Contains(animationInstance))        // TODO: This is slow if there are many animation instances!
        throw new ArgumentException("Cannot add animation instance to animation system. The animation instance is already registered.");
      if (!animationInstance.IsRoot)
        throw new ArgumentException("Cannot add animation instance to animation system because it is not a root instance. You need to disconnect the instance from its parent first.");

      _rootInstances.Add(animationInstance);

      // Normally, the time is NaN and starts to run now. (Unless the user has set a custom 
      // time value.)
      if (animationInstance.Time == null)
        animationInstance.Time = TimeSpan.Zero;

      animationInstance.AddToCompositionChains(this, handoffBehavior, previousInstance);
    }


    /// <summary>
    /// Unregisters the specified animation instance.
    /// </summary>
    /// <param name="animationInstance">The animation instance.</param>
    /// <remarks>
    /// <para>
    /// This method removes the specified animation tree from the animation system. If 
    /// <see cref="AnimationInstance.AutoRecycleEnabled"/> is set on the root node of the animation 
    /// tree, then all instances will be recycled.
    /// </para>
    /// <para>
    /// Adding and removing animation instances is usually controlled by 
    /// <see cref="AnimationTransition"/> instances.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Cannot remove <paramref name="animationInstance"/> from the animation system. The animation 
    /// instance is not a root instance.
    /// </exception>
    internal void Remove(AnimationInstance animationInstance)
    {
      if (!animationInstance.IsRoot)
        throw new ArgumentException("Cannot remove animation instance from animation system because it is not the root instance.");

      // Remove animation instance.
      bool removed = _rootInstances.Remove(animationInstance);
      if (removed)
      {
        // Remove animation transitions, if any.
        StopTransitions(animationInstance);

        // Stop all secondary animations.
        animationInstance.StopSecondaryAnimations(this);

        // Remove animations from composition chains.
        animationInstance.RemoveFromCompositionChains(this);

        // Note: The instances are not unassigned from the properties.
        // Animations can be restart when using an animation controller.
        //animationInstance.Unassign();

        // Reset animation time (State = Stopped).
        animationInstance.Time = null;

        // Recycle instance, if no longer needed.
        if (animationInstance.AutoRecycleEnabled)
        {
          // If the instance is in the completedInstances list, we need to wait 
          // until the Completed events were fired in ApplyAnimations().
          if (!_completedInstances.Contains(animationInstance))
            animationInstance.Recycle();
        }
      }
    }


    /// <summary>
    /// Removes all animations before the specified animation.
    /// </summary>
    /// <param name="animationInstance">The animation instance.</param>
    internal void RemoveBefore(AnimationInstance animationInstance)
    {
      Debug.Assert(animationInstance.IsRoot, "The animation instance in RemoveBefore needs to be a root instance.");
      Debug.Assert(_tempProperties.Count == 0, "Temporary list of animatable properties has not been reset.");
      Debug.Assert(_tempInstances.Count == 0, "Temporary list of animation instances has not been reset.");

      // Get all animated properties.
      animationInstance.GetAssignedProperties(_tempProperties);

      // Collect all animation instances before animationInstance.
      foreach (var property in _tempProperties)
      {
        if (property == null)
          continue;

        int index;
        IAnimationCompositionChain compositionChain;
        _compositionChains.Get(property, out index, out compositionChain);

        if (compositionChain == null)
          continue;

        var numberOfInstances = compositionChain.Count;
        for (int i = 0; i < numberOfInstances; i++)
        {
          var instance = compositionChain[i];
          var rootInstance = instance.GetRoot();
          if (rootInstance == animationInstance)
          {
            // Break inner loop, continue with next composition chain.
            break;
          }

          if (!_tempInstances.Contains(rootInstance))
            _tempInstances.Add(rootInstance);
        }
      }

      // Stop the found animation instances.
      foreach (var oldInstance in _tempInstances)
        Remove(oldInstance);

      _tempProperties.Clear();
      _tempInstances.Clear();
    }


    /// <summary>
    /// Removes the animation transitions controlling the given animation instance.
    /// </summary>
    /// <param name="animationInstance">The animation instance.</param>
    private void StopTransitions(AnimationInstance animationInstance)
    {
      for (int i = _transitions.Count - 1; i >= 0; i--)
      {
        if (i >= _transitions.Count)
        {
          // Multiple transitions were removed in the previous iteration.
          // --> Skip index.
          continue;
        }

        var transition = _transitions[i];
        if (transition.AnimationInstance == animationInstance)
          Remove(transition);
      }
    }


    /// <summary>
    /// Adds the specified animation transition.
    /// </summary>
    /// <param name="transition">The animation transition.</param>
    internal void Add(AnimationTransition transition)
    {
      _transitions.Add(transition);
      transition.Initialize(this);
    }


    /// <summary>
    /// Removes the specified animation transition.
    /// </summary>
    /// <param name="transition">The animation transition.</param>
    internal void Remove(AnimationTransition transition)
    {
      bool removed = _transitions.Remove(transition);
      if (removed)
        transition.Uninitialize(this);
    }


    /// <summary>
    /// Updates all animations.
    /// </summary>
    /// <param name="deltaTime">
    /// The elapsed time since the last update. (If <paramref name="deltaTime"/> is negative,
    /// this method does nothing. It does not reverse the animations.)
    /// </param>
    /// <remarks>
    /// <para>
    /// The method <see cref="Update"/> advances all animations and computes the animation values.
    /// Note however, that the new animation values are not yet applied to the animated properties. 
    /// <see cref="ApplyAnimations"/> needs to be called to write the new animation values!
    /// </para>
    /// <para>
    /// Similarly, completion events (see <see cref="AnimationInstance.Completed"/>) are recorded, 
    /// but are not yet triggered. Completion events are also triggered in 
    /// <see cref="ApplyAnimations"/>!
    /// </para>
    /// </remarks>
    /// <seealso cref="ApplyAnimations"/>
    public void Update(TimeSpan deltaTime)
    {
      // Note: We allow updates with deltaTime = 0. This can be useful if the animations 
      // changes and the animation properties should be updated without advancing the 
      // animation time.
      if (deltaTime < TimeSpan.Zero)
        return;

      // Advance all animations.
      UpdateAnimations(deltaTime);

      // Update all transitions.
      UpdateTransitions(deltaTime);

      // Remove stopped animations.
      CheckAnimations();

      // Update composition chains.
      UpdateCompositionChains();
    }


    /// <summary>
    /// Advances the animation instances.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    private void UpdateAnimations(TimeSpan deltaTime)
    {
      _deltaTime = deltaTime;
      var numberOfAnimations = _rootInstances.Count;
      if (EnableMultithreading && numberOfAnimations > 1)
      {
        Parallel.For(0, numberOfAnimations, _updateAnimationsMethod);
      }
      else
      {
        for (int i = 0; i < numberOfAnimations; i++)
          UpdateAnimation(i);
      }
    }


    /// <summary>
    /// Advances the animation instance with the given index.
    /// </summary>
    /// <param name="i">The index.</param>
    private void UpdateAnimation(int i)
    {
      var animationInstance = _rootInstances[i];

      // Store the previous animation state to check whether the animation has completed.
      var oldState = animationInstance.State;

      // Advance animation time.
      animationInstance.AdvanceTime(_deltaTime);

      // Get the new animation state and check whether the Completed event needs to be raised.
      var newState = animationInstance.State;
      if (newState == AnimationState.Stopped || newState == AnimationState.Filling && oldState != AnimationState.Filling)
      {
        if (animationInstance.RequiresCompletedEvent)
        {
          lock (_completedInstances)
            _completedInstances.Add(animationInstance);
        }
      }
    }


    /// <summary>
    /// Updates the animation transitions.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    private void UpdateTransitions(TimeSpan deltaTime)
    {
      // Make a temporary copy of the animation transitions. 
      // (Transition can add or remove other transitions in Update().)
      Debug.Assert(_tempTransitions.Count == 0, "Temporary list of animation transitions should be empty.");
      foreach (var transition in _transitions)
        _tempTransitions.Add(transition);

      // The following loop is not parallelized. Transition do a lot of management tasks: 
      // Removing old animations, adding new animations, applying animations to animation 
      // weights, etc. In order to parallelize these tasks we would have to sync operations
      // in a lot of places.
      foreach (var transition in _tempTransitions)
        transition.Update(this);

      _tempTransitions.Clear();
    }


    /// <summary>
    /// Removes all animation instances which have stopped.
    /// </summary>
    private void CheckAnimations()
    {
      for (int i = _rootInstances.Count - 1; i >= 0; i--)
      {
        if (i >= _rootInstances.Count)
        {
          // Multiple instances were removed in the previous iteration.
          // --> Skip index.
          continue;
        }

        var animationInstance = _rootInstances[i];
        if (animationInstance.State == AnimationState.Stopped)
        {
          Remove(animationInstance);
        }
      }

      // Remove animation instances if targets have been garbage collected.
      IncrementalCleanup();
    }


    /// <summary>
    /// Incremental cleanup: Removes animation instances if their targets have been garbage 
    /// collected.
    /// </summary>
    private void IncrementalCleanup()
    {
      // We need to check whether animation instances are no longer needed and should be removed.
      // Checking all animation instances every frame is too expensive. Therefore, we do an 
      // incremental clean-up: Every frame we check one animation instance.
      // (Alternatively, we could notify the animation system every time an animation instance is
      // removed from an animation composition chain.)

      var numberOfInstances = _rootInstances.Count;
      if (numberOfInstances == 0)
        return;

      unchecked { _incrementalIndex++; }
      int index = (int)(_incrementalIndex % numberOfInstances);
      var animationInstance = _rootInstances[index];
      if (!animationInstance.IsActive(this))
        Remove(animationInstance);
    }


    /// <summary>
    /// Updates the animation composition chains.
    /// </summary>
    private void UpdateCompositionChains()
    {
      // The update of the animated properties is the most expensive part. The animations are
      // evaluated and blended. The individual properties (animation composition chains) can be 
      // updated in parallel.

      // Note: "Animations on animation weights"
      // Composition chains for animated animation weights are sorted in Add(compositionChain). 
      // They are the first in the list. The new animation values need to be applied immediately, 
      // because the weights are read in subsequent composition chains. The method 
      // AnimationCompositionChain.Update() immediately applies animations to animations weights.

      // Note: "Animations on animations weights that control other animated weights"
      // When we update all composition chains sequentially then all is fine. In the 
      // multithreaded update, the animation weights are updated in parallel. That means,
      // the update order of animated weights that influence other animated weights is
      // non-deterministic. This should not be a problem in practice.

      // First validate the AnimationChainCollection. (Only done in DEBUG!)
      _compositionChains.Validate();

      var numberOfCompositionChains = _compositionChains.NumberOfChains;
      if (EnableMultithreading && numberOfCompositionChains > 1)
      {
        int numberOfWeights = _compositionChains.NumberOfImmediateChains;

        // Update and apply animation weights.
        Parallel.For(0, numberOfWeights, _updateCompositionChainMethod);

        // Update remaining composition chains.
        Parallel.For(numberOfWeights, numberOfCompositionChains, _updateCompositionChainMethod);
      }
      else
      {
        for (int i = 0; i < numberOfCompositionChains; i++)
          UpdateCompositionChain(i);
      }
    }


    /// <summary>
    /// Updates the composition chain with the given index.
    /// </summary>
    /// <param name="index">The index.</param>
    private void UpdateCompositionChain(int index)
    {
      var compositionChain = _compositionChains[index];
      compositionChain.Update(this);
    }


    /// <summary>
    /// Applies the new animation values to all animated properties.
    /// </summary>
    /// <inheritdoc cref="Update"/>
    /// <seealso cref="Update"/>
    public void ApplyAnimations()
    {
      // Apply animation values.
      foreach (var compositionChain in _compositionChains)
        compositionChain.Apply();

      // Raise Completed events.
      foreach (var animationInstance in _completedInstances)
      {
        animationInstance.RaiseCompletedEvent();
        if (animationInstance.State == AnimationState.Stopped && animationInstance.AutoRecycleEnabled)
          animationInstance.Recycle();
      }

      _completedInstances.Clear();

      // Remove empty animation composition chains.
      // (Empty composition chains may only be removed after they have been applied, 
      // because they need to reset the animated properties first.)
      for (int i = _compositionChains.NumberOfChains - 1; i >= 0; i--)
      {
        var compositionChain = _compositionChains[i];
        if (compositionChain.IsEmpty)
        {
          _compositionChains.RemoveAt(i);
          compositionChain.Recycle();
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Immediately evaluates the specified animations and applies the new animation values.
    /// </summary>
    /// </overloads>
    /// 
    /// <inheritdoc/>
    public void UpdateAndApplyAnimation(IEnumerable<IAnimatableObject> animatedObjects)
    {
      if (animatedObjects == null)
        return;

      foreach (var animatedObject in animatedObjects)
        UpdateAndApplyAnimation(animatedObject);
    }


    /// <inheritdoc/>
    public void UpdateAndApplyAnimation(IAnimatableObject animatedObject)
    {
      if (animatedObject == null)
        return;

      foreach (var animatedProperty in animatedObject.GetAnimatedProperties())
        UpdateAndApplyAnimation(animatedProperty);
    }


    /// <summary>
    /// Immediately evaluates the animations of the given properties and applies the new animation 
    /// values.
    /// </summary>
    /// <param name="properties">The properties that need to be updated.</param>
    public void UpdateAndApplyAnimation(List<IAnimatableProperty> properties)
    {
      Debug.Assert(properties != null);

      foreach (var property in properties)
        UpdateAndApplyAnimation(property);
    }


    /// <inheritdoc/>
    public void UpdateAndApplyAnimation(IAnimatableProperty property)
    {
      int index;
      IAnimationCompositionChain compositionChain;
      _compositionChains.Get(property, out index, out compositionChain);
      if (compositionChain != null)
      {
        compositionChain.Update(this);
        compositionChain.Apply();

        // Remove empty animation composition chains.
        if (compositionChain.IsEmpty)
          _compositionChains.RemoveAt(index);
      }
    }


    /// <summary>
    /// Gets the animation composition chain of the given animatable property and type.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="property">
    /// The animatable property. Can be <see langword="null"/>.
    /// </param>
    /// <param name="traits">
    /// The animation value traits.
    /// </param>
    /// <param name="createIfNotFound">
    /// If set to <see langword="true"/> a new animation composition chain will be created 
    /// automatically when necessary. (<paramref name="property"/> must not be 
    /// <see langword="null"/>.)
    /// </param>
    /// <returns>
    /// The <see cref="IAnimationCompositionChain"/> of the animated property.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="createIfNotFound"/> is set, but <paramref name="property"/> or 
    /// <paramref name="traits"/> is <see langword="null"/>.
    /// </exception>
    internal AnimationCompositionChain<T> GetCompositionChain<T>(IAnimatableProperty<T> property, IAnimationValueTraits<T> traits, bool createIfNotFound)
    {
      if (property == null)
      {
        if (createIfNotFound)
          throw new ArgumentNullException("property", "The property must not be null if a new animation composition chain should be created.");

        return null;
      }

      int index;
      IAnimationCompositionChain untypedCompositionChain;
      _compositionChains.Get(property, out index, out untypedCompositionChain);

      AnimationCompositionChain<T> compositionChain = untypedCompositionChain as AnimationCompositionChain<T>;
      if (compositionChain == null && createIfNotFound)
      {
        if (traits == null)
          throw new ArgumentNullException("traits", "The animation value traits need to be set when a new animation composition chain should be created.");

        // Create new animation composition chain.
        compositionChain = AnimationCompositionChain<T>.Create(property, traits);
        _compositionChains.Insert(index, compositionChain);
      }

      return compositionChain;
    }
    #endregion
  }
}
