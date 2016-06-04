// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Enumerates a list and converts from type <see cref="Point"/> to <see cref="DataPoint"/>.
    /// </summary>
    internal class PointListEnumerator : IEnumerator<DataPoint>
    {
        private bool _disposed;
        private IList<Point> _collection;
        private int _position = -1;


        public PointListEnumerator(IList<Point> collection)
        {
            _collection = collection;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MoveNext")]
        public bool MoveNext()
        {
            if (_disposed)
                throw new InvalidOperationException("Cannot execute MoveNext. Enumerator has already been disposed.");

            _position++;
            return (_position < _collection.Count);
        }


        public void Reset()
        {
            if (_disposed)
                throw new InvalidOperationException("Cannot execute Reset. Enumerator has already been disposed.");

            _position = -1;
        }


        public DataPoint Current
        {
            get
            {
                if (_disposed)
                    throw new InvalidOperationException("Cannot access Current. Enumerator has already been disposed.");

                try
                {
                    return new DataPoint(_collection[_position], null);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }


        object IEnumerator.Current
        {
            get { return Current; }
        }


        public void Dispose()
        {
            if (_disposed)
                throw new InvalidOperationException("Cannot execute Dispose. Enumerator has already been disposed.");

            _collection = null;
            _disposed = true;
        }
    }
}
