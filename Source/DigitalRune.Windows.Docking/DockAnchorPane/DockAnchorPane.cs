// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents an anchored pane in the docking layout.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <see cref="IDockAnchorPane"/> for additional information.
    /// </para>
    /// <para>
    /// <strong>Visual states:</strong> The control has two visual states: "Empty" and "Filled". The
    /// state is "Empty" if the content of the <see cref="DockAnchorPane"/> is empty or invisible;
    /// otherwise, the state is "Filled". The default behavior is to render a gray background when
    /// the state is "Empty". When the state is "Filled", the content is shown and the control
    /// background is transparent.
    /// </para>
    /// </remarks>
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    [TemplateVisualState(GroupName = "ContentStates", Name = "Empty")]
    [TemplateVisualState(GroupName = "ContentStates", Name = "Filled")]
    public class DockAnchorPane : ContentControl
    {
        // The DockAnchorPane is a ContentControl. The content is always created, even if
        // IDockPane.IsVisible = false. To hide the content the DockAnchorPane monitors
        // IDockPane.IsVisible of the content and collapses the associated ContentPresenter.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private ContentPresenter _contentPresenter;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------    

        /// <summary>
        /// Gets the <see cref="FrameworkElement"/> that represents the
        /// <see cref="IDockAnchorPane.ChildPane"/>.
        /// </summary>
        /// <value>
        /// The <see cref="FrameworkElement"/> that represents the
        /// <see cref="IDockAnchorPane.ChildPane"/>.
        /// </value>
        internal FrameworkElement ChildPane
        {
            get { return _contentPresenter.GetContentContainer<FrameworkElement>(); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="DockWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DockWidthProperty = DockControl.DockWidthProperty.AddOwner(typeof(DockAnchorPane));

        /// <inheritdoc cref="IDockElement.DockWidth"/>
        [Description("Gets or sets the desired width in the docking layout.")]
        [Category(Categories.Layout)]
        [TypeConverter(typeof(GridLengthConverter))]
        public GridLength DockWidth
        {
            get { return (GridLength)GetValue(DockWidthProperty); }
            set { SetValue(DockWidthProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="DockHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DockHeightProperty = DockControl.DockHeightProperty.AddOwner(typeof(DockAnchorPane));

        /// <inheritdoc cref="IDockElement.DockHeight"/>
        [Description("Gets or sets the desired height in the docking layout.")]
        [Category(Categories.Layout)]
        [TypeConverter(typeof(GridLengthConverter))]
        public GridLength DockHeight
        {
            get { return (GridLength)GetValue(DockHeightProperty); }
            set { SetValue(DockHeightProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="DockAnchorPane"/> class.
        /// </summary>
        static DockAnchorPane()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockAnchorPane), new FrameworkPropertyMetadata(typeof(DockAnchorPane)));
            HorizontalContentAlignmentProperty.OverrideMetadata(typeof(DockAnchorPane), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));
            VerticalContentAlignmentProperty.OverrideMetadata(typeof(DockAnchorPane), new FrameworkPropertyMetadata(VerticalAlignment.Stretch));

            // DockAnchorPane is not a tab stop, only the DockTabItems inside the DockPanes.
            IsTabStopProperty.OverrideMetadata(typeof(DockAnchorPane), new FrameworkPropertyMetadata(Boxed.BooleanFalse));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DockAnchorPane"/> class.
        /// </summary>
        public DockAnchorPane()
        {
            Loaded += OnLoaded;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _contentPresenter = null;

            base.OnApplyTemplate();

            _contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
            UpdateVisualStates(false);
        }


        /// <summary>
        /// Called when the <see cref="ContentControl.Content"/> property changes.
        /// </summary>
        /// <param name="oldContent">
        /// The old value of the <see cref="ContentControl.Content"/> property.
        /// </param>
        /// <param name="newContent">
        /// The new value of the <see cref="ContentControl.Content"/> property.
        /// </param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            var oldDockPane = oldContent as INotifyPropertyChanged;
            if (oldDockPane != null)
                PropertyChangedEventManager.RemoveHandler(oldDockPane, OnContentVisibilityChanged, nameof(IDockPane.IsVisible));

            base.OnContentChanged(oldContent, newContent);

            var newDockPane = newContent as INotifyPropertyChanged;
            if (newDockPane != null)
                PropertyChangedEventManager.AddHandler(newDockPane, OnContentVisibilityChanged, nameof(IDockPane.IsVisible));

            UpdateVisualStates(true);
        }


        private void OnContentVisibilityChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            UpdateVisualStates(true);
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            UpdateVisualStates(true);
        }


        private void UpdateVisualStates(bool useTransitions)
        {
            // Show/hide ChildPane.
            var content = Content;
            var dockPane = content as IDockPane;
            bool isContentVisible = dockPane?.IsVisible ?? (content != null);
            if (_contentPresenter != null)
                _contentPresenter.Visibility = isContentVisible ? Visibility.Visible : Visibility.Collapsed;

            // Update visual states.
            if (isContentVisible)
                VisualStateManager.GoToState(this, "Filled", useTransitions);
            else
                VisualStateManager.GoToState(this, "Empty", useTransitions);
        }
        #endregion
    }
}
