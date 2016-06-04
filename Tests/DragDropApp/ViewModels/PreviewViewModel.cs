using System.Windows;
using System.Windows.Input;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace DragDropApp
{
    /// <summary>
    /// Represents a preview area where a picture is shown.
    /// </summary>
    public class PreviewViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the picture.
        /// </summary>
        /// <value>The picture.</value>
        public PictureViewModel Picture
        {
            get { return _picture; }
            set
            {
                if (_picture == value)
                    return;

                _picture = value;
                RaisePropertyChanged(() => Picture);
            }
        }
        private PictureViewModel _picture;


        ///// <summary>
        ///// Gets the <see cref="ICommand"/> executed when a drag-and-drop operation is started.
        ///// </summary>
        ///// <value>The <see cref="ICommand"/> executed when a drag-and-drop operation is started.</value>
        //public ICommand DragCommand
        //{
        //  get
        //  {
        //    if (_dragCommand == null)
        //      _dragCommand = new DelegateCommand<DragCommandParameter>(Drag);

        //    return _dragCommand;
        //  }
        //}
        //private ICommand _dragCommand;


        /// <summary>
        /// Gets the <see cref="ICommand"/> executed when data is dropped on the preview area.
        /// </summary>
        /// <value>The <see cref="ICommand"/> executed when data is dropped on the preview area.</value>
        public ICommand DropCommand
        {
            get
            {
                if (_dropCommand == null)
                    _dropCommand = new DelegateCommand<DropCommandParameter>(Drop, CanDrop);

                return _dropCommand;
            }
        }
        private ICommand _dropCommand;


        //private void Drag(DragCommandParameter parameter)
        //{
        //  if (Picture != null)
        //  {
        //    IDataObject dataObject = DragDropBehavior.CreateDataObject(Picture);
        //    DragDropEffects effect = DragDrop.DoDragDrop(parameter.DragSource, dataObject, DragDropEffects.Move);
        //    if ((effect & DragDropEffects.Move) != 0)
        //      Picture = null;
        //  }
        //}


        private bool CanDrop(DropCommandParameter parameter)
        {
            // Extract data.
            var draggedPicture = DragDropBehavior.GetData(parameter.DragEventArgs.Data) as PictureViewModel;

            // To enable Drop the preview area needs to be empty and the dragged data needs to be a
            // picture.
            if (Picture == null && draggedPicture != null)
            {
                parameter.Data = draggedPicture;
                parameter.DragEventArgs.Effects = DragDropEffects.Move;
                return true;
            }

            return false;
        }


        private void Drop(DropCommandParameter parameter)
        {
            Picture = (PictureViewModel)parameter.Data;
        }
    }
}
