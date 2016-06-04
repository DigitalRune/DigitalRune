// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using DigitalRune.Windows;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Represents toolbar.
    /// </summary>
    public class ToolBarViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the command item.
        /// </summary>
        /// <value>The command item.</value>
        public CommandGroup CommandGroup { get; }


        /// <summary>
        /// Gets the collection of toolbar items.
        /// </summary>
        /// <value>The collection of toolbar items.</value>
        public ToolBarItemViewModelCollection Items { get; } = new ToolBarItemViewModelCollection();


        /// <summary>
        /// Gets or sets the toolbar band.
        /// </summary>
        /// <value>The toolbar band.</value>
        /// <remarks>
        /// See <see cref="ToolBar.Band"/> for more information.
        /// </remarks>
        public int Band
        {
            get { return _band; }
            set { SetProperty(ref _band, value); }
        }
        private int _band;


        /// <summary>
        /// Gets or sets the toolbar band index.
        /// </summary>
        /// <value>The toolbar band index.</value>
        /// <remarks>
        /// See <see cref="ToolBar.BandIndex"/> for more information.
        /// </remarks>
        public int BandIndex
        {
            get { return _bandIndex; }
            set { SetProperty(ref _bandIndex, value); }
        }
        private int _bandIndex;


        /// <summary>
        /// Gets or sets a value indicating whether this toolbar is visible.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this toolbar is visible; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        /// <inheritdoc/>
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (SetProperty(ref _isVisible, value))
                    RaiseActualIsVisibleChanged();
            }
        }
        private bool _isVisible = true;


        /// <summary>
        /// Gets the effective visibility.
        /// </summary>
        /// <value>The effective visibility.</value>
        /// <remarks>
        /// The actual visibility is computed from the local visibility and the visibility of the
        /// <see cref="CommandGroup"/>. This way an extension can hide a toolbar if it is not 
        /// relevant in the current context. A user can hide a toolbar using the "Hide Toolbar"
        /// menu.
        /// </remarks>
        public bool ActualIsVisible
        {
            get
            {
                return IsVisible
                       && CommandGroup.IsVisible
                       && Items != null
                       && Items.Any(item => item.CommandItem.IsVisible);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ToolBarItemViewModel"/> class.
        /// </summary>
        /// <param name="commandGroup">The command group. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commandGroup"/> is <see langword="null"/>.
        /// </exception>
        public ToolBarViewModel(CommandGroup commandGroup)
        {
            if (commandGroup == null)
                throw new ArgumentNullException(nameof(commandGroup));

            CommandGroup = commandGroup;
            CommandGroup.PropertyChanged += OnCommandGroupPropertyChanged;
            Items.CollectionChanged += OnItemsChanged;

            // ActualIsVisible may change if the visibility of an item changes.
            // --> The ToolBarManager monitor the items and calls RaiseActualIsVisibleChanged().
        }


        private void OnCommandGroupPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (string.IsNullOrEmpty(eventArgs.PropertyName) || eventArgs.PropertyName == nameof(CommandGroup.IsVisible))
                RaiseActualIsVisibleChanged();
        }


        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            RaiseActualIsVisibleChanged();
        }


        internal void RaiseActualIsVisibleChanged()
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(ActualIsVisible));
        }
    }
}
