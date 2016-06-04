// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Synchronizes a <see cref="Window"/>'s <see cref="Window.DialogResult"/> property with the
    /// view model.
    /// </summary>
    /// <remarks>
    /// The <see cref="Window.DialogResult"/> property cannot be easily data bound because it is a
    /// normal CLR property instead of a dependency property. The <see cref="DialogResult"/>
    /// provides a way to data bind the property.
    /// </remarks>
    /// <example>
    /// The following shows a window which binds its <see cref="Window.DialogResult"/> property to
    /// the property of a view model.
    /// <code lang="csharp">
    /// <![CDATA[
    /// <Window x:Class="MyApplication.DialogView"
    ///         xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///         xmlns:i="http://schemas.microsoft.com/expression/2010/interactivity"
    ///         xmlns:dr="http://schemas.digitalrune.com/windows">
    ///   <i:Interaction.Behaviors>
    ///     <dr:DialogResultBehavior DialogResult="{Binding DialogResult, Mode=TwoWay}"/>
    ///   </i:Interaction.Behaviors>
    /// 
    ///   <Grid>
    ///     ...
    ///   </Grid>
    /// </Window>
    /// ]]>
    /// </code>
    /// </example>
    public class DialogResultBehavior : Behavior<Window>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="DialogResult"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DialogResultProperty = DependencyProperty.Register(
            "DialogResult",
            typeof(bool?),
            typeof(DialogResultBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDialogResultChanged));

        /// <summary>
        /// Gets or sets the <see cref="Window.DialogResult"/> value. 
        /// This is a dependency property.
        /// </summary>
        /// <value>The <see cref="Window.DialogResult"/> value.</value>
        [Description("Gets or sets the DialogResult value.")]
        [Category(Categories.Default)]
        public bool? DialogResult
        {
            get { return (bool?)GetValue(DialogResultProperty); }
            set { SetValue(DialogResultProperty, value); }
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
            AssociatedObject.Closed += OnWindowClosed;
        }


        /// <summary>
        /// Called when the behavior is being detached from its
        /// <see cref="Behavior{T}.AssociatedObject"/>, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.Closed -= OnWindowClosed;
            base.OnDetaching();
        }


        /// <summary>
        /// Called when the window is closed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        private void OnWindowClosed(object sender, EventArgs eventArgs)
        {
            // Synchronize DialogResult: Window --> ViewModel
            var window = sender as Window;
            if (window != null)
                DialogResult = window.DialogResult;
        }


        /// <summary>
        /// Called when the <see cref="DialogResult"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnDialogResultChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            // Synchronize DialogResult: ViewModel --> Window
            var behavior = (DialogResultBehavior)dependencyObject;
            var window = behavior.AssociatedObject;
            bool? dialogResult = (bool?)eventArgs.NewValue;
            if (window.IsLoaded && window.DialogResult != dialogResult)
                window.DialogResult = dialogResult;
        }
        #endregion
    }
}
