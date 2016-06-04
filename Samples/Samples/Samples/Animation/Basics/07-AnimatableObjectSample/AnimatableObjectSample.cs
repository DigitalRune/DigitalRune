using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This samples shows how to use IAnimatableObjects.",
    @"See MyAnimatableSprite.cs to see how IAnimatableObject can be implemented.
MyAnimatableSprite has two animatable properties: Position and Color.
3 sprites are displayed.
The color of the second sprite is animated.
The color and the position of the third sprite are animated.",
    7)]
  public class AnimatableObjectSample : AnimationSample
  {
    private readonly AnimatableSprite _animatableSpriteA;
    private readonly AnimatableSprite _animatableSpriteB;
    private readonly AnimatableSprite _animatableSpriteC;


    public AnimatableObjectSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      Rectangle bounds = GraphicsService.GraphicsDevice.Viewport.TitleSafeArea;

      // ----- Create three named AnimatableSprite instances.
      _animatableSpriteA = new AnimatableSprite("SpriteA", SpriteBatch, Logo)
      {
        Position = new Vector2(bounds.Center.X, bounds.Center.Y / 2.0f),
        Color = Color.Red,
      };

      _animatableSpriteB = new AnimatableSprite("SpriteB", SpriteBatch, Logo)
      {
        Position = new Vector2(bounds.Center.X, bounds.Center.Y),
        Color = Color.Green,
      };

      _animatableSpriteC = new AnimatableSprite("SpriteC", SpriteBatch, Logo)
      {
        Position = new Vector2(bounds.Center.X, 3.0f * bounds.Center.Y / 2.0f),
        Color = Color.Blue,
      };

      // Create a looping color key-frame animation.
      ColorKeyFrameAnimation colorAnimation = new ColorKeyFrameAnimation
      {
        TargetProperty = "Color",
        EnableInterpolation = true,
      };
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(0.0), Color.Red));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(0.5), Color.Orange));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(1.0), Color.Yellow));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(1.5), Color.Green));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(2.0), Color.Blue));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(2.5), Color.Indigo));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(3.0), Color.Violet));
      colorAnimation.KeyFrames.Add(new KeyFrame<Color>(TimeSpan.FromSeconds(3.5), Color.Red));
      AnimationClip<Color> loopedColorAnimation = new AnimationClip<Color>(colorAnimation)
      {
        TargetObject = "SpriteB",
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };

      // Create a oscillating from/to animation for the position.
      Vector2FromToByAnimation vector2Animation = new Vector2FromToByAnimation
      {
        TargetProperty = "Position",
        From = new Vector2(bounds.Left + 100, _animatableSpriteC.Position.Y),
        To = new Vector2(bounds.Right - 100, _animatableSpriteC.Position.Y),
        Duration = TimeSpan.FromSeconds(2),
        EasingFunction = new HermiteEase { Mode = EasingMode.EaseInOut },
      };
      AnimationClip<Vector2> oscillatingVector2Animation = new AnimationClip<Vector2>(vector2Animation)
      {
        LoopBehavior = LoopBehavior.Oscillate,
        Duration = TimeSpan.MaxValue,
      };

      // Create a timeline group that contains both animations.
      TimelineGroup spriteCAnimation = new TimelineGroup { TargetObject = "SpriteC", };
      spriteCAnimation.Add(loopedColorAnimation);
      spriteCAnimation.Add(oscillatingVector2Animation);

      // Create a timeline group that contains the animations for SpriteB and SpriteC.
      TimelineGroup allSpriteAnimations = new TimelineGroup();
      allSpriteAnimations.Add(loopedColorAnimation);
      allSpriteAnimations.Add(spriteCAnimation);

      // There are several ways to apply animations to animatable objects and properties:
      //
      // Method 1: Apply to an IAnimatableProperty directly.
      // We either have direct access to a IAnimatableProperty or we can ask the IAnimatableObject
      // to give us a named IAnimatableProperty. 
      // For example:
      //     AnimationService.StartAnimation(loopedColorAnimation, _animatableSpriteB.GetAnimatableProperty<Color>("Color"));
      //     AnimationService.StartAnimation(loopedColorAnimation, _animatableSpriteC.GetAnimatableProperty<Color>("Color"));
      //     AnimationService.StartAnimation(oscillatingLeftRightAnimation, _animatableSpriteC.GetAnimatableProperty<Vector2>("Position"));
      // In this case, the "TargetObject" and the "TargetProperty" values of the timelines/animations do not matter. 
      //
      // Method 2: Apply to an IAnimatableObject directly.
      // For example: 
      //     AnimationService.StartAnimation(loopedColorAnimation, _animatableSpriteB);
      //     AnimationService.StartAnimation(spriteCAnimation, _animatableSpriteC);
      // The animation service checks the TargetProperty value of the animations to see which
      // IAnimatableProperty of the IAnimatableObjects should be animated by the animation.
      // The "TargetObject" values of the timelines are ignored.
      // 
      // Method 3: Apply a timeline to a collection of IAnimatableObjects.
      // For example: 
      var animationController = AnimationService.StartAnimation(allSpriteAnimations, new[] { _animatableSpriteA, _animatableSpriteB, _animatableSpriteC });
      // The "TargetObject" and "TargetProperty" values are used to check which objects and
      // properties must be animated by which animation.
      // combinedSpriteAnimation is a "tree" of timelines:
      // 
      //                                  Type                         TargetObject   TargetProperty
      // --------------------------------------------------------------------------------------------------
      // allSpriteAnimations              TimelineGroup                -              -
      //   loopedColorAnimation           AnimationClip<Color>         "SpriteB"      -
      //     colorAnimation               ColorKeyFrameAnimation       -              "Color"
      //   spriteCAnimation               TimelineGroup                "SpriteC"      -
      //     loopedColorAnimation         AnimationClip<Color>         "SpriteB"      - 
      //       colorAnimation             ColorKeyFrameAnimation       -             "Color"
      //     oscillatingVector2Animation  AnimationClip<Vector2>       -             -
      //       vector2Animation           FromToByAnimation            -             "Position"
      //
      // No animation specifies "SpriteA" as its TargetObject, therefore no animation
      // are applied to SpriteA.
      //
      // The first loopedColorAnimation is applied to "SpriteB". And the contained 
      // colorAnimation is applied to the Color property of SpriteB.
      //
      // The spriteCAnimation is applied to "SpriteC". Therefore, all "children" of 
      // spriteCAnimation are also applied to this object! The TargetObject property of
      // the second loopedColorAnimation is ignored! The second loopedColorAnimation is 
      // applied to SpriteC!

      animationController.UpdateAndApply();
    }


    protected override void OnRender(RenderContext context)
    {
      GraphicsService.GraphicsDevice.Clear(Color.White);

      _animatableSpriteA.Draw();
      _animatableSpriteB.Draw();
      _animatableSpriteC.Draw();
    }
  }
}
