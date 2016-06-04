using System.Linq;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Content.Pipeline;
using DigitalRune.Linq;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace Samples.Content.Pipeline
{
  /// <summary>
  /// Processes a game asset mesh to a ModelNode that is optimal for runtime. Additionally, a 
  /// <see cref="TriangleMeshShape"/> that can be used for collision detection is created and stored
  /// in the ModelNode.UserData.
  /// </summary>
  /// <remarks>
  /// This content processor extends the DigitalRune DRModelProcessor. It creates a 
  /// TriangleMeshShape that can be used for collision detection. Duplicate vertices are removed. 
  /// Contact welding is applied to improve the results of the collision detection. A 
  /// CompressedAabbTree is assigned to improve performance at runtime. The TriangleMeshShape is 
  /// stored in the UserData property of the model node.
  /// </remarks>
  [ContentProcessor(DisplayName = "DigitalRune Model with Collision Mesh")]
  public class ModelWithCollisionMeshProcessor : DRModelProcessor
  {
    /// <summary>
    /// Converts mesh content to model content with a <see cref="TriangleMeshShape"/>.
    /// </summary>
    /// <param name="input">The root node content.</param>
    /// <param name="context">Context for the specified processor.</param>
    /// <returns>The <see cref="Shape"/>.</returns>
    public override DRModelNodeContent Process(NodeContent input, ContentProcessorContext context)
    {
      // ----- Process Model
      var model = base.Process(input, context);

      // ----- Extract Triangles
      var triangleMesh = new TriangleMesh();

      // The input node is usually a tree of nodes. We need to collect all MeshContent nodes
      // in the tree. The DigitalRune Helper library provides a TreeHelper that can be used 
      // to traverse trees using LINQ.
      // The following returns an IEnumerable that returns all nodes of the tree.
      var nodes = TreeHelper.GetSubtree(input, n => n.Children);

      // We only need nodes of type MeshContent.
      var meshes = nodes.OfType<MeshContent>();

      foreach (var mesh in meshes)
      {
        // Apply any transformations to vertices.
        Matrix transform = mesh.AbsoluteTransform;
        for (int i = 0; i < mesh.Positions.Count; i++)
          mesh.Positions[i] = Vector3.Transform(mesh.Positions[i], transform);

        // Extract triangles from submeshes.
        foreach (var geometry in mesh.Geometry)
        {
          int numberOfTriangles = geometry.Indices.Count / 3;
          for (int i = 0; i < numberOfTriangles; i++)
          {
            int index0 = geometry.Indices[3 * i + 0];
            int index1 = geometry.Indices[3 * i + 2]; // Note: DigitalRune Geometry uses a different winding
            int index2 = geometry.Indices[3 * i + 1]; // order. Therefore, the indices need to be swapped.

            Vector3F vertex0 = (Vector3F)geometry.Vertices.Positions[index0];
            Vector3F vertex1 = (Vector3F)geometry.Vertices.Positions[index1];
            Vector3F vertex2 = (Vector3F)geometry.Vertices.Positions[index2];

            triangleMesh.Add(new Triangle(vertex0, vertex1, vertex2), false, Numeric.EpsilonF, true);
          }
        }
      }

      // Remove duplicate vertices.
      triangleMesh.WeldVertices();

      // ----- Create TriangleMeshShape
      // Create a TriangleMeshShape that can be used for collision detection.
      // Note: 
      // - Contact-welding is enabled to improve the results of the collision detection.
      // - A CompressedAabbTree is used for spatial partitioning to speed up collision detection.
      var triangleMeshShape = new TriangleMeshShape(triangleMesh, true, new CompressedAabbTree());

      // Export the ModelNode together with the TriangleMeshShape.
      model.UserData = triangleMeshShape;
      return model;
    }
  }
}