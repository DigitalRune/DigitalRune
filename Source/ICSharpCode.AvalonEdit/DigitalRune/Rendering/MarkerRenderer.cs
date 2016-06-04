using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;


namespace ICSharpCode.AvalonEdit.Rendering
{
    /// <summary>
    /// Draws the background of marked text segments.
    /// </summary>
    public class MarkerRenderer : IBackgroundRenderer
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly TextView _textView;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether this renderer is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this renderer is enabled; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value)
                    return;

                _isEnabled = value;
                InvalidateMarkers();
            }
        }
        private bool _isEnabled = true;


        /// <summary>
        /// Gets or sets the markers.
        /// </summary>
        /// <value>The markers. The default value is <see langword="null"/>.</value>
        public TextSegmentCollection<Marker> Markers
        {
            get { return _markers; }
            set
            {
                if (_markers == value)
                    return;

                if (_markers != null)
                    CollectionChangedEventManager.RemoveHandler(_markers, OnMarkersChanged);

                _markers = value;

                if (_markers != null)
                    CollectionChangedEventManager.AddHandler(_markers, OnMarkersChanged);

                InvalidateMarkers();
            }
        }
        private TextSegmentCollection<Marker> _markers;


        /// <inheritdoc/>
        public KnownLayer Layer
        {
            get { return KnownLayer.Selection; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkerRenderer"/> class.
        /// </summary>
        /// <param name="textView">The <see cref="TextView"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="textView"/> is <see langword="null"/>.
        /// </exception>
        public MarkerRenderer(TextView textView)
        {
            if (textView == null)
                throw new ArgumentNullException(nameof(textView));

            _textView = textView;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnMarkersChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            InvalidateMarkers();
        }


        private void InvalidateMarkers()
        {
            _textView.InvalidateLayer(Layer);
        }


        /// <inheritdoc/>
        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            Debug.Assert(_textView == textView, "The MarkerRenderer belongs to a different TextEditor.");

            if (!IsEnabled || Markers == null || Markers.Count == 0)
                return;

            // Draw only the relevant part.
            if (textView.VisualLines.Count == 0)
                return;

            // Offsets of the first and last visual text.
            int visualStart = textView.VisualLines[0].FirstDocumentLine.Offset;
            int visualEnd = textView.VisualLines.Last().LastDocumentLine.Offset
                            + textView.VisualLines.Last().LastDocumentLine.Length;

            foreach (var marker in Markers.FindOverlappingSegments(visualStart, visualEnd))
                marker.Draw(textView, drawingContext);
        }
        #endregion
    }
}
