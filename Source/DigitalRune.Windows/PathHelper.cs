// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Provides helper functions for <see cref="Path"/> objects.
    /// </summary>
    [Obsolete("Use PathRenderer instead of PathHelper.")]
    public static class PathHelper
    {
        /// <summary>
        /// Assigns a new <see cref="PathGeometry"/> to the given <see cref="Path"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The assigned <see cref="PathGeometry"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        public static PathGeometry SetPathGeometry(this Path path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var pathGeometry = new PathGeometry();

            Debug.Assert(pathGeometry.Figures != null);
            Debug.Assert(pathGeometry.Figures.Count == 0);

            path.Data = pathGeometry;
            return pathGeometry;
        }


        /// <summary>
        /// Adds a line to a <see cref="PathGeometry"/>.
        /// </summary>
        /// <param name="pathGeometry">The <see cref="PathGeometry"/>.</param>
        /// <param name="start">The start point of the line.</param>
        /// <param name="end">The end point of the line.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="pathGeometry"/> is <see langword="null"/>.
        /// </exception>
        public static void AddLine(this PathGeometry pathGeometry, Point start, Point end)
        {
            if (pathGeometry == null)
                throw new ArgumentNullException(nameof(pathGeometry));

            var pathFigure = new PathFigure { StartPoint = start };
            var lineSegment = new LineSegment { Point = end };
            pathFigure.Segments.Add(lineSegment);
            pathGeometry.Figures.Add(pathFigure);
        }


        /// <summary>
        /// Adds a polygon to a <see cref="PathGeometry"/>.
        /// </summary>
        /// <param name="pathGeometry">The <see cref="PathGeometry"/>.</param>
        /// <param name="points">The points on the polygon.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="pathGeometry"/> is <see langword="null"/>.
        /// </exception>
        public static void AddPolygon(this PathGeometry pathGeometry, params Point[] points)
        {
            if (pathGeometry == null)
                throw new ArgumentNullException(nameof(pathGeometry));

            if (points == null || points.Length <= 1)
                return;

            var pathFigure = new PathFigure { StartPoint = points[0] };
            for (int i = 1; i < points.Length; i++)
            {
                var lineSegment = new LineSegment { Point = points[i] };
                pathFigure.Segments.Add(lineSegment);
            }

            pathGeometry.Figures.Add(pathFigure);
        }


#if !SILVERLIGHT && !WINDOWS_PHONE
        /// <summary>
        /// Assigns a new <see cref="StreamGeometry"/> to the given <see cref="Path"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The assigned <see cref="StreamGeometry"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        public static StreamGeometry SetStreamGeometry(this Path path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var streamGeometry = new StreamGeometry();
            path.Data = streamGeometry;
            return streamGeometry;
        }


        /// <summary>
        /// Adds a line to a <see cref="StreamGeometryContext"/>.
        /// </summary>
        /// <param name="streamGeometryContext">The <see cref="StreamGeometryContext"/>.</param>
        /// <param name="start">The start point of the line.</param>
        /// <param name="end">The end point of the line.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="streamGeometryContext"/> is <see langword="null"/>.
        /// </exception>
        public static void AddLine(this StreamGeometryContext streamGeometryContext, Point start, Point end)
        {
            if (streamGeometryContext == null)
                throw new ArgumentNullException(nameof(streamGeometryContext));

            streamGeometryContext.BeginFigure(start, false, false);
            streamGeometryContext.LineTo(end, true, false);
        }


        /// <summary>
        /// Adds a polygon to a <see cref="StreamGeometryContext"/>.
        /// </summary>
        /// <param name="streamGeometryContext">The <see cref="StreamGeometryContext"/>.</param>
        /// <param name="points">The points on the polygon.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="streamGeometryContext"/> is <see langword="null"/>.
        /// </exception>
        public static void AddPolygon(this StreamGeometryContext streamGeometryContext, params Point[] points)
        {
            if (streamGeometryContext == null)
                throw new ArgumentNullException(nameof(streamGeometryContext));

            if (points == null || points.Length <= 1)
                return;

            streamGeometryContext.BeginFigure(points[0], true, true);
            for (int i = 1; i < points.Length; i++)
            {
                streamGeometryContext.LineTo(points[i], true, false);
            }
        }
#endif


#if SILVERLIGHT || WINDOWS_PHONE
        /// <summary>
        /// Removes all <see cref="PathFigure"/> objects from this <see cref="PathGeometry"/>. 
        /// </summary>
        /// <param name="pathGeometry">The <see cref="PathGeometry"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="pathGeometry"/> is <see langword="null"/>.
        /// </exception>
        public static void Clear(this PathGeometry pathGeometry)
        {
            if (pathGeometry == null)
                throw new ArgumentNullException(nameof(pathGeometry));

            PathFigureCollection figures = pathGeometry.Figures;
            if (figures != null)
            {
                figures.Clear();
            }

            // Reassign figures collection - otherwise Silverlight won't update the visual.
            pathGeometry.Figures = figures;
        }
#endif
    }
}
