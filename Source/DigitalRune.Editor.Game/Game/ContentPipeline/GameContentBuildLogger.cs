// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Text;
using DigitalRune.Editor.Output;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Forwards content pipeline build messages to the <see cref="IOutputService"/>.
    /// </summary>
    //[CLSCompliant(false)]
    internal sealed class GameContentBuildLogger : ContentBuildLogger
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly IOutputService _outputService;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="GameContentBuildLogger"/> class.
        /// </summary>
        public GameContentBuildLogger(IOutputService outputService)
        {
            if (outputService == null)
                throw new ArgumentNullException(nameof(outputService));

            _outputService = outputService;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public override void LogMessage(string message, params object[] messageArgs)
        {
            if (messageArgs != null && messageArgs.Length > 0)
                _outputService.WriteLine(string.Format(CultureInfo.InvariantCulture, message, messageArgs));
            else
                _outputService.WriteLine(message);
        }


        /// <inheritdoc/>
        public override void LogImportantMessage(string message, params object[] messageArgs)
        {
            if (messageArgs != null && messageArgs.Length > 0)
                _outputService.WriteLine(string.Format(CultureInfo.InvariantCulture, message, messageArgs));
            else
                _outputService.WriteLine(message);
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
        {
            var warning = new StringBuilder();

            if (!string.IsNullOrEmpty(contentIdentity?.SourceFilename))
            {
                warning.Append(contentIdentity.SourceFilename);
                if (!string.IsNullOrEmpty(contentIdentity.FragmentIdentifier))
                {
                    warning.Append(" (");
                    warning.Append(contentIdentity.FragmentIdentifier);
                    warning.Append(")");
                }

                warning.Append(": ");
            }

            if (messageArgs != null && messageArgs.Length > 0)
                warning.AppendFormat(CultureInfo.InvariantCulture, message, messageArgs);
            else
                warning.Append(message);

            _outputService.WriteLine(warning.ToString());
        }
        #endregion
    }
}
