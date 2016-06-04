using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace Samples.Content.Pipeline
{
  /// <summary>
  /// Takes an imported 3D model and creates a <see cref="Shape"/> that can be used for collision 
  /// detection.
  /// </summary>
  /// <remarks>
  /// The imported model may contain boxes, spheres and convex meshes that define the collision 
  /// shape. Following naming convention is used:
  /// - The mesh of a box ends with "Box". 
  /// - The mesh of a sphere ends with "Sphere". 
  /// - The mesh of a convex mesh ends with "Convex".
  /// If the 3D model contains several collision shapes they are combined into one CompositeShape.
  /// </remarks>
  [ContentProcessor(DisplayName = "Collision Shape")]
  public class CollisionShapeProcessor : ContentProcessor<NodeContent, Shape>
  {
    /// <summary>
    /// Gets or sets the scaling factor applied to the model.
    /// </summary>
    /// <value>The scaling factor that is applied to the model.</value>
    [DefaultValue(1f)]
    [DisplayName("Scale")]
    [Description("Scales the model uniformly along all three axes.")]
    public virtual float Scale
    {
      get { return _scale; }
      set { _scale = value; }
    }
    private float _scale = 1f;


    /// <summary>
    /// Converts mesh content to a <see cref="Shape"/>.
    /// </summary>
    /// <param name="input">The root node content.</param>
    /// <param name="context">Context for the specified processor.</param>
    /// <returns>The <see cref="Shape"/>.</returns>
    public override Shape Process(NodeContent input, ContentProcessorContext context)
    {
      // ----- Apply Scale factor.
      if (Scale != 1f)
      {
        // The user has set a scale. Use MeshHelper to apply the scale to the whole model.
        Matrix transform = Matrix.CreateScale(Scale);
        MeshHelper.TransformScene(input, transform);
      }

      // ----- Convert Mesh to Shapes
      // The input node is usually a tree of nodes. We need to collect all MeshContent nodes
      // in the tree. The DigitalRune Helper library provides a TreeHelper that can be used 
      // to traverse trees using LINQ.
      // The following returns an IEnumerable that contains all nodes of the tree.
      IEnumerable<NodeContent> nodes = TreeHelper.GetSubtree(input, n => n.Children);

      // We only need nodes of type MeshContent.
      IEnumerable<MeshContent> meshes = nodes.OfType<MeshContent>();

      // For each MeshContent we extract one shape and its pose (position and orientation).
      List<Pose> poses = new List<Pose>();
      List<Shape> shapes = new List<Shape>();

      foreach (var mesh in meshes)
      {
        if (mesh.Positions.Count == 0)
          continue;

        Pose pose = Pose.Identity;
        Shape shape = null;

        // The meshes in the imported file must follow a naming convention. The end of the name
        // of each mesh must be "Box", "Sphere" or "Convex" to tell us which kind of collision
        // shape we must create.
        if (mesh.Name.EndsWith("Box"))
          LoadBox(mesh, out pose, out shape);
        else if (mesh.Name.EndsWith("Sphere"))
          LoadSphere(mesh, out pose, out shape);
        else if (mesh.Name.EndsWith("Convex"))
          LoadConvex(mesh, out pose, out shape);

        if (shape != null)
        {
          poses.Add(pose);
          shapes.Add(shape);
        }
      }

      // The CollisionShapeProcessor exports a single shape.
      Shape collisionShape;

      if (shapes.Count == 0)
      {
        // We did not find any collision shapes. --> Return a dummy shape.
        collisionShape = Shape.Empty;
      }
      else if (shapes.Count == 1)
      {
        // We have found 1 shape.
        if (poses[0].HasRotation || poses[0].HasTranslation)
        {
          // The shape is not centered in origin of the model space or it is rotated, 
          // therefore we create a TransformedShape that applies the transformation.
          collisionShape = new TransformedShape(new GeometricObject(shapes[0], poses[0]));
        }
        else
        {
          // Use the shape directly, there is no translation or rotation we have to apply.
          collisionShape = shapes[0];
        }
      }
      else
      {
        // We have found several collision shapes. --> Combine all shapes into one CompositeShape.
        CompositeShape compositeShape = new CompositeShape();
        for (int i = 0; i < shapes.Count; i++)
          compositeShape.Children.Add(new GeometricObject(shapes[i], poses[i]));

        // If the composite shape has many children, the performance is improved if the composite
        // shape uses a spatial partition. 
        //compositeShape.Partition = new CompressedAabbTree();

        collisionShape = compositeShape;
      }

      return collisionShape;
    }


    // Extracts a box shape from the given MeshContent.
    private void LoadBox(MeshContent mesh, out Pose pose, out Shape shape)
    {
      // Get transformation of the node.
      pose = GetPose(mesh);

      // The naming convention told us that the mesh describes a box. We need to extract
      // the box extents (width, height, depth). The mesh will probably have 8 vertices that 
      // define the box. We use the Aabb struct to find the extents for us.

      // Create an AABB that contains the first vertex.
      Aabb aabb = new Aabb((Vector3F)mesh.Positions[0], (Vector3F)mesh.Positions[0]);

      // Extend the AABB to include the other 7 vertices.
      for (int i = 1; i < mesh.Positions.Count; i++)
        aabb.Grow((Vector3F)mesh.Positions[i]);

      // If the box is not centered on the node, add a translation to the pose.
      pose = pose * new Pose(aabb.Center);

      // Now, aabb has the box size. 
      // We return a BoxShape with the same size. (Note: Aabb is only a helper structure it is
      // not itself derived from class Shape.)
      shape = new BoxShape(aabb.Extent);
    }


    // Extracts a sphere shape from the given MeshContent.
    // Note: We expect that the sphere is centered on the node!
    private void LoadSphere(MeshContent mesh, out Pose pose, out Shape shape)
    {
      // Get transformation of the node.
      pose = GetPose(mesh);

      // Rotating a sphere does not change its shape, so we can eliminate the Orientation 
      // part of the pose.
      pose = new Pose(pose.Position);

      // We need to extract the radius of the sphere. This is simple: The distance of each 
      // vertex to the node's origin is equal to the radius of the sphere.
      float radius = mesh.Positions[0].Length();
      shape = new SphereShape(radius);
    }


    // Creates a convex shape for the given MeshContent.
    private void LoadConvex(MeshContent mesh, out Pose pose, out Shape shape)
    {
      // Apply node's transformation to all vertices.
      pose = Pose.Identity;
      Matrix transform = mesh.AbsoluteTransform;
      for (int i = 0; i < mesh.Positions.Count; i++)
        mesh.Positions[i] = Vector3.Transform(mesh.Positions[i], transform);

      // Convert the vertices from Microsoft.Xna.Framework.Vector3 to 
      // DigitalRune.Mathematics.Algebra.Vector3F.
      IEnumerable<Vector3F> vertices = mesh.Positions.Select(pos => (Vector3F)pos);

      // Return a ConvexPolyhedron (convex hull) consisting of the mesh vertices.
      shape = new ConvexPolyhedron(vertices);
    }


    // Extracts the Pose (= position + orientation) of a MeshContent node.
    // If the node contains a scaling, the scaling is directly applied to all vertices.
    private Pose GetPose(MeshContent mesh)
    {
      Matrix transform = mesh.AbsoluteTransform;

      Vector3 scale;
      Vector3 translation;
      Quaternion rotation;
      transform.Decompose(out scale, out rotation, out translation);

      if (scale != Vector3.One)
      {
        // Apply scale to vertex positions.
        Matrix scaling = Matrix.CreateScale(scale);
        for (int i = 0; i < mesh.Positions.Count; i++)
          mesh.Positions[i] = Vector3.Transform(mesh.Positions[i], scaling);
      }

      return new Pose((Vector3F)translation, (QuaternionF)rotation);
    }
  }
}