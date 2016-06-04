// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DigitalRune.Editor.Errors;
using DigitalRune.Editor.Text;
using DigitalRune.Windows.Framework;
using Microsoft.Practices.ServiceLocation;
using static System.FormattableString;


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
        /// Gets or sets a value indicating whether to compile effects with XNA.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if compile effects with XNA; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsXnaEffectProcessorEnabled
        {
            get { return _isXnaEffectProcessorEnabled; }
            set
            {
                if (_isXnaEffectProcessorEnabled == value)
                    return;

                _isXnaEffectProcessorEnabled = value;
                UpdateCommands();
            }
        }
        private bool _isXnaEffectProcessorEnabled = true;
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static async Task<Tuple<bool, List<Error>>> BuildXnaAsync(
            IServiceLocator services, TextDocument document, DelegateCommand<Error> goToLocationCommand)
        {
            var xnaContentBuilder = services.GetInstance<XnaContentBuilder>().ThrowIfMissing();
            xnaContentBuilder.Clear();
            xnaContentBuilder.Add(document.Uri.AbsolutePath, Path.GetFileNameWithoutExtension(document.Uri.LocalPath), null, "EffectProcessor");
            var result = await xnaContentBuilder.BuildAsync();

            // TODO: Asynchronously parse errors.
            List<Error> errors = null;
            if (result.Item2?.Count > 0)
            {
                // Use regular expression to parse the MSBuild output lines.
                // Example: "X:\DigitalRune\Samples\DigitalRune.Graphics.Content\DigitalRune\Billboard.fx(101,10): error : (101,10): error X3089: 'x': invalid shader target/usage [C:\Users\MartinG\AppData\Local\Temp\EditorApp\15812\XnaContentBuilder-0000\content.contentproj]"
                var regex = new Regex(@"(?<file>[^\\/]*)\((?<line>\d+),(?<column>\d+)\): (([eE]rror)|([wW]arning)) : (\((?<line2>\d+),(?<column2>\d+)\): )?.*(?<message>(((?<error>error )|(?<warning>warning ))).*?)(:? \[.*\])");
                errors = new List<Error>();
                foreach (var line in result.Item2)
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        var lineText = match.Groups["line"].Value;
                        if (match.Groups["line2"].Success)
                            lineText = match.Groups["line2"].Value;

                        var columnText = match.Groups["column"].Value;
                        if (match.Groups["column2"].Success)
                            columnText = match.Groups["column2"].Value;

                        var message = match.Groups["message"].Value;
                        bool isWarning = match.Groups["warning"].Success;
                        var error = new Error(
                            isWarning ? ErrorType.Warning : ErrorType.Error,
                            $"[XNA] {message}",
                            match.Groups["file"].Value,
                            int.Parse(lineText),
                            int.Parse(columnText));

                        error.UserData = document;
                        error.GoToLocationCommand = goToLocationCommand;
                        errors.Add(error);
                    }
                }
            }

            return Tuple.Create(result.Item1, errors);
        }
        #endregion
    }
}
