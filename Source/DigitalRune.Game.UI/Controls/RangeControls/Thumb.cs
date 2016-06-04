// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using DigitalRune.Game.Input;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Represents a control that can be dragged by the user. 
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Visual States:</strong> The <see cref="VisualState"/>s of this control are:
  /// "Disabled", "Default", "MouseOver", "Focused", "Dragging"
  /// </para>
  /// </remarks>
  public class Thumb : UIControl
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Vector2F _offset;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override string VisualState
    {
      get
      {
        if (!ActualIsEnabled)
          return "Disabled";

        if (IsDragging)
          return "Dragging";

        if (IsMouseOver)
          return "MouseOver";

        if (IsFocused || (VisualParent != null && VisualParent.IsFocused))
          return "Focused";

        return "Default";
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="IsDragging"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int IsDraggingPropertyId = CreateProperty(
      typeof(Thumb), "IsDragging", GamePropertyCategories.Default, null, false,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets a value indicating whether this thumb is currently dragged by the user. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this thumb is currently dragged by the user; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsDragging
    {
      get { return GetValue<bool>(IsDraggingPropertyId); }
      private set { SetValue(IsDraggingPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="DragDelta"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int DragDeltaPropertyId = CreateProperty(
      typeof(Thumb), "DragDelta", GamePropertyCategories.Default, null, Vector2F.Zero,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets the dragging distance relative to the start position of the drag operation. 
    /// This is a game object property.
    /// </summary>
    /// <value>The dragging distance relative to the start position of the drag operation.</value>
    public Vector2F DragDelta
    {
      get { return GetValue<Vector2F>(DragDeltaPropertyId); }
      private set { SetValue(DragDeltaPropertyId, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Thumb"/> class.
    /// </summary>
    public Thumb()
    {
      Style = "Thumb";
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
    protected override void OnHandleInput(InputContext context)
    {
      base.OnHandleInput(context);

      if (!IsLoaded)
        return;

      var inputService = InputService;

      if (IsDragging)
      {
        if (inputService.IsMouseOrTouchHandled || inputService.IsUp(MouseButtons.Left))
        {
          // Dragging ends.
          DragDelta = Vector2F.Zero;
          IsDragging = false;
        }
        else
        {
          // Dragging continues.
          Vector2F newOffset = context.MousePosition - new Vector2F(ActualX + ActualWidth / 2, ActualY + ActualHeight / 2);
          DragDelta = newOffset - _offset;
        }

        // Mouse or touch input is "captured" while dragging.
        inputService.IsMouseOrTouchHandled = true;
      }
      else
      {
        if (IsMouseOver && inputService.IsPressed(MouseButtons.Left, false))
        {
          inputService.IsMouseOrTouchHandled = true;

          // Dragging starts.
          IsDragging = true;
          DragDelta = Vector2F.Zero;

          // Store the mouse position offset relative to the control center.
          _offset = context.MousePosition - new Vector2F(ActualX + ActualWidth / 2, ActualY + ActualHeight / 2);
        }
      }
    }
    #endregion
  }
}
