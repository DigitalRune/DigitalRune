// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Defines an area within which you can explicitly position child elements by using coordinates 
  /// that are relative to the <see cref="Canvas"/> area.
  /// </summary>
  /// <remarks>
  /// To place controls on a canvas, add the controls to the <see cref="Panel.Children"/> 
  /// collection. Set the properties <see cref="UIControl.X"/> and <see cref="UIControl.Y"/> to 
  /// position the controls.
  /// </remarks>
  public class Canvas : Panel
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Canvas"/> class.
    /// </summary>
    public Canvas()
    {
      Style = "Canvas";
    }


    /// <inheritdoc/>
    protected override Vector2F OnMeasure(Vector2F availableSize)
    {
      // Measure children. 
      foreach (var child in VisualChildren)
        child.Measure(new Vector2F(float.PositiveInfinity));

      float width = Width;
      float height = Height;
      bool hasWidth = Numeric.IsPositiveFinite(width);
      bool hasHeight = Numeric.IsPositiveFinite(height);

      // When computing the desired size.
      // (The Canvas checks UIControl.X/Y. Other controls do not do this.)
      Vector2F desiredSize = Vector2F.Zero;
      if (hasWidth)
      {
        desiredSize.X = width;
      }
      else
      {
        foreach (var child in VisualChildren)
          desiredSize.X = Math.Max(desiredSize.X, child.X + child.DesiredWidth);
      }

      if (hasHeight)
      {
        desiredSize.Y = height;
      }
      else
      {
        foreach (var child in VisualChildren)
          desiredSize.Y = Math.Max(desiredSize.Y, child.Y + child.DesiredHeight);
      }

      return desiredSize;
    }
  }
}
