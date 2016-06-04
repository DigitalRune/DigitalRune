// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Enables drag-and-drop operations for applications that use MVVM.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DragDropBehavior"/> is an attached behavior that enables drag-and-drop
    /// operations on any controls. <see cref="AllowDrag"/> defines whether a drag-and-drop
    /// operation can be started on this control. <see cref="AllowDrop"/> defines whether an element
    /// can be dropped onto this control. <see cref="AllowDrag"/> and <see cref="AllowDrop"/> are
    /// <see langword="true"/> by default.
    /// </para>
    /// <para>
    /// <strong>Drag command:</strong><br/>
    /// The <see cref="DragCommand"/> is called when the user starts a drag operation by pressing
    /// the left mouse button and moving the mouse. The command is usually implemented by the view
    /// model.
    /// </para>
    /// <para>
    /// When the <see cref="DragDropBehavior"/> detects a drag operation on the element, it calls
    /// the <see cref="ICommand.CanExecute"/> method. If this method returns <see langword="true"/>,
    /// the <see cref="ICommand.Execute"/> method is called. In <see cref="ICommand.Execute"/> the
    /// view model has to start the drag-and-drop operation by calling
    /// <see cref="DragDrop.DoDragDrop"/>. All required parameters to start the drag-and-drop
    /// operation are provided by the <see cref="DragCommandParameter"/>. Please note that
    /// <see cref="DragDrop.DoDragDrop"/> is a synchronous operation that blocks until the
    /// drag-and-drop operation is finished. When the drag-and-drop operation is finished
    /// <see cref="DragDrop.DoDragDrop"/> returns the result of the operation, and the
    /// <see cref="ICommand.Execute"/> can execute the final steps, such as removing the original
    /// data if the drag-and-drop operation was a <see cref="DragDropEffects.Move"/> operation.
    /// </para>
    /// <para>
    /// By default, <see cref="DragCommand"/> is <see langword="null"/>, in which case a default
    /// drag-and-drop implementation is used.
    /// </para>
    /// <para>
    /// <strong>Drop command:</strong><br/>
    /// The <see cref="DropCommand"/> is called to check if data can be dropped onto the element and
    /// to commit a drag-and-drop operation. The command is usually implemented by the view model.
    /// The <see cref="DropCommandParameter"/> is passed as the parameter of the
    /// <see cref="ICommand.CanExecute"/> and <see cref="ICommand.Execute"/> methods.
    /// </para>
    /// <para>
    /// <see cref="ICommand.CanExecute"/> of the <see cref="DropCommand"/> is called during the
    /// drag-and-drop operation when the state of the drag-and-drop operation changes - e.g. a
    /// modifier key was pressed, etc. It is also called when the drop is committed by releasing the
    /// mouse button.
    /// </para>
    /// <para>
    /// The <see cref="ICommand.CanExecute"/> should extract the data from the data object in the
    /// <see cref="DragEventArgs"/>. If the data is allowed to be dropped on the element, the method
    /// should set the data in <see cref="DropCommandParameter.Data"/>. This property will be used
    /// to preview the data near the mouse cursor using the <see cref="DragTemplate"/>. The
    /// <see cref="ICommand.CanExecute"/> method should also set the property
    /// <see cref="DragEventArgs.Effects"/> which indicates the type of drag-and-drop operation.
    /// </para>
    /// <para>
    /// If the drop is committed and <see cref="ICommand.CanExecute"/> returns
    /// <see langword="true"/> then the <see cref="ICommand.Execute"/> method will be called. (Note:
    /// The same instance of the <see cref="DragCommandParameter"/> is passed to both methods. So
    /// properties set in <see cref="ICommand.CanExecute"/> will also be available in
    /// <see cref="ICommand.Execute"/>.) The <see cref="ICommand.Execute"/> method should update the
    /// target depending on the drag-and-drop operation, such as setting the dragged data in the
    /// view model.
    /// </para>
    /// <para>
    /// By default, <see cref="DropCommand"/> is <see langword="null"/>, in which case a default
    /// drag-and-drop implementation is used.
    /// </para>
    /// <para>
    /// <strong>Default drag-and-drop behavior:</strong><br/>
    /// By default, when <see cref="AllowDrag"/> is set and the <see cref="DragCommand"/> is not
    /// set, a drag-and-drop using the content of the control is initiated automatically.
    /// <see cref="DefaultEffects"/> can be used to define the allowed actions during the default
    /// drag-and-drop.
    /// </para>
    /// <para>
    /// By default, when <see cref="AllowDrop"/> is set and the <see cref="DropCommand"/> is not
    /// set, a default drop command implementation is used which drops the dragged data into the
    /// content of the control.
    /// </para>
    /// <para>
    /// The default drag-and-drop implementation supports drag-and-drop between
    /// <see cref="ContentControl"/>s and <see cref="ItemsControl"/>s. If the source of the
    /// operation is of type <see cref="ContentControl"/>, then the data dragged is the
    /// <see cref="ContentControl.Content"/>. If the source is of type <see cref="ItemsControl"/>
    /// then the data dragged is the selected <see cref="ItemsControl.Items"/>. (Note: Multi-
    /// selections are supported.)
    /// </para>
    /// <para>
    /// The <see cref="DragDropBehavior"/> by default creates an <see cref="IDataObject"/> using a
    /// custom format. The <see cref="DragDropBehavior"/> expects this format and will not be able
    /// to handle drag-and-drop operations with other formats. The methods <see cref="SetData"/> and
    /// <see cref="GetData"/> can be used to create <see cref="IDataObject"/>s with the required
    /// format and extract the data from them.
    /// </para>
    /// <para>
    /// <strong>Inter-process drag-and-drop:</strong><br/>
    /// The data can be dragged from one application into another if it is serializable (see
    /// <see cref="SerializableAttribute"/>). Currently there is no mechanism for the source or the
    /// target to detect if the data was moved across application.
    /// </para>
    /// </remarks>
    public partial class DragDropBehavior : Behavior<UIElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DragDropBehavior"/> class.
        /// </summary>
        public DragDropBehavior()
        {
            DefaultDragCommand = new DelegateCommand<DragCommandParameter>(ExecuteDrag, CanExecuteDrag);
            DefaultDropCommand = new DelegateCommand<DropCommandParameter>(ExecuteDrop, CanExecuteDrop);
        }


        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the <see cref="Behavior{T}.AssociatedObject"/>.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AllowDrag)
                EnableDrag();

            if (AllowDrop)
                EnableDrop();
        }


        /// <summary>
        /// Called when the <see cref="Behavior{T}"/> is about to detach from the 
        /// <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        /// <remarks>
        /// When this method is called, detaching can not be canceled. The 
        /// <see cref="Behavior{T}.AssociatedObject"/> is still set.
        /// </remarks>
        protected override void OnDetaching()
        {
            if (AllowDrag)
                DisableDrag();

            if (AllowDrop)
                DisableDrop();

            base.OnDetaching();
        }



        ///// <summary>
        ///// Tests whether the given object is serializable by serializing the object in a memory
        ///// stream. (Debug only: This method does nothing unless the conditional compilation symbol
        ///// "DEBUG" is defined.)
        ///// </summary>
        ///// <param name="data">The object to be serialized.</param>
        ///// <exception cref="ArgumentNullException">
        ///// <paramref name="data"/> is <see langword="null"/>.
        ///// </exception>
        ///// <exception cref="SerializationException">
        ///// An error has occurred during serialization, such as if a type in the object graph is not
        ///// marked as serializable.
        ///// </exception>
        ///// <exception cref="SecurityException">
        ///// The caller does not have the required permission.
        ///// </exception>
        //[Conditional("DEBUG")]
        //public static void TestSerialization(object data)
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        var binaryFormatter = new BinaryFormatter();
        //        binaryFormatter.Serialize(memoryStream, data);
        //    }
        //}
    }
}
