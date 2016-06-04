using System;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// We use the DigitalRune classes - not the XNA classes!!!
using DirectionalLight = DigitalRune.Graphics.DirectionalLight;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to use a Scene to manage SceneNodes (like ModelNodes, MeshNodes,
LightNodes, CameraNodes, ...) and how to animate a skinned model.",
    "",
    7)]
  public class SceneSample : Sample
  {
    private readonly CameraObject _cameraObject;

    private readonly Scene _scene;

    private readonly ModelNode _model0;
    private readonly ModelNode _model1;
    private AnimationController _animationController0;
    private AnimationController _animationController1;

    private readonly MeshRenderer _meshRenderer;
    private readonly DebugRenderer _debugRenderer;

    private float _angle;


    public SceneSample(Microsoft.Xna.Framework.Game game)
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

      // Create a new empty scene.
      _scene = new Scene();

      // Add the camera node to the scene.
      _scene.Children.Add(_cameraObject.CameraNode);

      // Load a model. This model uses the DigitalRune Model Processor. Several XML 
      // files (*.drmdl and *.drmat) in the folder of dude.fbx define the materials and other properties. 
      // The DigitalRune Model Processor also imports the animations of the dude model.
      var model = ContentManager.Load<ModelNode>("Dude/Dude");

      // Add two clones of the model to the scene.
      _model0 = model.Clone();
      _model1 = model.Clone();
      _scene.Children.Add(_model0);
      _scene.Children.Add(_model1);

      // The dude model contains a single mesh node.
      var meshNode0 = (MeshNode)_model0.Children[0];
      var meshNode1 = (MeshNode)_model1.Children[0];

      // The imported animation data (skeleton and animations) is stored with the mesh.
      var animations = meshNode0.Mesh.Animations;

      // The MeshNodes of skinned models has a SkeletonPose which can be animated.
      // Let's start the first animation.
      var timeline0 = new TimelineClip(animations.Values.First())
      {
        LoopBehavior = LoopBehavior.Cycle, // Loop animation...
        Duration = TimeSpan.MaxValue,      // ...forever.
      };
      _animationController0 = AnimationService.StartAnimation(timeline0, (IAnimatableProperty)meshNode0.SkeletonPose);
      _animationController0.UpdateAndApply();

      var timeline1 = new TimelineClip(animations.Values.First())
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,

        // Start second animation at a different animation time to add some variety.
        Delay = TimeSpan.FromSeconds(-1),
      };
      _animationController1 = AnimationService.StartAnimation(timeline1, (IAnimatableProperty)meshNode1.SkeletonPose);
      _animationController1.UpdateAndApply();

      // Add some lights to the scene which have the same properties as the lights 
      // of BasicEffect.EnableDefaultLighting().
      InitializeDefaultXnaLights(_scene);

      _meshRenderer = new MeshRenderer();

      var spriteFont = UIContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
      _debugRenderer = new DebugRenderer(GraphicsService, spriteFont);
    }


    // Creates light sources with the same settings as BasicEffect.EnableDefaultLighting() 
    // in the XNA Framework.
    internal static void InitializeDefaultXnaLights(Scene scene)
    {
      var ambientLight = new AmbientLight
      {
        Color = new Vector3F(0.05333332f, 0.09882354f, 0.1819608f),
        Intensity = 1,
        HemisphericAttenuation = 0,
      };
      scene.Children.Add(new LightNode(ambientLight));

      var keyLight = new DirectionalLight
      {
        Color = new Vector3F(1, 0.9607844f, 0.8078432f),
        DiffuseIntensity = 1,
        SpecularIntensity = 1,
      };
      var keyLightNode = new LightNode(keyLight)
      {
        Name = "KeyLight",
        Priority = 10,   // This is the most important light.
        PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(-0.5265408f, -0.5735765f, -0.6275069f))),
      };
      scene.Children.Add(keyLightNode);

      var fillLight = new DirectionalLight
      {
        Color = new Vector3F(0.9647059f, 0.7607844f, 0.4078432f),
        DiffuseIntensity = 1,
        SpecularIntensity = 0,
      };
      var fillLightNode = new LightNode(fillLight)
      {
        Name = "FillLight",
        PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(0.7198464f, 0.3420201f, 0.6040227f))),
      };
      scene.Children.Add(fillLightNode);

      var backLight = new DirectionalLight
      {
        Color = new Vector3F(0.3231373f, 0.3607844f, 0.3937255f),
        DiffuseIntensity = 1,
        SpecularIntensity = 1,
      };
      var backLightNode = new LightNode(backLight)
      {
        Name = "BackLight",
        PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(0.4545195f, -0.7660444f, 0.4545195f))),
      };
      scene.Children.Add(backLightNode);
    }


    public override void Update(GameTime gameTime)
    {
      // Move models in circles.
      _angle += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.6f;

      // Set SceneNode.LastPoseWorld to the current pose. This is only required 
      // when advanced effects are used, like post-process motion blur. Since this 
      // is not the case in this sample, we could skip this step. However, it is a
      // good practice to always update SceneNode.LastPoseWorld and LastScaleWorld
      // before the pose or scale is changed.
      _model0.SetLastPose(true);
      _model1.SetLastPose(true);

      // Update the position and orientation of the models.
      _model0.PoseWorld = new Pose(new Vector3F(2, 0, 0))
                          * new Pose(Matrix33F.CreateRotationY(_angle))
                          * new Pose(new Vector3F(2, 0, 0));
      _model1.PoseWorld = new Pose(new Vector3F(-2, 0, 0))
                          * new Pose(Matrix33F.CreateRotationY(-_angle + 0.5f))
                          * new Pose(new Vector3F(-2, 0, 0));

      // Draw the bounding shapes of the meshes.
      //
      // Note - Bounding shapes of skinned models:
      // The model description file (dude.drmdl) specifies a custom bounding shape 
      // (see the attributes AabbMinimum and AabbMaximum in this file). For 
      // skinned models this bounding shape must be chosen manually. It must be big 
      // enough to contain all poses of the model. - In future DigitalRune Graphics 
      // versions we will automatically compute suitable bounding shapes for 
      // animated models.
      _debugRenderer.Clear();
      foreach (var sceneNode in _scene.GetSubtree().OfType<MeshNode>())
        _debugRenderer.DrawObject(sceneNode, Color.Orange, true, false);

      // Draw a coordinate cross at the world space origin.
      _debugRenderer.DrawAxes(Pose.Identity, 1, false);

      // Update the scene - this must be called once per frame.
      // The scene will compute internal collision information for camera frustum 
      // culling or light culling (which lights overlap with which mesh nodes).
      _scene.Update(gameTime.ElapsedGameTime);

      base.Update(gameTime);
    }


    private void Render(RenderContext context)
    {
      // Set render context info.
      context.CameraNode = _cameraObject.CameraNode;
      context.Scene = _scene;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      graphicsDevice.Clear(Color.CornflowerBlue);

      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;

      // Frustum culling: Get all scene nodes which overlap the view frustum.
      var query = _scene.Query<CameraFrustumQuery>(context.CameraNode, context);

      // Render all meshes that are in the camera frustum.
      context.RenderPass = "Default";
      _meshRenderer.Render(query.SceneNodes, context);

      _debugRenderer.Render(context);

      // Clean up.
      context.RenderPass = null;
      context.Scene = null;
      context.CameraNode = null;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _animationController0.Stop();
        _animationController0.Recycle();
        _animationController1.Stop();
        _animationController1.Recycle();

        // IMPORTANT: Dispose scene nodes if they are no longer needed!
        _scene.Dispose(false);  // Disposes current and all descendant nodes.

        _meshRenderer.Dispose();
        _debugRenderer.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
