// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Geometry.Content
{
  /// <summary>
  /// Reads a <see cref="CompressedAabbTree"/> from binary format.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class CompressedAabbTreeReader : ContentTypeReader<CompressedAabbTree>
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
    protected override CompressedAabbTree Read(ContentReader input, CompressedAabbTree existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new CompressedAabbTree();
      else
        existingInstance.Clear();

      bool isEmpty = input.ReadBoolean();
      if (isEmpty)
      {
        // ----- AABB tree is empty: Load only relevant properties.
        existingInstance.EnableSelfOverlaps = input.ReadBoolean();
        existingInstance.BottomUpBuildThreshold = input.ReadInt32();
        input.ReadSharedResource<IPairFilter<int>>(filter => existingInstance.Filter = filter);
      }
      else
      {
        // ----- AABB tree has content: Load internal structure.
        existingInstance._state = (CompressedAabbTree.State)input.ReadInt32();
        existingInstance._numberOfItems = input.ReadInt32();

        // _items should be null.
        if (existingInstance._items != null)
        {
          DigitalRune.ResourcePools<int>.Lists.Recycle(existingInstance._items);
          existingInstance._items = null;
        }

        // _nodes
        int numberOfNodes = input.ReadInt32();
        existingInstance._nodes = new CompressedAabbTree.Node[numberOfNodes];
        for (int i = 0; i < numberOfNodes; i++)
        {
          var node = new CompressedAabbTree.Node();
          node.MinimumX = input.ReadUInt16();
          node.MinimumY = input.ReadUInt16();
          node.MinimumZ = input.ReadUInt16();
          node.MaximumX = input.ReadUInt16();
          node.MaximumY = input.ReadUInt16();
          node.MaximumZ = input.ReadUInt16();
          node.EscapeOffsetOrItem = input.ReadInt32();
          existingInstance._nodes[i] = node;
        }

        existingInstance._aabb = input.ReadRawObject<Aabb>();
        existingInstance._quantizationFactor = input.ReadRawObject<Vector3F>();
        existingInstance._dequantizationFactor = input.ReadRawObject<Vector3F>();

        existingInstance.EnableSelfOverlaps = input.ReadBoolean();
        existingInstance.BottomUpBuildThreshold = input.ReadInt32();
        input.ReadSharedResource<IPairFilter<int>>(filter =>
                                                   {
                                                     if (existingInstance._filter != null)
                                                       existingInstance._filter.Changed -= existingInstance.OnFilterChanged;

                                                     existingInstance._filter = filter;

                                                     if (existingInstance._filter != null)
                                                       existingInstance._filter.Changed += existingInstance.OnFilterChanged;
                                                   });
      }

      return existingInstance;
    }
  }
}
