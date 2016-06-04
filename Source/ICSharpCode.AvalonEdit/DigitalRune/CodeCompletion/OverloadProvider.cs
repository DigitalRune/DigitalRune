using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Provides the items for the <see cref="OverloadViewer"/>. (Default implementation of
    /// <see cref="IOverloadProvider"/>.)
    /// </summary>
    public class OverloadProvider : IOverloadProvider
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public int Count
        {
            get { return Overloads?.Count ?? 0; }
        }


        /// <inheritdoc/>
        public string CurrentIndexText
        {
            get { return Overloads?[SelectedIndex].IndexText; }
        }


        /// <inheritdoc/>
        public object CurrentHeader
        {
            get { return Overloads?[SelectedIndex].Header; }
        }


        /// <inheritdoc/>
        public object CurrentContent
        {
            get { return Overloads?[SelectedIndex].Content; }
        }


        /// <summary>
        /// Gets or sets the overloads.
        /// </summary>
        /// <value>The overloads. The default value is an empty list.</value>
        public IList<OverloadDescription> Overloads
        {
            get { return _overloads; }
            set
            {
                if (_overloads == value)
                    return;

                _overloads = value;
                OnPropertyChanged(new PropertyChangedEventArgs(null));
            }
        }
        private IList<OverloadDescription> _overloads;


        /// <summary>
        /// Gets/Sets the selected index.
        /// </summary>
        /// <value>The selected index.</value>
        /// <exception cref="ArgumentOutOfRangeException">The index is out of range.</exception>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex == value)
                    return;

                _selectedIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs(null));
            }
        }
        private int _selectedIndex;


        /// <summary>
        /// Occurs when a property value changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="OverloadProvider"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="OverloadProvider"/> class.
        /// </summary>
        public OverloadProvider()
        {
            Overloads = new List<OverloadDescription>();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="OverloadProvider"/> class.
        /// </summary>
        /// <param name="overloads">The overloads.</param>
        public OverloadProvider(IList<OverloadDescription> overloads)
        {
            Overloads = overloads;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="PropertyChangedEventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors: </strong>When overriding <see cref="OnPropertyChanged"/> in a 
        /// derived class, be sure to call the base class's <see cref="OnPropertyChanged"/> method so that 
        /// registered delegates receive the event.
        /// </remarks>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            PropertyChanged?.Invoke(this, eventArgs);
        }
        #endregion
    }
}
