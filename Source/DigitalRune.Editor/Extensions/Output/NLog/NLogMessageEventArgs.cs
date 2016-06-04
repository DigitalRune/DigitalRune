// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Editor.Output
{
    /// <summary>
    /// Provides arguments for the <see cref="INLogTarget.MessageWritten"/> event.
    /// </summary>
    public class NLogMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="NLogMessageEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public NLogMessageEventArgs(string message)
        {
            Message = message;
        }
    }
}
