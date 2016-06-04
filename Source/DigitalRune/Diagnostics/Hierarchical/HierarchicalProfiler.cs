// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;


namespace DigitalRune.Diagnostics
{
  /// <summary>
  /// Provides support for hierarchical profiling. (Not available in Silverlight.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// This profiler allows to measure time of method calls and nested method calls, creating a tree
  /// structure of time measurements. (Use <see cref="Profiler"/> instead for simple, 
  /// non-hierarchical profiling.)
  /// </para>
  /// <para>
  /// This hierarchical profiler is similar to <see cref="Profiler"/> but every time 
  /// <see cref="Start"/> is called, a <see cref="HierarchicalProfilerNode"/> is added to a tree of
  /// nodes starting with <see cref="Root"/>. Each node stores the time of a method or code section.
  /// The parent of a node represents the method/code section that is the caller. The children of a
  /// node represent methods called by the current method. See example.
  /// </para>
  /// <para>
  /// <strong>Conditional Compilation Symbol "DIGITALRUNE_PROFILE":</strong> The methods of this
  /// class are decorated with the <see cref="ConditionalAttribute"/>. Compilers that support 
  /// <see cref="ConditionalAttribute"/> ignore calls to these methods unless "DIGITALRUNE_PROFILE"
  /// is defined as a conditional compilation symbol. That means calling the methods 
  /// <see cref="Reset()"/>, <see cref="NewFrame"/>, <see cref="Start"/> and <see cref="Stop"/> does
  /// not influence execution performance unless the conditional compilation symbol
  /// "DIGITALRUNE_PROFILE" is defined. The conditional compilation symbol should be undefined for
  /// public, released versions of an application and profiling should only be used during
  /// development. (This is similar to the standard .NET class 
  /// <strong>System.Diagnostics.Trace</strong> and the conditional compilation symbol "TRACE". See
  /// documentation of class <strong>System.Diagnostics.Trace</strong>.)
  /// </para>
  /// <para>
  /// <strong>Thread-Safety:</strong> The <see cref="HierarchicalProfiler"/> is not thread-safe and
  /// single instance of <see cref="HierarchicalProfiler"/> cannot be used to profile the timing of 
  /// parallel running code sections.
  /// </para>
  /// </remarks>
  /// <example>
  /// <code lang="csharp">
  /// <![CDATA[
  ///  // The compilation symbol "DIGITALRUNE_PROFILE" must be defined to activate profiling.
  ///  #define DIGITALRUNE_PROFILE
  /// 
  ///  using System;
  ///  using System.Threading;
  ///  using DigitalRune.Diagnostics;
  ///
  ///namespace ProfilingTest
  ///  {
  ///    class Program
  ///    {
  ///      // The profiler instance.
  ///      public static HierarchicalProfiler _profiler = new HierarchicalProfiler("MyProfiler");
  ///
  ///      static void Main(string[] args)
  ///      {
  ///        // Start profiling.
  ///        _profiler.Reset();
  ///
  ///        // This represents the main-loop of a game.
  ///        for (int i = 0; i < 10; i++)
  ///        {
  ///          // NewFrame() must be called when a new frame of the game begins.
  ///          _profiler.NewFrame();
  ///
  ///          Update();
  ///          Draw();
  ///        }
  ///
  ///        // Write the profiler data to the console. We start at the root node and include 
  ///        // up to 5 child levels.
  ///        Console.WriteLine(_profiler.Dump(_profiler.Root, 5));
  ///      }
  ///
  ///      private static void Update()
  ///      {
  ///        _profiler.Start("Update");
  ///        
  ///        Physics();
  ///        AI();
  ///        AI();
  ///        AI();
  ///        Thread.Sleep(1);
  ///        
  ///        _profiler.Stop();
  ///      }
  ///
  ///      private static void Physics()
  ///      {
  ///        _profiler.Start("Physics");
  ///        
  ///        Thread.Sleep(6);
  ///        
  ///        _profiler.Stop();
  ///      }
  ///
  ///      private static void AI()
  ///      {
  ///        _profiler.Start("AI");
  ///        
  ///        Thread.Sleep(3);
  ///        
  ///        _profiler.Stop();
  ///      }
  ///
  ///      private static void Draw()
  ///      {
  ///        _profiler.Start("Draw");
  ///        
  ///        Thread.Sleep(4);
  ///        
  ///         _profiler.Stop();
  ///      }
  ///    }
  ///  }
  ///
  ///  /* This program creates following output:
  ///     (The percent values show the time of the node relative to the root of the dump.
  ///     The values in () are Minimum/Average/Maximum times.
  ///     'Other' represents the time of a node that was not measured by a child node.
  ///
  ///  Profile 'MyProfiler' Node 'Root' 201.718ms total 10 frames
  ///    Update 79.2% 15.978ms/frame 1 calls/frame (15.838ms/15.978ms/16.006ms)
  ///      Physics 29.8% 6.005ms/frame 1 calls/frame (5.983ms/6.005ms/6.083ms)
  ///      AI 44.3% 8.938ms/frame 3 calls/frame (2.412ms/2.979ms/3.559ms)
  ///      Other 5.1% 10.35ms 1.035ms/frame
  ///    Draw 19.8% 3.984ms/frame 1 calls/frame (3.81ms/3.984ms/4.018ms)
  ///    Other 2.4% 4.787ms 478.69us/frame
  ///  */
  /// ]]>
  /// </code>
  /// </example>
  public class HierarchicalProfiler : INamedObject
  {
    // Notes:
    // See Hierarchical Profiler in GPG3.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the name of this profiler.
    /// </summary>
    /// <value>The name of this profiler.</value>
    public string Name { get; private set; }


