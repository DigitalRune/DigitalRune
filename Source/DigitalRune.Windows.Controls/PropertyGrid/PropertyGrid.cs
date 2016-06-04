// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a control that provides a user interface for browsing the properties of an
    /// object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The properties that shall be displayed in the <see cref="PropertyGrid"/> can be set using
    /// the property <see cref="PropertySource"/>. The helper class <see cref="PropertyGridHelper"/>
    /// provides methods to create a property sources for any CLR object.
    /// </para>
    /// <para>
    /// When <see cref="IsCategorized"/> is <see langword="true"/>, the keyboard LEFT and RIGHT keys
    /// can be used to collapse/expand categories. When SHIFT is pressed, the LEFT and RIGHT keys
    /// collapse/expand ALL categories at the same time.
    /// </para>
    /// </remarks>
    [TemplatePart(Name = "PART_Thumb", Type = typeof(Thumb))]       // The thumb that determines the size of the name column.
    [TemplatePart(Name = "PART_ListBox", Type = typeof(ListBox))]
    public class PropertyGrid : Control
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private Thumb _thumb;
        private ListBox _listBox;

        private DispatcherTimer _timer;

        // True if the collapse or expand operation was triggered in explicitly in code.
        private bool _isManualCollapseOrExpand;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="PropertySource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PropertySourceProperty = DependencyProperty.Register(
            "PropertySource",
            typeof(IPropertySource),
            typeof(PropertyGrid),
            new FrameworkPropertyMetadata(null, OnPropertiesSourceChanged));

        /// <summary>
        /// Gets or sets the object that provides the properties.
        /// This is a dependency property.
        /// </summary>
        /// <value>The object that provides the properties.</value>
        [Description("Gets or sets object that provides the properties.")]
        [Category(Categories.Default)]
        public IPropertySource PropertySource
        {
            get { return (IPropertySource)GetValue(PropertySourceProperty); }
            set { SetValue(PropertySourceProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Properties"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register(
            "Properties",
            typeof(IEnumerable),
            typeof(PropertyGrid),
            new FrameworkPropertyMetadata(null, OnPropertiesChanged));

        /// <summary>
        /// Gets or sets the properties to be shown.
        /// This is a dependency property.
        /// </summary>
        /// <value>The properties to be shown.</value>
        [Description("Gets or sets the properties to be shown.")]
        [Category(Categories.Default)]
        public IEnumerable Properties
        {
            get { return (IEnumerable)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Delay"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register(
            "Delay",
            typeof(int),
            typeof(PropertyGrid),
            new FrameworkPropertyMetadata(Boxed.Int32Zero));

        /// <summary>
        /// Gets or sets the amount of time, in milliseconds, to wait before updating the
        /// <see cref="Properties"/> after the <see cref="PropertySource"/> changed.
        /// This is a dependency property.
        /// </summary>
        /// <value>The delay before showing the new properties. The default value is 0 ms.</value>
        /// <remarks>
        /// Frequently updating the property source may slow down the user interface. To improve the
        /// responsiveness of the application, the update of the properties can be delayed. The
        /// property <see cref="IsLoading"/> is <see langword="true"/> during the delay.
        /// </remarks>
        [Description("Gets or sets the amount of time, in milliseconds, to wait before updating the Properties after the PropertySource changed.")]
        [Category(Categories.Behavior)]
        public int Delay
        {
            get { return (int)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }


        private static readonly DependencyPropertyKey IsLoadingPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsLoading",
            typeof(bool),
            typeof(PropertyGrid),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse));

        /// <summary>
        /// Identifies the <see cref="IsLoading"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLoadingProperty = IsLoadingPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a value indicating whether control is busy loading the new properties.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the control is busy loading the new properties; otherwise,
        /// <see langword="false"/>.
        /// </value>
        [Browsable(false)]
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            private set { SetValue(IsLoadingPropertyKey, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="NameColumnWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register(
            "NameColumnWidth",
            typeof(double),
            typeof(PropertyGrid),
            new FrameworkPropertyMetadata(150.0));

        /// <summary>
        /// Gets or sets the width of the name column. (To be bound in the control template.)
        /// This is a dependency property.
        /// </summary>
        /// <value>The width of the name column. The default value is 150.</value>
        [Description("Gets or sets the width of the name column.")]
        [Category(Categories.Default)]
        public double NameColumnWidth
        {
            get { return (double)GetValue(NameColumnWidthProperty); }
            set { SetValue(NameColumnWidthProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SelectedProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedPropertyProperty = DependencyProperty.Register(
            "SelectedProperty",
            typeof(IProperty),
            typeof(PropertyGrid),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the selected property.
        /// This is a dependency property.
        /// </summary>
        /// <value>The selected property.</value>
        [Description("Gets or sets the selected property.")]
        [Category(Categories.Default)]
        public IProperty SelectedProperty
        {
            get { return (IProperty)GetValue(SelectedPropertyProperty); }
            set { SetValue(SelectedPropertyProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="IsCategorized"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCategorizedProperty = DependencyProperty.Register(
            "IsCategorized",
            typeof(bool),
            typeof(PropertyGrid),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnIsCategorizedChanged));


        /// <summary>
        /// Gets or sets a value indicating whether properties are sorted by category.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the properties are sorted by category; otherwise,
        /// <see langword="false"/> if all properties are sorted alphabetically.
        /// </value>
        [Description("Gets or sets a value indicating whether properties are sorted by category.")]
        [Category(Categories.Default)]
        public bool IsCategorized
        {
            get { return (bool)GetValue(IsCategorizedProperty); }
            set { SetValue(IsCategorizedProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="Filter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            "Filter",
            typeof(string),
            typeof(PropertyGrid),
            new FrameworkPropertyMetadata(string.Empty, OnFilterChanged));

        /// <summary>
        /// Gets or sets the filter that is applied to the properties list.
        /// This is a dependency property.
        /// </summary>
        /// <value>The filter that is applied to the properties list.</value>
        [Description("Gets or sets the filter that is applied to the properties list.")]
        [Category(Categories.Default)]
        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes the static members of the <see cref="PropertyGrid"/>.
        /// </summary>
        static PropertyGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGrid), new FrameworkPropertyMetadata(typeof(PropertyGrid)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGrid"/> class.
        /// </summary>
        public PropertyGrid()
        {
            CommandBindings.Add(new CommandBinding(PropertyGridCommands.ClearFilter, OnClearFilter, OnCanClearFilter));
            CommandBindings.Add(new CommandBinding(PropertyGridCommands.ResetProperty, OnResetProperty, OnCanResetProperty));

            SetBinding(PropertiesProperty, new Binding("PropertySource.Properties") { Source = this });

            AddHandler(Expander.ExpandedEvent, new RoutedEventHandler(OnCategoryExpanded));
            AddHandler(Expander.CollapsedEvent, new RoutedEventHandler(OnCategoryCollapsed));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnPropertiesSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            Debug.Assert(eventArgs.OldValue != eventArgs.NewValue, "OnPropertiesSourceChanged should only be called when value has changed.");

            var propertyGrid = (PropertyGrid)dependencyObject;
            propertyGrid.SetSorting();
            propertyGrid.SetFilter();
        }


        /// <summary>
        /// Called when the <see cref="Properties"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnPropertiesChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var propertyGrid = (PropertyGrid)dependencyObject;
            propertyGrid.OnPropertiesChanged();
        }


        /// <summary>
        /// Called when the <see cref="Properties"/> property changed.
        /// </summary>
        private void OnPropertiesChanged()
        {
            int delay = Delay;
            if (delay <= 0 || Properties == null)
            {
                _timer?.Stop();
                IsLoading = false;
                return;
            }

            // Set IsLoading and reset after delay.
            IsLoading = true;

            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Tick += (s, e) =>
                               {
                                   _timer.Stop();
                                   IsLoading = false;
                               };
            }

            _timer.Interval = TimeSpan.FromMilliseconds(delay);
            _timer.Start(); // Resets timer interval.
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (_thumb != null)
                _thumb.DragDelta -= OnThumbDragDelta;

            base.OnApplyTemplate();
            _thumb = GetTemplateChild("PART_Thumb") as Thumb;
            _listBox = GetTemplateChild("PART_ListBox") as ListBox;

            if (_thumb != null)
                _thumb.DragDelta += OnThumbDragDelta;
        }


        private static void OnIsCategorizedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var propertyGrid = (PropertyGrid)dependencyObject;
            propertyGrid.SetSorting();
        }


        private static void OnFilterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var propertyGrid = (PropertyGrid)dependencyObject;
            propertyGrid.SetFilter();

            // Requery PropertyGridCommands.ClearFilter command if necessary.
            string oldValue = (string)eventArgs.OldValue;
            string newValue = (string)eventArgs.NewValue;
            if (string.IsNullOrEmpty(oldValue) != string.IsNullOrEmpty(newValue))
                CommandManager.InvalidateRequerySuggested();
        }


        private void OnThumbDragDelta(object sender, DragDeltaEventArgs eventArgs)
        {
            var width = Math.Max(20, NameColumnWidth + eventArgs.HorizontalChange);
            width = Math.Min(ActualWidth - 30, width);
            NameColumnWidth = width;
        }


        private void SetSorting()
        {
            var properties = PropertySource?.Properties;
            if (properties == null)
                return;

            var view = CollectionViewSource.GetDefaultView(PropertySource.Properties);
            view.GroupDescriptions.Clear();
            view.SortDescriptions.Clear();

            if (IsCategorized)
            {
                view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(IProperty.Category)));

                // (Do not sort. Use user-defined order.)
                //view.SortDescriptions.Add(new SortDescription(nameof(IProperty.Category), ListSortDirection.Ascending));
                //view.SortDescriptions.Add(new SortDescription(nameof(IProperty.Name), ListSortDirection.Ascending));
            }
            else
            {
                view.SortDescriptions.Add(new SortDescription(nameof(IProperty.Name), ListSortDirection.Ascending));
            }
        }


        private void SetFilter()
        {
            var properties = PropertySource?.Properties;
            if (properties == null)
                return;

            var view = CollectionViewSource.GetDefaultView(PropertySource.Properties);

            if (string.IsNullOrEmpty(Filter))
            {
                view.Filter = null;
            }
            else
            {
                view.Filter = item =>
                {
                    var property = item as IProperty;
                    return property?.Name?.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) >= 0;
                };
            }
        }


        private void OnCanClearFilter(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = !string.IsNullOrEmpty(Filter);
            eventArgs.Handled = true;
        }


        private void OnClearFilter(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            Filter = string.Empty;
            eventArgs.Handled = true;
        }


        private void OnCanResetProperty(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = SelectedProperty != null && SelectedProperty.CanReset;
            eventArgs.Handled = true;
        }


        private void OnResetProperty(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            SelectedProperty?.Reset();
            eventArgs.Handled = true;
        }


        /// <inheritdoc/>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            // Override navigation between properties because the ListBox does not navigate to
            // collapsed expander items.
            var key = e.Key;
            if (key == Key.Down)
                e.Handled = NavigateUpDown(true);
            else if (key == Key.Up)
                e.Handled = NavigateUpDown(false);

            base.OnPreviewKeyDown(e);
        }


        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            // Is key event coming from a list box with categories?
            if (IsCategorized && _listBox != null && _listBox.IsKeyboardFocusWithin)
            {
                // LEFT/RIGHT keys are used to collapse/expand categories.
                if (e.Key == Key.Left)
                {
                    // Get expander which contains the selected property.
                    var expander = GetExpander(SelectedProperty);

                    if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                        CollapseAll(expander);
                    else
                        Collapse(expander);
                }
                if (e.Key == Key.Right)
                {
                    var expander = Keyboard.FocusedElement as Expander;

                    if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                        ExpandAll(expander);
                    else
                        Expand(expander);
                }
            }

            base.OnKeyDown(e);
        }


        private void CollapseAll(Expander selectedExpander)
        {
            _isManualCollapseOrExpand = true;

            try
            {
                // Collapse all expanders.
                foreach (var expander in _listBox.GetVisualDescendants().OfType<Expander>())
                {
                    expander.IsExpanded = false;
                    expander.Focusable = true;
                }

                // Clear list box selection
                _listBox.SelectedItem = null;

                // Move focus to expander which contained the selected property
                selectedExpander?.Focus();
                selectedExpander?.BringIntoView();
            }
            finally
            {
                _isManualCollapseOrExpand = false;
            }
        }


        private void Collapse(Expander expander)
        {
            if (expander == null)
                return;

            _isManualCollapseOrExpand = true;

            try
            {
                expander.IsExpanded = false;
                expander.Focusable = true;
                if (_listBox != null)
                    _listBox.SelectedItem = null;

                expander.Focus();
                expander.BringIntoView();
            }
            finally
            {
                _isManualCollapseOrExpand = false;
            }
        }


        private void ExpandAll(Expander selectedExpander)
        {
            _isManualCollapseOrExpand = true;

            try
            {
                // Expand all expanders.
                foreach (var expander in _listBox.GetVisualDescendants().OfType<Expander>())
                {
                    expander.IsExpanded = true;

                    // When an expander is expanded, only its properties should get focus.
                    expander.Focusable = false;
                }

                // Move focus to first property of the selected expander.
                var listBoxItem = selectedExpander?.GetVisualDescendants().OfType<ListBoxItem>().FirstOrDefault();
                if (listBoxItem != null)
                {
                    WindowsHelper.BeginInvokeOnUI(() =>
                                                  {
                                                      listBoxItem.Focus();
                                                      listBoxItem.BringIntoView();
                                                  });
                }
            }
            finally
            {
                _isManualCollapseOrExpand = false;
            }
        }


        private void Expand(Expander expander)
        {
            if (expander == null)
                return;

            _isManualCollapseOrExpand = true;

            try
            {
                expander.IsExpanded = true;
                expander.Focusable = false;
                var selectedProperty = expander.GetVisualDescendants().OfType<ListBoxItem>().FirstOrDefault();
                selectedProperty?.Focus();
                selectedProperty?.BringIntoView();
            }
            finally
            {
                _isManualCollapseOrExpand = false;
            }
        }


        private void OnCategoryCollapsed(object sender, RoutedEventArgs eventArgs)
        {
            if (_isManualCollapseOrExpand)
                return;

            var expander = eventArgs.OriginalSource as Expander;
            if (expander == null || !expander.IsLoaded)
                return;

            // If this collapse was triggered by the user and Shift is pressed, collapse all.
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                CollapseAll(expander);
            }
            else
            {
                expander.Focusable = true;
                expander.Focus();
                expander.BringIntoView();
            }
        }


        private void OnCategoryExpanded(object sender, RoutedEventArgs eventArgs)
        {
            if (_isManualCollapseOrExpand)
                return;

            var expander = eventArgs.OriginalSource as Expander;
            if (expander == null || !expander.IsLoaded)
                return;

            // If this collapse was triggered by the user and Shift is pressed, expand all.
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                ExpandAll(expander);
            }
            else
            {
                expander.Focusable = false;
                var listBoxItem = expander.GetVisualDescendants().OfType<ListBoxItem>().FirstOrDefault();
                if (listBoxItem != null)
                {
                    listBoxItem.Focus();    // TODO: This does not work. :-(
                    listBoxItem.BringIntoView();
                }
            }
        }


        private bool NavigateUpDown(bool down)
        {
            if (!IsCategorized)
                return false;

            var properties = PropertySource?.Properties;
            if (properties == null)
                return false;

            var view = CollectionViewSource.GetDefaultView(properties) as ListCollectionView;

            var selectedExpander = Keyboard.FocusedElement as Expander;
            if (selectedExpander != null)
            {
                // An expander has the focus.
                // Find next expander. Do nothing if it is expanded. If it is collapsed it gets
                // the focus.
                var selectedGroup = view.Groups
                                        .OfType<CollectionViewGroup>()
                                        .FirstOrDefault(g => Equals(g.Name, selectedExpander.Header));
                if (selectedGroup == null)
                    return false;

                var selectedGroupIndex = view.Groups.IndexOf(selectedGroup);
                if (selectedGroupIndex == (down ? view.Groups.Count - 1 : 0))
                    return false;

                var nextGroup = (CollectionViewGroup)view.Groups[selectedGroupIndex + (down ? +1 : -1)];
                var nextProperty = (IProperty)nextGroup.Items[0];
                var nextExpander = GetExpander(nextProperty);
                if (nextExpander == null || nextExpander.IsExpanded)
                    return false;

                nextExpander.Focus();
                if (_listBox != null)
                    _listBox.SelectedItem = null;

                return true;
            }

            {
                var selectedProperty = SelectedProperty;
                if (selectedProperty == null)
                    return false;

                // A property has the focus. If the next property is in a collapsed expander, then
                // focus the expander.

                var index = view.IndexOf(selectedProperty);
                if (index == (down ? view.Count - 1 : 0))
                    return false;

                var nextProperty = (IProperty)view.GetItemAt(index + (down ? +1 : -1));
                if (selectedProperty.Category == nextProperty.Category)
                    return false;

                var nextExpander = GetExpander(nextProperty);
                if (nextExpander.IsExpanded)
                    return false;

                if (_listBox != null)
                    _listBox.SelectedItem = null;

                nextExpander.Focus();

                return true;
            }
        }


        /// <summary>
        /// Gets the expander which contains the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The expander.</returns>
        private Expander GetExpander(IProperty property)
        {
            if (property == null)
                return null;

            // Important: This works only if Expander.Header is a string and not something else
            // (like a TextBlock for example).
            return this.GetVisualDescendants()
                       .OfType<Expander>()
                       .FirstOrDefault(expander => Equals(expander.Header, property.Category));
        }
        #endregion
    }
}
