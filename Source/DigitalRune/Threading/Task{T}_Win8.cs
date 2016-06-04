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
  /// Represents an asynchronous operation that can return a value.
  /// </summary>
  /// <typeparam name="T">The type of result produced by the asynchronous operation.</typeparam>
  /// <remarks>
  /// An asynchronous task that produces a value can be created by using the class 
  /// <see cref="Parallel"/> and calling <see cref="Parallel.Start{T}(System.Func{T})"/> or one of 
  /// its overloads.
  /// </remarks>
  public struct Task<T>
  {
    private readonly Tpl.Task<T> _tplTask;


    /// <summary>
    /// Gets a value which indicates if this task has completed.
    /// </summary>
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


    internal Task(Tpl.Task<T> tplTask)
    {
      _tplTask = tplTask;
    }


    /// <summary>
    /// Gets the result. (Blocks the calling thread until the asynchronous operation has completed 
    /// execution.)
    /// </summary>
    /// <returns>The result of the asynchronous operation.</returns>
    /// <exception cref="TaskException">
    /// The task or a child task has thrown an exception.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The result of the <see cref="Task{T}"/> has already been retrieved. The method 
    /// <see cref="GetResult"/> can only be called once.
    /// </exception>
    public T GetResult()
    {
      if (_tplTask == null)
        throw new InvalidOperationException("The result of a Task<T> can only be retrieved once.");

      try
      {
        return _tplTask.Result;
      }
      catch (AggregateException exception)
      {
        throw new TaskException(exception);
      }
    }
  }
}
#endif
