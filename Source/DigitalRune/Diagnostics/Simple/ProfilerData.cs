// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Diagnostics
{
  /// <summary>
  /// Stores profiler data.
  /// </summary>
  /// <remarks>
  /// Profiler data is automatically created when the profiler methods <see cref="Profiler.Start"/> 
  /// or <see cref="Profiler.AddValue"/> are used.
  /// </remarks>
  public class ProfilerData : INamedObject
  {
    // Methods are all internal because the user should only use the thread-safe Profiler methods.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The total elapsed seconds when Start() was called. -1 means that Start() was not called.
    private double _startTime = -1;
    #endregion
      
      
    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    
    /// <summary>
    /// Gets the name of the profiler data.
    /// </summary>
    /// <value>The name of the profiler data.</value>
    public string Name { get; private set; }


    /// <summary>
    /// Gets the number of measured values.
    /// </summary>
    /// <value>The number of measured values.</value>
    /// <remarks>
    /// The <see cref="Count"/> is incremented when <see cref="Profiler.Stop"/> or 
    /// <see cref="Profiler.AddValue"/> are called.
    /// </remarks>
    public int Count { get; private set; }


    /// <summary>
    /// Gets the sum of all measured values.
    /// </summary>
    /// <value>The sum.</value>
    public double Sum { get; private set; }


    /// <summary>
    /// Gets the minimum of all measured values.
    /// </summary>
    /// <value>The minimum.</value>
    public double Minimum { get; private set; }


    /// <summary>
    /// Gets the maximum of all measured values.
    /// </summary>
    /// <value>The maximum.</value>
    public double Maximum { get; private set; }


    /// <summary>
    /// Gets the average (arithmetic mean) of all measured values.
    /// </summary>
    /// <value>The average (arithmetic mean).</value>
    public double Average { get { return Sum / Count; } }


    /// <summary>
    /// Gets the last value that was measured.
    /// </summary>
    /// <value>The last value that was measured.</value>
    public double Last { get; private set; }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Profiler"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    internal ProfilerData(string name)
    {
      Name = name;

      Reset();
    }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    internal void Reset()
    {
      Count = 0;
      _startTime = -1;
      Sum = double.NaN;
      Minimum = double.NaN;
      Maximum = double.NaN;
      Last = double.NaN;
    }

    
    internal void Start()
    {
#if PORTABLE
      throw Portable.NotImplementedException;
#elif SILVERLIGHT
      throw new NotSupportedException();
#else
      _startTime = Profiler.Stopwatch.Elapsed.TotalSeconds;
#endif
    }


    internal void Stop()
    {
      // Abort if the Start() was not called. - This can also happen if 
      // Profiler.ClearAll() is called between Start() and Stop().
// ReSharper disable CompareOfFloatsByEqualityOperator
      if (_startTime == -1)
        return;
// ReSharper restore CompareOfFloatsByEqualityOperator

#if !PORTABLE && !SILVERLIGHT
      var stopTime = Profiler.Stopwatch.Elapsed.TotalSeconds;
      AddValue(stopTime - _startTime);
#endif
      _startTime = -1;
    }


    internal void AddValue(double value)
    {
      if (Count == 0)
      {
        // First added value.
        Sum = value;
        Minimum = value;
        Maximum = value;
        Last = value;
      }
      else
      {
        Sum += value;
        Minimum = Math.Min(Minimum, value);
        Maximum = Math.Max(Maximum, value);
        Last = value;
      }

      Count++;
    }
    #endregion
  }
}
