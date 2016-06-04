// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a view that displays data items similar to the Windows Explorer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="ExplorerView"/> supports different modes - similar to Microsoft's Windows
    /// Explorer. The different view modes can be selected by setting <see cref="Mode"/>.
    /// </para>
    /// <para>
    /// The default mode is <see cref="ExplorerViewMode.Details"/> which looks and functions like
    /// a <see cref="GridView"/> (<see cref="ExplorerView"/> is derived from
    /// <see cref="GridView"/>).
    /// </para>
    /// <para>
    /// The view modes are:
    /// <list type="bullet">
    /// <listheader><term>Mode</term><description>Remarks</description></listheader>
    /// <item>
    /// <term><see cref="ExplorerViewMode.Details"/></term>
    /// <description>Default view mode.</description>
    /// </item>
    /// <item>
    /// <term><see cref="ExplorerViewMode.List"/></term>
    /// <description>
    /// The data items are presented in columns (top-to-bottom). The items panel scrolls
    /// horizontally if necessary. The properties <see cref="ListTemplate"/> and
    /// <see cref="ListTemplateSelector"/> define how the data item is rendered.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="ExplorerViewMode.Tiles"/></term>
    /// <description>
    /// The data items are presented in rows (left-to-right). The items panel scrolls vertically if
    /// necessary. The properties <see cref="TileTemplate"/> and <see cref="TileTemplateSelector"/>
    /// define how the data item is rendered.
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// <see cref="ExplorerViewMode.SmallIcons"/>, <see cref="ExplorerViewMode.MediumIcons"/>,
    /// <see cref="ExplorerViewMode.LargeIcons"/>, <see cref="ExplorerViewMode.ExtraLargeIcons"/>
    /// </term>
    /// <description>
    /// The data items are presented in rows (left-to-right). The items panel scrolls vertically if
    /// necessary. The properties <see cref="IconTemplate"/> and <see cref="IconTemplateSelector"/>
    /// define how the data item is rendered. The property <see cref="Scale"/> defines the size of
    /// the item. (In the data template the size of the item needs to be bound to
    /// <see cref="Scale"/>.)
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// Each <see cref="ExplorerView"/> owns a <see cref="Menu"/> that can be used to control
    /// <see cref="Mode"/> and <see cref="Scale"/>.
    /// </para>
    /// </remarks>
    public class ExplorerView : GridView
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private bool _updating;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the object that is associated with the style for the view mode.
        /// </summary>
        /// <returns>
        /// The style to use for the view mode.
        /// </returns>
        protected override object DefaultStyleKey
        {
            get
            {
                if (_defaultStyleKey == null)
                    _defaultStyleKey = new ComponentResourceKey(typeof(ExplorerView), "ExplorerViewStyle");

                return _defaultStyleKey;
            }
        }
        private static ComponentResourceKey _defaultStyleKey;


        /// <summary>
        /// Gets the style to use for the items in the view mode.
        /// </summary>
        /// <returns>
        /// The style to use for item containers.
        /// </returns>
        protected override object ItemContainerDefaultStyleKey
        {
            get
            {
                if (_itemContainerDefaultStyleKey == null)
                    _itemContainerDefaultStyleKey = new ComponentResourceKey(typeof(ExplorerView), "ExplorerViewItemContainerStyle");

                return _itemContainerDefaultStyleKey;
            }
        }
        private static ComponentResourceKey _itemContainerDefaultStyleKey;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="IconTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IconTemplateProperty = DependencyProperty.Register(
            "IconTemplate",
            typeof(DataTemplate),
            typeof(ExplorerView));

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> used to display each item when
        /// <see cref="Mode"/> is set to <see cref="ExplorerViewMode.SmallIcons"/>,
        /// <see cref="ExplorerViewMode.MediumIcons"/>, <see cref="ExplorerViewMode.LargeIcons"/>,
        /// or <see cref="ExplorerViewMode.ExtraLargeIcons"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// A <see cref="DataTemplate"/> that specifies the visualization of the data objects. The
        /// default value is <see langword="null"/>.
        /// </value>
        [Description("Gets or sets the data template that is applied when the view shows icons.")]
        [Category(Categories.Default)]
        public DataTemplate IconTemplate
        {
            get { return (DataTemplate)GetValue(IconTemplateProperty); }
            set { SetValue(IconTemplateProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="IconTemplateSelector"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IconTemplateSelectorProperty = DependencyProperty.Register(
            "IconTemplateSelector",
            typeof(DataTemplateSelector),
            typeof(ExplorerView));

        /// <summary>
        /// Gets or sets the custom logic for choosing a template used to display each item when
        /// <see cref="Mode"/> is set to <see cref="ExplorerViewMode.SmallIcons"/>,
        /// <see cref="ExplorerViewMode.MediumIcons"/>, <see cref="ExplorerViewMode.LargeIcons"/>,
        /// or <see cref="ExplorerViewMode.ExtraLargeIcons"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// A custom <see cref="DataTemplateSelector"/> object that provides logic and returns a
        /// <see cref="DataTemplate"/>. The default is <see langword="null"/>.
        /// </value>
        [Description("Gets or sets the data template selector that is applied when the view shows icons.")]
        [Category(Categories.Default)]
        public DataTemplateSelector IconTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(IconTemplateSelectorProperty); }
            set { SetValue(IconTemplateSelectorProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ListTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ListTemplateProperty = DependencyProperty.Register(
            "ListTemplate",
            typeof(DataTemplate),
            typeof(ExplorerView));

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> used to display each item when
        /// <see cref="Mode"/> is set to <see cref="ExplorerViewMode.List"/>.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A <see cref="DataTemplate"/> that specifies the visualization of the data objects. The
        /// default value is <see langword="null"/>.
        /// </value>
        [Description("Gets or sets the data template that is applied when the view shows details.")]
        [Category(Categories.Default)]
        public DataTemplate ListTemplate
        {
            get { return (DataTemplate)GetValue(ListTemplateProperty); }
            set { SetValue(ListTemplateProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ListTemplateSelector"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ListTemplateSelectorProperty = DependencyProperty.Register(
            "ListTemplateSelector",
            typeof(DataTemplateSelector),
            typeof(ExplorerView));

        /// <summary>
        /// Gets or sets the custom logic for choosing a template used to display each item when
        /// <see cref="Mode"/> is set to <see cref="ExplorerViewMode.List"/>.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A custom <see cref="DataTemplateSelector"/> object that provides logic and returns a
        /// <see cref="DataTemplate"/>. The default is <see langword="null"/>.
        /// </value>
        [Description("Gets or sets the data template selector that is applied when the view shows details.")]
        [Category(Categories.Default)]
        public DataTemplateSelector ListTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ListTemplateSelectorProperty); }
            set { SetValue(ListTemplateSelectorProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="TileTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TileTemplateProperty = DependencyProperty.Register(
            "TileTemplate",
            typeof(DataTemplate),
            typeof(ExplorerView));

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> used to display each item when
        /// <see cref="Mode"/> is set to <see cref="ExplorerViewMode.Tiles"/>.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A <see cref="DataTemplate"/> that specifies the visualization of the data objects. The
        /// default value is <see langword="null"/>.
        /// </value>
        [Description("Gets or sets the data template that is applied when the view shows tiles.")]
        [Category(Categories.Default)]
        public DataTemplate TileTemplate
        {
            get { return (DataTemplate)GetValue(TileTemplateProperty); }
            set { SetValue(TileTemplateProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="TileTemplateSelector"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TileTemplateSelectorProperty = DependencyProperty.Register(
            "TileTemplateSelector",
            typeof(DataTemplateSelector),
            typeof(ExplorerView));

        /// <summary>
        /// Gets or sets the custom logic for choosing a template used to display each item when
        /// <see cref="Mode"/> is set to <see cref="ExplorerViewMode.Tiles"/>.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A custom <see cref="DataTemplateSelector"/> object that provides logic and returns a
        /// <see cref="DataTemplate"/>. The default is <see langword="null"/>.
        /// </value>
        [Description("Gets or sets the data template selector that is applied when the view shows a tiles.")]
        [Category(Categories.Default)]
        public DataTemplateSelector TileTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(TileTemplateSelectorProperty); }
            set { SetValue(TileTemplateSelectorProperty, value); }
        }


        private static readonly DependencyPropertyKey IconPropertyKey = DependencyProperty.RegisterReadOnly(
            "Icon",
            typeof(object),
            typeof(ExplorerView),
            new FrameworkPropertyMetadata((object)null));

        /// <summary>
        /// Identifies the <see cref="Icon"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty = IconPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets an icon (<see cref="ImageSource"/>  or <see cref="MultiColorGlyph"/>) representing
        /// the current view mode. This is a dependency property.
        /// </summary>
        /// <value>The icon representing the current view mode.</value>
        [Description("Gets the icon that represents the current view mode.")]
        [Category(Categories.Default)]
        public object Icon
        {
            get { return GetValue(IconProperty); }
            private set { SetValue(IconPropertyKey, value); }
        }


        private static readonly DependencyPropertyKey MenuPropertyKey = DependencyProperty.RegisterReadOnly(
            "Menu",
            typeof(ExplorerViewMenu),
            typeof(ExplorerView),
            new FrameworkPropertyMetadata((ExplorerViewMenu)null));

        /// <summary> 
        /// Identifies the <see cref="Menu"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MenuProperty = MenuPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets an <see cref="ExplorerViewMenu"/> that controls <see cref="Mode"/> and
        /// <see cref="Scale"/>. This is a dependency property.
        /// </summary>
        /// <value>The explorer view menu.</value>
        [Description("Gets the explorer view menu that controls the view mode.")]
        [Category(Categories.Default)]
        public ExplorerViewMenu Menu
        {
            get { return (ExplorerViewMenu)GetValue(MenuProperty); }
            private set { SetValue(MenuPropertyKey, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Mode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            "Mode",
            typeof(ExplorerViewMode),
            typeof(ExplorerView),
            new FrameworkPropertyMetadata(
                ExplorerViewMode.Details,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnModeChanged));

        /// <summary>
        /// Gets or sets view mode of the <see cref="ExplorerView"/>.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The current <see cref="ExplorerViewMode"/>. The default value is
        /// <see cref="ExplorerViewMode.LargeIcons"/>.
        /// </value>
        [Description("Gets or sets current view mode.")]
        [Category(Categories.Default)]
        public ExplorerViewMode Mode
        {
            get { return (ExplorerViewMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Scale"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale",
            typeof(double),
            typeof(ExplorerView),
            new FrameworkPropertyMetadata(
                Boxed.DoubleOne,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnScaleChanged));

        /// <summary>
        /// Gets or sets the scale used to display the items in the <see cref="ExplorerView"/>.
        /// This is a dependency property.
        /// </summary>
        /// <value>A value in the range [1, 16]. The default value is 1.</value>
        /// <remarks>
        /// This property is only relevant when the <see cref="Mode"/> is set to
        /// <see cref="ExplorerViewMode.SmallIcons"/>, <see cref="ExplorerViewMode.MediumIcons"/>,
        /// <see cref="ExplorerViewMode.LargeIcons"/>, or
        /// <see cref="ExplorerViewMode.ExtraLargeIcons"/>.
        /// </remarks>
        [Description("Gets or sets the scale of the current view in the range [1, 16].")]
        [Category(Categories.Default)]
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ExplorerView"/> class.
        /// </summary>
        public ExplorerView()
        {
            Menu = new ExplorerViewMenu();
            Menu.Loaded += OnMenuLoaded;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------    

        private void OnMenuLoaded(object sender, RoutedEventArgs eventArgs)
        {
            // Initialize ExplorerViewMenu with identical values, because binding should not destroy
            // our current values.
            // First 'scale', then 'mode'!
            Menu.Scale = Scale;
            Menu.Mode = Mode;

            // Bind Mode and Scale to the context menu.
            var binding = new Binding
            {
                Source = this,
                Path = new PropertyPath(ModeProperty),
                Mode = BindingMode.TwoWay
            };
            Menu.SetBinding(ExplorerViewMenu.ModeProperty, binding);

            binding = new Binding
            {
                Source = this,
                Path = new PropertyPath(ScaleProperty),
                Mode = BindingMode.TwoWay
            };
            Menu.SetBinding(ExplorerViewMenu.ScaleProperty, binding);
        }


        private static void OnModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var explorerView = (ExplorerView)dependencyObject;
            explorerView.UpdateFromMode((ExplorerViewMode)eventArgs.NewValue);
        }


        private static void OnScaleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var explorerView = (ExplorerView)dependencyObject;
            explorerView.UpdateFromScale((double)eventArgs.NewValue);
        }


        private void UpdateFromMode(ExplorerViewMode mode)
        {
            // Avoid re-entrance.
            if (_updating)
                return;

            _updating = true;

            try
            {
                SetIcon(mode);
                SetScale(mode);
                Mode = mode;
            }
            finally
            {
                _updating = false;
            }
        }


        private void UpdateFromScale(double scale)
        {
            // Avoid re-entrance.
            if (_updating)
                return;

            _updating = true;

            try
            {
                SetMode(scale);
                SetIcon(Mode);
            }
            finally
            {
                _updating = false;
            }
        }


        private void SetIcon(ExplorerViewMode mode)
        {
            switch (mode)
            {
                case ExplorerViewMode.ExtraLargeIcons:
                    Icon = Menu.ImageSourceExtraLargeIcons;
                    break;
                case ExplorerViewMode.LargeIcons:
                    Icon = Menu.ImageSourceLargeIcons;
                    break;
                case ExplorerViewMode.MediumIcons:
                    Icon = Menu.ImageSourceMediumIcons;
                    break;
                case ExplorerViewMode.SmallIcons:
                    Icon = Menu.ImageSourceSmallIcons;
                    break;
                case ExplorerViewMode.List:
                    Icon = Menu.ImageSourceList;
                    break;
                case ExplorerViewMode.Details:
                    Icon = Menu.ImageSourceDetails;
                    break;
                default:
                    Icon = Menu.ImageSourceTiles;
                    break;
            }
        }


        private void SetMode(double scale)
        {
            if (Numeric.IsGreater(scale, ExplorerViewMenu.ScaleSmall))
            {
                if (Numeric.IsLess(scale, ExplorerViewMenu.ScaleMedium))
                    Mode = ExplorerViewMode.SmallIcons;
                else if (Numeric.IsLess(scale, ExplorerViewMenu.ScaleLarge))
                    Mode = ExplorerViewMode.MediumIcons;
                else if (Numeric.IsLess(scale, ExplorerViewMenu.ScaleExtraLarge))
                    Mode = ExplorerViewMode.LargeIcons;
                else
                    Mode = ExplorerViewMode.ExtraLargeIcons;
            }
        }


        private void SetScale(ExplorerViewMode mode)
        {
            switch (mode)
            {
                case ExplorerViewMode.ExtraLargeIcons:
                    Scale = ExplorerViewMenu.ScaleExtraLarge;
                    break;
                case ExplorerViewMode.LargeIcons:
                    Scale = ExplorerViewMenu.ScaleLarge;
                    break;
                case ExplorerViewMode.MediumIcons:
                    Scale = ExplorerViewMenu.ScaleMedium;
                    break;
                case ExplorerViewMode.SmallIcons:
                    Scale = ExplorerViewMenu.ScaleSmall;
                    break;
                default:
                    // Do not change Scale in all other cases,
                    // because the Scale is irrelevant for those modes.
                    break;
            }
        }


        /// <summary>
        /// Increases the scale (switching to a different mode if necessary).
        /// </summary>
        public void IncreaseScale()
        {
            switch (Mode)
            {
                case ExplorerViewMode.Tiles:
                    Mode = ExplorerViewMode.Details;
                    break;
                case ExplorerViewMode.Details:
                    Mode = ExplorerViewMode.List;
                    break;
                case ExplorerViewMode.List:
                    Mode = ExplorerViewMode.SmallIcons;
                    Scale = 1.0;
                    break;
                case ExplorerViewMode.SmallIcons:
                case ExplorerViewMode.MediumIcons:
                case ExplorerViewMode.LargeIcons:
                    Scale += 1.0;
                    break;
            }
        }


        /// <summary>
        /// Decreases the scale (switching to a different mode if necessary).
        /// </summary>
        public void DecreaseScale()
        {
            switch (Mode)
            {
                case ExplorerViewMode.ExtraLargeIcons:
                case ExplorerViewMode.LargeIcons:
                case ExplorerViewMode.MediumIcons:
                case ExplorerViewMode.SmallIcons:
                    if (Scale == 1.0)
                        Mode = ExplorerViewMode.List;
                    else
                        Scale -= 1.0;
                    break;
                case ExplorerViewMode.List:
                    Mode = ExplorerViewMode.Details;
                    Scale = 1.0;
                    break;
                case ExplorerViewMode.Details:
                    Mode = ExplorerViewMode.Tiles;
                    break;
            }
        }
        #endregion
    }
}
