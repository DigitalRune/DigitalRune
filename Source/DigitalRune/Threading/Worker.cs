#region ----- Copyright -----
/*
  The class in this file is based on the Worker from the ParallelTasks library (see 
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
using System.Threading;
using DigitalRune.Collections;


namespace DigitalRune.Threading
{
  internal sealed class Worker
  {
#if WP7
    // Cannot access Environment.ProcessorCount in phone app. (Security issue).
    private static readonly SynchronizedHashtable<int, Worker> Workers = new SynchronizedHashtable<int, Worker>(1);
#else
    private static readonly SynchronizedHashtable<int, Worker> Workers = new SynchronizedHashtable<int, Worker>(Environment.ProcessorCount);
#endif

#if XBOX
    private static int _affinityIndex;
#endif

    private readonly Thread _thread;
    private readonly WorkStealingQueue<Task> _tasks;
    private readonly WorkStealingScheduler _scheduler;


    public AutoResetEvent Gate { get; private set; }


    public static Worker CurrentWorker
    {
      get
      {
        Worker worker;
        Workers.TryGet(Thread.CurrentThread.ManagedThreadId, out worker);
        return worker;
      }
    }


    public Worker(WorkStealingScheduler scheduler, int index)
    {
      _thread = new Thread(Work)
      {
        Name = "Parallel Worker " + index,
        IsBackground = true
      };
      _tasks = new WorkStealingQueue<Task>();
      _scheduler = scheduler;
      Gate = new AutoResetEvent(false);

      Workers.Add(_thread.ManagedThreadId, this);
    }


    public void Start()
    {
      _thread.Start();
    }


    public void AddWork(Task task)
    {
      _tasks.LocalPush(task);
    }


    // ReSharper disable FunctionNeverReturns
    private void Work()
    {
#if XBOX
      int i = Interlocked.Increment(ref _affinityIndex) - 1;
      int affinity = Parallel.ProcessorAffinity[i % Parallel.ProcessorAffinity.Length];
      Thread.CurrentThread.SetProcessorAffinity(affinity);
#endif

      Task task;
      while (true)
      {
        FindWork(out task);
        task.DoWork();
      }
    }
    // ReSharper restore FunctionNeverReturns


    private void FindWork(out Task task)
    {
      bool foundWork = false;
      task = default(Task);

      do
      {
        // Priority #1: Check local task queue.
        if (_tasks.LocalPop(ref task))
          break;

        // Priority #2: Check global task queue.
        if (_scheduler.TryGetTask(ref task))
          break;

        // Priority #3: Check replicables.
        if (WorkItem.TryGetReplicable(ref task))
        {
          task.DoWork();
          WorkItem.RemoveReplicable(task);
          task = default(Task);
          continue;
        }

        // Priority #4: Check local task queues of other threads.
        int numberOfWorkers = _scheduler.Workers.Count;
        for (int i = 0; i < numberOfWorkers; i++)
        {
          var worker = _scheduler.Workers[i];
          if (worker == this)
            continue;

          if (worker._tasks.TrySteal(ref task, 0))
          {
            foundWork = true;
            break;
          }
        }

        if (!foundWork)
        {
          // Wait until a new task gets scheduled.
          Gate.WaitOne();
        }
      } while (!foundWork);
    }
  }
}
#endif
