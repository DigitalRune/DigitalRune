// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents LogLuv encoding of color values.
  /// </summary>
  /// <seealso href="http://www.anyhere.com/gward/papers/jgtpap1.pdf">The LogLuv Encoding for Full Gamut, High Dynamic Range Images</seealso>
  /// <seealso href="http://realtimecollisiondetection.net/blog/?p=15">Converting RGB to LogLuv in a fragment shader</seealso>.
  public class LogLuvEncoding : ColorEncoding
  {
    // LogLuv in DigitalRune Graphics stores color values in linear space.
  }
}
