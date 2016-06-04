// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Media;
using DigitalRune.Windows;
using DigitalRune.Windows.Controls;


namespace DigitalRune.Editor.Outlines
{
    /// <summary>
    /// Represents a tree view item in the outline.
    /// </summary>
    public class OutlineItem : ObservableObject
    {
        /// <summary>
        /// Gets the parent item.
        /// </summary>
        /// <value>The parent item.</value>
        public OutlineItem Parent
        {
            get { return _parent; }
            internal set { SetProperty(ref _parent, value); }
        }
        private OutlineItem _parent;


        /// <summary>
        /// Gets or sets the icon (<see cref="ImageSource"/> or <see cref="MultiColorGlyph"/>) that
        /// represents this item.
        /// </summary>
        /// <value>The icon. The default value is <see langword="null"/>.</value>
        /// <inheritdoc/>
        public object Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }
        private object _icon;


        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }
        private string _text;


        /// <summary>
        /// Gets or sets a value indicating whether this item is selected.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this item is selected; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
        private bool _isSelected;


        /// <summary>
        /// Gets or sets a value indicating whether this item is expanded.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this item is expanded; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }
        private bool _isExpanded = true;


        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        /// <value>
        /// The children. The default value is <see langword="null"/>.
        /// </value>
        public OutlineItemCollection Children
        {
            get { return _children; }
            set
            {
                if (_children == value)
                    return;

                if (_children != null)
                    _children.Parent = null;

                _children = value;

                if (_children != null)
                    _children.Parent = this;

                RaisePropertyChanged();
            }
        }
        private OutlineItemCollection _children;


        /// <summary>
        /// Gets or sets the tool tip.
        /// </summary>
        /// <value>The tool tip.</value>
        public string ToolTip
        {
            get { return _toolTip; }
            set { SetProperty(ref _toolTip, value); }
        }
        private string _toolTip;


        /// <summary>
        /// Gets or sets user-defined data.
        /// </summary>
        /// <value>User-defined data.</value>
        public object UserData
        {
            get { return _userData; }
            set { SetProperty(ref _userData, value); }
        }
        private object _userData;
    }
}
