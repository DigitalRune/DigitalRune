// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game.Timing
{
  /// <summary>
  /// A simple game clock that needs to be updated manually.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This <see cref="IGameClock"/> does not measure time by itself. <see cref="Update"/> must be 
  /// called regularly to advance the time.
  /// </para>
  /// <para>
  /// See <see cref="IGameClock"/> for more information.
  /// </para>
  /// </remarks>
  public class ManualClock : IGameClock
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // TimeEventArgs object is reused to avoid unnecessary memory allocations.
    private readonly GameClockEventArgs _eventArgs = new GameClockEventArgs(); 
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public bool IsRunning { get; private set; }


    /// <inheritdoc/>
    public TimeSpan DeltaTime { get; private set; }


    /// <value>
    /// The max limit for <see cref="DeltaTime" />. The default value is 100 ms.
    /// </value>
    /// <inheritdoc />
    public TimeSpan MaxDeltaTime { get; set; }


    /// <inheritdoc/>
    public TimeSpan GameTime { get; private set; }


    /// <inheritdoc/>
    public TimeSpan TotalTime { get; private set; }


    /// <inheritdoc/>
    public event EventHandler<GameClockEventArgs> TimeChanged;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualClock"/> class.
    /// </summary>
    public ManualClock()
    {
      MaxDeltaTime = TimeSpan.FromMilliseconds(100);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public void Start()
    {
      IsRunning = true;
    }


    /// <inheritdoc/>
    public void Stop()
    {
      IsRunning = false;
    }


    /// <inheritdoc/>
    public void Reset()
    {
      IsRunning = false;
      DeltaTime = TimeSpan.Zero;
      GameTime = TimeSpan.Zero;
      TotalTime = TimeSpan.Zero;
    }
    

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// This method is called.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    void IGameClock.ResetDeltaTime()
    {
      throw new NotSupportedException();
    }



    /// <summary>
    /// Increases the time by the specified time span if the clock is running. (This method needs to
    /// be called regularly.)
    /// </summary>
    /// <param name="deltaTime">
    /// The elapsed time since the last <see cref="Update"/> call.
    /// </param>
    public void Update(TimeSpan deltaTime)
    {
      if (IsRunning)
      {
        // Limit excessively large time change (e.g. after paused in the debugger).
        if (deltaTime > MaxDeltaTime)
          deltaTime = MaxDeltaTime;

        GameTime += deltaTime;
        TotalTime += deltaTime;
        DeltaTime = deltaTime;

        // Raise the TimeChanged event.
        _eventArgs.DeltaTime = deltaTime;
        _eventArgs.GameTime = GameTime;
        _eventArgs.TotalTime = GameTime;
        OnTimeChanged(_eventArgs);
      }
    }



    /// <summary>
    /// Raises the <see cref="TimeChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// The <see cref="GameClockEventArgs"/> instance containing the event data.
    /// </param>
    protected virtual void OnTimeChanged(GameClockEventArgs eventArgs)
    {
      var handler = TimeChanged;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
