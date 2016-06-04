// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

// The MonoGame portable DLL does not have an Accelerometer class. Since it also 
// doesn't wrap the other sensors (compass, gyrometer, etc.), we leave sensor handling
// to the user.
#if !MONOGAME

#if USE_DIGITALRUNE_MATHEMATICS
using DigitalRune.Mathematics.Algebra;
#else
using Vector2F = Microsoft.Xna.Framework.Vector2;
using Vector3F = Microsoft.Xna.Framework.Vector3;
#endif

#if WP7
using System.Diagnostics;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework.Input;
#endif


namespace DigitalRune.Game.Input
{
  partial class InputManager
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _isAccelerometerInitialized;

#if WP7
    // The accelerometer sensor on the device.
    private readonly Accelerometer _accelerometer = new Accelerometer();

    // Value is set asynchronously in callback.
    private Vector3F _accelerometerCallbackValue;
    private readonly object _syncRoot = new object();
#endif
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    public bool IsAccelerometerActive { get; private set; }


    /// <inheritdoc/>
    public Vector3F AccelerometerValue
    {
      get
      {
        if (!_isAccelerometerInitialized)
        {
          _isAccelerometerInitialized = true;

#if WP7
          try
          {
            _accelerometer.ReadingChanged += OnAccelerometerReadingChanged;
            _accelerometer.Start();
            IsAccelerometerActive = true;
          }
          catch (AccelerometerFailedException)
          {
            IsAccelerometerActive = false;
          }
#endif
        }

        return _accelerometerValue;
      }
    }
    private static Vector3F _accelerometerValue = new Vector3F(0, 0, -1);
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

#if WP7
    // Accelerometer callback.
    private void OnAccelerometerReadingChanged(object sender, AccelerometerReadingEventArgs eventArgs)
    {
      // Store the accelerometer value in our variable to be used on the next Update. This callback
      // can come from another thread!
      lock (_syncRoot)
      {
        _accelerometerCallbackValue = new Vector3F((float)eventArgs.X, (float)eventArgs.Y, (float)eventArgs.Z);
      }
    }
#endif


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
    private void UpdateAccelerometer()
    {
#if WP7
      if (_isAccelerometerInitialized)
      {
        if (IsAccelerometerActive)
        {
          lock (_syncRoot)
          {
            _accelerometerValue = _accelerometerCallbackValue;
          }
        }
      }
#endif
    }
    #endregion
  }
}
#endif