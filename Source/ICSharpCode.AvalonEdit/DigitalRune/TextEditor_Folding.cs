using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Utils;


namespace ICSharpCode.AvalonEdit
{
    partial class TextEditor
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private DispatcherTimer _foldingTimer;
        private bool _foldingValid;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the folding manager.
        /// </summary>
        /// <value>
        /// The folding manager. (The folding manager is <see langword="null"/> if folding is
        /// disabled. See <see cref="EnableFolding"/>.)
        /// </value>
        public FoldingManager FoldingManager { get; private set; }


        /// <summary>
        /// Gets or sets the folding strategy.
        /// </summary>
        /// <value>The folding strategy.</value>
        public FoldingStrategy FoldingStrategy
        {
            get { return _foldingStrategy; }
            set
            {
                if (_foldingStrategy == value)
                    return;

                _foldingStrategy = value;
                _foldingValid = false;
            }
        }
        private FoldingStrategy _foldingStrategy;


        /// <summary>
        /// Gets or sets the interval at which the foldings are updated.
        /// </summary>
        /// <value>The interval at which the foldings are updated.</value>
        /// <remarks>
        /// If a <see cref="FoldingStrategy"/> is set, the foldings are automatically updated
        /// periodically. This interval defines how often foldings are updated.
        /// </remarks>
        public TimeSpan FoldingUpdateInterval
        {
            get { return _foldingTimer.Interval; }
            set { _foldingTimer.Interval = value; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="EnableFolding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableFoldingProperty = DependencyProperty.Register(
          "EnableFolding",
          typeof(bool),
          typeof(TextEditor),
          new FrameworkPropertyMetadata(Boxes.False, OnEnableFoldingChanged));

        /// <summary>
        /// Gets or sets a value indicating whether folding ("outlining") is enabled.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if folding is enabled; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether folding (\"outlining\") is enabled.")]
        [Category("Behavior")]
        public bool EnableFolding
        {
            get { return (bool)GetValue(EnableFoldingProperty); }
            set { SetValue(EnableFoldingProperty, Boxes.Box(value)); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnEnableFoldingChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var textEditor = (TextEditor)dependencyObject;
            textEditor.UpdateFoldingManager();
        }


        /// <summary>
        /// Installs/uninstalls the folding manager. (Also needs to be called when the document 
        /// changes.)
        /// </summary>
        private void UpdateFoldingManager()
        {
            // Notes: 
            // Folding manager cannot be installed when the document is null.
            // When the document changes we need to recreate the folding manager.
            if (FoldingManager != null)
                UninstallFoldingManager();

            if (EnableFolding && Document != null)
                InstallFoldingManager();
        }


        private void InstallFoldingManager()
        {
            // Initialize Folding Manager and add fold margin to text editor.
            FoldingManager = FoldingManager.Install(TextArea);

            // Folding must be manually updated, e.g. every 2 seconds.
            _foldingTimer = new DispatcherTimer();
            _foldingTimer.Interval = TimeSpan.FromSeconds(2);
            _foldingTimer.Tick += OnFoldingTimerTick;
            _foldingTimer.Start();
        }


        private void UninstallFoldingManager()
        {
            _foldingTimer.Stop();
            _foldingTimer.Tick -= OnFoldingTimerTick;
            _foldingTimer = null;

            FoldingManager.Uninstall(FoldingManager);
            FoldingManager = null;
        }


        private void OnFoldingTimerTick(object sender, EventArgs eventArgs)
        {
            UpdateFoldings();
        }


        private void InvalidateFolding()
        {
            _foldingValid = false;
        }


        private void CanToggleFold(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            // We can execute fold commands only if we have set a folding strategy.
            eventArgs.CanExecute = (FoldingManager != null && _foldingStrategy != null);
        }


        /// <summary>
        /// Collapses all foldings.
        /// </summary>
        public void FoldAllFoldings()
        {
            if (FoldingManager == null)
                return;

            foreach (var foldingSection in FoldingManager.AllFoldings)
                foldingSection.IsFolded = true;
        }


        /// <summary>
        /// Expands all foldings.
        /// </summary>
        public void UnfoldAllFoldings()
        {
            if (FoldingManager == null)
                return;

            foreach (var foldingSection in FoldingManager.AllFoldings)
                foldingSection.IsFolded = false;
        }


        /// <summary>
        /// Toggles all foldings.
        /// </summary>
        public void ToggleAllFoldings()
        {
            if (FoldingManager == null)
                return;

            // Fold all foldings if all foldings are currently unfolded.
            // Unfold all folding if at least one is currently folded.
            bool fold = FoldingManager.AllFoldings.All(folding => !folding.IsFolded);

            // Now, fold or unfold all.
            foreach (var foldingSection in FoldingManager.AllFoldings)
                foldingSection.IsFolded = fold;
        }


        /// <summary>
        /// Toggles the folding at the caret position.
        /// </summary>
        public void ToggleCurrentFolding()
        {
            if (FoldingManager == null)
                return;

            // Get next folding on the current line.
            var folding = FoldingManager.GetNextFolding(CaretOffset);
            if (folding == null || Document.GetLocation(folding.StartOffset).Line != Document.GetLocation(CaretOffset).Line)
            {
                // No folding after caret on the same line. --> Search for innermost folding that contains
                // the caret.
                folding = FoldingManager.GetFoldingsContaining(CaretOffset).LastOrDefault();
            }

            // Toggle folding if a folding was found.
            if (folding != null)
                folding.IsFolded = !folding.IsFolded;
        }


        /// <summary>
        /// Updates the foldings using the current <see cref="FoldingStrategy"/>.
        /// </summary>
        /// <remarks>
        /// If a <see cref="FoldingStrategy"/> is set, this method is automatically called periodically 
        /// (see <see cref="FoldingUpdateInterval"/>). This method can also be called manually to 
        /// update the foldings.
        /// </remarks>
        public void UpdateFoldings()
        {
            if (FoldingManager == null)
                return;

            Debug.Assert(EnableFolding, "Folding manager should be null if folding is disabled.");

            if (_foldingValid)
                return;

            if (FoldingStrategy != null)
                FoldingStrategy.UpdateFoldings(FoldingManager, Document);
            else if (FoldingManager.AllFoldings.Any())
                FoldingManager.Clear();

            _foldingValid = true;
        }


        /// <summary>
        /// Saves the state of the foldings in the current text editor.
        /// </summary>
        /// <returns>An object representing the state of the foldings.</returns>
        public object SaveFoldings()
        {
            return FoldingManager?.AllFoldings
                                  .Select(folding => new NewFolding
                                  {
                                      StartOffset = folding.StartOffset,
                                      EndOffset = folding.EndOffset,
                                      Name = folding.Title,
                                      DefaultClosed = folding.IsFolded
                                  })
                                  .ToList();
        }


        /// <summary>
        /// Restores the state of the foldings in the current text editor.
        /// </summary>
        /// <param name="foldings">The object representing the state of the foldings.</param>
        public void RestoreFoldings(object foldings)
        {
            var list = foldings as IEnumerable<NewFolding>;
            if (list == null)
                return;

            EnableFolding = true;
            FoldingManager.Clear();
            FoldingManager.UpdateFoldings(list, -1);
        }
        #endregion
    }
}
