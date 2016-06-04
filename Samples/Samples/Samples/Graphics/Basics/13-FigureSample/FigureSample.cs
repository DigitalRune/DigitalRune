using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = Microsoft.Xna.Framework.MathHelper;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples shows how to render lines and 2d shapes using FigureNodes.",
    @"Collision detection is implemented to detect if a figure is near the reticle. Figures near 
the reticle are drawn in a different color.

Note: Alpha-Blending and Sorting
The figures are rendered back-to-front for correct alpha blending. The sorting is done per 
object using the scene node origins. Therefore, wrong alpha blending is still possible. This 
can be observed between the big grid and the smaller transparent figures.",
    13)]
  public class FigureSample : Sample
  {
    private readonly CameraObject _cameraObject;
    private readonly Scene _scene;
    private readonly FigureRenderer _figureRenderer;
    private readonly SpriteRenderer _spriteRenderer;
    private readonly DebugRenderer _debugRenderer;


    public FigureSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      var delegateGraphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, delegateGraphicsScreen);

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      GameObjectService.Objects.Add(_cameraObject);

      var spriteFont = UIContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");

      _figureRenderer = new FigureRenderer(GraphicsService, 2048);
      _spriteRenderer = new SpriteRenderer(GraphicsService, spriteFont);
      _debugRenderer = new DebugRenderer(GraphicsService, spriteFont)
      {
        DefaultColor = Color.Black,
        DefaultTextPosition = new Vector2F(20, 40)
      };

      _scene = new Scene();

      // To draw figures, they are flattened (= converted to line segments) 
      // internally. Figure.Tolerance defines the allowed error between the 
      // smooth and the flattened curve.
      Figure.Tolerance = 0.0001f;

      // Add some FigureNodes to the scene.
      CreateGrid();
      CreateGridClone();
      CreateRandomPath();
      CreateRectangles();
      CreateEllipses();
      CreateAlphaBlendedFigures();
      CreateChain();
      CreateGizmo(spriteFont);
      CreateFlower();

      // Add a game object which handles the picking:
      GameObjectService.Objects.Add(new FigurePickerObject(GraphicsService, _scene, _cameraObject, _debugRenderer));
    }


    // Add some rectangles.
    private void CreateRectangles()
    {
      Figure figure = new RectangleFigure
      {
        IsFilled = false,
        WidthX = 1f,
        WidthY = 0.5f,
      };
      FigureNode figureNode = new FigureNode(figure)
      {
        Name = "Rectangle #1",
        StrokeThickness = 1,
        StrokeColor = new Vector3F(0.7f, 0.3f, 0.5f),
        StrokeAlpha = 1,
        PoseLocal = new Pose(new Vector3F(-2, 1, 0))
      };
      _scene.Children.Add(figureNode);

      figure = new RectangleFigure
      {
        IsFilled = false,
        WidthX = 0.5f,
        WidthY = 0.8f,
      };
      figureNode = new FigureNode(figure)
      {
        Name = "Rectangle #2",
        StrokeThickness = 3,
        StrokeColor = new Vector3F(0.2f, 0.3f, 0.3f),
        StrokeAlpha = 0.5f,
        StrokeDashPattern = new Vector4F(10, 2, 3, 2),
        DashInWorldSpace = false,
        PoseLocal = new Pose(new Vector3F(-1, 1, 0))
      };
      _scene.Children.Add(figureNode);

      figure = new RectangleFigure
      {
        IsFilled = true,
        WidthX = 0.6f,
        WidthY = 0.7f,
      };
      figureNode = new FigureNode(figure)
      {
        Name = "Rectangle #3",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0.3f, 0, 0.2f),
        StrokeAlpha = 1,
        StrokeDashPattern = new Vector4F(10, 2, 3, 2) / 100,
        DashInWorldSpace = true,
        FillColor = new Vector3F(0.7f, 0, 0.5f),
        FillAlpha = 0.5f,
        PoseLocal = new Pose(new Vector3F(-0, 1, 0))
      };
      _scene.Children.Add(figureNode);

      figure = new RectangleFigure
      {
        IsFilled = true,
        WidthX = 1f,
        WidthY = 0.2f,
      };
      figureNode = new FigureNode(figure)
      {
        Name = "Rectangle #4",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0, 0, 0),
        StrokeAlpha = 1,
        StrokeDashPattern = new Vector4F(1, 1, 1, 1) / 100,
        DashInWorldSpace = true,
        FillColor = new Vector3F(0.3f, 0.3f, 0.3f),
        FillAlpha = 0.5f,
        PoseLocal = new Pose(new Vector3F(1, 1, 0))
      };
      _scene.Children.Add(figureNode);

      figure = new RectangleFigure
      {
        IsFilled = true,
        WidthX = 0.4f,
        WidthY = 0.5f,
      };
      figureNode = new FigureNode(figure)
      {
        Name = "Rectangle #5",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0.3f),
        StrokeAlpha = 1,
        FillColor = new Vector3F(0.3f),
        FillAlpha = 1,
        PoseLocal = new Pose(new Vector3F(2, 1, 0))
      };
      _scene.Children.Add(figureNode);
    }


    // Add some ellipses.
    private void CreateEllipses()
    {
      Figure figure = new EllipseFigure
      {
        IsFilled = false,
        RadiusX = 0.5f,
        RadiusY = 0.25f,
      };
      FigureNode figureNode = new FigureNode(figure)
      {
        Name = "Ellipse #1",
        StrokeThickness = 1,
        StrokeColor = new Vector3F(0.7f, 0.3f, 0.5f),
        StrokeAlpha = 1,
        PoseLocal = new Pose(new Vector3F(-2, 2, 0))
      };
      _scene.Children.Add(figureNode);

      figure = new EllipseFigure
      {
        IsFilled = false,
        RadiusX = 0.25f,
        RadiusY = 0.4f,
      };
      figureNode = new FigureNode(figure)
      {
        Name = "Ellipse #2",
        StrokeThickness = 3,
        StrokeColor = new Vector3F(0.2f, 0.3f, 0.3f),
        StrokeAlpha = 0.5f,
        StrokeDashPattern = new Vector4F(10, 2, 3, 2),
        DashInWorldSpace = false,
        PoseLocal = new Pose(new Vector3F(-1, 2, 0))
      };
      _scene.Children.Add(figureNode);

      figure = new EllipseFigure
      {
        IsFilled = true,
        RadiusX = 0.3f,
        RadiusY = 0.35f,
      };
      figureNode = new FigureNode(figure)
      {
        Name = "Ellipse #3",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0.3f, 0, 0.2f),
        StrokeAlpha = 1,
        StrokeDashPattern = new Vector4F(10, 2, 3, 2) / 100,
        DashInWorldSpace = true,
        FillColor = new Vector3F(0.7f, 0, 0.5f),
        FillAlpha = 0.5f,
        PoseLocal = new Pose(new Vector3F(-0, 2, 0))
      };
      _scene.Children.Add(figureNode);

      figure = new EllipseFigure
      {
        IsFilled = true,
        RadiusX = 0.5f,
        RadiusY = 0.1f,
      };
      figureNode = new FigureNode(figure)
      {
        Name = "Ellipse #4",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0, 0, 0),
        StrokeAlpha = 1,
        StrokeDashPattern = new Vector4F(1, 1, 1, 1) / 100,
        DashInWorldSpace = true,
        FillColor = new Vector3F(0.3f, 0.3f, 0.3f),
        FillAlpha = 0.5f,
        PoseLocal = new Pose(new Vector3F(1, 2, 0))
      };
      _scene.Children.Add(figureNode);

      figure = new EllipseFigure
      {
        IsFilled = true,
        RadiusX = 0.2f,
        RadiusY = 0.25f,
      };
      figureNode = new FigureNode(figure)
      {
        Name = "Ellipse #5",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0.3f),
        StrokeAlpha = 1,
        FillColor = new Vector3F(0.3f),
        FillAlpha = 1,
        PoseLocal = new Pose(new Vector3F(2, 2, 0))
      };
      _scene.Children.Add(figureNode);
    }


    // Add some transparent figures to test alpha blending.
    private void CreateAlphaBlendedFigures()
    {
      var rectangle = new RectangleFigure
      {
        IsFilled = true,
        WidthX = 0.5f,
        WidthY = 0.9f,
      };

      var figureNode = new FigureNode(rectangle)
      {
        Name = "Rectangle #6",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0.1f, 0.2f, 0.3f),
        FillColor = new Vector3F(0.1f, 0.2f, 0.3f),
        StrokeAlpha = 0.5f,
        FillAlpha = 0.5f,
        PoseLocal = new Pose(new Vector3F(-4, 1, -2))
      };
      _scene.Children.Add(figureNode);

      figureNode = new FigureNode(rectangle)
      {
        Name = "Rectangle #7",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0.1f, 0.2f, 0.3f),
        FillColor = new Vector3F(0.1f, 0.2f, 0.3f),
        StrokeAlpha = 0.5f,
        FillAlpha = 0.5f,
        PoseLocal = new Pose(new Vector3F(-4, 1, -1))
      };
      _scene.Children.Add(figureNode);

      figureNode = new FigureNode(rectangle)
      {
        Name = "Rectangle #8",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0.1f, 0.2f, 0.3f),
        FillColor = new Vector3F(0.1f, 0.2f, 0.3f),
        StrokeAlpha = 0.5f,
        FillAlpha = 0.5f,
        PoseLocal = new Pose(new Vector3F(-4, 1, 0))
      };
      _scene.Children.Add(figureNode);

      figureNode = new FigureNode(rectangle)
      {
        Name = "Rectangle #9",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0.1f, 0.2f, 0.3f),
        FillColor = new Vector3F(0.1f, 0.2f, 0.3f),
        StrokeAlpha = 0.5f,
        FillAlpha = 0.5f,
        PoseLocal = new Pose(new Vector3F(-4, 1, 1))
      };
      _scene.Children.Add(figureNode);

      figureNode = new FigureNode(rectangle)
      {
        Name = "Rectangle #10",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0.1f, 0.2f, 0.3f),
        FillColor = new Vector3F(0.1f, 0.2f, 0.3f),
        StrokeAlpha = 0.5f,
        FillAlpha = 0.5f,
        PoseLocal = new Pose(new Vector3F(-4, 1, 2))
      };
      _scene.Children.Add(figureNode);
    }


    // Add a grid with thick major grid lines and thin stroked minor grid lines.
    private void CreateGrid()
    {
      var majorGridLines = new PathFigure3F();
      for (int i = 0; i <= 10; i++)
      {
        majorGridLines.Segments.Add(new LineSegment3F
        {
          Point1 = new Vector3F(-5, 0, -5 + i),
          Point2 = new Vector3F(5, 0, -5 + i),
        });
        majorGridLines.Segments.Add(new LineSegment3F
        {
          Point1 = new Vector3F(-5 + i, 0, -5),
          Point2 = new Vector3F(-5 + i, 0, 5),
        });
      }

      var minorGridLines = new PathFigure3F();
      for (int i = 0; i < 10; i++)
      {
        minorGridLines.Segments.Add(new LineSegment3F
        {
          Point1 = new Vector3F(-5, 0, -4.5f + i),
          Point2 = new Vector3F(5, 0, -4.5f + i),
        });
        minorGridLines.Segments.Add(new LineSegment3F
        {
          Point1 = new Vector3F(-4.5f + i, 0, -5),
          Point2 = new Vector3F(-4.5f + i, 0, 5),
        });
      }

      var majorLinesNode = new FigureNode(majorGridLines)
      {
        Name = "Major grid lines",
        PoseLocal = Pose.Identity,
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0.1f),
        StrokeAlpha = 1f,
      };
      var minorLinesNode = new FigureNode(minorGridLines)
      {
        Name = "Minor grid lines",
        PoseLocal = Pose.Identity,
        StrokeThickness = 1,
        StrokeColor = new Vector3F(0.1f),
        StrokeAlpha = 1f,
        DashInWorldSpace = true,
        StrokeDashPattern = new Vector4F(10, 4, 0, 0) / 200,
      };
      var gridNode = new SceneNode
      {
        Name = "Grid",
        Children = new SceneNodeCollection(),
        PoseLocal = new Pose(new Vector3F(0, -0.5f, 0)),
      };
      gridNode.Children.Add(majorLinesNode);
      gridNode.Children.Add(minorLinesNode);
      _scene.Children.Add(gridNode);
    }


    // Add a clone of the grid.
    private void CreateGridClone()
    {
      var gridNode2 = _scene.GetSceneNode("Grid").Clone();
      gridNode2.Name = "Grid (Clone)";
      ((FigureNode)gridNode2.Children[0]).StrokeColor = new Vector3F(0, 0.8f, 0.2f);
      ((FigureNode)gridNode2.Children[1]).StrokeColor = new Vector3F(0, 0.8f, 0.2f);
      gridNode2.PoseLocal = new Pose(new Vector3F(-4, 3, 0), RandomHelper.Random.NextQuaternionF());
      gridNode2.ScaleLocal = new Vector3F(0.2f, 1, 0.1f);
      _scene.Children.Add(gridNode2);
    }


    // Add a random path.
    private void CreateRandomPath()
    {
      var path = new Path3F();
      var point = new Vector3F(0, 0, 0);
      path.Add(new PathKey3F
      {
        Interpolation = SplineInterpolation.CatmullRom,
        Parameter = 0,
        Point = point
      });
      for (int i = 1; i < 10; i++)
      {
        point += RandomHelper.Random.NextQuaternionF().Rotate(new Vector3F(0, 0.5f, 0));
        path.Add(new PathKey3F
        {
          Interpolation = SplineInterpolation.CatmullRom,
          Parameter = i,
          Point = point
        });
      }
      var pathFigure = new PathFigure3F();
      pathFigure.Segments.Add(path);

      var pathLineNode = new FigureNode(pathFigure)
      {
        Name = "RandomPath",
        PoseLocal = new Pose(new Vector3F(4, 1, 2)),
        StrokeThickness = 3,
        StrokeColor = new Vector3F(0.5f, 0.3f, 1),
        StrokeAlpha = 1f,
        DashInWorldSpace = true,
        StrokeDashPattern = new Vector4F(10, 1, 1, 1) / 100,
      };
      _scene.Children.Add(pathLineNode);
    }


    // Add a chain-like figure using TransformedFigure and CompositeFigure.
    private void CreateChain()
    {
      var ellipse = new EllipseFigure
      {
        IsFilled = false,
        RadiusX = 1f,
        RadiusY = 1f,
      };

      var compositeFigure = new CompositeFigure();
      for (int i = 0; i < 9; i++)
      {
        var transformedEllipse = new TransformedFigure(ellipse)
        {
          Scale = new Vector3F(0.4f, 0.2f, 1),
          Pose = new Pose(new Vector3F(-2 + i * 0.5f, 0, 0), Matrix33F.CreateRotationX(ConstantsF.PiOver2 * (i % 2)))
        };
        compositeFigure.Children.Add(transformedEllipse);
      }

      _scene.Children.Add(new FigureNode(compositeFigure)
      {
        Name = "Chain",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0.1f),
        StrokeAlpha = 1,
        PoseLocal = new Pose(new Vector3F(0, 3, 0)),
      });
    }


    // Add a 3D coordinate cross.
    private void CreateGizmo(SpriteFont spriteFont)
    {
      var gizmoNode = new SceneNode
      {
        Name = "Gizmo",
        Children = new SceneNodeCollection(),
        PoseLocal = new Pose(new Vector3F(3, 2, 0)),
        ScaleLocal = new Vector3F(0.5f)
      };

      // Red arrow
      var arrow = new PathFigure2F();
      arrow.Segments.Add(new LineSegment2F { Point1 = new Vector2F(0, 0), Point2 = new Vector2F(1, 0) });
      arrow.Segments.Add(new LineSegment2F { Point1 = new Vector2F(1, 0), Point2 = new Vector2F(0.9f, 0.02f) });
      arrow.Segments.Add(new LineSegment2F { Point1 = new Vector2F(1, 0), Point2 = new Vector2F(0.9f, -0.02f) });
      var figureNode = new FigureNode(arrow)
      {
        Name = "Gizmo X",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(1, 0, 0),
        PoseLocal = new Pose(new Vector3F(0, 0, 0))
      };
      gizmoNode.Children.Add(figureNode);

      // Green arrow
      var transformedArrow = new TransformedFigure(arrow)
      {
        Pose = new Pose(Matrix33F.CreateRotationZ(MathHelper.ToRadians(90)))
      };
      figureNode = new FigureNode(transformedArrow)
      {
        Name = "Gizmo Y",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0, 1, 0),
        PoseLocal = new Pose(new Vector3F(0, 0, 0))
      };
      gizmoNode.Children.Add(figureNode);

      // Blue arrow
      transformedArrow = new TransformedFigure(arrow)
      {
        Pose = new Pose(Matrix33F.CreateRotationY(MathHelper.ToRadians(-90)))
      };
      figureNode = new FigureNode(transformedArrow)
      {
        Name = "Gizmo Z",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0, 0, 1),
        PoseLocal = new Pose(new Vector3F(0, 0, 0))
      };
      gizmoNode.Children.Add(figureNode);

      // Red arc
      var arc = new PathFigure2F();
      arc.Segments.Add(
        new StrokedSegment2F(
          new LineSegment2F { Point1 = new Vector2F(0, 0), Point2 = new Vector2F(1, 0), },
          false));
      arc.Segments.Add(
        new ArcSegment2F
        {
          Point1 = new Vector2F(1, 0),
          Point2 = new Vector2F(0, 1),
          Radius = new Vector2F(1, 1)
        });
      arc.Segments.Add(
        new StrokedSegment2F(
        new LineSegment2F { Point1 = new Vector2F(0, 1), Point2 = new Vector2F(0, 0), },
        false));
      var transformedArc = new TransformedFigure(arc)
      {
        Scale = new Vector3F(0.333f),
        Pose = new Pose(Matrix33F.CreateRotationY(MathHelper.ToRadians(-90)))
      };
      figureNode = new FigureNode(transformedArc)
      {
        Name = "Gizmo YZ",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(1, 0, 0),
        FillColor = new Vector3F(1, 0, 0),
        FillAlpha = 0.5f,
        PoseLocal = new Pose(new Vector3F(0, 0, 0))
      };
      gizmoNode.Children.Add(figureNode);

      // Green arc
      transformedArc = new TransformedFigure(arc)
      {
        Scale = new Vector3F(0.333f),
        Pose = new Pose(Matrix33F.CreateRotationX(MathHelper.ToRadians(90)))
      };
      figureNode = new FigureNode(transformedArc)
      {
        Name = "Gizmo XZ",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0, 1, 0),
        FillColor = new Vector3F(0, 1, 0),
        FillAlpha = 0.5f,
        PoseLocal = new Pose(new Vector3F(0, 0, 0))
      };
      gizmoNode.Children.Add(figureNode);

      // Blue arc
      transformedArc = new TransformedFigure(arc)
      {
        Scale = new Vector3F(0.333f),
      };
      figureNode = new FigureNode(transformedArc)
      {
        Name = "Gizmo XY",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(0, 0, 1),
        FillColor = new Vector3F(0, 0, 1),
        FillAlpha = 0.5f,
        PoseLocal = new Pose(new Vector3F(0, 0, 0))
      };
      gizmoNode.Children.Add(figureNode);

      // Labels "X", "Y", "Z"
      var spriteNode = new SpriteNode(new TextSprite("X", spriteFont))
      {
        Color = new Vector3F(1, 0, 0),
        Origin = new Vector2F(0, 1),
        PoseLocal = new Pose(new Vector3F(1, 0, 0))
      };
      gizmoNode.Children.Add(spriteNode);
      spriteNode = new SpriteNode(new TextSprite("Y", spriteFont))
      {
        Color = new Vector3F(0, 1, 0),
        Origin = new Vector2F(0, 1),
        PoseLocal = new Pose(new Vector3F(0, 1, 0))
      };
      gizmoNode.Children.Add(spriteNode);
      spriteNode = new SpriteNode(new TextSprite("Z", spriteFont))
      {
        Color = new Vector3F(0, 0, 1),
        Origin = new Vector2F(0, 1),
        PoseLocal = new Pose(new Vector3F(0, 0, 1))
      };
      gizmoNode.Children.Add(spriteNode);

      _scene.Children.Add(gizmoNode);
    }


    // Add a flower shape.
    private void CreateFlower()
    {
      // Define single flower petal.
      var petalPath = new Path2F
      {
        new PathKey2F
        {
          Parameter = 0,
          Interpolation = SplineInterpolation.Bezier,
          Point = new Vector2F(0, 0),
          TangentIn = new Vector2F(0, 0),
          TangentOut = new Vector2F(-0.2f, 0.2f)
        },
        new PathKey2F
        {
          Parameter = 1,
          Interpolation = SplineInterpolation.Bezier,
          Point = new Vector2F(0, 1),
          TangentIn = new Vector2F(-0.3f, 1.1f),
          TangentOut = new Vector2F(0.3f, 1.1f)
        },
        new PathKey2F
        {
          Parameter = 2,
          Interpolation = SplineInterpolation.Bezier,
          Point = new Vector2F(0, 0),
          TangentIn = new Vector2F(0.2f, 0.2f),
          TangentOut = new Vector2F(0, 0)
        }
      };

      var petal = new PathFigure2F();
      petal.Segments.Add(petalPath);

      // Duplicate and rotate flower petal several times.
      const int numberOfPetals = 9;
      var flower = new CompositeFigure();
      flower.Children.Add(petal);
      for (int i = 1; i < numberOfPetals; i++)
      {
        var transformedPetal = new TransformedFigure(petal)
        {
          Pose = new Pose(Matrix33F.CreateRotationZ(i * ConstantsF.TwoPi / numberOfPetals))
        };
        flower.Children.Add(transformedPetal);
      }

      var flowerNode = new FigureNode(flower)
      {
        Name = "Flower",
        StrokeThickness = 2,
        StrokeColor = new Vector3F(1, 0.2f, 0.2f),
        FillColor = new Vector3F(1, 0.5f, 0.5f),
        FillAlpha = 1,
        PoseLocal = new Pose(new Vector3F(3, 1, 0)),
        ScaleLocal = new Vector3F(0.5f)
      };
      _scene.Children.Add(flowerNode);
    }


    public override void Update(GameTime gameTime)
    {
      _scene.Update(gameTime.ElapsedGameTime);

      _debugRenderer.Clear();

      base.Update(gameTime);
    }


    private void Render(RenderContext context)
    {
      context.CameraNode = _cameraObject.CameraNode;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.White);

      // Find all objects within camera frustum.
      var query = _scene.Query<CameraFrustumQuery>(_cameraObject.CameraNode, context);

      // Draw figure nodes.
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.BlendState = BlendState.AlphaBlend;
      _figureRenderer.Render(query.SceneNodes, context, RenderOrder.BackToFront);

      // Draw sprite nodes.
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.BlendState = BlendState.AlphaBlend;
      _spriteRenderer.Render(query.SceneNodes, context, RenderOrder.BackToFront);

      // Draw debug information.
      _debugRenderer.Render(context);

      context.CameraNode = null;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // IMPORTANT: Dispose scene nodes if they are no longer needed!
        _scene.Dispose(false);  // Disposes current and all descendant nodes.

        _figureRenderer.Dispose();
        _spriteRenderer.Dispose();
        _debugRenderer.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
