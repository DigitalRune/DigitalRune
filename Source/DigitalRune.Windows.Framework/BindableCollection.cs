// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
/*
  The "screen conduction" pattern implemented in DigitalRune.Windows.Framework was 
  inspired by the Caliburn.Micro framework (see http://caliburnmicro.codeplex.com/).
*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Represents a dynamic data collection that supports change notifications and automatic UI
    /// thread marshaling.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <remarks>
    /// All <see cref="INotifyCollectionChanged.CollectionChanged"/> events are raised synchronously
    /// on the UI thread.
    /// </remarks>
#if !SILVERLIGHT && !WINDOWS_PHONE
    [Serializable]
#endif
    public class BindableCollection<T> : ObservableCollection<T>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

#if !SILVERLIGHT && !WINDOWS_PHONE
        [field: NonSerialized]
#endif
        // Default value is false. Otherwise, we need to initialize correctly in constructor and
        // on deserialization.
        private bool _suppressNotifications;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether listeners are notified of changes to the
        /// collection.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if listeners are notified; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// Note that enabling <see cref="IsNotifying"/> does not automatically raise any change
        /// notifications. Call <see cref="Refresh"/> if there were any changes to the collection
        /// while <see cref="IsNotifying"/> was disabled.
        /// </remarks>
        public bool IsNotifying
        {
            get { return !_suppressNotifications; }
            set { _suppressNotifications = !value; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="BindableCollection{T}"/> class.
        /// </summary>
        public BindableCollection()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BindableCollection{T}"/> class that
        /// contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public BindableCollection(IEnumerable<T> collection)
        {
            AddRange(collection);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether the <see cref="IsNotifying"/> property should be
        /// serialized.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> to serialize the <see cref="IsNotifying"/> property; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </returns>
        public virtual bool ShouldSerializeIsNotifying()
        {
            return false;
        }

        /// <summary>
        /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event followed by the
        /// <see cref="INotifyCollectionChanged.CollectionChanged"/> event to indicate that all
        /// bindings should be refreshed.
        /// </summary>
        public void Refresh()
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }


        /// <summary>
        /// Raises the <see cref="ObservableCollection{T}.CollectionChanged"/> event with the
        /// provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotifications)
                base.OnCollectionChanged(e);
        }


        /// <summary>
        /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event with the provided
        /// arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!_suppressNotifications)
                base.OnPropertyChanged(e);
        }


        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddRange(IEnumerable<T> items)
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                bool previousSuppressNotifications = _suppressNotifications;
                _suppressNotifications = true;
                try
                {
                    var index = Count;
                    foreach (var item in items)
                    {
                        InsertItemBase(index, item);
                        index++;
                    }
                }
                finally
                {
                    _suppressNotifications = previousSuppressNotifications;
                }

                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }


        /// <summary>
        /// Removes the range.
        /// </summary>
        /// <param name="items">The items.</param>
        public void RemoveRange(IEnumerable<T> items)
        {
            WindowsHelper.InvokeOnUI(() =>
            {
                bool previousSuppressNotifications = _suppressNotifications;
                _suppressNotifications = true;
                try
                {
                    foreach (var item in items)
                    {
                        var index = IndexOf(item);
                        if (index >= 0)
                            RemoveItemBase(index);
                    }
                }
                finally
                {
                    _suppressNotifications = previousSuppressNotifications;
                }

                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }


        /// <summary>
        /// Removes all items from the collection. (Sealed)
        /// </summary>
        /// <remarks>
        /// The method <see cref="ClearItems"/> is marked as sealed to prevent it from being
        /// overridden. The method <see cref="ClearItemsBase"/> can be used instead to override the
        /// behavior.
        /// </remarks>
        protected sealed override void ClearItems()
        {
            WindowsHelper.InvokeOnUI(ClearItemsBase);
        }


        /// <summary>
        /// Removes all items from the collection. (Exposes the base class's 
        /// <see cref="ObservableCollection{T}.ClearItems"/> method. Can be overridden.)
        /// </summary>
        /// <inheritdoc cref="ClearItems"/>
        protected virtual void ClearItemsBase()
        {
            base.ClearItems();
        }


        /// <summary>
        /// Inserts an item into the collection at the specified index. (Sealed)
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which <paramref name="item"/> should be inserted.
        /// </param>
        /// <param name="item">The object to insert.</param>
        /// <remarks>
        /// The method <see cref="InsertItem"/> is marked as sealed to prevent it from being
        /// overridden. The method <see cref="InsertItemBase"/> can be used instead to override the
        /// behavior.
        /// </remarks>
        protected sealed override void InsertItem(int index, T item)
        {
            WindowsHelper.InvokeOnUI(() => InsertItemBase(index, item));
        }


        /// <summary>
        /// Inserts an item into the collection at the specified index. (Exposes the base class's
        /// <see cref="ObservableCollection{T}.InsertItem"/> method. Can be overridden.)
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which <paramref name="item"/> should be inserted.
        /// </param>
        /// <param name="item">The object to insert.</param>
        /// <inheritdoc cref="InsertItem"/>
        protected virtual void InsertItemBase(int index, T item)
        {
            base.InsertItem(index, item);
        }


#if !SILVERLIGHT && !WINDOWS_PHONE
        /// <summary>
        /// Moves the item at the specified index to a new location in the collection. (Sealed)
        /// </summary>
        /// <param name="oldIndex">
        /// The zero-based index specifying the location of the item to be moved.
        /// </param>
        /// <param name="newIndex">
        /// The zero-based index specifying the new location of the item.
        /// </param>
        /// <remarks>
        /// The method <see cref="MoveItem"/> is marked as sealed to prevent it from being
        /// overridden. The method <see cref="MoveItemBase"/> can be used instead to override the
        /// behavior.
        /// </remarks>
        protected sealed override void MoveItem(int oldIndex, int newIndex)
        {
            WindowsHelper.InvokeOnUI(() => MoveItemBase(oldIndex, newIndex));
        }


        /// <summary>
        /// Moves the item at the specified index to a new location in the collection. (Exposes the
        /// base class's <see cref="ObservableCollection{T}.InsertItem"/> method. Can be
        /// overridden.)
        /// </summary>
        /// <param name="oldIndex">
        /// The zero-based index specifying the location of the item to be moved.
        /// </param>
        /// <param name="newIndex">
        /// The zero-based index specifying the new location of the item.
        /// </param>
        /// <inheritdoc cref="MoveItem"/>
        protected virtual void MoveItemBase(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
        }
#endif


        /// <summary>
        /// Removes the item at the specified index of the collection. (Sealed)
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <remarks>
        /// The method <see cref="RemoveItem"/> is marked as sealed to prevent it from being
        /// overridden. The method <see cref="RemoveItemBase"/> can be used instead to override the
        /// behavior.
        /// </remarks>
        protected sealed override void RemoveItem(int index)
        {
            WindowsHelper.InvokeOnUI(() => RemoveItemBase(index));
        }


        /// <summary>
        /// Removes the item at the specified index of the collection. (Exposes the base class's
        /// <see cref="ObservableCollection{T}.RemoveItem"/> method. Can be overridden.)
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <inheritdoc cref="RemoveItem"/>
        protected virtual void RemoveItemBase(int index)
        {
            base.RemoveItem(index);
        }


        /// <summary>
        /// Replaces the element at the specified index. (Sealed)
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        /// <remarks>
        /// The method <see cref="SetItem"/> is marked as sealed to prevent it from being
        /// overridden. The method <see cref="SetItemBase"/> can be used instead to override the
        /// behavior.
        /// </remarks>
        protected sealed override void SetItem(int index, T item)
        {
            WindowsHelper.InvokeOnUI(() => SetItemBase(index, item));
        }


        /// <summary>
        /// Replaces the element at the specified index. (Exposes the base class's
        /// <see cref="ObservableCollection{T}.SetItem"/> method. Can be overridden.)
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        /// <inheritdoc cref="SetItem"/>
        protected virtual void SetItemBase(int index, T item)
        {
            base.SetItem(index, item);
        }
        #endregion
    }
}
