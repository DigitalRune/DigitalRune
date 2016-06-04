// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DigitalRune.Windows;


namespace DigitalRune.Editor.QuickLaunch
{
    /// <summary>
    /// Represents the Quick Launch box.
    /// </summary>
    internal partial class QuickLaunchView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuickLaunchView"/> class.
        /// </summary>
        public QuickLaunchView()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Invoked when an unhandled <strong>Keyboard.GotKeyboardFocus</strong> attached event
        /// reaches an element in its route that is derived from this class. Implement this method
        /// to add class handling for this event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="KeyboardFocusChangedEventArgs"/> that contains the event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs eventArgs)
        {
            base.OnGotKeyboardFocus(eventArgs);

            if (ReferenceEquals(eventArgs.NewFocus, this))
            {
                // Move focus to text box. (Usually only necessary when the view is focused
                // using the menu item or a key binding.)
                CommandTextBox.Focus();
            }
        }


        /// <summary>
        /// Invoked when an unhandled <strong>Keyboard.PreviewKeyDown</strong> attached event
        /// reaches an element in its route that is derived from this class. Implement this method
        /// to add class handling for this event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="KeyEventArgs"/> that contains the event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnPreviewKeyDown(KeyEventArgs eventArgs)
        {
            if (IsNavigationKey(eventArgs.Key))
            {
                // Move selection up/down.
                NavigateListBox(eventArgs.Key);
                eventArgs.Handled = true;
            }
            else if (eventArgs.Key == Key.Escape)
            {
                // Let the view model handle the Escape key.
                var viewModel = DataContext as QuickLaunchViewModel;
                if (viewModel != null)
                {
                    viewModel.CancelCommand.Execute();
                    eventArgs.Handled = true;
                }
            }
            else if (eventArgs.Key == Key.Enter)
            {
                // Start new search or execute the selected item depending on whether the
                // results popup is visible.
                var viewModel = DataContext as QuickLaunchViewModel;
                if (viewModel != null)
                {
                    if (ResultsPopup.IsOpen)
                        viewModel.ExecuteCommand.Execute();
                    else
                        viewModel.FindCommand.Execute();

                    eventArgs.Handled = true;
                }
            }

            base.OnPreviewKeyDown(eventArgs);
        }


        private static bool IsNavigationKey(Key key)
        {
            switch (key)
            {
                case Key.Up:
                case Key.Down:
                case Key.PageUp:
                case Key.PageDown:
                    return true;
                default:
                    return false;
            }
        }


        private void NavigateListBox(Key key)
        {
            int numberOfItems = ResultsListBox.Items.Count;
            if (numberOfItems <= 1)
                return;

            int index = ResultsListBox.SelectedIndex;
            int scrollOffset;
            int pageHeight;

            // The vertical offsets and extent in the ScrollViewer is measured in 'items'.
            var scrollViewer = ResultsListBox.GetVisualDescendants()
                                             .OfType<ScrollViewer>()
                                             .FirstOrDefault();
            if (scrollViewer != null)
            {
                // ScrollViewer.VerticalOffset is the index of the first visible item.
                scrollOffset = (int)scrollViewer.VerticalOffset;

                // ScrollViewer.ViewportHeight is number of visible items (rounded down).
                pageHeight = Math.Max(1, (int)scrollViewer.ViewportHeight);
            }
            else
            {
                scrollOffset = 0;
                pageHeight = 1;
            }

            if (key == Key.Down)
            {
                // Move down.
                index = Cycle(++index, numberOfItems);
            }
            else if (key == Key.Up)
            {
                // Move up.
                index = Cycle(--index, numberOfItems);
            }
            else if (key == Key.PageUp)
            {
                if (index == -1)
                {
                    // No item selected: Select first item.
                    index = 0;
                }
                else if (index == 0)
                {
                    // First item selected: Jump to last item.
                    index = numberOfItems - 1;
                }
                else if (index == scrollOffset)
                {
                    // First visible item selected: Jump up by one page.
                    index = Clamp(index - pageHeight, numberOfItems);
                }
                else
                {
                    // Jump to first visible item.
                    index = scrollOffset;
                }
            }
            else if (key == Key.PageDown)
            {
                if (index == -1)
                {
                    // No item selected: Select first item.
                    index = 0;
                }
                else if (index == numberOfItems - 1)
                {
                    // Last item selected: Jump to first item.
                    index = 0;
                }
                else if (index == scrollOffset + pageHeight - 1)
                {
                    // Last visible item selected: Jump down by one page.
                    index = Clamp(index + pageHeight - 1, numberOfItems);
                }
                else
                {
                    // Jump to end of page.
                    index = Clamp(scrollOffset + pageHeight - 1, numberOfItems);
                }
            }

            if (index != -1)
            {
                Debug.Assert(0 <= index && index < numberOfItems, "Invalid index.");

                ResultsListBox.SelectedIndex = index;

                // Important: ScrollIntoView() causes an exception if virtualization is enabled and the 
                // layout (e.g. width) of the ListBox changes.
                // --> Use a fixed-size popup to ensure that the layout does not change while scrolling!
                ResultsListBox.ScrollIntoView(ResultsListBox.Items[index]);
            }
        }


        private static int Clamp(int index, int numberOfItems)
        {
            if (index < 0)
                index = 0;
            else if (index >= numberOfItems)
                index = numberOfItems - 1;

            return index;
        }


        private static int Cycle(int index, int numberOfItems)
        {
            // Circular navigation.
            if (index < 0)
                index = numberOfItems - 1;
            else if (index >= numberOfItems)
                index = 0;

            return index;
        }


        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
        {
            // Catch mouse-clicks on ListBoxItems.
            var listBoxItem = GetListBoxItem(eventArgs.OriginalSource as DependencyObject);
            if (listBoxItem != null)
                eventArgs.Handled = true;
        }


        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs eventArgs)
        {
            var listBoxItem = GetListBoxItem(eventArgs.OriginalSource as DependencyObject);
            if (listBoxItem != null)
                listBoxItem.IsSelected = true;
        }


        private void ListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs eventArgs)
        {
            // Select and execute item.
            var listBoxItem = GetListBoxItem(eventArgs.OriginalSource as DependencyObject);
            if (listBoxItem != null)
            {
                listBoxItem.IsSelected = true;

                var viewModel = DataContext as QuickLaunchViewModel;
                viewModel?.ExecuteCommand.Execute();
            }
        }


        private static ListBoxItem GetListBoxItem(DependencyObject element)
        {
            return element?.GetSelfAndVisualAncestors()
                           .OfType<ListBoxItem>()
                           .FirstOrDefault();
        }
    }
}
