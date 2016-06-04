// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Shows an insertion indicator.
    /// </summary>
    internal class DropAdorner : Adorner
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private static readonly Pen Pen;
        private static readonly PathGeometry Triangle;
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly bool _isIndicatorHorizontal;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether the data is to be insert before or after the
        /// container (the adorned element).
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the data is to be insert after the container;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool InsertAfter { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="DropAdorner"/> class.
        /// </summary>
        static DropAdorner()
        {
            // Create the Pen and Triangle in a static constructor and freeze them to improve 
            // performance.
            Pen = new Pen { Brush = Brushes.Gray, Thickness = 2 };
            Pen.Freeze();

            var firstLine = new LineSegment(new Point(0, -5), false);
            firstLine.Freeze();
            var secondLine = new LineSegment(new Point(0, 5), false);
            secondLine.Freeze();

            var figure = new PathFigure { StartPoint = new Point(5, 0) };
            figure.Segments.Add(firstLine);
            figure.Segments.Add(secondLine);
            figure.Freeze();

            Triangle = new PathGeometry();
            Triangle.Figures.Add(figure);
            Triangle.Freeze();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DropAdorner"/> class.
        /// </summary>
        /// <param name="isIndicatorHorizontal">
        /// If set to <see langword="true"/> the insertion indicator is drawn horizontally.
        /// </param>
        /// <param name="insertAfter">
        /// <see langword="true"/> if the indicator is shown after the container; otherwise,
        /// <see langword="false"/>.
        /// </param>
        /// <param name="adornedElement">The item container (= the adorned element).</param>
        public DropAdorner(bool isIndicatorHorizontal, bool insertAfter, UIElement adornedElement)
            : base(adornedElement)
        {
            _isIndicatorHorizontal = isIndicatorHorizontal;
            IsHitTestVisible = false;
            InsertAfter = insertAfter;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// When overridden in a derived class, participates in rendering operations that are
        /// directed by the layout system. The rendering instructions for this element are not used
        /// directly when this method is invoked, and are instead preserved for later asynchronous
        /// use by layout and drawing.
        /// </summary>
        /// <param name="drawingContext">
        /// The drawing instructions for a specific element. This context is provided to the layout
        /// system.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnRender(DrawingContext drawingContext)
        {
            // This draws one line and two triangles at each end of the line.
            Point startPoint;
            Point endPoint;

            CalculateStartAndEndPoint(out startPoint, out endPoint);
            drawingContext.DrawLine(Pen, startPoint, endPoint);

            if (_isIndicatorHorizontal)
            {
                DrawTriangle(drawingContext, startPoint, 0);
                DrawTriangle(drawingContext, endPoint, 180);
            }
            else
            {
                DrawTriangle(drawingContext, startPoint, 90);
                DrawTriangle(drawingContext, endPoint, -90);
            }
        }


        private void CalculateStartAndEndPoint(out Point startPoint, out Point endPoint)
        {
            startPoint = new Point();
            endPoint = new Point();

            double width = AdornedElement.RenderSize.Width;
            double height = AdornedElement.RenderSize.Height;

            if (_isIndicatorHorizontal)
            {
                endPoint.X = width;
                if (InsertAfter)
                {
                    startPoint.Y = height;
                    endPoint.Y = height;
                }
            }
            else
            {
                endPoint.Y = height;
                if (InsertAfter)
                {
                    startPoint.X = width;
                    endPoint.X = width;
                }
            }
        }


        private static void DrawTriangle(DrawingContext drawingContext, Point origin, double angle)
        {
            drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
            drawingContext.PushTransform(new RotateTransform(angle));
            drawingContext.DrawGeometry(Pen.Brush, null, Triangle);
            drawingContext.Pop();
            drawingContext.Pop();
        }
        #endregion
    }
}
