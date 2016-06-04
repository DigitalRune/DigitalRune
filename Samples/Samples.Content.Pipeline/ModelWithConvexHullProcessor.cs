using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Content.Pipeline;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace Samples.Content.Pipeline
{
  /// <summary>
  /// Processes a model and adds a convex polyhedron for collision detection.
  /// </summary>
  /// <remarks>
  /// This content processor extends the DigitalRune DRModelProcessor. It creates a ConvexPolyhedron
  /// shape from the model vertices. The ConvexPolyhedron represents the convex hull of the model, 
  /// which can be used for collision detection. The shape and is stored in ModelNode.UserData.
  /// </remarks>
  [ContentProcessor(DisplayName = "DigitalRune Model with Convex Hull")]
  public class ModelWithConvexHullProcessor : DRModelProcessor
  {
    private readonly List<Vector3F> _vertices = new List<Vector3F>();


    /// <summary>
    /// Processes the specified input.
    /// </summary>
    /// <param name="input">The existing content object being processed.</param>
    /// <param name="context">Contains any required custom process parameters.</param>
    /// <returns>A typed object representing the processed input.</returns>
    public override DRModelNodeContent Process(NodeContent input, ContentProcessorContext context)
    {
      // Call the DigitalRune model processor (base class).
      var model = base.Process(input, context);

      // Create collision shape and store it in the ModelNode.UserData.
      _vertices.Clear();
      GetVertices(input);
      model.UserData = new ConvexPolyhedron(_vertices);

      return model;
    }


    /// <summary>
    /// Gets all vertices of a model.
    /// </summary>
    /// <param name="node">The node with the model.</param>
    private void GetVertices(NodeContent node)
    {
      var mesh = node as MeshContent;
      if (mesh != null)
      {
        // Extract vertices from mesh.
        Matrix absoluteTransform = mesh.AbsoluteTransform;
        foreach (Vector3 vertex in mesh.Positions)
        {
          _vertices.Add((Vector3F)Vector3.Transform(vertex, absoluteTransform));
        }
      }

      // Recursively scan over the children of this node.
      foreach (NodeContent child in node.Children)
      {
        GetVertices(child);
      }
    }
  }
}