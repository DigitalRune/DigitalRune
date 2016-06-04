#region ----- Copyright -----
/*
  The class in this file is based on the WorkStealingScheduler from the ParallelTasks library (see 
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


namespace DigitalRune.Threading
{
  /// <summary>
  /// A task scheduler that supports "work stealing" to balance tasks across multiple worker 
  /// threads.
  /// </summary>
  public class WorkStealingScheduler : ITaskScheduler
  {
    private readonly Queue<Task> _tasks;

    
    internal List<Worker> Workers { get; private set; }


    /// <summary>
    /// Creates a new instance of the <see cref="WorkStealingScheduler"/> class.
    /// </summary>
    /// <remarks>
    /// By default, the <see cref="WorkStealingScheduler"/> creates one thread per processor (CPU
    /// core) on Windows, and 3 threads on the Xbox 360 (on the hardware threads 3, 4 and 5).
    /// </remarks>
    public WorkStealingScheduler()
#if XBOX
      : this(3)
#elif WP7
      : this(1)
#else
      : this(Environment.ProcessorCount)
#endif
    {
    }


    /// <summary>
    /// Creates a new instance of the <see cref="WorkStealingScheduler"/> class.
    /// </summary>
    /// <param name="numberOfThreads">The number of threads to create.</param>
    public WorkStealingScheduler(int numberOfThreads)
    {
      _tasks = new Queue<Task>();
      Workers = new List<Worker>(numberOfThreads);
      for (int i = 0; i < numberOfThreads; i++)
        Workers.Add(new Worker(this, i));

      for (int i = 0; i < numberOfThreads; i++)
        Workers[i].Start();
    }


    internal bool TryGetTask(ref Task task)
    {
      if (_tasks.Count > 0)
      {
        lock (_tasks)
        {
          if (_tasks.Count > 0)
          {
            task = _tasks.Dequeue();
            return true;
          }
        }
      }

      return false;
    }


    /// <summary>
    /// Schedules a task for execution.
    /// </summary>
    /// <param name="task">The task to schedule.</param>
    public void Schedule(Task task)
    {
      int threads = task.Item.Work.Options.MaximumThreads;
      var worker = Worker.CurrentWorker;
      if (!task.Item.Work.Options.QueueFIFO   // Task does not require FIFO order.
          && worker != null)                  // Schedule() is called from a worker thread.
      {
        // We can add the task to the queue of the local worker.
        worker.AddWork(task);
      }
      else
      {
        // Task requires FIFO order - or no local worker is available.
        // Add the task to the global task queue. The task will be executed after all
        // currently existing tasks.
        lock (_tasks)
        {
          _tasks.Enqueue(task);
        }
      }

      if (threads > 1)
        WorkItem.AddReplicable(task);

      int numberOfWorkers = Workers.Count;
      for (int i = 0; i < numberOfWorkers; i++)
      {
        Workers[i].Gate.Set();
      }
    }
  }
}
#endif
