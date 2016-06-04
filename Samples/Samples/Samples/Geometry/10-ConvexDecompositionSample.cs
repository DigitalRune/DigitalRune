#if WINDOWS
using System.ComponentModel;
using System.Linq;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
  @"This sample shows how to create an approximate convex decomposition of a complex mesh.",
  @"The ConvexDecomposition class computes a set of convex hulls which approximate the given
mesh. This can be used to create convex collision shapes for concave triangle mesh.
You need to experiment with the decomposition parameter to get good results.
The decomposition process can take a while (several seconds or even minutes). 
At regular intervals, the intermediate decomposition is updated and rendered to show the progress.",
  100)]
  public class ConvexDecompositionSample : BasicSample
  {
    // The model which will be decomposed.
    private readonly ModelNode _modelNode;

    // The class which computes the convex shapes.
    private readonly ConvexDecomposition _convexDecomposition;

    // The resulting composite shape consisting of convex child shapes.
    private CompositeShape _decomposition;

    private int _oldProgress;
    private int _newProgress;


    public ConvexDecompositionSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3F(3, 3, 3), 0.8f, -0.6f);

      // Load model.
      _modelNode = ContentManager.Load<ModelNode>("Saucer/Saucer").Clone();

      // Combine all meshes of the model into a single TriangleMesh.
      TriangleMesh mesh = new TriangleMesh();
      foreach (var meshNode in _modelNode.GetChildren().OfType<MeshNode>())
      {
        var childMesh = MeshHelper.ToTriangleMesh(meshNode.Mesh);
        childMesh.Transform(meshNode.PoseWorld * Matrix44F.CreateScale(meshNode.ScaleWorld));
        mesh.Add(childMesh);
      }

      // Start convex decomposition on another thread.
      _convexDecomposition = new ConvexDecomposition();
      _convexDecomposition.ProgressChanged += OnProgressChanged;
      _convexDecomposition.AllowedConcavity = 0.8f;
      _convexDecomposition.IntermediateVertexLimit = 65536;
      _convexDecomposition.VertexLimit = 64;

      // 0 gives optimal results but is the slowest. Small positive values improve
      // speed but the result might be less optimal.
      _convexDecomposition.SmallIslandBoost = 0.02f;

      _convexDecomposition.SampleTriangleCenters = true;
      _convexDecomposition.SampleTriangleVertices = true;

      // Experimental multithreading. Enable at own risk ;-)
      _convexDecomposition.EnableMultithreading = true;

      _convexDecomposition.DecomposeAsync(mesh);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Abort decomposition.
        _convexDecomposition.CancelAsync();

        _modelNode.Dispose(false);
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      // If progress has changed significantly, get current decomposition.
      if (_newProgress - _oldProgress >= 10
          || _newProgress > 90 && _newProgress - _oldProgress > 1)
      {
        _decomposition = _convexDecomposition.Decomposition;
        _oldProgress = _newProgress;
      }

      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // Draw a wireframe of the model.
      debugRenderer.DrawModel(_modelNode, Color.White, true, false);

      // Show current progress.
      debugRenderer.DrawText("\n\nProgress: " + _newProgress + " %");

      // Draw the current convex hulls.
      if (_decomposition != null)
      {
        foreach (var childGeometry in _decomposition.Children)
          debugRenderer.DrawObject(childGeometry, GraphicsHelper.GetUniqueColor(childGeometry), false, false);

        debugRenderer.DrawText("Number of convex parts: " + _decomposition.Children.Count);
      }
    }


    private void OnProgressChanged(object sender, ProgressChangedEventArgs eventArgs)
    {
      _newProgress = eventArgs.ProgressPercentage;
    }
  }
}
#endif