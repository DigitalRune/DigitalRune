// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

//using System;
//using System.Diagnostics;


//namespace DigitalRune.Diagnostics
//{
//  /// <summary>
//  /// Automatically calls <see cref="Profiler.Start"/> and <see cref="Profiler.Stop"/> to 
//  /// measure time.
//  /// </summary>
//  /// <remarks>
//  /// <para>
//  /// When an instance of this type is created, time measurement for the given profiler data is 
//  /// started. When the instance is disposed, the time measurement is stopped.
//  /// </para>
//  /// <para>
//  /// <strong>Thread-safety:</strong> If <see cref="IDisposable.Dispose"/> is called manually
//  /// for a <see cref="ProfilerSample"/> instance (instead of using the "using" statement), 
//  /// it must be called by the same thread that created the instance.
//  /// </para>
//  /// </remarks>
//  /// <example>
//  /// Use <see cref="ProfilerSample"/> instead of calling Start/Stop of the profiler manually:
//  /// <code lang="csharp">
//  /// <![CDATA[
//  /// public void Foo()
//  /// {
//  ///   using (new ProfilerSample("Foo"))     // Profile time of Foo().
//  ///   {
//  ///     // Do work of this method.
//  ///     ...
//  ///   }
//  /// }
//  /// ]]>
//  /// </code>
//  /// </example>
//  public struct ProfilerSample : IDisposable
//  {
//    private readonly ProfilerData _profiler;


//    /// <summary>
//    /// Initializes a new instance of the <see cref="ProfilerSample"/> struct and calls
//    /// <see cref="Profiler.Start"/> with the given profiler data name.
//    /// </summary>
//    /// <param name="name">
//    /// The name that identifies this profiler data.
//    /// </param>    
//    public ProfilerSample(string name)
//    {
//      _profiler = Profiler.Get(name);
//      _profiler.Start();
//    }


//    /// <summary>
//    /// Disposes this instance and calls <see cref="Profiler.Stop"/>.
//    /// </summary>
//    public void Dispose()
//    {
//      _profiler.Stop();
//    }
//  }
//}
