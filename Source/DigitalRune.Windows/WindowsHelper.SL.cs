// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DigitalRune.Linq;


namespace DigitalRune.Windows
{
    partial class WindowsHelper
    {
        /// <summary>
        /// Determines whether the visual object is an ancestor of the descendant visual object.
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/>.</param>
        /// <param name="descendant">The <see cref="DependencyObject"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="element"/> is an ancestor of
        /// <paramref name="descendant"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsAncestorOf(this UIElement element, DependencyObject descendant)
        {
            if (element == null || descendant == null)
                return false;

            if (element == descendant)
                return true;

            return descendant.GetVisualAncestors().Contains(element);
        }


        #region ----- LINQ to Logical Tree -----

        // Silverlight does not provide an API to access the logical tree. Instead, Silverlight has
        // an 'object tree'. The relationship is defined using the property FrameworkElement.Parent.
        // FrameworkElement.Parent is either the logical parent, if there is any, otherwise it set
        // to the visual parent.

        /// <summary>
        /// Gets the logical parent of the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical parent for.
        /// </param>
        /// <returns>
        /// The logical parent of the <see cref="DependencyObject"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static DependencyObject GetLogicalParent(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            DependencyObject parent = null;
            if (dependencyObject is FrameworkElement)
                parent = ((FrameworkElement)dependencyObject).Parent;

            return parent;
        }


