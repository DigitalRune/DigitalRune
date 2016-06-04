// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Implements the <see cref="IWeakEventListener"/>.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of <see cref="EventArgs"/>.</typeparam>
    /// <remarks>
    /// The <see cref="WeakEventListener{TEventArgs}"/> can be used by a class to listen to a
    /// specific weak event without implementing the interface <see cref="IWeakEventListener"/>.
    /// </remarks>
    /// <example>
    /// In the following example a <see cref="WeakEventListener{TEventArgs}"/> is used to listen for
    /// the <see cref="INotifyPropertyChanged.PropertyChanged"/> event of another object without
    /// creating a strong reference from the source to the listener.
    /// <code lang="csharp">
    /// <![CDATA[
    /// INotifyPropertyChanged source = ...; // Any object that implements INotifyPropertyChanged.
    /// 
    /// var listener = new WeakEventListener<PropertyChangedEventArgs>(MyEventHandler);
    /// 
    /// // Attach the listener to the PropertyChanged event of source.
    /// PropertyChangedEventManager.AddListener(source, listener, String.Empty);
    /// 
    /// // Now, every time a property of source changes MyEventHandler is called.
    /// 
    /// // Detach the listener from the PropertyChanged event.
    /// PropertyChangedEventManager.RemoveListener(source, listener, String.Empty);
    /// ]]>
    /// </code>
    /// </example>
    [Obsolete("Should be obsolete in .NET 4.5. Use XxxEventManager.AddHandler instead.")]
    public class WeakEventListener<TEventArgs> : IWeakEventListener where TEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the event handler that is called when a weak event is received.
        /// </summary>
        /// <value>The event handler that is called when a weak event is received.</value>
        /// <remarks>
        /// The class <see cref="WeakEventListener{TEventArgs}"/> allows that the property
        /// <see cref="EventHandler"/> is <see langword="null"/>. However, it is considered an error
        /// by the <see cref="WeakEventManager"/> handling in WPF to register a listener for an
        /// event that the listener does not handle.
        /// </remarks>
        public EventHandler<TEventArgs> EventHandler { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="WeakEventListener{TEventArgs}"/> class.
        /// </summary>
        /// <param name="eventHandler">
        /// The event handler that is called when a weak event is received.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="eventHandler"/> is <see langword="null"/>.
        /// </exception>
        public WeakEventListener(EventHandler<TEventArgs> eventHandler)
        {
            EventHandler = eventHandler;
        }


        /// <summary>
        /// Receives the weak event.
        /// </summary>
        /// <param name="managerType">Type of the manager.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns>
        /// <see langword="true"/> if the listener handled the event. It is considered an error by
        /// the <see cref="WeakEventManager"/> handling in WPF to register a listener for an event
        /// that the listener does not handle. Regardless, the method should return
        /// <see langword="false"/> if it receives an event that it does not recognize or handle.
        /// </returns>
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs eventArgs)
        {
            if (EventHandler != null && eventArgs is TEventArgs)
            {
                EventHandler(sender, (TEventArgs)eventArgs);
                return true;
            }

            return false;
        }
    }
}
