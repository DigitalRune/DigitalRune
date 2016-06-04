// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Animation.Traits;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Defines the change of a value over time.
  /// </summary>
  /// <remarks>
  /// <para>
  /// An animation in the traditional sense is the rapid display of images to create the illusion of
  /// movement. Here, in <strong>DigitalRune Animation</strong>, the term animation is used in a 
  /// more general way: An animation determines the change of value over time. 
  /// </para>
  /// <para>
  /// Animations can be applied to all kinds of properties in an application. In a user-interface 
  /// properties such as size, position, rotation, color, opacity, etc. can be animated to create 
  /// interesting visual effects. In virtual worlds more complex animations can be used to control 
  /// the motions of objects or virtual characters.
  /// </para>
  /// <para>
  /// An animation defines the transition of a value over time. It is basically a function 
  /// <i>x</i> = <i>f</i>(<i>t</i>) where the parameter <i>t</i> is called the <i>animation time</i>
  /// and the result <i>x</i> is the <i>animation value</i>. The method 
  /// <see cref="IAnimation{T}.GetValue"/> can be used to evaluate the animation value.
  /// </para>
  /// <para>
  /// <strong>Target Object and Property:</strong> An animation can be applied to a property of a 
  /// given object. The animation system allows to apply an animation to an object or a group of 
  /// objects without explicitly specifying the property that should be animated. In this case the 
  /// animation system automatically matches the target object and properties using the properties 
  /// <see cref="ITimeline.TargetObject"/> and <see cref="TargetProperty"/>. 
  /// <see cref="ITimeline.TargetObject"/> and <see cref="TargetProperty"/> are optional strings
  /// that can be set to identify the target.
  /// </para>
  /// <para>
  /// For example, a <see cref="TimelineGroup"/> can be used to define a complex set of animations. 
  /// Each animation is assigned to an object/property by setting 
  /// <see cref="ITimeline.TargetObject"/> and <see cref="TargetProperty"/>. At runtime, when the 
  /// <see cref="TimelineGroup"/> is started the animation system will automatically apply the
  /// animations to the matching objects.
  /// </para>
  /// <para>
  /// The animation system first checks whether the property <see cref="ITimeline.TargetObject"/> is
  /// set and matches the name of any of the given objects. The animation system assumes that all
  /// objects have a unique name. Therefore, as soon as one match is found the animation system
  /// ignores all remaining objects/properties. If a match is found, it checks whether the object
  /// has a matching animatable property. If the property <see cref="ITimeline.TargetObject"/> is
  /// not set (the value is <see langword="null"/> or an empty string), the animation system checks
  /// all objects for a matching property.
  /// </para>
  /// <para>
  /// The animation system checks whether an animatable property matches the property 
  /// <see cref="TargetProperty"/> and whether the type of the property is compatible with the 
  /// animation. An animation is applied to the first matching property. If there are multiple 
  /// potential matches the animation is only applied to the first match.
  /// </para>
  /// <para>
  /// Note that timelines can be nested. In this case the parent's 
  /// <see cref="ITimeline.TargetObject"/> or <see cref="TargetProperty"/> property overrides the 
  /// values set by child animations. For example, if the property 
  /// <see cref="ITimeline.TargetObject"/> is set in a <see cref="TimelineGroup"/> the value 
  /// overrides the <see cref="ITimeline.TargetObject"/> properties of all child 
  /// animations/timelines.
  /// </para>
  /// <para>
  /// <strong>Additive Animations:</strong> Animations can be additive, meaning that the animation
  /// value is added to the target property. If there are multiple animation in an composition 
  /// chain, the animation value will be added to the output of the previous stage. If the animation 
  /// is the first animation in a composition chain, the animation value is added to the base value 
  /// of the property that is being animated.
  /// </para>
  /// </remarks>
  public interface IAnimation : ITimeline
  {
    /// <summary>
    /// Gets the property to which the animation is applied by default.
    /// </summary>
    /// <value>The property to which the animation is applied by default.</value>
    /// <remarks>
    /// See <see cref="IAnimation{T}"/> for more information.
    /// </remarks>
    string TargetProperty { get; }


    /// <summary>
    /// Creates a new <see cref="BlendAnimation{T}"/>. (For internal use only.)
    /// </summary>
    /// <returns>
    /// A new <see cref="BlendAnimation{T}"/>.
    /// </returns>
    BlendAnimation CreateBlendAnimation();
  }


  /// <summary>
  /// Defines the change of a value over time.
  /// </summary>
  /// <typeparam name="T">The type of the animation value.</typeparam>
  /// <inheritdoc/>
  public interface IAnimation<T> : IAnimation 
  {
    /// <summary>
    /// Gets the traits of the animation values.
    /// </summary>
    /// <value>The traits of the animation values.</value>
    IAnimationValueTraits<T> Traits { get; }


    /// <summary>
    /// Gets the value of the animation at the specified time.
    /// </summary>
    /// <param name="time">The time value on the timeline.</param>
    /// <param name="defaultSource">
    /// In: The source value that should be used by the animation if the animation does not have its 
    /// own source value.
    /// </param>
    /// <param name="defaultTarget">
    /// In: The target value that should be used by the animation if the animation does not have its 
    /// own target value.
    /// </param>
    /// <param name="result">
    /// Out: The value of the animation at the given time. (The animation returns 
    /// <paramref name="defaultSource"/> if the animation is <see cref="AnimationState.Delayed"/> 
    /// or <see cref="AnimationState.Stopped"/> at <paramref name="time"/>.)
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
    void GetValue(TimeSpan time, ref T defaultSource, ref T defaultTarget, ref T result);
  }
}
