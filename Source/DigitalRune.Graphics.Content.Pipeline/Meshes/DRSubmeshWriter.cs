// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="DRSubmeshContent"/> to binary format that can be read by the
  /// <strong>SubmeshReader</strong> to load a <strong>Submesh</strong>.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  [ContentTypeWriter]
  public class DRSubmeshWriter : ContentTypeWriter<DRSubmeshContent>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return "DigitalRune.Graphics.Submesh, DigitalRune.Graphics, Version=1.2.0.0";
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return "DigitalRune.Graphics.Content.SubmeshReader, DigitalRune.Graphics, Version=1.2.0.0";
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void Write(ContentWriter output, DRSubmeshContent value)
    {
      output.WriteSharedResource(value.VertexBuffer);
      output.Write(value.StartVertex);
      output.Write(value.VertexCount);

      output.WriteSharedResource(value.IndexBuffer);
      output.Write(value.StartIndex);
      output.Write(value.PrimitiveCount);

      if (value.MorphTargets != null)
      {
        output.Write(value.MorphTargets.Count);
        foreach (var morphTarget in value.MorphTargets)
          output.WriteObject(morphTarget);
      }
      else
      {
        output.Write(0);
      }

      if (value.ExternalMaterial != null)
      {
        // Submesh uses external material.
        output.Write(true);
        output.WriteExternalReference(value.ExternalMaterial);
      }
      else
      {
        // Submesh uses local material.
        output.Write(false);
        output.WriteSharedResource(value.LocalMaterial);
      }

      output.WriteSharedResource(value.UserData);
    }
  }
}
