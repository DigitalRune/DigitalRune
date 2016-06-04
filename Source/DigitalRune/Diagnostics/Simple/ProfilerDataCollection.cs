// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections;
using System.Globalization;
using System.Text;
using System.Threading;
using DigitalRune.Collections;


namespace DigitalRune.Diagnostics
{
  /// <summary>
  /// Stores a collection of <see cref="ProfilerData"/> instances for one thread.
  /// </summary>
  /// <remarks>
  /// <see cref="Profiler"/> can be used to profile threaded applications. 
  /// <see cref="ProfilerDataCollection"/> stores all <see cref="ProfilerData"/> instances that were
  /// created by one specific thread.
  /// </remarks>
  public class ProfilerDataCollection : NamedObjectCollection<ProfilerData>
  {
    /// <summary>
    /// Gets the name of the thread. 
    /// </summary>
    /// <value>The name of the thread.</value>
    public string ThreadName { get; private set; }


    /// <summary>
    /// Gets the thread ID.
    /// </summary>
    /// <value>The thread ID.</value>
    public int ThreadId { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ProfilerDataCollection" /> class.
    /// </summary>
    /// <param name="name">The thread name.</param>
    /// <param name="managedThreadId">The managed thread ID.</param>
    internal ProfilerDataCollection(string name, int managedThreadId)
    {
      ThreadName = name;
      ThreadId = managedThreadId;
    }


    /// <summary>
    /// Resets all <see cref="ProfilerData"/> instances. 
    /// </summary>
    internal void Reset()
    {
      foreach (var data in this)
        data.Reset();
    }


    /// <summary>
    /// Returns a string that contains a table with all <see cref="ProfilerData"/> instances.
    /// </summary>
    /// <returns>A string containing a table with all profiler data values.</returns>
    internal string Dump()
    {
      // Access to Profiler.Formats is synchronized.
      lock (((ICollection)Profiler.Formats).SyncRoot)
      {
        var formatProvider = CultureInfo.InvariantCulture;
        StringBuilder stringBuilder = new StringBuilder();
#if NETFX_CORE
        stringBuilder.Append(string.Format(formatProvider, "Thread: #{0}\n", ThreadId));
#else
        stringBuilder.Append(string.Format(formatProvider, "Thread: {0} (#{1})\n", ThreadName, ThreadId));
#endif
        stringBuilder.Append("Name              Calls      Sum          Min        Avg        Max Description\n");
        stringBuilder.Append("-------------------------------------------------------------------------------\n");
        foreach (var data in this)
        {
          // Get format for this data.
          ProfilerDataFormat format;
          if (!Profiler.Formats.TryGetValue(data.Name, out format))
            format = ProfilerDataFormat.Default;

          ProfileFormatter.Append(stringBuilder, data.Name);
          ProfileFormatter.Append(stringBuilder, data.Count, formatProvider);
          stringBuilder.Append(' ');
          ProfileFormatter.Append(stringBuilder, data.Sum * format.Scale, formatProvider);
          stringBuilder.Append(' ');
          ProfileFormatter.Append(stringBuilder, data.Minimum * format.Scale, formatProvider);
          stringBuilder.Append(' ');
          ProfileFormatter.Append(stringBuilder, data.Average * format.Scale, formatProvider);
          stringBuilder.Append(' ');
          ProfileFormatter.Append(stringBuilder, data.Maximum * format.Scale, formatProvider);
          stringBuilder.Append(' ');
          stringBuilder.Append(format.Description);
          stringBuilder.AppendLine();
        }
        return stringBuilder.ToString();
      }
    }
  }
}
