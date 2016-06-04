#if KINECT
using System;
using System.Linq;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;


namespace Samples.Kinect
{
  [Sample(SampleCategory.Kinect,
    @"This sample application shows how to use Kinect and Skeleton Mapping to animate 3D human
models in real-time.",
    @"This sample uses two tracked bodies of the Kinect sensor to animate two different 3D models
(Dude and Marine). 
The difficulty is that the bone hierarchy of a Kinect body, the Dude skeleton, and the
Marine skeleton are different. (The bone names and even the number of bones are different!)
The SkeletonMapper class (from DigitalRune Animation) is used to transfer a Kinect SkeletonPose
to model's SkeletonPose. Translations of the root bones will be transferred. For all other
bones, only bone rotations are transferred. A model is only visible when a player is detected.

This samples is a quick proofs of concept. There are still many aspects that can be improved.
Also have a look at the avateering sample in the original Microsoft Kinect for Windows Developer
Kit. This Microsoft samples shows how you can better clean up and filter the Kinect skeleton
data before you retarget it to a 3D model.",
    1)]
  [Controls(@"Sample
  Press <NumPad 7>/<NumPad 4> to increase/decrease Kinect sensor height.
  Press <NumPad 8>/<NumPad 5> to increase/decrease y offset of Kinect skeleton.
  Press <NumPad 9>/<NumPad 6> to increase/decrease scale of Kinect skeleton.
  Press <NumPad +>/<NumPad -> to increase/decrease the filter strength.
  Press <NumPad 1> to toggle drawing of Kinect skeletons.
  Press <NumPad 2> to toggle drawing of the model skeletons (green).")]
  class KinectSkeletonMappingSample : BasicSample
  {
    private readonly KinectWrapper _kinectWrapper;
    
    // The height of the Kinect sensor position. In our case it was 0.8 m above the floor.
    private float _kinectSensorHeight = 0.8f;

    // The 3D models.
    private MeshNode _meshNodeA;
    private MeshNode _meshNodeB;

    // The SkeletonMappers that map Kinect SkeletonPoses to the model's SkeletonPoses.
    private SkeletonMapper _skeletonMapperA;
    private SkeletonMapper _skeletonMapperB;

    // Low-pass filters that remove jitter from bone rotations in the SkeletonPoses.
    private readonly SkeletonPoseFilter _filterA;
    private readonly SkeletonPoseFilter _filterB;

    private bool _drawKinectSkeletons = true;
    private bool _drawModelSkeletons;


    public KinectSkeletonMappingSample(Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      SetCamera(new Vector3F(0, 1, 1), 0, 0);
      
      // Add a background object.
      GameObjectService.Objects.Add(new SandboxObject(Services));

      // Add a KinectWrapper which controls the Kinect device.
      _kinectWrapper = new KinectWrapper(game);
      game.Components.Add(_kinectWrapper);

      InitializeModels();

      _filterA = new SkeletonPoseFilter(_meshNodeA.SkeletonPose);
      _filterB = new SkeletonPoseFilter(_meshNodeB.SkeletonPose);

      InitializeSkeletonMappers();
    }


    private void InitializeModels()
    {
      var contentManager = Services.GetInstance<ContentManager>();

      var dudeModelNode = contentManager.Load<ModelNode>("Dude/Dude");
      _meshNodeA = dudeModelNode.GetSubtree().OfType<MeshNode>().First().Clone();
      _meshNodeA.PoseLocal = new Pose(new Vector3F(0, 0, 0));
      SampleHelper.EnablePerPixelLighting(_meshNodeA);
      GraphicsScreen.Scene.Children.Add(_meshNodeA);

      var marineModelNode = contentManager.Load<ModelNode>("Marine/PlayerMarine");
      _meshNodeB = marineModelNode.GetSubtree().OfType<MeshNode>().First().Clone();
      _meshNodeB.PoseLocal = new Pose(new Vector3F(0, 0, 0));
      SampleHelper.EnablePerPixelLighting(_meshNodeB);
      GraphicsScreen.Scene.Children.Add(_meshNodeB);
    }


    private void InitializeSkeletonMappers()
    {
      // Create a SkeletonMapper for each model. 
      // In this sample, the models on the screen should act like a mirror for the players'
      // movements. Therefore, we mirror the skeletons, e.g. the right Kinect arm controls left 
      // model arm.

      //
      // ----- SkeletonMapper for the Dude model.
      //
      _skeletonMapperA = new SkeletonMapper(_kinectWrapper.SkeletonPoseA, _meshNodeA.SkeletonPose);
      var ks = _kinectWrapper.SkeletonPoseA.Skeleton;
      var ms = _meshNodeA.SkeletonPose.Skeleton;

      // So far _skeletonMapperA does nothing. We have to configure how bones or bone chains
      // from the Kinect skeleton should map to the Dude skeleton. This is done using 
      // BoneMappers:      
      // A DirectBoneMapper transfers the rotation and scale of a single bone.
      _skeletonMapperA.BoneMappers.Add(new DirectBoneMapper(ks.GetIndex("SpineBase"), ms.GetIndex("Root"))
      {
        MapTranslations = true,
        ScaleAToB = 1f,           // TODO: Make this scale factor configurable.
      });

      // An UpperBackBoneMapper is a special bone mapper that is specifically designed for
      // spine bones. It uses the spine, neck and shoulders to compute the rotation of the spine
      // bone. This rotations is transferred to the Dude's "Spine" bone. 
      // (An UpperBackBoneMapper does not transfer bone translations.)
      _skeletonMapperA.BoneMappers.Add(new UpperBackBoneMapper(
        ks.GetIndex("SpineMid"), ks.GetIndex("SpineShoulder"), ks.GetIndex("ShoulderLeft"), ks.GetIndex("ShoulderRight"),
        ms.GetIndex("Spine"), ms.GetIndex("Neck"), ms.GetIndex("R_UpperArm"), ms.GetIndex("L_UpperArm")));

      // A ChainBoneMapper transfers the rotation of a bone chain. In this case, it rotates
      // the Dude's "R_UpperArm" bone. It makes sure that the direction from the Dude's
      // "R_Forearm" bone origin to the "R_UpperArm" origin is parallel, to the direction
      // "ElbowLeft" to "ShoulderLeft" of the Kinect skeleton.
      // (An ChainBoneMapper does not transfer bone translations.)
      _skeletonMapperA.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("ShoulderLeft"), ks.GetIndex("ElbowLeft"), ms.GetIndex("R_UpperArm"), ms.GetIndex("R_Forearm")));

      // And so on...
      _skeletonMapperA.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("ShoulderRight"), ks.GetIndex("ElbowRight"), ms.GetIndex("L_UpperArm"), ms.GetIndex("L_Forearm")));
      _skeletonMapperA.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("ElbowLeft"), ks.GetIndex("WristLeft"), ms.GetIndex("R_Forearm"), ms.GetIndex("R_Hand")));
      _skeletonMapperA.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("ElbowRight"), ks.GetIndex("WristRight"), ms.GetIndex("L_Forearm"), ms.GetIndex("L_Hand")));
      _skeletonMapperA.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("HipLeft"), ks.GetIndex("KneeLeft"), ms.GetIndex("R_Thigh"), ms.GetIndex("R_Knee")));
      _skeletonMapperA.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("HipRight"), ks.GetIndex("KneeRight"), ms.GetIndex("L_Thigh1"), ms.GetIndex("L_Knee2")));
      _skeletonMapperA.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("KneeLeft"), ks.GetIndex("AnkleLeft"), ms.GetIndex("R_Knee"), ms.GetIndex("R_Ankle")));
      _skeletonMapperA.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("KneeRight"), ks.GetIndex("AnkleRight"), ms.GetIndex("L_Knee2"), ms.GetIndex("L_Ankle1")));
      _skeletonMapperA.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("SpineShoulder"), ks.GetIndex("Neck"), ms.GetIndex("Neck"), ms.GetIndex("Head")));

      // We could also try to map the hand bones - but the Kinect input for the hands jitters a lot. 
      // It looks better if we do not animate the hands.
      //_skeletonMapperA.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("WristLeft"), ks.GetIndex("HandLeft"), ms.GetIndex("R_Hand"), ms.GetIndex("R_Middle1")));
      //_skeletonMapperA.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("WristRight"), ks.GetIndex("HandRight"), ms.GetIndex("L_Hand"), ms.GetIndex("L_Middle1")));

      //
      // ----- SkeletonMapper for the Marine model.
      //
      // (Same as for the Dude - only different bone names.)
      _skeletonMapperB = new SkeletonMapper(_kinectWrapper.SkeletonPoseB, _meshNodeB.SkeletonPose);
      ks = _kinectWrapper.SkeletonPoseB.Skeleton;
      ms = _meshNodeB.SkeletonPose.Skeleton;
      _skeletonMapperB.BoneMappers.Add(new DirectBoneMapper(ks.GetIndex("SpineBase"), ms.GetIndex("Spine_0"))
      {
        MapTranslations = true,
        ScaleAToB = 1f,             // TODO: Make this scale factor configurable.
      });
      _skeletonMapperB.BoneMappers.Add(new UpperBackBoneMapper(
        ks.GetIndex("SpineMid"), ks.GetIndex("SpineShoulder"), ks.GetIndex("ShoulderLeft"), ks.GetIndex("ShoulderRight"),
        ms.GetIndex("Spine1"), ms.GetIndex("Neck"), ms.GetIndex("R_Arm"), ms.GetIndex("L_Arm")));
      _skeletonMapperB.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("ShoulderLeft"), ks.GetIndex("ElbowLeft"), ms.GetIndex("R_Arm"), ms.GetIndex("R_Elbow")));
      _skeletonMapperB.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("ShoulderRight"), ks.GetIndex("ElbowRight"), ms.GetIndex("L_Arm"), ms.GetIndex("L_Elbow")));
      _skeletonMapperB.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("ElbowLeft"), ks.GetIndex("WristLeft"), ms.GetIndex("R_Elbow"), ms.GetIndex("R_Hand")));
      _skeletonMapperB.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("ElbowRight"), ks.GetIndex("WristRight"), ms.GetIndex("L_Elbow"), ms.GetIndex("L_Hand")));
      _skeletonMapperB.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("HipLeft"), ks.GetIndex("KneeLeft"), ms.GetIndex("R_Hip"), ms.GetIndex("R_Knee")));
      _skeletonMapperB.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("HipRight"), ks.GetIndex("KneeRight"), ms.GetIndex("L_Hip"), ms.GetIndex("L_Knee")));
      _skeletonMapperB.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("KneeLeft"), ks.GetIndex("AnkleLeft"), ms.GetIndex("R_Knee"), ms.GetIndex("R_Ankle")));
      _skeletonMapperB.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("KneeRight"), ks.GetIndex("AnkleRight"), ms.GetIndex("L_Knee"), ms.GetIndex("L_Ankle")));
      _skeletonMapperB.BoneMappers.Add(new ChainBoneMapper(ks.GetIndex("SpineShoulder"), ks.GetIndex("Neck"), ms.GetIndex("Neck"), ms.GetIndex("Head")));
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Clean up.
        Game.Components.Remove(_kinectWrapper);
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      HandleInput(deltaTime);

      // Map the _kinectSkeletonPoses of tracked players to the _modelSkeletonPoses.
      if (_kinectWrapper.IsTrackedA)
      {
        _skeletonMapperA.MapAToB();
        _filterA.Update(deltaTime);
      }
      if (_kinectWrapper.IsTrackedA)
      {
        _skeletonMapperB.MapAToB();
        _filterB.Update(deltaTime);
      }

      // Hide models if Kinect does not track the player.
      _meshNodeA.IsEnabled = _kinectWrapper.IsTrackedA;
      _meshNodeB.IsEnabled = _kinectWrapper.IsTrackedB;
      
      DrawDebugInfo();
    }


    private void HandleInput(float deltaTime)
    {
      // Change sensor height.
      if (InputService.IsDown(Keys.NumPad7))
        _kinectSensorHeight += 0.1f * deltaTime;
      if (InputService.IsDown(Keys.NumPad4))
        _kinectSensorHeight -= 0.1f * deltaTime;

      // Change y offset of Kinect skeleton data.
      if (InputService.IsDown(Keys.NumPad8))
        _kinectWrapper.Offset = _kinectWrapper.Offset + new Vector3F(0, 0.1f * deltaTime, 0);
      if (InputService.IsDown(Keys.NumPad5))
        _kinectWrapper.Offset = _kinectWrapper.Offset - new Vector3F(0, 0.1f * deltaTime, 0);

      // Change scale of Kinect skeleton data.
      if (InputService.IsDown(Keys.NumPad9))
        _kinectWrapper.Scale = _kinectWrapper.Scale + new Vector3F(0.1f * deltaTime);
      if (InputService.IsDown(Keys.NumPad6))
        _kinectWrapper.Scale = _kinectWrapper.Scale - new Vector3F(0.1f * deltaTime);

      // Toggle drawing of Kinect skeletons.
      if (InputService.IsPressed(Keys.NumPad1, false))
        _drawKinectSkeletons = !_drawKinectSkeletons;

      // Toggle drawing of model skeletons.
      if (InputService.IsPressed(Keys.NumPad2, false))
        _drawModelSkeletons = !_drawModelSkeletons;

      // Increase filter strength.
      if (InputService.IsDown(Keys.Add))
      {
        _filterA.TimeConstant += 0.05f * deltaTime;
        _filterB.TimeConstant = _filterA.TimeConstant;
      }
      // Decrease filter strength.
      if (InputService.IsDown(Keys.Subtract))
      {
        _filterA.TimeConstant = Math.Max(0, _filterA.TimeConstant - 0.05f * deltaTime);
        _filterB.TimeConstant = _filterA.TimeConstant;
      }
    }


    private void DrawDebugInfo()
    {
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      
      if (!_kinectWrapper.IsRunning)
      {
        // Kinect not found. Draw error message.
        debugRenderer.DrawText("\n\nKINECT SENSOR IS NOT AVAILABLE! PLEASE MAKE SURE THE DEVICE IS CONNECTED.");
        return;
      }

      // Draw Kinect skeletons for debugging.
      if (_drawKinectSkeletons)
      {
        var pose = new Pose(new Vector3F(0, _kinectSensorHeight, 0));
        debugRenderer.DrawSkeleton(_kinectWrapper.SkeletonPoseA, pose, Vector3F.One, 0.1f, Color.Orange, true);
        debugRenderer.DrawSkeleton(_kinectWrapper.SkeletonPoseB, pose, Vector3F.One, 0.1f, Color.Yellow, true);
      }

      // Draw model skeletons of tracked players for debugging.
      if (_drawModelSkeletons)
      {
        if (_kinectWrapper.IsTrackedA)
          debugRenderer.DrawSkeleton(_meshNodeA, 0.1f, Color.GreenYellow, true);
        if (_kinectWrapper.IsTrackedB)
          debugRenderer.DrawSkeleton(_meshNodeB, 0.1f, Color.DarkGreen, true);
      }

      // Draw Kinect settings info.
      debugRenderer.DrawText("\n\nKinect sensor height (<NumPad 7>/<NumPad 4>): " + _kinectSensorHeight
                             + "\nKinect skeleton Y offset (<NumPad 8>/<NumPad 5>): " + _kinectWrapper.Offset.Y
                             + "\nKinect skeleton scale (<NumPad 9>/<NumPad 6>): " + _kinectWrapper.Scale.X
                             + "\nFilter strength (<NumPad +>/<NumPad ->: " + _filterA.TimeConstant);
    }
  }
}
#endif