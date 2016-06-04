// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Windows;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using NLog;


namespace DigitalRune.Editor.QuickLaunch
{
    /// <summary>
    /// Represents the Quick Launch box.
    /// </summary>
    internal class QuickLaunchViewModel : ObservableObject
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly char[] Separators = { '\t', ' ', ',', ';' };

        private readonly IQuickLaunchService _quickLaunchService;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets an instance of the <see cref="QuickLaunchViewModel"/> that can be used at
        /// design-time.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="QuickLaunchViewModel"/> that can be used at design-time.
        /// </value>
        internal static QuickLaunchViewModel DesignInstance
        {
            get
            {
                var vm = new QuickLaunchViewModel();
                ((IList)vm.Results).Add(QuickLaunchItem.DesignInstance);
                ((IList)vm.Results).Add(QuickLaunchItem.DesignInstance);
                vm.ShowResults = true;
                return vm;
            }
        }


        /// <summary>
        /// Gets the editor.
        /// </summary>
        /// <value>The editor.</value>
        public IEditorService Editor { get; }


        /// <summary>
        /// Gets or sets the search query.
        /// </summary>
        /// <value>The search query.</value>
        public string Query
        {
            get { return _query; }
            set
            {
                if (SetProperty(ref _query, value))
                {
                    FindCommand.RaiseCanExecuteChanged();

                    // Update search results.
                    Find();
                }
            }
        }
        private string _query = string.Empty;


        /// <summary>
        /// Gets or sets a value indicating whether the result popup is shown.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the results popup is shown; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool ShowResults
        {
            get { return _showResults; }
            set { SetProperty(ref _showResults, value); }
        }
        private bool _showResults;


        /// <summary>
        /// Gets the search results.
        /// </summary>
        /// <value>The search results.</value>
        public IEnumerable Results
        {
            get { return _results; }
        }
        private readonly ObservableCollection<QuickLaunchItem> _results = new ObservableCollection<QuickLaunchItem>();


        /// <summary>
        /// Gets or sets the currently selected search result.
        /// </summary>
        /// <value>The currently selected search result.</value>
        public QuickLaunchItem SelectedResult
        {
            get { return _selectedResult; }
            set { SetProperty(ref _selectedResult, value); }
        }
        private QuickLaunchItem _selectedResult;


        /// <summary>
        /// Gets the command that starts a search.
        /// </summary>
        public DelegateCommand FindCommand { get; }


        /// <summary>
        /// Gets the command that executes the selected command.
        /// </summary>
        public DelegateCommand ExecuteCommand { get; }


        /// <summary>
        /// Gets the command that is invoked when the user presses Escape.
        /// </summary>
        public DelegateCommand CancelCommand { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        private QuickLaunchViewModel()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="QuickLaunchViewModel"/> class.
        /// </summary>
        /// <param name="editor">The editor view model.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> is <see langword="null"/>.
        /// </exception>
        public QuickLaunchViewModel(IEditorService editor)
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            Editor = editor;
            _quickLaunchService = editor.Services.GetInstance<IQuickLaunchService>().ThrowIfMissing();

            FindCommand = new DelegateCommand(Find, CanFind);
            ExecuteCommand = new DelegateCommand(Execute);
            CancelCommand = new DelegateCommand(Cancel);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private bool CanFind()
        {
            return !string.IsNullOrWhiteSpace(Query);
        }


        private void Find()
        {
            _results.Clear();
            ShowResults = false;

            if (string.IsNullOrWhiteSpace(Query))
                return;

            string[] keywords = Query.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            if (keywords.Length == 0)
                return;

            // RoutedCommands need to be routed to the previously focused element.
            var targetElement = GetTargetElement();

            // Find matching items.
            var matchingItems = _quickLaunchService.Items
                                                   .Where(item => item != null
                                                                  && !string.IsNullOrEmpty(item.Title)
                                                                  && CanExecute(item.Command, targetElement)
                                                                  && Match(item.Title, keywords))
                                                   .OrderBy(item => item.Title);
            _results.AddRange(matchingItems);

            if (_results.Count > 0)
            {
                SelectedResult = _results[0];
                ShowResults = true;
            }
        }


        private IInputElement GetTargetElement()
        {
            var dockControlViewModel = (IDockControl)Editor;
            var activeDockTabItemViewModel = dockControlViewModel.ActiveDockTabItem;
            if (activeDockTabItemViewModel == null)
                return null;

            var dockControl = dockControlViewModel.DockControl;
            var activeDockTabItem = dockControl?.GetView(activeDockTabItemViewModel);
            if (activeDockTabItem == null)
                return null;

            var focusScope = FocusManager.GetFocusScope(activeDockTabItem);
            if (focusScope == null)
                return null;

            return FocusManager.GetFocusedElement(focusScope);
        }


        private static bool Match(string text, string[] keywords)
        {
            foreach (string keyword in keywords)
                if (!Match(text, keyword))
                    return false;

            return true;
        }


        private static bool Match(string text, string keyword)
        {
            return !string.IsNullOrEmpty(text) && text.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }


        private static bool CanExecute(ICommand command, IInputElement targetElement)
        {
            if (command == null)
                return false;

            var routedCommand = command as RoutedCommand;
            if (routedCommand != null)
                return routedCommand.CanExecute(null, targetElement);

            return command.CanExecute(null);
        }


        private void Execute()
        {
            if (SelectedResult == null)
                return;

            var command = SelectedResult.Command;
            var commandParameter = SelectedResult.CommandParameter;
            if (command == null)
                return;

            Logger.Debug(CultureInfo.InvariantCulture, "Executing \"{0}\" from Quick Launch.", SelectedResult.Title);

            // Clear Quick Launch box and close Search Result list. (Needs to be done 
            // before the command is executed. Resetting the Query string after calling
            // Execute() has no effect, if Execute() changes the EditorWindow layout.
            // Not sure why...)
            Query = string.Empty;

            var focusedElement = Keyboard.FocusedElement;
            var targetElement = GetTargetElement();
            var routedCommand = command as RoutedCommand;
            if (routedCommand != null)
                routedCommand.Execute(commandParameter, targetElement);
            else
                command.Execute(commandParameter);

            if (focusedElement == Keyboard.FocusedElement)
                targetElement?.Focus();
        }


        private void Cancel()
        {
            // Pressing the Escape key can have multiple effects.
            //if (ShowResults)
            //{
            //  // Close the results list.
            //  ShowResults = false;
            //}
            //else 
            if (!string.IsNullOrEmpty(Query))
            {
                // Clear the search query.
                Query = string.Empty;
            }
            else
            {
                // Move the focus back to the previous element.
                var targetElement = GetTargetElement();
                if (targetElement != null)
                {
                    targetElement.Focus();
                    ShowResults = false;
                }
            }
        }
        #endregion
    }
}
