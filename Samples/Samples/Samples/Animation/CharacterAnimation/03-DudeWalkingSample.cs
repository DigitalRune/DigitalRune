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
    "This sample shows how to play a looping walk animation.",
    "",
    53)]
  [Controls(@"Sample
  Press <Up>/<Down> to increase/decrease the animation speed.")]
  public class DudeWalkingSample : CharacterAnimationSample
  {
    private AnimationController _animationController;


    public DudeWalkingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var modelNode = ContentManager.Load<ModelNode>("Dude/Dude");
      var meshNode = modelNode.GetSubtree().OfType<MeshNode>().First().Clone();
      meshNode.PoseLocal = new Pose(new Vector3F(0, 0, 0), Matrix33F.CreateRotationY(ConstantsF.Pi));
      SampleHelper.EnablePerPixelLighting(meshNode);
      GraphicsScreen.Scene.Children.Add(meshNode);

      // The imported animations are stored in the mesh.
      Dictionary<string, SkeletonKeyFrameAnimation> animations = meshNode.Mesh.Animations;

      // The Dude model contains only one animation, which is a SkeletonKeyFrameAnimation with 
      // a walk cycle.
      SkeletonKeyFrameAnimation walkAnimation = animations.Values.First();

      // Wrap the walk animation in an animation clip that loops the animation forever.
      AnimationClip<SkeletonPose> loopingAnimation = new AnimationClip<SkeletonPose>(walkAnimation)
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };

      // Start the animation and keep the created AnimationController.
      // We must cast the SkeletonPose to IAnimatableProperty because SkeletonPose implements
      // IAnimatableObject and IAnimatableProperty. We must tell the AnimationService if we want
      // to animate an animatable property of the SkeletonPose (IAnimatableObject), or if we want to
      // animate the whole SkeletonPose (IAnimatableProperty).
      _animationController = AnimationService.StartAnimation(loopingAnimation, (IAnimatableProperty)meshNode.SkeletonPose);

      // The animation will be applied the next time AnimationManager.ApplyAnimations() is called
      // in the main loop. ApplyAnimations() is called before this method is called, therefore
      // the model will be rendered in the bind pose in this frame and in the first animation key
      // frame in the next frame - this creates an annoying visual popping effect. 
      // We can avoid this if we call AnimationController.UpdateAndApply(). This will immediately
      // change the model pose to the first key frame pose.
      _animationController.UpdateAndApply();

      // (Optional) Enable Auto-Recycling: 
      // After the animation is stopped, the animation service will recycle all
      // intermediate data structures. 
      _animationController.AutoRecycle();
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Clean up.
        _animationController.Stop();
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // Increase animation speed when <Up> is pressed.
      if (InputService.IsPressed(Keys.Up, true))
        _animationController.Speed += 0.2f;

      // Decrease animation speed when <Down> is pressed.
      if (InputService.IsPressed(Keys.Down, true))
        _animationController.Speed -= 0.2f;

      // Clamp the speed to 0 if negative.
      if (_animationController.Speed < 0)
        _animationController.Speed = 0;
    }
  }
}
