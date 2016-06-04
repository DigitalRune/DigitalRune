#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample uses an ObjectMotionBlur which blurs object that are moving relative to the camera.",
    @"Important notes:
The ObjectMotionBlur needs a ""velocity buffer"". This buffer is created in PostProcessingGraphicsScreen.
In this sample application only the cubes are motion blurred. The cube uses a material which
supports the ""Velocity"" render pass - please have a look at the MetalGrateBox asset folder with 
the files Velocity.fx (velocity buffer shader code) and Metal_Grate.drmat (material definition).
Further the property SceneNode.LastPose must be set for the CameraNode and the moving scene nodes.
This is typically done by calling SceneHelper.SetLastPose() before setting new poses.",
    45)]
  public class ObjectMotionBlurSample : PostProcessingSample
  {
    private readonly ObjectMotionBlur _objectMotionBlur;

    // Two cubes which are moving to demonstrate object motion blur.
    private readonly ModelNode _movingCube;
    private readonly ModelNode _rotatingCube;
    private float _animationTime;


    public ObjectMotionBlurSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _objectMotionBlur = new ObjectMotionBlur(GraphicsService)
      {
        NumberOfSamples = 11,
        SoftenEdges = true,
      };
      GraphicsScreen.PostProcessors.Add(_objectMotionBlur);


      // ----- 2 cubes that will be animated in this class.
      var cube = ContentManager.Load<ModelNode>("MetalGrateBox/MetalGrateBox");
      _movingCube = cube.Clone();
      GraphicsScreen.Scene.Children.Add(_movingCube);
      _rotatingCube = cube.Clone();
      GraphicsScreen.Scene.Children.Add(_rotatingCube);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // ----- Animate the 2 cubes.
      _animationTime += (float)gameTime.ElapsedGameTime.TotalSeconds * 10;

      // Before updating the positions and orientations, store the previous poses of 
      // the scene nodes. (The property SceneNode.LastPoseWorld is read by certain 
      // post-processing effects, such as motion blur.)
      _movingCube.SetLastPose(true);
      _rotatingCube.SetLastPose(true);

      _movingCube.PoseWorld = new Pose(new Vector3F(-6, 1, -3 + (float)Math.Sin(_animationTime) * 2f));
      _rotatingCube.PoseWorld = new Pose(new Vector3F(-4, 1, -1), Matrix33F.CreateRotationX(_animationTime));

      // <1> / <Shift> + <1> --> Change number of samples.
      if (InputService.IsPressed(Keys.D1, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float delta = isShiftDown ? +1 : -1;
        _objectMotionBlur.NumberOfSamples = MathHelper.Max(_objectMotionBlur.NumberOfSamples + delta, 1);
      }

      // <2> / <Shift> + <2> --> Change max blur radius.
      if (InputService.IsDown(Keys.D2))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _objectMotionBlur.MaxBlurRadius =
          MathHelper.Max(_objectMotionBlur.MaxBlurRadius * (float)Math.Pow(factor, time * 60), 1);
      }

      // <3> / <Shift> + <3> --> Toggle soft edges.
      if (InputService.IsPressed(Keys.D3, false))
        _objectMotionBlur.SoftenEdges = !_objectMotionBlur.SoftenEdges;

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the number of samples: " + _objectMotionBlur.NumberOfSamples
        + "\nHold <2> or <Shift>+<2> to decrease or increase the max blur radius: " + _objectMotionBlur.MaxBlurRadius
        + "\nPress <3> to toggle soft edges: " + _objectMotionBlur.SoftenEdges);
    }
  }
}
#endif