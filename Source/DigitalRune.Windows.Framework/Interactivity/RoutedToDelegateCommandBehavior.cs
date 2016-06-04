// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Forwards a <see cref="RoutedCommand"/> to a <see cref="Framework.DelegateCommand"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// View models cannot easily consume <see cref="RoutedCommand"/>s. This behavior provides a 
    /// solution: Attach this behavior to the view. Specify the <see cref="RoutedCommand"/> that 
    /// should be handled, and specify a <see cref="DelegateCommand"/> in the view model. The
    /// <see cref="System.Windows.Input.RoutedCommand.Execute"/> and
    /// <see cref="System.Windows.Input.RoutedCommand.CanExecute"/> methods are forwarded from the
    /// <see cref="RoutedCommand"/> to the <see cref="DelegateCommand"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// The PrintPreview routed command is forwarded to the PrintPreviewCommand, which is a command
    /// in the view model.
    /// <code lang="csharp">
    /// <![CDATA[
    /// <Window x:Class="MyApplication.MyView"
    ///         xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///         xmlns:i="http://schemas.microsoft.com/expression/2010/interactivity"
    ///         xmlns:dr="http://schemas.digitalrune.com/windows">
    ///   <i:Interaction.Behaviors>
    ///     <dr:RoutedToDelegateCommandBehavior RoutedCommand="PrintPreview" DelegateCommand="{Binding PrintPreviewCommand}"/>
    ///   </i:Interaction.Behaviors>
    /// 
    ///   <Grid>
    ///     ...
    ///   </Grid>
    /// </Window>
    /// ]]>
    /// </code>
    /// </example>
    public class RoutedToDelegateCommandBehavior : Behavior<FrameworkElement>
    {
        // An alternative to this behavior is the ICommandSink pattern: 
        // The view model implements ICommandSink (or has uses a default 
        // CommandSink instance); and the view specifies the CommandSinkBinding.CommandSink property 
        // and uses CommandSinkBindings instead of regular CommandBindings.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private CommandBinding _commandBinding;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="System.Windows.Input.RoutedCommand"/>.
        /// </summary>
        /// <value>The <see cref="System.Windows.Input.RoutedCommand"/>.</value>
        public RoutedCommand RoutedCommand
        {
            get { return _routedCommand; }
            set
            {
                _routedCommand = value;
                Uninstall();
                Install();
            }
        }
        private RoutedCommand _routedCommand;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="DelegateCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DelegateCommandProperty = DependencyProperty.Register(
            "DelegateCommand",
            typeof(ICommand),
            typeof(RoutedToDelegateCommandBehavior),
            new FrameworkPropertyMetadata(null, OnDelegateCommandChanged));

        /// <summary>
        /// Gets or sets the <see cref="Framework.DelegateCommand"/> of the view model. 
        /// This is a dependency property.
        /// </summary>
        /// <value>The <see cref="Framework.DelegateCommand"/> of the view model.</value>
        [Description("Gets or sets the delegate command of the view model.")]
        [Category(Categories.Default)]
        public ICommand DelegateCommand
        {
            get { return (ICommand)GetValue(DelegateCommandProperty); }
            set { SetValue(DelegateCommandProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="CommandParameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter",
            typeof(object),
            typeof(RoutedToDelegateCommandBehavior),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the command parameter.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The command parameter. (Overrides the default command parameter - see remarks.)
        /// </value>
        /// <remarks>
        /// By default the command parameter of the <see cref="RoutedCommand"/> is passed to the 
        /// <see cref="DelegateCommand"/>. But this property can be set to override the default 
        /// parameter and pass a custom value to the <see cref="DelegateCommand"/>.
        /// </remarks>
        [Description("Gets or sets the command parameter.")]
        [Category(Categories.Default)]
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
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
            Install();
        }


        /// <summary>
        /// Called when the behavior is being detached from its <see cref="Behavior{T}.AssociatedObject"/>, 
        /// but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            Uninstall();
            base.OnDetaching();
        }


        /// <summary>
        /// Called when the <see cref="DelegateCommand"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnDelegateCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (RoutedToDelegateCommandBehavior)dependencyObject;

            var oldCommand = eventArgs.OldValue as ICommand;
            if (oldCommand != null)
                oldCommand.CanExecuteChanged -= behavior.OnCanExecuteChanged;

            behavior.Uninstall();
            behavior.Install();
        }


        private void Install()
        {
            if (AssociatedObject != null && RoutedCommand != null && DelegateCommand != null)
            {
                DelegateCommand.CanExecuteChanged += OnCanExecuteChanged;

                _commandBinding = new CommandBinding(RoutedCommand, OnExecuted, OnCanExecute);
                AssociatedObject.CommandBindings.Add(_commandBinding);
            }
        }


        private void Uninstall()
        {
            if (DelegateCommand != null)
                DelegateCommand.CanExecuteChanged -= OnCanExecuteChanged;

            if (AssociatedObject != null && _commandBinding != null)
            {
                AssociatedObject.CommandBindings.Remove(_commandBinding);
                _commandBinding = null;
            }
        }


        private void OnCanExecuteChanged(object sender, EventArgs eventArgs)
        {
            CommandManager.InvalidateRequerySuggested();
        }


        private void OnCanExecute(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            if (DelegateCommand != null)
            {
                eventArgs.CanExecute = DelegateCommand.CanExecute(CommandParameter ?? eventArgs.Parameter);
                eventArgs.Handled = true;
            }
        }


        private void OnExecuted(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            if (DelegateCommand != null)
            {
                DelegateCommand.Execute(CommandParameter ?? eventArgs.Parameter);
                eventArgs.Handled = true;
            }
        }
        #endregion
    }
}
