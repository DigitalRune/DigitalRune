// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Provides a dependency property to bind the <see cref="TreeView.SelectedItem"/> property of a
    /// <see cref="TreeView"/> control.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="TreeView.SelectedItem"/> property of a <see cref="TreeView"/> control is
    /// read-only. A view model can bind the property, but cannot update the selected item from the
    /// view model. The <see cref="TreeViewItemSelectionBehavior"/> provides a
    /// <see cref="SelectedItem"/> property which allows to control the selected item from the view
    /// model.
    /// </para>
    /// <para>
    /// This behavior works with trees that are virtualized but could reduce performance because
    /// it forces tree view items to be realized.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code lang="csharp">
    /// <![CDATA[
    /// <UserControl x:Class="MyApplication.MyView"
    ///              xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///              xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///              xmlns:dr="http://schemas.digitalrune.com/windows"
    ///              xmlns:i="http://schemas.microsoft.com/expression/2010/interactivity">
    /// 
    ///   <TreeView ItemsSource="{Binding Items}">
    ///     <i:Interaction.Behaviors>
    ///       <dr:TreeViewItemSelectionBehavior SelectedItem="{Binding SelectedItem}" />
    ///     </i:Interaction.Behaviors>
    ///   </TreeView>
    /// 
    /// </UserControl>
    /// ]]>
    /// </code>
    /// </example>
    public class TreeViewItemSelectionBehavior : Behavior<TreeView>
    {
        /// <summary>
        /// Identifies the <see cref="SelectedItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(TreeViewItemSelectionBehavior),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnSelectedItemChanged));

        /// <summary>
        /// Gets or sets the selected item in the <see cref="TreeView"/>.
        /// This is a dependency property.
        /// </summary>
        /// <value>The selected item in the <see cref="TreeView"/>.</value>
        [Description("Gets or sets the selected item in a TreeView.")]
        [Category(Categories.Behavior)]
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }


        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }


        /// <summary>
        /// Called when the behavior is being detached from its
        /// <see cref="Behavior{T}.AssociatedObject"/>, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            base.OnDetaching();
        }


        /// <summary>
        /// Called when <see cref="TreeView.SelectedItem"/> of the <see cref="TreeView"/> changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="RoutedPropertyChangedEventArgs{T}"/> instance containing the event data.
        /// </param>
        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> eventArgs)
        {
            SelectedItem = eventArgs.NewValue;
        }


        /// <summary>
        /// Called when the <see cref="SelectedItem"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnSelectedItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var treeViewItem = eventArgs.NewValue as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.SetValue(TreeViewItem.IsSelectedProperty, true);
                return;
            }

            var behavior = (TreeViewItemSelectionBehavior)dependencyObject;
            var treeView = behavior.AssociatedObject;

            treeViewItem = GetTreeViewItem(treeView, eventArgs.NewValue);
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
            }
        }


        /// <summary>
        /// Gets the <see cref="TreeViewItem"/> that contains the specified item.
        /// </summary>
        /// <param name="container">
        /// The <see cref="ItemsControl"/>. This can be a <see cref="TreeView"/> or a
        /// <see cref="TreeViewItem"/>.
        /// </param>
        /// <param name="item">The item to search for.</param>
        /// <returns>
        /// The <see cref="TreeViewItem"/> that contains the specified item or
        /// <see langword="null"/> if no matching <see cref="TreeViewItem"/> could be found.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "children")]
        private static TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            // See http://stackoverflow.com/questions/11065995/binding-selecteditem-in-a-hierarchicaldatatemplate-applied-wpf-treeview
            // This method contains some modifications compared to the article.

            if (container == null)
                return null;

            if (container.DataContext == item)
                return container as TreeViewItem;

            var parentTreeViewItem = container as TreeViewItem;

            // This works for non-hierarchical item templates.
            var childContainer = container.ItemContainerGenerator.ContainerFromItem(item);
            if (childContainer != null)
            {
                // Child is in this parent tree view item. Expand the parent.
                if (parentTreeViewItem != null)
                    parentTreeViewItem.IsExpanded = true;

                return childContainer as TreeViewItem;
            }

            // For a HierarchicalDataTemplate we have to do more work:
            // Expand the current container. If the target is not in the children, we will collapse
            // it again later.
            bool collapse = false;
            if (parentTreeViewItem != null && !parentTreeViewItem.IsExpanded)
            {
                parentTreeViewItem.IsExpanded = true;
                collapse = true;
            }

            try
            {
                // Try to generate the ItemsPresenter and the ItemsPanel by calling ApplyTemplate.
                // Note that in the virtualizing case even if the item is marked expanded we still 
                // need to do this step in order to regenerate the visuals because they may have 
                // been virtualized away.
                container.ApplyTemplate();
                var itemsPresenter = (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter == null)
                {
                    // The Tree template has not named the ItemsPresenter. Walk the descendants and find
                    // the child.
                    itemsPresenter = container.GetVisualDescendants().OfType<ItemsPresenter>().FirstOrDefault();
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();
                        itemsPresenter = container.GetVisualDescendants().OfType<ItemsPresenter>().FirstOrDefault();
                    }
                }

                if (itemsPresenter == null)
                    return null;

                itemsPresenter.ApplyTemplate();

                var itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

                // Ensure that the generator for this panel has been created.
#pragma warning disable 168
                var children = itemsHostPanel.Children;
#pragma warning restore 168

                var bringIndexIntoView = GetBringIndexIntoView(itemsHostPanel);
                for (int i = 0, count = container.Items.Count; i < count; i++)
                {
                    TreeViewItem childTreeViewItem;
                    if (bringIndexIntoView != null)
                    {
                        // Bring the item into view so that the container will be generated.
                        bringIndexIntoView(i);
                        childTreeViewItem = (TreeViewItem)container.ItemContainerGenerator.ContainerFromIndex(i);
                    }
                    else
                    {
                        childTreeViewItem = (TreeViewItem)container.ItemContainerGenerator.ContainerFromIndex(i);

                        // Bring the item into view to maintain the same behavior as with a virtualizing panel.
                        childTreeViewItem.BringIntoView();
                    }

                    if (childTreeViewItem == null)
                        continue;

                    // Search the next level for the object.
                    var resultContainer = GetTreeViewItem(childTreeViewItem, item);
                    if (resultContainer != null)
                    {
                        collapse = false;   // TreeViewItem found. Leave parent expanded.
                        return resultContainer;
                    }
                }

                return null;
            }
            finally
            {
                if (collapse)
                    parentTreeViewItem.IsExpanded = false;
            }
        }


        /// <summary>
        /// Gets a delegate which brings the item with a specific index into view.
        /// </summary>
        /// <param name="itemsHostPanel">The items host panel.</param>
        /// <returns>
        /// A delegate which takes an integer index argument and brings the element into view.
        /// Can be <see langword="null"/> if no suitable method was found.
        /// </returns>
        private static Action<int> GetBringIndexIntoView(Panel itemsHostPanel)
        {
            var virtualizingPanel = itemsHostPanel as VirtualizingStackPanel;

            var method = virtualizingPanel?.GetType().GetMethod(
                "BringIndexIntoView",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new[] { typeof(int) },
                null);

            if (method == null)
                return null;

            return i => method.Invoke(virtualizingPanel, new object[] { i });
        }
    }
}
