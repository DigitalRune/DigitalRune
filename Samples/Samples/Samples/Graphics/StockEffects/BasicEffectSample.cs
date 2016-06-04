using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to use the XNA BasicEffect.",
    @"The sample draws a grid and a tank model using the XNA BasicEffect. The scene nodes of the 
tank model are animated.",
    21)]
  public class BasicEffectSample : BasicSample
  {
    private readonly ModelNode _grid;
    private readonly ModelNode _tank;

    // A few selected scene nodes of the tank model, which we will animate.
    private SceneNode _frontWheelLeft;
    private SceneNode _frontWheelRight;
    private SceneNode _hatch;
    private SceneNode _turret;
    private SceneNode _cannon;

    // The original positions and orientations of the tank scene nodes.
    private Pose _frontWheelLeftRestPose;
    private Pose _frontWheelRightRestPose;
    private Pose _hatchRestPose;
    private Pose _turretRestPose;
    private Pose _cannonRestPose;

    // Animatable float values which will be used to animate the tank scene node orientations.
    private readonly AnimatableProperty<float> _frontWheelSteeringAngle = new AnimatableProperty<float>();
    private readonly AnimatableProperty<float> _hatchAngle = new AnimatableProperty<float>();
    private readonly AnimatableProperty<float> _turretAngle = new AnimatableProperty<float>();
    private readonly AnimatableProperty<float> _cannonAngle = new AnimatableProperty<float>();


    public BasicEffectSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3F(8, 6, 8), ConstantsF.PiOver4, -0.4f);

      // Load the models. The models are processed using the DigitalRune Model 
      // Processor - not the default XNA model processor!
      // In the folder that contains tank.fbx, there is an XML file tank.drmdl which defines 
      // properties of the model. These XML files are automatically processed by 
      // the DigitalRune Model Processor. Please browse to the content folder and 
      // have a look at the *.drmdl file. For the grid model there is no such file but
      // the DigitalRune model content processor will create one automatically with the default
      // materials found in the model. 
      // Each model itself is a tree of scene nodes. The grid model 
      // contains one mesh node. The tank model contains several mesh nodes (turret, 
      // cannon, hatch, wheels, ...).
      _grid = ContentManager.Load<ModelNode>("Ground/Ground");
      _tank = ContentManager.Load<ModelNode>("Tank/tank");

      // The XNA ContentManager manages a single instance of each model. We clone 
      // the models, to get a copy that we can modify without changing the original 
      // instance. 
      // Cloning is fast because it only duplicates the scene nodes - but not the 
      // mesh and material information.
      _grid = _grid.Clone();
      _tank = _tank.Clone();

      // The grid is a bit too large. We can scale it to make it smaller.
      _grid.ScaleLocal = new Vector3F(0.3f);

      // No need to scale the tank model - the tank was already scaled by the 
      // DigitalRune Model Processor because a scale factor is defined in the 
      // Tank.drmdl file.

      // Add the models to the scene.
      GraphicsScreen.Scene.Children.Add(_grid);
      GraphicsScreen.Scene.Children.Add(_tank);

      /*
        // If you want to turn off some of the default lights, you can get them by 
        // their name and change IsEnabled flags.
        var keyLight = (LightNode)_graphicsScreen.Scene.GetSceneNode("KeyLight");
        var fillLight = (LightNode)_graphicsScreen.Scene.GetSceneNode("FillLight");
        fillLight.IsEnabled = false;
        var backLight = (LightNode)_graphicsScreen.Scene.GetSceneNode("BackLight");
        backLight.IsEnabled = false;
      */

      /*
        // If you want to change the material properties of the tank, you can do this:
        // Loop through all mesh nodes of the tank.
        foreach (var meshNode in _tank.GetSubtree().OfType<MeshNode>())
        {
          // Loop through all materials of this mesh (each mesh can consist of several 
          // submeshes with different materials).
          foreach (var material in meshNode.Mesh.Materials)
          {
            // Get all BasicEffectBindings which wrap the XNA BasicEffect. 
            // A material can consist of several effects - one effect for each render 
            // pass. (Per default there is only one render pass called "Default".)
            foreach (var effectBinding in material.EffectBindings.OfType<BasicEffectBinding>())
            {
              effectBinding.PreferPerPixelLighting = true;
              effectBinding.Set("SpecularColor", new Vector3(1, 0, 0));
            }
          }
        }
      */

      CreateAndStartAnimations();
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Dispose model nodes.
        // Note: These operations are redundant because the Sample base class 
        // automatically disposes the GraphicsScreen.
        //GraphicsScreen.Scene.Children.Remove(_grid);
        //_grid.Dispose(false);

        //GraphicsScreen.Scene.Children.Remove(_tank);
        //_tank.Dispose(false);
      }

      base.Dispose(disposing);
    }


    private void CreateAndStartAnimations()
    {
      // Get the scene nodes that we want to animate using their names (as defined 
      // in the .fbx file).
      _frontWheelLeft = _tank.GetSceneNode("l_steer_geo");
      _frontWheelLeftRestPose = _frontWheelLeft.PoseLocal;
      _frontWheelRight = _tank.GetSceneNode("r_steer_geo");
      _frontWheelRightRestPose = _frontWheelRight.PoseLocal;
      _hatch = _tank.GetSceneNode("hatch_geo");
      _hatchRestPose = _hatch.PoseLocal;
      _turret = _tank.GetSceneNode("turret_geo");
      _turretRestPose = _turret.PoseLocal;
      _cannon = _tank.GetSceneNode("canon_geo");
      _cannonRestPose = _cannon.PoseLocal;

      // Create and start some animations. For general information about the DigitalRune Animation
      // system, please check out the user documentation and the DigitalRune Animation samples.

      // The front wheel should rotate left/right; oscillating endlessly.
      var frontWheelSteeringAnimation = new AnimationClip<float>(
        new SingleFromToByAnimation
        {
          From = -0.3f,
          To = 0.3f,
          Duration = TimeSpan.FromSeconds(3),
          EasingFunction = new SineEase { Mode = EasingMode.EaseInOut }
        })
      {
        Duration = TimeSpan.MaxValue,
        LoopBehavior = LoopBehavior.Oscillate,
      };
      AnimationService.StartAnimation(frontWheelSteeringAnimation, _frontWheelSteeringAngle)
                      .AutoRecycle();

      // The hatch opens using a bounce ease. 
      var bounceOpenAnimation = new SingleFromToByAnimation
      {
        By = -0.8f,
        Duration = TimeSpan.FromSeconds(1),
        EasingFunction = new BounceEase { Mode = EasingMode.EaseOut }
      };
      // Then it should close again.
      var bounceCloseAnimation = new SingleFromToByAnimation
      {
        By = 0.8f,
        Duration = TimeSpan.FromSeconds(0.5f),
      };
      // We combine the open and close animation. The close animation should start 
      // 2 seconds after the open animation (Delay = 2) and it should stay some 
      // time in the final position (Duration = 2).
      var bounceOpenCloseAnimation = new TimelineGroup
      {
        bounceOpenAnimation,
        new TimelineClip(bounceCloseAnimation) { Delay = TimeSpan.FromSeconds(2), Duration = TimeSpan.FromSeconds(2)},
      };
      // The bounceOpenCloseAnimation should loop forever.
      var hatchAnimation = new TimelineClip(bounceOpenCloseAnimation)
      {
        Duration = TimeSpan.MaxValue,
        LoopBehavior = LoopBehavior.Cycle,
      };
      AnimationService.StartAnimation(hatchAnimation, _hatchAngle)
                      .AutoRecycle();

      // The turret rotates left/right endlessly.
      var turretAnimation = new AnimationClip<float>(
        new SingleFromToByAnimation
        {
          From = -0.5f,
          To = 0.5f,
          Duration = TimeSpan.FromSeconds(4),
          EasingFunction = new HermiteEase { Mode = EasingMode.EaseInOut }
        })
      {
        Duration = TimeSpan.MaxValue,
        LoopBehavior = LoopBehavior.Oscillate,
      };
      AnimationService.StartAnimation(turretAnimation, _turretAngle)
                      .AutoRecycle();

      // The cannon rotates up/down endlessly.
      var cannonAnimation = new AnimationClip<float>(
        new SingleFromToByAnimation
        {
          By = -0.7f,
          Duration = TimeSpan.FromSeconds(6),
          EasingFunction = new HermiteEase { Mode = EasingMode.EaseInOut }
        })
      {
        Duration = TimeSpan.MaxValue,
        LoopBehavior = LoopBehavior.Oscillate,
      };
      AnimationService.StartAnimation(cannonAnimation, _cannonAngle)
                      .AutoRecycle();
    }


    public override void Update(GameTime gameTime)
    {
      // Update the poses of the animated scene nodes.
      var frontWheelSteeringRotation = Matrix33F.CreateRotationY(_frontWheelSteeringAngle.Value);
      _frontWheelLeft.PoseLocal = _frontWheelLeftRestPose * new Pose(frontWheelSteeringRotation);
      _frontWheelRight.PoseLocal = _frontWheelRightRestPose * new Pose(frontWheelSteeringRotation);
      _hatch.PoseLocal = _hatchRestPose * new Pose(Matrix33F.CreateRotationX(_hatchAngle.Value));
      _turret.PoseLocal = _turretRestPose * new Pose(Matrix33F.CreateRotationY(_turretAngle.Value));
      _cannon.PoseLocal = _cannonRestPose * new Pose(Matrix33F.CreateRotationX(_cannonAngle.Value));

      base.Update(gameTime);
    }
  }
}
