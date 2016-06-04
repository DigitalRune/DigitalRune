// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows.Markup;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// A scale that displays text labels.
    /// </summary>
    /// <remarks>
    /// The values on the scale can be associated with text labels (see <see cref="Labels"/>).
    /// The text labels are drawn instead of data values.
    /// </remarks>
    [ContentProperty("Labels")]
    public class TextScale : AxisScale
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------   
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a collection of text labels.
        /// </summary>
        /// <value>
        /// The collection of labels. The default value is empty <see cref="TextLabelCollection"/>.
        /// </value>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public TextLabelCollection Labels
        {
            get { return _labels; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (_labels == value)
                    return;

                if (_labels != null)
                    _labels.CollectionChanged -= OnLabelsCollectionChanged;

                _labels = value;
                _labels.CollectionChanged += OnLabelsCollectionChanged;

                OnPropertyChanged("Labels");
            }
        }
        private TextLabelCollection _labels;


        /// <summary>
        /// Gets or sets a value indicating whether major ticks are drawn between labels, rather
        /// than at
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the major ticks are drawn between labels; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        public bool TicksBetweenLabels
        {
            get { return _ticksBetweenLabels; }
            set
            {
                if (_ticksBetweenLabels == value)
                    return;

                _ticksBetweenLabels = value;
                OnPropertyChanged("TicksBetweenLabels");
            }
        }
        private bool _ticksBetweenLabels;
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="TextScale"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="TextScale"/> class.
        /// </summary>
        public TextScale()
            : this(0, 1)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TextScale"/> class with the given range.
        /// </summary>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TextScale(double min, double max)
            : base(min, max)
        {
            Labels = new TextLabelCollection();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the contents of the <see cref="Labels"/> collection changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.
        /// </param>
        private void OnLabelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            OnPropertyChanged("Labels");
        }


        /// <summary>
        /// Computes the major and minor tick values.
        /// </summary>
        /// <param name="axisLength">Length of the axis in device-independent pixels.</param>
        /// <param name="minDistance">
        /// The minimum distance between major ticks in device-independent pixels.
        /// </param>
        /// <param name="labelValues">The data values at which labels should be drawn.</param>
        /// <param name="majorTicks">The data values at which major ticks should be drawn.</param>
        /// <param name="minorTicks">The data values at which minor ticks should be drawn.</param>
        /// <exception cref="ChartException">
        /// <see cref="AxisScale.Min"/> and <see cref="AxisScale.Max"/> have invalid values.
        /// </exception>
        public override void ComputeTicks(double axisLength,
                                          double minDistance,
                                          out double[] labelValues,
                                          out double[] majorTicks,
                                          out double[] minorTicks)
        {
            if (Numeric.IsZero(axisLength))
            {
                // The size of the axis is 0.
                // Do nothing.
                labelValues = majorTicks = minorTicks = new double[0];
                return;
            }

            // Check Min and Max.
            if (Numeric.IsNaN(Min) || Numeric.IsNaN(Max) || Min > Max)
                throw new ChartException("Properties Min and Max of the scale have invalid values.");

            if (Numeric.AreEqual(Min, Max))
                throw new ChartException("Properties Min and Max of the scale have invalid values (nearly identical).");

            var internalMajorTicks = new List<double>();

            if (!TicksBetweenLabels)
            {
                // Ticks correspond to position of labels.
                for (int i = 0; i < Labels.Count; ++i)
                {
                    double value = Labels[i].Value;
                    if (Min <= value && value <= Max)
                        internalMajorTicks.Add(value);
                }

                internalMajorTicks.Sort();
            }
            else
            {
                // Ticks correspond to gaps between text 
                // Sort data.
                var sortedValues = new List<double>(Labels.Count);
                foreach (TextLabel label in Labels)
                    sortedValues.Add(label.Value);

                sortedValues.Sort();

                for (int i = 1; i < Labels.Count; ++i)
                {
                    double worldPosition = (sortedValues[i] + sortedValues[i - 1]) / 2.0;
                    if (Min <= worldPosition && worldPosition <= Max)
                        internalMajorTicks.Add(worldPosition);
                }
            }

            majorTicks = internalMajorTicks.ToArray();
            minorTicks = new double[0];
            labelValues = Labels.Select(label => label.Value)
                                .Where(value => Min <= value && value <= Max)
                                .OrderBy(value => value)
                                .ToArray();
        }


        /// <summary>
        /// Gets the label text (short text) for a specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="cultureInfo">
        /// The <see cref="CultureInfo"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>The label text.</returns>
        /// <remarks>
        /// Per default, the label text is equal to the string representation of the
        /// <paramref name="value"/>. This behavior can be changed in derived classes.
        /// </remarks>
        public override string GetText(double value, CultureInfo cultureInfo)
        {
            for (int i = 0; i < Labels.Count; i++)
                if (Numeric.AreEqual(value, Labels[i].Value))
                    return Labels[i].Text;

            return String.Empty;
        }


        /// <summary>
        /// Gets the description of a label.
        /// </summary>
        /// <param name="value">The data value of the label.</param>
        /// <returns>
        /// The description for this label or <see cref="String.Empty"/> if there is no text for the
        /// given data value.
        /// </returns>
        public string GetDescription(double value)
        {
            for (int i = 0; i < Labels.Count; i++)
                if (Numeric.AreEqual(value, Labels[i].Value))
                    return Labels[i].Description;

            return String.Empty;
        }
        #endregion
    }
}
