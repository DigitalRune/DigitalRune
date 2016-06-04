// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Defines settings for a projected grid.
  /// </summary>
  /// <remarks>
  /// A projected grid is a grid defined in screen space. It is projected into the world to draw
  /// objects, like an infinite ocean, with high resolution near the camera and low resolution in
  /// the distance.
  /// </remarks>
  public class ProjectedGridParameters
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly GraphicsDevice _graphicsDevice;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the horizontal resolution of the grid.
    /// </summary>
    /// <value>The horizontal resolution of the grid (number of cells).</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is 0 or negative.
    /// </exception>
    public int Width
    {
      get { return _width; }
      set
      {
        if (value == _width)
          return;

        if (value < 1)
          throw new ArgumentOutOfRangeException("value", "Width must be greater than 0.");

        _width = value;
        Dispose();
      }
    }
    private int _width;


    /// <summary>
    /// Gets or sets the vertical resolution of the grid.
    /// </summary>
    /// <value>The vertical resolution of the grid (number of cells).</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is 0 or negative.
    /// </exception>
    public int Height
    {
      get { return _height; }
      set
      {
        if (value == _height)
          return;

        if (value < 1)
          throw new ArgumentOutOfRangeException("value", "Height must be greater than 0.");

        _height = value;
        Dispose();
      }
    }
    private int _height;


    /// <summary>
    /// Gets or sets the camera offset.
    /// </summary>
    /// <value>The camera offset. Must be a positive value.</value>
    /// <remarks>
    /// This offset is used to move the camera which projects the grid behind the player camera.
    /// This will cause part of the grid to be outside of the player camera's field of view. This
    /// additional border is needed when the grid points are displaced using a displacement map.
    /// Without the camera offset, the grid edge could be visible in the player's field of view.
    /// </remarks>
    public float Offset { get; set; }


    /// <summary>
    /// Gets or sets the edge attenuation.
    /// </summary>
    /// <value>The edge attenuation in [0, 1] relative to the whole projected grid.</value>
    /// <remarks>
    /// <para>
    /// If the projected grid is displaced in world space, the displacement fades out near the
    /// borders of the projected grid. This helps to hide any artifacts which could occur near
    /// the grid borders (e.g. the grid border being displaced into the visible field of view).
    /// </para>
    /// <para>
    /// An <see cref="EdgeAttenuation"/> of 0.01 means that the displacement fades out in the outer
    /// 1 % of the grid.
    /// </para>
    /// </remarks>
    public float EdgeAttenuation { get; set; }


    /// <summary>
    /// Gets or sets the start distance for distance-based attenuation.
    /// </summary>
    /// <value>The start distance for distance-based attenuation. In world space units.</value>
    /// <remarks>
    /// The projected grid might cause aliasing in the distance. To avoid artifacts, any grid
    /// displacement should be faded out. <see cref="DistanceAttenuationStart"/> defines the world
    /// space distance from the camera where the fade out starts.
    /// <see cref="DistanceAttenuationEnd"/> defines the distance beyond which all displacement is
    /// disabled.
    /// </remarks>
    public float DistanceAttenuationStart { get; set; }


    /// <summary>
    /// Gets or sets the end distance for distance-based attenuation.
    /// </summary>
    /// <value>The end distance for distance-based attenuation. In world space units.</value>
    /// <inheritdoc cref="DistanceAttenuationStart"/>
    public float DistanceAttenuationEnd { get; set; }


    internal Submesh Submesh
    {
      get
      {
        if (_submesh == null)
          _submesh = MeshHelper.CreateGrid(_graphicsDevice, 1, 1, Width, Height);

        return _submesh;
      }
    }
    private Submesh _submesh;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    internal ProjectedGridParameters(GraphicsDevice graphicsDevice)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");

      _graphicsDevice = graphicsDevice;

      Width = 128;
      Height = 128;
      Offset = 1;
      EdgeAttenuation = 0.1f;
      DistanceAttenuationStart = 20;
      DistanceAttenuationEnd = 300;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    internal void Dispose()
    {
      if (_submesh != null)
      {
        _submesh.Dispose();
        _submesh = null;
      }
    }
    #endregion
  }
}
