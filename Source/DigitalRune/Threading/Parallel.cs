#region ----- Copyright -----
/*
  The class in this file is based on the class Parallel from the ParallelTasks library (see 
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
using System.Linq;
using System.Threading;


namespace DigitalRune.Threading
{
  /// <summary>
  /// Provides support for parallel execution of tasks.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The namespace <strong>DigitalRune.Threading</strong> and the class <see cref="Parallel"/> 
  /// provides support for concurrency to run multiple tasks in parallel and automatically balance 
  /// work across all available processors. The implementation is a replacement for Microsoft's Task 
  /// Parallel Library (see <see href="http://msdn.microsoft.com/en-us/library/dd537609.aspx">
  /// Task Parallelism (Task Parallel Library)</see>) which is not yet supported by the .NET Compact 
  /// Framework. This class <see cref="Parallel"/> provides a lightweight and cross-platform 
  /// implementation (supported on Windows, Silverlight, Windows Phone 7, and Xbox 360).
  /// </para>
  /// <para>
  /// The API has similarities to Microsoft's Task Parallel Library, but it is not identical. The 
  /// names in the namespace <strong>DigitalRune.Threading</strong> conflict with the types of the 
  /// namespace <strong>System.Threading.Tasks</strong>. This is on purpose as only one solution for 
  /// concurrency should be used in an application. The library has been optimized for the .NET 
  /// Compact Framework: Only the absolute minimum of memory is allocated at runtime.
  /// </para>
  /// <para>
  /// The DigitalRune libraries, such as 
  /// <see href="http://digitalrune.github.io/DigitalRune-Documentation/html/335dc86a-c68d-4d7b-8641-81dd80de5e76.htm">DigitalRune Geometry</see> 
  /// and 
  /// <see href="http://digitalrune.github.io/DigitalRune-Documentation/html/79a8677d-9460-4118-b27b-cef353dfbd92.htm">DigitalRune Physics</see>,   /// make extensive use of the class <see cref="Parallel"/>. We highly recommend, that if you need 
  /// support for multithreading in your application, you should take advantage of this class. 
  /// (Using different solutions for concurrency can reduce performance.)
  /// </para>
  /// <para>
  /// <strong>Tasks:</strong><br/>
  /// A task is an asynchronous operation which is started, for example, by calling 
  /// <see cref="Start(System.Action)"/>. This method returns a handle of type <see cref="Task"/>. 
  /// This handle can be used to query the status of the asynchronous operation (see 
  /// <see cref="Task.IsComplete"/>). The method <see cref="Task.Wait"/> can be called to wait until 
  /// the operation has completed.
  /// </para>
  /// <para>
  /// <strong>Futures (Task&lt;T&gt;):</strong><br/>
  /// A future is an asynchronous operation that returns a value. A future is created, for example,
  /// by calling <see cref="Start{T}(System.Func{T})"/> and specifying a function that computes a 
  /// value. The method returns a handle of type <see cref="Task{T}"/>, which is similar to 
  /// <see cref="Task"/>. The result of a future can be queried by calling 
  /// <see cref="Task{T}.GetResult"/>. Note that <see cref="Task{T}.GetResult"/> can only be called
  /// once - the handle becomes invalid after the first call!
  /// </para>
  /// <para>
  /// <strong>Background Tasks:</strong><br/>
  /// Long running operations which may block (i.e. wait for I/O operation to finish) should be 
  /// scheduled as background tasks. Background tasks are created by using the method 
  /// <see cref="StartBackground(System.Action)"/> (or one of its overloads). Background tasks will 
  /// not be scheduled using the <see cref="Scheduler"/> (see below). Instead the class 
  /// <see cref="Parallel"/> manages an additional pool of threads that are used for background 
  /// tasks. The processor affinity of these threads is not set automatically. The background tasks 
  /// will usually run on the same hardware thread where the background thread was created first or 
  /// run last. The processor affinity can be set manually from within the task by calling 
  /// <see href="http://msdn.microsoft.com/en-us/library/system.threading.thread.setprocessoraffinity.aspx">Thread.SetProcessorAffinity</see>.
  /// </para>
  /// <para>
  /// <strong>Exception Handling:</strong><br/>
  /// The tasks executed asynchronously can raise exceptions. The exceptions are stored internally 
  /// and a <see cref="TaskException"/> containing these exceptions is thrown when 
  /// <see cref="Task.Wait"/> is called.
  /// </para>
  /// <para>
  /// <strong>Completion Callbacks:</strong><br/>
  /// It is possible to specify a completion callbacks when starting a new tasks. For example, see 
  /// method <see cref="Start(Action, Action)"/>. The completion callbacks are executed after the 
  /// corresponding tasks have completed. Completion callbacks are executed regardless of whether 
  /// tasks have completed successfully or have thrown an exception.
  /// </para>
  /// <para>
  /// However, the callbacks are not executed immediately! 
  /// Instead, the method <see cref="RunCallbacks"/> needs to be called manually - usually on the 
  /// main thread - to invoke the callbacks. 
  /// </para>
  /// <para>
  /// <strong>Task Scheduling:</strong><br/>
  /// The number of threads used for parallelization is determined by the task scheduler (see 
  /// <see cref="Scheduler"/>). The task scheduler creates a number of threads and distributes the 
  /// tasks among these worker threads. The default task scheduler is a 
  /// <see cref="WorkStealingScheduler"/> that creates one thread per CPU core on Windows and 3 
  /// threads on Xbox 360 (on the hardware threads 3, 4, and 5). The number of worker threads can be
  /// specified in the constructor of the <see cref="WorkStealingScheduler"/>.
  /// </para>
  /// <para>
  /// The property <see cref="Scheduler"/> can be changed at runtime. The default task scheduler can
  /// be replaced with another task scheduler (e.g. with a <see cref="WorkStealingScheduler"/> that 
  /// uses a different number of tasks, or with a custom <see cref="ITaskScheduler"/>). Replacing a
  /// task scheduler will affect all future tasks that have not yet been scheduled. However, it is
  /// highly recommended to use the default scheduler or specify the scheduler only once at the 
  /// startup of the application.
  /// </para>
  /// <para>
  /// <strong>Processor Affinity:</strong><br/>
  /// In the .NET Compact Framework for Xbox 360 the processor affinity determines the processors on
  /// which a thread runs. Setting the processor affinity in Windows has no effect.
  /// </para>
  /// <para>
  /// The processor affinity is defined as an array using the property 
  /// <see cref="ProcessorAffinity"/>. Each entry in the array specifies the hardware thread that 
  /// the corresponding worker thread will use. The default value is <c>{ 3, 4, 5, 1 }</c>. The 
  /// default task scheduler reads this array and assigns the worker threads to the specified 
  /// hardware threads. (See also 
  /// <see href="http://msdn.microsoft.com/en-us/library/system.threading.thread.setprocessoraffinity.aspx">Thread.SetProcessorAffinity</see> 
  /// in the MSDN Library to find out more.)
  /// </para>
  /// <para>
  /// <strong>Important:</strong> The processor affinity needs to be set before any parallel tasks
  /// are created or before a new <see cref="WorkStealingScheduler"/> is created. Changing the 
  /// processor affinity afterwards has no effect.
  /// </para>
  /// </remarks>
  /// 
  /// <example>
  /// The following example demonstrates how to configure <see cref="Parallel"/> to schedule tasks
  /// only on the hardware threads 3 and 4 of the Xbox 360.
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Configure the class Parallel to use the hardware threads 3 and 4 on the Xbox 360.
  /// // (Note: Setting the processor affinity has no effect on Windows.)
  /// Parallel.ProcessorAffinity = new[] { 3, 4 };
  /// 
  /// // Create task scheduler that uses 2 worker threads.
  /// Parallel.Scheduler = new WorkStealingScheduler(2);
  /// 
  /// // Note: Above code is usually run at the start of an application. It is not recommended to 
  /// // change the processor affinity or the task scheduler at runtime of the application.
  /// ]]>
  /// </code>
  /// <para>
  /// The following example demonstrates how a task is started.
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Start a method call on another thread:
  /// // DoSomeWork can either be an Action delegate, or an object which implements IWork.
  /// Task task = Parallel.Start(DoSomeWork);
  /// 
  /// // Do something else on this thread for a while.
  /// DoSomethingElse();
  /// 
  /// // Wait for the task to complete. This ensures that after this call returns, the task has 
  /// //finished.
  /// task.Wait();
  /// ]]>
  /// </code>
  /// <para>
  /// The following example demonstrates how task can be used to compute values in parallel and 
  /// return the result when needed.
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Task<T> is similar to Task, but you can retrieve a result from it.
  /// Task<double> piTask = Parallel.Start(CalculatePi);
  /// 
  /// // Do something else for a while.
  /// DoSomethingElse();
  /// 
  /// // Retrieve the result. The caller will block until the task has completed. 
  /// // GetResult() can only be called once!
  /// double pi = piTask.GetResult();
  /// ]]>
  /// </code>
  /// <para>
  /// The following example demonstrates how to run a long task in the background to avoid that the 
  /// current thread is blocked.
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Begin loading some files.
  /// Parallel.StartBackground(LoadFiles);
  /// ]]>
  /// </code>
  /// <para>
  /// The following demonstrates how a for-loop can be executed in parallel.
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Sequential loop:
  /// for (int i = 0; i < count; i++)
  /// {
  ///   DoWork(i);
  /// }
  /// 
  /// // Same loop, but each iteration may happen in parallel on multiple threads.
  /// Parallel.For(0, count, i =>
  /// {
  ///   DoWork(i);
  /// });
  /// ]]>
  /// </code>
  /// <para>
  /// The following demonstrates how a foreach-loop can be executed in parallel.
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Sequential loop:
  /// foreach (var item in list)
  /// {
  ///   DoWork(item);
  /// }
  /// 
  /// // Same loop, but each iteration may happen in parallel on multiple threads.
  /// Parallel.ForEach(list, item =>
  /// {
  ///   DoWork(item);
  /// });
  /// ]]>
  /// </code>
  /// </example>
  public static class Parallel
  {
#if !UNITY
    private static readonly ResourcePool<List<Task>> TaskListPool = new ResourcePool<List<Task>>(
      () => new List<Task>(),   // Create
      null,                     // Initialize
      list => list.Clear());    // Uninitialize

    private static readonly List<WorkItem> CallbackBuffer = new List<WorkItem>();
#endif

    /// <summary>
    /// Executes all task callbacks on a single thread.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is possible to specify a completion callbacks when starting a new tasks. For example, see
    /// method <see cref="Start(Action, Action)"/>. The completion callbacks are executed when the
    /// corresponding tasks have completed. However, the callbacks are not executed immediately!
    /// Instead, the method <see cref="RunCallbacks"/> needs to be called manually to invoke the 
    /// callbacks. 
    /// </para>
    /// <para>
    /// This method is not re-entrant. It is suggested to call the method only on the main thread.
    /// </para>
    /// </remarks>
    public static void RunCallbacks()
    {
#if !UNITY
      if (WorkItem.AwaitingCallbacks.Count > 0)
      {
        lock (WorkItem.AwaitingCallbacks)
        {
          foreach (var callback in WorkItem.AwaitingCallbacks)
            CallbackBuffer.Add(callback);

          WorkItem.AwaitingCallbacks.Clear();
        }
      }

      int numberOfCallbacks = CallbackBuffer.Count;
      for (int i = 0; i < numberOfCallbacks; i++)
      {
        var workItem = CallbackBuffer[i];
        workItem.Callback();
        workItem.Callback = null;
        workItem.Recycle();
      }

      CallbackBuffer.Clear();
#else
      throw new NotSupportedException("Unity builds do not yet support multithreading.");
#endif
    }


    /// <summary>
    /// Gets or sets the processor affinity of the worker threads.
    /// </summary>
    /// <value>
    /// The processor affinity of the worker threads. The default value is <c>{ 3, 4, 5, 1 }</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// In the .NET Compact Framework for Xbox 360 the processor affinity determines the processors 
    /// on which a thread runs. 
    /// </para>
    /// <para>
    /// <strong>Note:</strong> The processor affinity is only relevant in the .NET Compact Framework 
    /// for Xbox 360. Setting the processor affinity has no effect in Windows!
    /// </para>
    /// <para>
    /// <strong>Important:</strong> The processor affinity needs to be set before any parallel tasks
    /// are created. Changing the processor affinity afterwards has no effect.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The specified array is empty or contains invalid values.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public static int[] ProcessorAffinity
    {
      get { return _processorAffinity; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (value.Length < 1)
          throw new ArgumentException("The Parallel.ProcessorAffinity must contain at least one value.", "value");

        if (value.Any(id => id < 0))
          throw new ArgumentException("The processor affinity must not be negative.", "value");

#if XBOX
        if (value.Any(id => id == 0 || id == 2))
          throw new ArgumentException("The hardware threads 0 and 2 are reserved and should not be used on Xbox 360.", "value");

        if (value.Any(id => id > 5))
          throw new ArgumentException("Invalid value. The Xbox 360 has max. 6 hardware threads.", "value");
#endif

        _processorAffinity = value;
      }
    }
    private static int[] _processorAffinity = { 3, 4, 5, 1 };


    /// <summary>
    /// Gets or sets the task scheduler.
    /// </summary>
    /// <value>
    /// The task scheduler. The default value is a <see cref="WorkStealingScheduler"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public static ITaskScheduler Scheduler
    {
      get
      {
        if (_scheduler == null)
        {
          ITaskScheduler newScheduler = new WorkStealingScheduler();
          Interlocked.CompareExchange(ref _scheduler, newScheduler, null);
        }

        return _scheduler;
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        Interlocked.Exchange(ref _scheduler, value);
      }
    }
    private static ITaskScheduler _scheduler;


    /// <overloads>
    /// <summary>
    /// Starts a task in a secondary worker thread. Intended for long running, blocking work such as
    /// I/O.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Starts a task in a secondary worker thread. Intended for long running, blocking work such as
    /// I/O.
    /// </summary>
    /// <param name="work">The work to execute.</param>
    /// <returns>A task which represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="work"/> is <see langword="null"/>.
    /// </exception>
    public static Task StartBackground(IWork work)
    {
      return StartBackground(work, null);
    }


    /// <summary>
    /// Starts a task in a secondary worker thread. Intended for long running, blocking work such as
    /// I/O.
    /// </summary>
    /// <param name="work">The work to execute.</param>
    /// <param name="completionCallback">
    /// A method which will be called in <see cref="RunCallbacks"/> once this task has completed.
    /// </param>
    /// <returns>A task which represents the asynchronous operation.</returns>
    /// <remarks>
    /// <strong>Important:</strong> The completion callback is not executed automatically. Instead, 
    /// the callback is only executed when <see cref="RunCallbacks"/> is called. See 
    /// <see cref="RunCallbacks"/> for additional information.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="work"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Invalid number of maximum threads set in <see cref="IWork.Options"/>.
    /// </exception>
    public static Task StartBackground(IWork work, Action completionCallback)
    {
#if !UNITY      
      if (work == null)
        throw new ArgumentNullException("work");

      if (work.Options.MaximumThreads < 1)
        throw new ArgumentException("work.Options.MaximumThreads cannot be less than one.");

      var workItem = WorkItem.Create();
      workItem.Callback = completionCallback;
      var task = workItem.PrepareStart(work);
      BackgroundWorker.StartWork(task);
      return task;
#else
      throw new NotSupportedException("Unity builds do not yet support multithreading.");
#endif
    }


    /// <summary>
    /// Starts a task in a secondary worker thread. Intended for long running, blocking work such as
    /// I/O.
    /// </summary>
    /// <param name="action">The work to execute.</param>
    /// <returns>A task which represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static Task StartBackground(Action action)
    {
      return StartBackground(action, null);
    }


    /// <summary>
    /// Starts a task in a secondary worker thread. Intended for long running, blocking, work
    /// such as I/O.
    /// </summary>
    /// <param name="action">The work to execute.</param>
    /// <param name="completionCallback">
    /// A method which will be called in <see cref="RunCallbacks"/> once this task has completed.
    /// </param>
    /// <returns>A task which represents the asynchronous operation.</returns>
    /// <remarks>
    /// <strong>Important:</strong> The completion callback is not executed automatically. Instead, 
    /// the callback is only executed when <see cref="RunCallbacks"/> is called. See 
    /// <see cref="RunCallbacks"/> for additional information.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static Task StartBackground(Action action, Action completionCallback)
    {
      if (action == null)
        throw new ArgumentNullException("action");

      var work = DelegateWork.Create();
      work.Action = action;
      work.Options = WorkOptions.Default;
      return StartBackground(work, completionCallback);
    }


    /// <overloads>
    /// <summary>
    /// Creates and starts a task to execute the given work.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates and starts a task to execute the given work.
    /// </summary>
    /// <param name="work">The work to execute in parallel.</param>
    /// <returns>A task which represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="work"/> is <see langword="null"/>.
    /// </exception>
    public static Task Start(IWork work)
    {
      return Start(work, null);
    }


    /// <summary>
    /// Creates and starts a task to execute the given work.
    /// </summary>
    /// <param name="work">The work to execute in parallel.</param>
    /// <param name="completionCallback">
    /// A method which will be called in <see cref="RunCallbacks"/> once this task has completed.
    /// </param>
    /// <returns>A task which represents the asynchronous operation.</returns>
    /// <remarks>
    /// <strong>Important:</strong> The completion callback is not executed automatically. Instead, 
    /// the callback is only executed when <see cref="RunCallbacks"/> is called. See 
    /// <see cref="RunCallbacks"/> for additional information.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Invalid number of maximum threads set in <see cref="IWork.Options"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="work"/> is <see langword="null"/>.
    /// </exception>
    public static Task Start(IWork work, Action completionCallback)
    {
#if !UNITY
      if (work == null)
        throw new ArgumentNullException("work");

      if (work.Options.MaximumThreads < 1)
        throw new ArgumentException("work.Options.MaximumThreads cannot be less than one.");

      var workItem = WorkItem.Create();
      workItem.Callback = completionCallback;
      var task = workItem.PrepareStart(work);
      Scheduler.Schedule(task);
      return task;
#else
      throw new NotSupportedException("Unity builds do not yet support multithreading.");
#endif
    }


    /// <summary>
    /// Creates and starts a task to execute the given work.
    /// </summary>
    /// <param name="action">The work to execute in parallel.</param>
    /// <returns>A task which represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static Task Start(Action action)
    {
      return Start(action, null);
    }


    /// <summary>
    /// Creates and starts a task to execute the given work.
    /// </summary>
    /// <param name="action">The work to execute in parallel.</param>
    /// <param name="completionCallback">
    /// A method which will be called in <see cref="RunCallbacks"/> once this task has completed.
    /// </param>
    /// <returns>A task which represents the asynchronous operation.</returns>
    /// <remarks>
    /// <strong>Important:</strong> The completion callback is not executed automatically. Instead, 
    /// the callback is only executed when <see cref="RunCallbacks"/> is called. See 
    /// <see cref="RunCallbacks"/> for additional information.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static Task Start(Action action, Action completionCallback)
    {
      return Start(action, WorkOptions.Default, completionCallback);
    }


    /// <summary>
    /// Creates and starts a task to execute the given work.
    /// </summary>
    /// <param name="action">The work to execute in parallel.</param>
    /// <param name="options">The work options to use with this action.</param>
    /// <returns>A task which represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static Task Start(Action action, WorkOptions options)
    {
      return Start(action, options, null);
    }


    /// <summary>
    /// Creates and starts a task to execute the given work.
    /// </summary>
    /// <param name="action">The work to execute in parallel.</param>
    /// <param name="options">The work options to use with this action.</param>
    /// <param name="completionCallback">
    /// A method which will be called in <see cref="RunCallbacks"/> once this task has completed.
    /// </param>
    /// <returns>A task which represents the asynchronous operation.</returns>
    /// <remarks>
    /// <strong>Important:</strong> The completion callback is not executed automatically. Instead, 
    /// the callback is only executed when <see cref="RunCallbacks"/> is called. See 
    /// <see cref="RunCallbacks"/> for additional information.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Invalid number of maximum threads set in <see cref="IWork.Options"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static Task Start(Action action, WorkOptions options, Action completionCallback)
    {
#if !UNITY
      if (options.MaximumThreads < 1)
        throw new ArgumentException("options.MaximumThreads cannot be less than 1.", "options");

      var work = DelegateWork.Create();
      work.Action = action;
      work.Options = options;
      return Start(work, completionCallback);
#else
      throw new NotSupportedException("Unity builds do not yet support multithreading.");
#endif
    }


    /// <summary>
    /// Creates and starts a task which executes the given function and stores the result for later 
    /// retrieval.
    /// </summary>
    /// <typeparam name="T">The type of result.</typeparam>
    /// <param name="function">The function to execute in parallel.</param>
    /// <returns>A <see cref="Task{T}"/> which stores the result of the function.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="function"/> is <see langword="null"/>.
    /// </exception>
    public static Task<T> Start<T>(Func<T> function)
    {
      return Start(function, null);
    }


    /// <summary>
    /// Creates and starts a task which executes the given function and stores the result for later 
    /// retrieval.
    /// </summary>
    /// <typeparam name="T">The type of result the function returns.</typeparam>
    /// <param name="function">The function to execute in parallel.</param>
    /// <param name="completionCallback">
    /// A method which will be called in <see cref="RunCallbacks"/> once this task has completed.
    /// </param>
    /// <returns>A <see cref="Task{T}"/> which stores the result of the function.</returns>
    /// <remarks>
    /// <strong>Important:</strong> The completion callback is not executed automatically. Instead, 
    /// the callback is only executed when <see cref="RunCallbacks"/> is called. See 
    /// <see cref="RunCallbacks"/> for additional information.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="function"/> is <see langword="null"/>.
    /// </exception>
    public static Task<T> Start<T>(Func<T> function, Action completionCallback)
    {
      return Start(function, WorkOptions.Default, completionCallback);
    }


    /// <summary>
    /// Creates an starts a task which executes the given function and stores the result for later 
    /// retrieval.
    /// </summary>
    /// <typeparam name="T">The type of result the function returns.</typeparam>
    /// <param name="function">The function to execute in parallel.</param>
    /// <param name="options">The work options to use with this action.</param>
    /// <returns>A <see cref="Task{T}"/> which stores the result of the function.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="function"/> is <see langword="null"/>.
    /// </exception>
    public static Task<T> Start<T>(Func<T> function, WorkOptions options)
    {
      return Start(function, options, null);
    }


    /// <summary>
    /// Creates and starts a task which executes the given function and stores the result for later 
    /// retrieval.
    /// </summary>
    /// <typeparam name="T">The type of result the function returns.</typeparam>
    /// <param name="function">The function to execute in parallel.</param>
    /// <param name="options">The work options to use with this action.</param>
    /// <param name="completionCallback">
    /// A method which will be called in <see cref="RunCallbacks"/> once this task has completed.
    /// </param>
    /// <returns>A <see cref="Task{T}"/> which stores the result of the function.</returns>
    /// <remarks>
    /// <strong>Important:</strong> The completion callback is not executed automatically. Instead, 
    /// the callback is only executed when <see cref="RunCallbacks"/> is called. See 
    /// <see cref="RunCallbacks"/> for additional information.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="function"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Invalid number of maximum threads set in <see cref="IWork.Options"/>.
    /// </exception>
    public static Task<T> Start<T>(Func<T> function, WorkOptions options, Action completionCallback)
    {
#if !UNITY
      if (options.MaximumThreads < 1)
        throw new ArgumentException("options.MaximumThreads cannot be less than 1.", "options");

      var work = FutureWork<T>.Create();
      work.Function = function;
      work.Options = options;
      var task = Start(work, completionCallback);
      return new Task<T>(task, work);
#else
      throw new NotSupportedException("Unity builds do not yet support multithreading.");
#endif
    }


    /// <summary>
    /// Executes the given work items potentially in parallel with each other.
    /// This method will block until all work is completed.
    /// </summary>
    /// <param name="work0">Work to execute.</param>
    /// <param name="work1">Work to execute.</param>
    /// <exception cref="ArgumentNullException">
    /// Either <paramref name="work0"/> or <paramref name="work1"/> is <see langword="null"/>.
    /// </exception>
    public static void Do(IWork work0, IWork work1)
    {
#if !UNITY
      if (work0 == null)
        throw new ArgumentNullException("work0");
      if (work1 == null)
        throw new ArgumentNullException("work1");

      Task task = Start(work1);
      work0.DoWork();
      task.Wait();
#else
      throw new NotSupportedException("Unity builds do not yet support multithreading.");
#endif
    }


    /// <summary>
    /// Executes the given work items potentially in parallel with each other.
    /// This method will block until all work is completed.
    /// </summary>
    /// <param name="work">The work to execute.</param>
    /// <exception cref="ArgumentNullException">
    /// One of the parameters is <see langword="null"/>.
    /// </exception>
    public static void Do(params IWork[] work)
    {
#if !UNITY
      if (work == null)
        throw new ArgumentNullException("work");

      List<Task> tasks = TaskListPool.Obtain();

      int numberOfTasks = work.Length;
      for (int i = 0; i < numberOfTasks; i++)
        tasks.Add(Start(work[i]));

      for (int i = 0; i < numberOfTasks; i++)
        tasks[i].Wait();

      TaskListPool.Recycle(tasks);
#else
      throw new NotSupportedException("Unity builds do not yet support multithreading.");
#endif
    }


    /// <overloads>
    /// <summary>
    /// Executes the given work items potentially in parallel with each other.
    /// This method will block until all work is completed.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Executes the given work items potentially in parallel with each other.
    /// This method will block until all work is completed.
    /// </summary>
    /// <param name="action1">The first piece of work to execute.</param>
    /// <param name="action2">The second piece of work to execute.</param>
    /// <exception cref="ArgumentNullException">
    /// Either <paramref name="action1"/> or <paramref name="action2"/> is <see langword="null"/>.
    /// </exception>
    public static void Do(Action action1, Action action2)
    {
#if !UNITY
      if (action1 == null)
        throw new ArgumentNullException("action1");
      if (action2 == null)
        throw new ArgumentNullException("action2");

      var work = DelegateWork.Create();
      work.Action = action2;
      work.Options = WorkOptions.Default;
      var task = Start(work);
      action1();
      task.Wait();
#else
      throw new NotSupportedException("Unity builds do not yet support multithreading.");
#endif
    }


    /// <summary>
    /// Executes the given work items potentially in parallel with each other.
    /// This method will block until all work is completed.
    /// </summary>
    /// <param name="actions">The work to execute.</param>
    /// <exception cref="ArgumentNullException">
    /// One of the parameters is <see langword="null"/>.
    /// </exception>
    public static void Do(params Action[] actions)
    {
#if !UNITY
      if (actions == null)
        throw new ArgumentNullException("actions");

      List<Task> tasks = TaskListPool.Obtain();

      int numberOfTasks = actions.Length;
      for (int i = 0; i < numberOfTasks; i++)
      {
        var work = DelegateWork.Create();
        work.Action = actions[i];
        work.Options = WorkOptions.Default;
        tasks.Add(Start(work));
      }

      for (int i = 0; i < numberOfTasks; i++)
        tasks[i].Wait();

      TaskListPool.Recycle(tasks);
#else
      throw new NotSupportedException("Unity builds do not yet support multithreading.");
#endif
    }


    /// <overloads>
    /// <summary>
    /// Executes a for loop where each iteration can potentially occur in parallel with others.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Executes a for loop where each iteration can potentially occur in parallel with others.
    /// </summary>
    /// <param name="startInclusive">The index (inclusive) at which to start iterating.</param>
    /// <param name="endExclusive">The index (exclusive) at which to end iterating.</param>
    /// <param name="body">
    /// The method to execute at each iteration. The current index is supplied as the parameter.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="body"/> is <see langword="null"/>.
    /// </exception>
    public static void For(int startInclusive, int endExclusive, Action<int> body)
    {
      For(startInclusive, endExclusive, body, 1);
    }


    /// <summary>
    /// Executes a for loop where each iteration can potentially occur in parallel with others. 
    /// </summary>
    /// <param name="startInclusive">The index (inclusive) at which to start iterating.</param>
    /// <param name="endExclusive">The index (exclusive) at which to end iterating.</param>
    /// <param name="body">
    /// The method to execute at each iteration. The current index is supplied as the parameter.
    /// </param>
    /// <param name="stride">The number of iterations that each processor takes at a time.</param>
    public static void For(int startInclusive, int endExclusive, Action<int> body, int stride)
    {
#if !UNITY
      var work = ForLoopWork.Create();
      work.Prepare(body, startInclusive, endExclusive, stride);
      var task = Start(work);
      task.Wait();
      work.Recycle();
#else
      throw new NotSupportedException("Unity builds do not yet support multithreading.");
#endif
    }


    /// <summary>
    /// Executes a for-each loop where each iteration can potentially occur in parallel with others.
    /// </summary>
    /// <typeparam name="T">The type of item to iterate over.</typeparam>
    /// <param name="collection">The enumerable data source.</param>
    /// <param name="action">
    /// The method to execute at each iteration. The item to process is supplied as the parameter.
    /// </param>
    /// <remarks>
    /// The parallel foreach-loop has a few disadvantages: Enumerating the sequence in parallel 
    /// requires locking. In addition, creating an <see cref="IEnumerator{T}"/> object allocates
    /// memory on the managed heap. It is therefore recommended to use the parallel for-loop instead
    /// of the parallel for-each loop where possible.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Either <paramref name="collection"/> or <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static void ForEach<T>(IEnumerable<T> collection, Action<T> action)
    {
#if !UNITY
      if (collection == null)
        throw new ArgumentNullException("collection");

      var enumerator = collection.GetEnumerator();
      try
      {
        var work = ForEachLoopWork<T>.Create();
        work.Prepare(action, enumerator);
        var task = Start(work);
        task.Wait();
        work.Recycle();
      }
      finally
      {
        enumerator.Dispose();
      }
#else
      throw new NotSupportedException("Unity builds do not yet support multithreading.");
#endif
    }
  }
}
#endif
