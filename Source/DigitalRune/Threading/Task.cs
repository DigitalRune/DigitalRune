#region ----- Copyright -----
/*
  The class in this file is based on the Task from the ParallelTasks library (see 
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
  /// Represents an asynchronous operation.
  /// </summary>
  /// <remarks>
  /// An asynchronous task is created by using the class <see cref="Parallel"/> and calling 
  /// <see cref="Parallel.Start(System.Action)"/> or one of its overloads.
  /// </remarks>
  public struct Task : IEquatable<Task>
  {
    // Since a Task is a struct we cannot prevent users from creating a task using the default 
    // constructor. The following field indicates whether a Task was created for an actual WorkItem.
    private readonly bool _valid;


    internal WorkItem Item { get; private set; }
    internal int ID { get; private set; }


    /// <summary>
    /// Gets a value indicating whether the task has completed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the task has completed; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsComplete
    {
      get { return !_valid || Item.RunCount != ID; }
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
      get
      {
        if (_valid && IsComplete)
        {
          Exception[] exceptions;
          Item.Exceptions.TryGet(ID, out exceptions);
          return exceptions;
        }

        return null;
      }
    }


    internal Task(WorkItem item)
      : this()
    {
      ID = item.RunCount;
      Item = item;
      _valid = true;
    }


    /// <summary>
    /// Waits for the task to complete execution.
    /// </summary>
    /// <exception cref="TaskException">
    /// The task or a child task has thrown an exception.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The method is called from within the same task. A task cannot wait on itself.
    /// </exception>
    public void Wait()
    {
      if (_valid)
      {
        var currentTask = WorkItem.CurrentTask;
        if (currentTask.HasValue && currentTask.Value.Item == Item && currentTask.Value.ID == ID)
          throw new InvalidOperationException("A task cannot wait on itself.");

        Item.Wait(ID);
      }
    }


    internal void DoWork()
    {
      if (_valid)
        Item.DoWork(ID);
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
      return obj is Task && Equals((Task)obj);
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other" />
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Task other)
    {
      return _valid == other._valid
             && Item == other.Item
             && ID == other.ID;
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
      unchecked
      {
        int hashCode = _valid.GetHashCode();
        hashCode = (hashCode * 397) ^ (Item != null ? Item.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ ID;
        return hashCode;
      }
    }


    /// <summary>
    /// Compares two <see cref="Task"/>s to determine whether they are the same.
    /// </summary>
    /// <param name="left">The first <see cref="Task"/>.</param>
    /// <param name="right">The second <see cref="Task"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the 
    /// same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Task left, Task right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares two <see cref="Task"/>s to determine whether they are different.
    /// </summary>
    /// <param name="left">The first <see cref="Task"/>.</param>
    /// <param name="right">The second <see cref="Task"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are 
    /// different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Task left, Task right)
    {
      return !left.Equals(right);
    }
    #endregion
  }
}
#endif
