#region ----- Copyright -----
/*
  The class in this file is based on the LoopWork from the ParallelTasks library (see 
  http://paralleltasks.codeplex.com/) which is licensed under Ms-PL (see below).


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
*/
#endregion

#if !NETFX_CORE && !PORTABLE && !USE_TPL
using System;
using System.Collections.Generic;
using System.Threading;


namespace DigitalRune.Threading
{
  internal sealed class ForLoopWork : IWork, IRecyclable
  {
    private static readonly ResourcePool<ForLoopWork> Pool = new ResourcePool<ForLoopWork>(
      () => new ForLoopWork(),  // Create
      null,                     // Initialize
      null);                    // Uninitialize


    private int _length;
    private int _stride;
    private volatile int _index;

    
    public Action<int> Action { get; private set; }
    public WorkOptions Options { get; private set; }


    private ForLoopWork()
    {
      Options = new WorkOptions { MaximumThreads = int.MaxValue };
    }


    public static ForLoopWork Create()
    {
      return Pool.Obtain();
    }


    public void Recycle()
    {
      Action = null;
      Pool.Recycle(this);
    }


    public void Prepare(Action<int> action, int startInclusive, int endExclusive, int stride)
    {
      Action = action;
      _index = startInclusive;
      _length = endExclusive;
      _stride = stride;
    }


    public void DoWork()
    {
      int start = IncrementIndex();
      while (start < _length)
      {
        int end = Math.Min(start + _stride, _length);
        for (int i = start; i < end; i++)
          Action(i);

        start = IncrementIndex();
      }
    }


    private int IncrementIndex()
    {
#pragma warning disable 0420
#if !SILVERLIGHT && !WP7 && !XBOX
        return Interlocked.Add(ref _index, _stride) - _stride;
#else
      // Important: Interlocked.Add() does exist in .NET Compact Framework. But it does not work
      // on the Xbox 360!
      int x;
      do
      {
        x = _index;
      } while (Interlocked.CompareExchange(ref _index, x + _stride, x) != x);
      return x;
#endif
    }
  }


  internal sealed class ForEachLoopWork<T> : IWork, IRecyclable
  {
    // ReSharper disable StaticFieldInGenericType
    private static readonly ResourcePool<ForEachLoopWork<T>> Pool = new ResourcePool<ForEachLoopWork<T>>(
      () => new ForEachLoopWork<T>(),   // Create
      null,                             // Initialize
      null);                            // Uninitialize
    // ReSharper restore StaticFieldInGenericType

    
    private IEnumerator<T> _enumerator;
    private volatile bool _notDone;
    private readonly object _syncLock = new object();


    public Action<T> Action { get; private set; }
    public WorkOptions Options { get; private set; }


    private ForEachLoopWork()
    {
      Options = new WorkOptions { MaximumThreads = int.MaxValue };
    }


    public static ForEachLoopWork<T> Create()
    {
      return Pool.Obtain();
    }


    public void Recycle()
    {
      _enumerator = null;
      Action = null;
      Pool.Recycle(this);
    }


    public void Prepare(Action<T> action, IEnumerator<T> enumerator)
    {
      Action = action;
      _enumerator = enumerator;
      _notDone = true;
    }


    public void DoWork()
    {
      T item = default(T);
      while (_notDone)
      {
        bool hasValue = false;
        lock (_syncLock)
        {
          _notDone = _enumerator.MoveNext();
          if (_notDone)
          {
            item = _enumerator.Current;
            hasValue = true;
          }
        }

        if (hasValue)
          Action(item);
      }
    }
  }
}
#endif
