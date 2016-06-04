// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Input;
using DigitalRune.Windows;
using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.QuickLaunch
{
    /// <summary>
    /// Represents an item (search result) in the Quick Launch box.
    /// </summary>
    public class QuickLaunchItem : ObservableObject
    {
        /// <summary>
        /// An instance of the <see cref="QuickLaunchItem"/> that can be used at
        /// design-time.
        /// </summary>
        internal static QuickLaunchItem DesignInstance
        {
            get
            {
                return new QuickLaunchItem
                {
                    Icon = MultiColorGlyphs.Cut,
                    Title = "Edit → Cut",
                    KeyGesture = ApplicationCommands.Cut.InputGestures[0] as KeyGesture,
                    Description = "Remove selected item and copy it to the clipboard",
                    Command = ApplicationCommands.Cut,
                    CommandParameter = null,
                };
            }
        }


        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        public object Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }
        private object _icon;


        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private string _title;


        /// <summary>
        /// Gets or sets the key gesture.
        /// </summary>
        /// <value>The key gesture.</value>
        public KeyGesture KeyGesture
        {
            get { return _keyGesture; }
            set { SetProperty(ref _keyGesture, value); }
        }
        private KeyGesture _keyGesture;


        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }
        private string _description;


        /// <summary>
        /// Gets or sets the command that is executed when the item is selected.
        /// </summary>
        /// <value>The command that is executed when the item is selected.</value>
        public ICommand Command
        {
            get { return _command; }
            set { SetProperty(ref _command, value); }
        }
        private ICommand _command;


        /// <summary>
        /// Gets or sets the command parameter.
        /// </summary>
        /// <value>The command parameter.</value>
        public object CommandParameter
        {
            get { return _commandParameter; }
            set { SetProperty(ref _commandParameter, value); }
        }
        private object _commandParameter;


        /// <summary>
        /// Gets or sets user-defined data.
        /// </summary>
        /// <value>The user-defined data.</value>
        public object Tag
        {
            get { return _tag; }
            set { SetProperty(ref _tag, value); }
        }
        private object _tag;
    }
}
