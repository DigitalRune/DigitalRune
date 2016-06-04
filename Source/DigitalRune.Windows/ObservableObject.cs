// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.Serialization;
#if NETFX_CORE || NET45
using System.Runtime.CompilerServices;
#endif


namespace DigitalRune.Windows
{
    /// <summary>
    /// Provides a base class for all <see cref="INotifyPropertyChanged"/> implementations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Properties:</strong><br/>
    /// The following example shows to implement a property in a derived class that raises the
    /// <see cref="PropertyChanged"/> event.
    /// <code lang="csharp">
    /// <![CDATA[
    /// public string FirstName
    /// {
    ///   get { return _firstName; }
    ///   set
    ///   {
    ///     if (_firstName == value)
    ///       return;
    /// 
    ///     _firstName = value;
    ///     RaisePropertyChanged();
    ///   }
    /// }
    /// private string _firstName;
    /// ]]>
    /// </code>
    /// </para>
    /// <para>
    /// The code can be shortened using the <see cref="SetProperty{T}"/> method.
    /// <code lang="csharp">
    /// <![CDATA[
    /// public string FirstName
    /// {
    ///   get { return _firstName; }
    ///   set { SetProperty(ref _firstName, value); }
    /// }
    /// private string _firstName;
    /// ]]>
    /// </code>
    /// </para>
    /// <para>
    /// The following example shows how to raise additional <see cref="PropertyChanged"/> events.
    /// <code lang="csharp">
    /// <![CDATA[
    /// public string FirstName
    /// {
    ///   get { return _firstName; }
    ///   set 
    ///   { 
    ///     if (SetProperty(ref _firstName, value))
    ///       RaisePropertyChanged(() => FullName);
    ///   }
    /// }
    /// private string _firstName;
    /// 
    /// public string LastName
    /// {
    ///   get { return _lastName; }
    ///   set 
    ///   { 
    ///     if (SetProperty(ref _lastName, value))
    ///       RaisePropertyChanged(() => FullName);
    ///   }
    /// }
    /// private string _lastName;
    /// 
    /// public string FullName 
    /// { 
    ///   get { return _FirstName + " " + _LastName; }
    /// }
    /// ]]>
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Indexers:</strong><br/>
    /// The method <see cref="RaisePropertyChanged{T}"/> does not support indexers. To raise a
    /// <see cref="PropertyChanged"/> notification when the value of an indexer changes use the
    /// following code in WPF:
    /// <code lang="csharp">
    /// <![CDATA[
    /// public string this[string key]
    /// {
    ///   get { return _items[key]; }
    ///   set
    ///   {
    ///     _items[key] = value;
    ///     RaisePropertyChanged(Binding.IndexerName);
    ///   }
    /// }
    /// ]]>
    /// </code>
    /// </para>
    /// <para>
    /// And in Silverlight use the following code:
    /// <code lang="csharp">
    /// <![CDATA[
    /// public string this[string key]
    /// {
    ///   get { return _items[key]; }
    ///   set
    ///   {
    ///     _items[key] = value;
    ///     RaisePropertyChanged(string.Format("Item[{0}]", key));
    ///   }
    /// }
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX
    [Serializable]
#endif
    [DataContract(IsReference = true)]
    public abstract class ObservableObject : INotifyPropertyChanged
    {
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX
        [field: NonSerialized]
#endif
        private bool _enableUIThreadMarshaling;


        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="PropertyChanged"/> event is
        /// raised on the UI thread.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="PropertyChanged"/> event is raised
        /// asynchronously on the UI thread; otherwise, <see langword="false"/> if the
        /// <see cref="PropertyChanged"/> event is raised synchronously on the current thread. The
        /// default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// Changes of a view model that affect the view should be performed on the UI thread.
        /// Therefore UI thread marshaling is usually not necessary and disabled by default.
        /// </remarks>
        protected bool EnableUIThreadMarshaling
        {
            get { return _enableUIThreadMarshaling; }
            set { _enableUIThreadMarshaling = value; }
        }


        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX
        [field: NonSerialized]
#endif
        public event PropertyChangedEventHandler PropertyChanged;


#if NETFX_CORE || NET45
        /// <summary>
        /// Sets the value of the property and raises the <see cref="PropertyChanged"/> event. (Does
        /// nothing if the new value matches the current property value.)
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="field">The backing field storing the property value.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="propertyName">
        /// Optional: The name of the property. If the parameter is not provided, it is set
        /// automatically by the compiler using the <see cref="CallerMemberNameAttribute"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            return true;
        }
#endif


        /// <overloads>
        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event indicating that a certain property has
        /// changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="expression">
        /// A lambda expressions that selects the property that has changed. For example,
        /// <c>() =&gt; Value1</c>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="expression"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// It is recommended to call this method instead of calling <see cref="OnPropertyChanged"/>
        /// to raise the <see cref="PropertyChanged"/> event.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected void RaisePropertyChanged<T>(Expression<Func<T>> expression)
        {
#if DEBUG
            // (Only in DEBUG to avoid double check in RELEASE.)
            if (expression == null)
                throw new ArgumentNullException(nameof(expression), "expression cannot be null. Call RaisePropertyChanged() instead, if you want to indicate that all properties have changed.");
#endif

            string propertyName = ObjectHelper.GetPropertyName(expression);
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }


#if NETFX_CORE || NET45
        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event indicating that a certain property or all
        /// properties have changed.
        /// </summary>
        /// <param name="propertyName">
        /// Optional: The name of the property that has changed. To indicate that all properties
        /// have changed pass <see langword="null"/> or <see cref="string.Empty"/> as the parameter.
        /// If the parameter is not provided, it is set automatically by the compiler using the
        /// <see cref="CallerMemberNameAttribute"/>.
        /// </param>
        /// <remarks>
        /// It is recommended to call this method instead of calling <see cref="OnPropertyChanged"/>
        /// to raise the <see cref="PropertyChanged"/> event.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
#endif


        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="PropertyChangedEventArgs"/> describing the property that has changed.
        /// </param>
        /// <remarks>
        /// <para>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnPropertyChanged"/> in
        /// a derived class, be sure to call the base class's <see cref="OnPropertyChanged"/> method
        /// so that registered delegates receive the event.
        /// </para>
        /// <para>
        /// It is recommended to call <see cref="RaisePropertyChanged"/> or
        /// <see cref="RaisePropertyChanged{T}"/> instead of calling this method to raise the
        /// <see cref="PropertyChanged"/> event.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="eventArgs"/> is <see langword="null"/>.
        /// </exception>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException(nameof(eventArgs));

            var handler = PropertyChanged;
            if (handler != null)
            {
                if (EnableUIThreadMarshaling)
                {
                    // Ensure that the changed notifications are raised on the UI thread.
                    WindowsHelper.CheckBeginInvokeOnUI(() => handler(this, eventArgs));
                }
                else
                {
                    handler(this, eventArgs);
                }
            }
        }
    }
}
