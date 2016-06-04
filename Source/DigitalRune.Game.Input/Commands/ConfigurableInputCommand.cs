// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics;


namespace DigitalRune.Game.Input
{
  /// <summary>
  /// Represents an input command that supports a flexible input mapping.
  /// <i>(Experimental: This class is experimental and subject to change.)</i>
  /// </summary>
  /// <remarks>
  /// This command can be triggered with keys and buttons defined in the 
  /// <see cref="PrimaryMapping"/> and <see cref="SecondaryMapping"/>. 
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public class ConfigurableInputCommand : IInputCommand
  {
    // Notes:
    // - We could turn digital input (buttons and keys) into a continuous input: When the
    //   user presses a button, the value does not immediately jump to 1. Instead, the user
    //   has to press the button for a longer time until 1 is reached. - But this is 
    //   application dependent and is also relevant for analog input (axes). 
    //   --> Do not handle this case here.

    // TODO:
    // - Integrate handling of IsHandled flags.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public IInputService InputService { get; set; }


    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// It is not allowed to change the name of an input command while the command is added to an 
    /// input service.
    /// </exception>
    public string Name
    {
      get { return _name; }
      set
      {
        if (_name != value)
        {
          if (InputService != null)
            throw new InvalidOperationException("It is not allowed to change the name of an input command while the command is added to an input service.");

          _name = value;
        }
      }
    }
    private string _name = "Unnamed";


    /// <inheritdoc/>
    public float Value { get; private set; }


    /// <summary>
    /// Gets or sets the description of this command.
    /// </summary>
    /// <value>
    /// The description of this command. The default value is <see langword="null"/>.
    /// </value>
    public string Description { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="ConfigurableInputCommand"/> is 
    /// enabled. If the command is not enabled, the <see cref="Value"/> is always 0.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if enabled; otherwise, <see langword="false"/>.
    /// The default is <see langword="true"/>.
    /// </value>
    public bool Enabled { get; set; }


    /// <summary>
    /// Gets or sets the primary input mapping that triggers the command.
    /// </summary>
    /// <value>The primary input mapping. The default is <see langword="null"/>.</value>
    public InputMapping PrimaryMapping { get; set; }


    /// <summary>
    /// Gets or sets the secondary input mapping that triggers the command.
    /// This mapping is not evaluated if the <see cref="PrimaryMapping"/> has influenced
    /// the <see cref="Value"/>.
    /// </summary>
    /// <value>The secondary mapping. The default is <see langword="null"/>.</value>
    public InputMapping SecondaryMapping { get; set; }


    /// <summary>
    /// Gets or sets the scale that is applied to the <see cref="Value"/>.
    /// </summary>
    /// <value>
    /// The scale that is applied to the <see cref="Value"/>. The default value is 1.
    /// </value>
    public float Scale { get; set; }


    /// <summary>
    /// Gets or sets the sensitivity that is used for analog input.
    /// </summary>
    /// <value>
    /// The sensitivity for analog input in the range ]0, ∞]. The default value is 1. 
    /// </value>
    /// <remarks>
    /// <para>
    /// A <see cref="Sensitivity"/> of 1 creates a linear response curve (default). A value of 1/2
    /// creates a quadratic response curve. A value of 1/3 creates a cubic response curve. Etc.
    /// </para>
    /// <para>
    /// The <see cref="Sensitivity"/> does not change the minimal and maximal values of the
    /// <see cref="Value"/>. But they change how the <see cref="Value"/> changes in response to
    /// analog input. If a thumb stick is pressed half way and the <see cref="Sensitivity"/> is 1,
    /// the output <see cref="Value"/> is 0.5. If the <see cref="Sensitivity"/> is less than 1, then
    /// the <see cref="Value"/> would be less than 0.5f. If the <see cref="Sensitivity"/> is greater
    /// than 1, then the <see cref="Value"/> would be greater than 0.5f. In other words: If the 
    /// <see cref="Sensitivity"/> is high, then a small thumb stick movement has a larger reaction.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Sensitivity is negative or equal to 0.
    /// </exception>
    public float Sensitivity
    {
      get { return _sensitivity; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "Sensitivity must be greater than 0.");

        _sensitivity = value;
      }
    }
    private float _sensitivity;


    /// <summary>
    /// Gets or sets the index of the logical player.
    /// </summary>
    /// <value>
    /// The index of the logical player. The default value is 
    /// <see cref="Input.LogicalPlayerIndex.One"/>. This value must not be set to 
    /// <see cref="Input.LogicalPlayerIndex.Any"/>.
    /// </value>
    /// <exception cref="ArgumentException">
    /// Property is set to <see cref="Input.LogicalPlayerIndex.Any"/>.
    /// </exception>
    public LogicalPlayerIndex LogicalPlayerIndex
    {
      get { return _logicalPlayerIndex; }
      set
      {
        if (value == LogicalPlayerIndex.Any)
          throw new ArgumentException("LogicalPlayerIndex.Any is not allowed.");

        _logicalPlayerIndex = value;
      }
    }
    private LogicalPlayerIndex _logicalPlayerIndex = LogicalPlayerIndex.One;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurableInputCommand"/> class.
    /// </summary>
    public ConfigurableInputCommand()
    {
      Sensitivity = 1;
      Enabled = true;
      Scale = 1;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurableInputCommand"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    public ConfigurableInputCommand(string name)
    {
      _name = name;
      Sensitivity = 1;
      Enabled = true;
      Scale = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public void Update(TimeSpan deltaTime)
    {
      Value = 0;

      if (!Enabled || InputService == null)
        return;

      HandleMapping(PrimaryMapping);

      if (Value == 0)
        HandleMapping(SecondaryMapping);

      Value *= Scale;
    }


    private void HandleMapping(InputMapping mapping)
    {
      if (mapping == null)
        return;

      if (mapping.ModifierKeys == ModifierKeys.None 
          || (InputService.ModifierKeys & mapping.ModifierKeys) == mapping.ModifierKeys)
      {
        // No modifiers necessary, or all modifiers are down.
        HandleKeys(mapping);
        HandleMouseButtons(mapping);
      }

#if !SILVERLIGHT
      if (mapping.ModifierButtons == null
          || InputService.IsDown(mapping.ModifierButtons.Value, LogicalPlayerIndex))
      {
        // No modifiers necessary, or all modifiers are down.
        HandleButtons(mapping);
      }
#endif

      Value = MathHelper.Clamp(Value, -1, 1);
      
      float axisValue = HandleAxis(mapping);

      // Combine Value and axisValue.
      // The normal value limit is 1, but mouse axis can give a value beyond 1. We take the
      // max of these values as the limit. The Value must not increase above the limit when
      // the user uses axis + keys/buttons. (For instance, concurrent presses should not 
      // make a player faster.)
      float limit = Math.Max(1, Math.Abs(axisValue));
      Value = MathHelper.Clamp(Value + axisValue, -limit, limit);
      
      if (mapping.Invert)
        Value = -Value;
    }


    private void HandleKeys(InputMapping mapping)
    {
      if (mapping.PositiveKey == null && mapping.NegativeKey == null)
        return;

      if (mapping.PressType == PressType.Down)
      {
        if (mapping.PositiveKey != null && InputService.IsDown(mapping.PositiveKey.Value))
          Value++;
        if (mapping.NegativeKey != null && InputService.IsDown(mapping.NegativeKey.Value))
          Value--;
      }
      else if (mapping.PressType == PressType.Press)
      {
        if (mapping.PositiveKey != null && InputService.IsPressed(mapping.PositiveKey.Value, false))
          Value++;
        if (mapping.NegativeKey != null && InputService.IsPressed(mapping.NegativeKey.Value, false))
          Value--;
      }
      else 
      {
        Debug.Assert(mapping.PressType == PressType.DoubleClick, "Unhandled press type.");

        if (mapping.PositiveKey != null && InputService.IsDoubleClick(mapping.PositiveKey.Value))
          Value++;
        if (mapping.NegativeKey != null && InputService.IsDoubleClick(mapping.NegativeKey.Value))
          Value--;
      }
    }

#if !SILVERLIGHT
    private void HandleButtons(InputMapping mapping)
    {
      if (mapping.PositiveButton == null && mapping.NegativeButton == null)
        return;

      if (mapping.PressType == PressType.Down)
      {
        if (mapping.PositiveButton != null && InputService.IsDown(mapping.PositiveButton.Value, LogicalPlayerIndex))
          Value++;
        if (mapping.NegativeButton != null && InputService.IsDown(mapping.NegativeButton.Value, LogicalPlayerIndex))
          Value--;
      }
      else if (mapping.PressType == PressType.Press)
      {
        if (mapping.PositiveButton != null && InputService.IsPressed(mapping.PositiveButton.Value, false, LogicalPlayerIndex))
          Value++;
        if (mapping.NegativeButton != null && InputService.IsPressed(mapping.NegativeButton.Value, false, LogicalPlayerIndex))
          Value--;
      }
      else
      {
        Debug.Assert(mapping.PressType == PressType.DoubleClick, "Unhandled press type.");

        if (mapping.PositiveButton != null && InputService.IsDoubleClick(mapping.PositiveButton.Value, LogicalPlayerIndex))
          Value++;
        if (mapping.NegativeButton != null && InputService.IsDoubleClick(mapping.NegativeButton.Value, LogicalPlayerIndex))
          Value--;
      }
    }
#endif


    private void HandleMouseButtons(InputMapping mapping)
    {
      if (mapping.PositiveMouseButton == null && mapping.NegativeMouseButton == null)
        return;

      if (mapping.PressType == PressType.Down)
      {
        if (mapping.PositiveMouseButton != null && InputService.IsDown(mapping.PositiveMouseButton.Value))
          Value++;
        if (mapping.NegativeMouseButton != null && InputService.IsDown(mapping.NegativeMouseButton.Value))
          Value--;
      }
      else if (mapping.PressType == PressType.Press)
      {
        if (mapping.PositiveMouseButton != null && InputService.IsPressed(mapping.PositiveMouseButton.Value, false))
          Value++;
        if (mapping.NegativeMouseButton != null && InputService.IsPressed(mapping.NegativeMouseButton.Value, false))
          Value--;
      }
      else
      {
        Debug.Assert(mapping.PressType == PressType.DoubleClick, "Unhandled press type.");

        if (mapping.PositiveMouseButton != null && InputService.IsDoubleClick(mapping.PositiveMouseButton.Value))
          Value++;
        if (mapping.NegativeMouseButton != null && InputService.IsDoubleClick(mapping.NegativeMouseButton.Value))
          Value--;
      }
    }


    private float HandleAxis(InputMapping mapping)
    {
      if (mapping.Axis == null)
        return 0;

      float axisValue = 0;

      switch (mapping.Axis.Value)
      {
        case DeviceAxis.MouseXAbsolute:
          axisValue = InputService.MousePosition.X;
          break;
        case DeviceAxis.MouseYAbsolute:
          axisValue = InputService.MousePosition.Y;
          break;
        case DeviceAxis.MouseXRelative:
          axisValue = InputService.MousePositionDelta.X;
          break;
        case DeviceAxis.MouseYRelative:
          axisValue = InputService.MousePositionDelta.Y;
          break;
        case DeviceAxis.MouseWheel:
          axisValue = InputService.MouseWheelDelta;
          break;
#if !SILVERLIGHT
        case DeviceAxis.GamePadStickLeftX:
          axisValue = InputService.GetGamePadState(LogicalPlayerIndex).ThumbSticks.Left.X;
          break;
        case DeviceAxis.GamePadStickLeftY:
          axisValue = InputService.GetGamePadState(LogicalPlayerIndex).ThumbSticks.Left.Y;
          break;
        case DeviceAxis.GamePadStickRightX:
          axisValue = InputService.GetGamePadState(LogicalPlayerIndex).ThumbSticks.Right.X;
          break;
        case DeviceAxis.GamePadStickRightY:
          axisValue = InputService.GetGamePadState(LogicalPlayerIndex).ThumbSticks.Right.Y;
          break;
        case DeviceAxis.GamePadTriggerLeft:
          axisValue = InputService.GetGamePadState(LogicalPlayerIndex).Triggers.Left;
          break;
        case DeviceAxis.GamePadTriggerRight:
          axisValue = InputService.GetGamePadState(LogicalPlayerIndex).Triggers.Right;
          break;
#endif
        default:
          Debug.Assert(false, "Unhandled device axis.");
          break;
      }

      if (Sensitivity != 1.0f)
        axisValue = Math.Sign(axisValue) * (float)Math.Pow(Math.Abs(axisValue), 1.0f / Sensitivity);

      return axisValue;
    }
    #endregion
  }
}
