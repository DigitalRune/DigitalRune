#region ----- Copyright -----
/*
  The class in this file is based on and the SmartWeakEvent/FastWeakEvent from Daniel Grundwald's 
  article "Weak Events in C#" (http://www.codeproject.com/KB/cs/WeakEvents.aspx) which is licensed 
  under the MIT License (see below).
 
 
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
using System.Reflection;


namespace DigitalRune
{
  /// <summary>
  /// Represents an event that stores the target objects as weak 
  /// references.
  /// </summary>
  /// <typeparam name="T">
  /// The type of event handler. Must be of type <see cref="EventHandler"/> or 
  /// <see cref="EventHandler{TEventArgs}"/>.
  /// </typeparam>
  /// <remarks>
  /// <strong>Important:</strong> In Silverlight, the event handlers that handle the weak event 
  /// need to be public methods (no private, protected or anonymous methods). This is necessary 
  /// because of security restrictions in Silverlight.
  /// </remarks>
  /// <example>
  /// The following examples shows how a class can implement a weak event.
  /// <code lang="csharp">
  /// <![CDATA[
  /// class MyEventSource
  /// {
  ///   private readonly WeakEvent<EventHandler<EventArgs>> _myEvent = new WeakEvent<EventHandler<EventArgs>>();
  /// 
  ///   public event EventHandler<EventArgs> MyEvent
  ///   {
  ///     add { _myEvent.Add(value); }
  ///     remove { _myEvent.Remove(value); }
  ///   }
  ///   
  ///   protected virtual void OnMyEvent(EventArgs eventArgs)
  ///   {
  ///     _myEvent.Invoke(this, eventArgs);
  ///   }  
  /// }  
  /// ]]>
  /// </code>
  /// </example>
  public sealed class WeakEvent<T> : WeakMulticastDelegate<T> where T : class
  {
    /// <summary>
    /// Initializes static members of the <see cref="WeakEvent{T}"/> class.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <typeparamref name="T"/> is not of type <see cref="EventHandler"/> or 
    /// <see cref="EventHandler{TEventArgs}"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    static WeakEvent()
    {
#if !NETFX_CORE && !NET45
      MethodInfo invoke = typeof(T).GetMethod("Invoke");
#else
      MethodInfo invoke = typeof(T).GetTypeInfo().GetDeclaredMethod("Invoke");
#endif

      if (invoke == null || invoke.GetParameters().Length != 2)
        throw new ArgumentException("T must be a delegate type taking 2 parameters");

      ParameterInfo senderParameter = invoke.GetParameters()[0];
      if (senderParameter.ParameterType != typeof(object))
        throw new ArgumentException("The first delegate parameter must be of type 'object'");

      ParameterInfo argsParameter = invoke.GetParameters()[1];
#if !NETFX_CORE && !NET45
      if (!(typeof(EventArgs).IsAssignableFrom(argsParameter.ParameterType)))
#else
      if (!(typeof(EventArgs).GetTypeInfo().IsAssignableFrom(argsParameter.ParameterType.GetTypeInfo())))
#endif
      throw new ArgumentException("The second delegate parameter must be derived from type 'EventArgs'");

      if (invoke.ReturnType != typeof(void))
        throw new ArgumentException("The delegate return type must be void.");
    }


    /// <overloads>
    /// <summary>
    /// Raises the event.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Raises the event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="EventArgs"/> instance containing the event data.
    /// </param>
    public void Invoke(object sender, EventArgs eventArgs)
    {
      base.Invoke(sender, eventArgs);
    }
  }
}
