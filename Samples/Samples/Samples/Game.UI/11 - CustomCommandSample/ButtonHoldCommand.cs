using System;
using DigitalRune.Game.Input;
using Microsoft.Xna.Framework.Input;


namespace Samples.Game.UI
{
  /// <summary>
  /// An input command that triggers when one or more buttons are pressed for a certain amount of
  /// time.
  /// </summary>
  /// <remarks>
  /// The property <see cref="Value"/> is the relative button hold time. It indicates whether the
  /// required buttons are pressed and the time for which the buttons have been pressed.
  /// </remarks>
  class ButtonHoldCommand : IInputCommand
  {
    /// <summary>
    /// Gets or sets the input service.
    /// </summary>
    /// <value>The input service.</value>
    public IInputService InputService { get; set; }


    /// <summary>
    /// Gets or sets the name of the input command.
    /// </summary>
    /// <value>The name of the input command.</value>
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


    /// <summary>
    /// Gets or sets the buttons that need to be pressed.
    /// </summary>
    /// <value>The buttons that need to be pressed.</value>
    public Buttons Buttons { get; set; }


    /// <summary>
    /// Gets or sets the time required to hold the buttons in seconds.
    /// </summary>
    /// <value>The time required to hold the buttons in seconds.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public float HoldTime
    {
      get { return _holdTime; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "The hold time must be greater than 0.");

        _holdTime = value;
      }
    }
    private float _holdTime;


    /// <summary>
    /// Gets or sets the index of the logical player.
    /// </summary>
    /// <value>
    /// The index of the logical player. The default value is 
    /// <see cref="DigitalRune.Game.Input.LogicalPlayerIndex.One"/>.
    /// </value>
    public LogicalPlayerIndex LogicalPlayerIndex { get; set; }


    /// <summary>
    /// Gets a value indicating whether the buttons are pressed and for how long the buttons have
    /// been pressed.
    /// </summary>
    /// <value>The relative button hold time.</value>
    /// <remarks>
    /// A value greater than 0 means that the buttons are pressed. A value equal to or greater than
    /// 1 means that the buttons were held for <see cref="HoldTime"/> seconds or longer.
    /// </remarks>
    public float Value { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonHoldCommand" /> class.
    /// </summary>
    /// <param name="buttons">The buttons that need to be pressed.</param>
    /// <param name="holdTime">The time required to hold the buttons in seconds.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="holdTime"/> is negative or 0.
    /// </exception>
    public ButtonHoldCommand(Buttons buttons, float holdTime)
    {
      Buttons = buttons;
      HoldTime = holdTime;
    }


    /// <summary>
    /// Updates internal values of this command. This method is called automatically in each frame 
    /// by the input service.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    public void Update(TimeSpan deltaTime)
    {
      if (InputService != null && InputService.IsDown(Buttons, LogicalPlayerIndex))
        Value += (float)deltaTime.TotalSeconds / HoldTime;
      else
        Value = 0;
    }
  }
}
