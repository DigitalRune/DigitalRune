// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines how a curve is continued before the first curve key or after the last curve key.
  /// </summary>
  public enum CurveLoopType
  {
    /// <summary>
    /// The curve value is constant and equal to the nearest key.
    /// </summary>
    Constant,
    
    /// <summary>
    /// The curve value is a linear extrapolation of the nearest key value in the direction of the
    /// tangent. 
    /// </summary>
    Linear,
    
    /// <summary>
    /// Parameters specified past the ends of the curve will wrap around to the opposite side of the
    /// curve. If the values of the first and last key are different, the value will "jump" 
    /// instantly from one value to the other at the curve ends.
    /// </summary> 
    Cycle,
    
    /// <summary>
    /// Same as <see cref="Cycle"/> but the curve values are offset by the difference of the first
    /// and last key value. Unlike <see cref="Cycle"/> the curve is continued without "jumps" at
    /// the curve ends. 
    /// </summary>
    CycleOffset,
    
    /// <summary>
    /// Parameters specified past the ends of the curve act as an offset from the same side of the 
    /// curve toward the opposite side. This is similar to <see cref="Cycle"/> where the curve is
    /// mirrored beyond the curve ends.
    /// </summary>
    Oscillate,
  }
}
