// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Interop;
using DigitalRune.Windows;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Implements the <see cref="IPresentationTarget"/>
    /// </summary>
    public class GamePresentationTarget : D3DImagePresentationTarget
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        // Used by GameExtension
        internal Size LastSize { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the front buffer needs to be updated.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the font buffer needs to be updated; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// The flag <see cref="IsDirty"/> is read and written by the <see cref="GameExtension"/>.
        /// When a new Direct3D 11 scene was rendered into the back buffer the flag is set
        /// indicating that the front buffer of the <see cref="D3DImage"/> needs to be updated. It
        /// is automatically reset by the <see cref="GameExtension"/> when back buffer was
        /// successfully copied to the the front buffer.
        /// </remarks>
        public bool IsDirty { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="GraphicsScreens"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GraphicsScreensProperty = DependencyProperty.Register(
            "GraphicsScreens",
            typeof(IList<GraphicsScreen>),
            typeof(GamePresentationTarget),
            new PropertyMetadata(null, OnGraphicsScreensChanged));

        /// <summary>
        /// Gets or sets the graphics screens to be rendered.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The graphics screens to be rendered. The default value is <see langword="null"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets the graphics screens to be rendered.")]
        [Category(Categories.Default)]
        public IList<GraphicsScreen> GraphicsScreens
        {
            get { return (IList<GraphicsScreen>)GetValue(GraphicsScreensProperty); }
            set { SetValue(GraphicsScreensProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePresentationTarget"/> class.
        /// </summary>
        public GamePresentationTarget()
        {
            if (!WindowsHelper.IsInDesignMode)
            {
                //DataContextChanged += OnDataContextChanged;
                Loaded += OnLoaded;
                Unloaded += OnUnloaded;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        //private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
        //{
        //    // Optional: Inject PresentationTarget in view model.
        //    var presentationTargetScreen = (IPresentationTargetScreen)DataContext;
        //    presentationTargetScreen.PresentationTarget = this;
        //}


        /// <summary>
        /// Called when the <see cref="GraphicsScreens"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnGraphicsScreensChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (GamePresentationTarget)dependencyObject;
            var oldValue = (IEnumerable<GraphicsScreen>)eventArgs.OldValue;
            var newValue = (IEnumerable<GraphicsScreen>)eventArgs.NewValue;
            target.OnGraphicsScreensChanged(oldValue, newValue);
        }


        /// <summary>
        /// Called when the <see cref="GraphicsScreens"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnGraphicsScreensChanged(IEnumerable<GraphicsScreen> oldValue, IEnumerable<GraphicsScreen> newValue)
        {
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            // Register the presentation target in the graphics service.
            if (GraphicsService == null)
            {
                var editor = this.GetEditor().WarnIfMissing();
                var graphicsService = editor?.Services.GetInstance<IGraphicsService>().WarnIfMissing();
                graphicsService?.PresentationTargets.Add(this);
            }
        }


        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            // Unregister the presentation target from the graphics service.
            GraphicsService?.PresentationTargets.Remove(this);
        }


        ///// <summary>
        ///// Raises the <see cref="FrameworkElement.SizeChanged" /> event, using the specified
        ///// information as part of the eventual event data.
        ///// </summary>
        ///// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        //{
        //    base.OnRenderSizeChanged(sizeInfo);
        //}
        #endregion
    }
}
