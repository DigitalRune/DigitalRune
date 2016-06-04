using System;
using System.Collections.ObjectModel;


namespace DragDropApp
{
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();

            // Create ViewModels and connect to View.
            SmallAlbum.DataContext = new AlbumViewModel
            {
                Pictures = new ObservableCollection<PictureViewModel>
                {
                    new PictureViewModel(new Uri("Images\\Red.png", UriKind.Relative), "Red", "A red dot."),
                    new PictureViewModel(new Uri("Images\\Orange.png", UriKind.Relative), "Orange", "An orange dot."),
                    new PictureViewModel(new Uri("Images\\Yellow.png", UriKind.Relative), "Yellow", "A yellow dot."),
                    new PictureViewModel(new Uri("Images\\Green.png", UriKind.Relative), "Green", "A green dot."),
                    new PictureViewModel(new Uri("Images\\Cyan.png", UriKind.Relative), "Purple", "A cyan dot."),
                    new PictureViewModel(new Uri("Images\\Blue.png", UriKind.Relative), "Blue", "A blue dot."),
                    new PictureViewModel(new Uri("Images\\Magenta.png", UriKind.Relative), "Magenta", "A magenta dot."),
                }
            };
            DetailedAlbum.DataContext = new AlbumViewModel();
            PreviewArea.DataContext = new PreviewViewModel();
        }
    }
}
