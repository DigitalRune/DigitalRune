#if XBOX
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample shows how to use the AvatarPose class to manipulate an Xbox avatar.",
    @"One arm is rotated in code.
The bones are drawn for debugging. - This is very useful to check bone coordinate systems
and bone names.",
    101)]
  public class BasicAvatarSample : AnimationSample
  {
    private readonly DebugRenderer _debugRenderer;
    private readonly CameraObject _cameraObject;

    private readonly AvatarDescription _avatarDescription;
    private readonly AvatarRenderer _avatarRenderer;

    // The AvatarPose which defines the pose of the skeleton and the avatar expression.
    private AvatarPose _avatarPose;

    // The world space position and orientation of the avatar.
    private Pose _pose = new Pose(new Vector3F(-0.5f, 0, 0));


    public BasicAvatarSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // This sample uses for a DebugRenderer for the rendering Avatar skeleton.
      _debugRenderer = new DebugRenderer(GraphicsService, SpriteFont)
      {
        DefaultColor = Color.Black,
        DefaultTextPosition = new Vector2F(10),
      };

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      _cameraObject.ResetPose(new Vector3F(0, 1, -3), ConstantsF.Pi, 0);
      GameObjectService.Objects.Add(_cameraObject);

      // Create a random avatar.
      _avatarDescription = AvatarDescription.CreateRandom();
      _avatarRenderer = new AvatarRenderer(_avatarDescription);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _debugRenderer.Dispose();
        _avatarRenderer.Dispose();
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      _debugRenderer.Clear();

      if (_avatarPose == null)
      {
        // Must wait till renderer is ready. Before that we do not get skeleton info.
        if (_avatarRenderer.State == AvatarRendererState.Ready)
        {
          // Create AvatarPose.
          _avatarPose = new AvatarPose(_avatarRenderer);

          // A 'bone transform' is the transformation of a bone relative to its bind pose.
          // Bone transforms define the pose of a skeleton.

          // Rotate arm of avatar. 
          SkeletonPose skeletonPose = _avatarPose.SkeletonPose;
          int shoulderIndex = skeletonPose.Skeleton.GetIndex("ShoulderLeft");
          skeletonPose.SetBoneTransform(shoulderIndex, new SrtTransform(QuaternionF.CreateRotationZ(-0.9f)));

          // The class SkeletonHelper provides some useful extension methods.
          // One is SetBoneRotationAbsolute() which sets the orientation of a bone relative 
          // to model space.
          // Rotate elbow to make the lower arm point forward.
          int elbowIndex = skeletonPose.Skeleton.GetIndex("ElbowLeft");
          SkeletonHelper.SetBoneRotationAbsolute(skeletonPose, elbowIndex, QuaternionF.CreateRotationY(ConstantsF.PiOver2));

          // Draw avatar skeleton for debugging.
          _debugRenderer.DrawSkeleton(skeletonPose, _pose, Vector3F.One, 0.02f, Color.Orange, true);
        }
      }
    }


    protected override void OnRender(RenderContext context)
    {
      // Set render context info.
      context.CameraNode = _cameraObject.CameraNode;

      // Clear screen.
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.CornflowerBlue);

      // Draw avatar.
      if (_avatarPose != null)
      {
        _avatarRenderer.World = _pose;
        _avatarRenderer.View = (Matrix)_cameraObject.CameraNode.View;
        _avatarRenderer.Projection = _cameraObject.CameraNode.Camera.Projection;
        _avatarRenderer.Draw(_avatarPose);
      }

      // Draw debug info.
      _debugRenderer.Render(context);

      // Clean up.
      context.CameraNode = null;
    }
  }
}
#endif