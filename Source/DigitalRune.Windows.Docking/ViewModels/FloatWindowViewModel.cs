// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a window that contains elements when they are dragged from the docking layout.
    /// </summary>
    /// <inheritdoc cref="IFloatWindow"/>
    public class FloatWindowViewModel : ObservableObject, IFloatWindow
    {
        /// <inheritdoc/>
        public IDockPane RootPane
        {
            get { return _rootPane; }
            set { SetProperty(ref _rootPane, value); }
        }
        private IDockPane _rootPane;


        /// <inheritdoc/>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
        private bool _isVisible;


        /// <inheritdoc/>
        public double Left
        {
            get { return _left; }
            set { SetProperty(ref _left, value); }
        }
        private double _left = double.NaN;


        /// <inheritdoc/>
        public double Top
        {
            get { return _top; }
            set { SetProperty(ref _top, value); }
        }
        private double _top = double.NaN;


        /// <inheritdoc/>
        public double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }
        private double _width = double.NaN;


        /// <inheritdoc/>
        public double Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }
        private double _height = double.NaN;


        /// <inheritdoc/>
        public WindowState WindowState
        {
            get { return _windowState; }
            set { SetProperty(ref _windowState, value); }
        }
        private WindowState _windowState = WindowState.Normal;
    }
}
