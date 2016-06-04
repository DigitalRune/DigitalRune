// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Markup;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Provides suggestions for the <see cref="AutoCompleteBehavior"/>.
    /// </summary>
    public interface IAutoCompleteProvider
    {
        /// <summary>
        /// Gets the suggestions for the given text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The asynchronous result containing the suggestions.</returns>
        /// <remarks>
        /// The suggestions are listed in the popup, which is shown when the user types in the text
        /// box. The items needs to be strings or need to implement the
        /// <see cref="object.ToString()"/> method.
        /// </remarks>
        IObservable<object> GetSuggestions(string text);
    }


    /// <summary>
    /// Provides suggestions for the <see cref="AutoCompleteBehavior"/>. (Default implementation of
    /// the interface <see cref="IAutoCompleteProvider"/>.)
    /// </summary>
    [ContentProperty("Suggestions")]
    public class AutoCompleteProvider : IAutoCompleteProvider
    {
        /// <summary>
        /// Gets or sets a list of suggestions.
        /// </summary>
        /// <value>A list of suggestions.</value>
        public IEnumerable Suggestions { get; set; }


        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoCompleteProvider"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoCompleteProvider"/> class.
        /// </summary>
        /// <inheritdoc cref="IAutoCompleteProvider.GetSuggestions"/>
        public AutoCompleteProvider()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AutoCompleteProvider"/> class.
        /// </summary>
        /// <param name="suggestions">The complete list of suggestions.</param>
        /// <inheritdoc cref="IAutoCompleteProvider.GetSuggestions"/>
        public AutoCompleteProvider(IEnumerable suggestions)
        {
            Suggestions = suggestions;
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IObservable<object> IAutoCompleteProvider.GetSuggestions(string text)
        {
            if (Suggestions == null)
                return Observable.Empty<object>();

            return Suggestions.Cast<object>()
                              .Where(suggestion => suggestion != null && suggestion.ToString().StartsWith(text, StringComparison.OrdinalIgnoreCase))
                              .ToObservable();
        }
    }


    /// <summary>
    /// Provides auto-completion for <see cref="TextBox"/> elements.
    /// </summary>
    /// <example>
    /// The <see cref="AutoCompleteBehavior"/> can be set directly in XAML:
    /// <code lang="xaml">
    /// <![CDATA[
    /// <Window x:Class="MyApplication.MainWindow"
    ///         xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///         xmlns:dr="http://schemas.digitalrune.com/windows"
    ///         xmlns:i="http://schemas.microsoft.com/expression/2010/interactivity"
    ///         xmlns:Specialized="clr-namespace:System.Collections.Specialized;assembly=System"
    ///         xmlns:sys="clr-namespace:System;assembly=mscorlib"
    ///         Title="MainWindow"
    ///         Width="525"
    ///         Height="350" 
    ///         FocusManager.FocusedElement="{Binding ElementName=MyTextBox}">
    ///   <StackPanel Margin="11">
    ///     <TextBox x:Name="MyTextBox" Width="200" Margin="0,5,0,0" HorizontalAlignment="Left">
    ///       <i:Interaction.Behaviors>
    ///         <dr:AutoCompleteBehavior>
    ///           <dr:AutoCompleteBehavior.Provider>
    ///             <dr:AutoCompleteProvider>
    ///               <Specialized:StringCollection>
    ///                 <sys:String>Suggestion #1</sys:String>
    ///                 <sys:String>Suggestion #2</sys:String>
    ///                 ...
    ///               </Specialized:StringCollection>
    ///             </dr:AutoCompleteProvider>
    ///           </dr:AutoCompleteBehavior.Provider>
    ///         </dr:AutoCompleteBehavior>
    ///       </i:Interaction.Behaviors>
    ///     </TextBox>
    ///   </StackPanel>
    /// </Window>
    /// ]]>
    /// </code>
    /// In more complex situation the view model can provide a custom 
    /// <see cref="IAutoCompleteProvider"/>, which can be bound to the <see cref="Provider"/>
    /// property of the <see cref="AutoCompleteBehavior"/>.
    /// </example>
    public class AutoCompleteBehavior : Behavior<TextBox>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private Window _window;
        private Popup _popup;
        private ListBox _listBox;
        private IDisposable _providerSubscription;
        private bool _updatingText;
        private bool _suppressAutoAppend;
        private string _originalText;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the key that references the style that is defined for the <see cref="ListBox"/>
        /// that shows the suggestions of the <see cref="AutoCompleteBehavior"/>.
        /// </summary>
        /// <value>
        /// A <see cref="ResourceKey"/> that references the <see cref="Style"/> that is applied to
        /// the <see cref="ListBox"/> control for the <see cref="AutoCompleteBehavior"/>.
        /// </value>
        public static ResourceKey AutoCompleteListBoxStyleKey
        {
            get
            {
                if (_autoCompleteListBoxStyleKey == null)
                    _autoCompleteListBoxStyleKey = new ComponentResourceKey(typeof(AutoCompleteBehavior), "AutoCompleteListBoxStyleKey");

                return _autoCompleteListBoxStyleKey;
            }
        }
        private static ResourceKey _autoCompleteListBoxStyleKey;


        /// <summary>
        /// Gets the key that references the shadow depth used for the drop shadow effect of the
        /// <see cref="AutoCompleteBehavior"/>.
        /// </summary>
        /// <value>
        /// A <see cref="ResourceKey"/> that references the shadow depth used for the drop shadow
        /// effect of the <see cref="AutoCompleteBehavior"/>.
        /// </value>
        public static ResourceKey AutoCompleteShadowDepthKey
        {
            get
            {
                if (_autoCompleteShadowDepthKey == null)
                    _autoCompleteShadowDepthKey = new ComponentResourceKey(typeof(AutoCompleteBehavior), "AutoCompleteShadowDepthKey");

                return _autoCompleteShadowDepthKey;
            }
        }
        private static ResourceKey _autoCompleteShadowDepthKey;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(AutoCompleteBehavior),
            new PropertyMetadata(Boxed.BooleanTrue, OnIsEnabledChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the behavior is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the behavior is enabled; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the behavior is enabled.")]
        [Category(Categories.Common)]
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="MaxHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxHeightProperty = DependencyProperty.Register(
            "MaxHeight",
            typeof(double),
            typeof(AutoCompleteBehavior),
            new PropertyMetadata(400.0));

        /// <summary>
        /// Gets or sets the max height of the auto-complete list in device-independent pixels.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The max height of the auto-complete list in device-independent pixels. The default value
        /// is
        /// 400. 0.
        /// </value>
        [Description("Gets or sets the max height of the auto-complete list in device-independent pixels.")]
        [Category(Categories.Layout)]
        public double MaxHeight
        {
            get { return (double)GetValue(MaxHeightProperty); }
            set { SetValue(MaxHeightProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register(
            "Provider",
            typeof(IAutoCompleteProvider),
            typeof(AutoCompleteBehavior),
            new PropertyMetadata(null, OnProviderChanged));

        /// <summary>
        /// Gets or sets the auto-complete provider. 
        /// This is a dependency property.
        /// </summary>
        /// <value>The auto-complete provider.</value>
        [Description("Gets or sets the auto-complete provider.")]
        [Category(Categories.Default)]
        public IAutoCompleteProvider Provider
        {
            get { return (IAutoCompleteProvider)GetValue(ProviderProperty); }
            set { SetValue(ProviderProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            _window = Window.GetWindow(AssociatedObject);
            if (_window != null)
            {
                _window.Deactivated += Window_Deactivated;
                _window.LocationChanged += Window_LocationChanged;
                _window.PreviewMouseDown += Window_PreviewMouseDown;
                _window.StateChanged += Window_StateChanged;
            }

            _popup = new Popup
            {
                AllowsTransparency = true,
                IsOpen = false,
                MinHeight = SystemParameters.HorizontalScrollBarHeight,
                MinWidth = 2 * SystemParameters.VerticalScrollBarWidth,
                PlacementTarget = AssociatedObject,
                Placement = PlacementMode.Bottom,
                SnapsToDevicePixels = true,
                StaysOpen = true
            };

            _listBox = new ListBox { Focusable = false };
            _listBox.SetResourceReference(FrameworkElement.StyleProperty, AutoCompleteListBoxStyleKey);
            _popup.Child = _listBox;

            AssociatedObject.LostFocus += TextBox_LostFocus;
            AssociatedObject.PreviewKeyDown += TextBox_PreviewKeyDown;
            AssociatedObject.TextChanged += TextBox_TextChanged;

            _listBox.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
            _listBox.PreviewMouseLeftButtonUp += ListBox_PreviewMouseLeftButtonUp;
            _listBox.PreviewMouseMove += ListBox_PreviewMouseMove;
        }


        /// <summary>
        /// Called when the behavior is being detached from its 
        /// <see cref="Behavior{T}.AssociatedObject"/>, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            if (_providerSubscription != null)
            {
                _providerSubscription.Dispose();
                _providerSubscription = null;
            }

            if (_window != null)
            {
                _window.Deactivated -= Window_Deactivated;
                _window.LocationChanged -= Window_LocationChanged;
                _window.PreviewMouseDown -= Window_PreviewMouseDown;
                _window.StateChanged -= Window_StateChanged;
            }

            AssociatedObject.LostFocus -= TextBox_LostFocus;
            AssociatedObject.PreviewKeyDown -= TextBox_PreviewKeyDown;
            AssociatedObject.TextChanged -= TextBox_TextChanged;

            _listBox.PreviewMouseLeftButtonDown -= ListBox_PreviewMouseLeftButtonDown;
            _listBox.PreviewMouseLeftButtonUp -= ListBox_PreviewMouseLeftButtonUp;
            _listBox.PreviewMouseMove -= ListBox_PreviewMouseMove;

            _window = null;
            _popup = null;
            _listBox = null;

            base.OnDetaching();
        }


        /// <summary>
        /// Called when the <see cref="IsEnabled"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnIsEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (AutoCompleteBehavior)dependencyObject;
            bool oldValue = (bool)eventArgs.OldValue;
            bool newValue = (bool)eventArgs.NewValue;
            behavior.OnIsEnabledChanged(oldValue, newValue);
        }


        /// <summary>
        /// Called when the <see cref="IsEnabled"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnIsEnabledChanged(bool oldValue, bool newValue)
        {
            ClosePopup();
        }


        /// <summary>
        /// Called when the <see cref="Provider"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnProviderChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (AutoCompleteBehavior)dependencyObject;
            var oldProvider = (IAutoCompleteProvider)eventArgs.OldValue;
            var newProvider = (IAutoCompleteProvider)eventArgs.NewValue;
            behavior.OnProviderChanged(oldProvider, newProvider);
        }

        /// <summary>
        /// Called when the <see cref="Provider"/> property changed.
        /// </summary>
        /// <param name="oldProvider">The old value.</param>
        /// <param name="newProvider">The new value.</param>
        protected virtual void OnProviderChanged(IAutoCompleteProvider oldProvider, IAutoCompleteProvider newProvider)
        {
            ClosePopup();
        }


        private void Window_Deactivated(object sender, EventArgs eventArgs)
        {
            ClosePopup();
        }


        private void Window_LocationChanged(object sender, EventArgs eventArgs)
        {
            ClosePopup();
        }


        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.Source != AssociatedObject)
                ClosePopup();
        }


        private void Window_StateChanged(object sender, EventArgs eventArgs)
        {
            ClosePopup();
        }


        private void TextBox_LostFocus(object sender, RoutedEventArgs eventArgs)
        {
            ClosePopup();
        }


        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs eventArgs)
        {
            _suppressAutoAppend = (eventArgs.Key == Key.Back || eventArgs.Key == Key.Delete);

            if (_popup == null || !_popup.IsOpen)
                return;

            if (eventArgs.Key == Key.Enter)
            {
                ClosePopup();
                AssociatedObject.SelectAll();
                return;
            }

            if (eventArgs.Key == Key.Escape)
            {
                ClosePopup();
                eventArgs.Handled = true;
                return;
            }

            int index = _listBox.SelectedIndex;
            int numberOfItems = _listBox.Items.Count;

            // The vertical offset and extent in the ScrollViewer is measured in 'items'.
            int scrollOffset = 0;
            int pageHeight = 1;
            var scrollViewer = GetScrollViewer();
            if (scrollViewer != null)
            {
                // ScrollViewer.VerticalOffset is the index of the first visible item.
                scrollOffset = (int)scrollViewer.VerticalOffset;

                // ScrollViewer.ViewportHeight is number of visible items (rounded down).
                pageHeight = Math.Max(1, (int)scrollViewer.ViewportHeight);
            }

            if (eventArgs.Key == Key.Down)
            {
                // Move down.
                index++;
            }
            else if (eventArgs.Key == Key.Up)
            {
                if (index == -1)
                {
                    // Circular navigation: Jump to end of list.
                    index = numberOfItems - 1;
                }
                else
                {
                    // Move up (or clear selection if index == 0).
                    index--;
                }
            }
            else if (eventArgs.Key == Key.PageUp)
            {
                if (index == -1)
                {
                    // Circular navigation: Jump to end of list.
                    index = numberOfItems - 1;
                }
                else if (index == 0)
                {
                    // Clear selection.
                    index = -1;
                }
                else
                {
                    if (index == scrollOffset)
                    {
                        // Jump to start of page.
                        index -= pageHeight;
                        if (index < 0)
                            index = 0;
                    }
                    else
                    {
                        // Page up.
                        index = scrollOffset;
                    }
                }
            }
            else if (eventArgs.Key == Key.PageDown)
            {
                if (index == -1)
                {
                    // Select first item.
                    index = 0;
                }
                else if (index == numberOfItems - 1)
                {
                    // Clear selection.
                    index = -1;
                }
                else if (index == scrollOffset + pageHeight - 1)
                {
                    // Page down.
                    index += pageHeight - 1;
                    if (index >= numberOfItems)
                        index = numberOfItems - 1;
                }
                else
                {
                    // Jump to end of page.
                    index = scrollOffset + pageHeight - 1;
                }
            }

            if (index != _listBox.SelectedIndex)
            {
                string text;
                if (index < 0 || index >= numberOfItems)
                {
                    // Clear selection and restore original text.
                    text = _originalText;
                    _listBox.SelectedIndex = -1;
                }
                else
                {
                    // Update selection.
                    _listBox.SelectedIndex = index;
                    _listBox.ScrollIntoView(_listBox.SelectedItem);
                    text = _listBox.SelectedItem.ToString();
                }

                SetText(text);
                AssociatedObject.SelectionStart = text.Length;
                eventArgs.Handled = true;
            }
        }


        private ScrollViewer GetScrollViewer()
        {
            return _listBox.GetVisualDescendants()
                           .OfType<ScrollViewer>()
                           .FirstOrDefault();
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs eventArgs)
        {
            if (_updatingText || !IsEnabled || Provider == null)
                return;

            string text = AssociatedObject.Text;
            if (string.IsNullOrEmpty(text))
            {
                ClosePopup();
                return;
            }

            _providerSubscription?.Dispose();

            // The current list of suggestions stays open until the new suggestions are 
            // available. (To reduce flickering.)
            int count = 0;
            _providerSubscription =
              Provider.GetSuggestions(text)
                      .ObserveOnDispatcher()
                      .Subscribe(
                        suggestion =>
                        {
                            if (count == 0)
                                _listBox.Items.Clear();

                            _listBox.Items.Add(suggestion);
                            count++;
                        },
                        () =>
                        {
                            if (count == 0)
                            {
                                ClosePopup();
                                return;
                            }

                            string firstSuggestion = _listBox.Items[0].ToString();
                            if (_listBox.Items.Count == 1 && text.Equals(firstSuggestion, StringComparison.OrdinalIgnoreCase))
                            {
                                // The text matches the first and only suggestion in the list.
                                ClosePopup();
                                return;
                            }

                            _listBox.SelectedIndex = -1;
                            _originalText = text;

                            GetScrollViewer()?.ScrollToHome();

                            ShowPopup();

                            if (!_suppressAutoAppend
                                && AssociatedObject.SelectionLength == 0
                                && AssociatedObject.SelectionStart == text.Length
                                && firstSuggestion.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                            {
                                // Auto-append: Complete text if it matches the first suggestion in the list.
                                try
                                {
                                    _updatingText = true;
                                    string appendText = firstSuggestion.Substring(Math.Min(firstSuggestion.Length, text.Length));
                                    if (!string.IsNullOrEmpty(appendText))
                                    {
                                        AssociatedObject.SelectedText = appendText;
                                    }
                                }
                                finally
                                {
                                    _updatingText = false;
                                }
                            }
                        });
        }


        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
        {
            // Catch mouse-clicks on items.
            var listBoxItem = GetListBoxItem(eventArgs.OriginalSource as DependencyObject);
            if (listBoxItem != null)
                eventArgs.Handled = true;
        }


        private void ListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs eventArgs)
        {
            // Select and execute item.
            var listBoxItem = GetListBoxItem(eventArgs.OriginalSource as DependencyObject);
            if (listBoxItem != null)
            {
                listBoxItem.IsSelected = true;
                ClosePopup();
                SetText(listBoxItem.Content.ToString());
                AssociatedObject.SelectAll();
            }
        }


        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs eventArgs)
        {
            var listBoxItem = GetListBoxItem(eventArgs.OriginalSource as DependencyObject);
            if (listBoxItem != null)
                listBoxItem.IsSelected = true;
        }


        private static ListBoxItem GetListBoxItem(DependencyObject element)
        {
            return element?.GetSelfAndVisualAncestors()
                           .OfType<ListBoxItem>()
                           .FirstOrDefault();
        }


        private void ShowPopup()
        {
            if (_popup == null || _popup.IsOpen || _listBox.Items.Count == 0)
                return;

            // The shadow depth is a added to the popup size.
            object resource = AssociatedObject.TryFindResource(AutoCompleteShadowDepthKey);
            double shadowDepth = (resource is double) ? (double)resource : 0.0;

            _popup.Width = AssociatedObject.ActualWidth + shadowDepth;
            _popup.MaxHeight = MaxHeight + shadowDepth;
            _popup.IsOpen = true;
        }


        private void ClosePopup()
        {
            if (_popup != null)
                _popup.IsOpen = false;
        }


        private void SetText(string text)
        {
            try
            {
                _updatingText = true;
                AssociatedObject.Text = text;
            }
            finally
            {
                _updatingText = false;
            }
        }
        #endregion
    }
}
