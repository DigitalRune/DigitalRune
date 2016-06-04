// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides additional data for a <see cref="Texture"/>.
  /// </summary>
  internal sealed class Texture2DEx : GraphicsResourceEx<Texture2D>
  {
    /// <summary>
    /// Gets the <see cref="Texture2DEx"/> for the specified <see cref="Texture2D"/>.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <returns>The <see cref="Texture2DEx"/>.</returns>
    public static Texture2DEx From(Texture2D texture)
    {
      return GetOrCreate<Texture2DEx>(texture, null);
    }
  }
}
