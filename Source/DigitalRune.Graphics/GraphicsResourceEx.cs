// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  // Unfortunately, we cannot easily extend a graphics resource: There is no way for sub-classing 
  // most GraphicsResource types in XNA.

  /// <summary>
  /// Stores additional data for a graphics resource.
  /// </summary>
  /// <typeparam name="T">The type of graphics resource.</typeparam>
  /// <remarks>
  /// <para>
  /// A <see cref="GraphicsResourceEx{T}"/> represents additional information attached to a graphics
  /// resource. For example: The <see cref="EffectEx"/> extends the <see cref="Effect"/>.
  /// </para>
  /// <para>
  /// The <see cref="GraphicsResourceEx{T}"/> is stored in the <see cref="GraphicsResource.Tag"/>
  /// property. Note that accessing the <see cref="GraphicsResource.Tag"/> property is slow because
  /// XNA internally uses some complex store instead of simple backing fields. It is therefore
  /// recommended to cache the reference to the <see cref="GraphicsResourceEx{T}"/> object where
  /// needed!
  /// </para>
  /// </remarks>
  internal abstract class GraphicsResourceEx<T> where T : GraphicsResource
  {
    /// <summary>Temporary ID set during rendering.</summary>
    internal uint Id;


    /// <summary>
    /// Gets the graphics resource.
    /// </summary>
    /// <value>The graphics resource.</value>
    public T Resource { get; private set; }


    /// <summary>
    /// Initializes additional information for a graphics resource.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    protected virtual void Initialize(IGraphicsService graphicsService)
    {      
    }


    /// <summary>
    /// Gets or creates the <see cref="GraphicsResourceEx{T}"/> object for a graphics resource.
    /// </summary>
    /// <typeparam name="TEx">The derived type.</typeparam>
    /// <param name="resource">The graphics resource.</param>
    /// <param name="graphicsService">The graphics service. Can be <see langword="null"/>.</param>
    /// <returns>
    /// The additional information attached to <paramref name="resource"/>.
    /// </returns>
    /// <exception cref="GraphicsException">
    /// The <see cref="GraphicsResource.Tag"/> property is already used.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected static TEx GetOrCreate<TEx>(T resource, IGraphicsService graphicsService) 
      where TEx : GraphicsResourceEx<T>, new()
    {
      if (resource == null)
        return null;

      var tag = resource.Tag;
      var resourceEx = tag as TEx;
      if (tag != null && resourceEx == null)
        throw new GraphicsException(
          "The graphics service needs to store additional data in GraphicsResource.Tag, " +
          "but the property is already used. Clear the Tag property and try again!");

      // ReSharper disable ConditionIsAlwaysTrueOrFalse
      // ReSharper disable ExpressionIsAlwaysNull
      // ReSharper disable RedundantAssignment
      
      // Create GraphicsResourceEx<T> if missing. (Use double-check to avoid unnecessary lock.)
      if (resourceEx == null)
      {
        lock (resource)
        {
          resourceEx = resource.Tag as TEx;
          if (resourceEx == null)
          {
            resourceEx = new TEx { Resource = resource };
            resource.Tag = resourceEx;
            resourceEx.Initialize(graphicsService);
          }
        }
      }

      // Note: The backing field of Effect.Tag should be declared as 'volatile'.
      // Unfortunately, that is out of our control.

      return resourceEx;

      // ReSharper restore ConditionIsAlwaysTrueOrFalse
      // ReSharper restore ExpressionIsAlwaysNull
      // ReSharper restore RedundantAssignment
    }
  }
}
