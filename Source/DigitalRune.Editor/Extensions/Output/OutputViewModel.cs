// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DigitalRune.Editor.Commands;
using DigitalRune.Editor.Printing;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using ICSharpCode.AvalonEdit;
using NLog;
using TextDocument = ICSharpCode.AvalonEdit.Document.TextDocument;


namespace DigitalRune.Editor.Output
{
    /// <summary>
    /// Represents the output window.
    /// </summary>
    internal sealed class OutputViewModel : EditorDockTabItemViewModel, IOutputService
    {
        // Notes: Continuations of asynchronous operations ("async/await") are scheduled with
        // DispatcherPriority.Send. Therefore, it is important to log with the same priority.
        // Otherwise, log messages could be out of order.


        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        internal const string DockIdString = "OutputWindow";
        public const string DefaultView = "Default";
        public const string NLogView = "NLog";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEditorService _editor;
        private readonly INLogTarget _nlogTarget;
        private readonly IWindowService _windowService;
        private readonly Dictionary<string, StringBuilder> _buffers;
        private IDisposable _nlogSubscription;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets an instance of the <see cref="OutputViewModel"/> that can be used at design-time.
        /// </summary>
        /// <value>
        /// an instance of the <see cref="OutputViewModel"/> that can be used at design-time.
        /// </value>
        internal static OutputViewModel DesignInstance
        {
            get
            {
                var instance = new OutputViewModel(null)
                {
                    Output = new TextDocument("Output text.")
                };
                instance.Views.Add("Build");
                instance.Views.Add("Log");
                return instance;
            }
        }


        /// <summary>
        /// Gets the text output.
        /// </summary>
        /// <value>The text output.</value>
        public TextDocument Output { get; private set; }


        /// <summary>
        /// Gets the names of the output views.
        /// </summary>
        /// <value>The names of the output views.</value>
        public ObservableCollection<string> Views { get; }


        /// <summary>
        /// Gets or sets the currently selected view.
        /// </summary>
        /// <value>The currently selected view.</value>
        public string SelectedView
        {
            get { return _selectedView; }
            set
            {
                if (SetProperty(ref _selectedView, value))
                    OnSelectedViewChanged(value);
            }
        }
        private string _selectedView;


        /// <summary>
        /// Gets or sets the text editor.
        /// </summary>
        /// <value>The text editor.</value>
        /// <remarks>
        /// This property is automatically set by the code-behind of the <see cref="OutputView"/>.
        /// </remarks>
        public TextEditor TextEditor { get; internal set; }


        /// <summary>
        /// Gets or sets the context menu of the text editor.
        /// </summary>
        /// <value>The context menu of the text editor.</value>
        public MenuItemViewModelCollection TextContextMenu
        {
            get { return _contextMenu; }
            set { SetProperty(ref _contextMenu, value); }
        }
        private MenuItemViewModelCollection _contextMenu;


        /// <summary>
        /// Gets or sets the "Print Preview" command.
        /// </summary>
        /// <value>The "Print Preview" command.</value>
        public DelegateCommand PrintPreviewCommand { get; private set; }


        /// <summary>
        /// Gets or sets the "Print" command.
        /// </summary>
        /// <value>The "Print" command.</value>
        public DelegateCommand PrintCommand { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputViewModel" /> class.
        /// </summary>
        /// <param name="editor">The editor. Can be <see langword="null"/> at design-time.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> is <see langword="null"/>.
        /// </exception>
        public OutputViewModel(IEditorService editor)
        {
            if (editor == null && !WindowsHelper.IsInDesignMode)
                throw new ArgumentNullException(nameof(editor));

            _editor = editor;
            DisplayName = "Output";
            IsPersistent = true;
            DockId = DockIdString;
            //Icon = MultiColorGlyphs.Output;

            _buffers = new Dictionary<string, StringBuilder>
            {
                { DefaultView, new StringBuilder() }
            };
            Views = new ObservableCollection<string> { DefaultView };
            PrintPreviewCommand = new DelegateCommand(ShowPrintPreview, CanPrint);
            PrintCommand = new DelegateCommand(Print, CanPrint);

            _nlogTarget = editor?.Services.GetInstance<INLogTarget>();
            if (_nlogTarget != null)
            {
                _buffers.Add(NLogView, new StringBuilder());
                Views.Add(NLogView);
            }
            _windowService = editor?.Services.GetInstance<IWindowService>().ThrowIfMissing();

            Output = new TextDocument { UndoStack = { SizeLimit = 0 } };
            SelectedView = DefaultView;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            if (eventArgs.Opened)
            {
                // We cannot do this in the ctor because the output view model is created before
                // all other services are registered. 
                // --> To improve this, let the OutputExtension implement IOutputService.
                // Or do not register an instance in the service container - only the type which is
                // then created by the service container on-demand.
                
                // We can use the context menu of the text service, but then we also have menu items
                // for syntax highlighting and formatting.
                //TextContextMenu = _editor?.Services.GetInstance<ITextService>()?.ContextMenu;

                var commandExtension = _editor.Extensions.OfType<CommandExtension>().ThrowIfMissing().First();
                TextContextMenu = new MenuItemViewModelCollection
                {
                    commandExtension.CommandItems["Cut"].CreateMenuItem(),
                    commandExtension.CommandItems["Copy"].CreateMenuItem(),
                    commandExtension.CommandItems["Paste"].CreateMenuItem(),
                    commandExtension.CommandItems["Delete"].CreateMenuItem(),
                    new CommandSeparator("ClipboardSeparator").CreateMenuItem(),
                    commandExtension.CommandItems["SelectAll"].CreateMenuItem(),
                    new CommandSeparator("SelectSeparator").CreateMenuItem()
                };
            }

            base.OnActivated(eventArgs);
        }


        private bool CanPrint()
        {
            return TextEditor != null;
        }


        private void ShowPrintPreview()
        {
            if (TextEditor == null)
                return;

            Logger.Info(CultureInfo.InvariantCulture, "Showing print preview for Output window (selected view: \"{0}\").", SelectedView);

            // Get the page size from the print dialog.
            PrintDialog printDialog = new PrintDialog();
            var pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

            // Convert document into FixedDocument.
            var fixedDocument = TextEditor.CreateFixedDocument(pageSize, DisplayName);

            var printPreview = new PrintPreviewViewModel { PrintDocument = fixedDocument };

            // Show print preview dialog.
            _windowService.ShowDialog(printPreview);
        }


        private void Print()
        {
            if (TextEditor == null)
                return;

            Logger.Info(CultureInfo.InvariantCulture, "Printing content of Output window (selected view: \"{0}\").", SelectedView);

            // Show print dialog.
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // Get the page size from the print dialog.
                var pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

                // Convert document into FixedDocument.
                var fixedDocument = TextEditor.CreateFixedDocument(pageSize, DisplayName);

                // Print.
                printDialog.PrintDocument(fixedDocument.DocumentPaginator, DisplayName);
            }
        }


