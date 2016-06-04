// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Provides the base implementation for all easing functions.
  /// </summary>
  public abstract class EasingFunction : IEasingFunction
  {
    /// <summary>
    /// Gets or sets a value that indicates how the easing function interpolates.
    /// </summary>
    /// <value>
    /// The value of the <see cref="EasingMode"/> enumeration that indicates how the easing function
    /// interpolates.
    /// </value>
    public EasingMode Mode { get; set; }


    /// <summary>
    /// Determines the current progress of a transition.
    /// </summary>
    /// <param name="normalizedTime">
    /// The normalized time of the transition. (0 represents the start and 1 represents the end of
    /// the transition.)
    /// </param>
    /// <returns>
    /// The current progress of the transition. (0 represents the start and 1
    /// represents the end of the transition.)
    /// </returns>
    /// <exception cref="InvalidAnimationException">
    /// Invalid enumeration value set in property <see cref="Mode"/>.
    /// </exception>
    public float Ease(float normalizedTime)
    {
      switch (Mode)
      {
        case EasingMode.EaseIn:
          return EaseIn(normalizedTime);

        case EasingMode.EaseOut:
          return 1.0f - EaseIn(1.0f - normalizedTime);

        case EasingMode.EaseInOut:
          if (normalizedTime <= 0.5f)
            return 0.5f * EaseIn(2.0f * normalizedTime);

          return 0.5f + 0.5f * (1.0f - EaseIn(2.0f * (1 - normalizedTime)));

        default:
          throw new InvalidAnimationException("Invalid enumeration value set in property EasingFunction.Mode.");
      }
    }


    /// <summary>
    /// Evaluates the easing function.
    /// </summary>
    /// <param name="normalizedTime">The normalized time.</param>
    /// <returns>The current progress of the transition.</returns>
    protected abstract float EaseIn(float normalizedTime);
  }
}
