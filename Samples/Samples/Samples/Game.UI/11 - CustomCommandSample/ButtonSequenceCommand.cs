using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Game.Input;
using Microsoft.Xna.Framework.Input;


namespace Samples.Game.UI
{
  /// <summary>
  /// An input command that detects whether a sequence of buttons is pressed.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The following buttons can be part of the sequence: <see cref="Buttons.A"/>,
  /// <see cref="Buttons.B"/>, <see cref="Buttons.X"/>, and <see cref="Buttons.Y"/>. Pressing these
  /// buttons in the wrong order resets the sequence. Any other button presses are ignored. This
  /// means, for example, that is allowed to hold a trigger button during the sequence.
  /// </para>
  /// <para>
  /// The property <see cref="Value"/> tracks the progress of the button sequence.
  /// </para>
  /// </remarks>
  public class ButtonSequenceCommand : IInputCommand
  {
    private int _index;   // The index of the next button in the sequence.
    private float _time;  // The time since the start of the sequence.


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
    /// Gets or sets the button sequence.
    /// </summary>
    /// <value>The button sequence.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The sequence of buttons is empty or contains an invalid button.
    /// </exception>
    public IList<Buttons> ButtonSequence
    {
      get { return _buttonSequence; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (value.Count == 0)
          throw new ArgumentException("The sequence of buttons must not be empty.", "value");

        foreach (var button in value)
        {
          switch (button)
          {
            // The following buttons are allowed in sequence.
            case Buttons.A: 
            case Buttons.B:
            case Buttons.X:
            case Buttons.Y:
              break;

            // Other buttons are not allowed.
            default:
              throw new ArgumentException("Only the buttons A, B, X, Y are allowed in the sequence.", "value");
          }
        }

        _buttonSequence = value;
        _index = 0;
      }
    }
    private IList<Buttons> _buttonSequence;


    /// <summary>
    /// Gets or sets the max time for the entire sequence in seconds.
    /// </summary>
    /// <value>The max time for the entire sequence in seconds.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public float MaxTime
    {
      get { return _maxTime; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "The max time must be greater than 0.");

        _maxTime = value;
      }
    }
    private float _maxTime;


    /// <summary>
    /// Gets or sets the index of the logical player.
    /// </summary>
    /// <value>
    /// The index of the logical player. The default value is 
    /// <see cref="DigitalRune.Game.Input.LogicalPlayerIndex.One"/>.
    /// </value>
    public LogicalPlayerIndex LogicalPlayerIndex { get; set; }


    /// <summary>
    /// Gets the value indicating the progress of button sequence.
    /// </summary>
    /// <value>The progress of the button sequence.</value>
    /// <remarks>
    /// A value of 0 means that the button sequence has not been started. A value of 1 means that
    /// the buttons sequence is complete.
    /// </remarks>
    public float Value { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonSequenceCommand"/> class.
    /// </summary>
    /// <param name="buttonSequence">The sequence of buttons that needs to be pressed.</param>
    /// <param name="maxTime">The max time to enter the entire sequence in seconds.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buttonSequence"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The sequence of buttons is empty or contains an invalid button.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxTime"/> is negative or 0.
    /// </exception>
    public ButtonSequenceCommand(IList<Buttons> buttonSequence, float maxTime)
    {
      ButtonSequence = buttonSequence;
      MaxTime = maxTime;
    }


    /// <summary>
    /// Updates internal values of this command. This method is called automatically in each frame 
    /// by the input service.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    public void Update(TimeSpan deltaTime)
    {
      Debug.Assert(ButtonSequence != null);
      Debug.Assert(ButtonSequence.Count > 0);
      Debug.Assert(0 <= _index && _index < ButtonSequence.Count);

      // (Note: Unfortunately XNA does not a expose a bit field with all buttons that are pressed,
      // therefore we need to check buttons individually. Working with bit fields would be a lot
      // easier.)

      _time += (float)deltaTime.TotalSeconds;

      var expectedButton = ButtonSequence[_index];
      if (_time > MaxTime
          || InputService == null
          || expectedButton != Buttons.A && InputService.IsPressed(Buttons.A, false, LogicalPlayerIndex)
          || expectedButton != Buttons.B && InputService.IsPressed(Buttons.B, false, LogicalPlayerIndex)
          || expectedButton != Buttons.X && InputService.IsPressed(Buttons.X, false, LogicalPlayerIndex)
          || expectedButton != Buttons.Y && InputService.IsPressed(Buttons.Y, false, LogicalPlayerIndex))
      {
        // Reset sequence.
        _time = 0;
        _index = 0;
        Value = 0;
      }

      expectedButton = ButtonSequence[_index];
      if (InputService != null && InputService.IsPressed(expectedButton, false, LogicalPlayerIndex))
      {
        // Move to next button in sequence.
        _index++;
        Value = (float)_index / ButtonSequence.Count;

        if (_index == ButtonSequence.Count)
        {
          _time = 0;
          _index = 0;
        }
      }
    }
  }
}
