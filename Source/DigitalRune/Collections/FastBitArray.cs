// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Collections
{
  /// <summary>
  /// A fast implementation of a bit array. Minimal overhead.
  /// </summary>
  internal sealed class FastBitArray
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly int[] _array;
    private readonly int _length;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    public int Length
    {
      get { return _length; }
    }


    public bool this[int index]
    {
      get
      {
        return (_array[index / 32] & (1 << (index % 32))) != 0;
      }
      set
      {
        if (value)
          _array[index / 32] |= (1 << (index % 32));
        else
          _array[index / 32] &= ~(1 << (index % 32));
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FastBitArray"/> class.
    /// </summary>
    /// <param name="length">The number of bits.</param>
    /// <remarks>
    /// All bits are <see langword="false"/> per default.
    /// </remarks>
    public FastBitArray(int length)
    {
      int arrayLength = (length - 1) / 32 + 1;
      _array = new int[arrayLength];
      _length = length;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    public void SetAll(bool value)
    {
      int v = value ? unchecked((int)0xffffffff) : 0;
      for (int i = 0; i < _array.Length; i++)
        _array[i] = v;
    }
    #endregion
  }
}
