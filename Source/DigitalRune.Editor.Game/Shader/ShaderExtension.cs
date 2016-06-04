// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Editor.Output;
using DigitalRune.Editor.Text;
using DigitalRune.Windows.Framework;
using DigitalRune.Editor.About;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Errors;
using DigitalRune.Editor.Game.Properties;
using DigitalRune.Editor.Options;
using DigitalRune.Editor.Status;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Themes;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using NLog;
using ILogger = Microsoft.Build.Framework.ILogger;
using static System.FormattableString;
using TextDocument = DigitalRune.Editor.Text.TextDocument;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Provides functions for editing GPU shaders.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public sealed partial class ShaderExtension : EditorExtension
    {
        // TODO: Make options dialog to set paths and select settings (e.g. GPUs for ShaderPerf).


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IDocumentService _documentService;
        private IOutputService _outputService;
        private IStatusService _statusService;
        private IErrorService _errorService;

        private ResourceDictionary _resourceDictionary;
        private EditorExtensionDescription _extensionDescription;
        private ShaderDocumentFactory _shaderDocumentFactory;
        private bool _showCommands = true;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        private MergeableNodeCollection<ICommandItem> _toolBarNodes;
        private MergeableNodeCollection<OptionsPageViewModel> _optionsNodes;
        private bool _isBusy;

        // A dictionary that stores error lists per document.
        private readonly Dictionary<TextDocument, List<Error>> _errorLists = new Dictionary<TextDocument, List<Error>>();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderExtension"/> class.
        /// </summary>
        public ShaderExtension()
        {
            Logger.Debug("Initializing ShaderExtension.");
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            // Register services.
            Editor.Services.Register(typeof(ILogger), null, typeof(BuildLogger));
            Editor.Services.Register(typeof(XnaContentBuilder), null, typeof(XnaContentBuilder));
            Editor.Services.Register(typeof(Fxc), null, typeof(Fxc));
            Editor.Services.Register(typeof(ShaderPerf), null, typeof(ShaderPerf));
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        protected override void OnStartup()
        {
            _documentService = Editor.Services.GetInstance<IDocumentService>().ThrowIfMissing();
            _statusService = Editor.Services.GetInstance<IStatusService>().ThrowIfMissing();
            _outputService = Editor.Services.GetInstance<IOutputService>().ThrowIfMissing();
            _errorService = Editor.Services.GetInstance<IErrorService>().WarnIfMissing();

            AddDataTemplates();
            AddExtensionDescription();
            AddCommands();
            AddMenus();
            AddToolBars();
            AddOptions();
            AddDocumentFactories();

            LoadSettings();

            _documentService.ActiveDocumentChanged += OnActiveDocumentChanged;
            Editor.LayoutChanged += OnEditorLayoutChanged;

            ShowCommands(false);
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            Editor.LayoutChanged -= OnEditorLayoutChanged;
            _documentService.ActiveDocumentChanged -= OnActiveDocumentChanged;

            SaveSettings();

            RemoveDocumentFactories();
            RemoveOptions();
            RemoveToolBars();
            RemoveMenus();
            RemoveCommands();
            RemoveExtensionDescription();
            RemoveDataTemplates();

            // Clear services.
            _documentService = null;
            _statusService = null;
            _outputService = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.Unregister(typeof(ILogger));
            Editor.Services.Unregister(typeof(XnaContentBuilder));
            Editor.Services.Unregister(typeof(Fxc));
            Editor.Services.Unregister(typeof(ShaderPerf));
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor.Game;component/Shader/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
            EditorHelper.RegisterResources(_resourceDictionary);
        }


        private void RemoveDataTemplates()
        {
            EditorHelper.UnregisterResources(_resourceDictionary);
            _resourceDictionary = null;
        }


        private void AddExtensionDescription()
        {
            var aboutService = Editor.Services.GetInstance<IAboutService>();
            if (aboutService != null)
            {
                // Get version of current assembly.
                var version = Assembly.GetAssembly(typeof(ShaderExtension)).GetName().Version;
                _extensionDescription = new EditorExtensionDescription
                {
                    Name = "DigitalRune Shader Extension",
                    Description = "The DigitalRune Shader extension provides functions for editing GPU shaders. It supports HLSL (DirectX 9 and 10) and Cg effects.",
                    Version = Invariant($"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}"),
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/DigitalRune.Editor;component/Resources/Images/TextEditor.ico", UriKind.RelativeOrAbsolute)),
                };
                aboutService.ExtensionDescriptions.Add(_extensionDescription);
            }
        }


        private void RemoveExtensionDescription()
        {
            var aboutService = Editor.Services.GetInstance<IAboutService>();
            if (aboutService != null)
            {
                aboutService.ExtensionDescriptions.Remove(_extensionDescription);
                _extensionDescription = null;
            }
        }


        private void AddCommands()
        {
            // Add items.
            CommandItems.AddRange(new ICommandItem[]
            {
                new DelegateCommandItem("Build", new DelegateCommand(Build, CanBuild))
                {
                    Category = "Build",
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.F6) },
                    Icon = MultiColorGlyphs.Build,
                    Text = "_Build",
                    ToolTip = "Builds the asset.",
                },
                new DelegateCommandItem("Assembler", new DelegateCommand(ShowAssembler, CanShowAssembler))
                {
                    Category = "Build",
                    Text = "Show assembly code",
                    ToolTip = "Output the assembly code of the effect.",
                },
                new DelegateCommandItem("Analyze", new DelegateCommand(Analyze, CanAnalyze))
                {
                    Category = "Build",
                    Text = "Analyze performance",
                    ToolTip = "Estimate the effect's throughput on various GPUs.",
                },
            });
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();
        }


        private void AddMenus()
        {
            var insertBeforeToolsGroup = new[] { new MergePoint(MergeOperation.InsertBefore, "ToolsGroup"), MergePoint.Append };
            _menuNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("BuildGroup", "_Build"), insertBeforeToolsGroup,
                    new MergeableNode<ICommandItem>(CommandItems["Build"]),
                    new MergeableNode<ICommandItem>(CommandItems["Assembler"]),
                    new MergeableNode<ICommandItem>(CommandItems["Analyze"])),
            };

            Editor.MenuNodeCollections.Add(_menuNodes);
        }


        private void RemoveMenus()
        {
            Editor.MenuNodeCollections.Remove(_menuNodes);
            _menuNodes = null;
        }


        private void AddToolBars()
        {
            _toolBarNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("BuildGroup", "Build"),
                    new MergeableNode<ICommandItem>(CommandItems["Build"]),
                    new MergeableNode<ICommandItem>(CommandItems["Assembler"]),
                    new MergeableNode<ICommandItem>(CommandItems["Analyze"])),
            };

            Editor.ToolBarNodeCollections.Add(_toolBarNodes);
        }


        private void RemoveToolBars()
        {
            Editor.ToolBarNodeCollections.Remove(_toolBarNodes);
            _toolBarNodes = null;
        }


        private void AddOptions()
        {
            _optionsNodes = new MergeableNodeCollection<OptionsPageViewModel>
            {
                new MergeableNode<OptionsPageViewModel> { Content = new ShaderOptionsPageViewModel(this) }
            };

            var optionsService = Editor.Services.GetInstance<IOptionsService>().WarnIfMissing();
            optionsService?.OptionsNodeCollections.Add(_optionsNodes);
        }


        private void RemoveOptions()
        {
            if (_optionsNodes == null)
                return;

            var optionsService = Editor.Services.GetInstance<IOptionsService>().WarnIfMissing();
            optionsService?.OptionsNodeCollections.Remove(_optionsNodes);
            _optionsNodes = null;
        }


        private void AddDocumentFactories()
        {
            _shaderDocumentFactory = new ShaderDocumentFactory(Editor);
            _documentService.Factories.Add(_shaderDocumentFactory);
        }


        private void RemoveDocumentFactories()
        {
            _documentService.Factories.Remove(_shaderDocumentFactory);
            _shaderDocumentFactory = null;
        }


        private void LoadSettings()
        {
            IsXnaEffectProcessorEnabled = Settings.Default.IsXnaEffectProcessorEnabled;
            IsMonoGameEffectProcessorEnabled = Settings.Default.IsMonoGameEffectProcessorEnabled;
            IsFxcEffectProcessorEnabled = Settings.Default.IsFxcEffectProcessorEnabled;
        }


        private void SaveSettings()
        {
            Settings.Default.IsXnaEffectProcessorEnabled = IsXnaEffectProcessorEnabled;
            Settings.Default.IsMonoGameEffectProcessorEnabled = IsMonoGameEffectProcessorEnabled;
            Settings.Default.IsFxcEffectProcessorEnabled = IsFxcEffectProcessorEnabled;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            return null;
        }


        private void OnActiveDocumentChanged(object sender, EventArgs eventArgs)
        {
            ShowCommands(_documentService.ActiveDocument?.DocumentType.Factory == _shaderDocumentFactory);
            UpdateCommands();
        }


        private void ShowCommands(bool show)
        {
            if (_showCommands == show)
                return;

            _showCommands = show;
            foreach (var commandItem in CommandItems)
                commandItem.IsVisible = show;
        }


        private void UpdateCommands()
        {
            if (_showCommands)
            {
                ((DelegateCommandItem)CommandItems["Build"]).Command.RaiseCanExecuteChanged();
                ((DelegateCommandItem)CommandItems["Assembler"]).Command.RaiseCanExecuteChanged();
                ((DelegateCommandItem)CommandItems["Analyze"]).Command.RaiseCanExecuteChanged();
            }
        }


        private bool CanBuild()
        {
            var document = _documentService.ActiveDocument as TextDocument;
            return !_isBusy
                   && document != null
                   && document.DocumentType.Name == ShaderDocumentFactory.FxFile
                   && (IsFxcEffectProcessorEnabled || IsMonoGameEffectProcessorEnabled || IsXnaEffectProcessorEnabled);
        }


        private async void Build()
        {
            // Abort if a build is already in progress.
            if (_isBusy)
                return;

            var document = _documentService.ActiveDocument as TextDocument;
            if (document == null || document.DocumentType.Name != ShaderDocumentFactory.FxFile)
                return;

            Logger.Debug("Building effect {0}.", document.GetName());

            // Document must be saved to file system before we can build it.
            bool isSaved = !document.IsUntitled && !document.IsModified;
            if (!isSaved)
                isSaved = _documentService.Save(document);

            // Abort if the save operation was canceled.
            if (!isSaved || document.Uri == null)
            {
                Logger.Debug("Document was not saved. Build canceled by user.");
                return;
            }

            // Delete old Error window items.
            _outputService.Clear();
            RemoveErrors(document);

            // Push a new message to the status bar.
            var status = new StatusViewModel
            {
                Message = "Building effect...",
                ShowProgress = true,
                Progress = double.NaN,
            };
            _statusService.Show(status);

            // Disable buttons to avoid re-entrance.
            _isBusy = true;
            UpdateCommands();

            int successes = 0;
            int failures = 0;
            int exceptions = 0;

            var goToLocationCommand = new DelegateCommand<Error>(GoToLocation);
            if (IsFxcEffectProcessorEnabled)
            {
                try
                {
                    Logger.Info("Building effect using FXC.");
                    _outputService.WriteLine($"----- Build started: {document.GetName()}, Compiler: FXC -----");

                    var result = await BuildFxcAsync(Editor.Services, document, goToLocationCommand);
                    if (result.Item1)
                        successes++;
                    else
                        failures++;

                    AddErrors(document, result.Item2, result.Item1);
                }
                catch (Exception exception)
                {
                    exceptions++;
                    Logger.Warn(exception, "Build using FXC failed.");
                    _outputService.WriteLine(Invariant($"Exception: {exception.Message}"));
                    _outputService.Show();
                }
            }

            if (IsMonoGameEffectProcessorEnabled)
            {
                try
                {
                    Logger.Info("Building effect using MonoGame.");
                    _outputService.WriteLine($"----- Build started: {document.GetName()}, Compiler: MonoGame -----");

                    var result = await BuildMonoGameAsync(Editor.Services, Editor.ApplicationName, document, goToLocationCommand);
                    if (result.Item1)
                        successes++;
                    else
                        failures++;

                    AddErrors(document, result.Item2, result.Item1);
                }
                catch (Exception exception)
                {
                    exceptions++;

                    Logger.Warn(exception, "Build using MonoGame failed.");
                    _outputService.WriteLine(Invariant($"Exception: {exception.Message}"));
                    _outputService.Show();
                }
            }

            if (IsXnaEffectProcessorEnabled)
            {
                try
                {
                    Logger.Info("Building effect using XNA.");
                    _outputService.WriteLine($"----- Build started: {document.GetName()}, Compiler: XNA -----");

                    var result = await BuildXnaAsync(Editor.Services, document, goToLocationCommand);
                    if (result.Item1)
                        successes++;
                    else
                        failures++;

                    AddErrors(document, result.Item2, result.Item1);
                }
                catch (Exception exception)
                {
                    exceptions++;
                    Logger.Warn(exception, "Build using XNA failed.");
                    _outputService.WriteLine(Invariant($"Exception: {exception.Message}"));
                    _outputService.Show();
                }
            }

            _outputService.WriteLine($"========== Build completed: {successes} succeeded, {failures + exceptions} failed ==========");

            if (failures + exceptions == 0)
            {
                status.Progress = 100;
                status.IsCompleted = true;
            }
            else
            {
                status.Message = "Build failed.";
                status.ShowProgress = false;
                _outputService.Show();

                if (failures > 0)
                    _errorService?.Show();
            }

            // Enable buttons.
            _isBusy = false;
            UpdateCommands();

            status.CloseAfterDefaultDurationAsync().Forget();
        }


        private void OnEditorLayoutChanged(object sender, EventArgs eventArgs)
        {
            // If a document was closed, we have to remove its errors.

            if (_errorLists.Count == 0)
                return;

            foreach (var document in _errorLists.Keys.ToArray())
                if (document.IsDisposed)
                    RemoveErrors(document);
        }


        private void AddErrors(TextDocument document, List<Error> newErrors, bool success)
        {
            if (success && (newErrors == null || newErrors.Count == 0))
                return;

            List<Error> errors;
            if (!_errorLists.TryGetValue(document, out errors))
            {
                errors = new List<Error>();
                _errorLists.Add(document, errors);
            }

            bool foundErrorMessage = false;
            foreach (var error in newErrors)
            {
                _errorService?.Errors.Add(error);
                errors.Add(error);
                MarkError(error);

                if (error.ErrorType == ErrorType.Error)
                    foundErrorMessage = true;
            }

            // If we did not find a useful error message we write a generic one.
            if (!success && !foundErrorMessage)
            {
                var error = new Error(
                    ErrorType.Error,
                    "Could not build effect. See Output window for more information.",
                    DocumentHelper.GetName(document));
                _errorService?.Errors.Add(error);
                errors.Add(error);
            }

            _errorLists[document] = errors;
            _errorService?.Show();
        }


        private void RemoveErrors(TextDocument document)
        {
            List<Error> errors;
            if (_errorLists.TryGetValue(document, out errors))
            {
                document.ErrorMarkers.Clear();

                foreach (var error in errors)
                    _errorService?.Errors.Remove(error);

                _errorLists.Remove(document);
            }
        }


        private void MarkError(Error error)
        {
            if (!error.Line.HasValue || !error.Column.HasValue)
                return;

            var document = (TextDocument)error.UserData;
            if (error.Location != Path.GetFileName(document.GetName()))
                return;

            var startOffset = document.AvalonEditDocument.GetOffset(error.Line.Value, error.Column.Value);

            // Skip error if location is already marked in text document.
            if (document.ErrorMarkers.FindSegmentsContaining(startOffset).Count > 0)
                return;

            int endOffset = TextUtilities.FindEndOfIdentifier(document.AvalonEditDocument, startOffset);
            if (endOffset <= startOffset)
                endOffset = startOffset + 1;

            document.ErrorMarkers.Add(new ZigzagMarker
            {
                StartOffset = startOffset,
                EndOffset = endOffset + 1
            });
        }


        private void GoToLocation(Error error)
        {
            var document = (Document)error.UserData;
            if (error.Location == Path.GetFileName(document.GetName()))
                GoToLocation(document, error.Line.Value, error.Column.Value);

            // TODO: Handle jumping to .fxh header files.
        }


        private void GoToLocation(Document document, int line, int column)
        {
            var vm = document.ViewModels.FirstOrDefault();
            if (vm == null)
                return;

            Editor.ActivateItem(vm);

            var textDocumentVM = vm as TextDocumentViewModel;
            if (textDocumentVM == null)
                return;

            textDocumentVM.TextEditor.TextArea.Caret.Line = line;
            textDocumentVM.TextEditor.TextArea.Caret.Column = column;
            textDocumentVM.TextEditor.TextArea.Caret.BringCaretToView();
        }
        #endregion
    }
}
