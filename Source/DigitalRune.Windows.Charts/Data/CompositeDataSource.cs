// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Combines two separate collections for use as a single data source.
    /// </summary>
    /// <remarks>
    /// The properties <see cref="XValues"/> and <see cref="YValues"/> define the collections that
    /// contain the x and y values. These collections may contain any type that is convertible to
    /// <see cref="double"/>. It can also contain complex data types: In this case the properties
    /// <see cref="XValuePath"/> and <see cref="YValuePath"/> need to be set to indicate where the
    /// actual x or y values are stored. The x and y values will be retrieved using data binding.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class CompositeDataSource : IEnumerable<DataPoint>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets culture-specific formatting information.
        /// </summary>
        /// <value>
        /// The <see cref="CultureInfo"/> object that provides culture-specific formatting
        /// information.
        /// </value>
        public CultureInfo Culture { get; set; }


        /// <summary>
        /// Gets or sets the collection containing the x values.
        /// </summary>
        /// <value>The collection containing the x values.</value>
        public IEnumerable XValues { get; set; }


        /// <summary>
        /// Gets or sets the collection containing the y values.
        /// </summary>
        /// <value>The collection containing the y values.</value>
        public IEnumerable YValues { get; set; }


        /// <summary>
        /// Gets or sets the binding path for the x values.
        /// </summary>
        /// <value>
        /// The binding path for the x values.</value>
        public PropertyPath XValuePath { get; set; }


        /// <summary>
        /// Gets or sets the binding path for the y values.
        /// </summary>
        /// <value>
        /// The binding path for the y values.</value>
        public PropertyPath YValuePath { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeDataSource"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeDataSource"/> class.
        /// </summary>
        public CompositeDataSource()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeDataSource"/> class with two 
        /// collections.
        /// </summary>
        /// <param name="xValues">The x values.</param>
        /// <param name="yValues">The y values.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public CompositeDataSource(IEnumerable xValues, IEnumerable yValues)
        {
            XValues = xValues;
            YValues = yValues;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Converts the composite data source to a list of data points.
        /// </summary>
        /// <returns>The list of data points.</returns>
        /// <exception cref="InvalidOperationException">
        /// Either <see cref="XValues"/> or <see cref="YValues"/> is not yet set.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public IList<DataPoint> ToChartDataSource()
        {
            if (XValues == null)
                throw new InvalidOperationException("XValues is missing.");
            if (YValues == null)
                throw new InvalidOperationException("XValues is missing");

            // Extract x values from XValues collection.
            var xValueExtractor = new DoubleValueExtractor
            {
                Collection = XValues,
                Culture = Culture,
                ValuePath = XValuePath,
            };
            var xValues = xValueExtractor.Extract();

            // Extract y values from YValues collection.
            var yValueExtractor = new DoubleValueExtractor
            {
                Collection = YValues,
                Culture = Culture,
                ValuePath = YValuePath,
            };
            var yValues = yValueExtractor.Extract();

            Debug.Assert(xValues.Count == yValues.Count, "Number of x values in data source does not match the number of y values?!");

            var chartDataSource = new DataPointCollection();
            int index = 0;
            int numberOfDataPoints = Math.Min(xValues.Count, yValues.Count);
            var xValuesEnumerator = XValues.GetEnumerator();
            var yValuesEnumerator = YValues.GetEnumerator();
            while (index < numberOfDataPoints)
            {
                xValuesEnumerator.MoveNext();
                yValuesEnumerator.MoveNext();
                var dataPoint = new DataPoint
                {
                    X = xValues[index],
                    Y = yValues[index],
                    DataContext = new CompositeData(xValuesEnumerator.Current, yValuesEnumerator.Current)
                };
                chartDataSource.Add(dataPoint);
                index++;
            }

            return chartDataSource;
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<DataPoint> GetEnumerator()
        {
            return ToChartDataSource().GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}