        /// <summary>
        /// Gets the logical ancestors of the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical ancestors for.
        /// </param>
        /// <returns>The logical ancestors of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetLogicalAncestors(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetAncestors(dependencyObject, GetLogicalParent);
        }


        /// <summary>
        /// Gets the <see cref="DependencyObject"/> and its logical ancestors.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical ancestors for.
        /// </param>
        /// <returns>The <see cref="DependencyObject"/> and its logical ancestors.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetSelfAndLogicalAncestors(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetSelfAndAncestors(dependencyObject, GetLogicalParent);
        }


        /// <summary>
        /// Gets the logical root of the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical root for.
        /// </param>
        /// <returns>The logical root of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static DependencyObject GetLogicalRoot(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetRoot(dependencyObject, GetLogicalParent);
        }


        /// <summary>
        /// Gets the logical children of the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical children for.
        /// </param>
        /// <returns>The logical children of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetLogicalChildren(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            if (dependencyObject is ItemsControl)
            {
                // ----- ItemsControl
                // Get the logical children from the ItemsContainerGenerator.
                var itemsControl = (ItemsControl)dependencyObject;
                var numberOfChildren = itemsControl.Items.Count;
                for (int index = 0; index < numberOfChildren; index++)
                {
                    var child = itemsControl.Items[index] as FrameworkElement;
                    if (child == null)
                    {
                        // Item is probably a CLR object wrapped in an item container.
                        Debug.Assert(itemsControl.ItemContainerGenerator != null);
                        child = itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
                    }

                    if (child != null)
                        yield return child;
                }
            }
            else if (dependencyObject is ContentControl)
            {
                // ----- ContentControl
                // Get the logical child directly from Content.
                var contentControl = (ContentControl)dependencyObject;
                var child = contentControl.Content as FrameworkElement;
                if (child != null)
                    yield return child;
            }
            else if (dependencyObject is Popup)
            {
                // ----- Popup
                // Get the logical child directly from the Popup.
                var popup = (Popup)dependencyObject;
                var child = popup.Child as FrameworkElement;
                if (child != null)
                    yield return child;
            }
            else
            {
                // ----- Unknown Type
                // The other controls (such as the ContentControl, etc.) are not as simple because
                // they might use data templates and there is no direct way to get the logical
                // child.

                // But we can go through all visual children and check whether the property
                // Framework.Parent is set. (It is usually set to the logical parent. If there is
                // not logical parent, it usually matches the visual parent.)

                // Make a breadth-first search. (Recursion stops if logical child is found or there
                // are no more visual children.)
                var visualChildren = dependencyObject.GetVisualChildren().OfType<FrameworkElement>();
                Queue<FrameworkElement> queue = new Queue<FrameworkElement>(visualChildren);

                while (queue.Count > 0)
                {
                    var visualDescendant = queue.Dequeue();
                    if (visualDescendant.Parent == dependencyObject)
                    {
                        yield return visualDescendant;
                    }
                    else
                    {
                        // The current element is not a logical child. Check its visual children.
                        visualChildren = visualDescendant.GetVisualChildren().OfType<FrameworkElement>();
                        foreach (var visualChild in visualChildren)
                        {
                            queue.Enqueue(visualChild);
                        }
                    }
                }
            }
        }


        /// <overloads>
        /// <summary>
        /// Gets the logical descendants of the <see cref="DependencyObject"/>.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Gets the logical descendants of the <see cref="DependencyObject"/> using depth-first
        /// search.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical descendants for.
        /// </param>
        /// <returns>
        /// The logical descendants of the <see cref="DependencyObject"/> using a depth-first
        /// search.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetLogicalDescendants(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetDescendants(dependencyObject, GetLogicalChildren, true);
        }


        /// <summary>
        /// Gets the logical descendants of the <see cref="DependencyObject"/> using either a depth-
        /// or a breadth-first search.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical descendants for.
        /// </param>
        /// <param name="depthFirst">
        /// If set to <see langword="true"/> then a depth-first search for descendants will be made;
        /// otherwise a breadth-first search will be made.
        /// </param>
        /// <returns>The logical descendants of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetLogicalDescendants(this DependencyObject dependencyObject, bool depthFirst)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetDescendants(dependencyObject, GetLogicalChildren, depthFirst);
        }


        /// <overloads>
        /// <summary>
        /// Gets the logical subtree (the given <see cref="DependencyObject"/> and all of its 
        /// descendants).
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Gets the logical subtree (the given <see cref="DependencyObject"/> and all of its
        /// descendants) using depth-first search.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> that is the root of the logical subtree.
        /// </param>
        /// <returns>
        /// The <paramref name="dependencyObject"/> and all of its descendants using a depth-first
        /// search.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetLogicalSubtree(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetSubtree(dependencyObject, GetLogicalChildren, true);
        }


        /// <summary>
        /// Gets the logical subtree (the given <see cref="DependencyObject"/> and all of its
        /// descendants) using either a depth- or a breadth-first search.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical descendants for.
        /// </param>
        /// <param name="depthFirst">
        /// If set to <see langword="true"/> then a depth-first search for descendants will be made;
        /// otherwise a breadth-first search will be made.
        /// </param>
        /// <returns>The <paramref name="dependencyObject"/> and all of its descendants.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetLogicalSubtree(this DependencyObject dependencyObject, bool depthFirst)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetSubtree(dependencyObject, GetLogicalChildren, depthFirst);
        }
        #endregion


        #region ----- ClipToBounds Attached Property -----

        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.WindowsHelper.ClipToBounds"/> attached 
        /// dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a value indicating whether the content of a <see cref="FrameworkElement"/>
        /// should be clipped to the bounds of the element. The default value is 
        /// <see langword="false"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to clip the content; otherwise, <see langword="false"/>.
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty ClipToBoundsProperty = DependencyProperty.RegisterAttached(
          "ClipToBounds",
          typeof(bool),
          typeof(WindowsHelper),
          new PropertyMetadata(Boxed.BooleanFalse, OnClipToBoundsChanged));

        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.WindowsHelper.ClipToBounds"/>
        /// attached property from a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <return>
        /// The value of the <see cref="P:DigitalRune.Windows.WindowsHelper.ClipToBounds"/> attached
        /// property.
        /// </return>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static bool GetClipToBounds(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return (bool)obj.GetValue(ClipToBoundsProperty);
        }


        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.WindowsHelper.ClipToBounds"/>
        /// attached property to a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static void SetClipToBounds(DependencyObject obj, bool value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.SetValue(ClipToBoundsProperty, Boxed.Get(value));
        }


        /// <summary>
        /// Called when the <see cref="P:DigitalRune.Windows.WindowsHelper.ClipToBounds"/> property
        /// changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// <para>
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </para>
        /// </param>
        private static void OnClipToBoundsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            bool oldValue = (bool)eventArgs.OldValue;
            bool newValue = (bool)dependencyObject.GetValue(ClipToBoundsProperty);

            if (oldValue == newValue)
                return;

            var element = dependencyObject as FrameworkElement;
            if (element == null)
                return;

            SetClip(element, newValue);

            if (newValue)
            {
                element.Loaded += OnClipToBoundsFrameworkElementChanged;
                element.SizeChanged += OnClipToBoundsFrameworkElementChanged;
            }
            else
            {
                element.Loaded -= OnClipToBoundsFrameworkElementChanged;
                element.SizeChanged -= OnClipToBoundsFrameworkElementChanged;
            }
        }


        private static void OnClipToBoundsFrameworkElementChanged(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;

            SetClip(element, (bool)element.GetValue(ClipToBoundsProperty));
        }


        private static void SetClip(FrameworkElement element, bool enable)
        {
            if (enable)
            {
                element.Clip = new RectangleGeometry
                {
                    Rect = new Rect(0, 0, element.ActualWidth, element.ActualHeight)
                };
            }
            else
            {
                element.Clip = null;
            }
        }
        #endregion
    }
}
#endif
