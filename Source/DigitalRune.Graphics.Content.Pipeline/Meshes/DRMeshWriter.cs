// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="DRMeshContent"/> to binary format that can be read by the
  /// <strong>MeshReader</strong> to load a <strong>Mesh</strong>
  /// </summary>
  [ContentTypeWriter]
  public class DRMeshWriter : ContentTypeWriter<DRMeshContent>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return "DigitalRune.Graphics.Mesh, DigitalRune.Graphics, Version=1.2.0.0";
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return "DigitalRune.Graphics.Content.MeshReader, DigitalRune.Graphics, Version=1.2.0.0";
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void Write(ContentWriter output, DRMeshContent value)
    {
      output.WriteObject(value.BoundingShape);

      if (value.Submeshes != null)
      {
        output.Write(value.Submeshes.Count);
        foreach (var submesh in value.Submeshes)
          output.WriteObject(submesh);
      }
      else
      {
        output.Write(0);
      }

      output.Write(value.Name ?? string.Empty);

      bool hasOccluder = (value.Occluder != null);
      output.Write(hasOccluder);
      if (hasOccluder)
        output.WriteSharedResource(value.Occluder);

#if ANIMATION
      bool hasSkeleton = (value.Skeleton != null);
      output.Write(hasSkeleton);
      if (hasSkeleton)
        output.WriteSharedResource(value.Skeleton);

      var hasAnimations = (value.Animations != null && value.Animations.Count > 0);
      output.Write(hasAnimations);
      if (hasAnimations)
        output.WriteSharedResource(value.Animations);
#else
      output.Write(false);  // hasSkeleton = false
      output.Write(false);  // hasAnimations = false
#endif

      output.WriteSharedResource(value.UserData);
    }
  }
}
