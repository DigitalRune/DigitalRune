// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

//using System;

//namespace DigitalRune.Diagnostics
//{
//  /// <summary>
//  /// Automatically calls <see cref="HierarchicalProfiler.Start"/> and 
//  /// <see cref="HierarchicalProfiler.Stop"/> to measure time for a given 
//  /// <see cref="HierarchicalProfiler"/>.
//  /// </summary>
//  /// <remarks>
//  /// <para>
//  /// When an instance of this type is created, time measurement for the given profiler is 
//  /// started. When the instance is disposed, the time measurement is stopped.
//  /// </para>
//  /// </remarks>
//  /// <example>
//  /// Use <see cref="HierarchicalProfilerSample"/> instead of calling Start/Stop of the profiler 
//  /// manually:
//  /// <code lang="csharp">
//  /// <![CDATA[
//  /// public void Foo()
//  /// {
//  ///   using (new HierarchicalProfilerSample(myProfiler, "Foo"))     // Profile time of Foo().
//  ///   {
//  ///     // Do work of this method.
//  ///     ...
//  ///   }
//  /// }
//  /// ]]>
//  /// </code>
//  /// </example>
//  public struct HierarchicalProfilerSample : IDisposable
//  {
//    private readonly HierarchicalProfiler _profiler;


//    /// <summary>
//    /// Initializes a new instance of the <see cref="HierarchicalProfilerSample"/> struct and calls
//    /// <see cref="HierarchicalProfiler.Start"/> for the given <see cref="HierarchicalProfiler"/>.
//    /// </summary>
//    /// <param name="profiler">The profiler instance.</param>
//    /// <param name="name">The name that identifies the profiled section/method.</param>
//    public HierarchicalProfilerSample(HierarchicalProfiler profiler, string name)
//    {
//      _profiler = profiler;
//      _profiler.Start(name);
//    }


//    /// <summary>
//    /// Disposes this instance and calls <see cref="HierarchicalProfiler.Stop"/>.
//    /// </summary>
//    public void Dispose()
//    {
//      _profiler.Stop();
//    }
//  }
//}
