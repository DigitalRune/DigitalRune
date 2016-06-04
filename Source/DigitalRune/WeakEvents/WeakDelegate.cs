#region ----- Copyright -----
/*
  The class in this file is based on the WeakAction from Josh Smith's mediator prototype 
  (http://joshsmithonwpf.wordpress.com/2009/04/06/a-mediator-prototype-for-wpf-apps/), the 
  DelegateReference the MSDN article "Composite Application Guidance for WPF" 
  (http://msdn.microsoft.com/en-us/library/dd458809.aspx) which is licensed under Ms-PL (see below)
  and the SmartWeakEvent/FastWeakEvent from Daniel Grundwald's article "Weak Events in C#" 
  (http://www.codeproject.com/KB/cs/WeakEvents.aspx) which is licensed under the MIT License (see 
  below).


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
using System.Reflection;


namespace DigitalRune
{
  /// <summary>
  /// Represents a <see cref="Delegate"/> that stores the target object as a weak reference.
  /// </summary>
  /// <remarks>
  /// <strong>Important:</strong> In Silverlight, the target of a <see cref="WeakDelegate"/> needs 
  /// to be a public method (not a private, protected or anonymous method). This is necessary 
  /// because of security restrictions in Silverlight.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
  public class WeakDelegate
  {
    // Note: The implementation in PRISM (see DelegateReference) has an option to keep the target 
    // alive.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private InternalWeakDelegate _internalWeakDelegate;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the weak reference of the target object.
    /// </summary>
    /// <value>
    /// The weak reference of the target object, or <see langword="null"/> if the delegate method is
    /// a static method.
    /// </value>
    public WeakReference TargetReference
    {
      get { return _internalWeakDelegate.TargetReference; }
    }


    /// <summary>
    /// Gets the metadata of the delegate method.
    /// </summary>
    /// <value>The metadata of the delegate method.</value>
    public MethodInfo MethodInfo
    {
      get { return _internalWeakDelegate.MethodInfo; }
    }


    /// <summary>
    /// Gets the type of delegate.
    /// </summary>
    /// <value>The type of delegate.</value>
    public Type DelegateType
    {
      get { return _internalWeakDelegate.DelegateType; }
    }


    /// <summary>
    /// Gets the <see cref="System.Delegate"/> stored by the current <see cref="WeakDelegate"/> 
    /// object.
    /// </summary>
    /// <value>
    /// <see langword="null"/> if the object referenced by the current <see cref="WeakDelegate"/> 
    /// object has been garbage collected; otherwise, a reference to the <see cref="System.Delegate"/>.
    /// </value>
    public Delegate Delegate
    {
      get { return _internalWeakDelegate.Delegate; }
    }


    /// <summary>
    /// Gets a value indicating whether the <see cref="System.Delegate"/> referenced by this 
    /// <see cref="WeakDelegate"/> has been garbage collected.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the <see cref="System.Delegate"/> referenced by the current 
    /// <see cref="WeakDelegate"/> has not been garbage collected and is still accessible; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> Because an object could potentially be reclaimed for garbage 
    /// collection immediately after the <see cref="IsAlive"/> property returns 
    /// <see langword="true"/>, using this property is not recommended unless you are testing only 
    /// for a <see langword="false"/> return value. 
    /// </para>
    /// <para>
    /// If the referenced <see cref="System.Delegate"/> is a static method, <see cref="IsAlive"/> 
    /// will always return <see langword="true"/>.
    /// </para>
    /// </remarks>
    public bool IsAlive
    {
      get { return _internalWeakDelegate.IsAlive; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="WeakDelegate"/> class.
    /// </summary>
    /// <param name="delegate">
    /// The original <see cref="System.Delegate"/> to create a weak reference for.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="delegate"/> is <see langword="null"/>.
    /// </exception>
    public WeakDelegate(Delegate @delegate)
    {
      _internalWeakDelegate = new InternalWeakDelegate(@delegate);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Invokes the stored <see cref="System.Delegate"/> with the given arguments.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public void Invoke(params object[] args)
    {
      _internalWeakDelegate.Invoke(args);
    }
    #endregion
  }
}
