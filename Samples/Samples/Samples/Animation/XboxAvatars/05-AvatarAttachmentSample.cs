#if XBOX
using System;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Samples.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;

namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    "This sample shows how to attach an object to an animated avatar.",
    "",
    105)]
  public class AvatarAttachmentSample : AnimationSample
  {
    private readonly Scene _scene;
    private readonly MeshRenderer _meshRenderer;
    private readonly CameraObject _cameraObject;

    private readonly AvatarDescription _avatarDescription;
    private readonly AvatarRenderer _avatarRenderer;

    private AvatarPose _avatarPose;
    private Pose _pose = new Pose(new Vector3F(-0.5f, 0, 0));
    private TimelineClip _walkAnimation;
    private MeshNode _baseballBatMeshNode;


    public AvatarAttachmentSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // This sample uses Scene and MeshRenderer for rendering the attached models.
      _scene = new Scene();
      _meshRenderer = new MeshRenderer();

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      _cameraObject.ResetPose(new Vector3F(0, 1, -3), ConstantsF.Pi, 0);
      GameObjectService.Objects.Add(_cameraObject);

      // Create a random avatar.
      _avatarDescription = AvatarDescription.CreateRandom();
      _avatarRenderer = new AvatarRenderer(_avatarDescription);

      // Load walk animation using the content pipeline.
      TimelineGroup animation = ContentManager.Load<TimelineGroup>("XboxAvatars/Walk");

      // Create a looping walk animation.
      _walkAnimation = new TimelineClip(animation)
      {
        LoopBehavior = LoopBehavior.Cycle,  // Cycle Walk animation...
        Duration = TimeSpan.MaxValue,       // ...forever.
      };

      var baseballBatModelNode = ContentManager.Load<ModelNode>("XboxAvatars/BaseballBat").Clone();
      _baseballBatMeshNode = baseballBatModelNode.GetChildren().OfType<MeshNode>().First();

      // If we only render the baseball bat, it appears black. We need to add it to
      // a scene with some lights. (The lights do not affect the avatar.)
      SceneSample.InitializeDefaultXnaLights(_scene);

      // We must detach the mesh node from its current parent (the model node) before
      // we can add it to the scene.
      _baseballBatMeshNode.Parent.Children.Remove(_baseballBatMeshNode);
      _scene.Children.Add(_baseballBatMeshNode);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _scene.Dispose(false);
        _meshRenderer.Dispose();
        _avatarRenderer.Dispose();
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      if (_avatarPose == null)
      {
        if (_avatarRenderer.State == AvatarRendererState.Ready)
        {
          _avatarPose = new AvatarPose(_avatarRenderer);
          AnimationService.StartAnimation(_walkAnimation, _avatarPose).AutoRecycle();
        }
      }
      else
      {
        // Update pose of attached baseball bat.
        // The offset of the baseball bat origin to the bone origin (in bone space)
        Pose offset = new Pose(new Vector3F(0.01f, 0.05f, 0.0f), Matrix33F.CreateRotationY(MathHelper.ToRadians(-20)));
        // The bone position in model space
        SrtTransform bonePose = _avatarPose.SkeletonPose.GetBonePoseAbsolute((int)AvatarBone.SpecialRight);
        // The baseball bat matrix in world space
        _baseballBatMeshNode.PoseWorld = _pose * (Pose)bonePose * offset;
      }
    }


    protected override void OnRender(RenderContext context)
    {
      // Set render context info.
      context.CameraNode = _cameraObject.CameraNode;
      context.Scene = _scene;

      // Clear screen.
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.CornflowerBlue);

      if (_avatarPose != null)
      {
        // Draw baseball bat.
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        graphicsDevice.BlendState = BlendState.Opaque;
        context.RenderPass = "Default";
        _meshRenderer.Render(_baseballBatMeshNode, context);
        context.RenderPass = null;

        // Draw avatar.
        _avatarRenderer.World = _pose;
        _avatarRenderer.View = (Matrix)_cameraObject.CameraNode.View;
        _avatarRenderer.Projection = _cameraObject.CameraNode.Camera.Projection;
        _avatarRenderer.Draw(_avatarPose);
      }

      // Clean up.
      context.CameraNode = null;
      context.Scene = null;
    }
  }
}
#endif