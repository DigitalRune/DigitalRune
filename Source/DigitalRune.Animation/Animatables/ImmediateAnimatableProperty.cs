// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation
{
  /// <summary>
  /// Represents an <see cref="IAnimatableProperty"/> which needs to be applied immediately in the
  /// animation system.
  /// </summary>
  /// <remarks>
  /// These types of properties have priority and are treated special by the animation system.
  /// </remarks>
  internal interface IImmediateAnimatableProperty : IAnimatableProperty
  {
  }


  /// <summary>
  /// Represents an <see cref="AnimatableProperty{T}"/> which needs to be applied immediately in the
  /// animation system.
  /// </summary>
  /// <typeparam name="T">The type of the property.</typeparam>
  /// <inheritdoc/>
  internal sealed class ImmediateAnimatableProperty<T> : AnimatableProperty<T>, IImmediateAnimatableProperty
  {
  }
}
