// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Animation.Transitions;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Exposes the functionality of the animation system.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The animation service can be used to start/stop animations directly or create animation 
  /// controllers, which can be used to interactively control animations.
  /// </para>
  /// <para>
  /// <strong>Animatable Objects:</strong> Animations can be applied to objects that implement the
  /// interface <see cref="IAnimatableObject"/> or properties that implement 
  /// <see cref="IAnimatableProperty{T}"/>. The class <see cref="AnimatableProperty{T}"/> can be 
  /// used to create a standalone property which can be animated. The 
  /// <see cref="DelegateAnimatableProperty{T}"/> can be used to wrap an existing property or field
  /// and make it "animatable".
  /// </para>
  /// <para>
  /// <strong>Important:</strong> When animations are started or stopped the animations do not take 
  /// effect immediately. That means the new animation values are not immediately applied to the 
  /// properties that are being animated. The animations are evaluated when the animation system is
  /// updated (see <see cref="AnimationManager.Update"/>) and new animation values are written when 
  /// <see cref="AnimationManager.ApplyAnimations"/> is called.
  /// </para>
  /// <para>
  /// The method <see cref="UpdateAndApplyAnimation(IAnimatableProperty)"/> (or one of its 
  /// overloads) can be called to immediately evaluate and apply animations. But in most cases it is
  /// not necessary to call this method explicitly.
  /// </para>
  /// <para>
  /// <strong>Weak References:</strong> The animated objects and properties are stored using weak 
  /// references in the animation system. This means, animations can be started in a 
  /// "fire-and-forget" manner. The caller does not have to worrying about "memory leaks". If a 
  /// target object is garbage collected the animation system will automatically remove all 
  /// associated animations and resources. This clean-up happens regularly when 
  /// <see cref="AnimationManager.Update"/> is called.
  /// </para>
  /// <para>
  /// Note however, by registering a completion event handler for an animation (see 
  /// <see cref="AnimationInstance.Completed"/>) a strong reference is created from the animation to
  /// the event handler. If the event handler accidentally keeps the animated object or properties 
  /// alive then the animation is not removed automatically. Therefore, use the completion event 
  /// handlers with caution. See <see cref="AnimationInstance.Completed"/> for more details.
  /// </para>
  /// <para>
  /// <strong>Thread-Safety:</strong> The animation service in general is not thread-safe. It is not
  /// allowed to simultaneously start multiple animations in different threads. Access to the 
  /// animation service needs to be synchronized!
  /// </para>
  /// </remarks>
  public interface IAnimationService
  {
    /// <overloads>
    /// <summary>
    /// Determines whether an object or property is controlled by animations.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified object is controlled by one or more animations.
    /// </summary>
    /// <param name="animatableObject">The object.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="animatableObject"/> is animated; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animatableObject"/> is <see langword="null"/>.
    /// </exception>
    bool IsAnimated(IAnimatableObject animatableObject);


    /// <summary>
    /// Determines whether the specified property is controlled by one or more animations.
    /// </summary>
    /// <param name="animatableProperty">The property.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="animatableProperty"/> is animated; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animatableProperty"/> is <see langword="null"/>.
    /// </exception>
    bool IsAnimated(IAnimatableProperty animatableProperty);


    /// <overloads>
    /// <summary>
    /// Creates a new animation controller which can be used to apply the given animation to the
    /// specified objects or properties.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates a new animation controller which can be used to apply the given animation to the
    /// specified objects.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="targetObjects">The target objects that should be animated.</param>
    /// <returns>The <see cref="AnimationController"/>.</returns>
    /// <remarks>
    /// The returned animation controller can be used to interactively control the animation.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="targetObjects"/> or <paramref name="animation"/> is <see langword="null"/>.
    /// </exception>
    AnimationController CreateController(ITimeline animation, IEnumerable<IAnimatableObject> targetObjects);


    /// <summary>
    /// Creates a new animation controller which can be used to apply the given animation to the
    /// specified object.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="targetObject">The target object that should be animated.</param>
    /// <returns>The <see cref="AnimationController"/>.</returns>
    /// <inheritdoc cref="CreateController(ITimeline,IEnumerable{IAnimatableObject})"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="targetObject"/> or <paramref name="animation"/> is <see langword="null"/>.
    /// </exception>
    AnimationController CreateController(ITimeline animation, IAnimatableObject targetObject);


    /// <summary>
    /// Creates a new animation controller which can be used to apply the given animation to the
    /// specified property.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="targetProperty">The target property that should be animated.</param>
    /// <returns>The <see cref="AnimationController"/>.</returns>
    /// <inheritdoc cref="CreateController(ITimeline,IEnumerable{IAnimatableObject})"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="targetProperty"/> or <paramref name="animation"/> is <see langword="null"/>.
    /// </exception>
    AnimationController CreateController(ITimeline animation, IAnimatableProperty targetProperty);


    /// <overloads>
    /// <summary>
    /// Starts an animation on the specified objects or properties.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Starts an animation and applies it to the specified objects.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="targetObjects">The target objects that should be animated.</param>
    /// <returns>The <see cref="AnimationController"/>.</returns>
    /// <remarks>
    /// <para>
    /// The returned animation controller can be used to interactively control the animation.
    /// </para>
    /// <para>
    /// If no <see cref="AnimationTransition"/> is specified explicitly, then 
    /// <see cref="AnimationTransitions.SnapshotAndReplace"/> will be used. 
    /// </para>
    /// <para>
    /// <strong>Important:</strong> When animations are started or stopped the animations do not 
    /// take effect immediately. That means the new animation values are not immediately applied to 
    /// the properties that are being animated. The animations are evaluated when the animation 
    /// system is updated (see <see cref="AnimationManager.Update"/>) and new animation values are
    /// written when <see cref="AnimationManager.ApplyAnimations"/> is called.
    /// </para>
    /// <para>
    /// The method <see cref="AnimationController.UpdateAndApply"/> can be called to immediately 
    /// evaluate and apply the animation. But in most cases it is not necessary to call this method 
    /// explicitly.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="targetObjects"/> or <paramref name="animation"/> is <see langword="null"/>.
    /// </exception>
    AnimationController StartAnimation(ITimeline animation, IEnumerable<IAnimatableObject> targetObjects);


    /// <summary>
    /// Starts an animation using a given transition and applies it to the specified objects.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="targetObjects">The target objects that should be animated.</param>
    /// <param name="transition">
    /// The transition that determines how the new animation is applied. The class 
    /// <see cref="AnimationTransitions"/> provides a set of predefined animation transitions.
    /// </param>
    /// <returns>The <see cref="AnimationController"/>.</returns>
    /// <inheritdoc cref="StartAnimation(ITimeline,IEnumerable{IAnimatableObject})"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="targetObjects"/> or <paramref name="animation"/> is <see langword="null"/>.
    /// </exception>
    AnimationController StartAnimation(ITimeline animation, IEnumerable<IAnimatableObject> targetObjects, AnimationTransition transition);


    /// <summary>
    /// Starts an animation and applies it to the specified object.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="targetObject">The target object that should be animated.</param>
    /// <returns>The <see cref="AnimationController"/>.</returns>
    /// <inheritdoc cref="StartAnimation(ITimeline,IEnumerable{IAnimatableObject})"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="targetObject"/> or <paramref name="animation"/> is <see langword="null"/>.
    /// </exception>
    AnimationController StartAnimation(ITimeline animation, IAnimatableObject targetObject);


    /// <summary>
    /// Starts an animation using a given transition and applies it to the specified object.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="targetObject">The target object that should be animated.</param>
    /// <param name="transition">
    /// The transition that determines how the new animation is applied. The class 
    /// <see cref="AnimationTransitions"/> provides a set of predefined animation transitions.
    /// </param>
    /// <returns>The <see cref="AnimationController"/>.</returns>
    /// <inheritdoc cref="StartAnimation(ITimeline,IEnumerable{DigitalRune.Animation.IAnimatableObject})"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="targetObject"/> or <paramref name="animation"/> is <see langword="null"/>.
    /// </exception>
    AnimationController StartAnimation(ITimeline animation, IAnimatableObject targetObject, AnimationTransition transition);


    /// <summary>
    /// Starts an animation and applies it to the specified property.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="targetProperty">The target property that should be animated.</param>
    /// <returns>The <see cref="AnimationController"/>.</returns>
    /// <inheritdoc cref="StartAnimation(ITimeline,IEnumerable{IAnimatableObject})"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="targetProperty"/> or <paramref name="animation"/> is <see langword="null"/>.
    /// </exception>
    AnimationController StartAnimation(ITimeline animation, IAnimatableProperty targetProperty);


    /// <summary>
    /// Starts an animation using a given transition and applies it to the specified property.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="targetProperty">The target property that should be animated.</param>
    /// <param name="transition">
    /// The transition that determines how the new animation is applied. The class 
    /// <see cref="AnimationTransitions"/> provides a set of predefined animation transitions.
    /// </param>
    /// <returns>The <see cref="AnimationController"/>.</returns>
    /// <inheritdoc cref="StartAnimation(ITimeline,IEnumerable{IAnimatableObject})"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="targetProperty"/> or <paramref name="animation"/> is <see langword="null"/>.
    /// </exception>
    AnimationController StartAnimation(ITimeline animation, IAnimatableProperty targetProperty, AnimationTransition transition);


    /// <overloads>
    /// <summary>
    /// Stops animations.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Stops all animations affecting the specified objects.
    /// </summary>
    /// <param name="animatedObjects">The animated objects.</param>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> When animations are started or stopped the animations do not 
    /// take effect immediately. That means the new animation values are not immediately applied to 
    /// the properties that are being animated. The animations are evaluated when the animation 
    /// system is updated (see <see cref="AnimationManager.Update"/>) and new animation values are
    /// written when <see cref="AnimationManager.ApplyAnimations"/> is called.
    /// </para>
    /// <para>
    /// The method <see cref="AnimationController.UpdateAndApply"/> can be called to immediately 
    /// evaluate and apply the animation. But in most cases it is not necessary to call this method 
    /// explicitly.
    /// </para>
    /// </remarks>
    void StopAnimation(IEnumerable<IAnimatableObject> animatedObjects);


    /// <summary>
    /// Stops all animations affecting the specified object.
    /// </summary>
    /// <param name="animatedObject">The animated object.</param>
    void StopAnimation(IAnimatableObject animatedObject);


    /// <summary>
    /// Stops all animations affecting the specified property.
    /// </summary>
    /// <param name="animatedProperty">The animated property.</param>
    void StopAnimation(IAnimatableProperty animatedProperty);


    /// <overloads>
    /// <summary>
    /// Immediately evaluates the specified animations and applies the new animation values.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Immediately evaluates the animations of the given objects and applies the new animation 
    /// values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When an animations are started or stopped, the values of the animated properties do not 
    /// change immediately. The new animation values will be computed and set when the animation 
    /// system is updated. See <see cref="AnimationManager.Update"/> and 
    /// <see cref="AnimationManager.ApplyAnimations"/>.
    /// </para>
    /// <para>
    /// But in certain cases when animations are started or stopped the animated properties should 
    /// be updated immediately. In these case the method 
    /// <see cref="UpdateAndApplyAnimation(IAnimatableProperty)"/> (or one of its overloads) needs 
    /// to be called after the animations are started or stopped. This method immediately evaluates 
    /// the animations and applies the new animation values to the specified objects or properties.
    /// </para>
    /// <para>
    /// The method can also be called if animations are modified (e.g. key frames are added or 
    /// removed) and the changes should take effect immediately.
    /// </para>
    /// <para>
    /// In most cases it is not necessary to call this method because the animation system updates 
    /// and applies animations automatically. 
    /// </para>
    /// <para>
    /// Note that <see cref="UpdateAndApplyAnimation(IAnimatableProperty)"/> does not advance the 
    /// time of the animations. The animations are evaluated at their current time.
    /// </para>
    /// </remarks>
    /// <param name="animatedObjects">The animated objects.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animatedObjects" /> is <see langword="null"/>.
    /// </exception>
    void UpdateAndApplyAnimation(IEnumerable<IAnimatableObject> animatedObjects);


    /// <summary>
    /// Immediately evaluates the animations the given object and applies the new animation values.
    /// </summary>
    /// <param name="animatedObject">The animated object.</param>
    /// <inheritdoc cref="UpdateAndApplyAnimation(IEnumerable{IAnimatableObject})"/>
    void UpdateAndApplyAnimation(IAnimatableObject animatedObject);


    /// <summary>
    /// Immediately evaluates the animation composition chains of the given property and applies
    /// the new animation values.
    /// </summary>
    /// <param name="property">The property that needs to be updated.</param>
    /// <inheritdoc cref="UpdateAndApplyAnimation(IEnumerable{IAnimatableObject})"/>
    void UpdateAndApplyAnimation(IAnimatableProperty property);
  }
}
