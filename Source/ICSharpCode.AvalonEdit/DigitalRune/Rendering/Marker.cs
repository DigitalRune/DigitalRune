using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;


namespace ICSharpCode.AvalonEdit.Rendering
{
    /// <summary>
    /// Marks a text segment.
    /// </summary>
    public abstract class Marker : TextSegment
    {
        /// <summary>
        /// Draws the marker.
        /// </summary>
        /// <param name="textView">The <see cref="TextView"/>.</param>
        /// <param name="drawingContext">The <see cref="DrawingContext"/>.</param>
        public abstract void Draw(TextView textView, DrawingContext drawingContext);
    }
}
