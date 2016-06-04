// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Geometry.Content
{
  /// <summary>
  /// Reads a <see cref="CompositeShape"/> from binary format.
  /// </summary>
  public class CompositeShapeReader : ContentTypeReader<CompositeShape>
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
    protected override CompositeShape Read(ContentReader input, CompositeShape existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new CompositeShape();
      else
        existingInstance.Children.Clear();

      int numberOfChildren = input.ReadInt32();
      var partition = input.ReadObject<ISpatialPartition<int>>();

      // Store the children (shared resources) in a temporary array.
      // Assign the children and the spatial partition to the composite shape
      // after all children are loaded. (This is necessary to ensure the correct order 
      // of the children.)
      var children = new GeometricObject[numberOfChildren];
      int count = 0;
      for (int i = 0; i < numberOfChildren; i++)
      {
        int index = i;

        input.ReadSharedResource<GeometricObject>(geometry =>
                                                  {
                                                    children[index] = geometry;

                                                    // ReSharper disable AccessToModifiedClosure
                                                    count++;
                                                    // ReSharper restore AccessToModifiedClosure

                                                    if (count == numberOfChildren)
                                                    {
                                                      existingInstance.Children.AddRange(children);
                                                      existingInstance.SetPartition(partition);
                                                    }
                                                  });
      }

      return existingInstance;
    }
  }
}
