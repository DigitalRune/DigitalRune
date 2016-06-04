// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Controls;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Options
{
    /// <summary>
    /// Represents a single options page in the Options dialog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All registered options pages are listed in the <see cref="TreeView"/> of the Options dialog
    /// using their <see cref="IDisplayName.DisplayName"/>. So the display name is required for data
    /// binding with the view. The <see cref="IDisplayName.DisplayName"/> is also used as the key
    /// when different options are merged. Therefore, the display name should be unique.
    /// </para>
    /// <para>
    /// When the node in the <see cref="TreeView"/> of the Options dialog is selected, the options
    /// page is activated and shown in a content control, which is 500 x 350 large. (The options
    /// page needs to use a <see cref="ScrollViewer"/> in its view, if it requires more space.)
    /// </para>
    /// <para>
    /// Derived classes must override <see cref="OnApply"/> which is called when the user presses
    /// the Apply button.
    /// </para>
    /// </remarks>
    public abstract class OptionsPageViewModel : Screen, INamedObject
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        string INamedObject.Name { get { return DisplayName; } }


        /// <summary>
        /// Gets or sets a value indicating whether this options group is expanded.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this options group is expanded; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This property is only relevant if the options node has child nodes when shown in the
        /// <see cref="TreeView"/> of the Options dialog.
        /// </remarks>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }
        private bool _isExpanded;


        ///// <summary>
        ///// Gets or sets a value indicating whether this options page is selected.
        ///// </summary>
        ///// <value>
        ///// <see langword="true"/> if this options page is selected; otherwise,
        ///// <see langword="false"/>.
        ///// </value>
        //public bool IsSelected
        //{
        //    // This property is a bit similar to Screen.IsActive, except that it is used to restore
        //    // the tree view selection when the Options dialog is closed and reopened.
        //    get { return _isSelected; }
        //    set { SetProperty(ref _isSelected, value); }
        //}
        //private bool _isSelected;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPageViewModel"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        protected OptionsPageViewModel(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            DisplayName = name;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Applies the changes.
        /// </summary>
        internal void Apply()
        {
            OnApply();
        }


        /// <summary>
        /// Called when the options should be applied.
        /// </summary>
        protected abstract void OnApply();
        #endregion
    }
}
