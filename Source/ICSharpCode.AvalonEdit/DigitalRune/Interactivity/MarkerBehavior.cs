using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;


namespace ICSharpCode.AvalonEdit
{
    /// <summary>
    /// Adds text markers to the <see cref="TextEditor"/>.
    /// </summary>
    public sealed class MarkerBehavior : Behavior<TextEditor>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private MarkerRenderer _markerRenderer;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(MarkerBehavior),
            new PropertyMetadata(Boxes.True, OnIsEnabledChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the behavior is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the behavior is enabled; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the behavior is enabled.")]
        [Category("Common Properties")]
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, Boxes.Box(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="Markers"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkersProperty = DependencyProperty.Register(
            "Markers",
            typeof(TextSegmentCollection<Marker>),
            typeof(MarkerBehavior),
            new PropertyMetadata(null, OnMarkersChanged));

        /// <summary>
        /// Gets or sets the text markers.
        /// This is a dependency property.
        /// </summary>
        /// <value>The text markers. The default value is <see langword="null"/>.</value>
        [Description("Gets or sets the text markers.")]
        [Category("Appearance")]
        public TextSegmentCollection<Marker> Markers
        {
            get { return (TextSegmentCollection<Marker>)GetValue(MarkersProperty); }
            set { SetValue(MarkersProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            _markerRenderer = new MarkerRenderer(AssociatedObject.TextArea.TextView) { Markers = Markers };
            AssociatedObject.TextArea.TextView.BackgroundRenderers.Add(_markerRenderer);
        }


        /// <summary>
        /// Called when the behavior is being detached from its 
        /// <see cref="Behavior{T}.AssociatedObject"/>, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            _markerRenderer.Markers = null;
            AssociatedObject.TextArea.TextView.BackgroundRenderers.Remove(_markerRenderer);
            _markerRenderer = null;
        
            base.OnDetaching();
        }


        /// <summary>
        /// Called when the <see cref="IsEnabled"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnIsEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (MarkerBehavior)dependencyObject;
            bool oldValue = (bool)eventArgs.OldValue;
            bool newValue = (bool)eventArgs.NewValue;
            behavior.OnIsEnabledChanged(oldValue, newValue);
        }


        /// <summary>
        /// Called when the <see cref="IsEnabled"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private /* protected virtual */ void OnIsEnabledChanged(bool oldValue, bool newValue)
        {
            _markerRenderer.IsEnabled = newValue;
        }


        /// <summary>
        /// Called when the <see cref="Markers"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnMarkersChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (MarkerBehavior)dependencyObject;
            var oldValue = (TextSegmentCollection<Marker>)eventArgs.OldValue;
            var newValue = (TextSegmentCollection<Marker>)eventArgs.NewValue;
            behavior.OnMarkersChanged(oldValue, newValue);
        }


        /// <summary>
        /// Called when the <see cref="Markers"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private /* protected virtual */ void OnMarkersChanged(TextSegmentCollection<Marker> oldValue, TextSegmentCollection<Marker> newValue)
        {
            if (_markerRenderer != null)
                _markerRenderer.Markers = newValue;
        }
        #endregion
    }
}
