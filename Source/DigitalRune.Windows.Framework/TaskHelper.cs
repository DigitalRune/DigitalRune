// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Threading.Tasks;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Provides helper methods for working with parallel tasks.
    /// </summary>
    public static class TaskHelper
    {
        // TODO: Cache reusable Task instances. Task.FromResult(false), Task.FromResult(true), etc.
        // For example, use ConcurrentDictionary<object, Task>.


        /// <summary>
        /// Creates a <see cref="Task"/> that has completed successfully.
        /// </summary>
        /// <returns>The successfully completed task.</returns>
        public static Task Completed()
        {
#if SILVERLIGHT || WINDOWS_PHONE
            return TaskEx.FromResult<object>(null);
#else
            return Task.FromResult<object>(null);
#endif
        }


        /// <summary>
        /// Creates a <see cref="Task{TResult}"/> that has completed successfully with the specified
        /// result.
        /// </summary>
        /// <typeparam name="T">The type of the result returned by the task.</typeparam>
        /// <param name="result">The result to store into the completed task.</param>
        /// <returns>The successfully completed task.</returns>
        public static Task<T> FromResult<T>(T result)
        {
#if SILVERLIGHT || WINDOWS_PHONE
            return TaskEx.FromResult(result);
#else
            return Task.FromResult(result);
#endif
        }


        /// <summary>
        /// Takes an asynchronous task and does nothing. (Used to indicate that no 'await' is
        /// required and suppress warning CS4014.)
        /// </summary>
        /// <param name="task">The task that is not awaited.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "task")]
        public static void Forget(this Task task)
        {
        }
    }
}
