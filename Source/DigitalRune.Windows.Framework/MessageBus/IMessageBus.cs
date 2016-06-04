// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Supports messaging between loosely-coupled components.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Any object can publish a message using the <see cref="Publish{T}"/> method. Other objects
    /// can subscribe to certain messages by calling <see cref="Listen{T}"/>. Messages are
    /// identified via their type and optional token. If the publisher specifies a token, only
    /// subscribers that have registered using this token will receive the messages.
    /// </para>
    /// <para>
    /// <strong>Important:</strong><br/>
    /// Subscribers are stored using weak references to prevent memory leaks. When a listener
    /// subscribes to a message it is important that the listener stores the
    /// <see cref="IDisposable"/> returned by the method <see cref="IObservable{T}.Subscribe"/>. The
    /// <see cref="IDisposable"/> can be used to explicitly unsubscribe from the message bus.
    /// Further, if the <see cref="IDisposable"/> is garbage collected the listener is also
    /// unsubscribed!
    /// </para>
    /// </remarks>
    public interface IMessageBus
    {
        /// <summary>
        /// Provides an <see cref="IObservable{T}"/> which can be used to listen to the specified
        /// type of messages.
        /// </summary>
        /// <typeparam name="T">The type of the message to listen to.</typeparam>
        /// <param name="token">
        /// An optional string which is used to distinguish messages of the same type. When
        /// specified, subscribers will only receive messages that are published using the same
        /// token.
        /// </param>
        /// <returns>An <see cref="IObservable{T}"/> providing the message notifications.</returns>
        /// <remarks>
        /// <strong>Important:</strong><br/>
        /// Subscribers are stored using weak references to prevent memory leaks. When a listener
        /// subscribes to a message it is important that the listener stores the
        /// <see cref="IDisposable"/> returned by the method <see cref="IObservable{T}.Subscribe"/>.
        /// The <see cref="IDisposable"/> can be used to explicitly unsubscribe from the message
        /// bus. Further, if the <see cref="IDisposable"/> is garbage collected the listener is also
        /// unsubscribed!
        /// </remarks>
        IObservable<T> Listen<T>(string token = null);


        ///// <summary>
        ///// Determines whether the specified message type is registered.
        ///// </summary>
        ///// <typeparam name="T">The type of the message.</typeparam>
        ///// <param name="token">
        ///// An optional string which is used to distinguish messages of the same type.
        ///// </param>
        ///// <returns>
        ///// <see langword="true"/> if the specified message type is registered; otherwise,
        ///// <see langword="false"/>.
        ///// </returns>
        ///// <remarks>
        ///// This method returns <see langword="true"/> when messages of the given type have been
        ///// published in the past or when objects have subscribed to this message type. It does not
        ///// return whether there are any active publisher or subscribers for the given message type.
        ///// </remarks>
        //bool IsRegistered<T>(string token = null);


        /// <summary>
        /// Registers a publisher and broadcast its messages over the message bus.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="publisher">The message source.</param>
        /// <param name="token">
        /// An optional string which is used to distinguish messages of the same type. When
        /// specified, subscribers will only receive messages that are published using the same
        /// token.
        /// </param>
        /// <returns>
        /// An <see cref="IDisposable"/> object that can be used to unregister the publisher from
        /// the message bus.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Important:</strong><br/>
        /// If the <paramref name="publisher"/> terminates (e.g. by calling
        /// <see cref="IObserver{T}.OnError"/> or <see cref="IObserver{T}.OnCompleted"/>) then the
        /// message bus will stop broadcasting message of the given type and token. Registering
        /// another publisher of the same type or calling <see cref="Publish{T}"/> explicitly, will
        /// have no effect once a message channel has terminated!
        /// </para>
        /// <para>
        /// The message bus does not store a strong reference to the publisher, that means the
        /// publisher is not automatically kept alive. (If no other component holds a strong
        /// reference then the publisher will be garbage collected.)
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="publisher"/> is <see langword="null"/>.
        /// </exception>
        IDisposable RegisterPublisher<T>(IObservable<T> publisher, string token = null);


        /// <summary>
        /// Broadcasts the specified message over the message bus.
        /// </summary>
        /// <typeparam name="T">The type of the message to broadcast.</typeparam>
        /// <param name="message">The message to send. Can be <see langword="null"/>.</param>
        /// <param name="token">
        /// An optional string which is used to distinguish messages of the same type. When
        /// specified, subscribers will only receive messages that are published using the token.
        /// </param>
        void Publish<T>(T message, string token = null);
    }
}
