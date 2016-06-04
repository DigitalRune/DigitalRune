// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Defines a function that controls the pace of a transition.
  /// </summary>
  public interface IEasingFunction
  {
    /// <summary>
    /// Determines the current progress of a transition.
    /// </summary>
    /// <param name="normalizedTime">
    /// The normalized time of the transition. (0 represents the start and 1 represents the end of
    /// the transition.)
    /// </param>
    /// <returns>
    /// The current progress of the transition. (0 represents the start and 1 represents the end of
    /// the transition.)
    /// </returns>
    float Ease(float normalizedTime);
  }
}