    /// <summary>
    /// Gets the root node.
    /// </summary>
    /// <value>The root node.</value>
    public HierarchicalProfilerNode Root { get; private set; }


    private HierarchicalProfilerNode CurrentNode { get; set; }


#if !PORTABLE && !SILVERLIGHT
    /// <summary>
    /// Gets the stopwatch.
    /// </summary>
    /// <value>The stopwatch.</value>
    internal System.Diagnostics.Stopwatch Stopwatch { get; private set; }
#endif


    /// <summary>
    /// Gets the number of frames.
    /// </summary>
    /// <value>The number of frames.</value>
    public int FrameCount { get; private set; }


    /// <summary>
    /// Gets the elapsed time since the creation of this instance or the last <see cref="Reset"/>.
    /// </summary>
    /// <value>The elapsed time.</value>
    public TimeSpan ElapsedTime
    {
      get
      {
#if PORTABLE
        throw Portable.NotImplementedException;
#elif SILVERLIGHT
        throw new NotSupportedException();
#else
        return Stopwatch.Elapsed;
#endif
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="HierarchicalProfiler"/> class.
    /// </summary>
    /// <param name="name">The name of this profiler.</param>
    public HierarchicalProfiler(string name)
    {
#if PORTABLE
      throw Portable.NotImplementedException;
#elif SILVERLIGHT
      throw new NotSupportedException();
#else
      Name = name;
      Root = new HierarchicalProfilerNode("Root", this);
      CurrentNode = Root;
      Stopwatch = System.Diagnostics.Stopwatch.StartNew();
#endif
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Resets all timing data.
    /// </summary>
    /// <remarks>
    /// The node hierarchy is not reset - only the stored values in all nodes.
    /// </remarks>
    [Conditional("DIGITALRUNE_PROFILE")]
    public void Reset()
    {
      Root.Reset();
      CurrentNode = Root;
      FrameCount = 0;
#if !PORTABLE && !SILVERLIGHT
      Stopwatch.Reset();
      Stopwatch.Start();
#endif
    }


    // This method should not be used because we cannot decorate it with the ConditionalAttribute.
    // (ConditionalAttribute can only be used if return type is void.)
    //
    ///// <summary>
    ///// Starts time measurement for a node and returns an object that can be used to stop the 
    ///// time measurement.
    ///// </summary>
    ///// <param name="nodeName">The name of the node.</param>
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
    /////   using (myProfiler.Sample("Foo"))     // Profile time of Foo().
    /////   {
    /////     // Do work of this method.
    /////     ...
    /////   }
    ///// }
    ///// ]]>
    ///// </code>
    ///// </example>
    //public HierarchicalProfilerSample Sample(string nodeName)
    //{
    //  return new HierarchicalProfilerSample(this, nodeName);
    //}


    /// <summary>
    /// Starts time measurement for a node.
    /// </summary>
    /// <param name="nodeName">The name of the node.</param>
    /// <remarks>
    /// If a child node with the same name exists, the time will be accumulated for this node.
    /// If no child with this name exists, a new child node is created.
    /// </remarks>
    [Conditional("DIGITALRUNE_PROFILE")]
    public void Start(string nodeName)
    {
      // Ignore any calls before the first NewFrame call. This occurs when Reset() 
      // is called in the middle of profiling.
      if (FrameCount <= 0)
        return;

      if (nodeName != CurrentNode.Name)
        CurrentNode = CurrentNode.GetSubNode(nodeName);

      CurrentNode.Call();
    }


    /// <summary>
    /// Stops time measurement for a node.
    /// </summary>
    [Conditional("DIGITALRUNE_PROFILE")]
    public void Stop()
    {
      // Ignore any calls before the first NewFrame call. This occurs when Reset() 
      // is called in the middle of profiling.
      if (FrameCount <= 0)
        return;

      bool noRecursion = CurrentNode.Return();

      if (noRecursion)
        CurrentNode = CurrentNode.Parent;
    }


    /// <summary>
    /// Must be called when a new frame begins.
    /// </summary>
    /// <remarks>
    /// This method is usually called at the beginning of the main-loop of the game.
    /// </remarks>
    [Conditional("DIGITALRUNE_PROFILE")]
    public void NewFrame()
    {
      FrameCount++;
    }


    /// <summary>
    /// Dumps the profiled data of the given node.
    /// </summary>
    /// <param name="node">The node. Can be <see langword="false"/> to use the root node.</param>
    /// <param name="maxLevelIncluded">
    /// The maximal level included relative to the given node. 0 means, no child data is included.
    /// 1 means, one level of children are included.
    /// </param>
    /// <returns>
    /// A string representing the profiled data.
    /// </returns>    
    public string Dump(HierarchicalProfilerNode node, int maxLevelIncluded)
    {
      if (maxLevelIncluded < 0 || FrameCount == 0)
        return string.Empty;

      if (node == null)
        node = Root;

      var stringBuilder = new StringBuilder();

      TimeSpan totalTime = (node == Root) ? ElapsedTime : node.Sum;

      stringBuilder.Append(Name);
      stringBuilder.Append(".");
      stringBuilder.Append(node.Name);
      stringBuilder.Append(" ");
      stringBuilder.Append(FrameCount);
      stringBuilder.Append(" frames ");
      AppendTime(stringBuilder, new TimeSpan(totalTime.Ticks / FrameCount));
      stringBuilder.Append("/frame ");
      AppendTime(stringBuilder, totalTime);
      stringBuilder.Append(" total");
      stringBuilder.AppendLine();

      Dump(stringBuilder, node, 1, maxLevelIncluded, totalTime);
      return stringBuilder.ToString();
    }


    /// <summary>
    /// Dumps the specified string builder (recursive).
    /// </summary>
    private void Dump(StringBuilder stringBuilder, HierarchicalProfilerNode node, int level, int maxLevelIncluded, TimeSpan totalTime)
    {
      if (level > maxLevelIncluded)
        return;

      TimeSpan childrenSum = TimeSpan.Zero;
      int numberOfChildren = 0;
      foreach (var child in node.Children)
      {
        numberOfChildren++;
        childrenSum += child.Sum;

        for (int i = 0; i < level; i++)
          stringBuilder.Append("  ");

        stringBuilder.Append(child.Name);
        stringBuilder.Append(" ");
        stringBuilder.Append(((double)child.Sum.Ticks / totalTime.Ticks).ToString("0.##% ", CultureInfo.InvariantCulture));
        //AppendTime(stringBuilder, child.Sum);
        //stringBuilder.Append(" ");
        AppendTime(stringBuilder, new TimeSpan(child.Sum.Ticks / FrameCount));
        stringBuilder.Append("/frame ");
        stringBuilder.Append(((double)child.Count / FrameCount).ToString("0.###", CultureInfo.InvariantCulture));
        stringBuilder.Append(" calls/frame (");
        AppendTime(stringBuilder, child.Minimum);
        stringBuilder.Append("/");
        AppendTime(stringBuilder, child.Average);
        stringBuilder.Append("/");
        AppendTime(stringBuilder, child.Maximum);
        stringBuilder.AppendLine(")");

        Dump(stringBuilder, child, level + 1, maxLevelIncluded, totalTime);
      }

      if (numberOfChildren > 0)
      {
        for (int i = 0; i < level; i++)
          stringBuilder.Append("  ");

        stringBuilder.Append("Other ");
        TimeSpan unaccountedTime = (node == Root ? ElapsedTime : node.Sum) - childrenSum;
        stringBuilder.Append(((double)unaccountedTime.Ticks / totalTime.Ticks).ToString("0.##% ", CultureInfo.InvariantCulture));
        AppendTime(stringBuilder, unaccountedTime);
        stringBuilder.Append(" ");
        AppendTime(stringBuilder, new TimeSpan(unaccountedTime.Ticks / FrameCount));
        stringBuilder.Append("/frame ");
        stringBuilder.AppendLine();
      }
    }


    /// <summary>
    /// Appends the formatted time value to the given string.
    /// </summary>
    /// <param name="stringBuilder">The string builder.</param>
    /// <param name="time">The time.</param>
    private static void AppendTime(StringBuilder stringBuilder, TimeSpan time)
    {
      // Scale time to s, ms or µs to use a suitable numerical range.
      double seconds = time.TotalSeconds;
      if (seconds >= 1)
      {
        stringBuilder.Append(seconds.ToString("0.###s", CultureInfo.InvariantCulture));
      }
      else if (seconds >= 0.001)
      {
        var milliseconds = seconds * 1000;
        stringBuilder.Append(milliseconds.ToString("0.###ms", CultureInfo.InvariantCulture));
      }
      else
      {
        var microseconds = seconds * 1e6;
        stringBuilder.Append(microseconds.ToString("0.###us", CultureInfo.InvariantCulture));
      }
    }
    #endregion
  }
}
