// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Media;


namespace DigitalRune.Windows
{

  // Note: ComponentResourceKeys are not properly supported in Expression Blend!
  // This file is not built. It is kept only for reference.


  /// <summary>
  /// Contains resource keys used by DigitalRune controls. 
  /// </summary>
  public class DigitalRuneResources
  {
    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    #region ----- Brushes -----

    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the checker <see cref="Brush"/>.
    /// </summary>
    public static ResourceKey CheckerBrushKey
    {
      get
      {
        if (_checkerBrushKey == null)
          _checkerBrushKey = CreateKey(DigitalRuneResourceKeyID.Checker);

        return _checkerBrushKey;
      }
    }
    private static ResourceKey _checkerBrushKey;


    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the <see cref="Brush"/> that is used to fill glyphs.
    /// </summary>
    public static ResourceKey GlyphFillBrushKey
    {
      get
      {
        if (_glyphFillBrushKey == null)
          _glyphFillBrushKey = CreateKey(DigitalRuneResourceKeyID.GlyphFill);

        return _glyphFillBrushKey;
      }
    }
    private static ResourceKey _glyphFillBrushKey;


    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the <see cref="Brush"/> that is used to draw the outline of glyphs.
    /// </summary>
    public static ResourceKey GlyphStrokeBrushKey
    {
      get
      {
        if (_glyphStrokeBrushKey == null)
          _glyphStrokeBrushKey = CreateKey(DigitalRuneResourceKeyID.GlyphStroke);

        return _glyphStrokeBrushKey;
      }
    }
    private static ResourceKey _glyphStrokeBrushKey;
    #endregion


    #region ----- Geometries -----

    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the <see cref="Geometry"/> that defines a triangle
    /// that points left.
    /// </summary>
    public static ResourceKey LeftTriangleGeometryKey
    {
      get
      {
        if (_leftTriangleGeometryKey == null)
          _leftTriangleGeometryKey = CreateKey(DigitalRuneResourceKeyID.LeftTriangle);

        return _leftTriangleGeometryKey;
      }
    }
    private static ResourceKey _leftTriangleGeometryKey;


    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the <see cref="Geometry"/> that defines a triangle
    /// that points right.
    /// </summary>
    public static ResourceKey RightTriangleGeometryKey
    {
      get
      {
        if (_rightTriangleGeometryKey == null)
          _rightTriangleGeometryKey = CreateKey(DigitalRuneResourceKeyID.RightTriangle);

        return _rightTriangleGeometryKey;
      }
    }
    private static ResourceKey _rightTriangleGeometryKey;


    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the <see cref="Geometry"/> that defines a triangle
    /// that points down.
    /// </summary>
    public static ResourceKey DownTriangleGeometryKey
    {
      get
      {
        if (_downTriangleGeometryKey == null)
          _downTriangleGeometryKey = CreateKey(DigitalRuneResourceKeyID.DownTriangle);

        return _downTriangleGeometryKey;
      }
    }
    private static ResourceKey _downTriangleGeometryKey;


    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the <see cref="Geometry"/> that defines a triangle
    /// that points up.
    /// </summary>
    public static ResourceKey UpTriangleGeometryKey
    {
      get
      {
        if (_upTriangleGeometryKey == null)
          _upTriangleGeometryKey = CreateKey(DigitalRuneResourceKeyID.UpTriangle);

        return _upTriangleGeometryKey;
      }
    }
    private static ResourceKey _upTriangleGeometryKey;


    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the <see cref="Geometry"/> that defines a symmetric 
    /// cross (like an X).
    /// </summary>
    public static ResourceKey CrossGeometryKey
    {
      get
      {
        if (_crossGeometryKey == null)
          _crossGeometryKey = CreateKey(DigitalRuneResourceKeyID.Cross);

        return _crossGeometryKey;
      }
    }
    private static ResourceKey _crossGeometryKey;


    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the <see cref="Geometry"/> that defines a magnifying
    /// glass.
    /// </summary>
    public static ResourceKey MagnifierGeometryKey
    {
      get
      {
        if (_magnifierGeometryKey == null)
          _magnifierGeometryKey = CreateKey(DigitalRuneResourceKeyID.Magnifier);

        return _magnifierGeometryKey;
      }
    }
    private static ResourceKey _magnifierGeometryKey;


    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the <see cref="Geometry"/> that defines an arrow
    /// that points left.
    /// </summary>
    public static ResourceKey LeftArrowGeometryKey
    {
      get
      {
        if (_leftArrowGeometryKey == null)
          _leftArrowGeometryKey = CreateKey(DigitalRuneResourceKeyID.LeftArrow);

        return _leftArrowGeometryKey;
      }
    }
    private static ResourceKey _leftArrowGeometryKey;


    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the <see cref="Geometry"/> that defines an arrow
    /// that points right.
    /// </summary>
    public static ResourceKey RightArrowGeometryKey
    {
      get
      {
        if (_rightArrowGeometryKey == null)
          _rightArrowGeometryKey = CreateKey(DigitalRuneResourceKeyID.RightArrow);

        return _rightArrowGeometryKey;
      }
    }
    private static ResourceKey _rightArrowGeometryKey;


    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the <see cref="Geometry"/> that defines an arrow
    /// that points down.
    /// </summary>
    public static ResourceKey DownArrowGeometryKey
    {
      get
      {
        if (_downArrowGeometryKey == null)
          _downArrowGeometryKey = CreateKey(DigitalRuneResourceKeyID.DownArrow);

        return _downArrowGeometryKey;
      }
    }
    private static ResourceKey _downArrowGeometryKey;


    /// <summary>
    /// Gets the <see cref="ResourceKey"/> for the <see cref="Geometry"/> that defines an arrow
    /// that points up.
    /// </summary>
    public static ResourceKey UpArrowGeometryKey
    {
      get
      {
        if (_upArrowGeometryKey == null)
          _upArrowGeometryKey = CreateKey(DigitalRuneResourceKeyID.UpArrow);

        return _upArrowGeometryKey;
      }
    }
    private static ResourceKey _upArrowGeometryKey;
    #endregion

    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private static ResourceKey CreateKey(DigitalRuneResourceKeyID id)
    {
      return new ComponentResourceKey(typeof(DigitalRuneResources), id);
    }
    #endregion
  }
}
