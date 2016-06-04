using System;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace InteropSample
{
  // A GraphicsScreen that draws a simple rotating triangle.
  public class TriangleGraphicsScreen : GraphicsScreen
  {
    private readonly VertexPositionColor[] _vertexList;
    private readonly BasicEffect _basicEffect;
    private float _angle;


    public TriangleGraphicsScreen(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      // Create triangle vertices.
      _vertexList = new VertexPositionColor[3];
      _vertexList[0] = new VertexPositionColor(new Vector3(0, 1, 0), Color.Red);
      _vertexList[1] = new VertexPositionColor(new Vector3(1, -0.5f, 0), Color.Blue);
      _vertexList[2] = new VertexPositionColor(new Vector3(-1, -0.5f, 0), Color.Green);

      // Initialize basic effect.
      _basicEffect = new BasicEffect(GraphicsService.GraphicsDevice)
      {
        VertexColorEnabled = true,
        View = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up),
      };
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Update rotation angle and compute new world matrix.
      _angle += MathHelper.Pi * (float)deltaTime.TotalSeconds;
      _basicEffect.World = Matrix.CreateFromYawPitchRoll(_angle, 0, 0);
    }


    protected override void OnRender(RenderContext context)
    {
      // Update the projection matrix. (The user can resize the windows and the
      // aspect ratio can change.)
      _basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(
        MathHelper.ToRadians(45),
        GraphicsService.GraphicsDevice.Viewport.AspectRatio,
        1.0f,
        100.0f);

      // No backface culling.
      GraphicsService.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

      foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
      {
        pass.Apply();
        GraphicsService.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertexList, 0, 1);
      }
    }
  }
}
