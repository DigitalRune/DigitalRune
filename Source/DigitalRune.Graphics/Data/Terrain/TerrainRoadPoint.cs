// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Interpolation;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines a <see cref="PathKey3F"/> for a <see cref="Path3F"/> which defines a road.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Roads (see <see cref="TerrainRoadLayer"/>) are usually defined by 3D paths. A 3D path is a
  /// curve which goes through several path keys. Each path key defines a point on the curve and the
  /// spline interpolation between the point and the next point. When dealing with roads, you can
  /// use the <see cref="TerrainRoadPathKey"/> class for the path keys to provide additional
  /// information.
  /// </para>
  /// <para>
  /// <see cref="Width"/> defines the absolute with of the road at the path key.
  /// <see cref="SideFalloff"/> defines an additional border of the road where the road influences
  /// the terrain height. This property is used when the road is "carved" into a terrain (see
  /// <see cref="TerrainRoadLayer.ClampTerrainToRoad"/>. For example, if a road with a width of 5
  /// and a side falloff of 4 is carved into a terrain, the center 5 units contain the actual
  /// road and the terrain height is adjusted to match the road. The terrain height next to the road
  /// is interpolated between the original height and the road. The total width of the terrain
  /// influenced by the road is <see cref="SideFalloff"/> (left) + <see cref="Width"/> +
  /// <see cref="SideFalloff"/> (right) = 13 units.
  /// </para>
  /// </remarks>
  public class TerrainRoadPathKey : PathKey3F
  {
    //public float Tilt { get; set;  }   // Would be useful to tilt roads, e.g. in curves.


    /// <summary>
    /// Gets or sets the width of the road.
    /// </summary>
    /// <value>The width of the road. The default value is 5.</value>
    /// <remarks>
    /// See <see cref="TerrainRoadPathKey"/> for more details.
    /// </remarks>
    public float Width { get; set; }


    /// <summary>
    /// Gets or sets the side falloff.
    /// </summary>
    /// <value>The side falloff. The default value is 4.</value>
    /// <remarks>
    /// See <see cref="TerrainRoadPathKey"/> for more details.
    /// </remarks>
    public float SideFalloff { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainRoadPathKey"/> class.
    /// </summary>
    public TerrainRoadPathKey()
    {
      Width = 5;
      SideFalloff = 4;
    }
  }
}
