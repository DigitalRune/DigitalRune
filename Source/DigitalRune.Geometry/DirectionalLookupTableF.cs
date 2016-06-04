// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;
#if !PORTABLE
using System.ComponentModel;
#endif
#if PORTABLE || WINDOWS
using System.Diagnostics;
using System.Dynamic;
#endif


namespace DigitalRune.Geometry
{
  /// <exclude/>
  [CLSCompliant(false)]
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class DirectionalLookupTableUInt16F : DirectionalLookupTableF<ushort>
  {
    internal DirectionalLookupTableUInt16F(ushort[,,] cubeMap) : base(cubeMap)
    {
    }


    /// <exclude/>
    public DirectionalLookupTableUInt16F(int width) : base(width)
    {
    }
  }


  /// <summary>
  /// Stores data that is accessed using a direction vector instead of indices. (Single-precision)
  /// </summary>
  /// <typeparam name="T">The type of data stored in the lookup table.</typeparam>
  /// <remarks>
  /// The directional lookup table internally uses a cube map to store the data.
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class DirectionalLookupTableF<T>
  {
    // Note: DirectionalLookupTableF<T> is only binary serializable, not xml-serializable.


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    // Face indices.
    private const int PositiveX = 0;
    private const int NegativeX = 1;
    private const int PositiveY = 2;
    private const int NegativeY = 3;
    private const int PositiveZ = 4;
    private const int NegativeZ = 5;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The cube map indexed as [face, y, x].
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
    internal T[,,] CubeMap;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the data associated with the specified direction.
    /// </summary>
    /// <value>The direction vector.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
    public T this[Vector3F direction]
    {
      get
      {
        int x, y, face;
        GetIndices(ref direction, out x, out y, out face);
        return CubeMap[face, y, x];
      }
      set
      {
        int x, y, face;
        GetIndices(ref direction, out x, out y, out face);
        CubeMap[face, y, x] = value;
      }
    }


    /// <summary>
    /// Gets the width of the cube map faces.
    /// </summary>
    /// <value>The width of the cube map faces.</value>
    private int Width
    {
      get { return CubeMap.GetLength(1); }
    }


#if PORTABLE || WINDOWS
    /// <exclude/>
#if !PORTABLE
    [Browsable(false)]
#endif
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public /*dynamic*/ object Internals
    {
      // Make internals visible to assemblies that cannot be added with InternalsVisibleTo().
      get
      {
        // ----- PCL Profile136 does not support dynamic.
        //dynamic internals = new ExpandoObject();
        //internals.Width = Width;
        //internals.CubeMap = CubeMap;
        //return internals;

        IDictionary<string, object> internals = new ExpandoObject();
        internals["Width"] = Width;
        internals["CubeMap"] = CubeMap;
        return internals;
      }
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectionalLookupTableF{T}"/> class. (For 
    /// internal use only.)
    /// </summary>
    /// <param name="cubeMap">The cube map.</param>
    internal DirectionalLookupTableF(T[, ,] cubeMap)
    {
      CubeMap = cubeMap;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DirectionalLookupTableF{T}"/> class.
    /// </summary>
    /// <param name="width">The width of the cube map faces.</param>
    public DirectionalLookupTableF(int width)
    {
      CubeMap = new T[6, width, width];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the cube map indices from a direction vector.
    /// </summary>
    /// <param name="direction">The direction vector.</param>
    /// <param name="x">The x index.</param>
    /// <param name="y">The y index.</param>
    /// <param name="face">The face index.</param>
    private void GetIndices(ref Vector3F direction, out int x, out int y, out int face)
    {
      // Precompute factor used for lookup.
      int width = CubeMap.GetLength(1);
      float s = 0.5f * width;

      // Convert the direction vector to indices.
      Vector3F abs = Vector3F.Absolute(direction);
      if (abs.X >= abs.Y && abs.X >= abs.Z)
      {
        float oneOverX = 1.0f / abs.X;
        if (direction.X >= 0)
        {
          x = (int)((direction.Z * oneOverX + 1.0f) * s);
          y = (int)((direction.Y * oneOverX + 1.0f) * s);
          face = PositiveX;
        }
        else
        {
          x = (int)((-direction.Z * oneOverX + 1.0f) * s);
          y = (int)((direction.Y * oneOverX + 1.0f) * s);
          face = NegativeX;
        }
      }
      else if (abs.Y >= abs.Z)
      {
        float oneOverY = 1.0f / abs.Y;
        if (direction.Y >= 0)
        {
          x = (int)((direction.X * oneOverY + 1.0f) * s);
          y = (int)((direction.Z * oneOverY + 1.0f) * s);
          face = PositiveY;
        }
        else
        {
          x = (int)((-direction.X * oneOverY + 1.0f) * s);
          y = (int)((direction.Z * oneOverY + 1.0f) * s);
          face = NegativeY;
        }
      }
      else
      {
        float oneOverZ = 1.0f / abs.Z;
        if (direction.Z >= 0)
        {
          x = (int)((-direction.X * oneOverZ + 1.0f) * s);
          y = (int)((direction.Y * oneOverZ + 1.0f) * s);
          face = PositiveZ;
        }
        else
        {
          x = (int)((direction.X * oneOverZ + 1.0f) * s);
          y = (int)((direction.Y * oneOverZ + 1.0f) * s);
          face = NegativeZ;
        }
      }

      // Clamp indices. (Necessary if direction points exactly to edge.)
      int maxIndex = width - 1;
      if (x > maxIndex)
        x = maxIndex;

      if (y > maxIndex)
        y = maxIndex;
    }


    /// <summary>
    /// Gets the sample directions.
    /// </summary>
    /// <returns>The sample directions.</returns>
    /// <remarks>
    /// A directional lookup table stores a limited amount of samples. The method 
    /// <see cref="GetSampleDirections"/> returns all direction vectors that point to the exact
    /// centers of the samples. The method can be used to iterate over all samples and fill the
    /// lookup table with data or read all entries.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public IEnumerable<Vector3F> GetSampleDirections()
    {
      int width = CubeMap.GetLength(1);

      // Positive X
      for (int i = 0; i < width; i++)
      {
        float y = (i + 0.5f) / width * 2.0f - 1.0f;
        for (int j = 0; j < width; j++)
        {
          float x = (j + 0.5f) / width * 2.0f - 1.0f;
          yield return new Vector3F(1.0f, x, y);
        }
      }

      // Negative X
      for (int i = 0; i < width; i++)
      {
        float y = (i + 0.5f) / width * 2.0f - 1.0f;
        for (int j = 0; j < width; j++)
        {
          float x = (j + 0.5f) / width * 2.0f - 1.0f;
          yield return new Vector3F(-1.0f, x, y);
        }
      }

      // Positive Y
      for (int i = 0; i < width; i++)
      {
        float y = (i + 0.5f) / width * 2.0f - 1.0f;
        for (int j = 0; j < width; j++)
        {
          float x = (j + 0.5f) / width * 2.0f - 1.0f;
          yield return new Vector3F(x, 1.0f, y);
        }
      }

      // Negative Y
      for (int i = 0; i < width; i++)
      {
        float y = (i + 0.5f) / width * 2.0f - 1.0f;
        for (int j = 0; j < width; j++)
        {
          float x = (j + 0.5f) / width * 2.0f - 1.0f;
          yield return new Vector3F(x, -1.0f, y);
        }
      }

      // Positive Z
      for (int i = 0; i < width; i++)
      {
        float y = (i + 0.5f) / width * 2.0f - 1.0f;
        for (int j = 0; j < width; j++)
        {
          float x = (j + 0.5f) / width * 2.0f - 1.0f;
          yield return new Vector3F(x, y, 1.0f);
        }
      }

      // Negative Z
      for (int i = 0; i < width; i++)
      {
        float y = (i + 0.5f) / width * 2.0f - 1.0f;
        for (int j = 0; j < width; j++)
        {
          float x = (j + 0.5f) / width * 2.0f - 1.0f;
          yield return new Vector3F(x, y, -1.0f);
        }
      }
    }
    #endregion
  }
}
