// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using DigitalRune.Collections;


namespace DigitalRune.Diagnostics
{
  /// <summary>
  /// Provides support for simple, non-hierarchical profiling. (Not available in Silverlight.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="Profiler"/>s can be used for simple time measurements of code run-time and to
  /// record interesting numbers. (If a hierarchical, more advanced system is desired, 
  /// <see cref="HierarchicalProfiler"/> can be used instead.)
  /// </para>
  /// <para>
  /// New profiling data can be added by calling <see cref="Start"/>/<see cref="Stop"/> to measure
  /// time, or by calling <see cref="AddValue"/> to add any other data. For each call of these 
  /// methods <see cref="ProfilerData"/> is recorded. <see cref="ProfilerData"/> is identified by 
  /// name. The name needs to be specified in the methods <see cref="Start"/>, <see cref="Stop"/> 
  /// and <see cref="Stop"/>. The name is user-defined, often the name of the method where the time 
  /// is measured, or the name of the value added (see example below).
  /// </para>
  /// <para>
  /// <strong>Conditional Compilation Symbol "DIGITALRUNE_PROFILE":</strong> The methods of this
  /// class are decorated with the <see cref="ConditionalAttribute"/>. Compilers that support 
  /// <see cref="ConditionalAttribute"/> ignore calls to these methods unless "DIGITALRUNE_PROFILE"
  /// is defined as a conditional compilation symbol. That means calling the methods 
  /// <see cref="Reset()"/>, <see cref="ResetAll()"/>, <see cref="Start"/>, <see cref="Stop"/> and 
  /// <see cref="AddValue"/> does not influence execution performance unless the conditional 
  /// compilation symbol "DIGITALRUNE_PROFILE" is defined. The conditional compilation symbol should
  /// be undefined for public, released versions of an application and profiling should only be used
  /// during development. (This is similar to the standard .NET class 
  /// <strong>System.Diagnostics.Trace</strong> and the conditional compilation symbol "TRACE". See 
  /// documentation of class <strong>System.Diagnostics.Trace</strong>.)
  /// </para>
  /// <para>
  /// <strong>Multithreading:</strong> The profiler can be used in multithreaded applications. For 
  /// each thread separate profiler data is collected (see <see cref="Data"/>). Most methods are 
  /// thread-safe and work lock-free. Following properties and methods are NOT thread-safe (see 
  /// property or method description): <see cref="Data"/>, <see cref="Get(int)"/>, 
  /// <see cref="ResetAll()"/>, <see cref="ResetAll(string)"/>, <see cref="Reset(int)"/>, 
  /// <see cref="DumpAll()"/>, <see cref="Dump(int)"/>.
  /// </para>
  /// </remarks>
  /// <example>
  /// This example shows how to use the profiler in a simple multithreaded application.
  /// <code lang="csharp">
  /// <![CDATA[
  ///  // The compilation symbol "DIGITALRUNE_PROFILE" must be defined to activate profiling.
  ///  #define DIGITALRUNE_PROFILE
  ///
  ///  using System;
  ///  using DigitalRune.Diagnostics;
  ///  using DigitalRune.Threading;
  ///
  ///  namespace ProfilingTest
  ///  {
  ///    class Program
  ///    {
  ///      static void Main(string[] args)
  ///      {
  ///        // Warmstart: We call Foo and the Parallel class so that all one-time initializations are 
  ///        // done before we start measuring.
  ///        Parallel.For(0, 100, i => Foo());
  ///        Profiler.ResetAll();
  ///
  ///        // Measure time of a sequential for-loop.
  ///        Profiler.Start("MainSequential");
  ///        for (int i = 0; i < 100; i++)
  ///          Foo();
  ///        Profiler.Stop("MainSequential");
  ///
  ///        // Measure time of a parallel for-loop.
  ///        Profiler.Start("MainParallel");
  ///        Parallel.For(0, 100, i => Foo());
  ///        Profiler.Stop("MainParallel");}
  ///
  ///        // Format the output by defining a useful scale. We add descriptions so that any other 
  ///        // developer looking at the output can interpret them more easily.
  ///        Profiler.SetFormat("MainSequential", 1e3f, "[ms]");
  ///        Profiler.SetFormat("MainParallel", 1e3f, "[ms]");
  ///        Profiler.SetFormat("Foo", 1e6f, "[µs]");
  ///        Profiler.SetFormat("ValuesBelow10", 1.0f / 100.0f, "[%]");
  ///
  ///        // Print the profiling results.
  ///        Console.WriteLine(Profiler.DumpAll());
  ///        Console.ReadKey();
  ///      }
  ///
  ///      public static void Foo()
  ///      {
  ///        Profiler.Start("Foo");
  ///        
  ///        var random = new Random();
  ///        int numberOfValuesBelow10 = 0;
  ///        for (int i = 0; i < 10000; i++)
  ///        {
  ///          int x = random.Next(0, 100);
  ///          if (x < 10)
  ///            numberOfValuesBelow10++;
  ///        }
  ///
  ///        // Profilers can also collect other interesting numbers (not only time). 
  ///        Profiler.AddValue("ValuesBelow10", numberOfValuesBelow10);
  /// 
  ///        Profiler.Stop("Foo");
  ///      }
  ///    }
  ///  }
  /// 
  /// /* This writes following output to the console:
  /// (The values after "Thread:" are the thread name and the ManagedThreadId.)
  /// 
  /// Thread:  (#1)
  /// Name              Calls      Sum          Min        Avg        Max Description
  /// -------------------------------------------------------------------------------
  /// Foo                 127  37895,500    286,800    298,390    385,100 [µs]
  /// ValuesBelow10       127   1271,060      9,700     10,008     10,160 [%]
  /// MainSequential        1     29,834     29,834     29,834     29,834 [ms]
  /// MainParallel          1      8,717      8,717      8,717      8,717 [ms]
  /// 
  /// Thread: Parallel Worker 0 (#3)
  /// Name              Calls      Sum          Min        Avg        Max Description
  /// -------------------------------------------------------------------------------
  /// Foo                  27   8128,200    288,000    301,044    417,400 [µs]
  /// ValuesBelow10        27    272,640     10,040     10,098     10,160 [%]
  /// 
  /// Thread: Parallel Worker 1 (#4)
  /// Name              Calls      Sum          Min        Avg        Max Description
  /// -------------------------------------------------------------------------------
  /// Foo                  19   7812,600    340,200    411,189   1307,900 [µs]
  /// ValuesBelow10        19    191,720     10,040     10,091     10,160 [%]
  /// 
  /// Thread: Parallel Worker 2 (#5)
  /// Name              Calls      Sum          Min        Avg        Max Description
  /// -------------------------------------------------------------------------------
  /// Foo                  27   7998,300    286,800    296,233    326,800 [µs]
  /// ValuesBelow10        27    272,640     10,040     10,098     10,160 [%]
  /// 
  /// Thread: Parallel Worker 3 (#6)
  /// Name              Calls      Sum          Min        Avg        Max Description
  /// -------------------------------------------------------------------------------
  /// Foo                   0          -          -          -          - [µs]
  /// ValuesBelow10         0          -          -          -          - [%]
  /// */
  /// ]]>
  /// </code>
  /// </example>  
  public static class Profiler
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the profiler data for each thread.
    /// </summary>
    /// <value>The profiler data.</value>
    /// <remarks>
    /// <para>
    /// Each thread that uses the <see cref="Profiler"/> has its own 
    /// <see cref="ProfilerDataCollection"/>.
    /// </para>
    /// <para>
    /// <strong>Thread-Safety:</strong> Access to this property is not thread-safe.
    /// </para>
    /// </remarks>
    public static IEnumerable<ProfilerDataCollection> Data
    {
      get
      {
        foreach (var pair in _data)
          yield return pair.Value;
      }
    }
    private static SynchronizedHashtable<int, ProfilerDataCollection> _data;