        private StringBuilder GetOrCreateBuffer(string view)
        {
            Debug.Assert(!string.IsNullOrEmpty(view));

            StringBuilder buffer;
            if (!_buffers.TryGetValue(view, out buffer))
            {
                buffer = new StringBuilder();
                _buffers.Add(view, buffer);
                Views.Add(view);
            }

            if (_nlogTarget != null && view == NLogView)
            {
                buffer.Clear();
                _nlogTarget.GetLog(buffer);
            }

            return buffer;
        }


        /// <inheritdoc/>
        public void Clear(string view = null)
        {
            WindowsHelper.CheckBeginInvokeOnUI(() => ClearUnchecked(view), DispatcherPriority.Send);
        }


        private void ClearUnchecked(string view = null)
        {
            Logger.Info(CultureInfo.InvariantCulture, "Clearing Output window (selected view: \"{0}\").", SelectedView);

            if (string.IsNullOrEmpty(view))
                view = DefaultView;

            var buffer = GetOrCreateBuffer(view);
            buffer.Clear();

            if (SelectedView == view)
                Output.Remove(0, Output.TextLength);
        }


        /// <inheritdoc/>
        public void WriteLine(string message, string view = null)
        {
            WindowsHelper.CheckBeginInvokeOnUI(() => WriteLineUnchecked(message, view), DispatcherPriority.Send);
        }


        private void WriteLineUnchecked(string message, string view = null)
        {
            if (string.IsNullOrEmpty(view))
                view = DefaultView;

            var buffer = GetOrCreateBuffer(view);
            buffer.AppendLine(message);

            if (SelectedView == view)
            {
                Output.Insert(Output.TextLength, message + "\n");
                TextEditor?.TextArea.Caret.BringCaretToView();
            }
        }


        /// <inheritdoc/>
        public void Show(string view = null)
        {
            WindowsHelper.CheckBeginInvokeOnUI(() => ShowUnchecked(view));
        }


        private void ShowUnchecked(string view)
        {
            Logger.Info(CultureInfo.InvariantCulture, "Showing Output window (selected view: \"{0}\").", SelectedView);

            if (string.IsNullOrEmpty(view))
                view = DefaultView;

            SelectedView = view;

            if (TextEditor != null)
            {
                TextEditor.TextArea.Caret.Offset = TextEditor.Document.TextLength;
                TextEditor.ScrollToEnd();
            }

            _editor.ActivateItem(this);
        }


        private void OnSelectedViewChanged(string value)
        {
            Logger.Debug(CultureInfo.InvariantCulture, "Switching to \"{0}\" in Output window.", SelectedView);

            _nlogSubscription?.Dispose();

            Output.Text = GetOrCreateBuffer(value).ToString();
            if (TextEditor != null)
            {
                TextEditor.TextArea.Caret.Offset = TextEditor.Document.TextLength;
                TextEditor.ScrollToEnd();
            }

            // Listen for NLog messages.
            if (_nlogTarget != null && value == NLogView)
            {
                _nlogSubscription = WeakEventHandler<NLogMessageEventArgs>.Register(
                    _nlogTarget,
                    this,
                    (sender, handler) => sender.MessageWritten += handler,
                    (sender, handler) => sender.MessageWritten -= handler,
                    (listener, sender, eventArgs) => WriteLine(eventArgs.Message, NLogView));
            }
        }
        #endregion
    }
}
