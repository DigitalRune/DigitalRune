// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation
{
  /// <summary>
  /// Defines the behavior of an animation when it is repeated.
  /// </summary>
  public enum LoopBehavior
  {
    /// <summary>
    /// The animation value is constant. The animation repeats the nearest valid animation value
    /// (the start value if time &lt; start time, or the end value of the animation if time &gt;
    /// end time).
    /// </summary>
    Constant,

    /// <summary>
    /// The animation will be repeated if the time value goes past the end of the animation. (Note
    /// that the start and end value of cyclic animations needs to be identical to have smooth 
    /// transitions between iterations.)
    /// </summary>
    Cycle,

    /// <summary>
    /// The animation will be repeated if the time value goes past the end animation. Additionally, 
    /// the animation values of the next cycle will be offset by the difference between the end 
    /// value and start value of the animation to enable smooth transitions between iterations.
    /// (This behavior can be used to achieve cyclic animations that accumulate the animation values
    /// from one iteration to the next time.)
    /// </summary>
    CycleOffset,

    /// <summary>
    /// The animation will be automatically reversed and repeated. (This behavior is also known as
    /// 'auto-reverse' or 'ping-pong'.)
    /// </summary>
    Oscillate

    // Note: The loop type 'Linear' is only supported by animation curves or paths. Animations in 
    // general do not provide tangent information.
  }
}
