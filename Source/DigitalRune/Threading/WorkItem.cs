#region ----- Copyright -----
/*
  The class in this file is based on the WorkItem from the ParallelTasks library (see 
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DigitalRune.Collections;


namespace DigitalRune.Threading
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
  internal sealed class WorkItem : IRecyclable
  {
    private static readonly Queue<Task> _replicables = new Queue<Task>();
    private static readonly object _replicablesLock = new object();


    internal static bool TryGetReplicable(ref Task task)
    {
      if (_replicables.Count > 0)
      {
        lock (_replicablesLock)
        {
          if (_replicables.Count > 0)
          {
            task = _replicables.Peek();
            return true;
          }
        }
      }

      return false;
    }


    internal static void AddReplicable(Task task)
    {
      lock (_replicablesLock)
      {
        _replicables.Enqueue(task);
      }
    }


    // Removes task, if task is on top of the stack.
    internal static void RemoveReplicable(Task task)
    {
      if (_replicables.Count > 0)
      {
        lock (_replicablesLock)
        {
          if (_replicables.Count > 0)
          {
            Task replicable = _replicables.Peek();
            if (replicable.ID == task.ID && replicable.Item == task.Item)
              _replicables.Dequeue();
          }
        }
      }
    }


    internal static readonly List<WorkItem> AwaitingCallbacks = new List<WorkItem>();


#if WP7
      // Cannot access Environment.ProcessorCount in phone app. (Security issue)
    private static readonly SynchronizedHashtable<int, Stack<Task>> RunningTasks = new SynchronizedHashtable<int, Stack<Task>>(8);
#else
    private static readonly SynchronizedHashtable<int, Stack<Task>> RunningTasks = new SynchronizedHashtable<int, Stack<Task>>(Environment.ProcessorCount * 4);
#endif


    public static Task? CurrentTask
    {
      get
      {
        Stack<Task> tasks;
        if (RunningTasks.TryGet(Thread.CurrentThread.ManagedThreadId, out tasks))
        {
          if (tasks.Count > 0)
            return tasks.Peek();
        }

        return null;
      }
    }


    private static readonly ResourcePool<WorkItem> Pool = new ResourcePool<WorkItem>(
      () => new WorkItem(),   // Create
      null,                   // Initialize
      null);                  // Uninitialize


    private List<Exception> _exceptionBuffer;
    private readonly SynchronizedHashtable<int, Exception[]> _exceptions;
    private readonly ManualResetEvent _resetEvent;
    private IWork _work;
    private volatile int _runCount;
    private volatile int _executing;
    private readonly List<Task> _children;
    private volatile int _waitCount;
    private readonly object _executionLock = new object();
    private Action _callback;


    public int RunCount
    {
      get { return _runCount; }
    }


    public SynchronizedHashtable<int, Exception[]> Exceptions
    {
      get { return _exceptions; }
    }


    public IWork Work
    {
      get { return _work; }
    }


    public Action Callback
    {
      get { return _callback; }
      set { _callback = value; }
    }


    private WorkItem()
    {
      _resetEvent = new ManualResetEvent(false);
      _exceptions = new SynchronizedHashtable<int, Exception[]>(1);
      _children = new List<Task>();
    }


    /// <summary>
    /// Creates an instance of the <see cref="WorkItem"/> class. (This method reuses a previously
    /// recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="WorkItem"/> class.
    /// </returns>
    public static WorkItem Create()
    {
      return Pool.Obtain();
    }


    public Task PrepareStart(IWork work)
    {
      _work = work;
      _resetEvent.Reset();

      var task = new Task(this);
      var currentTask = CurrentTask;
      if (currentTask.HasValue && currentTask.Value.Item == this)
        throw new TaskException("Internal parallelization failure. The work item is already in use.");

      if (!work.Options.DetachFromParent && currentTask.HasValue)
        currentTask.Value.Item.AddChild(task);

      return task;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    public bool DoWork(int expectedID)
    {
      lock (_executionLock)
      {
        if (expectedID < _runCount)
          return true;

        if (_executing == _work.Options.MaximumThreads)
          return false;

        _executing++;
      }

      // Associate the current task with this thread, so that Task.CurrentTask gives the correct result.
      Stack<Task> tasks;
      if (!RunningTasks.TryGet(Thread.CurrentThread.ManagedThreadId, out tasks))
      {
        tasks = new Stack<Task>();
        RunningTasks.Add(Thread.CurrentThread.ManagedThreadId, tasks);
      }

      tasks.Push(new Task(this));

      // Execute the task.
      try
      {
        _work.DoWork();
      }
      catch (Exception exception)
      {
        CatchException(exception);
      }

      tasks.Pop();

      lock (_executionLock)
      {
        _executing--;
        if (_executing == 0)
        {
          // Wait for all children to complete.
          if (_children.Count > 0)
          {
            foreach (var child in _children)
            {
              try
              {
                child.Wait();
              }
              catch (Exception exception)
              {
                CatchException(exception);
              }
            }

            _children.Clear();
          }

          if (_exceptionBuffer != null)
            _exceptions.Add(_runCount, _exceptionBuffer.ToArray());

          _runCount++;

          // Open the reset event, so tasks waiting on this one can continue.
          _resetEvent.Set();

          // Wait for waiting tasks to all exit.
          while (_waitCount > 0)
            Thread.SpinWait(1);   // Use Thread.SpinWait() to allow context switch on 
                                  // CPUs with Hyper-Threading.

          if (Callback == null)
          {
            Recycle();
          }
          else
          {
            // If we have a callback, then queue for execution.
            lock (AwaitingCallbacks)
            {
              AwaitingCallbacks.Add(this);
            }
          }

          return true;
        }

        return false;
      }
    }


    private void CatchException(Exception exception)
    {
      if (_exceptionBuffer == null)
      {
        var newExceptions = new List<Exception>();
        Interlocked.CompareExchange(ref _exceptionBuffer, newExceptions, null);
      }

      lock (_exceptionBuffer)
      {
        var taskException = exception as TaskException;
        if (taskException != null)
        {
          // Add exceptions of child (nested) task.
          foreach (var innerException in taskException.InnerExceptions)
          {
            if (!_exceptionBuffer.Contains(innerException))
              _exceptionBuffer.Add(innerException);
          }
        }
        else
        {
          // Add exception of current task.
          _exceptionBuffer.Add(exception);
        }
      }
    }


    /// <summary>
    /// Recycles this instance of the <see cref="WorkItem"/> class.
    /// </summary>
    public void Recycle()
    {
      // Requeue the WorkItem for reuse, but only if the runCount hasn't reached the maximum value.
      // Don't requeue if an exception has been caught, to stop potential memory leaks.
      if (_runCount < int.MaxValue && _exceptionBuffer == null)
      {
        Debug.Assert(_children.Count == 0, "Child work items should be cleared before recycling the work item.");
        Debug.Assert(Callback == null, "Callback should be reset before recycling the work item.");

        Pool.Recycle(this);
      }
    }


    public void Wait(int id)
    {
      WaitOrExecute(id);

      Exception[] exceptions;
      if (_exceptions.TryGet(id, out exceptions))
        throw new TaskException(exceptions);
    }


    private void WaitOrExecute(int id)
    {
      if (_runCount != id)
        return;

      if (DoWork(id))
        return;

#pragma warning disable 0420

      try
      {
        Interlocked.Increment(ref _waitCount);

        // According to Joe Duffy the cost of a context switch is 4000+ cycles.
        // http://www.bluebytesoftware.com/blog/2006/08/23/PriorityinducedStarvationWhySleep1IsBetterThanSleep0AndTheWindowsBalanceSetManager.aspx

        // Try to avoid unnecessary context switch:
        // --> Spin for 1000 iterations before sending thread to sleep.
        int i = 0;
        while (_runCount == id)
        {
          if (i > 1000)
          {
            _resetEvent.WaitOne();
          }
          else
          {
            // Note: The original implementation used Thread.Sleep(0), which caused 
            // performance hits on the Xbox 360! Thread.SpinWait() is more appropriate
            // for this case and performs better in our own benchmarks.
            Thread.SpinWait(1);
          }

          i++;
        }

        // Different variations of the loop have been benchmarked (on Windows). The 
        // implementation above shows the best performances. Here is a list of other 
        // variations from slowest to fastest:
        //
        //  while (_runCount == id)   // Slowest
        //    Thread.Sleep(0);
        //
        //  while (_runCount == id)   // Slow
        //    Thread.SpinWait(1);
        // 
        //  while (_runCount == id)   // Faster
        //    _resetEvent.WaitOne();
      }
      finally
      {
        Interlocked.Decrement(ref _waitCount);
      }
    }


    public void AddChild(Task item)
    {
      lock (_executionLock)
      {
        _children.Add(item);
      }
    }
  }
}
#endif
