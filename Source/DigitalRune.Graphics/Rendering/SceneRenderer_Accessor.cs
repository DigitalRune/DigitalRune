// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Graphics.SceneGraph;


namespace DigitalRune.Graphics.Rendering
{
  partial class SceneRenderer
  {
    /// <summary>
    /// Exposes a section of the jobs list as read-only <c>IList&lt;SceneNode&gt;</c>.
    /// </summary>
    internal class Accessor : IList<SceneNode>
    {
      //--------------------------------------------------------------
      #region Nested Types
      //--------------------------------------------------------------

      private class Enumerator : IEnumerator<SceneNode>
      {
        private readonly Job[] _drawList;
        private readonly int _startIndex;
        private readonly int _endExclusive;
        private int _index;
        private SceneNode _current;


        object IEnumerator.Current
        {
          get { return _current; }
        }


        public SceneNode Current
        {
          get { return _current; }
        }


        internal Enumerator(Job[] drawList, int startInclusive, int endExclusive)
        {
          _drawList = drawList;
          _startIndex = startInclusive;
          _endExclusive = endExclusive;
          _index = _startIndex;
          _current = null;
        }


        public void Dispose()
        {
        }


        public bool MoveNext()
        {
          if (_index >= _endExclusive)
          {
            _current = null;
            return false;
          }

          _current = _drawList[_index].Node;
          _index++;
          return true;
        }


        void IEnumerator.Reset()
        {
          _index = _startIndex;
          _current = null;
        }
      }
      #endregion


      //--------------------------------------------------------------
      #region Fields
      //--------------------------------------------------------------

      private const string MessageListIsReadOnly = "The list is read-only.";
      private Job[] _jobs;
      private int _startInclusive;
      private int _endExclusive;
      #endregion


      //--------------------------------------------------------------
      #region Properties & Events
      //--------------------------------------------------------------

      public int Count
      {
        get { return _endExclusive - _startInclusive; }
      }


      bool ICollection<SceneNode>.IsReadOnly
      {
        get { return true; }
      }


      public SceneNode this[int index]
      {
        get { return _jobs[_startInclusive + index].Node; }
        set { throw new NotSupportedException(MessageListIsReadOnly); }
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
      /// Assigns a section of the jobs list to the accessor.
      /// </summary>
      /// <param name="jobs">The jobs.</param>
      /// <param name="startInclusive">The start index (inclusive).</param>
      /// <param name="endExclusive">The end index (exclusive).</param>
      public void Set(ArrayList<Job> jobs, int startInclusive, int endExclusive)
      {
        Debug.Assert(jobs != null, "The jobs list must not be null.");
        Debug.Assert(startInclusive >= 0 && startInclusive < jobs.Count, "Invalid start index.");
        Debug.Assert(endExclusive >= 0 && endExclusive <= jobs.Count, "Invalid end index.");
        Debug.Assert(startInclusive <= endExclusive, "Invalid start and end index.");

        _jobs = jobs.Array;
        _startInclusive = startInclusive;
        _endExclusive = endExclusive;
      }


      /// <summary>
      /// Resets the accessor.
      /// </summary>
      public void Reset()
      {
        _jobs = null;
        _startInclusive = 0;
        _endExclusive = 0;
      }


      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }


      public IEnumerator<SceneNode> GetEnumerator()
      {
        return new Enumerator(_jobs, _startInclusive, _endExclusive);
      }


      void ICollection<SceneNode>.Add(SceneNode node)
      {
        throw new NotSupportedException(MessageListIsReadOnly);
      }


      void ICollection<SceneNode>.Clear()
      {
        throw new NotSupportedException(MessageListIsReadOnly);
      }


      public bool Contains(SceneNode node)
      {
        for (int i = _startInclusive; i < _endExclusive; i++)
          if (_jobs[i].Node == node)
            return true;

        return false;
      }


      public void CopyTo(SceneNode[] array, int arrayIndex)
      {
        if (array == null)
          throw new ArgumentNullException("array");
        if (arrayIndex < 0)
          throw new ArgumentOutOfRangeException("arrayIndex", "Array index must equal to or greater than 0.");
        if (array.Length > 0 && array.Length <= arrayIndex)
          throw new ArgumentOutOfRangeException("arrayIndex", "Array index must less than the length of the array.");
        if (array.Length - arrayIndex < _endExclusive - _startInclusive)
          throw new ArgumentException(
            "The number of elements in the list is greater than the available space from arrayIndex to the end of the array.");

        for (int i = _startInclusive; i < _endExclusive; i++)
          array[arrayIndex++] = _jobs[i].Node;
      }


      bool ICollection<SceneNode>.Remove(SceneNode node)
      {
        throw new NotSupportedException(MessageListIsReadOnly);
      }


      public int IndexOf(SceneNode node)
      {
        for (int i = _startInclusive; i < _endExclusive; i++)
          if (_jobs[i].Node == node)
            return i - _startInclusive;

        return -1;
      }


      void IList<SceneNode>.Insert(int index, SceneNode node)
      {
        throw new NotSupportedException(MessageListIsReadOnly);
      }


      void IList<SceneNode>.RemoveAt(int index)
      {
        throw new NotSupportedException(MessageListIsReadOnly);
      }
      #endregion
    }
  }
}
