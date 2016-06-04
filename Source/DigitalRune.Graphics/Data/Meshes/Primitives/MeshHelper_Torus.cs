#region ----- Copyright -----
/*
   Torus creation algorithm is taken from the Primitives3D sample, which is 
   licensed under the Microsoft Public License (MS-PL).
   See http://create.msdn.com/en-US/education/catalog/sample/primitives_3d
*/
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  partial class MeshHelper
  {
    /// <summary>
    /// Creates a new submesh that represents a torus using triangles.
    /// (The torus is centered at the origin.)
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="radius">The radius of the torus.</param>
    /// <param name="thickness">The thickness of the torus.</param>
    /// <param name="numberOfSegments">
    /// The number of segments. This parameter controls the detail of the mesh.
    /// </param>
    /// <returns>A new <see cref="Submesh"/> that represents a torus.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfSegments"/> is less than or equal to 2.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateTorus(GraphicsDevice graphicsDevice, float radius, float thickness, int numberOfSegments)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (numberOfSegments < 3)
        throw new ArgumentOutOfRangeException("numberOfSegments", "numberOfSegments must be greater than 2");

      var submesh = new Submesh
      {
        PrimitiveType = PrimitiveType.TriangleList,
      };

      var vertices = new List<VertexPositionNormal>();
      var indices = new List<ushort>();

      for (int i = 0; i < numberOfSegments; i++)
      {
        float outerAngle = i * MathHelper.TwoPi / numberOfSegments;

        // Create a transformation that will align geometry to slice perpendicularly 
        // though the current ring position.
        Matrix transform = Matrix.CreateTranslation(radius, 0, 0) * Matrix.CreateRotationY(outerAngle);

        // Now loop along the other axis, around the side of the tube.
        for (int j = 0; j < numberOfSegments; j++)
        {
          float innerAngle = j * MathHelper.TwoPi / numberOfSegments;

          float dx = (float)Math.Cos(innerAngle);
          float dy = (float)Math.Sin(innerAngle);

          // Create a vertex.
          Vector3 normal = new Vector3(dx, dy, 0);
          Vector3 position = normal * thickness / 2.0f;

          position = Vector3.Transform(position, transform);
          normal = Vector3.TransformNormal(normal, transform);

          vertices.Add(new VertexPositionNormal(position, normal));

          // And create indices for two triangles.
          int nextI = (i + 1) % numberOfSegments;
          int nextJ = (j + 1) % numberOfSegments;

          indices.Add((ushort)(i * numberOfSegments + j));
          indices.Add((ushort)(i * numberOfSegments + nextJ));
          indices.Add((ushort)(nextI * numberOfSegments + j));

          indices.Add((ushort)(i * numberOfSegments + nextJ));
          indices.Add((ushort)(nextI * numberOfSegments + nextJ));
          indices.Add((ushort)(nextI * numberOfSegments + j));
        }
      }

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        VertexPositionNormal.VertexDeclaration,
        vertices.Count,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(vertices.ToArray());

      submesh.VertexCount = submesh.VertexBuffer.VertexCount;

      submesh.IndexBuffer = new IndexBuffer(
        graphicsDevice,
        IndexElementSize.SixteenBits,
        indices.Count,
        BufferUsage.None);
      submesh.IndexBuffer.SetData(indices.ToArray());

      submesh.PrimitiveCount = indices.Count / 3;

      return submesh;
    }
  }
}
