using System.Collections.ObjectModel;
using DigitalRune.Windows;


namespace DragDropApp
{
    /// <summary>
    /// Represents an album.
    /// </summary>
    public class AlbumViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the picture of the album.
        /// </summary>
        /// <value>The picture of the album.</value>
        public ObservableCollection<PictureViewModel> Pictures
        {
            get { return _pictures; }
            set
            {
                if (_pictures == value)
                    return;

                _pictures = value;
                RaisePropertyChanged(() => Pictures);
            }
        }
        private ObservableCollection<PictureViewModel> _pictures = new ObservableCollection<PictureViewModel>();
    }
}
