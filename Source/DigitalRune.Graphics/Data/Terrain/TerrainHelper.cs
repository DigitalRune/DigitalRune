// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides helper methods for terrain rendering.
  /// </summary>
  public static class TerrainHelper
  {
    // TODO: In the future use perhaps we can use our content texture classes instead of the
    // texture format helper methods of this class.
    // TODO: Implement GetTextureLevel for levels > 0.
    // TODO: Discuss order of method arguments.

    //--------------------------------------------------------------
    #region Get/Set texture data
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the texture data of the specified mipmap level as a <see cref="Vector4"/> array.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="level">The mipmap level to read. Currently only 0 is supported!</param>
    /// <returns>
    /// The array containing the data of the specified mipmap level.
    /// (One <see cref="Vector4"/> element per pixel.)
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method can be used with following texture surface formats:
    /// <see cref="SurfaceFormat.Alpha8"/>, <see cref="SurfaceFormat.Color"/>, 
    /// <see cref="SurfaceFormat.Rg32"/>, <see cref="SurfaceFormat.Rgba64"/>,
    /// <see cref="SurfaceFormat.Single"/>, <see cref="SurfaceFormat.Vector2"/>,
    /// <see cref="SurfaceFormat.Vector4"/>, <see cref="SurfaceFormat.HalfSingle"/>,
    /// <see cref="SurfaceFormat.HalfVector2"/>, <see cref="SurfaceFormat.HalfVector4"/>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotImplementedException">
    /// Invalid mipmap level. Extracting mipmap levels other than 0 is not yet implemented.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Texture format is not yet supported.
    /// </exception>
    public static Vector4[] GetTextureLevelVector4(Texture2D texture, int level)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");
      if (level < 0)
        throw new ArgumentOutOfRangeException("level");
      if (level > 0)
        throw new NotImplementedException("GetTextureLevelVector4 for levels other than 0 is not yet implemented.");

      var bufferLevel0 = new Vector4[texture.Width * texture.Height];

      if (texture.Format == SurfaceFormat.Alpha8)
      {
        var buffer = new byte[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = new Vector4(0, 0, 0, buffer[i] / 255.0f);
      }
      else if (texture.Format == SurfaceFormat.Color)
      {
        var buffer = new Color[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = buffer[i].ToVector4();
      }
      else if (texture.Format == SurfaceFormat.Rg32)
      {
        var buffer = new Rg32[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
        {
          var v = buffer[i].ToVector2();
          bufferLevel0[i] = new Vector4(v.X, v.Y, 0, 0);
        }
      }
      else if (texture.Format == SurfaceFormat.Rgba64)
      {
        var buffer = new Rgba64[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = buffer[i].ToVector4();
      }
      else if (texture.Format == SurfaceFormat.Single)
      {
        var buffer = new Single[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = new Vector4(buffer[i]);
      }
      else if (texture.Format == SurfaceFormat.Vector2)
      {
        var buffer = new Vector2[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = new Vector4(buffer[i].X, buffer[i].Y, 0, 0);
      }
      else if (texture.Format == SurfaceFormat.Vector4)
      {
        texture.GetData(bufferLevel0);
      }
      else if (texture.Format == SurfaceFormat.HalfSingle)
      {
        var buffer = new HalfSingle[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = new Vector4(buffer[i].ToSingle(), 0, 0, 0);
      }
      else if (texture.Format == SurfaceFormat.HalfVector2)
      {
        var buffer = new HalfVector2[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
        {
          var v = buffer[i].ToVector2();
          bufferLevel0[i] = new Vector4(v.X, v.Y, 0, 0);
        }
      }
      else if (texture.Format == SurfaceFormat.HalfVector4)
      {
        var buffer = new HalfVector4[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = buffer[i].ToVector4();
      }
      else
      {
        throw new NotSupportedException("Texture format '" + texture.Format + "' is not yet supported.");
      }

      return bufferLevel0;
    }


    /// <summary>
    /// Gets the texture data of the specified mipmap level as a <see cref="float"/> array.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="level">The mipmap level to read. Currently only 0 is supported!</param>
    /// <returns>
    /// The array containing the data of the specified mipmap level. 
    /// (One <see cref="float"/> element per pixel. If the texture contains multiple channels,
    /// only the first channel (red) is copied.)
    /// </returns>
    /// <inheritdoc cref="GetTextureLevelVector4"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetTextureLevelSingle")]
    public static float[] GetTextureLevelSingle(Texture2D texture, int level)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");
      if (level < 0)
        throw new ArgumentOutOfRangeException("level");
      if (level > 0)
        throw new NotImplementedException("GetTextureLevelSingle for levels other than 0 is not yet implemented.");

      var bufferLevel0 = new float[texture.Width * texture.Height];

      if (texture.Format == SurfaceFormat.Alpha8)
      {
        var buffer = new byte[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = buffer[i] / 255.0f;
      }
      else if (texture.Format == SurfaceFormat.Color)
      {
        var buffer = new Color[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = buffer[i].R / 255.0f;
      }
      else if (texture.Format == SurfaceFormat.Rg32)
      {
        var buffer = new Rg32[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
        {
          var v = buffer[i].ToVector2();
          bufferLevel0[i] = v.X;
        }
      }
      else if (texture.Format == SurfaceFormat.Rgba64)
      {
        var buffer = new Rgba64[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = buffer[i].ToVector4().X;
      }
      else if (texture.Format == SurfaceFormat.Single)
      {
        texture.GetData(bufferLevel0);
      }
      else if (texture.Format == SurfaceFormat.Vector2)
      {
        var buffer = new Vector2[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = buffer[i].X;
      }
      else if (texture.Format == SurfaceFormat.Vector4)
      {
        var buffer = new Vector4[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = buffer[i].X;
      }
      else if (texture.Format == SurfaceFormat.HalfSingle)
      {
        var buffer = new HalfSingle[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = buffer[i].ToSingle();
      }
      else if (texture.Format == SurfaceFormat.HalfVector2)
      {
        var buffer = new HalfVector2[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
        {
          var v = buffer[i].ToVector2();
          bufferLevel0[i] = v.X;
        }
      }
      else if (texture.Format == SurfaceFormat.HalfVector4)
      {
        var buffer = new HalfVector4[bufferLevel0.Length];
        texture.GetData(buffer);
        for (int i = 0; i < buffer.Length; i++)
          bufferLevel0[i] = buffer[i].ToVector4().X;
      }
      else
      {
        throw new NotSupportedException("Texture format '" + texture.Format + "' is not yet supported.");
      }

      return bufferLevel0;
    }


    /// <overloads>
    /// <summary>
    /// Sets the texture data of the specified mipmap level.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets the texture data of the specified mipmap level.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="level">The mipmap level.</param>
    /// <param name="data">The data to be copied into the texture.</param>
    /// <remarks>
    /// <para>
    /// This method can be used with following texture formats:
    /// <see cref="SurfaceFormat.Alpha8"/>, <see cref="SurfaceFormat.Color"/>, 
    /// <see cref="SurfaceFormat.Rg32"/>, <see cref="SurfaceFormat.Rgba64"/>,
    /// <see cref="SurfaceFormat.Single"/>, <see cref="SurfaceFormat.Vector2"/>,
    /// <see cref="SurfaceFormat.Vector4"/>, <see cref="SurfaceFormat.HalfSingle"/>,
    /// <see cref="SurfaceFormat.HalfVector2"/>, <see cref="SurfaceFormat.HalfVector4"/>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> or <paramref name="data"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Texture format is not yet supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    public static void SetTextureLevel(Texture2D texture, int level, Vector4[] data)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");
      if (data == null)
        throw new ArgumentNullException("data");

      if (texture.Format == SurfaceFormat.Alpha8)
      {
        var buffer = new byte[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = (byte)(data[i].W * 255.0f);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.Color)
      {
        var buffer = new Color[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new Color(data[i]);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.Rg32)
      {
        var buffer = new Rg32[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new Rg32(data[i].X, data[i].Y);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.Rgba64)
      {
        var buffer = new Rgba64[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new Rgba64(data[i]);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.Single)
      {
        var b = new Single[data.Length];
        for (int i = 0; i < b.Length; i++)
          b[i] = data[i].X;
        texture.SetData(level, null, b, 0, b.Length);
      }
      else if (texture.Format == SurfaceFormat.Vector2)
      {
        var buffer = new Vector2[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new Vector2(data[i].X, data[i].Y);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.Vector4)
      {
        texture.SetData(level, null, data, 0, data.Length);
      }
      else if (texture.Format == SurfaceFormat.HalfSingle)
      {
        var buffer = new HalfSingle[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new HalfSingle(data[i].X);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.HalfVector2)
      {
        var buffer = new HalfVector2[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new HalfVector2(data[i].X, data[i].Y);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.HalfVector4)
      {
        var buffer = new HalfVector4[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new HalfVector4(data[i]);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else
      {
        throw new NotSupportedException("Texture format '" + texture.Format + "' is not yet supported.");
      }
    }


    /// <summary>
    /// Sets the texture data of the specified mipmap level.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="level">The mipmap level.</param>
    /// <param name="data">The data to be copied into the texture.</param>
    /// <remarks>
    /// <para>
    /// This method can be used with following texture formats:
    /// <see cref="SurfaceFormat.Alpha8"/>, <see cref="SurfaceFormat.Color"/>, 
    /// <see cref="SurfaceFormat.Rg32"/>, <see cref="SurfaceFormat.Rgba64"/>,
    /// <see cref="SurfaceFormat.Single"/>, <see cref="SurfaceFormat.Vector2"/>,
    /// <see cref="SurfaceFormat.Vector4"/>, <see cref="SurfaceFormat.HalfSingle"/>,
    /// <see cref="SurfaceFormat.HalfVector2"/>, <see cref="SurfaceFormat.HalfVector4"/>
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="SetTextureLevel(Texture2D,int,Vector4[])"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    public static void SetTextureLevel(Texture2D texture, int level, float[] data)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");
      if (data == null)
        throw new ArgumentNullException("data");

      if (texture.Format == SurfaceFormat.Alpha8)
      {
        var buffer = new byte[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = (byte)(data[i] * 255.0f);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.Color)
      {
        var buffer = new Color[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new Color(data[i], data[i], data[i], data[i]);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.Rg32)
      {
        var buffer = new Rg32[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new Rg32(data[i], data[i]);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.Rgba64)
      {
        var buffer = new Rgba64[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new Rgba64(data[i], data[i], data[i], data[i]);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.Single)
      {
        texture.SetData(level, null, data, 0, data.Length);
      }
      else if (texture.Format == SurfaceFormat.Vector2)
      {
        var buffer = new Vector2[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new Vector2(data[i], data[i]);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.Vector4)
      {
        var buffer = new Vector4[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new Vector4(data[i], data[i], data[i], data[i]);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.HalfSingle)
      {
        var buffer = new HalfSingle[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new HalfSingle(data[i]);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.HalfVector2)
      {
        var buffer = new HalfVector2[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new HalfVector2(data[i], data[i]);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else if (texture.Format == SurfaceFormat.HalfVector4)
      {
        var buffer = new HalfVector4[data.Length];
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = new HalfVector4(data[i], data[i], data[i], data[i]);
        texture.SetData(level, null, buffer, 0, buffer.Length);
      }
      else
      {
        throw new NotSupportedException("Texture format '" + texture.Format + "' is not yet supported.");
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Scaling
    //--------------------------------------------------------------

    ///// <overloads>
    ///// <summary>
    ///// Modifies the heights of a height texture.
    ///// </summary>
    ///// </overloads>
    ///// <summary>
    ///// Modifies the heights of a height texture.
    ///// </summary>
    ///// <param name="heights">The height values.</param>
    ///// <param name="scale">The scale.</param>
    ///// <param name="bias">The bias which is added to the scaled heights.</param>
    ////// <inheritdoc cref="TransformTexture(float[],float,float)"/>
    //public static void TransformTexture(Vector4[] data, float scale, float bias)
    //{
    //  if (data == null)
    //    throw new ArgumentNullException("data");

    // // Only transform X or all channels?
    //  for (int i = 0; i < heights.Length; i++)
    //    data[i].X = data[i].X * scale + bias;
    //}


    /// <summary>
    /// Modifies the data of a texture.
    /// </summary>
    /// <param name="data">The texture data.</param>
    /// <param name="scale">The scale factor.</param>
    /// <param name="bias">The bias which is added to the scaled data.</param>
    /// <remarks>
    /// All values in <paramref name="data"/> are multiplied by <paramref name="scale"/> and then
    /// <paramref name="bias"/> is added.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="data"/> is <see langword="null"/>.
    /// </exception>
    public static void TransformTexture(float[] data, float scale, float bias)
    {
      if (data == null)
        throw new ArgumentNullException("data");

      //for (int i = 0; i < data.Length; i++)
      Parallel.For(0, data.Length, i =>
      {
        data[i] = data[i] * scale + bias;
      }, 128);
    }
    #endregion


    //--------------------------------------------------------------
    #region Smoothing
    //--------------------------------------------------------------

    //public static void SmoothTexture(Texture2D texture, float smoothness)
    //{
    //  if (texture == null)
    //    throw new ArgumentNullException("texture");
    //  if (Numeric.IsZero(smoothness))
    //    return;

    //  var data = GetTextureLevelVector4(texture, 0);
    //  SmoothTexture(data, texture.Width, texture.Height, smoothness);
    //  SetTextureLevel(texture, 0, data);
    //}


    /// <overloads>
    /// <summary>
    /// Smooths the texture.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Smooths the texture.
    /// </summary>
    /// <param name="data">The texture data of mipmap level 0.</param>
    /// <param name="textureWidth">The width of the texture.</param>
    /// <param name="textureHeight">The height of the texture.</param>
    /// <param name="smoothness">
    /// The smoothness in the range [0, ∞). (0 means no smoothing. Values greater than 0 means more
    /// smoothing.)
    /// </param>
    /// <remarks>
    /// Use this method if the height map was loaded from an 8-bit image. 8-bit values are usually
    /// insufficient to represent smooth surfaces. This method removes non-smooth terrain parts
    /// caused by 8-bit quantization.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="data"/> is <see langword="null"/>.
    /// </exception>
    public static void SmoothTexture(Vector4[] data, int textureWidth, int textureHeight, float smoothness)
    {
      if (data == null)
        throw new ArgumentNullException("data");

      if (Numeric.IsZero(smoothness))
        return;

      float sharpness = 1.0f / smoothness;

      var result = new Vector4[data.Length];

      // Copy border.
      // (We do not smooth the border texels because then different terrain tiles will
      // not match.)
      // First row.
      Array.Copy(data, result, textureWidth);
      // Last row.
      var lastRowIndex = (textureHeight - 1) * textureWidth;
      Array.Copy(data, lastRowIndex, result, lastRowIndex, textureWidth);
      for (int i = 1; i < textureHeight - 1; i++)
      {
        // First column.
        result[i * textureWidth] = data[i * textureWidth];
        // Last column
        result[i * textureWidth + textureWidth - 1] = data[i * textureWidth + textureWidth - 1];
      }

      //for (int y = 0; y < targetHeight; y++)
      Parallel.For(1, textureHeight - 1, y =>
      {
        for (int x = 1; x < textureWidth - 1; x++)
        {
          Vector4 center = data[y * textureWidth + x];

          // Make texture smoother by averaging 3x3 texels.
          Vector4 average = Vector4.Zero;
          float weightSum = 0;
          for (int sampleY = y - 1; sampleY <= y + 1; sampleY++)
          {
            for (int sampleX = x - 1; sampleX <= x + 1; sampleX++)
            {
              // Use a weight which depends on the height difference to avoid smoothing away cliffs.
              Vector4 sample = data[sampleY * textureWidth + sampleX];
              float w = 1 / (1 + sharpness * Math.Abs(sample.X - center.X));
              average += w * sample;
              weightSum += w;
            }
          }

          average /= weightSum;
          result[y * textureWidth + x] = average;
        }
      });

      // Copy result back to data.
      Array.Copy(result, data, data.Length);
    }


    /// <summary>
    /// Smooths the texture.
    /// </summary>
    /// <param name="data">The texture data of mipmap level 0.</param>
    /// <param name="textureWidth">The width of the texture.</param>
    /// <param name="textureHeight">The height of the texture.</param>
    /// <param name="smoothness">
    /// The smoothness: 0 means no smoothing. Values greater than 0 means more smoothing.
    /// </param>
    /// <inheritdoc cref="SmoothTexture(Vector4[],int,int,float)"/>
    public static void SmoothTexture(float[] data, int textureWidth, int textureHeight, float smoothness)
    {
      if (data == null)
        throw new ArgumentNullException("data");

      if (Numeric.IsZero(smoothness))
        return;

      float sharpness = 1.0f / smoothness;

      var result = new float[data.Length];

      // Copy border.
      // (We do not smooth the border texels because then different terrain tiles will
      // not match.)
      // First row.
      Array.Copy(data, result, textureWidth);
      // Last row.
      var lastRowIndex = (textureHeight - 1) * textureWidth;
      Array.Copy(data, lastRowIndex, result, lastRowIndex, textureWidth);
      for (int i = 1; i < textureHeight - 1; i++)
      {
        // First column.
        result[i * textureWidth] = data[i * textureWidth];
        // Last column
        result[i * textureWidth + textureWidth - 1] = data[i * textureWidth + textureWidth - 1];
      }

      //for (int y = 0; y < targetHeight; y++)
      Parallel.For(1, textureHeight - 1, y =>
      {
        for (int x = 1; x < textureWidth - 1; x++)
        {
          float center = data[y * textureWidth + x];

          // Make texture smoother by averaging 3x3 texels.
          float average = 0;
          float weightSum = 0;
          for (int sampleY = y - 1; sampleY <= y + 1; sampleY++)
          {
            for (int sampleX = x - 1; sampleX <= x + 1; sampleX++)
            {

              // Use a weight which depends on the height difference to avoid smoothing away cliffs.
              float sample = data[sampleY * textureWidth + sampleX];
              float w = 1 / (1 + sharpness * Math.Abs(sample - center));
              average += w * sample;
              weightSum += w;
            }
          }

          average /= weightSum;
          result[y * textureWidth + x] = average;
        }
      });

      // Copy result back to data.
      Array.Copy(result, data, data.Length);
    }
    #endregion


    //--------------------------------------------------------------
    #region Mipmaps
    //--------------------------------------------------------------

    //private static void CreateTerrainGeometryMipLevels(Texture2D texture, bool useNearestNeighborFilter)
    //{
    //  if (texture == null)
    //    throw new ArgumentNullException("texture");
    //  if (texture.LevelCount == 0)
    //    throw new ArgumentException("Input texture must contain mipmap levels.", "texture");

    //  var dataLevel0 = GetTextureLevelVector4(texture, 0);
    //  CreateTerrainGeometryMipLevels(texture, dataLevel0, useNearestNeighborFilter);
    //}


    /// <overloads>
    /// <summary>
    /// Creates the mipmaps for a terrain texture.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates the mipmaps for a terrain texture.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="dataLevel0">
    /// Optional: The data of mipmap level 0, if available. (If the parameter is
    /// <see langword="null"/> the data is read from <paramref name="texture"/>.)
    /// </param>
    /// <param name="useNearestNeighborFilter">
    /// <see langword="true"/> to use nearest-neighbor filtering (= every second pixel is dropped)
    /// for creating mipmaps. <see langword="false"/> to use a 3x3 filter (default).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="texture"/> does not contain mipmaps.
    /// </exception>
    private static void CreateTerrainGeometryMipLevels(Texture2D texture, Vector4[] dataLevel0, bool useNearestNeighborFilter)
    {
      // TODO: The 3x3 filter is separable. Would this be faster if we make a horizontal and a vertical pass instead of one pass?

      if (texture == null)
        throw new ArgumentNullException("texture");
      if (texture.LevelCount == 0)
        throw new ArgumentException("Input texture must contain mipmap levels.", "texture");

      if (dataLevel0 == null)
        dataLevel0 = GetTextureLevelVector4(texture, 0);

      // ----- Create other mipmap levels using a 3x3 filter.
      // If mipmaps are computed using a standard 2x2 filter then the whole terrain seems
      // to shift horizontally and vertically when it changes the LOD.
      // (It would be faster to compute this on the GPU, but in XNA we cannot render into
      // individual mipmap levels.)
      int level = 1;
      int previousWidth = texture.Width;
      int previousHeight = texture.Height;
      var previousBuffer = dataLevel0;
      while (previousWidth != 1 && previousHeight != 1)
      {
        int currentWidth = Math.Max(1, previousWidth / 2);
        int currentHeight = Math.Max(1, previousHeight / 2);
        var currentBuffer = new Vector4[currentWidth * currentHeight];

        if (currentHeight <= 64 || useNearestNeighborFilter)
        {
          for (int y = 0; y < currentHeight; y++)
            FilterRowLinear(y, currentBuffer, currentWidth, previousBuffer, previousWidth, useNearestNeighborFilter);
        }
        else
        {
          var buffer = previousBuffer;
          var width = previousWidth;
          Parallel.For(0, currentHeight, y =>
            FilterRowLinear(y, currentBuffer, currentWidth, buffer, width, false));
        }

        SetTextureLevel(texture, level, currentBuffer);

        previousWidth = currentWidth;
        previousHeight = currentHeight;
        previousBuffer = currentBuffer;
        level++;
      }
    }


    /// <summary>
    /// Creates the mipmaps for a terrain texture.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="dataLevel0">
    /// Optional: The data of mipmap level 0, if available. (If the parameter is
    /// <see langword="null"/> the data is read from <paramref name="texture"/>.)
    /// </param>
    /// <param name="useNearestNeighborFilter">
    /// <see langword="true"/> to use nearest-neighbor filtering (= every second pixel is dropped)
    /// for creating mipmaps. <see langword="false"/> to use a 3x3 filter (default).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="texture"/> does not contain mipmaps.
    /// </exception>
    private static void CreateTerrainGeometryMipLevels(Texture2D texture, float[] dataLevel0, bool useNearestNeighborFilter)
    {
      // TODO: The 3x3 filter is separable. Would this be faster if we a horizontal and a vertical pass instead of one pass.

      if (texture == null)
        throw new ArgumentNullException("texture");
      if (texture.LevelCount == 0)
        throw new ArgumentException("Input texture must contain mipmap levels.", "texture");

      if (dataLevel0 == null)
        dataLevel0 = GetTextureLevelSingle(texture, 0);

      // ----- Create other mip levels using a 3x3 filter.
      // If mipmaps are computed using a standard 2x2 filter then the whole terrain seems
      // to shift horizontally and vertically when it changes the LOD.
      // (It would be faster to compute this on the GPU, but in XNA we cannot render into
      // individual mip levels.)
      int level = 1;
      int previousWidth = texture.Width;
      int previousHeight = texture.Height;
      var previousBuffer = dataLevel0;
      while (previousWidth != 1 && previousHeight != 1)
      {
        int currentWidth = Math.Max(1, previousWidth / 2);
        int currentHeight = Math.Max(1, previousHeight / 2);
        var currentBuffer = new float[currentWidth * currentHeight];

        if (currentHeight <= 64 || useNearestNeighborFilter)
        {
          for (int y = 0; y < currentHeight; y++)
            FilterRowLinear(y, currentBuffer, currentWidth, previousBuffer, previousWidth, useNearestNeighborFilter);
        }
        else
        {
          var buffer = previousBuffer;
          var width = previousWidth;
          Parallel.For(0, currentHeight, y =>
            FilterRowLinear(y, currentBuffer, currentWidth, buffer, width, false));
        }

        SetTextureLevel(texture, level, currentBuffer);

        previousWidth = currentWidth;
        previousHeight = currentHeight;
        previousBuffer = currentBuffer;
        level++;
      }
    }


    private static void FilterRowLinear(int y, Vector4[] currentBuffer, int currentWidth, Vector4[] previousBuffer, int previousWidth, bool useNearestNeighborFilter)
    {
      if (useNearestNeighborFilter)
      {
        for (int x = 0; x < currentWidth; x++)
          currentBuffer[y * currentWidth + x] = previousBuffer[y * 2 * previousWidth + x * 2];
      }
      else
      {
        // Filter 3x3 samples using [1, 2, 1] kernel.
        // In 2D this is 
        //  1 2 1
        //  2 4 2
        //  1 2 1
        float[] weights = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
        //float[] weights = { 0, 0, 0, 0, 1, 1, 0, 1, 1 };
        for (int x = 0; x < currentWidth; x++)
        {
          float weightSum = 0;
          int weightIndex = 0;
          Vector4 average = Vector4.Zero;
          for (int sampleY = y * 2 - 1; sampleY <= y * 2 + 1; sampleY++)
          {
            int clampedY = (sampleY >= 0) ? sampleY : 0;
            for (int sampleX = x * 2 - 1; sampleX <= x * 2 + 1; sampleX++)
            {
              float weight = weights[weightIndex++];
              int clampedX = (sampleX >= 0) ? sampleX : 0;
              average += previousBuffer[clampedY * previousWidth + clampedX] * weight;
              weightSum += weight;
            }
          }
          average /= weightSum;

          currentBuffer[y * currentWidth + x] = average;
        }
      }
    }


    private static void FilterRowLinear(int y, float[] currentBuffer, int currentWidth, float[] previousBuffer, int previousWidth, bool useNearestNeighborFilter)
    {
      if (useNearestNeighborFilter)
      {
        for (int x = 0; x < currentWidth; x++)
          currentBuffer[y * currentWidth + x] = previousBuffer[y * 2 * previousWidth + x * 2];
      }
      else
      {
        // Filter 3x3 samples using [1, 2, 1] kernel.
        // In 2D this is 
        //  1 2 1
        //  2 4 2
        //  1 2 1
        float[] weights = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
        //float[] weights = { 0, 0, 0, 0, 1, 1, 0, 1, 1 };
        for (int x = 0; x < currentWidth; x++)
        {
          float weightSum = 0;
          int weightIndex = 0;
          float average = 0;
          for (int sampleY = y * 2 - 1; sampleY <= y * 2 + 1; sampleY++)
          {
            int clampedY = (sampleY >= 0) ? sampleY : 0;
            for (int sampleX = x * 2 - 1; sampleX <= x * 2 + 1; sampleX++)
            {
              float weight = weights[weightIndex++];
              int clampedX = (sampleX >= 0) ? sampleX : 0;
              average += previousBuffer[clampedY * previousWidth + clampedX] * weight;
              weightSum += weight;
            }
          }
          average /= weightSum;

          currentBuffer[y * currentWidth + x] = average;
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Texture creation
    //--------------------------------------------------------------

    //public static void CreateHeightTexture(Texture2D inputHeightTexture, float minHeight, float maxHeight, float smoothness, bool useNearestNeighborFilter, ref Texture2D heightTexture)
    //{
    //  if (inputHeightTexture == null)
    //    throw new ArgumentNullException("inputHeightTexture");
    //  if (inputHeightTexture.Format == SurfaceFormat.Alpha8)
    //    throw new NotSupportedException("SurfaceFormat Alpha8 is not yet supported for height maps.");

    //  var heights = GetTextureLevelSingle(inputHeightTexture, 0);
    //  TransformTexture(heights, (maxHeight - minHeight), minHeight);
    //  SmoothTexture(heights, inputHeightTexture.Width, inputHeightTexture.Height, smoothness);
    //  CreateHeightTexture(inputHeightTexture.GraphicsDevice, heights, inputHeightTexture.Width, inputHeightTexture.Height, useNearestNeighborFilter, ref heightTexture);
    //}


    //// heights must already be scaled and smoothed as needed.
    //public static void CreateHeightTexture(GraphicsDevice graphicsDevice, Vector4[] heights, int textureWidth, int textureHeight, bool useNearestNeighborFilter, ref Texture2D heightTexture)
    //{
    //  if (graphicsDevice == null)
    //    throw new ArgumentNullException("graphicsDevice");
    //  if (heights == null)
    //    throw new ArgumentNullException("heights");

    //  if (heightTexture != null)
    //  {
    //    if (heightTexture.Height != textureHeight)
    //      throw new ArgumentException("Height of output texture does not match textureHeight.");
    //    if (heightTexture.Width != textureWidth)
    //      throw new ArgumentException("Width of output texture does not match textureWidth.");
    //    if (heightTexture.Format != SurfaceFormat.HalfSingle)
    //      throw new ArgumentException("Format of output texture must be HalfSingle.");
    //    if (heightTexture.LevelCount == 0)
    //      throw new ArgumentException("Output texture does not have mipmaps.");
    //  }

    //  if (heightTexture == null)
    //    heightTexture = new Texture2D(
    //      graphicsDevice,
    //      textureWidth,
    //      textureHeight,
    //      true,
    //      SurfaceFormat.HalfSingle);

    //  SetTextureLevel(heightTexture, 0, heights);
    //  CreateTerrainGeometryMipLevels(heightTexture, heights, useNearestNeighborFilter);
    //}


    /// <summary>
    /// Creates a height map which can be used for terrain rendering.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="heights">The heights (= texture content of the first mipmap level).</param>
    /// <param name="textureWidth">The width of the texture.</param>
    /// <param name="textureHeight">The height of the texture.</param>
    /// <param name="useNearestNeighborFilter">
    /// <see langword="true"/> to use nearest-neighbor filtering (= every second pixel is dropped)
    /// for creating mipmaps. <see langword="false"/> to use a 3x3 filter (default).
    /// </param>
    /// <param name="heightTexture">
    /// The created height texture. If this parameter is set to a matching texture, then the content
    /// of this texture is updated and no new texture is created. (If this method is called with a
    /// texture with wrong size or format, an exception is thrown.)
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> or <paramref name="heights"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="heightTexture"/> does not match the specified parameters.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
    public static void CreateHeightTexture(GraphicsDevice graphicsDevice, float[] heights,
                                           int textureWidth, int textureHeight,
                                           bool useNearestNeighborFilter, ref Texture2D heightTexture)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (heights == null)
        throw new ArgumentNullException("heights");

      if (heightTexture != null)
      {
        if (heightTexture.Height != textureHeight)
          throw new ArgumentException("Height of output texture does not match textureHeight.");
        if (heightTexture.Width != textureWidth)
          throw new ArgumentException("Width of output texture does not match textureWidth.");
        if (heightTexture.Format != SurfaceFormat.HalfSingle)
          throw new ArgumentException("Format of output texture must be HalfSingle.");
        if ((textureWidth > 1 || textureHeight > 1) && heightTexture.LevelCount == 1)
          throw new ArgumentException("Output texture does not have mipmaps.");
      }
      else
      {
        heightTexture = new Texture2D(
          graphicsDevice,
          textureWidth,
          textureHeight,
          true,
          SurfaceFormat.HalfSingle);
      }

      SetTextureLevel(heightTexture, 0, heights);
      CreateTerrainGeometryMipLevels(heightTexture, heights, useNearestNeighborFilter);
    }


    //// Caller must dispose the returned normal map when it is not needed anymore.
    //public static void CreateNormalTexture(Texture2D heightTexture, float cellSize, ref Texture2D normalTexture)
    //{
    //  if (heightTexture == null)
    //    throw new ArgumentNullException("heightTexture");
    //  if (heightTexture.Format == SurfaceFormat.Alpha8)
    //    throw new NotSupportedException("SurfaceFormat Alpha8 is not yet supported for height maps.");

    //  var heights = GetTextureLevelVector4(heightTexture, 0);
    //  CreateNormalTexture(heightTexture.GraphicsDevice, heights, heightTexture.Width, heightTexture.Height, cellSize, ref normalTexture);
    //}


    /// <summary>
    /// Creates a normal map which can be used for terrain rendering.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="heights">The terrain heights.</param>
    /// <param name="textureWidth">The width of the texture.</param>
    /// <param name="textureHeight">The height of the texture.</param>
    /// <param name="cellSize">The cell size of the height map.</param>
    /// <param name="useNearestNeighborFilter">
    /// <see langword="true"/> to use nearest-neighbor filtering (= every second pixel is dropped)
    /// for creating mipmaps. <see langword="false"/> to use a 3x3 filter (default).
    /// </param>
    /// <param name="normalTexture">
    /// The created height texture. If this parameter is set to a matching texture, then the content
    /// of this texture is updated and no new texture is created. (If this method is called with a
    /// texture with wrong size or format, an exception is thrown.)
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> or <paramref name="heights"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="normalTexture"/> does not match the specified parameters.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
    public static void CreateNormalTexture(GraphicsDevice graphicsDevice, float[] heights,
                                           int textureWidth, int textureHeight, float cellSize,
                                           bool useNearestNeighborFilter, ref Texture2D normalTexture)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (heights == null)
        throw new ArgumentNullException("heights");

      int w = textureWidth;
      int h = textureHeight;

      if (normalTexture != null)
      {
        if (normalTexture.Width != w)
          throw new ArgumentException("Width of normal texture does not match textureWidth.");
        if (normalTexture.Height != h)
          throw new ArgumentException("Height of normal texture does not match textureHeight.");
        if (normalTexture.Format != SurfaceFormat.Color)
          throw new ArgumentException("Format of normal texture must be Color.");
        if ((w > 1 || h > 1) && normalTexture.LevelCount == 1)
          throw new ArgumentException("Normal texture does not have mipmaps.");
      }
      else
      {
        normalTexture = new Texture2D(graphicsDevice, w, h, true, SurfaceFormat.Color);
      }

      var normals = new Vector4[heights.Length];

      //for (int y = 0; y < height; y++)
      Parallel.For(0, textureHeight, y =>
      {
        int yMinus1 = (y > 0) ? y - 1 : 0;
        int yPlus1 = (y < h - 2) ? y + 1 : y;
        for (int x = 0; x < w; x++)
        {
          // The normals are computed using the Sobel filter kernel:
          //  -1 0 +1
          //  -2 0 +2
          //  -1 0 +1
          float deltaX = 0;
          float deltaZ = 0;
          int xMinus1 = (x > 0) ? x - 1 : x;
          int xPlus1 = (x < w - 2) ? x + 1 : x;
          deltaX += 1 * (heights[yMinus1 * w + xMinus1] - heights[yMinus1 * w + xPlus1]);
          deltaX += 2 * (heights[y * w + xMinus1] - heights[y * w + xPlus1]);
          deltaX += 1 * (heights[yPlus1 * w + xMinus1] - heights[yPlus1 * w + xPlus1]);

          deltaZ += 1 * (heights[yMinus1 * w + xMinus1] - heights[yPlus1 * w + xMinus1]);
          deltaZ += 2 * (heights[yMinus1 * w + x] - heights[yPlus1 * w + x]);
          deltaZ += 1 * (heights[yMinus1 * w + xPlus1] - heights[yPlus1 * w + xPlus1]);

          deltaX /= 4.0f;
          deltaZ /= 4.0f;

          // Instead of the cross product we can compute the normal directly.
          // See derivation in "Fast Heightfield Normal Calculation", Game Programming Gems 3, pp. 344.
          Vector3F normal = new Vector3F(deltaX, 2 * cellSize, deltaZ);

          normal.TryNormalize();

          // Change order of z and y to standard normal map order.
          // Convert [-1, 1] range to [0, 1].
          normals[y * w + x] = new Vector4(
            normal.X / 2.0f + 0.5f,
            -normal.Z / 2.0f + 0.5f,    // Invert to create standard "green-up" normal maps.
            normal.Y / 2.0f + 0.5f,
            1);
        }
      });

      SetTextureLevel(normalTexture, 0, normals);
      CreateTerrainGeometryMipLevels(normalTexture, normals, useNearestNeighborFilter);
    }


    /// <summary>
    /// Creates a hole map which can be used for terrain rendering.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="data">An array with hole information (1 = no hole, 0 = hole).</param>
    /// <param name="textureWidth">The width of the texture.</param>
    /// <param name="textureHeight">The height of the texture.</param>
    /// <param name="useNearestNeighborFilter">
    /// <see langword="true"/> to use nearest-neighbor filtering (= every second pixel is dropped)
    /// for creating mipmaps. <see langword="false"/> to use a 3x3 filter (default).
    /// </param>
    /// <param name="holeTexture">
    /// The created height texture. If this parameter is set to a matching texture, then the content
    /// of this texture is updated and no new texture is created. (If this method is called with a
    /// texture with wrong size or format, an exception is thrown.)
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> or <paramref name="data"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="holeTexture"/> does not match the specified parameters.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
    public static void CreateHoleTexture(GraphicsDevice graphicsDevice, float[] data,
                                         int textureWidth, int textureHeight,
                                         bool useNearestNeighborFilter, ref Texture2D holeTexture)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (data == null)
        throw new ArgumentNullException("data");

      if (holeTexture != null)
      {
        if (holeTexture.Width != textureWidth)
          throw new ArgumentException("Width of hole texture does not match textureWidth.");
        if (holeTexture.Height != textureHeight)
          throw new ArgumentException("Height of hole texture does not match textureHeight.");
        if (holeTexture.Format != SurfaceFormat.Alpha8)
          throw new ArgumentException("Format of hole texture must be Alpha8.");
        if ((textureWidth > 1 || textureHeight > 1) && holeTexture.LevelCount == 1)
          throw new ArgumentException("Hole texture does not have mipmaps.");
      }
      else
      {
        holeTexture = new Texture2D(graphicsDevice, textureWidth, textureHeight, true, SurfaceFormat.Alpha8);
      }

      SetTextureLevel(holeTexture, 0, data);
      CreateTerrainGeometryMipLevels(holeTexture, data, useNearestNeighborFilter);
    }
    #endregion
  }
}
