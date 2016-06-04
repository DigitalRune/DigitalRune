// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Geometry.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="CompressedAabbTree"/> to binary format.
  /// </summary>
  [ContentTypeWriter]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class CompressedAabbTreeWriter : ContentTypeWriter<CompressedAabbTree>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(CompressedAabbTree).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(CompressedAabbTreeReader).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void Write(ContentWriter output, CompressedAabbTree value)
    {
      bool isEmpty = (value.Count == 0);
      output.Write(isEmpty);

      if (isEmpty)
      {
        // ----- AABB tree is empty: Serialize only relevant properties.
        output.Write(value.EnableSelfOverlaps);
        output.Write(value.BottomUpBuildThreshold);
        output.WriteSharedResource(value.Filter);
      }
      else
      {
        // ----- AABB tree has content: Serialize internal structure.

        // Ensure that spatial partition is up-to-date.
        value.Update(false);

        dynamic internals = value.Internals;
        int state = internals.State;
        int numberOfItems = internals.NumberOfItems;
        object items = internals.Items;
        int numberOfNodes = internals.NumberOfNodes;
        int[] data = internals.Data;
        Vector3F quantizationFactor = internals.QuantizationFactor;
        Vector3F dequantizationFactor = internals.DequantizationFactor;

        output.Write(state);
        output.Write(numberOfItems);

        // _items is null because tree is up-to-date.
        Debug.Assert(items == null);

        // _nodes
        output.Write(numberOfNodes);
        for (int i = 0; i < numberOfNodes; i++)
        {
          output.Write((ushort)data[i * 7 + 0]);  // MinimumX
          output.Write((ushort)data[i * 7 + 1]);  // MinimumY
          output.Write((ushort)data[i * 7 + 2]);  // MinimumZ
          output.Write((ushort)data[i * 7 + 3]);  // MaximumX
          output.Write((ushort)data[i * 7 + 4]);  // MaximumY
          output.Write((ushort)data[i * 7 + 5]);  // MaximumZ
          output.Write((int)data[i * 7 + 6]);     // EscapeOffsetOrItem
        }

        output.WriteRawObject(value.Aabb);
        output.WriteRawObject(quantizationFactor);
        output.WriteRawObject(dequantizationFactor);

        output.Write(value.EnableSelfOverlaps);
        output.Write(value.BottomUpBuildThreshold);
        output.WriteSharedResource(value.Filter);
      }
    }
  }
}
