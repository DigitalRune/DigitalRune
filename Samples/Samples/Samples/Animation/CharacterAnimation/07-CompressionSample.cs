using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample shows how to compress a SkeletonKeyFrameAnimation.",
    @"The uncompressed and the compressed animations are played on two models side-by-side to
visually compare the compression results.",
    57)]
  [Controls(@"Sample
  Press <Up> or <Down> to increase or decrease the allowed compression error.")]
  public class CompressionSample : CharacterAnimationSample
  {
    private readonly MeshNode _meshNodeUncompressed;
    private readonly MeshNode _meshNodeCompressed;

    private float _rotationCompressionThreshold = 0.01f;
    private readonly SkeletonKeyFrameAnimation _animation;
    private float _removedKeyFrames;


    public CompressionSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var modelNode = ContentManager.Load<ModelNode>("Dude/Dude");
      SampleHelper.EnablePerPixelLighting(modelNode);

      _meshNodeUncompressed = modelNode.GetSubtree().OfType<MeshNode>().First().Clone();
      _meshNodeUncompressed.PoseLocal = new Pose(new Vector3F(-0.5f, 0, 0), Matrix33F.CreateRotationY(ConstantsF.Pi));
      GraphicsScreen.Scene.Children.Add(_meshNodeUncompressed);

      _meshNodeCompressed = _meshNodeUncompressed.Clone();
      _meshNodeCompressed.PoseLocal = new Pose(new Vector3F(0.5f, 0, 0), Matrix33F.CreateRotationY(ConstantsF.Pi));
      GraphicsScreen.Scene.Children.Add(_meshNodeCompressed);

      Dictionary<string, SkeletonKeyFrameAnimation> animations = _meshNodeUncompressed.Mesh.Animations;
      _animation = animations.Values.First();
      
      RestartAnimations();
    }


    private void RestartAnimations()
    {
      // Start original animation on one model.
      var loopingAnimation = new AnimationClip<SkeletonPose>(_animation)
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };
      AnimationService.StartAnimation(loopingAnimation, (IAnimatableProperty)_meshNodeUncompressed.SkeletonPose);

      // Clone the original animation.
      var animationCompressed = _animation.Clone();

      // Compress animation. This removes key frames that can be computed from neighboring frames.
      // This animation is lossy and the parameters define the allowed error.
      _removedKeyFrames = animationCompressed.Compress(0.01f, _rotationCompressionThreshold, 0.001f);

      // Finalize the SkeletonKeyFrameAnimation. 
      // (This must be called to optimize the internal data structures.)
      animationCompressed.Freeze();

      // Start compressed animation on the other model.
      var loopingAnimationCompressed = new AnimationClip<SkeletonPose>(animationCompressed)
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };
      AnimationService.StartAnimation(loopingAnimationCompressed, (IAnimatableProperty)_meshNodeCompressed.SkeletonPose);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <Up>/<Down> --> Increase/Decrease rotation threshold and recreate animations.
      if (InputService.IsPressed(Keys.Up, true))
      {
        _rotationCompressionThreshold += 0.01f;
        RestartAnimations();
      }
      if (InputService.IsPressed(Keys.Down, true))
      {
        _rotationCompressionThreshold -= 0.01f;
        if (_rotationCompressionThreshold < 0)
          _rotationCompressionThreshold = 0;

        RestartAnimations();
      }

      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      debugRenderer.DrawText(
        "\n\nPress <Up> or <Down> to increase or decrease the allowed compression error.\n"
        + "Rotation Compression Threshold [°]: " + _rotationCompressionThreshold + "\n"
        + "Compression: " + _removedKeyFrames * 100 + "%");
    }
  }
}
