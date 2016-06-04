// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Errors;
using DigitalRune.Editor.Status;
using DigitalRune.Editor.Text;
using DigitalRune.ServiceLocation;
using DigitalRune.Windows.Framework;
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
        /// Gets or sets a value indicating whether to compile effects with FXC.EXE.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if compile effects with FXC.EXE; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsFxcEffectProcessorEnabled
        {
            get { return _isFxcEffectProcessorEnabled; }
            set
            {
                if (_isFxcEffectProcessorEnabled == value)
                    return;

                _isFxcEffectProcessorEnabled = value;
                UpdateCommands();
            }
        }
        private bool _isFxcEffectProcessorEnabled = true;
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static async Task<Tuple<bool, List<Error>>> BuildFxcAsync(
            ServiceContainer services, TextDocument document, DelegateCommand<Error> goToLocationCommand)
        {
            var fxc = services.GetInstance<Fxc>().ThrowIfMissing();
            var result = await fxc.BuildAsync(document.Uri.LocalPath);

            // TODO: Asynchronously parse errors.
            List<Error> errors = null;
            if (result.Item2?.Count > 0)
            {
                // Use regular expression to parse the MSBuild output lines.
                // Example: "X:\DigitalRune\Samples\DigitalRune.Graphics.Content\DigitalRune\Billboard.fx(100,3): error X3000: unrecognized identifier 'x'"
                var regex = new Regex(@"(?<file>[^\\/]*)\((?<line>\d+),(?<column>\d+)\): (?<message>(((?<error>error )|(?<warning>warning ))).*)");

                errors = new List<Error>();
                foreach (var line in result.Item2)
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        string lineText = match.Groups["line"].Value;
                        string columnText = match.Groups["column"].Value;
                        string message = match.Groups["message"].Value;
                        bool isWarning = match.Groups["warning"].Success;
                        var error = new Error(
                            isWarning ? ErrorType.Warning : ErrorType.Error,
                            $"[FXC] {message}",
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


        private bool CanShowAssembler()
        {
            var document = _documentService.ActiveDocument as TextDocument;
            return !_isBusy
                   && document != null
                   && document.DocumentType.Name == ShaderDocumentFactory.FxFile;
        }


        private async void ShowAssembler()
        {
            if (_isBusy)
                return;

            var document = _documentService.ActiveDocument as TextDocument;
            if (document == null || document.DocumentType.Name != ShaderDocumentFactory.FxFile)
                return;

            Logger.Debug("Generating assembly code for \"{0}\".", document.GetName());

            bool isSaved = !document.IsUntitled && !document.IsModified;
            if (!isSaved)
                isSaved = _documentService.Save(document);

            if (!isSaved || document.Uri == null)
            {
                Logger.Debug("Document was not saved. Generating assembly code canceled by user.");
                return;
            }

            var status = new StatusViewModel
            {
                Message = "Generating assembly code...",
                ShowProgress = true,
                Progress = double.NaN
            };
            _statusService.Show(status);

            // Disable buttons to avoid reentrance.
            _isBusy = true;
            UpdateCommands();

            try
            {
                var fxc = Editor.Services.GetInstance<Fxc>().ThrowIfMissing();
                string outputFile = await Task.Run(() => fxc.Run(document.Uri.LocalPath));

                status.Progress = 100;
                status.IsCompleted = true;

                if (!string.IsNullOrWhiteSpace(outputFile))
                {
                    status.Message = "Generating assembly code succeeded.";

                    // Show assembler output as a new document with standard syntax-highlighting.
                    var assemblerDocument = (TextDocument)_documentService.New(_shaderDocumentFactory.DocumentTypes.First());
                    assemblerDocument.AvalonEditDocument.Insert(0, File.ReadAllText(outputFile));

                    // The user usually don't wants to save the assembler output. Clearing 
                    // the undo stack resets IsModified. The user won't be prompted to save 
                    // the document.
                    assemblerDocument.AvalonEditDocument.UndoStack.ClearAll();

                    // Set document name.
                    string format = Path.GetFileNameWithoutExtension(document.Uri.LocalPath) + ".asm ({0})";
                    string name;
                    int index = 0;
                    do
                    {
                        name = string.Format(format, index);
                        index++;
                    } while (_documentService.Documents.Any(doc => doc.GetName() == name));

                    assemblerDocument.UntitledName = name;

                    if (File.Exists(outputFile))
                        File.Delete(outputFile);
                }
                else
                {
                    _outputService.Show();

                    status.Message = "Generating assembly code failed.";
                    status.ShowProgress = false;
                }
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Generating assembly code failed.");

                _outputService.WriteLine(Invariant($"Exception: {exception.Message}"));
                _outputService.Show();

                status.Message = "Generating assembly code failed.";
                status.ShowProgress = false;
            }

            _isBusy = false;
            UpdateCommands();

            status.CloseAfterDefaultDurationAsync().Forget();
        }
        #endregion
    }
}
