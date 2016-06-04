using System.Diagnostics;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    "This samples shows how to triangulate a 2D mesh.",
    "Add vertices using left mouse button.",
    9)]
  [Controls(@"Sample
  Click with left mouse button to add vertex.")]
  public class TriangulationSample : BasicSample
  {
    private readonly TriangleMesh _mesh;


    public TriangulationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;
      _mesh = new TriangleMesh();
    }


    public override void Update(GameTime gameTime)
    {
      // Left mouse button --> Add vertex.
      if (!InputService.IsMouseOrTouchHandled && InputService.IsPressed(MouseButtons.Left, false))
      {
        // Add vertex. 
        Vector2F position = InputService.MousePosition;
        _mesh.Vertices.Add(new Vector3F(position.X, position.Y, 0));

        // Use GeometryHelper to compute indices of triangulated polygon.
        _mesh.Indices.Clear();
        int numberOfTriangles = GeometryHelper.Triangulate(_mesh.Vertices, _mesh.Indices);

        Debug.WriteLine("Number of triangles: " + numberOfTriangles);

        var debugRenderer = GraphicsScreen.DebugRenderer2D;
        debugRenderer.Clear();
        for (int i = 0; i < _mesh.Vertices.Count; i++)
          debugRenderer.DrawPoint(_mesh.Vertices[i], Color.Black, true);

        for (int i = 0; i < _mesh.Vertices.Count - 1; i++)
          debugRenderer.DrawLine(_mesh.Vertices[i], _mesh.Vertices[i + 1], Color.Black, false);

        if (numberOfTriangles > 0)
        {
          // Draw fill triangles.
          debugRenderer.DrawTriangles(_mesh, Pose.Identity, Vector3F.One, new Color(1.0f, 0.0f, 0.0f, 0.25f), false, false);

          // Draw wireframe.
          debugRenderer.DrawTriangles(_mesh, Pose.Identity, Vector3F.One, new Color(1.0f, 0.0f, 0.0f, 1.0f), true, false);
        }
      }
    }
  }
}
