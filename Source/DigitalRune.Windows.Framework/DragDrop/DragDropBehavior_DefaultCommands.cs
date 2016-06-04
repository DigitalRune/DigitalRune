// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace DigitalRune.Windows.Framework
{
    public partial class DragDropBehavior
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly DataFormat DefaultFormat = DataFormats.GetDataFormat("DigitalRune.DragDropBehavior.DefaultFormat");

        private readonly ICommand DefaultDragCommand;
        private readonly ICommand DefaultDropCommand;

        // Usually the DropCommand updates the target and DragCommand updates the source.
        // In a special case, when the source is the same as the target, the DropCommand updates
        // both. In this case the DropCommand sets the _sourceUpdated flag to indicate that the
        // DragCommand should not do anything.
        private bool _sourceUpdated;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="DefaultEffects"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultEffectsProperty = DependencyProperty.Register(
            "DefaultEffects",
            typeof(DragDropEffects),
            typeof(DragDropBehavior),
            new PropertyMetadata(DragDropEffects.Move));

        /// <summary>
        /// Gets or sets the value that defines the allowed actions when a default drag-and-drop 
        /// operation is initiated. (This property is ignored if a <see cref="DragCommand"/> is set.)
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The value that defines the allowed actions when a default drag-and-drop 
        /// operation is initiated. 
        /// The default value is <see cref="DragDropEffects.Move"/>, meaning that the content of 
        /// the source element will be moved to the target element. 
        /// </value>
        [Description("Gets or sets the value that defines the allowed actions when a default drag-and-drop operation is initiated.")]
        [Category(Categories.Default)]
        public DragDropEffects DefaultEffects
        {
            get { return (DragDropEffects)GetValue(DefaultEffectsProperty); }
            set { SetValue(DefaultEffectsProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------


        private bool CanExecuteDrag(DragCommandParameter parameter)
        {
            // Determine the data that is dragged and store it in parameter.Tag.
            // If a ContentControl is dragged, use ContentControl.Content.
            var contentControl = AssociatedObject as ContentControl;
            if (contentControl != null)
            {
                // The dragged element is not within an ItemsControl.
                // --> Select the data-context.
                parameter.Tag = contentControl.Content;
                return true;
            }

            // If an item of an ItemsControl is dragged, use ItemContainer.DataContext.
            var itemsControl = AssociatedObject as ItemsControl;
            if (itemsControl != null)
            {
                // The dragged element is within an ItemsControl.
                // --> Select the clicked item.
                var visual = parameter.HitObject as Visual;
                Debug.Assert(visual != null, "HitObject should always be set.");
                var itemContainer = itemsControl.ContainerFromElement(visual) as FrameworkElement;
                if (itemContainer != null)
                {
                    parameter.Tag = itemContainer.DataContext;
                    return true;
                }
            }

            return false;
        }


        private void ExecuteDrag(DragCommandParameter parameter)
        {
            // The data was determined in CanExecuteDrag and stored in the Tag.
            var data = parameter.Tag;
            if (data == null)
                return;

            // Wrap data in IDataObject.
            var dataObject = SetData(data);

            _sourceUpdated = false;

            // Execute drag-and-drop.
            var effect = DragDrop.DoDragDrop(parameter.AssociatedObject, dataObject, DefaultEffects);

            // Update source if not already handled by DropCommand.
            if ((effect & DragDropEffects.Move) != 0 && !_sourceUpdated)
            {
                // Remove data from source.
                var contentControl = parameter.AssociatedObject as ContentControl;
                var itemsControl = parameter.AssociatedObject as ItemsControl;
                if (contentControl != null)
                {
                    // Source is a ContentControl.
                    // --> Remove content.
                    contentControl.Content = null;
                }
                else if (itemsControl != null)
                {
                    // Source is an ItemsControl.
                    // --> Remove item.
                    RemoveItemFromItemsControl(itemsControl, data);
                }
            }
        }


        private bool CanExecuteDrop(DropCommandParameter parameter)
        {
            // Update effect.
            bool isMoveAllowed = (parameter.DragEventArgs.AllowedEffects & DragDropEffects.Move) != 0;
            bool isCopyAllowed = (parameter.DragEventArgs.AllowedEffects & DragDropEffects.Copy) != 0;
            if (!isMoveAllowed && !isCopyAllowed)
            {
                parameter.DragEventArgs.Effects = DragDropEffects.None;
            }
            else
            {
                if (isMoveAllowed && isCopyAllowed)
                {
                    bool isCtrlPressed = (parameter.DragEventArgs.KeyStates & DragDropKeyStates.ControlKey) != 0;
                    parameter.DragEventArgs.Effects = isCtrlPressed ? DragDropEffects.Copy : DragDropEffects.Move;
                }
            }

            // Extract data.
            object draggedData = GetData(parameter.DragEventArgs.Data);
            parameter.Data = draggedData;

            if (draggedData == null)
                return false;

            var itemsControl = AssociatedObject as ItemsControl;
            if (itemsControl != null)
            {
                // The target is an ItemsControl.
                // --> Only permit drop if the ItemsSource is compatible with the data.
                if (!CanInsertDataInItemsControl(draggedData, itemsControl))
                    return false;
            }

            return true;
        }


        private void ExecuteDrop(DropCommandParameter parameter)
        {
            // parameter.DragSource is the AssociatedObject that started the drag-and-drop.
            // We could store this object in a static field.
            //ItemsControl sourceItemsControl = parameter.DragSource as ItemsControl;
            var targetItemsControl = parameter.AssociatedObject as ItemsControl;
            if (targetItemsControl != null && _dragCommandParameter != null)
            {
                // Special case: Item dragged and dropped on the same ItemsControl.
                // In this case we first remove the item from the source ItemsControl.
                if ((parameter.DragEventArgs.Effects & DragDropEffects.Move) != 0)
                {
                    int indexRemoved = RemoveItemFromItemsControl(targetItemsControl, parameter.Data);
                    if (indexRemoved != -1 && indexRemoved < parameter.TargetIndex)
                    {
                        // The item was dragged to a later position within the same ItemsControl.
                        // --> Update the insertion index.
                        parameter.TargetIndex--;
                    }

                    // Set a flag to indicate that the DragCommand should not do anything.
                    // We have already taken care of the source.
                    _sourceUpdated = true;
                }
            }

            // Insert data into target.
            var targetContentControl = parameter.AssociatedObject as ContentControl;
            if (targetContentControl != null)
            {
                if (_dragCommandParameter != null)
                {
                    // We are dragging from and dropping into the same control. --> Do nothing.
                    parameter.DragEventArgs.Effects = DragDropEffects.None;
                }
                else
                {
                    // Target is ContentControl. --> Set data as content.
                    targetContentControl.Content = parameter.Data;
                }
            }
            else if (targetItemsControl != null)
            {
                // Target is ItemsControl. --> Add data to items.
                InsertItemInItemsControl(targetItemsControl, parameter.Data, parameter.TargetIndex);
            }
        }



        /// <summary>
        /// Creates a default <see cref="IDataObject"/> for the given data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The <see cref="IDataObject"/>.</returns>
        /// <remarks>
        /// <para>
        /// This method creates a default <see cref="IDataObject"/> for drag-and-drop operations.
        /// Use <see cref="GetData"/> to extract the data from an <see cref="IDataObject"/>.
        /// </para>
        /// <para>
        /// This method is used by the default drag-and-drop implementation. Custom 
        /// <see cref="DragCommand"/>s and <see cref="DropCommand"/>s do not have to use this 
        /// method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="data"/> is <see langword="null"/>.
        /// </exception>
        public static IDataObject SetData(object data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new DataObject(DefaultFormat.Name, data);
        }


        /// <summary>
        /// Gets the data from a default <see cref="IDataObject"/>.
        /// </summary>
        /// <param name="dataObject">The <see cref="IDataObject"/> that contains the data.</param>
        /// <returns>
        /// The data from the <see cref="IDataObject"/>. Returns <see langword="null"/> if
        /// <paramref name="dataObject"/> has format that is not supported.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method extracts the original data from an <see cref="IDataObject"/> which has been
        /// created with <see cref="SetData"/>.
        /// </para>
        /// <para>
        /// This method is used by the default drag-and-drop implementation. Custom 
        /// <see cref="DragCommand"/>s and <see cref="DropCommand"/>s do not have to use this 
        /// method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dataObject"/> is <see langword="null"/>.
        /// </exception>
        public static object GetData(IDataObject dataObject)
        {
            if (dataObject == null)
                throw new ArgumentNullException(nameof(dataObject));

            return dataObject.GetData(DefaultFormat.Name);
        }


        // Can the dragged data be dropped into the ItemsControl?
        private static bool CanInsertDataInItemsControl(object draggedData, ItemsControl itemsControl)
        {
            // We drop into ItemsControl.Items if ItemsSource is null.
            // If ItemsSource is not null, then it has to be IList<data type> or IList.

            if (draggedData == null || itemsControl == null)
                return false;

            var itemsSource = itemsControl.ItemsSource;
            if (itemsSource == null)
                return true;

            var draggedType = draggedData.GetType();
            var collectionType = itemsSource.GetType();

            var genericIListType = collectionType.GetInterface("IList`1");
            if (genericIListType != null)
            {
                var genericArguments = genericIListType.GetGenericArguments();
                return genericArguments[0].IsAssignableFrom(draggedType);
            }

            if (typeof(IList).IsAssignableFrom(collectionType))
                return true;

            return false;
        }


        /// <summary>
        /// Inserts the given item in the items control.
        /// </summary>
        /// <param name="itemsControl">The items control.</param>
        /// <param name="itemToInsert">The item to insert.</param>
        /// <param name="insertionIndex">The index at which to insert the item.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="itemsControl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="itemToInsert"/> is <see langword="null"/>.
        /// </exception>
        private static void InsertItemInItemsControl(ItemsControl itemsControl, object itemToInsert, int insertionIndex)
        {
            if (itemsControl == null)
                throw new ArgumentNullException(nameof(itemsControl));
            if (itemToInsert == null)
                throw new ArgumentNullException(nameof(itemToInsert));

            var itemsSource = itemsControl.ItemsSource;
            if (itemsSource == null)
            {
                // ItemsSource is not used. --> Insert the item directly in the Items collection.
                itemsControl.Items.Insert(insertionIndex, itemToInsert);
                return;
            }

            var list = itemsSource as IList;
            if (list != null)
            {
                // ItemsSource is of type IList.
                list.Insert(insertionIndex, itemToInsert);
                return;
            }

            // ItemSource is generic IList<T>. --> Insert item using reflection.
            var type = itemsSource.GetType();
            var genericIListType = type.GetInterface("IList`1");
            if (genericIListType != null)
            {
                type.GetMethod("Insert").Invoke(itemsSource, new[] { insertionIndex, itemToInsert });
            }
        }


        /// <summary>
        /// Removes the given item from the items control.
        /// </summary>
        /// <param name="itemsControl">The items control.</param>
        /// <param name="itemToRemove">The item to remove.</param>
        /// <returns>
        /// The index at which the item was removed. -1 indicates that the specified item was not
        /// found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="itemsControl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="itemToRemove"/> is <see langword="null"/>.
        /// </exception>
        private static int RemoveItemFromItemsControl(ItemsControl itemsControl, object itemToRemove)
        {
            if (itemsControl == null)
                throw new ArgumentNullException(nameof(itemsControl));
            if (itemToRemove == null)
                throw new ArgumentNullException(nameof(itemToRemove));

            int indexToBeRemoved = itemsControl.Items.IndexOf(itemToRemove);
            if (indexToBeRemoved == -1)
                return indexToBeRemoved;

            var itemsSource = itemsControl.ItemsSource;
            if (itemsSource == null)
            {
                // ItemsSource is not set. --> Remove the item from the Items collection.
                itemsControl.Items.RemoveAt(indexToBeRemoved);
                return indexToBeRemoved;
            }

            var list = itemsSource as IList;
            if (list != null)
            {
                list.RemoveAt(indexToBeRemoved);
                return indexToBeRemoved;
            }

            // ItemSource is generic IList<T>.
            // --> Remove item using reflection.
            var type = itemsSource.GetType();
            var genericIListType = type.GetInterface("IList`1");
            if (genericIListType != null)
                type.GetMethod("RemoveAt").Invoke(itemsSource, new object[] { indexToBeRemoved });

            return indexToBeRemoved;
        }
        #endregion
    }
}
