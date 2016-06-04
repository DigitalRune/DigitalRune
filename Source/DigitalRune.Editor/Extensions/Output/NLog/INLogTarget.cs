// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Text;


namespace DigitalRune.Editor.Output
{
    /// <summary>
    /// Provides an in-app logging target for NLog.
    /// </summary>
    public interface INLogTarget
    {
        /// <summary>
        /// Occurs when a new message is written to the log.
        /// </summary>
        event EventHandler<NLogMessageEventArgs> MessageWritten;


        /// <summary>
        /// Copies the log into the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <see langword="null"/>.
        /// </exception>
        void GetLog(StringBuilder buffer);
    }
}
