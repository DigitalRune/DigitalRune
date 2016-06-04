// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if NETFX_CORE || PORTABLE || USE_TPL
using System;
using System.Collections.Generic;

// Use Task Parallel Library (TPL).
using Tpl = System.Threading.Tasks;

#pragma warning disable 1574


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
  /// <see href="http://digitalrune.github.io/DigitalRune-Documentation/html/79a8677d-9460-4118-b27b-cef353dfbd92.htm">DigitalRune Physics</see>, 
  /// make extensive use of the class <see cref="Parallel"/>. We highly recommend, that if you need 
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
    private static readonly List<Action> CallbackBuffer = new List<Action>();


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
      for (int i = 0; i < CallbackBuffer.Count; i++)
        CallbackBuffer[i]();

      CallbackBuffer.Clear();
    }


    private static void RunActionWithCallback(Action action, Action callback)
    {
      try
      {
        action();
      }
      finally
      {
        lock (CallbackBuffer)
          CallbackBuffer.Add(callback);
      }
    }


    private static T RunFuncWithCallback<T>(Func<T> func, Action callback)
    {
      try
      {
        return func();
      }
      finally
      {
        lock (CallbackBuffer)
          CallbackBuffer.Add(callback);
      }
    }


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
      var tplTask = (completionCallback == null)
                    ? new Tpl.Task(action, Tpl.TaskCreationOptions.AttachedToParent | Tpl.TaskCreationOptions.LongRunning)
                    : new Tpl.Task(() => RunActionWithCallback(action, completionCallback), Tpl.TaskCreationOptions.AttachedToParent | Tpl.TaskCreationOptions.LongRunning);

      tplTask.Start();
      return new Task(tplTask);
    }


    /// <overloads>
    /// <summary>
    /// Creates and starts a parallel task.
    /// </summary>
    /// </overloads>
    /// 
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
      var tplTask = (completionCallback == null)
                    ? new Tpl.Task(action, Tpl.TaskCreationOptions.AttachedToParent)
                    : new Tpl.Task(() => RunActionWithCallback(action, completionCallback), Tpl.TaskCreationOptions.AttachedToParent);

      tplTask.Start();
      return new Task(tplTask);
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
      var tplTask = (completionCallback == null)
                    ? new Tpl.Task<T>(function, Tpl.TaskCreationOptions.AttachedToParent)
                    : new Tpl.Task<T>(() => RunFuncWithCallback(function, completionCallback), Tpl.TaskCreationOptions.AttachedToParent);

      tplTask.Start();
      return new Task<T>(tplTask);
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
#if PORTABLE
        throw Portable.NotImplementedException;
#else
      try
      {
        Tpl.Parallel.Invoke(action1, action2);
      }
      catch (AggregateException exception)
      {
        throw new TaskException(exception);
      }
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
#if PORTABLE
        throw Portable.NotImplementedException;
#else
      try
      {
        Tpl.Parallel.Invoke(actions);
      }
      catch (AggregateException exception)
      {
        throw new TaskException(exception);
      }
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
#if PORTABLE
        throw Portable.NotImplementedException;
#else
      try
      {
        Tpl.Parallel.For(startInclusive, endExclusive, body);
      }
      catch (AggregateException exception)
      {
        throw new TaskException(exception);
      }
#endif
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
      For(startInclusive, endExclusive, body);
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
#if PORTABLE
        throw Portable.NotImplementedException;
#else
      try
      {
        Tpl.Parallel.ForEach(collection, action);
      }
      catch (AggregateException exception)
      {
        throw new TaskException(exception);
      }
#endif
    }
  }
}
#endif
