// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

/* ----- TickCountClock has been removed, because its resolution is too low.

using System;

namespace DigitalRune.Game.Timing
{
  /// <summary>
  /// Measures the real time (wall clock time) by polling the computer's system 
  /// timer (<see cref="System.Environment.TickCount"/>).
  /// </summary>
  /// <remarks>
  /// The resolution of this clock is equivalent to the resolution of 
  /// <see cref="System.Environment.TickCount"/>.
  /// </remarks>
  public class TickCountClock : GameClock
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
 
    private int _lastTickCount;  // The tick count of the last update.
    private int _stopTickCount;  // The tick count when the clock is suspended.
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override void Start()
    {
      if (!IsRunning)
      {
        int ticksLostToSuspension = Environment.TickCount - _stopTickCount;
        _lastTickCount += ticksLostToSuspension;
        base.Start();
      }
    }


    /// <inheritdoc/>
    public override void Stop()
    {
      if (IsRunning)
      {
        _stopTickCount = Environment.TickCount;
        base.Stop();
      }
    }


    /// <inheritdoc/>
    public override void Update()
    {
#if DEBUG
      if (!IsRunning)
        throw new InvalidOperationException("TickCountClock.Update() called, but clock is suspended.");
#endif

      if (IsRunning)
      {
        int tickCount = Environment.TickCount;
        
        // Compute deltaTime
        if (tickCount >= _lastTickCount)
        {
          var milliseconds = tickCount - _lastTickCount;
          DeltaTime = new TimeSpan(0, 0, 0, 0, milliseconds);
        }
        else
        {
          // Overflow detected.
          var milliseconds = int.MaxValue - _lastTickCount + tickCount - int.MinValue;
          DeltaTime = new TimeSpan(0, 0, 0, 0, milliseconds);
        }

        // Update time.
        Time += DeltaTime;
        _lastTickCount = tickCount;

        base.Update();
      }
    }
    #endregion
  }
}

*/