    /// <summary>
    /// Gets the format data for each <see cref="ProfilerData"/>.
    /// </summary>
    /// <value>The format data.</value>
    /// <remarks>
    /// All <see cref="ProfilerData"/> instances with the same name have the same format data
    /// (across threads). Therefore, this data is stored here and not in a specific
    /// <see cref="ProfilerData"/> instance.
    /// </remarks>
    internal static Dictionary<string, ProfilerDataFormat> Formats { get; private set; }


#if !PORTABLE && !SILVERLIGHT
    /// <summary>
    /// Gets the stopwatch.
    /// </summary>
    /// <value>The stopwatch.</value>
    /// <remarks>
    /// This <see cref="Stopwatch"/> is started when the <see cref="Profiler"/> class is 
    /// loaded and it runs permanently.
    /// </remarks>
    internal static System.Diagnostics.Stopwatch Stopwatch { get; private set; }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes the static members of the <see cref="Profiler"/> class.
    /// </summary>
    static Profiler()
    {
      ClearAll();

      // Create and start the stopwatch.
#if !PORTABLE && !SILVERLIGHT
      Stopwatch = System.Diagnostics.Stopwatch.StartNew();
#endif

      Formats = new Dictionary<string, ProfilerDataFormat>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Removes all profiler data.
    /// </summary>
    /// <remarks>
    /// Formatting data set with <see cref="SetFormat"/> are not removed.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule", Justification = "Accessing Environment.ProcessorCount on Xbox 360 works.")]
    public static void ClearAll()
    {
      // TODO: Add SynchronizedHashtable.Clear method.

#if WP7
      // Cannot access Environment.ProcessorCount in phone app. (Security issue).
      _data = new SynchronizedHashtable<int, ProfilerDataCollection>(8);
#else
      _data = new SynchronizedHashtable<int, ProfilerDataCollection>(Environment.ProcessorCount * 4);
#endif
    }


    /// <overloads>
    /// <summary>
    /// Gets the profiler data.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the <see cref="ProfilerDataCollection"/> for the current thread.
    /// </summary>
    /// <returns>The <see cref="ProfilerDataCollection"/> for the current thread.</returns>
    public static ProfilerDataCollection Get()
    {
#if !NETFX_CORE && !NET45
      return Get(Thread.CurrentThread.ManagedThreadId);
#else
      return Get(Environment.CurrentManagedThreadId);
#endif
    }


    /// <summary>
    /// Gets the <see cref="ProfilerDataCollection" /> for the specified thread.
    /// </summary>
    /// <param name="threadId">
    /// The thread ID, a unique identifier for the managed thread. (See 
    /// <see cref="Thread.ManagedThreadId"/>.)
    /// </param>
    /// <returns>
    /// The <see cref="ProfilerDataCollection" /> for the specified thread.
    /// </returns>
    /// <remarks>
    /// <strong>Thread-Safety:</strong> Accessing the profiler data of a thread that is not the
    /// current thread is not thread-safe.
    /// </remarks>
    public static ProfilerDataCollection Get(int threadId)
    {
      ProfilerDataCollection collection;
      if (!_data.TryGet(threadId, out collection))
      {
        // ProfilerDataCollection does not exist. --> Create a new one.
        collection = new ProfilerDataCollection("Unnamed", threadId);
        _data.Add(threadId, collection);
      }

      return collection;
    }


#if !NETFX_CORE && !PORTABLE
    /// <summary>
    /// Gets the <see cref="ProfilerDataCollection"/> for the specified thread. 
    /// (Not available on these platforms: WinRT)
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <returns>The <see cref="ProfilerDataCollection"/> for the specified thread.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Thread-Safety:</strong> Accessing the profiler data of a thread that is not the 
    /// current thread is not thread-safe.
    /// </para>
    /// <para>
    /// This method is not available on the following platforms: WinRT
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="thread"/> is <see langword="null"/>.
    /// </exception>
    public static ProfilerDataCollection Get(Thread thread)
    {
      if (thread == null)
        throw new ArgumentNullException("thread");

      ProfilerDataCollection collection;
      int threadId = thread.ManagedThreadId;
      if (!_data.TryGet(threadId, out collection))
      {
        // ProfilerDataCollection does not exist. --> Create a new one.
        collection = new ProfilerDataCollection(thread.Name, threadId);
        _data.Add(threadId, collection);
      }

      return collection;
    }
#endif


    /// <summary>
    /// Gets the <see cref="ProfilerData"/> with the given name for the current thread.
    /// </summary>
    /// <param name="name">The name of the profiler data.</param>
    /// <returns>
    /// The <see cref="ProfilerData"/> with the given name for the current thread.
    /// </returns>
    public static ProfilerData Get(string name)
    {
      var collection = Get();

      ProfilerData data;
      if (!collection.TryGet(name, out data))
      {
        // Not existing? --> Create a new instance.
        data = new ProfilerData(name);
        collection.Add(data);
      }

      return data;
    }


    /// <overloads>
    /// <summary>
    /// Resets profiler data for all threads.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Resets all <see cref="ProfilerData"/> for all threads.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Thread-Safety:</strong> This method is not thread-safe and must only be called when
    /// it is assured that no other thread uses the profiler.
    /// </para>
    /// </remarks>
    [Conditional("DIGITALRUNE_PROFILE")]
    public static void ResetAll()
    {
      foreach (var pair in _data)
        pair.Value.Reset();
    }


    /// <summary>
    /// Resets the <see cref="ProfilerData"/> with the given name for all threads.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <remarks>
    /// <para>
    /// <strong>Thread-Safety:</strong> This method is not thread-safe and must only be called when
    /// it is assured that no other thread uses the profiler.
    /// </para>
    /// </remarks>
    [Conditional("DIGITALRUNE_PROFILE")]
    public static void ResetAll(string name)
    {
      foreach (var pair in _data)
        foreach (var data in pair.Value)
          if (data.Name == name)
            data.Reset();
    }


    /// <overloads>
    /// <summary>
    /// Resets the profiler data.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Resets all <see cref="ProfilerData"/> for the current thread.
    /// </summary>
    [Conditional("DIGITALRUNE_PROFILE")]
    public static void Reset()
    {
#if NETFX_CORE || NET45
      Reset(Environment.CurrentManagedThreadId);
#else
      Reset(Thread.CurrentThread.ManagedThreadId);
#endif

    }


#if !NETFX_CORE && !PORTABLE
    /// <summary>
    /// Resets all <see cref="ProfilerData"/> for the given thread.
    /// (Not available on these platforms: WinRT)
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <remarks>
    /// <para>
    /// <strong>Thread-Safety:</strong> Accessing the profiler data of a thread that is not the
    /// current thread is not thread-safe.
    /// </para>
    /// <para>
    /// This method is not available on the following platforms: WinRT
    /// </para>
    /// </remarks>
    [Conditional("DIGITALRUNE_PROFILE")]
    public static void Reset(Thread thread)
    {
      Get(thread).Reset();
    }
#endif


    /// <summary>
    /// Resets all <see cref="ProfilerData"/> for the given thread.
    /// </summary>
    /// <param name="managedThreadId">The managed thread ID.</param>
    /// <remarks>
    /// <strong>Thread-Safety:</strong> Accessing the profiler data of a thread that is not the
    /// current thread is not thread-safe.
    /// </remarks>
    [Conditional("DIGITALRUNE_PROFILE")]
    public static void Reset(int managedThreadId)
    {
      Get(managedThreadId).Reset();
    }


    /// <summary>
    /// Resets the <see cref="ProfilerData"/> with the given name (only for the current thread).
    /// </summary>
    /// <param name="name">The name of the profiler data.</param>
    [Conditional("DIGITALRUNE_PROFILE")]
    public static void Reset(string name)
    {
      Get(name).Reset();
    }

    
    // This method should not be used because we cannot decorate it with the ConditionalAttribute.
    // (ConditionalAttribute can only be used if return type is void.)

    ///// <summary>
    ///// Starts time measurement for the given profiler data and returns an object that can be used 
    ///// to stop the time measurement.
    ///// </summary>
    ///// <param name="name">The name of the <see cref="ProfilerData"/>.</param>
    ///// <returns>An object that can be used to stop the time measurement.</returns>
    ///// <remarks>
    ///// <para>
    ///// This method starts time measurement for the given profiler data and returns an object 
    ///// (<see cref="IDisposable"/>) that can be used to stop the time measurement. The measurement
    ///// is stopped when <see cref="IDisposable.Dispose"/> is called on the returned object.
    ///// </para>
    ///// <para>
    ///// In C# the method can be used in combination with a <c>using</c> statement.
    ///// </para>
    ///// </remarks>
    ///// <example>
    ///// Use <see cref="Sample"/> instead of calling Start/Stop of the profiler manually:
    ///// <code lang="csharp">
    ///// <![CDATA[
    ///// public void Foo()
    ///// {
    /////   using (Profiler.Sample("Foo"))     // Profile time of Foo().
    /////   {
    /////     // Do work of this method.
    /////     ...
    /////   }
    ///// }
    ///// ]]>
    ///// </code>
    ///// </example>
    //public static ProfilerSample Sample(string name)
    //{
    //  return new ProfilerSample(name);
    //}


    /// <summary>
    /// Starts time measurement for the <see cref="ProfilerData"/> with the given name.
    /// </summary>
    /// <param name="name">The name of the <see cref="ProfilerData"/>.</param>
    [Conditional("DIGITALRUNE_PROFILE")]
    public static void Start(string name)
    {
      var profiler = Get(name);
      profiler.Start();
    }


    /// <summary>
    /// Stops time measurement for the <see cref="ProfilerData"/> with the given name and records
    /// the elapsed time in seconds.
    /// </summary>
    /// <param name="name">The name of the <see cref="ProfilerData"/>.</param>
    /// <remarks>
    /// This method calls <see cref="AddValue"/> with the elapsed time in seconds.
    /// </remarks>
    [Conditional("DIGITALRUNE_PROFILE")]
    public static void Stop(string name)
    {
      Get(name).Stop();
    }


    /// <summary>
    /// Adds the value to the <see cref="ProfilerData"/> with the given name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    [Conditional("DIGITALRUNE_PROFILE")]
    public static void AddValue(string name, double value)
    {
      Get(name).AddValue(value);
    }


    /// <summary>
    /// Sets the formatting data for <see cref="ProfilerData"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="ProfilerData"/>.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="description">The description.</param>
    /// <remarks>
    /// <para>
    /// This method allows to specify additional data that is used in the <see cref="DumpAll"/> and 
    /// <see cref="Dump()"/> methods.
    /// </para>
    /// <para>
    /// All <see cref="ProfilerData"/> instances with the same name use the same formatting data
    /// (across threads). The data values are multiplied with the <paramref name="scale"/> and the
    /// description is added to the dump output. 
    /// </para>
    /// </remarks>
    /// <example>
    /// Here, the profiler data of the method "Foo" is scaled, so that the values are in µs. A
    /// description is set so that the user knows the unit of the displayed values.
    /// <code lang="csharp">
    /// <![CDATA[
    /// Profiler.SetFormat("Foo", 1e6f, "[µs]");
    /// ]]>
    /// </code>
    /// </example>
    public static void SetFormat(string name, double scale, string description)
    {
      lock (((ICollection)Formats).SyncRoot)
      {
        ProfilerDataFormat format;
        if (!Formats.TryGetValue(name, out format))
        {
          format = new ProfilerDataFormat();
          Formats.Add(name, format);
        }

        format.Scale = scale;
        format.Description = description;
      }
    }


    /// <summary>
    /// Returns a string that contains all collected profiler data (for all threads). 
    /// </summary>
    /// <returns>
    /// A string containing all collected profiler data (for all threads). The string contains a
    /// table for each thread.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Thread-Safety:</strong> This method is not thread-safe and must only be called when
    /// it is assured that no other thread adds profiler data.
    /// </para>
    /// </remarks>
    public static string DumpAll()
    {
      StringBuilder stringBuilder = new StringBuilder();
      bool addNewLine = false;
      foreach (var pair in _data)
      {
        if (addNewLine)
          stringBuilder.AppendLine();
        addNewLine = true;

        stringBuilder.Append(pair.Value.Dump());
      }

      return stringBuilder.ToString();
    }


    /// <overloads>
    /// <summary>
    /// Returns a string containing profiler data.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Returns a string that contains a table with all <see cref="ProfilerData"/> instances for the
    /// current thread.
    /// </summary>
    /// <returns>
    /// A string containing a table with all profiler data values for the current thread.
    /// </returns>
    public static string Dump()
    {
      return Get().Dump();
    }


#if !NETFX_CORE && !PORTABLE
    /// <summary>
    /// Returns a string that contains a table with all <see cref="ProfilerData"/> instances for the
    /// given thread.
    /// (Not available on these platforms: WinRT)
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <returns>
    /// A string containing a table with all profiler data values for the given thread.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Thread-Safety:</strong> This method is not thread-safe and must only be called when 
    /// it is assured that the given <paramref name="thread"/> does not add new profiler data.
    /// </para>
    /// <para>
    /// This method is not available on the following platforms: WinRT
    /// </para>
    /// </remarks>
    public static string Dump(Thread thread)
    {
      return Get(thread).Dump();
    }
#endif


    /// <summary>
    /// Returns a string that contains a table with all <see cref="ProfilerData"/> instances for the
    /// given thread.
    /// </summary>
    /// <param name="managedThreadId">The managed thread ID.</param>
    /// <returns>
    /// A string containing a table with all profiler data values for the given thread.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Thread-Safety:</strong> This method is not thread-safe and must only be called when
    /// it is assured that the thread with the given <paramref name="managedThreadId"/> does not add 
    /// new profiler data.
    /// </para>
    /// </remarks>
    public static string Dump(int managedThreadId)
    {
      return Get(managedThreadId).Dump();
    }
    #endregion
  }
}
