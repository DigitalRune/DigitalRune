#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Animation.Traits;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample demonstrates the use of of morph targets for facial animation.",
    @"Use the sliders in the Options window to control the weights of the morph targets.
The vertex shader can mix up to 5 morph targets per submesh simulteneously. If more
than 5 morph targets are active, the morph targets with the largest weights are used.",
    127)]
  public class FacialAnimationSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly MeshNode _sintel;

    private Slider[] _sliders;

    private bool _drawSkeleton;
    private readonly SkeletonPose _mouthClosedPose;
    private readonly SkeletonPose _mouthOpenPose;

    // Test animation
    private readonly ITimeline _morphingAnimation;
    private readonly ITimeline _skeletalAnimation;
    private bool _isPlaying;
    private AnimationController _morphingAnimationController;
    private AnimationController _skeletalAnimationController;


    public FacialAnimationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _graphicsScreen = new DeferredGraphicsScreen(Services) { DrawReticle = false };
      GraphicsService.Screens.Insert(0, _graphicsScreen);
      Services.Register(typeof(DebugRenderer), null, _graphicsScreen.DebugRenderer);
      Services.Register(typeof(IScene), null, _graphicsScreen.Scene);

      // Add a game object which adds some GUI controls for the deferred graphics
      // screen to the Options window.
      GameObjectService.Objects.Add(new DeferredGraphicsOptionsObject(Services));

      // Use a fixed camera.
      var projection = new PerspectiveProjection();
      projection.SetFieldOfView(
        ConstantsF.PiOver4,
        GraphicsService.GraphicsDevice.Viewport.AspectRatio,
        0.1f,
        10);
      var cameraNode = new CameraNode(new Camera(projection));
      cameraNode.LookAt(new Vector3F(0.15f, 0.15f, 0.5f), new Vector3F(0.1f, 0.15f, 0), Vector3F.Up);
      _graphicsScreen.Scene.Children.Add(cameraNode);
      _graphicsScreen.ActiveCameraNode = cameraNode;

      // Lighting setup:
      var keyLight = new LightNode(new Spotlight { DiffuseIntensity = 0.6f, SpecularIntensity = 0.4f });
      keyLight.LookAt(new Vector3F(-2, 2, 2), new Vector3F(), Vector3F.Up);
      _graphicsScreen.Scene.Children.Add(keyLight);

      var backLight = new LightNode(new Spotlight { DiffuseIntensity = 0.3f, SpecularIntensity = 0.3f });
      backLight.LookAt(new Vector3F(1, 0.5f, -2), new Vector3F(), Vector3F.Up);
      _graphicsScreen.Scene.Children.Add(backLight);
      
      var fillLight = new LightNode(new AmbientLight { HemisphericAttenuation = 1, Intensity = 0.1f });
      _graphicsScreen.Scene.Children.Add(fillLight);

      // The scene does not have a proper background. That's why the exposure is a 
      // bit off. --> Reduce the max exposure.
      var hdrFilter = _graphicsScreen.PostProcessors.OfType<HdrFilter>().First();
      hdrFilter.MaxExposure = 6;

      // Load the customized "Sintel" model (original: Durian Open Movie Project - http://www.sintel.org/).
      var model = ContentManager.Load<ModelNode>("Sintel/Sintel-Head").Clone();
      model.PoseWorld = new Pose(new Vector3F(0, 0, 0), Matrix33F.CreateRotationY(MathHelper.ToRadians(10)) * Matrix33F.CreateRotationX(-MathHelper.ToRadians(90)));
      _graphicsScreen.Scene.Children.Add(model);

      // The model consists of a root node and a mesh node.
      //  ModelNode "Sintel-Head"
      //    MeshNode "Sintel"
      _sintel = (MeshNode)model.Children[0];

      // The model contains two skeletal animations:
      // - "MOUTH-open" is just a single frame.
      // - "Test" is a short animation (250 frames).

      // In the Options window, we will add a slider to move the jaw.
      // Slider.Value = 0 ... mouth closed (default)
      _mouthClosedPose = SkeletonPose.Create(_sintel.Mesh.Skeleton);
      // Slider.Value = 1 ... mouth open (copied from the "MOUTH-open" animation)
      SkeletonKeyFrameAnimation mouthOpen = _sintel.Mesh.Animations["MOUTH-open"];
      _mouthOpenPose = SkeletonPose.Create(_sintel.Mesh.Skeleton);
      mouthOpen.GetValue(TimeSpan.Zero, ref _mouthOpenPose, ref _mouthOpenPose, ref _mouthOpenPose);

      // Turn the "Test" animation into an endless loop.
      _skeletalAnimation = new AnimationClip<SkeletonPose>(_sintel.Mesh.Animations["Test"])
      {
        Duration = TimeSpan.MaxValue,
        LoopBehavior = LoopBehavior.Cycle
      };

      // Mesh has several morph targets for facial animation, which are imported
      // automatically via the content pipeline. Unfortunately, the XNA content
      // pipeline cannot import morph target animations automatically.
      // In this demo, we will create a morph target animation in code.
      _morphingAnimation = CreateMorphingAnimation();

      CreateGuiControls();
    }


    // Create morph target animation in code.
    private static ITimeline CreateMorphingAnimation()
    {
      // The weight of each morph target is controlled by a keyframe animation.
      var browMad = new SingleKeyFrameAnimation { TargetProperty = "BROW-mad" };
      browMad.KeyFrames.Add(new KeyFrame<float>(FrameToTime(15), 0));
      browMad.KeyFrames.Add(new KeyFrame<float>(FrameToTime(30), 1));
      browMad.KeyFrames.Add(new KeyFrame<float>(FrameToTime(65), 1));
      browMad.KeyFrames.Add(new KeyFrame<float>(FrameToTime(80), 0));

      var browSurp = new SingleKeyFrameAnimation { TargetProperty = "BROW-surp" };
      browSurp.KeyFrames.Add(new KeyFrame<float>(FrameToTime(0), 0));
      browSurp.KeyFrames.Add(new KeyFrame<float>(FrameToTime(15), 0.5f));
      browSurp.KeyFrames.Add(new KeyFrame<float>(FrameToTime(30), 0));
      browSurp.KeyFrames.Add(new KeyFrame<float>(FrameToTime(210), 0));
      browSurp.KeyFrames.Add(new KeyFrame<float>(FrameToTime(220), 0.5f));
      browSurp.KeyFrames.Add(new KeyFrame<float>(FrameToTime(230), 0.5f));
      browSurp.KeyFrames.Add(new KeyFrame<float>(FrameToTime(250), 0));

      var cheekIn = new SingleKeyFrameAnimation { TargetProperty = "CHEEK-in" };
      cheekIn.KeyFrames.Add(new KeyFrame<float>(FrameToTime(0), 0));
      cheekIn.KeyFrames.Add(new KeyFrame<float>(FrameToTime(15), 1));
      cheekIn.KeyFrames.Add(new KeyFrame<float>(FrameToTime(30), 0));

      var cheekOut = new SingleKeyFrameAnimation { TargetProperty = "CHEEK-out" };
      cheekOut.KeyFrames.Add(new KeyFrame<float>(FrameToTime(25), 0));
      cheekOut.KeyFrames.Add(new KeyFrame<float>(FrameToTime(35), 1));
      cheekOut.KeyFrames.Add(new KeyFrame<float>(FrameToTime(65), 1));
      cheekOut.KeyFrames.Add(new KeyFrame<float>(FrameToTime(80), 0));

      var eyeClosed = new SingleKeyFrameAnimation { TargetProperty = "EYE-closed" };
      eyeClosed.KeyFrames.Add(new KeyFrame<float>(FrameToTime(20), 0));
      eyeClosed.KeyFrames.Add(new KeyFrame<float>(FrameToTime(25), 1));
      eyeClosed.KeyFrames.Add(new KeyFrame<float>(FrameToTime(55), 0.2f));
      eyeClosed.KeyFrames.Add(new KeyFrame<float>(FrameToTime(65), 0.2f));
      eyeClosed.KeyFrames.Add(new KeyFrame<float>(FrameToTime(80), 0));
      eyeClosed.KeyFrames.Add(new KeyFrame<float>(FrameToTime(230), 0));
      eyeClosed.KeyFrames.Add(new KeyFrame<float>(FrameToTime(235), 1));
      eyeClosed.KeyFrames.Add(new KeyFrame<float>(FrameToTime(240), 0));

      var mouthE = new SingleKeyFrameAnimation { TargetProperty = "MOUTH-e" };
      mouthE.KeyFrames.Add(new KeyFrame<float>(FrameToTime(115), 0));
      mouthE.KeyFrames.Add(new KeyFrame<float>(FrameToTime(125), 0.7f));
      mouthE.KeyFrames.Add(new KeyFrame<float>(FrameToTime(130), 0.7f));
      mouthE.KeyFrames.Add(new KeyFrame<float>(FrameToTime(140), 0.9f));
      mouthE.KeyFrames.Add(new KeyFrame<float>(FrameToTime(145), 0.9f));
      mouthE.KeyFrames.Add(new KeyFrame<float>(FrameToTime(155), 0.0f));

      var mouthU = new SingleKeyFrameAnimation { TargetProperty = "MOUTH-u" };
      mouthU.KeyFrames.Add(new KeyFrame<float>(FrameToTime(25), 0));
      mouthU.KeyFrames.Add(new KeyFrame<float>(FrameToTime(35), 0.5f));
      mouthU.KeyFrames.Add(new KeyFrame<float>(FrameToTime(55), 0));
      mouthU.KeyFrames.Add(new KeyFrame<float>(FrameToTime(145), 0));
      mouthU.KeyFrames.Add(new KeyFrame<float>(FrameToTime(155), 0.5f));
      mouthU.KeyFrames.Add(new KeyFrame<float>(FrameToTime(160), 0.5f));
      mouthU.KeyFrames.Add(new KeyFrame<float>(FrameToTime(170), 0.9f));
      mouthU.KeyFrames.Add(new KeyFrame<float>(FrameToTime(175), 0.9f));
      mouthU.KeyFrames.Add(new KeyFrame<float>(FrameToTime(170), 0.9f));
      mouthU.KeyFrames.Add(new KeyFrame<float>(FrameToTime(190), 0.0f));

      var mouthSmile = new SingleKeyFrameAnimation { TargetProperty = "MOUTH-smile" };
      mouthSmile.KeyFrames.Add(new KeyFrame<float>(FrameToTime(210), 0));
      mouthSmile.KeyFrames.Add(new KeyFrame<float>(FrameToTime(220), 1));
      mouthSmile.KeyFrames.Add(new KeyFrame<float>(FrameToTime(230), 1));
      mouthSmile.KeyFrames.Add(new KeyFrame<float>(FrameToTime(250), 0));

      // Combine the key frame animations into a single animation.
      var timelineGroup = new TimelineGroup();
      timelineGroup.Add(browMad);
      timelineGroup.Add(browSurp);
      timelineGroup.Add(cheekIn);
      timelineGroup.Add(cheekOut);
      timelineGroup.Add(eyeClosed);
      timelineGroup.Add(mouthE);
      timelineGroup.Add(mouthU);
      timelineGroup.Add(mouthSmile);

      // Make an endless loop.
      return new TimelineClip(timelineGroup)
      {
        Duration = TimeSpan.MaxValue,
        LoopBehavior = LoopBehavior.Cycle
      };
    }


    // Convert frame number to time value.
    private static TimeSpan FrameToTime(double frame)
    {
      const double framesPerSecond = 24;
      return TimeSpan.FromSeconds(frame * 1.0 / framesPerSecond);
    }


    private void CreateGuiControls()
    {
      var panel = SampleFramework.AddOptions("Facial Animation");
      SampleHelper.AddCheckBox(panel, "Play animation", false, PlayAnimation);

      // Add UI controls for morph targets.
      var morphTargetsPanel = SampleHelper.AddGroupBox(panel, "Morph Targets");
      foreach (var morph in _sintel.MorphWeights)
      {
        string name = morph.Key;
        SampleHelper.AddSlider(morphTargetsPanel, name, null, 0, 1, 0, value => _sintel.MorphWeights[name] = value);
      }

      // Add controls for skeleton.
      var skeletonPanel = SampleHelper.AddGroupBox(panel, "Skeleton");
      SampleHelper.AddSlider(skeletonPanel, "Mouth-open", null, 0, 1, 0, value => InterpolatePose(_sintel, _mouthClosedPose, _mouthOpenPose, value));
      SampleHelper.AddCheckBox(skeletonPanel, "Draw skeleton", false, b => _drawSkeleton = b);

      _sliders = panel.GetDescendants().OfType<Slider>().ToArray();

      SampleFramework.ShowOptionsWindow("Facial Animation");
    }


    // Start/stop morphing and skeletal animation.
    private void PlayAnimation(bool play)
    {
      if (play)
      {
        // ----- Start
        _morphingAnimationController = AnimationService.StartAnimation(_morphingAnimation, _sintel.MorphWeights);
        _skeletalAnimationController = AnimationService.StartAnimation(_skeletalAnimation, (IAnimatableProperty)_sintel.SkeletonPose);
      }
      else
      {
        // ----- Stop
        _morphingAnimationController.Stop(TimeSpan.FromSeconds(1));
        _morphingAnimationController.AutoRecycle();

        _skeletalAnimationController.Stop(TimeSpan.FromSeconds(1));
        _skeletalAnimationController.AutoRecycle();
      }

      // Disable UI controls during animation.
      foreach (var slider in _sliders)
        slider.IsEnabled = !play;

      _isPlaying = play;
    }


    // Interpolate between to skeleton animation poses and apply the result to
    // the specified MeshNode.
    private static void InterpolatePose(MeshNode meshNode, SkeletonPose pose0, SkeletonPose pose1, float w)
    {
      var skeletonPose = meshNode.SkeletonPose;
      SkeletonPoseTraits.Instance.Interpolate(ref pose0, ref pose1, w, ref skeletonPose);
    }


    public override void Update(GameTime gameTime)
    {
      // Draw skeleton for debugging.
      _graphicsScreen.DebugRenderer.Clear();
      if (_drawSkeleton)
        _graphicsScreen.DebugRenderer.DrawSkeleton(_sintel, 0.02f, Color.Yellow, true);

      if (_isPlaying)
      {
        // Synchronize UI controls with animation.
        foreach (var slider in _sliders)
        {
          // Note: slider.UserData stores the name of the morph target.
          string name = (string)slider.UserData;
          float weight;
          _sintel.MorphWeights.TryGetValue(name, out weight);
          slider.Value = weight;
        }
      }
    }
  }
}
#endif
