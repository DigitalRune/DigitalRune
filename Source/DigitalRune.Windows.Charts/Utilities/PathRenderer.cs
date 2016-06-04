// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
#if SILVERLIGHT
    /// <summary>
    /// Helper class for the rendering a geometry using a <see cref="Path"/>.
    /// </summary>
    internal class PathRenderer
    {
        //--------------------------------------------------------------
        #region Nested Types
        //--------------------------------------------------------------

        /// <summary>
        /// Renders geometric segments of a <see cref="Path"/>.
        /// </summary>
        public class Context : IDisposable
        {
            private readonly PathGeometry _pathGeometry;
            private Point _lastPoint;


            /// <summary>
            /// Initializes a new instance of the <see cref="Context"/> class.
            /// </summary>
            /// <param name="pathGeometry">The <see cref="PathGeometry"/>.</param>
            internal Context(PathGeometry pathGeometry)
            {
                _pathGeometry = pathGeometry;
                _lastPoint = new Point(double.NaN, double.NaN);
            }


            /// <summary>
            /// Closes the render context and flushes its content so that it can be rendered.
            /// </summary>
            public void Close()
            {
            }


            /// <summary>
            /// Closes the render context and flushes its content so that it can be rendered.
            /// </summary>
            public void Dispose()
            {
            }


            /// <summary>
            /// Draws a line.
            /// </summary>
            /// <param name="start">The start point of the line.</param>
            /// <param name="end">The end point of the line.</param>
            public void DrawLine(Point start, Point end)
            {
                if (_lastPoint == start)
                {
                    // Continue previous PathFigure.
                    var pathFigure = _pathGeometry.Figures[_pathGeometry.Figures.Count - 1];
                    pathFigure.Segments.Add(new LineSegment { Point = end });
                }
                else
                {
                    // Start new PathFigure.
                    var pathFigure = new PathFigure { StartPoint = start };
                    _pathGeometry.Figures.Add(pathFigure);
                    pathFigure.Segments.Add(new LineSegment { Point = end });
                }

                // Store last point.
                _lastPoint = end;
            }


            /// <summary>
            /// Draws a polygon with 4 points.
            /// </summary>
            /// <param name="p0">The first point.</param>
            /// <param name="p1">The second point.</param>
            /// <param name="p2">The third point.</param>
            /// <param name="p3">The fourth point.</param>
            public void DrawPolygon(Point p0, Point p1, Point p2, Point p3)
            {
                var pathFigure = new PathFigure { StartPoint = p0 };
                var polyLineSegment = new PolyLineSegment { Points = new PointCollection() };
                polyLineSegment.Points.Add(p1);
                polyLineSegment.Points.Add(p2);
                polyLineSegment.Points.Add(p3);

                pathFigure.Segments.Add(polyLineSegment);
                _pathGeometry.Figures.Add(pathFigure);
            }


            /// <summary>
            /// Draws a polygon with an arbitrary number of points.
            /// </summary>
            /// <param name="points">The points on the polygon.</param>
            public void DrawPolygon(params Point[] points)
            {
                if (points == null || points.Length <= 1)
                    return;

                var pathFigure = new PathFigure { StartPoint = points[0] };
                var polyLineSegment = new PolyLineSegment { Points = new PointCollection() };
                for (int i = 1; i < points.Length; i++)
                    polyLineSegment.Points.Add(points[i]);

                pathFigure.Segments.Add(polyLineSegment);
                _pathGeometry.Figures.Add(pathFigure);
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly PathGeometry _pathGeometry;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="PathRenderer"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public PathRenderer(Path path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            _pathGeometry = new PathGeometry();

            Debug.Assert(_pathGeometry.Figures != null);
            Debug.Assert(_pathGeometry.Figures.Count == 0);

            path.Data = _pathGeometry;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Clears the geometry of the <see cref="Path"/>.
        /// </summary>
        public void Clear()
        {
            var figures = _pathGeometry.Figures;
            if (figures != null)
            {
                figures.Clear();

                // Reassign figures collection - otherwise Silverlight won't update the visual.
                _pathGeometry.Figures = figures;
            }
        }


        /// <summary>
        /// Opens a context that can be used to render geometry.
        /// </summary>
        /// <returns>The render context.</returns>
        public Context Open()
        {
            return new Context(_pathGeometry);
        }
        #endregion
    }
#else
    /// <summary>
    /// Helper class for rendering a geometry using a <see cref="Path"/>.
    /// </summary>
    internal class PathRenderer
    {
        //--------------------------------------------------------------
        #region Nested Types
        //--------------------------------------------------------------

        /// <summary>
        /// Renders geometric segments of a <see cref="Path"/>.
        /// </summary>
        public sealed class Context : IDisposable
        {
            private readonly StreamGeometryContext _streamGeometryContext;
            private Point _lastPoint;


            /// <summary>
            /// Initializes a new instance of the <see cref="Context"/> class.
            /// </summary>
            /// <param name="streamGeometry">The <see cref="StreamGeometry"/>.</param>
            internal Context(StreamGeometry streamGeometry)
            {
                _streamGeometryContext = streamGeometry.Open();
                _lastPoint = new Point(double.NaN, double.NaN);
            }


            /// <summary>
            /// Closes the render context and flushes its content so that it can be rendered.
            /// </summary>
            public void Close()
            {
                _streamGeometryContext.Close();
            }


            /// <summary>
            /// Closes the render context and flushes its content so that it can be rendered.
            /// </summary>
            void IDisposable.Dispose()
            {
                Close();
            }


            /// <summary>
            /// Draws a line.
            /// </summary>
            /// <param name="start">The start point of the line.</param>
            /// <param name="end">The end point of the line.</param>
            public void DrawLine(Point start, Point end)
            {
                // IMPORTANT: isSmoothJoin needs to be true! Otherwise thick lines may have very
                //            sharp corners. These corners can overshoot the actual point!

                Debug.Assert(!Numeric.IsNaN(start.X) && !Numeric.IsNaN(start.Y));
                Debug.Assert(!Numeric.IsNaN(end.X) && !Numeric.IsNaN(end.Y));

                if (_lastPoint == start)
                {
                    // Continue previous figure.
                    _streamGeometryContext.LineTo(end, true, true);
                }
                else
                {
                    // Start new figure.
                    _streamGeometryContext.BeginFigure(start, false, false);
                    _streamGeometryContext.LineTo(end, true, true);
                }

                // Store last point.
                _lastPoint = end;
            }


            /// <summary>
            /// Draws a polygon with 4 points.
            /// </summary>
            /// <param name="p0">The first point.</param>
            /// <param name="p1">The second point.</param>
            /// <param name="p2">The third point.</param>
            /// <param name="p3">The fourth point.</param>
            public void DrawPolygon(Point p0, Point p1, Point p2, Point p3)
            {
                Debug.Assert(!Numeric.IsNaN(p0.X) && !Numeric.IsNaN(p0.Y));
                Debug.Assert(!Numeric.IsNaN(p1.X) && !Numeric.IsNaN(p1.Y));
                Debug.Assert(!Numeric.IsNaN(p2.X) && !Numeric.IsNaN(p2.Y));
                Debug.Assert(!Numeric.IsNaN(p3.X) && !Numeric.IsNaN(p3.Y));

                _streamGeometryContext.BeginFigure(p0, true, true);
                _streamGeometryContext.LineTo(p1, true, true);
                _streamGeometryContext.LineTo(p2, true, true);
                _streamGeometryContext.LineTo(p3, true, true);
            }


            /// <summary>
            /// Draws a polygon with an arbitrary number of points.
            /// </summary>
            /// <param name="points">The points on the polygon.</param>
            public void DrawPolygon(params Point[] points)
            {
                if (points == null || points.Length <= 1)
                    return;

                Debug.Assert(points.All(p => !Numeric.IsNaN(p.X) && !Numeric.IsNaN(p.Y)));

                _streamGeometryContext.BeginFigure(points[0], true, true);
                for (int i = 1; i < points.Length; i++)
                    _streamGeometryContext.LineTo(points[i], true, true);
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly StreamGeometry _streamGeometry;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="PathRenderer"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public PathRenderer(Path path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            _streamGeometry = new StreamGeometry();
            path.Data = _streamGeometry;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Clears the geometry of the <see cref="Path"/>.
        /// </summary>
        public void Clear()
        {
            _streamGeometry.Clear();
        }


        /// <summary>
        /// Opens a context that can be used to render geometry.
        /// </summary>
        /// <returns>The render context.</returns>
        public Context Open()
        {
            return new Context(_streamGeometry);
        }
        #endregion
    }
#endif
}
