// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Extracts <see cref="Double"/> values from a given collection.
    /// </summary>
    internal class DoubleValueExtractor
#if SILVERLIGHT
        : FrameworkElement
#else
        : Freezable
#endif
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private Dictionary<string, double> _textLabelDictionary;
        private double _nextLabelValue;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        private Dictionary<string, double> TextLabelDictionary
        {
            get
            {
                Debug.Assert(TextLabels != null, "Getter of TextLabelDictionary should only be called when TextLabels is set.");

                if (_textLabelDictionary == null)
                {
                    _textLabelDictionary = new Dictionary<string, double>();
                    foreach (var textLabel in TextLabels)
                    {
                        _nextLabelValue = Math.Max(_nextLabelValue, textLabel.Value);
                        _textLabelDictionary.Add(textLabel.Text, textLabel.Value);
                    }
                    _nextLabelValue++;
                }

                return _textLabelDictionary;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="Collection"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CollectionProperty = DependencyProperty.Register(
            "Collection",
            typeof(IEnumerable),
            typeof(DoubleValueExtractor),
            new PropertyMetadata((IEnumerable)null));

        /// <summary>
        /// Gets or sets the collection from which the values should be extracted.
        /// This is a dependency property.
        /// </summary>
        /// <value>The collection. The default value is <see langword="null"/>.</value>
        [Description("Gets or sets the collection from which the values should be extracted.")]
        [Category(ChartCategories.Default)]
        public IEnumerable Collection
        {
            get { return (IEnumerable)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Culture"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CultureProperty = DependencyProperty.Register(
            "Culture",
            typeof(CultureInfo),
            typeof(DoubleValueExtractor),
            new PropertyMetadata((CultureInfo)null));

        /// <summary>
        /// Gets or sets culture-specific formatting information.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The <see cref="CultureInfo"/> object that provides culture-specific formatting
        /// information.
        /// </value>
        [Description("Gets or sets culture-specific formatting information.")]
        [Category(ChartCategories.Default)]
        public CultureInfo Culture
        {
            get { return (CultureInfo)GetValue(CultureProperty); }
            set { SetValue(CultureProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="TextLabels"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextLabelsProperty = DependencyProperty.Register(
            "TextLabels",
            typeof(IList<TextLabel>),
            typeof(DoubleValueExtractor),
            new PropertyMetadata((IList<TextLabel>)null));

        /// <summary>
        /// Gets or sets a collection of <see cref="TextLabel"/>s.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The collection of <see cref="TextLabel"/> objects. The default value is
        /// <see langword="null"/>.
        /// </value>
        [Description("Gets or sets a collection of text labels.")]
        [Category(ChartCategories.Default)]
        public IList<TextLabel> TextLabels
        {
            get { return (IList<TextLabel>)GetValue(TextLabelsProperty); }
            set { SetValue(TextLabelsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ValuePath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValuePathProperty = DependencyProperty.Register(
            "ValuePath",
            typeof(PropertyPath),
            typeof(DoubleValueExtractor),
            new PropertyMetadata((IList<TextLabel>)null));

        /// <summary>
        /// Gets or sets the binding path for the item property that contains the
        /// <see cref="Double"/> value. This is a dependency property.
        /// </summary>
        /// <value>
        /// The binding path for the <see cref="Double"/> value. The default value is
        /// <see langword="null"/>.
        /// </value>
        [Description("Gets or sets the binding path for the item property that contains the Double value.")]
        [Category(ChartCategories.Default)]
        public PropertyPath ValuePath
        {
            get { return (PropertyPath)GetValue(ValuePathProperty); }
            set { SetValue(ValuePathProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ValueHolder"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueHolderProperty = DependencyProperty.Register(
            "ValueHolder",
            typeof(object),
            typeof(DoubleValueExtractor),
            new PropertyMetadata(Boxed.DoubleNaN));

        /// <summary>
        /// Gets or sets the value (internal use only).
        /// </summary>
        [Browsable(false)]
        public object ValueHolder
        {
            get { return GetValue(ValueHolderProperty); }
            set { SetValue(ValueHolderProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

#if !SILVERLIGHT
        /// <summary>
        /// When implemented in a derived class, creates a new instance of the <see cref="Freezable"/> 
        /// derived class.
        /// </summary>
        /// <returns>The new instance.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new DoubleValueExtractor();
        }
#endif

        /// <summary>
        /// Extracts <see cref="Double"/> values from the collection.
        /// </summary>
        /// <returns>The collection of <see cref="Double"/> values.</returns>
        public List<double> Extract()
        {
            if (ValuePath == null)
                ValuePath = new PropertyPath(String.Empty);

            var points = new List<double>();
            foreach (object data in Collection)
            {
                CreateBinding(data);
                double value = ConvertToDouble(ValueHolder);
                points.Add(value);
            }

            return points;
        }


        private void CreateBinding(object data)
        {
#if SILVERLIGHT
            var binding = new Binding
            {
                Source = data,
                Path = ValuePath
            };

            var culture = Culture;
            if (culture != null)
                binding.ConverterCulture = culture;

            SetBinding(ValueHolderProperty, binding);
#else
            var binding = new Binding
            {
                Source = data
            };

            var culture = Culture;
            if (culture != null)
                binding.ConverterCulture = culture;

            if (data is System.Xml.XmlNode)
                binding.XPath = ValuePath.Path;
            else
                binding.Path = ValuePath;

            BindingOperations.SetBinding(this, ValueHolderProperty, binding);
#endif
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DateTime")]
        private double ConvertToDouble(object obj)
        {
            if (obj is double)
                return (double)obj;

            if (obj is DateTime)
                return ((DateTime)obj).Ticks;

            if (obj is string)
            {
                string valueString = (string)obj;

                // First, check whether string contains a date/time.
                DateTime dateTime;
                if (Culture != null)
                {
                    if (DateTime.TryParse(valueString, Culture, DateTimeStyles.None, out dateTime))
                        return dateTime.Ticks;
                }
                else
                {
                    if (DateTime.TryParse(valueString, out dateTime))
                        return dateTime.Ticks;
                }

                // Next, check whether string contains a double value.
                double value;
                if (Culture != null)
                {
                    if (Double.TryParse(valueString, NumberStyles.Any, Culture, out value))
                        return value;
                }
                else
                {
                    if (Double.TryParse(valueString, out value))
                        return value;
                }

                // Last, assume that string is a text label.
                if (TextLabels != null)
                {
                    // Check whether the collection of text labels contain the string.
                    if (TextLabelDictionary.ContainsKey(valueString))
                        return TextLabelDictionary[valueString];

                    // The collection of text labels does not contain the string.
                    // --> Add the string.
                    if (TextLabels.IsReadOnly)
                        throw new ChartDataException("Data source contains unspecified text labels. The text label cannot be added automatically because the collection of text labels is read-only.");

                    value = _nextLabelValue;
                    TextLabelDictionary.Add(valueString, value);
                    TextLabels.Add(new TextLabel(value, valueString));
                    _nextLabelValue++;

                    return value;
                }
            }

            if (obj is IConvertible)
            {
                // Last try, use System.Convert.
                if (Culture != null)
                    return Convert.ToDouble(obj, Culture);
                else
                    return Convert.ToDouble(obj, CultureInfo.InvariantCulture);
            }

            throw new ChartDataException("Unable to convert data to Double, DateTime, String. Value is either null, an unsupported type or the binding has failed.");
        }
        #endregion
    }
}
