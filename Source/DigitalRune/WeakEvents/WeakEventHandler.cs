#region ----- Copyright -----
/*
  The classes in this file are based on and the WeakEventHandler from Daniel Grundwald's article 
  "Weak Events in C#" (http://www.codeproject.com/KB/cs/WeakEvents.aspx) which is licensed under 
  the MIT License (see below).
 
 
  Copyright (c) 2008 Daniel Grunwald
 
  Permission is hereby granted, free of charge, to any person
  obtaining a copy of this software and associated documentation
  files (the "Software"), to deal in the Software without
  restriction, including without limitation the rights to use,
  copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the
  Software is furnished to do so, subject to the following
  conditions:
 
  The above copyright notice and this permission notice shall be
  included in all copies or substantial portions of the Software.
 
  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
  OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
  OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion


using System;
using System.Diagnostics.CodeAnalysis;


namespace DigitalRune
{
  /// <summary>
  /// Represents subscription of a weak event handler to an event. (Can be used to detach the event 
  /// handler. Is automatically disposed if the event handler is garbage-collected.)
  /// </summary>
  internal sealed class WeakEventSubscription : IDisposable
  {
    /// <summary>
    /// Gets or sets a callback that detaches the event handler from the event source.
    /// </summary>
    /// <value>The callback that detaches the event handler from the event source.</value>
    public Action RemoveHandler { get; set; }


    /// <summary>
    /// Gets the object listening to the event.
    /// </summary>
    /// <value>
    /// The object listening to the event. Can be <see langword="null"/> if the object has been
    /// garbage-collected.
    /// </value>
    public object Listener
    {
      get { return _weakListener.Target; }
      set { _weakListener.Target = value; }
    }
    private readonly WeakReference _weakListener = new WeakReference(null);


    /// <summary>
    /// Detaches the event handler from the event.
    /// </summary>
    public void Dispose()
    {
      var removeHandler = RemoveHandler;
      if (removeHandler != null)
      {
        removeHandler();

        // Cleanup.
        RemoveHandler = null;
        Listener = null;
      }
    }
  }


  /// <summary>
  /// Helper class to add weak event handlers to events of type <see cref="EventHandler"/>.
  /// </summary>
  [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
  public static class WeakEventHandler
  {
    /// <summary>
    /// Registers an event handler that works with a weak reference to the target object.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    /// <typeparam name="TListener">The type of the listener.</typeparam>
    /// <param name="sender">The object that provides the event.</param>
    /// <param name="listener">The object that listens to the event.</param>
    /// <param name="addHandler">A callback method that adds an event handler to the event.</param>
    /// <param name="removeHandler">
    /// A callback method that removes an event handler from the event.
    /// </param>
    /// <param name="forwardEvent">
    /// A callback method that forwards the event to the actual event handler.
    /// </param>
    /// <returns>
    /// An <see cref="IDisposable"/> which can be used to detach the weak event handler from the 
    /// event.
    /// </returns>
    /// <remarks>
    /// Access to the event and to the real event handler is done through lambda expressions. The
    /// code holds strong references to these expressions, so they must not capture any variables of
    /// the target object (listener)!
    /// </remarks>
    /// <example>
    /// <code lang="csharp">
    /// <![CDATA[
    /// WeakEventHandler.Register(
    ///     textDocument,
    ///     this,
    ///     (sender, eventHandler) => sender.Changed += eventHandler,
    ///     (sender, eventHandler) => sender.Changed -= eventHandler,
    ///     (listener, sender, eventArgs) => listener.OnDocumentChanged(sender, eventArgs));
    /// ]]>
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sender"/>, <paramref name="addHandler"/>, <paramref name="removeHandler"/>, 
    /// <paramref name="listener"/>, or <paramref name="forwardEvent"/> is <see langword="null"/>.
    /// </exception>
    public static IDisposable Register<TSender, TListener>(
      TSender sender, 
      TListener listener, 
      Action<TSender, EventHandler> addHandler, 
      Action<TSender, EventHandler> removeHandler, 
      Action<TListener, object, EventArgs> forwardEvent)
      where TSender : class
      where TListener : class
    {
      if (sender == null)
        throw new ArgumentNullException("sender");
      if (listener == null)
        throw new ArgumentNullException("listener");
      if (addHandler == null)
        throw new ArgumentNullException("addHandler");
      if (removeHandler == null)
        throw new ArgumentNullException("removeHandler");
      if (forwardEvent == null)
        throw new ArgumentNullException("forwardEvent");

      var weakEventSubscription = new WeakEventSubscription();
      weakEventSubscription.Listener = listener;
      EventHandler eventHandler = (s, e) =>
                                  {
                                    TListener l = weakEventSubscription.Listener as TListener;
                                    if (l != null)
                                    {
                                      forwardEvent(l, s, e);
                                    }
                                    else
                                    {
                                      weakEventSubscription.Dispose();
                                    }
                                  };
      weakEventSubscription.RemoveHandler = () => removeHandler(sender, eventHandler);
      addHandler(sender, eventHandler);
      return weakEventSubscription;
    }
  }


  /// <summary>
  /// Helper class to add weak event handlers to events of type
  /// <see cref="EventHandler{TEventArgs}"/>.
  /// </summary>
  /// <typeparam name="TEventArgs">The type of the event args.</typeparam>
  [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
  public static class WeakEventHandler<TEventArgs> where TEventArgs : EventArgs
  {
    /// <summary>
    /// Registers an event handler that works with a weak reference to the target object.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    /// <typeparam name="TListener">The type of the listener.</typeparam>
    /// <param name="sender">The object that provides the event.</param>
    /// <param name="listener">The object that listens to the event.</param>
    /// <param name="addHandler">A callback method that adds an event handler to the event.</param>
    /// <param name="removeHandler">
    /// A callback method that removes an event handler from the event.
    /// </param>
    /// <param name="forwardEvent">
    /// A callback method that forwards the event to the actual event handler.
    /// </param>
    /// <returns>
    /// An <see cref="IDisposable"/> which can be used to detach the weak event handler from the 
    /// event.
    /// </returns>
    /// <remarks>
    /// Access to the event and to the real event handler is done through lambda expressions. The
    /// code holds strong references to these expressions, so they must not capture any variables of
    /// the target object (listener)!
    /// </remarks>
    /// <example>
    /// <code lang="csharp">
    /// <![CDATA[
    /// WeakEventHandler<DocumentChangeEventArgs>.Register(
    ///    textDocument,
    ///    this,
    ///    (sender, eventHandler) => sender.Changed += eventHandler,
    ///    (sender, eventHandler) => sender.Changed -= eventHandler,
    ///    (listener, sender, eventArgs) => listener.OnDocumentChanged(sender, eventArgs));
    /// ]]>
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sender"/>, <paramref name="addHandler"/>, <paramref name="removeHandler"/>,
    /// <paramref name="listener"/>, or <paramref name="forwardEvent"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static IDisposable Register<TSender, TListener>(
      TSender sender, 
      TListener listener, 
      Action<TSender, EventHandler<TEventArgs>> addHandler, 
      Action<TSender, EventHandler<TEventArgs>> removeHandler, 
      Action<TListener, object, TEventArgs> forwardEvent)
      where TSender : class
      where TListener : class
    {
      if (sender == null)
        throw new ArgumentNullException("sender");
      if (listener == null)
        throw new ArgumentNullException("listener");
      if (addHandler == null)
        throw new ArgumentNullException("addHandler");
      if (removeHandler == null)
        throw new ArgumentNullException("removeHandler");
      if (forwardEvent == null)
        throw new ArgumentNullException("forwardEvent");

      var weakEventSubscription = new WeakEventSubscription();
      weakEventSubscription.Listener = listener;
      EventHandler<TEventArgs> eventHandler = (s, e) =>
                                              {
                                                TListener l = weakEventSubscription.Listener as TListener;
                                                if (l != null)
                                                {
                                                  forwardEvent(l, s, e);
                                                }
                                                else
                                                {
                                                  weakEventSubscription.Dispose();
                                                }
                                              };
      weakEventSubscription.RemoveHandler = () => removeHandler(sender, eventHandler);
      addHandler(sender, eventHandler);
      return weakEventSubscription;
    }
  }


  /// <summary>
  /// Helper class to add weak event handlers to events of a certain type of event handler.
  /// </summary>
  /// <typeparam name="TEventHandler">The type of the event handler.</typeparam>
  /// <typeparam name="TEventArgs">The type of the event args.</typeparam>
  [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
  public static class WeakEventHandler<TEventHandler, TEventArgs> where TEventArgs : EventArgs
  {
    /// <summary>
    /// Registers an event handler that works with a weak reference to the target object.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    /// <typeparam name="TListener">The type of the listener.</typeparam>
    /// <param name="sender">The object that provides the event.</param>
    /// <param name="listener">The object that listens to the event.</param>
    /// <param name="conversion">
    /// A function used to convert the given event handler to a delegate compatible with the 
    /// underlying .NET event.
    /// </param>
    /// <param name="addHandler">A callback method that adds an event handler to the event.</param>
    /// <param name="removeHandler">
    /// A callback method that removes an event handler from the event.
    /// </param>
    /// <param name="forwardEvent">
    /// A callback method that forwards the event to the actual event handler.
    /// </param>
    /// <returns>
    /// An <see cref="IDisposable"/> which can be used to detach the weak event handler from the 
    /// event.
    /// </returns>
    /// <remarks>
    /// Access to the event and to the real event handler is done through lambda expressions. The
    /// code holds strong references to these expressions, so they must not capture any variables of
    /// the target object (listener)!
    /// </remarks>
    /// <example>
    /// <code lang="csharp">
    /// <![CDATA[
    /// WeakEventHandler<PropertyChangedEventHandler, PropertyChangedEventArgs>.Register(
    ///     textDocument,
    ///     this,
    ///     handler => new PropertyChangedEventHandler(handler),
    ///     (sender, pcHandler) => sender.PropertyChanged += pcHandler,
    ///     (sender, pcHandler) => sender.PropertyChanged -= pcHandler,
    ///     (listener, sender, eventArgs) => listener.OnDocumentChanged(sender, eventArgs));
    /// ]]>
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sender"/>, <paramref name="addHandler"/>, <paramref name="removeHandler"/>,
    /// <paramref name="listener"/>, or <paramref name="forwardEvent"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static IDisposable Register<TSender, TListener>(
      TSender sender,
      TListener listener,
      Func<EventHandler<TEventArgs>, TEventHandler> conversion,
      Action<TSender, TEventHandler> addHandler,
      Action<TSender, TEventHandler> removeHandler,
      Action<TListener, object, TEventArgs> forwardEvent)
      where TSender : class
      where TListener : class
    {
      if (sender == null)
        throw new ArgumentNullException("sender");
      if (listener == null)
        throw new ArgumentNullException("listener");
      if (conversion == null)
        throw new ArgumentNullException("conversion");
      if (addHandler == null)
        throw new ArgumentNullException("addHandler");
      if (removeHandler == null)
        throw new ArgumentNullException("removeHandler");
      if (forwardEvent == null)
        throw new ArgumentNullException("forwardEvent");

      WeakEventSubscription weakEventSubscription = new WeakEventSubscription();
      weakEventSubscription.Listener = listener;
      EventHandler<TEventArgs> genericEventHandler = (s, e) =>
                                                     {
                                                       TListener l = weakEventSubscription.Listener as TListener;
                                                       if (l != null)
                                                       {
                                                         forwardEvent(l, s, e);
                                                       }
                                                       else
                                                       {
                                                         weakEventSubscription.Dispose();
                                                       }
                                                     };
      TEventHandler customEventHandler = conversion(genericEventHandler);
      weakEventSubscription.RemoveHandler = () => removeHandler(sender, customEventHandler);
      addHandler(sender, customEventHandler);
      return weakEventSubscription;
    }
  }
}
