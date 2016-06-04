// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using NLog.Targets;


namespace DigitalRune.Editor.Output
{
    /// <summary>
    /// Provides an in-app logging target for NLog.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/How-to-write-a-Target"/>
    [Target("NLogTarget")]
    internal sealed class NLogTarget : TargetWithLayout, INLogTarget
    {
        const int MaxNumberOfMessages = 100000; // Optional: Read limit from settings.
        private readonly Queue<string> _log = new Queue<string>();


        /// <summary>
        /// Writes logging event to the log target.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (_log.Count > MaxNumberOfMessages)
                _log.Dequeue();

            string message = Layout.Render(logEvent);
            _log.Enqueue(message);

            // Raise MessageWritten event.
            MessageWritten?.Invoke(this, new NLogMessageEventArgs(message));
        }


        #region ----- INLogTarget -----

        /// <inheritdoc/>
        public event EventHandler<NLogMessageEventArgs> MessageWritten;


        /// <inheritdoc/>
        public void GetLog(StringBuilder buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            foreach (var message in _log)
                buffer.AppendLine(message);
        }
        #endregion
    }
}
