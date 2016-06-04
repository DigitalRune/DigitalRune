// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;
using DigitalRune.Collections;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Provides the infrastructure for sending messages between loosely-coupled components.
    /// </summary>
    /// <inheritdoc cref="IMessageBus"/>
    public class MessageBus : IMessageBus
    {
        // Note: Unregistering message types (and thereby removing the internal subject) is not
        // supported, because it is dangerous. Another component might have already gotten a
        // reference to the IObservable<T>. If the message T is removed and then added again, the
        // subject will be different. Components that already have an IObservable<T> will not be
        // notified when a new message is broadcast.
        // Unregistering publishers is not supported because the IObservable<T> interface does not
        // provide an Unsubscribe-method. However, publisher can simply be stopped or garbage
        // collected. (They are not automatically kept alive!)

        private readonly Dictionary<Pair<Type, string>, object> _subjects = new Dictionary<Pair<Type, string>, object>();


        private object Gate
        {
            get { return ((ICollection)_subjects).SyncRoot; }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private ISubject<T> GetOrCreateSubject<T>(string token)
        {
            Pair<Type, string> key = new Pair<Type, string>(typeof(T), token);
            lock (Gate)
            {
                object value;
                if (_subjects.TryGetValue(key, out value))
                    return (ISubject<T>)value;

                var subject = new WeakSubject<T>();
                _subjects[key] = subject;
                return subject;
            }
        }


        /// <inheritdoc cref="IMessageBus.Listen{T}"/>
        public IObservable<T> Listen<T>(string token = null)
        {
            return GetOrCreateSubject<T>(token);
        }


        ///// <inheritdoc/>
        //public bool IsRegistered<T>(string token = null)
        //{
        //  lock (Gate)
        //  {
        //    return _subjects.ContainsKey(new Pair<Type, string>(typeof(T), token));
        //  }
        //}


        /// <inheritdoc/>
        public IDisposable RegisterPublisher<T>(IObservable<T> publisher, string token = null)
        {
            if (publisher == null)
                throw new ArgumentNullException(nameof(publisher));

            return publisher.Subscribe(GetOrCreateSubject<T>(token));
        }


        /// <inheritdoc/>
        public void Publish<T>(T message, string token = null)
        {
            GetOrCreateSubject<T>(token).OnNext(message);
        }
    }
}
