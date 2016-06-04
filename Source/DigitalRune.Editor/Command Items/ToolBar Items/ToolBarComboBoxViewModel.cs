// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Represents a combo box in a toolbar.
    /// </summary>
    public class ToolBarComboBoxViewModel : ToolBarItemViewModel
    {
        /// <summary>
        /// Gets or sets the width in device-independent pixels.
        /// </summary>
        /// <value>The width in device-independent pixels. The default value is NaN.</value>
        public double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }
        private double _width = double.NaN;


        /// <summary>
        /// Gets or sets the combo box items.
        /// </summary>
        /// <value>The combo box items.</value>
        public IEnumerable Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }
        private IEnumerable _items;


        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (SetProperty(ref _selectedItem, value))
                    OnSelectedItemChanged(EventArgs.Empty);
            }
        }
        private object _selectedItem;


        /// <summary>
        /// Raised when the <see cref="SelectedItem"/> changed.
        /// </summary>
        public event EventHandler<EventArgs> SelectedItemChanged;


        /// <summary>
        /// Initializes a new instance of the <see cref="ToolBarComboBoxViewModel"/> class.
        /// </summary>
        /// <param name="commandItem">The command item. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commandItem"/> is <see langword="null"/>.
        /// </exception>
        public ToolBarComboBoxViewModel(ICommandItem commandItem) 
            : base(commandItem)
        {
        }


        /// <summary>
        /// Raises the <see cref="SelectedItemChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong><br/> When overriding
        /// <see cref="OnSelectedItemChanged"/> in a derived class, be sure to call the base class's
        /// <see cref="OnSelectedItemChanged"/> method so that registered delegates receive the
        /// event.
        /// </remarks>
        protected virtual void OnSelectedItemChanged(EventArgs eventArgs)
        {
            SelectedItemChanged?.Invoke(this, eventArgs);
        }
    }
}
