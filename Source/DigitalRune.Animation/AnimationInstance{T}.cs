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
  /// Represents an instance of an animation.
  /// </summary>
  /// <typeparam name="T">The type of animation value.</typeparam>
  /// <inheritdoc/>
  public sealed class AnimationInstance<T> : AnimationInstance
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // ReSharper disable StaticFieldInGenericType
    private static readonly ResourcePool<AnimationInstance<T>> Pool = new ResourcePool<AnimationInstance<T>>(
      () => new AnimationInstance<T>(), // Create
      null,                             // Initialize
      null);                            // Uninitialize
    // ReSharper restore StaticFieldInGenericType
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the target property that is being animated.
    /// </summary>
    /// <value>
    /// The target property that is being animated. Returns <see langword="null"/> if the instance
    /// is not assigned to a property or if the owner of the property has been garbage collected.
    /// </value>
    public IAnimatableProperty<T> Property
    {
      get { return (IAnimatableProperty<T>)_weakReference.Target; }
      private set { _weakReference.Target = value; }
    }
    private readonly WeakReference _weakReference;


    /// <summary>
    /// Gets the animation that is being played back.
    /// </summary>
    /// <value>The animation that is being played back.</value>
    public new IAnimation<T> Animation
    {
      get { return (IAnimation<T>)base.Animation; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Prevents a default instance of the <see cref="AnimationInstance{T}"/> class from being
    /// created.
    /// </summary>
    private AnimationInstance()
    {
      _weakReference = new WeakReference(null);
    }


    /// <summary>
    /// Creates an instance of the <see cref="AnimationInstance{T}"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <param name="animation">The animation that should be played back.</param>
    /// <returns>
    /// A new or reusable instance of the <see cref="AnimationInstance{T}"/> class.
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
    public static AnimationInstance<T> Create(IAnimation<T> animation)
    {
      var animationInstance = Pool.Obtain();
      animationInstance.Initialize(animation);
      return animationInstance;
    }


    /// <summary>
    /// Recycles this instance of the <see cref="AnimationInstance"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    public override void Recycle()
    {
      Reset();
      _weakReference.Target = null;

      if (RunCount < int.MaxValue)
        Pool.Recycle(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    internal override AnimationInstanceCollection CreateChildCollection()
    {
      return ReadOnlyAnimationInstanceCollection.Instance;
    }


    /// <summary>
    /// Gets the current animation value.
    /// </summary>
    /// <param name="defaultSource">
    /// In: The source value that should be used by the animation if the animation does not have its
    /// own source value.
    /// </param>
    /// <param name="defaultTarget">
    /// In: The target value that should be used by the animation if the animation does not have its
    /// own target value.
    /// </param>
    /// <param name="result">
    /// Out: The value of the animation at the current time. (The animation returns 
    /// <paramref name="defaultSource"/> if the animation is currently 
    /// <see cref="AnimationState.Delayed"/> or <see cref="AnimationState.Stopped"/>.)
    /// </param>
    /// <remarks>
    /// <para>
    /// Note that the parameters need to be passed by reference. <paramref name="defaultSource"/> 
    /// and <paramref name="defaultTarget"/> are input parameters. The resulting animation value is 
    /// stored in <paramref name="result"/>.
    /// </para>
    /// <para>
    /// The values of the <paramref name="defaultSource"/> and the <paramref name="defaultTarget"/>
    /// parameters depend on where the animation is used. If the animation is used to animate an 
    /// <see cref="IAnimatableProperty{T}"/>, then the values depend on the position of the
    /// animation in the composition chain:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// If the animation has replaced another animation using 
    /// <see cref="AnimationTransitions.SnapshotAndReplace"/>: 
    /// <paramref name="defaultSource"/> is the last output value of the animation which was 
    /// replaced and <paramref name="defaultTarget"/> is the base value of the animated property.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// If the animation is the first in an animation composition chain: 
    /// <paramref name="defaultSource"/> and <paramref name="defaultTarget"/> are the base value of
    /// the animated property.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// If the animation is not the first in an animation composition chain: 
    /// <paramref name="defaultSource"/> is the output of the previous stage in the composition 
    /// chain and <paramref name="defaultTarget"/> is the base value of the animated property.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// If the animation is not used to animate an <see cref="IAnimatableProperty{T}"/>, the values
    /// need to be set by the user depending on the context where the animation is used. (In most
    /// cases it is safe to ignore the parameters and just pass default values.)
    /// </para>
    /// </remarks>
    internal void GetValue(ref T defaultSource, ref T defaultTarget, ref T result)
    {
      var state = State;
      if (state == AnimationState.Delayed || state == AnimationState.Stopped)
      {
        Animation.Traits.Copy(ref defaultSource, ref result);
        return;
      }

      float weight = GetEffectiveWeight();
      if (Numeric.IsZero(weight))
      {
        Animation.Traits.Copy(ref defaultSource, ref result);
        return;
      }

      Debug.Assert(Time.HasValue, "State is Playing or Filling. Time needs to be set.");
      TimeSpan time = Time.Value;
      if (weight == 1.0f)
      {
        // Evaluate animation.
        Animation.GetValue(time, ref defaultSource, ref defaultTarget, ref result);
      }
      else
      {
        // Weighted animation.
        var traits = Animation.Traits;

        // 'defaultSource' and 'result' may be the same instance! We need to ensure that
        // the source value is not overwritten by GetValue().
        // --> Use local variable to get animation value. 
        T value;
        traits.Create(ref defaultSource, out value);

        // Evaluate animation.
        Animation.GetValue(time, ref defaultSource, ref defaultTarget, ref value);

        // Blend animation with the output of the previous stage.
        traits.Interpolate(ref defaultSource, ref value, weight, ref result);

        traits.Recycle(ref value);
      }
    }


    private IAnimatableProperty<T> GetTargetProperty(IEnumerable<IAnimatableObject> objects)
    {
      string objectName = Animation.TargetObject;
      if (String.IsNullOrEmpty(objectName))
      {
        // No target object set. All objects are potential targets.
        foreach (var obj in objects)
        {
          var typedProperty = GetTargetProperty(obj);
          if (typedProperty != null)
            return typedProperty;
        }
      }
      else
      {
        // Find target object.
        foreach (var obj in objects)
        {
          if (obj.Name == objectName)
            return GetTargetProperty(obj);
        }
      }

      return null;
    }


    private IAnimatableProperty<T> GetTargetProperty(IAnimatableObject obj)
    {
      // Note: In this case we do not check whether Animation.TargetObject matches obj.Name.
      // This method should only be called to explicitly assign the animation to the given object.

      var propertyName = Animation.TargetProperty;
      if (!String.IsNullOrEmpty(propertyName))
        return obj.GetAnimatableProperty<T>(propertyName);

      return null;
    }


    /// <inheritdoc/>
    internal override bool IsAssignableTo(IEnumerable<IAnimatableObject> objects)
    {
      return GetTargetProperty(objects) != null;
    }


    /// <inheritdoc/>
    internal override bool IsAssignableTo(IAnimatableObject obj)
    {
      return GetTargetProperty(obj) != null;
    }


    /// <inheritdoc/>
    internal override bool IsAssignableTo(IAnimatableProperty property)
    {
      // Note: In this case we do not check whether Animation.TargetProperty matches property.Name.
      // This method should only be called to explicitly assign the animation to the given property.

      return property is IAnimatableProperty<T>;
    }


    /// <inheritdoc/>
    internal override bool IsAssignedTo(IAnimatableProperty property)
    {
      var assignedProperty = Property;
      return assignedProperty != null && assignedProperty == property;
    }


    /// <inheritdoc/>
    internal override void AssignTo(IEnumerable<IAnimatableObject> objects)
    {
      var property = GetTargetProperty(objects);
      if (property != null)
        Property = property;
    }


    /// <inheritdoc/>
    internal override void AssignTo(IAnimatableObject obj)
    {
      var property = GetTargetProperty(obj);
      if (property != null)
        Property = property;
    }


    /// <inheritdoc/>
    internal override void AssignTo(IAnimatableProperty property)
    {
      // Note: In this case we do not check whether Animation.TargetProperty matches property.Name.
      // This method should only be called to explicitly assign the animation to the given property.

      var typedProperty = property as IAnimatableProperty<T>;
      if (typedProperty != null)
        Property = typedProperty;
    }


    ///// <inheritdoc/>
    //internal override void Unassign()
    //{
    //  Property = null;
    //}


    /// <inheritdoc/>
    internal override AnimationInstance GetAssignedInstance(IAnimatableProperty property)
    {
      return IsAssignedTo(property) ? this : null;
    }


    /// <inheritdoc/>
    internal override void GetAssignedProperties(List<IAnimatableProperty> properties)
    {
      var property = Property;
      if (property != null)
        properties.Add(property);
    }


    /// <inheritdoc/>
    internal override bool IsActive(AnimationManager animationManager)
    {
      var property = Property;
      var compositionChain = animationManager.GetCompositionChain(property, null, false);
      if (compositionChain != null && compositionChain.Contains(this))
        return true;

      return false;
    }


    /// <inheritdoc/>
    internal override void AddToCompositionChains(AnimationManager animationManager, HandoffBehavior handoffBehavior, AnimationInstance previousInstance)
    {
      var property = Property;
      if (property == null)
        return;
      
      // Get (or create) the animation composition chain.
      var compositionChain = animationManager.GetCompositionChain(property, Animation.Traits, true);

      if (handoffBehavior == HandoffBehavior.SnapshotAndReplace)
      {
        // Make a snapshot of the current animation value.
        compositionChain.TakeSnapshot();
      }

      if (handoffBehavior == HandoffBehavior.Replace || handoffBehavior == HandoffBehavior.SnapshotAndReplace)
      {
        // Remove all previous animations.
        if (compositionChain.Count > 0)
        {
          var rootInstance = this.GetRoot();
          for (int i = compositionChain.Count - 1; i >= 0; i--)
          {
            var animationInstance = compositionChain[i];

            // Do not remove animation instances of the same animation tree!
            if (animationInstance.GetRoot() != rootInstance)
              compositionChain.RemoveAt(i);
          }
        }
      }

      AnimationInstance<T> referenceInstance = null;
      if (previousInstance != null)
      {
        // previousInstance is either a single animation instance or an animation tree that 
        // should be replaced. Find the instance that is assigned to the current property.
        referenceInstance = previousInstance.GetAssignedInstance(Property) as AnimationInstance<T>;
      }

      if (referenceInstance == null)
      {
        // Add at the end of the animation composition chain.
        compositionChain.Add(this);
      }
      else
      {
        // Insert in animation composition chain at a certain index.
        int index = compositionChain.IndexOf(referenceInstance);
        if (index == -1)
        {
          // The referenceInstance has not been applied to the current property.
          compositionChain.Add(this);
        }
        else
        {
          // Insert after referenceInstance.
          index++;

          // Other animation instances of the same animation tree might have already been
          // inserted after referenceInstance. To maintain the correct order we need to add 
          // this instance after the other instances of the same tree.
          var rootInstance = this.GetRoot();
          var numberOfInstances = compositionChain.Count;
          while (index < numberOfInstances && compositionChain[index].GetRoot() == rootInstance)
            index++;

          compositionChain.Insert(index, this);
        }
      }
    }


    /// <inheritdoc/>
    internal override void RemoveFromCompositionChains(AnimationManager animationManager)
    {
      var property = Property;
      var compositionChain = animationManager.GetCompositionChain(property, null, false);
      if (compositionChain != null)
        compositionChain.Remove(this);
    }


    /// <inheritdoc/>
    internal override void UpdateAndApply(AnimationManager animationManager)
    {
      var property = Property;
      if (property != null)
        animationManager.UpdateAndApplyAnimation(Property);
    }
    #endregion
  }
}
