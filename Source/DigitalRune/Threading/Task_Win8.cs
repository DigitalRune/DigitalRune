// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if NETFX_CORE || PORTABLE || USE_TPL
using System;
using System.Linq;

// Use Task Parallel Library (TPL).
using Tpl = System.Threading.Tasks;


namespace DigitalRune.Threading
{
  /// <summary>
  /// Represents an asynchronous operation.
  /// </summary>
  /// <remarks>
  /// An asynchronous task is created by using the class <see cref="Parallel"/> and calling 
  /// <see cref="Parallel.Start(System.Action)"/> or one of its overloads.
  /// </remarks>
  public struct Task
  {
    private readonly Tpl.Task _tplTask;


    /// <summary>
    /// Gets a value indicating whether the task has completed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the task has completed; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsComplete
    {
      get { return _tplTask == null || _tplTask.IsCompleted; }
    }


    /// <summary>
    /// Gets an array containing any exceptions thrown by this task.
    /// </summary>
    /// <value>
    /// An array containing all exceptions thrown by this task, or <see langword="null"/> if the 
    /// task is still running.
    /// </value>
    public Exception[] Exceptions
    {
      get
      {
        if (_tplTask != null && _tplTask.Exception != null)
          return _tplTask.Exception.InnerExceptions.ToArray();

        return null;
      }
    }


    internal Task(Tpl.Task tplTask)
    {
      _tplTask = tplTask;
    }


    /// <summary>
    /// Waits for the task to complete execution.
    /// </summary>
    /// <exception cref="TaskException">
    /// The task or a child task has thrown an exception.
    /// </exception>
    public void Wait()
    {
      try
      {
        if (_tplTask != null)
          _tplTask.Wait();
      }
      catch (AggregateException exception)
      {
        throw new TaskException(exception);
      }
    }
  }
}
#endif
