// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using DigitalRune.Collections;


namespace DigitalRune
{
    /// <summary>
    /// Represents an object that is both an observable sequence as well as an observer. All
    /// observers are stored using weak references.
    /// </summary>
    internal class WeakSubject<T> : ISubject<T>, IDisposable
    {
        //--------------------------------------------------------------
        #region Nested Types
        //--------------------------------------------------------------

        private class Subscription : IDisposable
        {
            private WeakSubject<T> _subject;
            private IObserver<T> _observer;

            public Subscription(WeakSubject<T> subject, IObserver<T> observer)
            {
                _subject = subject;
                _observer = observer;
            }

            public void Dispose()
            {
                // Use atomic exchange to ensure that Unsubscribe() is only called once, 
                // even if Dispose() is called simultaneously.
                IObserver<T> observer = Interlocked.Exchange(ref _observer, null);
                if (observer != null)
                {
                    _subject.Unsubscribe(observer);
                    _subject = null;
                }
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private WeakCollection<IObserver<T>> _observers;
        private Exception _error;
        private readonly object _gate = new object();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether this instance has been disposed of.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance has been disposed of; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        public bool IsDisposed { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakSubject{T}"/> class.
        /// </summary>
        public WeakSubject()
        {
            _observers = new WeakCollection<IObserver<T>>();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Subscribes an observer to the subject.
        /// </summary>
        /// <param name="observer">
        /// The <see cref="IObserver{T}"/> to subscribe to the subject.
        /// </param>
        /// <remarks>
        /// An <see cref="IDisposable"/> object that can be used to unsubscribe the observer from
        /// the subject.
        /// </remarks>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            lock (_gate)
            {
                ThrowIfDisposed();
                if (_observers != null)
                {
                    _observers.Add(observer);
                    return new Subscription(this, observer);
                }
                else
                {
                    // Subject is stopped.
                    if (_error != null)
                        observer.OnError(_error);
                    else
                        observer.OnCompleted();

                    return Disposable.Empty;
                }
            }
        }


        private void Unsubscribe(IObserver<T> observer)
        {
            lock (_gate)
                _observers?.Remove(observer);
        }


        /// <summary>
        /// Notifies all subscribed observers with the value.
        /// </summary>
        /// <param name="value">The value to send to all subscribed observers.</param>
        public void OnNext(T value)
        {
            IObserver<T>[] observers = null;
            lock (_gate)
            {
                ThrowIfDisposed();
                if (_observers != null)
                    observers = _observers.ToArray();
            }

            if (observers != null)
                foreach (var observer in observers)
                    observer.OnNext(value);
        }


        public void OnError(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            IObserver<T>[] observers = null;
            lock (_gate)
            {
                ThrowIfDisposed();
                if (_observers != null)
                {
                    observers = _observers.ToArray();
                    _observers = null;
                    _error = error;
                }
            }

            if (observers != null)
                foreach (var observer in observers)
                    observer.OnError(error);
        }


        /// <summary>
        /// Notifies all subscribed observers of the end of the sequence.
        /// </summary>
        public void OnCompleted()
        {
            IObserver<T>[] observers = null;
            lock (_gate)
            {
                ThrowIfDisposed();
                if (_observers != null)
                {
                    observers = _observers.ToArray();
                    _observers = null;
                }
            }

            if (observers != null)
                foreach (var observer in observers)
                    observer.OnCompleted();
        }


        /// <summary>
        /// Unsubscribe all observers and release resources.
        /// </summary>
        public void Dispose()
        {
            lock (_gate)
            {
                _observers = null;
                IsDisposed = true;
            }
        }


        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the service container has already
        /// been disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
        #endregion
    }
}
