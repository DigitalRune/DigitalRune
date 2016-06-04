#region ----- Copyright -----
/*
  The class in this file is based on the WeakDelegatesManager from the MSDN article "Composite 
  Application Guidance for WPF" (http://msdn.microsoft.com/en-us/library/dd458809.aspx) which is 
  licensed under Ms-PL (see below) and the SmartWeakEvent/FastWeakEvent from Daniel Grundwald's 
  article "Weak Events in C#" (http://www.codeproject.com/KB/cs/WeakEvents.aspx) which is licensed 
  under the MIT License (see below).


  Microsoft Public License (Ms-PL)

  This license governs use of the accompanying software. If you use the software, you accept this 
  license. If you do not accept the license, do not use the software.

  1. Definitions

  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same 
  meaning here as under U.S. copyright law.

  A "contribution" is the original software, or any additions or changes to the software.

  A "contributor" is any person that distributes its contribution under this license.

  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

  2. Grant of Rights

  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and 
  limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
  copyright license to reproduce its contribution, prepare derivative works of its contribution, and 
  distribute its contribution or any derivative works that you create.

  (B) Patent Grant- Subject to the terms of this license, including the license conditions and 
  limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
  license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or 
  otherwise dispose of its contribution in the software or derivative works of the contribution in 
  the software.

  3. Conditions and Limitations

  (A) No Trademark License- This license does not grant you rights to use any contributors' name, 
  logo, or trademarks.

  (B) If you bring a patent claim against any contributor over patents that you claim are infringed 
  by the software, your patent license from such contributor to the software ends automatically.

  (C) If you distribute any portion of the software, you must retain all copyright, patent, 
  trademark, and attribution notices that are present in the software.

  (D) If you distribute any portion of the software in source code form, you may do so only under 
  this license by including a complete copy of this license with your distribution. If you 
  distribute any portion of the software in compiled or object code form, you may only do so under a 
  license that complies with this license.

  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no 
  express warranties, guarantees or conditions. You may have additional consumer rights under your 
  local laws which this license cannot change. To the extent permitted under your local laws, the 
  contributors exclude the implied warranties of merchantability, fitness for a particular purpose 
  and non-infringement. 

 
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
using System.Collections.Generic;
#if NETFX_CORE || NET45
using System.Reflection;
#endif


namespace DigitalRune
{
  /// <summary>
  /// Represents a <see cref="MulticastDelegate"/> that stores the target objects as weak 
  /// references.
  /// </summary>
  /// <remarks>
  /// <strong>Important:</strong> In Silverlight, the targets of a 
  /// <see cref="WeakMulticastDelegate"/> need to be public methods (no private, protected or
  /// anonymous methods). This is necessary because of security restrictions in Silverlight.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
  public class WeakMulticastDelegate
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly List<InternalWeakDelegate> _delegates = new List<InternalWeakDelegate>();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the number of live delegates in the collection.
    /// </summary>
    /// <value>The number of live delegates in the collection.</value>
    public int Count
    {
      get
      {
        Purge();
        return _delegates.Count;
      }
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
    /// Adds a new <see cref="Delegate"/> to the <see cref="WeakMulticastDelegate"/>.
    /// </summary>
    /// <param name="delegate">The new <see cref="Delegate"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="delegate"/> is <see langword="null"/>.
    /// </exception>
    public void Add(Delegate @delegate)
    {
      if (@delegate == null)
        throw new ArgumentNullException("delegate");

      if (_delegates.Count == _delegates.Capacity)
        Purge();

      _delegates.Add(new InternalWeakDelegate(@delegate));
    }


    /// <summary>
    /// Removes a <see cref="Delegate"/> from the <see cref="WeakMulticastDelegate"/>.
    /// </summary>
    /// <param name="delegate">The <see cref="Delegate"/> to remove.</param>
    public void Remove(Delegate @delegate)
    {
      if (@delegate == null)
        return;

      for (int i = _delegates.Count - 1; i >= 0; i--)
      {
        InternalWeakDelegate weakDelegate = _delegates[i];
        if (weakDelegate.TargetReference != null)
        {
          // The delegate method is an object method.
          object target = weakDelegate.TargetReference.Target;
          if (target == null)
          {
            // Remove garbage collected entry.
            _delegates.RemoveAt(i);
          }
          else if (target == @delegate.Target 
#if !NETFX_CORE && !NET45
                  && weakDelegate.MethodInfo.Equals(@delegate.Method))
#else
                  && weakDelegate.MethodInfo.Equals(@delegate.GetMethodInfo()))
#endif

          {
            // Remove matching entry.
            _delegates.RemoveAt(i);
            break;
          }
        }
        else
        {
          // The delegate method is a class method.
          if (@delegate.Target == null
#if !NETFX_CORE && !NET45
              && weakDelegate.MethodInfo.Equals(@delegate.Method))
#else
              && weakDelegate.MethodInfo.Equals(@delegate.GetMethodInfo()))
#endif
          {
            // Remove matching entry.
            _delegates.RemoveAt(i);
            break;
          }
        }
      }
    }


    /// <summary>
    /// Invokes the stored <see cref="Delegate"/>s with the given arguments.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public void Invoke(params object[] args)
    {
      Purge();
      foreach (InternalWeakDelegate weakDelegate in _delegates.ToArray())
        weakDelegate.Invoke(args);
    }


    /// <summary>
    /// Purges the garbage-collected delegates.
    /// </summary>
    private void Purge()
    {
      // .NET CF does not support:
      //   _delegates.RemoveAll(d => !d.IsAlive);
      //
      // Therefore we have to write our own loop:
      for (int i = _delegates.Count - 1; i >= 0; i--)
      {
        var @delegate = _delegates[i];
        if (!@delegate.IsAlive)
          _delegates.RemoveAt(i);
      }
    }
    #endregion
  }
}
