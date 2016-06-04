using System.Windows;
using System.Windows.Media;


namespace ICSharpCode.AvalonEdit.Rendering
{
    /// <summary>
    /// Draws a zigzag line below the text.
    /// </summary>
    public class ZigzagMarker : Marker
    {
        private static readonly Pen DefaultPen;


        /// <summary>
        /// Gets or sets the pen.
        /// </summary>
        /// <value>The pen. The default value is a red pen.</value>
        public Pen Pen { get; set; }


        /// <summary>
        /// Initializes static members of the <see cref="ZigzagMarker"/> class.
        /// </summary>
        static ZigzagMarker()
        {
            var brush = new SolidColorBrush(Colors.Red);
            brush.Freeze();

            DefaultPen = new Pen(brush, 0.75);
            DefaultPen.Freeze();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ZigzagMarker"/> class.
        /// </summary>
        public ZigzagMarker()
        {
            Pen = DefaultPen;
        }


        /// <inheritdoc/>
        public override void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (Pen == null)
                return;

            foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, this))
            {
                if (rect.Width <= 1 || rect.Height <= 1)
                {
                    // Current segment is inside a folding.
                    continue;
                }

                var start = rect.BottomLeft;
                var end = rect.BottomRight;
                var geometry = new StreamGeometry();
                using (var context = geometry.Open())
                {
                    context.BeginFigure(start, false, false);

                    const double zigLength = 3;
                    const double zigHeight = 3;
                    int numberOfZigs = (int)((end.X - start.X) / zigLength + 0.5f);
                    if (numberOfZigs < 2)
                        numberOfZigs = 2;

                    for (int i = 0; i < numberOfZigs; i++)
                    {
                        var p = new Point(
                          start.X + (i + 1) * zigLength,
                          start.Y - (i % 2) * zigHeight + 1);

                        context.LineTo(p, true, false);
                    }
                }

                drawingContext.DrawGeometry(null, Pen, geometry);
            }
        }
    }
}
