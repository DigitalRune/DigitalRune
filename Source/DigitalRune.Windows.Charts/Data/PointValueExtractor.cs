// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Extracts <see cref="Point"/> values from a given collection.
    /// </summary>
    internal class PointValueExtractor
#if SILVERLIGHT
        : FrameworkElement
#else
        : Freezable
#endif
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
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
            typeof(PointValueExtractor),
            new PropertyMetadata((IEnumerable)null));

        /// <summary>
        /// Gets or sets the collection from which the points should be extracted. 
        /// This is a dependency property.
        /// </summary>
        /// <value>The collection. The default value is <see langword="null"/>.</value>
        [Description("Gets or sets the collection from which the points should be extracted.")]
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
            typeof(PointValueExtractor),
            new PropertyMetadata((CultureInfo)null));

        /// <summary>
        /// Gets or sets culture-specific formatting information.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The <see cref="CultureInfo"/> object that provides culture-specific formatting information.
        /// </value>
        [Description("Gets or sets culture-specific formatting information.")]
        [Category(ChartCategories.Default)]
        public CultureInfo Culture
        {
            get { return (CultureInfo)GetValue(CultureProperty); }
            set { SetValue(CultureProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ValuePath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValuePathProperty = DependencyProperty.Register(
            "ValuePath",
            typeof(PropertyPath),
            typeof(PointValueExtractor),
            new PropertyMetadata((PropertyPath)null));

        /// <summary>
        /// Gets or sets the binding path for the item property that contains the
        /// <see cref="Point"/> value. This is a dependency property.
        /// </summary>
        /// <value>
        /// The binding path for the <see cref="Point"/> value. The default value is
        /// <see langword="null"/>.
        /// </value>
        [Description("Gets or sets the binding path for the item property that contains the Point value.")]
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
            typeof(Point),
            typeof(PointValueExtractor),
            new PropertyMetadata(Boxed.PointNaN));

        /// <summary>
        /// Gets or sets the <see cref="Point"/> value (internal use only).
        /// </summary>
        [Browsable(false)]
        public Point ValueHolder
        {
            get { return (Point)GetValue(ValueHolderProperty); }
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
            return new PointValueExtractor();
        }
#endif

        /// <summary>
        /// Extracts <see cref="Point"/> values from the collection.
        /// </summary>
        /// <returns>The collection of <see cref="Point"/>s.</returns>
        /// <exception cref="InvalidOperationException">
        /// The property <see cref="ValuePath"/> is not set.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ValuePath")]
        public PointCollection Extract()
        {
            if (ValuePath == null)
                throw new InvalidOperationException("Cannot extract Points. The property ValuePath needs to be set first.");

            var points = new PointCollection();
            foreach (var data in Collection)
            {
                CreateBinding(data);
                points.Add(ValueHolder);
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
        #endregion
    }
}
