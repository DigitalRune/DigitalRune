using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    "This sample renders many models with independent animations to test performance.",
    "",
    56)]
  [Controls(@"Sample
  Press <Space> to start next animation.")]
  public class StressTestSample : CharacterAnimationSample
  {
#if WINDOWS_PHONE || ANDROID || IOS
    private const int NumberOfModels = 20;
#else
    private const int NumberOfModels = 100;
#endif
    private readonly MeshNode[] _meshNodes;
    private readonly ITimeline[] _animations;
    private int _currentAnimationIndex;


    public StressTestSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Load model.
      var modelNode = ContentManager.Load<ModelNode>("Marine/PlayerMarine");
      var meshNode = modelNode.GetSubtree().OfType<MeshNode>().First();
      SampleHelper.EnablePerPixelLighting(meshNode);

      // Create looping animations for all imported animations.
      Dictionary<string, SkeletonKeyFrameAnimation> animations = meshNode.Mesh.Animations;
      _animations = new ITimeline[animations.Count];
      int index = 0;
      foreach (var animation in animations.Values)
      {
        _animations[index] = new AnimationClip<SkeletonPose>(animation)
        {
          LoopBehavior = LoopBehavior.Cycle,
          Duration = TimeSpan.MaxValue,
        };
        index++;
      }

      // Create a lot of clones of the mesh node and start a new animation on each instance.
      _meshNodes = new MeshNode[NumberOfModels];
      for (int i = 0; i < NumberOfModels; i++)
      {
        var rowLength = (int)Math.Sqrt(NumberOfModels);
        var x = (i % rowLength) - rowLength / 2;
        var z = -i / rowLength;
        var position = new Vector3F(x, 0, z) * 1.5f;

        _meshNodes[i] = meshNode.Clone();
        _meshNodes[i].PoseWorld = new Pose(position);
        GraphicsScreen.Scene.Children.Add(_meshNodes[i]);
        AnimationService.StartAnimation(_animations[0], (IAnimatableProperty)_meshNodes[i].SkeletonPose)
                        .AutoRecycle();
      }
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <Space> --> Cross-fade to next animation.
      if (InputService.IsPressed(Keys.Space, true))
      {
        _currentAnimationIndex++;
        if (_currentAnimationIndex >= _animations.Length)
          _currentAnimationIndex = 0;

        for (int i = 0; i < NumberOfModels; i++)
        {
          // Start a next animation using a Replace transition with a fade-in time.
          AnimationService.StartAnimation(
            _animations[_currentAnimationIndex],
            (IAnimatableProperty<SkeletonPose>)_meshNodes[i].SkeletonPose,
            AnimationTransitions.Replace(TimeSpan.FromSeconds(0.2)))
            .AutoRecycle();
        }
      }

      // Side note:
      // SkeletonPose.Update() is a method that can be called to update all internal skeleton pose
      // data immediately. If SkeletonPose.Update() is not called, the internal data will
      // be updated when needed - for example, when SkeletonPose.SkinningMatricesXna are accessed.
      //for (int i = 0; i < NumberOfModels; i++)
      //  _meshNodes[i].SkeletonPose.Update();
    }
  }
}
