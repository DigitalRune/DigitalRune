#if XBOX
using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Traits;
using Microsoft.Xna.Framework.GamerServices;


namespace Samples.Animation
{
  // An animation that wraps an XNA AvatarAnimation to make it compatible with the 
  // DigitalRune Animation system.
  public class WrappedAvatarExpressionAnimation : Animation<AvatarExpression>
  {
    private readonly AvatarAnimation _avatarAnimation;


    // Traits tell the animation system how to create/recycle/add/interpolate animation values.
    public override IAnimationValueTraits<AvatarExpression> Traits
    {
      get { return AvatarExpressionTraits.Instance; }
    }


    public override TimeSpan GetTotalDuration()
    {
      return _avatarAnimation.Length;
    }


    public WrappedAvatarExpressionAnimation(AvatarAnimation avatarAnimation)
    {
      if (avatarAnimation == null)
        throw new ArgumentNullException("avatarAnimation");

      _avatarAnimation = avatarAnimation;

      // Per default, this animation animates the Expression property of the AvatarPose class.
      TargetProperty = "Expression";
    }


    protected override void GetValueCore(TimeSpan time, ref AvatarExpression defaultSource, ref AvatarExpression defaultTarget, ref AvatarExpression result)
    {
      // Update time of the AvatarAnimation and return the expression.
      _avatarAnimation.CurrentPosition = time;
      result = _avatarAnimation.Expression;
    }
  }
}
#endif