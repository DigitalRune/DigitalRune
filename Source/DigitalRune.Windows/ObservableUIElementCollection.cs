// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Represents an ordered collection of <see cref="UIElement"/>s that notifies listeners about
    /// changes.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public class ObservableUIElementCollection : UIElementCollection, INotifyCollectionChanged, INotifyPropertyChanged
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="UIElement"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <value>The element at <paramref name="index"/>.</value>
        public override UIElement this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                var oldElement = base[index];
                var newElement = value;

                base[index] = value;

                // Raise events.
                OnPropertyChanged(Binding.IndexerName);

                var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newElement, oldElement, index);
                OnCollectionChanged(eventArgs);
            }
        }


        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;


        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableUIElementCollection"/> class.
        /// </summary>
        /// <param name="visualParent">The <see cref="UIElement"/> parent of the collection.</param>
        /// <param name="logicalParent">The logical parent of the elements in the collection.</param>
        public ObservableUIElementCollection(UIElement visualParent, FrameworkElement logicalParent)
            : base(visualParent, logicalParent)
        {
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Removes all elements from a <see cref="UIElementCollection"/>.
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            // Raise events.
            OnPropertyChanged("Count");
            OnPropertyChanged(Binding.IndexerName);

            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(eventArgs);
        }


        /// <summary>
        /// Adds the specified element to the <see cref="UIElementCollection"/>.
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to add.</param>
        /// <returns>The index position of the added element.</returns>
        public override int Add(UIElement element)
        {
            int index = base.Add(element);

            // Raise events.
            OnPropertyChanged("Count");
            OnPropertyChanged(Binding.IndexerName);

            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, element, index);
            OnCollectionChanged(eventArgs);

            return index;
        }


        /// <summary>
        /// Inserts an element into a <see cref="UIElementCollection"/> at the specified index position.
        /// </summary>
        /// <param name="index">The index position where you want to insert the element.</param>
        /// <param name="element">
        /// The element to insert into the <see cref="UIElementCollection"/>.
        /// </param>
        public override void Insert(int index, UIElement element)
        {
            base.Insert(index, element);

            // Raise events.
            OnPropertyChanged("Count");
            OnPropertyChanged(Binding.IndexerName);

            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, element, index);
            OnCollectionChanged(eventArgs);

        }


        /// <summary>
        /// Removes the specified element from a <see cref="UIElementCollection"/>.
        /// </summary>
        /// <param name="element">The element to remove from the collection.</param>
        public override void Remove(UIElement element)
        {
            base.Remove(element);

            // Raise events.
            OnPropertyChanged("Count");
            OnPropertyChanged(Binding.IndexerName);

            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, element);
            OnCollectionChanged(eventArgs);
        }


        /// <summary>
        /// Removes the <see cref="UIElement"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// The index of the <see cref="UIElement"/> that you want to remove.
        /// </param>
        public override void RemoveAt(int index)
        {
            UIElement removedElement = base[index];
            base.RemoveAt(index);

            // Raise events.
            OnPropertyChanged("Count");
            OnPropertyChanged(Binding.IndexerName);

            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedElement, index);
            OnCollectionChanged(eventArgs);
        }


        /// <summary>
        /// Removes a range of elements from a <see cref="UIElementCollection"/>.
        /// </summary>
        /// <param name="index">The index position of the element where removal begins.</param>
        /// <param name="count">The number of elements to remove.</param>
        public override void RemoveRange(int index, int count)
        {
            // Save elements in array for event args.
            var removedElements = new UIElement[count];
            for (int i = 0; i < count; ++i)
                removedElements[i] = base[index + i];

            base.RemoveRange(index, count);

            // Raise events.
            OnPropertyChanged("Count");
            OnPropertyChanged(Binding.IndexerName);

            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedElements, index);
            OnCollectionChanged(eventArgs);
        }


        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="NotifyCollectionChangedEventArgs"/> object that provides the arguments for
        /// the event.
        /// </param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs eventArgs)
        {
            CollectionChanged?.Invoke(this, eventArgs);
        }


        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
