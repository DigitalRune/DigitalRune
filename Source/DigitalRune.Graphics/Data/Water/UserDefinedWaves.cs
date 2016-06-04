// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides a user-defined displacement and normal texture that define the water surface.
  /// </summary>
  /// <remarks>
  /// This class adds setters to the <see cref="WaterWaves"/> properties. All properties have to be
  /// specified by the user.
  /// </remarks>
  public class UserDefinedWaves : WaterWaves
  {
    /// <summary>
    /// Gets or sets the displacement map.
    /// </summary>
    /// <inheritdoc cref="WaterWaves.DisplacementMap"/>
    public new Texture2D DisplacementMap 
    {
      get { return base.DisplacementMap; }
      set { base.DisplacementMap = value; }
    }


    /// <summary>
    /// Gets or sets the normal map (using standard encoding, see remarks).
    /// </summary>
    /// <inheritdoc cref="WaterWaves.NormalMap"/>
    public new Texture2D NormalMap 
    {
      get { return base.NormalMap; }
      set { base.NormalMap = value; }
    }


    /// <summary>
    /// Gets or sets the size of a single tile (one texture repetition) in world space.
    /// </summary>
    /// <inheritdoc cref="WaterWaves.TileSize"/>
    public new float TileSize
    {
      get { return base.TileSize; }
      set { base.TileSize = value; }
    }


    /// <summary>
    /// Gets or sets the center of the first tile in world space.
    /// </summary>
    /// <inheritdoc cref="WaterWaves.TileCenter"/>
    public new Vector3F TileCenter
    {
      get { return base.TileCenter; }
      set { base.TileCenter = value; }
    }


    /// <summary>
    /// Gets or sets a value indicating whether the displacement map can be tiled seamlessly
    /// across the water surface.
    /// </summary>
    /// <inheritdoc cref="WaterWaves.IsTiling"/>
    public new bool IsTiling 
    {
      get { return base.IsTiling; }
      set { base.IsTiling = value; }
    }
  }
}
