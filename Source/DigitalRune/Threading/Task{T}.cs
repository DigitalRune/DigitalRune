#region ----- Copyright -----
/*
  The class in this file is based on the Future from the ParallelTasks library (see 
  http://paralleltasks.codeplex.com/) which is licensed under Ms-PL (see below).


  Microsoft Public License (Ms-PL)

  This license governs use of the accompanying software. If you use the software, you accept this 
  license. If you do not accept the license, do not use the software.

  1. Definitions

  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same 
  meaning here as under U.S. copyright law.

  A "contribution" is the original software, or any additions or changes to the software.

  A "contributor" is any person that distributes its contribution under this license.

  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

  2. Grant of Rights

  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and 
  limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
  copyright license to reproduce its contribution, prepare derivative works of its contribution, and 
  distribute its contribution or any derivative works that you create.

  (B) Patent Grant- Subject to the terms of this license, including the license conditions and 
  limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
  license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or 
  otherwise dispose of its contribution in the software or derivative works of the contribution in 
  the software.

  3. Conditions and Limitations

  (A) No Trademark License- This license does not grant you rights to use any contributors' name, 
  logo, or trademarks.

  (B) If you bring a patent claim against any contributor over patents that you claim are infringed 
  by the software, your patent license from such contributor to the software ends automatically.

  (C) If you distribute any portion of the software, you must retain all copyright, patent, 
  trademark, and attribution notices that are present in the software.

  (D) If you distribute any portion of the software in source code form, you may do so only under 
  this license by including a complete copy of this license with your distribution. If you 
  distribute any portion of the software in compiled or object code form, you may only do so under a 
  license that complies with this license.

  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no 
  express warranties, guarantees or conditions. You may have additional consumer rights under your 
  local laws which this license cannot change. To the extent permitted under your local laws, the 
  contributors exclude the implied warranties of merchantability, fitness for a particular purpose 
  and non-infringement.  
*/
#endregion

#if !NETFX_CORE && !PORTABLE && !USE_TPL
using System;


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
  public struct Task<T> : IEquatable<Task<T>>
  {
    private Task _task;
    private FutureWork<T> _work;
    private readonly int _id;


    /// <summary>
    /// Gets a value which indicates if this task has completed.
    /// </summary>
    public bool IsComplete
    {
      get { return _task.IsComplete; }
    }


    /// <summary>
    /// Gets an array containing any exceptions thrown by this task.
    /// </summary>
    /// <value>
    /// An array containing all exceptions thrown by this task, or <see langword="null"/> if the 
    /// task is still running.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public Exception[] Exceptions
    {
      get { return _task.Exceptions; }
    }


    internal Task(Task task, FutureWork<T> work)
    {
      _task = task;
      _work = work;
      _id = work.ID;
    }


    /// <summary>
    /// Gets the result. (Blocks the calling thread until the asynchronous operation has completed 
    /// execution. This can only be called once!)
    /// </summary>
    /// <returns>The result of the asynchronous operation.</returns>
    /// <exception cref="TaskException">
    /// The task or a child task has thrown an exception.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The result of the <see cref="Task{T}"/> has already been retrieved. The method 
    /// <see cref="GetResult"/> can only be called once.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public T GetResult()
    {
      if (_work == null || _work.ID != _id)
        throw new InvalidOperationException("The result of a Task<T> can only be retrieved once.");

      _task.Wait();
      var result = _work.Result;
      _work.Recycle();
      _work = null;

      return result;
    }


    #region ----- Equality Members -----

    /// <summary>
    /// Determines whether the specified <see cref="Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object" /> is equal to this instance;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is Task<T> && Equals((Task<T>)obj);
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other" />
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Task<T> other)
    {
      return _task == other._task
             && _work == other._work
             && _id == other._id;
    }


    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures
    /// like a hash table.
    /// </returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      unchecked
      {
        int hashCode = _task.GetHashCode();
        hashCode = (hashCode * 397) ^ (_work != null ? _work.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ _id;
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <summary>
    /// Compares two <see cref="Task{T}"/> to determine whether they are the same.
    /// </summary>
    /// <param name="left">The first <see cref="Task{T}"/>.</param>
    /// <param name="right">The second <see cref="Task{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the 
    /// same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Task<T> left, Task<T> right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares two <see cref="Task{T}"/> to determine whether they are different.
    /// </summary>
    /// <param name="left">The first <see cref="Task{T}"/>.</param>
    /// <param name="right">The second <see cref="Task{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are 
    /// different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Task<T> left, Task<T> right)
    {
      return !left.Equals(right);
    }
    #endregion
  }


  internal sealed class FutureWork<T> : IWork, IRecyclable
  {
    // ReSharper disable StaticFieldInGenericType
    private static readonly ResourcePool<FutureWork<T>> Pool = new ResourcePool<FutureWork<T>>(
      () => new FutureWork<T>(),  // Create
      null,                       // Initialize
      null);                      // Uninitialize
    // ReSharper restore StaticFieldInGenericType


    public int ID { get; private set; }
    public WorkOptions Options { get; set; }
    public Func<T> Function { get; set; }
    public T Result { get; set; }


    private FutureWork()
    {
    }


    public static FutureWork<T> Create()
    {
      return Pool.Obtain();
    }


    public void Recycle()
    {
      if (ID < int.MaxValue)
      {
        ID++;
        Function = null;
        Result = default(T);
        Pool.Recycle(this);
      }
    }


    public void DoWork()
    {
      Result = Function();
    }
  }
}
#endif
