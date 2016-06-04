using System.Windows.Media;


namespace ICSharpCode.AvalonEdit.Rendering
{
    /// <summary>
    /// Draws a block in the background of the text.
    /// </summary>
    public class BlockMarker : Marker
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the brush used to fill the block.
        /// </summary>
        /// <value>The brush. The default value is a dark red brush.</value>
        public Brush Brush { get; set; }


        /// <summary>
        /// Gets or sets the pen used for the outline.
        /// </summary>
        /// <value>The pen. The default value is <see langword="null"/>.</value>
        public Pen Pen { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Marker"/> class.
        /// </summary>
        public BlockMarker()
        {
            Brush = Brushes.DarkRed;
            Pen = null;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public override void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (Pen == null && Brush == null)
                return;

            var geometryBuilder = new BackgroundGeometryBuilder
            {
                CornerRadius = 1,
                AlignToMiddleOfPixels = true
            };

            geometryBuilder.AddSegment(textView, this);
            var geometry = geometryBuilder.CreateGeometry();
            if (geometry != null && geometry.Bounds.Width > 1 && geometry.Bounds.Height > 1)
                drawingContext.DrawGeometry(Brush, Pen, geometry);
        }
        #endregion
    }
}
