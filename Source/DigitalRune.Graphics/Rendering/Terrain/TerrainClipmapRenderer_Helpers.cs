// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Effects;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  public partial class TerrainClipmapRenderer
  {
    private static float RoundToGrid(float position, float cellSize)
    {
      return (float)Math.Floor(position / cellSize + 0.5f) * cellSize;
    }


    //private static float RoundDownToGrid(float position, float cellSize)
    //{
    //  // Snap to top-left grid cell, e.g. if cellSize is 1 then 6.x snaps to 6.0.
    //  // We add a small epsilon to avoid rounding 513.999999 down to 513.
    //  return (float)Math.Floor(position / cellSize + 0.01f) * cellSize;
    //}


    //private static float RoundUpToGrid(float position, float cellSize)
    //{
    //  return (float)Math.Ceiling(position / cellSize - 0.01f) * cellSize;
    //}


    private static Vector2F RoundToGrid(Vector2F position, float cellSize)
    {
      position.X = RoundToGrid(position.X, cellSize);
      position.Y = RoundToGrid(position.Y, cellSize);
      return position;
    }


    //private static Vector2F RoundDownToGrid(Vector2F position, float cellSize)
    //{
    //  position.X = RoundDownToGrid(position.X, cellSize);
    //  position.Y = RoundDownToGrid(position.Y, cellSize);
    //  return position;
    //}


    //private static Vector2F RoundUpToGrid(Vector2F position, float cellSize)
    //{
    //  position.X = RoundUpToGrid(position.X, cellSize);
    //  position.Y = RoundUpToGrid(position.Y, cellSize);
    //  return position;
    //}


    ///// <summary>
    ///// Converts a world space position to texture coordinates for a given clipmap level.
    ///// (Does not handle texture atlas.)
    ///// </summary>
    ///// <param name="position">The world space xz position.</param>
    ///// <param name="levelOrigin">The world space xz level origin.</param>
    ///// <param name="levelSize">The world space size of the level.</param>
    ///// <returns>
    ///// The texture coordinates.
    ///// </returns>
    //private static Vector2F ConvertPositionToTexCoord(Vector2F position, Vector2F levelOrigin, float levelSize)
    //{
    //  return (position - levelOrigin) / levelSize;
    //}


    //private static Vector2F ConvertTexCoordToClipmapTextureAtlas(Vector2F texCoord, int level, int numberOfLevels, int numberOfColumns)
    //{
    //  // The clipmaps are stored in a texture atlas. 

    //  Debug.Assert(texCoord.X >= -0.00001);
    //  Debug.Assert(texCoord.Y >= -0.00001);
    //  Debug.Assert(texCoord.X <= 1.0001);
    //  Debug.Assert(texCoord.Y <= 1.0001);

    //  int numberOfRows = (numberOfLevels - 1) / numberOfColumns + 1;
    //  int column = level % numberOfColumns;
    //  int row = level / numberOfColumns;
    //  texCoord.X = (texCoord.X + column) / numberOfColumns;
    //  texCoord.Y = (texCoord.Y + row) / numberOfRows;
    //  return texCoord;
    //}


    //private static Rectangle GetScreenSpaceRectangle(TerrainClipmap clipmap, int level, float levelSize, Vector2F startPosition, Vector2F endPosition, Viewport viewport)
    //{
    //  int numberOfLevels = clipmap.NumberOfLevels;
    //  int numberOfColumns = clipmap.NumberOfTextureAtlasColumns;
    //  var levelOrigin = clipmap.Origins[level];

    //  // TexCoords relative to a non-texture atlas texture.
    //  var startTexCoord = ConvertPositionToTexCoord(startPosition, levelOrigin, levelSize);
    //  var endTexCoord = ConvertPositionToTexCoord(endPosition, levelOrigin, levelSize);
      
    //  // TexCoords relative to the texture atlas.
    //  startTexCoord = ConvertTexCoordToClipmapTextureAtlas(startTexCoord, level, clipmap.NumberOfLevels, numberOfColumns);
    //  endTexCoord = ConvertTexCoordToClipmapTextureAtlas(endTexCoord, level, numberOfLevels, numberOfColumns);

    //  Debug.Assert(Numeric.AreEqual((int)(startTexCoord.X * viewport.Width + 0.5), startTexCoord.X * viewport.Width),
    //               "Clipmap coordinates are not snapped to grid positions.");

    //  return new Rectangle(
    //    (int)(startTexCoord.X * viewport.Width + 0.5),
    //    (int)(startTexCoord.Y * viewport.Height + 0.5),
    //    (int)((endTexCoord.X - startTexCoord.X) * viewport.Width + 0.5),
    //    (int)((endTexCoord.Y - startTexCoord.Y) * viewport.Height + 0.5));
    //}


    /// <summary>
    /// Gets the screen space rectangle of the given clipmap level.
    /// </summary>
    private static Rectangle GetScreenSpaceRectangle(TerrainClipmap clipmap, int level)
    {
      //int numberOfLevels = clipmap.NumberOfLevels;
      int numberOfColumns = clipmap.NumberOfTextureAtlasColumns;
      //int numberOfRows = (numberOfLevels - 1) / numberOfColumns + 1;
      int column = level % numberOfColumns;
      int row = level / numberOfColumns;
      int cellsPerLevel = clipmap.CellsPerLevel;

      return new Rectangle(column * cellsPerLevel, row * cellsPerLevel, cellsPerLevel, cellsPerLevel);
    }


    /// <summary>
    /// Initializes TerrainClipmap.Textures (and also updates TerrainClipmap.UseIncrementalUpdate).
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="clipmap">The clipmap.</param>
    private static void InitializeClipmapTextures(GraphicsDevice graphicsDevice, TerrainClipmap clipmap)
    {
      int width, height;
      GetClipmapSize(clipmap.NumberOfLevels, clipmap.CellsPerLevel, out width, out height);

      for (int i = 0; i < clipmap.Textures.Length; i++)
      {
        if (clipmap.Textures[i] == null
            || clipmap.Textures[i].IsDisposed
            || clipmap.Textures[i].Width != width
            || clipmap.Textures[i].Height != height
            // If width or height > 1 then check for mipmaps.
            || (width > 1 || height > 1) && (clipmap.Textures[i].LevelCount > 1) != clipmap.EnableMipMap
          )
        {
          // Texture format has changed.
          clipmap.UseIncrementalUpdate = false;
          clipmap.Textures[i].SafeDispose();

          try
          {
            bool enableMips = clipmap.EnableMipMap;

            clipmap.Textures[i] = new RenderTarget2D(
              graphicsDevice, width, height, enableMips,
              clipmap.Format, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
          }
          catch (Exception exception)
          {
            throw new GraphicsException(
              "Could not create terrain clipmap. See inner exception for details.",
              exception);
          }
        }
        else
        {
          // Texture format unchanged.
          if (((RenderTarget2D)clipmap.Textures[i]).IsContentLost)
            clipmap.UseIncrementalUpdate = false;
        }
      }
    }


    /// <summary>
    /// Computes the width and height of the whole clipmap.
    /// </summary>
    /// <exception cref="GraphicsException">
    /// Invalid clipmap layout. The number of levels or number of cells per level is too large.
    /// </exception>
    private static void GetClipmapSize(int numberOfLevels, int cellsPerLevel, out int width, out int height)
    {
#if !MONOGAME
      const int maxTextureWidth = 4096;
#else
      const int maxTextureWidth = 8192;
#endif
      int maxNumberOfColumns = maxTextureWidth / cellsPerLevel;

      // Simple brute force search for best layout:
      int minNumberOfWastedTiles = Int32.MaxValue;
      int bestNumberOfColumns = 0;
      int bestNumberOfRows = 0;
      for (int i = 1; i <= maxNumberOfColumns; i++)
      {
        int numberOfColumns = i;
        int numberOfRows = (numberOfLevels - 1) / numberOfColumns + 1;

        if (numberOfRows > numberOfColumns)
          continue;

        // Compute empty tiles in texture atlas.
        int numberOfWastedTiles = numberOfRows * numberOfColumns - numberOfLevels;
        if (numberOfWastedTiles < minNumberOfWastedTiles)
        {
          minNumberOfWastedTiles = numberOfWastedTiles;
          bestNumberOfColumns = numberOfColumns;
          bestNumberOfRows = numberOfRows;
        }
      }

      // Special cases:
      if (bestNumberOfColumns == 0)
        throw new GraphicsException(
          "Invalid clipmap layout. Cannot allocate clipmap. The number of levels or number of cells per level is too large.");

      Debug.Assert(bestNumberOfColumns * cellsPerLevel <= maxTextureWidth);
      Debug.Assert(bestNumberOfRows * cellsPerLevel <= maxTextureWidth);

      width = cellsPerLevel * bestNumberOfColumns;
      height = cellsPerLevel * bestNumberOfRows;
    }


    // Check for AABB contact, ignoring Y!
    private static bool HaveContactXZ(ref Aabb aabbA, ref Aabb aabbB)
    {
      // Note: The following check is safe if one AABB is undefined (NaN).
      // Do not change the comparison operator!
      return aabbA.Minimum.X < aabbB.Maximum.X
             && aabbA.Maximum.X > aabbB.Minimum.X
             && aabbA.Minimum.Z < aabbB.Maximum.Z
             && aabbA.Maximum.Z > aabbB.Minimum.Z;
    }


    // Add newAabb to the finalAabb list. If there is an overlap, the new AABB is clipped.
    // The final list will not have any overlaps.
    private void AddAabbWithClipping(List<Aabb> finalAabbs, ref Aabb newAabb)
    {
      // Add all new AABBs to stack.
      _aabbStack.Clear();
      _aabbStack.Push(newAabb);

      // Test all AABBs from the stack against the final AABBs. If an AABB passes all final AABB
      // overlap tests, it is added to final AABBs. If an AABB overlaps a final AABB, it is cut
      // into two parts. These two parts replace the AABB on the stack. 
      int finalAabbsIndex = 0;
      while (_aabbStack.Count > 0)
      {
        // Get AABB from stack but do not remove it.
        var aabb = _aabbStack.Peek();

        if (finalAabbsIndex >= finalAabbs.Count)
        {
          // AABB was checked against all final AABBs.
          // Move from stack to final list.
          _aabbStack.Pop();
          finalAabbs.Add(aabb);
          finalAabbsIndex = 0;
          continue;
        }

        // Cut against finalAabbs[finalAabbsIndex].
        var finalAabb = finalAabbs[finalAabbsIndex];
        if (!HaveContactXZ(ref aabb, ref finalAabb))
        {
          // No overlap. check against next AABB.
          finalAabbsIndex++;
          continue;
        }

        finalAabbsIndex = 0;

        // Overlap! Cut this AABB into two.
        // Remove from stack.
        _aabbStack.Pop();

        var aabbA = aabb;
        var aabbB = aabb;

        if (aabb.Minimum.X < finalAabb.Minimum.X)
        {
          // Cut at final AABB min x:
          aabbA.Maximum.X = finalAabb.Minimum.X;
          aabbB.Minimum.X = finalAabb.Minimum.X;
        }
        else if (aabb.Maximum.X > finalAabb.Maximum.X)
        {
          // Cut at final AABB max x:
          aabbA.Maximum.X = finalAabb.Maximum.X;
          aabbB.Minimum.X = finalAabb.Maximum.X;
        }
        else if (aabb.Minimum.Z < finalAabb.Minimum.Z)
        {
          // Cut at final AABB min z:
          aabbA.Maximum.Z = finalAabb.Minimum.Z;
          aabbB.Minimum.Z = finalAabb.Minimum.Z;
        }
        else if (aabb.Maximum.Z > finalAabb.Maximum.Z)
        {
          // Cut at final AABB max z:
          aabbA.Maximum.Z = finalAabb.Maximum.Z;
          aabbB.Minimum.Z = finalAabb.Maximum.Z;
        }
        else
        {
          // AABB is totally inside and is not needed.
          
          continue;
        }

        // Add non-empty AABBs to stack.
        if (!Numeric.AreEqual(aabbA.Minimum.X, aabbA.Maximum.X)
            && !Numeric.AreEqual(aabbA.Minimum.Z, aabbA.Maximum.Z))
          _aabbStack.Push(aabbA);

        if (!Numeric.AreEqual(aabbB.Minimum.X, aabbB.Maximum.X)
            && !Numeric.AreEqual(aabbB.Minimum.Z, aabbB.Maximum.Z))
          _aabbStack.Push(aabbB);
      }
    }


    private static bool UpdateAndApplyParameter<T>(EffectBinding effectBinding, string name, T value, RenderContext context)
    {
      var parameterBinding = effectBinding.ParameterBindings[name] as ConstParameterBinding<T>;
      if (parameterBinding != null)
      {
        parameterBinding.Value = value;
        parameterBinding.Update(context);
        parameterBinding.Apply(context);
        return true;
      }

      return false;
    }
  }
}
#endif
