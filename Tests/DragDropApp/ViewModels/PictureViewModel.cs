using System;
using DigitalRune.Windows;


namespace DragDropApp
{
    /// <summary>
    /// Represents a picture of an album.
    /// </summary>
    //[Serializable]
    public class PictureViewModel : ObservableObject
    {
        // Note: PictureViewModel can be made serializable by uncommenting the line above.
        // When it is serializable the pictures can be dragged across applications of the same type.

        /// <summary>
        /// Gets or sets the URI of the image source.
        /// </summary>
        /// <value>The URI of the image source.</value>
        public Uri Location
        {
            get { return _location; }
            set
            {
                if (_location == value)
                    return;

                _location = value;
                RaisePropertyChanged(() => Location);
            }
        }
        private Uri _location;


        /// <summary>
        /// Gets or sets the name of the picture.
        /// </summary>
        /// <value>The name of the picture.</value>
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                    return;

                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }
        private string _name;


        /// <summary>
        /// Gets or sets the description of the picture.
        /// </summary>
        /// <value>The description of the picture.</value>
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value)
                    return;

                _description = value;
                RaisePropertyChanged(() => Description);
            }
        }
        private string _description;


        /// <summary>
        /// Initializes a new instance of the <see cref="PictureViewModel"/> class.
        /// </summary>
        /// <param name="location">The URI of the image source.</param>
        /// <param name="name">The name of the picture.</param>
        /// <param name="description">The description of the picture.</param>
        public PictureViewModel(Uri location, string name, string description)
        {
            Location = location;
            Name = name;
            Description = description;
        }
    }
}
