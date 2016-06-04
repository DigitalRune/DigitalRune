#region ----- Copyright -----
/*
  Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
  
  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:
  
  The above copyright notice and this permission notice shall be included in
  all copies or substantial portions of the Software.
  
  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
  THE SOFTWARE.
*/
#endregion

using System;
using System.Globalization;
using System.Runtime.InteropServices;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Represents a FourCC descriptor.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Size = 4)]
  internal struct FourCC : IEquatable<FourCC>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly uint _value;

    /// <summary>Empty FourCC.</summary>
    public static readonly FourCC Empty = new FourCC(0);
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FourCC" /> struct.
    /// </summary>
    /// <param name="fourCC">The fourCC value as a string .</param>
    public FourCC(string fourCC)
    {
      if (fourCC.Length != 4)
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Invalid length for FourCC(\"{0}\". Must be be 4 characters long ", fourCC);
        throw new ArgumentException(message, "fourCC");
      }

      _value = ((uint)fourCC[3]) << 24 | ((uint)fourCC[2]) << 16 | ((uint)fourCC[1]) << 8 | ((uint)fourCC[0]);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="FourCC" /> struct.
    /// </summary>
    /// <param name="byte1">The byte1.</param>
    /// <param name="byte2">The byte2.</param>
    /// <param name="byte3">The byte3.</param>
    /// <param name="byte4">The byte4.</param>
    public FourCC(char byte1, char byte2, char byte3, char byte4)
    {
      _value = ((uint)byte4) << 24 | ((uint)byte3) << 16 | ((uint)byte2) << 8 | ((uint)byte1);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="FourCC" /> struct.
    /// </summary>
    /// <param name="fourCC">The fourCC value as an uint.</param>
    [CLSCompliant(false)]
    public FourCC(uint fourCC)
    {
      _value = fourCC;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="FourCC" /> struct.
    /// </summary>
    /// <param name="fourCC">The fourCC value as an int.</param>
    public FourCC(int fourCC)
    {
      _value = unchecked((uint)fourCC);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Performs an implicit conversion from <see cref="FourCC"/> to <see cref="Int32"/>.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator uint(FourCC descriptor)
    {
      return descriptor._value;
    }


    /// <summary>
    /// Performs an implicit conversion from <see cref="FourCC"/> to <see cref="Int32"/>.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator int(FourCC descriptor)
    {
      return unchecked((int)descriptor._value);
    }


    /// <summary>
    /// Performs an implicit conversion from <see cref="Int32"/> to <see cref="FourCC"/>.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>The result of the conversion.</returns>
    [CLSCompliant(false)]
    public static implicit operator FourCC(uint descriptor)
    {
      return new FourCC(descriptor);
    }


    /// <summary>
    /// Performs an implicit conversion from <see cref="Int32"/> to <see cref="FourCC"/>.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator FourCC(int descriptor)
    {
      return new FourCC(descriptor);
    }


    /// <summary>
    /// Performs an implicit conversion from <see cref="FourCC"/> to <see cref="String"/>.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator string(FourCC descriptor)
    {
      return descriptor.ToString();
    }


    /// <summary>
    /// Performs an implicit conversion from <see cref="String"/> to <see cref="FourCC"/>.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator FourCC(string descriptor)
    {
      return new FourCC(descriptor);
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other" />
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(FourCC other)
    {
      return _value == other._value;
    }


    /// <summary>
    /// Determines whether the specified <see cref="Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object" /> is equal to this instance;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is FourCC && Equals((FourCC)obj);
    }


    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures
    /// like a hash table.
    /// </returns>
    public override int GetHashCode()
    {
      return (int)_value;
    }


    /// <summary>
    /// Compares two <see cref="FourCC"/> descriptors to determine whether they are the
    /// same.
    /// </summary>
    /// <param name="left">The first descriptor.</param>
    /// <param name="right">The second descriptor.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the
    /// same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(FourCC left, FourCC right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares two <see cref="FourCC"/> descriptors to determine whether they are different.
    /// </summary>
    /// <param name="left">The first descriptor.</param>
    /// <param name="right">The second descriptor.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the
    /// different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(FourCC left, FourCC right)
    {
      return !left.Equals(right);
    }


    /// <summary>
    /// Returns a <see cref="String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="String" /> that represents this instance.</returns>
    public override string ToString()
    {
      return string.Format("{0}", new string(new[]
                                  {
                                    (char) (_value & 0xFF),
                                    (char) ((_value >> 8) & 0xFF),
                                    (char) ((_value >> 16) & 0xFF),
                                    (char) ((_value >> 24) & 0xFF)
                                  }));
    }
    #endregion
  }
}
