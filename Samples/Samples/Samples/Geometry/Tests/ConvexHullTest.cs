#if WINDOWS
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    @"",
    @"",
    1000)]
  public class ConvexHullTest : BasicSample
  {
    const int MaxFileIndex = 0; // Test files have been removed.

    private List<Vector3F> _points;
    private TriangleMesh _mesh;
    private GeometricObject _geometricObject;


    private int _fileIndex = int.MaxValue;
    private int _pointIndex = -1;
    private float _loadTime = float.NaN;
    private float _skinWidth = 0.01f;


    public ConvexHullTest(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3F(0, 1, 10), 0, 0);

      //var model = Game.Content.Load<Model>("Pedestal_crane_30t_braccio2");
      //var points = TriangleMesh.FromModel(model).Vertices;

      //var points = FileHelper.LoadPoints("..\\..\\..\\..\\..\\Testcases\\Mesh006.xml");

      System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
      //FileHelper.SavePoints(points, "..\\..\\..\\..\\..\\Testcases\\Mesh007.xml");

      //LoadFile();
    }


    private void LoadFile()
    {
      if (_fileIndex == MaxFileIndex)
      {
        // Instead of a file load procedural test data.

        //var model = Game.Content.Load<Model>("Dude");
        //_points = TriangleMesh.FromModel(model).Vertices;

        //RandomHelper.Random = new Random(_fileIndex);
        _points = new List<Vector3F>();
        for (int i = 0; i < 12000; i++)
        {
          _points.Add(new Vector3F(
            RandomHelper.Random.NextFloat(-10, -9),
            RandomHelper.Random.NextFloat(100, 101),
            RandomHelper.Random.NextFloat(-10.001f, -10)));
        }
      }
      else if (_fileIndex == MaxFileIndex - 1)
      {
        // Instead of a file load procedural test data.

        //var model = Game.Content.Load<Model>("Dude");
        //_points = TriangleMesh.FromModel(model).Vertices;

        //RandomHelper.Random = new Random(_fileIndex);
        _points = new List<Vector3F>();
        for (int i = 0; i < 12000; i++)
        {
          _points.Add(new Vector3F(
            RandomHelper.Random.NextFloat(-10, -9),
            RandomHelper.Random.NextFloat(100, 101),
            RandomHelper.Random.NextFloat(-10f, -10)));
        }
      }
      else if (_fileIndex < MaxFileIndex)
      {
        var path = string.Format("..\\..\\..\\..\\..\\Testcases\\Mesh{0:000}.xml", _fileIndex);
        _points = LoadPoints(path);
      }

      // Adjust camera position and speed.
      var aabb = new Aabb(_points[0], _points[0]);
      foreach (var point in _points)
        aabb.Grow(point);

      SetCamera(aabb.Center - aabb.Extent.Length * Vector3F.Forward * 2, 0, 0);
    }


    public override void Update(GameTime gameTime)
    {
      var lastFileIndex = _fileIndex;
      if (InputService.IsPressed(Keys.NumPad7, true))
        _fileIndex++;
      if (InputService.IsPressed(Keys.NumPad4, true))
        _fileIndex--;
      if (_fileIndex < 0)
        _fileIndex = MaxFileIndex;
      if (_fileIndex > MaxFileIndex)
        _fileIndex = 0;
      if (lastFileIndex != _fileIndex)
      {
        LoadFile();
        _pointIndex = -1;
      }

      var lastPointIndex = _pointIndex;
      if (InputService.IsPressed(Keys.Right, true))
        _pointIndex++;
      if (InputService.IsPressed(Keys.Left, true))
        _pointIndex--;
      if (_pointIndex <= 0)
        _pointIndex = _points.Count;
      if (_pointIndex > _points.Count)
        _pointIndex = 1;

      var lastSkinWidth = _skinWidth;
      if (InputService.IsDown(Keys.Up))
        _skinWidth *= 1.1f;
      if (InputService.IsDown(Keys.Down))
        _skinWidth /= 1.1f;

      if (lastPointIndex != _pointIndex || _skinWidth != lastSkinWidth)
      {
        var watch = Stopwatch.StartNew();
        DcelMesh hull = GeometryHelper.CreateConvexHull(_points.Take(_pointIndex), int.MaxValue, _skinWidth);
        watch.Stop();
        _loadTime = (float)watch.Elapsed.TotalMilliseconds;
        _mesh = hull.ToTriangleMesh();
        _geometricObject = new GeometricObject(new TriangleMeshShape(_mesh));
      }

      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      debugRenderer.DrawAxes(Pose.Identity, 1, false);
      debugRenderer.DrawObject(_geometricObject, new Color(0.5f, 0.5f, 0.5f, 0.8f), false, false);
      debugRenderer.DrawObject(_geometricObject, Color.Black, true, true);
      foreach (var point in _points)
        debugRenderer.DrawPoint(point, Color.White, false);

      debugRenderer.DrawText(
        "\n\nFile: " + _fileIndex + "\n" +
        "Point Index: " + _pointIndex + "\n" +
        "Time: " + _loadTime + "\n");
    }


    public static List<Vector3F> LoadPoints(string file)
    {
      List<Vector3F> points = new List<Vector3F>();
      var doc = XDocument.Load(file);
      var root = doc.Elements().First();
      foreach (var e in root.Elements())
        points.Add(Vector3F.Parse(e.Value));
      return points;
    }


    public static void SavePoints(IEnumerable<Vector3F> points, string file)
    {
      var doc = new XDocument();
      var root = new XElement("Vertices");
      doc.Add(root);
      foreach (var p in points)
        root.Add(new XElement("Vertex", p.ToString()));
      doc.Save(file);
    }
  }
}
#endif