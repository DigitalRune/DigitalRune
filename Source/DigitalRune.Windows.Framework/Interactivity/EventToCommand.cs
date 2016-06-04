#region ----- Copyright -----
/*
   The EventToCommand class is taken from the MVVM Light Toolkit (see http://mvvmlight.codeplex.com/)
   by Laurent Bugnion which is licensed under the MIT License (MIT).

   Copyright (c) 2009 Laurent Bugnion (GalaSoft), laurent@galasoft.ch

   Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
   associated documentation files (the "Software"), to deal in the Software without restriction, 
   including without limitation the rights to use, copy, modify, merge, publish, distribute, 
   sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
   furnished to do so, subject to the following conditions:

   The above copyright notice and this permission notice shall be included in all copies or 
   substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
  NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
  DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT 
  OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/
#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// This <see cref="System.Windows.Interactivity.TriggerAction"/> can be used to bind any event
    /// on any <see cref="FrameworkElement"/> to an <see cref="ICommand"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Typically, this element is used in XAML to connect the attached element to a command located
    /// in a view model. This trigger can only be attached to a <see cref="FrameworkElement"/> or a
    /// class deriving from <see cref="FrameworkElement"/>.
    /// </para>
    /// <para>
    /// To access the <see cref="EventArgs"/> of the fired event, use a
    /// <c>DelegateCommand&lt;EventArgs&gt;</c> set <see cref="PassEventArgsToCommand"/> to
    /// <see langword="true"/> and leave the <see cref="CommandParameter"/> and
    /// <see cref="CommandParameterValue"/> empty!
    /// </para>
    /// </remarks>
    public class EventToCommand : TriggerAction<DependencyObject>
    {
        /// <summary>
        /// Identifies the <see cref="CommandParameter" /> dependency property
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter",
            typeof(object),
            typeof(EventToCommand),
            new PropertyMetadata(
                null,
                (s, e) =>
                {
                    var sender = s as EventToCommand;
                    if (sender == null)
                    {
                        return;
                    }

                    if (sender.AssociatedObject == null)
                    {
                        return;
                    }

                    sender.EnableDisableElement();
                }));


        /// <summary>
        /// Identifies the <see cref="Command" /> dependency property
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(EventToCommand),
            new PropertyMetadata(
                null,
                (s, e) => OnCommandChanged(s as EventToCommand, e)));


        /// <summary>
        /// Identifies the <see cref="MustToggleIsEnabled" /> dependency property
        /// </summary>
        public static readonly DependencyProperty MustToggleIsEnabledProperty = DependencyProperty.Register(
            "MustToggleIsEnabled",
            typeof(bool),
            typeof(EventToCommand),
            new PropertyMetadata(
                Boxed.BooleanFalse,
                (s, e) =>
                {
                    var sender = s as EventToCommand;
                    if (sender == null)
                    {
                        return;
                    }

                    if (sender.AssociatedObject == null)
                    {
                        return;
                    }

                    sender.EnableDisableElement();
                }));


        private object _commandParameterValue;

        private bool? _mustToggleValue;


        /// <summary>
        /// Gets or sets the <see cref="ICommand"/> that this trigger is bound to. This
        /// is a dependency property.
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }

            set { SetValue(CommandProperty, value); }
        }


        /// <summary>
        /// Gets or sets an object that will be passed to the <see cref="Command"/> attached to this
        /// trigger. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// Because of limitations of Silverlight, you can only set data-bindings on this property.
        /// If you wish to use hard coded values, use the <see cref="CommandParameterValue"/>
        /// property.
        /// </remarks>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }

            set { SetValue(CommandParameterProperty, value); }
        }


        /// <summary>
        /// Gets or sets an object that will be passed to the <see cref="Command"/> attached to this
        /// trigger.
        /// </summary>
        /// <remarks>
        /// This is NOT a dependency property. Use this property if you want to set a hard coded
        /// value. For data-binding, use the <see cref="CommandParameter"/> property.
        /// </remarks>
        public object CommandParameterValue
        {
            get { return _commandParameterValue ?? CommandParameter; }

            set
            {
                _commandParameterValue = value;
                EnableDisableElement();
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the attached element must be disabled when the
        /// <see cref="Command"/> property's <see cref="ICommand.CanExecuteChanged"/> event fires.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// If this property is <see langword="true"/>, and the command's
        /// <see cref="ICommand.CanExecute"/> method returns <see langword="false"/>, the element
        /// will be disabled. If this property is <see langword="false"/>, the element will not be
        /// disabled when the command's <see cref="ICommand.CanExecute"/> method changes.
        /// </value>
        /// <remarks>
        /// <para>
        /// If the attached element is not a <see cref="Control"/>, this property has no effect.
        /// </para>
        /// <para>
        /// Because of limitations of Silverlight, you can only set data-bindings on this property.
        /// If you wish to use hard coded values, use the <see cref="CommandParameterValue"/>
        /// property.
        /// </para>
        /// </remarks>
        public bool MustToggleIsEnabled
        {
            get { return (bool)GetValue(MustToggleIsEnabledProperty); }

            set { SetValue(MustToggleIsEnabledProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the attached element must be disabled when the
        /// <see cref="Command"/> property's <see cref="ICommand.CanExecuteChanged"/> event fires.
        /// </summary>
        /// <value>
        /// If this property is <see langword="true"/>, and the command's
        /// <see cref="ICommand.CanExecute"/> method returns <see langword="false"/>, the element
        /// will be disabled. If this property is <see langword="false"/>, the element will not be
        /// disabled when the command's <see cref="ICommand.CanExecute"/> method changes.
        /// </value>
        /// <remarks>
        /// <para>
        /// If the attached element is not a <see cref="Control"/>, this property has no effect.
        /// </para>
        /// <para>
        /// This property is here for compatibility with the Silverlight version. This is NOT a
        /// dependency property. Use this property if you want to set a hard coded value. For
        /// data-binding, use the <see cref="MustToggleIsEnabled"/> property.
        /// </para>
        /// </remarks>
        public bool MustToggleIsEnabledValue
        {
            get
            {
                return _mustToggleValue ?? MustToggleIsEnabled;
            }

            set
            {
                _mustToggleValue = value;
                EnableDisableElement();
            }
        }


        /// <summary>
        /// Called when this trigger is attached to a <see cref="FrameworkElement"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            EnableDisableElement();
        }


#if SILVERLIGHT || WINDOWS_PHONE
        private Control GetAssociatedObject()
        {
            return AssociatedObject as Control;
        }
#else
        /// <summary>
        /// This method is here for compatibility with the Silverlight version.
        /// </summary>
        /// <returns>The <see cref="FrameworkElement"/> to which this trigger is attached.</returns>
        private FrameworkElement GetAssociatedObject()
        {
            return AssociatedObject as FrameworkElement;
        }
#endif


        /// <summary>
        /// This method is here for compatibility
        /// with the Silverlight 3 version.
        /// </summary>
        /// <returns>The command that must be executed when
        /// this trigger is invoked.</returns>
        private ICommand GetCommand()
        {
            return Command;
        }


        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="EventArgs"/> passed to the event
        /// handler will be forwarded to the <see cref="ICommand"/>'s <see cref="ICommand.Execute"/>
        /// method when the event is fired (if the bound <see cref="ICommand"/> accepts an argument
        /// of type <see cref="EventArgs"/>).
        /// </summary>
        /// <remarks>
        /// For example, use a <c>DelegateCommand&lt;MouseEventArgs&gt;</c> to get the arguments of
        /// a <see cref="UIElement.MouseMove"/> event.
        /// </remarks>
        public bool PassEventArgsToCommand { get; set; }


        /// <summary>
        /// Provides a simple way to invoke this trigger programmatically without any 
        /// <see cref="EventArgs"/>.
        /// </summary>
        public void Invoke()
        {
            Invoke(null);
        }


        /// <summary>
        /// Executes the trigger.
        /// </summary>
        /// <param name="parameter">The <see cref="EventArgs"/> of the fired event.</param>
        /// <remarks>
        /// To access the <see cref="EventArgs"/> of the fired event, use a
        /// <c>DelegateCommand&lt;EventArgs&gt;</c> and leave the <see cref="CommandParameter"/> and
        /// <see cref="CommandParameterValue"/> empty!
        /// </remarks>
        protected override void Invoke(object parameter)
        {
            if (AssociatedElementIsDisabled())
                return;

            var command = GetCommand();
            var commandParameter = CommandParameterValue;

            if (commandParameter == null && PassEventArgsToCommand)
                commandParameter = parameter;

            if (command != null && command.CanExecute(commandParameter))
                command.Execute(commandParameter);
        }


        private static void OnCommandChanged(EventToCommand element, DependencyPropertyChangedEventArgs e)
        {
            if (element == null)
                return;

            if (e.OldValue != null)
            {
#if NET45
                CanExecuteChangedEventManager.RemoveHandler((ICommand)e.OldValue, element.OnCommandCanExecuteChanged);
#else
                ((ICommand)e.OldValue).CanExecuteChanged -= element.OnCommandCanExecuteChanged;
#endif
            }

            var command = (ICommand)e.NewValue;
            if (command != null)
            {
#if NET45
                CanExecuteChangedEventManager.AddHandler(command, element.OnCommandCanExecuteChanged);
#else
                command.CanExecuteChanged += element.OnCommandCanExecuteChanged;
#endif
            }

            element.EnableDisableElement();
        }


        private bool AssociatedElementIsDisabled()
        {
            var element = GetAssociatedObject();
            return AssociatedObject == null
                   || (element != null && !element.IsEnabled);
        }


        private void EnableDisableElement()
        {
            var element = GetAssociatedObject();
            if (element == null)
                return;

            var command = GetCommand();
            if (MustToggleIsEnabledValue && command != null)
                element.IsEnabled = command.CanExecute(CommandParameterValue);
        }


#if SILVERLIGHT || WINDOWS_PHONE
        // The CanExecuteChanged handler needs to be public because of security restrictions in 
        // Silverlight. (The CanExecuteChanged event is usually implemented as a weak-event which 
        // requires reflection.)

        /// <exclude/>
        public
#else
        private
#endif
        void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            EnableDisableElement();
        }
    }
}
