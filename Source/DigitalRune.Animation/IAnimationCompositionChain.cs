// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Manages a collection of animations that are combined and applied to a certain property.
  /// </summary>
  internal interface IAnimationCompositionChain : IList<AnimationInstance>
  {
    /// <summary>
    /// Gets a value indicating whether this composition chain is empty.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this composition chain is empty; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    bool IsEmpty { get; }


    /// <summary>
    /// Gets the target property that is being animated.
    /// </summary>
    /// <value>
    /// The target property that is being animated. (Returns <see langword="null"/> if the owner of 
    /// the property has been garbage collected.)
    /// </value>
    /// <remarks>
    /// The animated property is stored using a weak reference.
    /// </remarks>
    IAnimatableProperty Property { get; }


    /// <summary>
    /// Evaluates the animations in the composition chain. (Does not apply the result to the target
    /// property.)
    /// </summary>
    /// <param name="animationManager">The <see cref="AnimationManager"/>.</param>
    void Update(AnimationManager animationManager);


    /// <summary>
    /// Applies the result of the composition chain to the target property.
    /// </summary>
    void Apply();


    /// <summary>
    /// Recycles this instance of the <see cref="IAnimationCompositionChain"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    void Recycle();
  }
}
