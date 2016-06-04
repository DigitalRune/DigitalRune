// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Game.UI.Controls
{
  public partial class UIControl
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _isMeasureInProgress;
    private bool _isArrangeInProgress;

    // The results of the last layout process.
    private Vector2F _lastMeasureConstraintSize = new Vector2F(float.NaN);
    private Vector2F _lastArrangePosition = new Vector2F(float.NaN);
    private Vector2F _lastArrangeSize = new Vector2F(float.NaN);
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the desired width (including <see cref="Margin"/>) (which is computed in
    /// <see cref="Measure"/>).
    /// </summary>
    /// <value>The desired width (including <see cref="Margin"/>).</value>
    public float DesiredWidth { get; private set; }


    /// <summary>
    /// Gets the desired height (including <see cref="Margin"/>) (which is computed in
    /// <see cref="Measure"/>).
    /// </summary>
    /// <value>The desired height (including <see cref="Margin"/>).</value>
    public float DesiredHeight { get; private set; }


    /// <summary>
    /// Gets a value indicating whether the <see cref="Measure"/> results 
    /// (<see cref="DesiredWidth"/> and <see cref="DesiredHeight"/>) are up-to-date.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if <see cref="Measure"/> results are up-to-date; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This flag is set to <see langword="false"/> when <see cref="InvalidateMeasure"/> is called.
    /// </remarks>
    public bool IsMeasureValid { get; private set; }


    /// <summary>
    /// Gets the actual X position of the top left corner of the control's bounds in screen 
    /// coordinates (computed in <see cref="Arrange(Vector2F, Vector2F)"/>).
    /// </summary>
    /// <value>The actual X position of the top left corner in screen coordinates.</value>
    public float ActualX { get; private set; }


    /// <summary>
    /// Gets the actual Y position of the top left corner of the control's bounds in screen 
    /// coordinates (computed in <see cref="Arrange(Vector2F, Vector2F)"/>).
    /// </summary>
    /// <value>The actual Y position of the top left corner in screen coordinates.</value>
    public float ActualY { get; private set; }


    /// <summary>
    /// Gets the actual width of the control (computed in 
    /// <see cref="Arrange(Vector2F, Vector2F)"/>).
    /// </summary>
    /// <value>The actual width of the control.</value>
    public float ActualWidth { get; private set; }


    /// <summary>
    /// Gets the actual height of the control (computed in 
    /// <see cref="Arrange(Vector2F, Vector2F)"/>).
    /// </summary>
    /// <value>The actual height of the control.</value>
    public float ActualHeight { get; private set; }


    /// <summary>
    /// Gets the actual bounding rectangle of the control (defined by <see cref="ActualX"/>, 
    /// <see cref="ActualY"/>, <see cref="ActualWidth"/> and <see cref="ActualHeight"/>).
    /// </summary>
    /// <value>The actual bounding rectangle of the control in screen coordinates.</value>
    public RectangleF ActualBounds
    {
      get { return new RectangleF(ActualX, ActualY, ActualWidth, ActualHeight); }
    }


    /// <summary>
    /// Gets a value indicating whether the <see cref="Arrange(Vector2F, Vector2F)"/> results are 
    /// up-to-date.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the <see cref="Arrange(Vector2F, Vector2F)"/> results are 
    /// up-to-date; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This flag is set to <see langword="false"/> when <see cref="InvalidateArrange"/> is called.
    /// </remarks>
    public bool IsArrangeValid { get; private set; }


    /// <summary>
    /// Gets or sets the cached renderer data.
    /// </summary>
    /// <value>The cached renderer data.</value>
    /// <remarks>
    /// This property is not used by the <see cref="UIControl"/> itself; it is reserved for use by a 
    /// <see cref="IUIRenderer"/>. The renderer can cache data in this property and re-use it as 
    /// long as <see cref="IsVisualValid"/> is <see langword="true"/>. <see cref="IsVisualValid"/> 
    /// is automatically set after <see cref="Render"/> was executed and reset when 
    /// <see cref="InvalidateVisual"/> is called. 
    /// </remarks>
    [Obsolete("The property 'RendererInfo' has been replaced by 'RenderData'.")]
    public object RendererInfo
    {
      get { return RenderData; }
      set { RenderData = value; }
    }


    /// <summary>
    /// Gets or sets the cached renderer data.
    /// </summary>
    /// <value>The cached renderer data.</value>
    /// <remarks>
    /// This property is not used by the <see cref="UIControl"/> itself; it is reserved for use by a 
    /// <see cref="IUIRenderer"/>. The renderer can cache data in this property and re-use it as 
    /// long as <see cref="IsVisualValid"/> is <see langword="true"/>. <see cref="IsVisualValid"/> 
    /// is automatically set after <see cref="Render"/> was executed and reset when 
    /// <see cref="InvalidateVisual"/> is called. 
    /// </remarks>
    public object RenderData { get; set; }


    /// <summary>
    /// Gets a value indicating whether the cached <see cref="RenderData"/> is up-to-date.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the cached <see cref="RenderData"/> is up-to-date; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This flag is set to <see langword="false"/> when <see cref="InvalidateVisual"/> is called.
    /// </remarks>
    public bool IsVisualValid { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Invalidates the measurement state (layout) for the control.
    /// </summary>
    public void InvalidateMeasure()
    {
      IsArrangeValid = false;
      IsVisualValid = false;

      // During the layout process this flag is not set to avoid recursions.
      if (_isMeasureInProgress || _isArrangeInProgress || !IsMeasureValid)
        return;

      // Invalidate this control and parent.
      IsMeasureValid = false;
      if (VisualParent != null)
        VisualParent.InvalidateMeasure();
    }


    /// <summary>
    /// Invalidates the arrange state (layout) for the control.
    /// </summary>
    public void InvalidateArrange()
    {
      IsVisualValid = false;

      // During the layout process this flag is not set to avoid recursions.
      if (_isArrangeInProgress || !IsArrangeValid)
        return;

      // Invalidate this control and parent.
      IsArrangeValid = false;
      if (VisualParent != null)
        VisualParent.InvalidateArrange();
    }


    /// <summary>
    /// Invalidates the cached <see cref="RenderData"/>.
    /// </summary>
    public void InvalidateVisual()
    {
      if (!IsVisualValid)
        return;

      IsVisualValid = false;
      if (VisualParent != null)
        VisualParent.InvalidateVisual();
    }


    /// <summary>
    /// Ensures that all visual child controls of this element are properly updated for layout.
    /// </summary>
    public void UpdateLayout()
    {
      if (!IsArrangeValid)
      {
        if (VisualParent != null)
        {
          // Go to root.
          VisualParent.UpdateLayout();
        }
        else
        {
          // This is the root. Starts update process.
          Measure(new Vector2F(float.PositiveInfinity));
          Arrange(Vector2F.Zero, new Vector2F(DesiredWidth, DesiredHeight));
        }
      }
    }


    /// <summary>
    /// Updates the <see cref="DesiredWidth"/> and <see cref="DesiredHeight"/> of the control.
    /// </summary>
    /// <param name="availableSize">
    /// The available space that the parent can allocate to this control. The control can ignore
    /// this parameter and request a larger size.
    /// </param>
    public void Measure(Vector2F availableSize)
    {
      // Check removed: Measure can be called manually before control is added to screen for
      // manual layouting.
      //if (!IsLoaded)
      //  throw new InvalidOperationException("Control was not yet added to a UIScreen.");

      // We have to remeasure if InvalidateMeasure() was called or if the parent changes the
      // available size.
      if (IsMeasureValid && availableSize == _lastMeasureConstraintSize)
        return;

      _lastMeasureConstraintSize = availableSize;
      IsArrangeValid = false;
      IsVisualValid = false;

      // Invisible controls are not rendered and do not consume layout space.
      if (!IsVisible)
      {
        DesiredWidth = 0;
        DesiredHeight = 0;
        IsMeasureValid = true;
        return;
      }

      Debug.Assert(!_isMeasureInProgress, "Recursive Measure() call!");
      _isMeasureInProgress = true;

      // Subtract margin before calling OnMeasure().
      Vector4F margin = Margin;
      availableSize.X -= margin.X + margin.Z;
      availableSize.Y -= margin.Y + margin.W;

      // Restrict available size to MaxWidth/MaxHeight.
      float maxWidth = MaxWidth;
      if (availableSize.X > maxWidth)
        availableSize.X = maxWidth;

      float maxHeight = MaxHeight;
      if (availableSize.Y > maxHeight)
        availableSize.Y = maxHeight;

      Vector2F desiredSize = OnMeasure(availableSize);

      // Ensure MinWidth, MinHeight, MaxWidth, and MaxHeight.
      float minWidth = MinWidth;
      if (desiredSize.X < minWidth)
        desiredSize.X = minWidth;
      else if (desiredSize.X > maxWidth)
        desiredSize.X = maxWidth;

      float minHeight = MinHeight;
      if (desiredSize.Y < minHeight)
        desiredSize.Y = minHeight;
      else if (desiredSize.Y > maxHeight)
        desiredSize.Y = maxHeight;

      // Add margin to desired size.
      DesiredWidth = desiredSize.X + margin.X + margin.Z;
      DesiredHeight = desiredSize.Y + margin.Y + margin.W;

      IsMeasureValid = true;
      _isMeasureInProgress = false;
    }


    /// <summary>
    /// Called by <see cref="Measure"/> to compute the control-specific desired size.
    /// </summary>
    /// <param name="availableSize">
    /// The available space that the parent can allocate to this control. The <see cref="Margin"/>
    /// has already been subtracted from this size. The control can ignore this parameter and
    /// request a larger size. This parameter is used by controls that adapt their size to the
    /// available space, like wrap panels.
    /// </param>
    /// <returns>
    /// The desired size (without <see cref="Margin"/>).
    /// </returns>
    /// <remarks>
    /// This method does not change the <see cref="DesiredWidth"/> and <see cref="DesiredHeight"/>
    /// properties, it only returns the desired size as the result of the method. This method must
    /// call <see cref="Measure"/> of the visual children.
    /// </remarks>
    protected virtual Vector2F OnMeasure(Vector2F availableSize)
    {
      // If Width/Height are set, they further restrict the allowed area.
      if (Numeric.IsPositiveFinite(Width) && Width < availableSize.X)
        availableSize.X = Width;
      if (Numeric.IsPositiveFinite(Height) && Height < availableSize.Y)
        availableSize.Y = Height;

      // Measure children.
      foreach (var child in VisualChildren)
        child.Measure(availableSize);

      // The desired size is either Width/Height if they are set, or the max of the child 
      // desired sizes.
      Vector2F desiredSize = Vector2F.Zero;
      if (Numeric.IsPositiveFinite(Width))
      {
        desiredSize.X = Width;
      }
      else
      {
        foreach (var child in VisualChildren)
          desiredSize.X = Math.Max(desiredSize.X, child.DesiredWidth);
      }

      if (Numeric.IsPositiveFinite(Height))
      {
        desiredSize.Y = Height;
      }
      else
      {
        foreach (var child in VisualChildren)
          desiredSize.Y = Math.Max(desiredSize.Y, child.DesiredHeight);
      }

      return desiredSize;
    }


    /// <summary>
    /// Positions child elements and determines a size for a control. 
    /// </summary>
    /// <param name="position">
    /// The actual position of this control as determined by the parent.
    /// </param>
    /// <param name="size">
    /// The actual size of this control as determined by the parent.
    /// </param>
    public void Arrange(Vector2F position, Vector2F size)
    {
      // Check removed: Arrange can be called manually before control is added to screen for 
      // manual layouting.
      //if (!IsLoaded)
      //  throw new InvalidOperationException("Control was not yet added to a UIScreen.");

      if (!IsVisible)
        return;

      if (IsArrangeValid && _lastArrangePosition == position && _lastArrangeSize == size)
        return;

      _lastArrangePosition = position;
      _lastArrangeSize = size;
      IsVisualValid = false;

      Debug.Assert(!_isArrangeInProgress, "Recursive Arrange() call!");
      _isArrangeInProgress = true;

      // Determine actual position.
      Vector4F margin = Margin;
      ActualX = position.X + margin.X;
      ActualY = position.Y + margin.Y;

      // Determine actual width.
      float width = Width;
      bool hasWidth = Numeric.IsPositiveFinite(width);
      ActualWidth = hasWidth ? width : Math.Max(0, size.X - margin.X - margin.Z);

      // Determine actual height.
      float height = Height;
      bool hasHeight = Numeric.IsPositiveFinite(height);
      ActualHeight = hasHeight ? height : Math.Max(0, size.Y - margin.Y - margin.W);

      OnArrange(new Vector2F(ActualX, ActualY), new Vector2F(ActualWidth, ActualHeight));

      IsArrangeValid = true;
      _isArrangeInProgress = false;
    }


    /// <summary>
    /// Called by <see cref="Arrange(Vector2F, Vector2F)"/> to arrange the visual children.
    /// </summary>
    /// <param name="position">The actual position of this control.</param>
    /// <param name="size">The actual size of this control.</param>
    /// <remarks>
    /// When this method is called, <see cref="ActualX"/>, <see cref="ActualY"/>, 
    /// <see cref="ActualWidth"/> and <see cref="ActualHeight"/> are already up-to-date.
    /// </remarks>
    protected virtual void OnArrange(Vector2F position, Vector2F size)
    {
      foreach (var child in VisualChildren)
        Arrange(child, position, size);
    }


    /// <summary>
    /// Arranges the specified control considering the horizontal and vertical alignment of the
    /// given control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="position">The position.</param>
    /// <param name="constraintSize">The constraint size.</param>
    internal static void Arrange(UIControl control, Vector2F position, Vector2F constraintSize)
    {
      if (control == null)
        return;

      Vector2F childPosition = position;
      Vector2F childSize = constraintSize;
      switch (control.HorizontalAlignment)
      {
        case HorizontalAlignment.Left:
          childPosition.X = position.X + control.X;
          childSize.X = control.DesiredWidth;
          break;
        case HorizontalAlignment.Center:
          childPosition.X = position.X + constraintSize.X / 2 - control.DesiredWidth / 2;
          childSize.X = control.DesiredWidth;
          break;
        case HorizontalAlignment.Right:
          childPosition.X = position.X + constraintSize.X - control.DesiredWidth;
          childSize.X = control.DesiredWidth;
          break;
        default: // HorizontalAlignment.Stretch
          break;
      }

      switch (control.VerticalAlignment)
      {
        case VerticalAlignment.Top:
          childPosition.Y = position.Y + control.Y;
          childSize.Y = control.DesiredHeight;
          break;
        case VerticalAlignment.Center:
          childPosition.Y = position.Y + constraintSize.Y / 2 - control.DesiredHeight / 2;
          childSize.Y = control.DesiredHeight;
          break;
        case VerticalAlignment.Bottom:
          childPosition.Y = position.Y + constraintSize.Y - control.DesiredHeight;
          childSize.Y = control.DesiredHeight;
          break;
        default: // VerticalAlignment.Stretch
          break;
      }

      control.Arrange(childPosition, childSize);
    }
    #endregion
  }
}
