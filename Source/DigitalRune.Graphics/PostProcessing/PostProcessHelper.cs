// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Provides default instances of post-processors that are used frequently.
  /// </summary>
  public static class PostProcessHelper
  {
    /// <summary>
    /// Gets a default <see cref="DownsampleFilter"/> that can be used to downsample an image into
    /// a low-resolution render target.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>The default <see cref="DownsampleFilter"/>.</returns>
    /// <inheritdoc cref="PostProcessHelper"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static DownsampleFilter GetDownsampleFilter(this IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string Key = "__DownsampleFilter";
      object obj;
      graphicsService.Data.TryGetValue(Key, out obj);
      var instance = obj as DownsampleFilter;
      if (instance == null)
      {
        instance = new DownsampleFilter(graphicsService);
        graphicsService.Data[Key] = instance;
      }

      return instance;
    }


    /// <summary>
    /// Gets a default <see cref="UpsampleFilter"/> that can be used to upsample a low-resolution
    /// image. (Internal only. Properties of <see cref="UpsampleFilter"/> need to be set before
    /// use.)
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>The default <see cref="DownsampleFilter"/>.</returns>
    /// <inheritdoc cref="PostProcessHelper"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    internal static UpsampleFilter GetUpsampleFilter(this IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string Key = "__UpsampleFilter";
      object obj;
      graphicsService.Data.TryGetValue(Key, out obj);
      var instance = obj as UpsampleFilter;
      if (instance == null)
      {
        instance = new UpsampleFilter(graphicsService);
        graphicsService.Data[Key] = instance;
      }

      return instance;
    }


    /// <summary>
    /// Gets a default <see cref="CopyFilter"/> that can be used to copy a texture into a render
    /// target.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>The default <see cref="CopyFilter"/>.</returns>
    /// <inheritdoc cref="PostProcessHelper"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static CopyFilter GetCopyFilter(this IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string Key = "__CopyFilter";
      object obj;
      graphicsService.Data.TryGetValue(Key, out obj);
      var instance = obj as CopyFilter;
      if (instance == null)
      {
        instance = new CopyFilter(graphicsService);
        graphicsService.Data[Key] = instance;
      }

      return instance;
    }
  }
}
#endif
