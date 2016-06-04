// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents an item in a <see cref="Legend"/> or <see cref="PieChartLegend"/>.
    /// </summary>
    public class LegendItem : Control
    {
        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="Symbol"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
            "Symbol",
            typeof(object),
            typeof(LegendItem),
            new PropertyMetadata((object)null));

        /// <summary>
        /// Gets or sets the legend symbol.
        /// This is a dependency property.
        /// </summary>
        /// <value>The legend symbol.</value>
        [Description("Gets or sets the legend symbol.")]
        [Category(Categories.Default)]
        public object Symbol
        {
            get { return GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Label"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label",
            typeof(object),
            typeof(LegendItem),
            new PropertyMetadata((object)null));

        /// <summary>
        /// Gets or sets the label.
        /// This is a dependency property.
        /// </summary>
        /// <value>The label.</value>
        [Description("Gets or sets the label.")]
        [Category(Categories.Default)]
        public object Label
        {
            get { return GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="LabelTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelTemplateProperty = DependencyProperty.Register(
          "LabelTemplate",
          typeof(DataTemplate),
          typeof(LegendItem),
          new PropertyMetadata((DataTemplate)null));

        /// <summary>
        /// Gets or sets the data template that is applied to the label.
        /// This is a dependency property.
        /// </summary>
        /// <value>The data template that is applied to the label.</value>
        [Description("Gets or sets the data template that is applied to the label.")]
        [Category(Categories.Default)]
        public DataTemplate LabelTemplate
        {
            get { return (DataTemplate)GetValue(LabelTemplateProperty); }
            set { SetValue(LabelTemplateProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="LegendItem"/> class.
        /// </summary>
        public LegendItem()
        {
            DefaultStyleKey = typeof(LegendItem);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="LegendItem"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static LegendItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LegendItem), new FrameworkPropertyMetadata(typeof(LegendItem)));
        }
#endif
        #endregion
    }
}
