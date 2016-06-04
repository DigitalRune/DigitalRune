// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines a displacement of the water surface to create waves.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Water waves are defined using a <see cref="DisplacementMap"/> and a <see cref="NormalMap"/>.
  /// These maps are used to deform the water surface (usually by displacing the vertices in the
  /// vertex shader).
  /// </para>
  /// <para>
  /// <see cref="DisplacementMap"/> and <see cref="NormalMap"/> are expected to have the same
  /// resolution. The <see cref="DisplacementMap"/> can be <see langword="null"/> if only the normal
  /// map should be used.
  /// </para>
  /// </remarks>
  public abstract class WaterWaves : IDisposable
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------    

    /// <summary>
    /// Gets (or sets) the displacement map.
    /// </summary>
    /// <value>The displacement map.</value>
    /// <remarks>
    /// The R channel contains the x displacement. The G channel contains the y (height)
    /// displacement. The B channel contains the z displacement.
    /// </remarks>
    public Texture2D DisplacementMap { get; protected internal set; }


    /// <summary>
    /// Gets (or sets) the normal map (using standard encoding, see remarks).
    /// </summary>
    /// <value>The normal map (using standard encoding, see remarks).</value>
    /// <remarks>
    /// The normal map value will be decoded in the shader using 
    /// <c>normal = tex2D(NormalSampler, texCoord) * 2 - 1</c>
    /// </remarks>
    public Texture2D NormalMap { get; protected internal set; }


    /// <summary>
    /// Gets (or sets) the size of a single tile (one texture repetition) in world space.
    /// </summary>
    /// <value>The size of a single tile (one texture repetition) in world space.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The tile size must be positive and finite.
    /// </exception>
    public float TileSize
    {
      get { return _tileSize; }
      protected set
      {
        if (!Numeric.IsPositiveFinite(value))
          throw new ArgumentOutOfRangeException("value", "The tile size must be positive and finite.");

        _tileSize = value;
      }
    }
    private float _tileSize;


    /// <summary>
    /// Gets (or sets) the center of the first tile in world space.
    /// </summary>
    /// <value>
    /// The center of the first tile in world space. (Only x and z are relevant. y is ignored.)
    /// </value>
    public Vector3F TileCenter { get; protected set; }


    /// <summary>
    /// Gets (or sets) a value indicating whether the displacement map can be tiled seamlessly
    /// across the water surface.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the texture repeats seamlessly; otherwise,
    /// <see langword="false"/>.
    /// </value>
    public bool IsTiling { get; protected set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="WaterWaves"/> class.
    /// </summary>
    protected WaterWaves()
    {
      TileSize = 1;
      IsTiling = true;
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="WaterWaves"/> class.
    /// </summary>
    /// <remarks>
    /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in 
    /// <see langword="true"/>, and then suppresses finalization of the instance.
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="WaterWaves"/> class
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        DisplacementMap.SafeDispose();
        DisplacementMap = null;

        NormalMap.SafeDispose();
        NormalMap = null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
