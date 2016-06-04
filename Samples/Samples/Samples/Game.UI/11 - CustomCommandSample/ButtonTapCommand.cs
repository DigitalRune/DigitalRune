using System;
using DigitalRune.Game.Input;
using Microsoft.Xna.Framework.Input;


namespace Samples.Game.UI
{
  /// <summary>
  /// An input command that requires the user to rapidly tap a button for a certain amount of time.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This input command tracks buttons taps. The property <see cref="Value"/> tracks the progress
  /// of the command. The property stores a value from 0 to 1. The value decreases over time. Tapping the specified button
  /// increases the value by a fixed amount. The user has to tap the button fast enough to reach a
  /// value of 1.
  /// </para>
  /// <para>
  /// To reset the progress, you need to set <see cref="Value"/> to 0.
  /// </para>
  /// </remarks>
  public class ButtonTapCommand : IInputCommand
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
    /// Gets or sets the button that need to be pressed.
    /// </summary>
    /// <value>The button that need to be pressed.</value>
    public Buttons Button { get; set; }


    /// <summary>
    /// Gets or sets the increment by which the value increases when the button is tapped.
    /// </summary>
    /// <value>The tap increment.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public float TapIncrement
    {
      get { return _tapIncrement; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "The tap increment must be greater than 0.");

        _tapIncrement = value;
      }
    }
    private float _tapIncrement;


    /// <summary>
    /// Gets or sets the rate by which the value decreases.
    /// </summary>
    /// <value>
    /// The rate by which the value decreases. (The value is multiplied by the elapsed time and
    /// subtracted from <see cref="Value"/>.)
    /// </value>
    public float Decrease
    {
      get { return _decrease; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "The decrease must be greater than 0.");

        _decrease = value;
      }
    }
    private float _decrease;


    /// <summary>
    /// Gets or sets the index of the logical player.
    /// </summary>
    /// <value>
    /// The index of the logical player. The default value is 
    /// <see cref="DigitalRune.Game.Input.LogicalPlayerIndex.One"/>.
    /// </value>
    public LogicalPlayerIndex LogicalPlayerIndex { get; set; }


    /// <summary>
    /// Gets the value indicating the progress.
    /// </summary>
    /// <value>The progress of the button tap command.</value>
    /// <remarks>
    /// This property stores a value from 0 to 1. Rapidly tapping the specified button increases the
    /// value. If the buttons is not pressed fast enough the value decreases over time. A value of 1
    /// means that command is complete.
    /// </remarks>
    public float Value { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonTapCommand"/> class.
    /// </summary>
    /// <param name="button">The button that needs to be tapped.</param>
    /// <param name="tapIncrement">
    /// The increment by which the value increases when the button is tapped.
    /// </param>
    /// <param name="decrease">The rate by which the value decreases.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="tapIncrement"/> or <see cref="decrease"/> is negative or 0.
    /// </exception>
    public ButtonTapCommand(Buttons button, float tapIncrement, float decrease)
    {
      Button = button;
      TapIncrement = tapIncrement;
      Decrease = decrease;
    }


    /// <summary>
    /// Updates internal values of this command. This method is called automatically in each frame 
    /// by the input service.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    public void Update(TimeSpan deltaTime)
    {
      if (InputService != null 
          && InputService.IsPressed(Button, false, LogicalPlayerIndex)
          && Value < 1)
      {
        // Increase value.
        Value += TapIncrement;
        if (Value > 1)
          Value = 1;
      }
      else if (Value > 0)
      {
        // Decrease value.
        Value -= Decrease * (float)deltaTime.TotalSeconds;
        if (Value < 0)
          Value = 0;
      }
    }
  }
}
