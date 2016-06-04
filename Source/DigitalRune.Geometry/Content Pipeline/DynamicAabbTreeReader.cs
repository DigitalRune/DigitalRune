// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Partitioning;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Geometry.Content
{
  /// <summary>
  /// Reads a <see cref="DynamicAabbTree{T}"/> from binary format.
  /// </summary>
  /// <typeparam name="T">The type of item in the spatial partition.</typeparam>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class DynamicAabbTreeReader<T> : ContentTypeReader<DynamicAabbTree<T>>
  {
#if !MONOGAME
    /// <summary>
    /// Determines if deserialization into an existing object is possible.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the type can be deserialized into an existing instance; 
    /// <see langword="false"/> otherwise.
    /// </value>
    public override bool CanDeserializeIntoExistingObject
    {
      get { return true; }
    }
#endif


    /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override DynamicAabbTree<T> Read(ContentReader input, DynamicAabbTree<T> existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new DynamicAabbTree<T>();
      else
        existingInstance.Clear();

      // BasePartition<T>
      existingInstance.EnableSelfOverlaps = input.ReadBoolean();
      input.ReadSharedResource<IPairFilter<T>>(filter => existingInstance.Filter = filter);

      // DynamicAabbTree<T>
      existingInstance.BottomUpBuildThreshold = input.ReadInt32();
      existingInstance.EnableMotionPrediction = input.ReadBoolean();
      existingInstance.MotionPrediction = input.ReadSingle();
      existingInstance.OptimizationPerFrame = input.ReadSingle();
      existingInstance.RelativeMargin = input.ReadSingle();
      return existingInstance;
    }
  }
}
