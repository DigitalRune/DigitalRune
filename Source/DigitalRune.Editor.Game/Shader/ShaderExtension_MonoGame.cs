// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DigitalRune.Editor.Errors;
using DigitalRune.Editor.Game;
using DigitalRune.Editor.Text;
using DigitalRune.ServiceLocation;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Shader
{
    partial class ShaderExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether to compile effects with MonoGame.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if compile effects with MonoGame; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsMonoGameEffectProcessorEnabled
        {
            get { return _isMonoGameEffectProcessorEnabled; }
            set
            {
                if (_isMonoGameEffectProcessorEnabled == value)
                    return;

                _isMonoGameEffectProcessorEnabled = value;
                UpdateCommands();
            }
        }
        private bool _isMonoGameEffectProcessorEnabled = true;
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static async Task<Tuple<bool, List<Error>>> BuildMonoGameAsync(
            ServiceContainer services, string applicationName, TextDocument document, DelegateCommand<Error> goToLocationCommand)
        {
            string errorMessage = await Task.Run(() =>
            {
                using (var tempDirectoryHelper = new TempDirectoryHelper(applicationName, "ShaderDocument"))
                {
                    var contentBuilder = new GameContentBuilder(services)
                    {
                        IntermediateFolder = tempDirectoryHelper.TempDirectoryName + "\\obj",
                        OutputFolder = tempDirectoryHelper.TempDirectoryName + "\\bin",
                    };

                    string message;
                    contentBuilder.Build(document.Uri.LocalPath, null, "EffectProcessor", null, out message);
                    return message;
                }
            });

            List<Error> errors = null;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = errorMessage.TrimEnd(' ', '\r', '\n');
                errors = new List<Error>();

                // Use regular expression to parse the MSBuild output lines.
                // Example: "X:\DigitalRune\Samples\DigitalRune.Graphics.Content\DigitalRune\Billboard.fx(22,3) : Unexpected token 'x' found. Expected CloseBracket"
                var regex = new Regex(@"(?<file>[^\\/]*)\((?<line>\d+),(?<column>\d+)\) : (?<message>.*)");

                var match = regex.Match(errorMessage);
                if (match.Success)
                {
                    string lineText = match.Groups["line"].Value;
                    string columnText = match.Groups["column"].Value;
                    string message = match.Groups["message"].Value;
                    bool isWarning = match.Groups["warning"].Success;
                    var error = new Error(
                        isWarning ? ErrorType.Warning : ErrorType.Error,
                        $"[MonoGame] {message}",
                        match.Groups["file"].Value,
                        int.Parse(lineText),
                        int.Parse(columnText));

                    error.UserData = document;
                    error.GoToLocationCommand = goToLocationCommand;
                    errors.Add(error);
                }
                else
                {
                    errors.Add(new Error(ErrorType.Error, errorMessage));
                }
            }

            return Tuple.Create(string.IsNullOrEmpty(errorMessage), errors);
        }
        #endregion
    }
}
