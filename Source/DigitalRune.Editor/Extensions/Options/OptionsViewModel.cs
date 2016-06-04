// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Windows.Framework;
using NLog;


namespace DigitalRune.Editor.Options
{
    /// <summary>
    /// Represents the Options dialog.
    /// </summary>
    internal sealed class OptionsViewModel : OneActiveItemsConductor
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets an instance of the <see cref="OptionsViewModel"/> that can be used at design-time.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="OptionsViewModel"/> that can be used at design-time.
        /// </value>
        internal static OptionsViewModel DesignInstance
        {
            get
            {
                var vm = new OptionsViewModel
                {
                    // Create some sample data.
                    _rootNode = new MergeableNode<OptionsPageViewModel>(null,
                        new MergeableNode<OptionsPageViewModel>(new DesignTimeOptionsPageViewModel("Options #1")),
                        new MergeableNode<OptionsPageViewModel>(new DesignTimeOptionsPageViewModel("Options #2")),
                        new MergeableNode<OptionsPageViewModel>(new DesignTimeOptionsPageViewModel("Options #3")),
                        new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Options Group") { IsExpanded = true },
                            new MergeableNode<OptionsPageViewModel>(new DesignTimeOptionsPageViewModel("Suboptions #1")),
                            new MergeableNode<OptionsPageViewModel>(new DesignTimeOptionsPageViewModel("Suboptions #1"))))
                };

                return vm;
            }
        }


        /// <summary>
        /// Gets the options to show.
        /// </summary>
        /// <value>The options to show.</value>
        public MergeableNodeCollection<OptionsPageViewModel> OptionsNodes
        {
            get { return _rootNode.Children; }
        }
        private MergeableNode<OptionsPageViewModel> _rootNode;


        /// <summary>
        /// Gets or sets the currently selected node.
        /// </summary>
        /// <value>The selected node.</value>
        public MergeableNode<OptionsPageViewModel> SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (SetProperty(ref _selectedNode, value))
                    ActivateNode(value);
            }
        }
        private MergeableNode<OptionsPageViewModel> _selectedNode;


        /// <summary>
        /// Gets the command that is executed when the Options dialog is closed by clicking the OK
        /// button.
        /// </summary>
        /// <value>The OK command.</value>
        public DelegateCommand OkCommand { get; private set; }


        /// <summary>
        /// Gets the command that is executed when the Apply button is clicked.
        /// </summary>
        /// <value>The Apply command.</value>
        public DelegateCommand ApplyCommand { get; private set; }


        /// <summary>
        /// Gets the command that is executed when the Options dialog is closed by clicking the
        /// Cancel button.
        /// </summary>
        /// <value>The Cancel command.</value>
        public DelegateCommand CancelCommand { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        private OptionsViewModel()
        {
            DisplayName = "Options";
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsViewModel"/> class.
        /// </summary>
        /// <param name="optionsService">The options service.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="optionsService"/> is <see langword="null"/>.
        /// </exception>
        public OptionsViewModel(IOptionsService optionsService)
            : this()
        {
            if (optionsService == null)
                throw new ArgumentNullException(nameof(optionsService));

            _rootNode = new MergeableNode<OptionsPageViewModel>(null) { Children = new MergeableNodeCollection<OptionsPageViewModel>() };
            OkCommand = new DelegateCommand(Ok);
            ApplyCommand = new DelegateCommand(Apply);
            CancelCommand = new DelegateCommand(Cancel);

            // Merge all options node collections.
            _rootNode.Children.Clear();
            var merger = new OptionsMergeAlgorithm();
            foreach (var optionsNodes in optionsService.OptionsNodeCollections)
                merger.Merge(_rootNode.Children, optionsNodes);

            SelectedNode = OptionsNodes.FirstOrDefault();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            base.OnActivated(eventArgs);

            if (eventArgs.Opened)
                ActivateNode(SelectedNode);
        }


        private void ActivateNode(MergeableNode<OptionsPageViewModel> node)
        {
            if (node == null || node.Content == ActiveItem)
                return;

            // Find a suitable node to activate. If the node contains an OptionsGroupViewModel
            // then find the first descendant which isn't an OptionsGroupViewModel.
            while (node.Content is OptionsGroupViewModel && node.Children != null && node.Children.Count > 0)
                node = node.Children[0];

            ActivateItem(node.Content);
        }


        private void Ok()
        {
            Logger.Debug("Applying changes and closing Options dialog. (\"OK\" selected.)");
            ApplyChanges();
            Conductor.DeactivateItemAsync(this, true).Forget();
        }


        private void Apply()
        {
            Logger.Debug("Applying changes in Options dialog. (\"Apply\" selected.)");
            ApplyChanges();
        }


        private void ApplyChanges()
        {
            // Call apply for all option pages which have been activated.
            foreach (var item in Items)
                ((OptionsPageViewModel)item).Apply();
        }


        private void Cancel()
        {
            Logger.Debug("Closing Options dialog. (\"Cancel\" selected.)");
            Conductor.DeactivateItemAsync(this, true).Forget();
        }
        #endregion
    }
}
