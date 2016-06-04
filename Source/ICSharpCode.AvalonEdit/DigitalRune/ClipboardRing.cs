using System.Collections.Generic;
using System.Diagnostics;


namespace ICSharpCode.AvalonEdit
{
    /// <summary>
    /// Stores the most recent text entries of the clipboard.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The clipboard ring does not automatically monitor the clipboard. Text needs to be added
    /// manually to the clipboard ring by calling <see cref="Add"/>.
    /// </para>
    /// <para>
    /// <strong>Thread-Safety:</strong>
    /// Accessing the <see cref="ClipboardRing"/> is thread safe.
    /// </para>
    /// </remarks>
    internal static class ClipboardRing
    {
        private const int Capacity = 20;
        private static readonly object _syncRoot = new object();
        private static readonly List<string> _entries = new List<string>(Capacity);


        /// <summary>
        /// Gets a value indicating whether the clipboard ring is empty.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the clipboard ring is empty; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public static bool IsEmpty
        {
            get
            {
                lock (_syncRoot)
                {
                    return _entries.Count == 0;
                }
            }
        }


        /// <summary>
        /// Adds a text as the most recent entry into the clipboard ring.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void Add(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            lock (_syncRoot)
            {
                int index = _entries.IndexOf(text);
                if (index >= 0)
                {
                    _entries.RemoveAt(index);
                }
                else
                {
                    while (_entries.Count > Capacity - 1)
                        _entries.RemoveAt(_entries.Count - 1);
                }

                Debug.Assert(_entries.Count < Capacity, "Sanity check.");
                _entries.Insert(0, text);
            }
        }


        /// <summary>
        /// Gets the entries in the clipboard ring.
        /// </summary>
        /// <value>The entries in the clipboard ring.</value>
        public static IReadOnlyCollection<string> GetEntries()
        {
            lock (_syncRoot)
            {
                return _entries.ToArray();
            }
        }
    }
}
