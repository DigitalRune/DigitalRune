// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
#if WP7 || XBOX
using System.Diagnostics;
#else
using DigitalRune.Diagnostics;
#endif


namespace DigitalRune.Game.Timing
{
  /// <summary>
  /// Accurately measures the time by using the system's performance counter.
  /// (Not available in Silverlight.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// The method <see cref="Update"/> has to be called regularly to trigger <see cref="TimeChanged"/>
  /// events.
  /// </para>
  /// <para>
  /// The <see cref="HighPrecisionClock"/> by default uses a performance counter to accurately 
  /// measure time. If no performance counter is available, the system timer is used to measures 
  /// elapsed time. 
  /// </para>
  /// <para>
  /// See <see cref="IGameClock"/> for more information.
  /// </para>
  /// </remarks>
  public class HighPrecisionClock : IGameClock
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // TimeEventArgs object is reused to avoid unnecessary memory allocations.
    private readonly GameClockEventArgs _eventArgs = new GameClockEventArgs(); 

#if WP7 || XBOX
    private readonly System.Diagnostics.Stopwatch _totalTimeWatch = new System.Diagnostics.Stopwatch();
#else
    private readonly DigitalRune.Diagnostics.Stopwatch _totalTimeWatch = new DigitalRune.Diagnostics.Stopwatch();
#endif

    private TimeSpan _lastTime;   // time of last TimeChangedEvent.
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public bool IsRunning
    {
      get { return _totalTimeWatch.IsRunning; }
    }


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
    /// Initializes a new instance of the <see cref="HighPrecisionClock"/> class.
    /// </summary>
    public HighPrecisionClock()
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
      _totalTimeWatch.Start();
    }


    /// <inheritdoc/>
    public void Stop()
    {
      _totalTimeWatch.Stop();
    }


    /// <inheritdoc/>
    public void Reset()
    {
      _totalTimeWatch.Reset();
      
      DeltaTime = TimeSpan.Zero;
      GameTime = TimeSpan.Zero;
      TotalTime = TimeSpan.Zero;
    }


    /// <inheritdoc/>
    public void ResetDeltaTime()
    {
      _lastTime = _totalTimeWatch.Elapsed;
    }


    /// <summary>
    /// Updates the time (needs to be called regularly).
    /// </summary>
    /// <remarks>
    /// This method advances the <see cref="GameTime"/>, calculates the <see cref="DeltaTime"/> 
    /// (time that has elapsed since the last call of <see cref="Update"/>) and raises the 
    /// <see cref="TimeChanged"/> event.
    /// </remarks>
    public void Update()
    {
      if (_totalTimeWatch.IsRunning)
      {
        TotalTime = _totalTimeWatch.Elapsed;
        DeltaTime = TotalTime - _lastTime;

        // Limit excessively large time change (e.g. after paused in the debugger).
        if (DeltaTime > MaxDeltaTime)
          DeltaTime = MaxDeltaTime;

        GameTime += DeltaTime;
        
        _lastTime = TotalTime;

        // Raise TimeChanged event.
        _eventArgs.DeltaTime = DeltaTime;
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
