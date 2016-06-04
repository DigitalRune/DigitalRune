// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Windows;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Provides functions to create a list of data points from other data sources.
    /// (For internal use only.)
    /// </summary>
#if SILVERLIGHT
    public      // Type needs to be public for Silverlight unit testing.
#else
    internal
#endif
 static class ChartDataHelper
    {
        /// <summary>
        /// Creates list of data points from an <see cref="IEnumerable"/>.
        /// </summary>
        /// <param name="dataSource">The data source (can be <see langword="null"/>).</param>
        /// <param name="xPath">The binding path for the x value.</param>
        /// <param name="yPath">The binding path for the y value.</param>
        /// <param name="xyPath">The binding path for the (x, y) value.</param>
        /// <param name="culture">
        /// The <see cref="CultureInfo"/> object that provides culture-specific formatting
        /// information.
        /// </param>
        /// <param name="xLabels">
        /// The collection of text labels used for x values. Can be <see langword="null"/>. This
        /// parameter is only relevant when one chart axis shows text labels and the data source
        /// contains <see cref="String"/> values instead of <see cref="Double"/> values.
        /// </param>
        /// <param name="yLabels">
        /// The collection of text labels used for y values. Can be <see langword="null"/>. This
        /// parameter is only relevant when one chart axis shows text labels and the data source
        /// contains <see cref="String"/> values instead of <see cref="Double"/> values.
        /// </param>
        /// <returns>
        /// The list of data points created from <paramref name="dataSource"/>. When
        /// <paramref name="dataSource"/> is <see langword="null"/> or empty, an empty list of data
        /// points is returned.
        /// </returns>
        /// <exception cref="ChartDataException">
        /// Invalid properties - cannot create data source.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public static IList<DataPoint> CreateChartDataSource(IEnumerable dataSource, PropertyPath xPath, PropertyPath yPath, PropertyPath xyPath, CultureInfo culture, IList<TextLabel> xLabels, IList<TextLabel> yLabels)
        {
            if (dataSource == null)
            {
                // No data source connected, create empty list.
                return new DataPointCollection();
            }

            if (dataSource is CompositeDataSource)
            {
                // The data source is a combination of two collections.
                var compositeDataSource = (CompositeDataSource)dataSource;

                // Forward settings if not already set.
                if (culture != null && compositeDataSource.Culture == null)
                    compositeDataSource.Culture = culture;
                if (xPath != null && compositeDataSource.XValuePath == null)
                    compositeDataSource.XValuePath = xPath;
                if (yPath != null && compositeDataSource.YValuePath == null)
                    compositeDataSource.YValuePath = yPath;

                return compositeDataSource.ToChartDataSource();
            }


            if (dataSource is IList<DataPoint>)
                return (IList<DataPoint>)dataSource;

            if (dataSource is IList<Point>)
            {
                if (dataSource is INotifyCollectionChanged)
                    return new ObservablePointListWrapper((IList<Point>)dataSource);

                return new PointListWrapper((IList<Point>)dataSource);
            }

            if (dataSource is IEnumerable<Point>)
                return new DataPointCollection((IEnumerable<Point>)dataSource);

            // We need to extract the chart data using a binding mechanism.
            // Precondition: Either xyPath or xPath and yPath should be set.
            if (xyPath == null && (xPath == null || yPath == null))
            {
                if (xPath != null)
                {
                    Debug.Assert(yPath == null, "Sanity check.");
                    throw new ChartDataException("The property YValuePath is missing.");
                }

                if (yPath != null)
                {
                    Debug.Assert(xPath == null, "Sanity check.");
                    throw new ChartDataException("The property XValuePath is missing.");
                }

                Debug.Assert(xyPath == null, "Sanity check.");
                throw new ChartDataException("A binding path needs to be set to extract the data from data source. You should either set XYValuePath or XValuePath together with YValuePath");
            }

            // Extract Point values from the collection using a binding mechanism.
            if (xyPath != null)
                return ExtractPointsFromCollection(dataSource, xyPath, culture);

            // Extract x values and y values from the collection using a binding mechanism.
            Debug.Assert(xPath != null && yPath != null);
            return ExtractPointsFromCollection(dataSource, xPath, yPath, culture, xLabels, yLabels);
        }


        /// <summary>
        /// Extracts the <see cref="Point"/>s from a collection.
        /// </summary>
        /// <param name="dataSource">The data source (can be <see langword="null"/>).</param>
        /// <param name="xyPath">The binding path for the (x, y) value.</param>
        /// <param name="culture">
        /// The <see cref="CultureInfo"/> object that provides culture-specific formatting
        /// information.
        /// </param>
        /// <returns>The chart data as a list of data points.</returns>
        private static IList<DataPoint> ExtractPointsFromCollection(IEnumerable dataSource, PropertyPath xyPath, CultureInfo culture)
        {
            var pointValueExtractor = new PointValueExtractor
            {
                Collection = dataSource,
                Culture = culture,
                ValuePath = xyPath,
            };
            var points = pointValueExtractor.Extract();

            int index = 0;
            var chartDataSource = new DataPointCollection();
            foreach (var data in dataSource)
            {
                chartDataSource.Add(new DataPoint(points[index], data));
                index++;
            }

            return chartDataSource;
        }


        /// <summary>
        /// Extracts the points from collection.
        /// </summary>
        /// <param name="dataSource">The data source (can be <see langword="null"/>).</param>
        /// <param name="xPath">The binding path for the x value.</param>
        /// <param name="yPath">The binding path for the y value.</param>
        /// <param name="xLabels">
        /// The collection of text labels used for x values. Can be <see langword="null"/>. This
        /// parameter is only relevant when one chart axis shows text labels and the data source
        /// contains <see cref="String"/> values instead of <see cref="Double"/> values.
        /// </param>
        /// <param name="yLabels">
        /// The collection of text labels used for y values. Can be <see langword="null"/>. This
        /// parameter is only relevant when one chart axis shows text labels and the data source
        /// contains <see cref="String"/> values instead of <see cref="Double"/> values.
        /// </param>
        /// <param name="culture">
        /// The <see cref="CultureInfo"/> object that provides culture-specific formatting
        /// information.
        /// </param>
        /// <returns>The chart data as a list of data points.</returns>
        private static IList<DataPoint> ExtractPointsFromCollection(
            IEnumerable dataSource, PropertyPath xPath, PropertyPath yPath, CultureInfo culture, 
            IList<TextLabel> xLabels, IList<TextLabel> yLabels)
        {
            var xValueExtractor = new DoubleValueExtractor
            {
                Collection = dataSource,
                Culture = culture,
                TextLabels = xLabels,
                ValuePath = xPath,
            };
            var xValues = xValueExtractor.Extract();

            var yValueExtractor = new DoubleValueExtractor
            {
                Collection = dataSource,
                Culture = culture,
                TextLabels = yLabels,
                ValuePath = yPath,
            };
            var yValues = yValueExtractor.Extract();

            Debug.Assert(xValues.Count == yValues.Count, "Number of x values in data source does not match the number of y values?!");

            int index = 0;
            var chartDataSource = new DataPointCollection();
            foreach (var data in dataSource)
            {
                chartDataSource.Add(new DataPoint(xValues[index], yValues[index], data));
                index++;
            }

            return chartDataSource;
        }
    }
}
