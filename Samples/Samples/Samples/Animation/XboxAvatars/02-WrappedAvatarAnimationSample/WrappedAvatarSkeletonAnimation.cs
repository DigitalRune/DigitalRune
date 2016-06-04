#if XBOX
using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Animation.Traits;
using Microsoft.Xna.Framework.GamerServices;


namespace Samples.Animation
{
  // An animation that wraps an XNA AvatarAnimation to make it compatible with the
  // DigitalRune Animation system.
  public class WrappedAvatarSkeletonAnimation : Animation<SkeletonPose>
  {
    private readonly AvatarAnimation _avatarAnimation;


    // Traits tell the AnimationSystem how to create/recycle/add/interpolate animation values.
    public override IAnimationValueTraits<SkeletonPose> Traits
    {
      get { return SkeletonPoseTraits.Instance; }
    }


    public override TimeSpan GetTotalDuration()
    {
      return _avatarAnimation.Length;
    }


    public WrappedAvatarSkeletonAnimation(AvatarAnimation avatarAnimation)
    {
      if (avatarAnimation == null)
        throw new ArgumentNullException("avatarAnimation");

      _avatarAnimation = avatarAnimation;

      // Per default, this animation animates the SkeletonPose property of the AvatarPose class.
      TargetProperty = "SkeletonPose";
    }


    protected override void GetValueCore(TimeSpan time, ref SkeletonPose defaultSource, ref SkeletonPose defaultTarget, ref SkeletonPose result)
    {
      // Update time of the AvatarAnimation.
      _avatarAnimation.CurrentPosition = time;

      // Get bone transforms and convert to type SrtTransform.
      for (int i = 0; i < AvatarRenderer.BoneCount; i++)
      {
        result.SetBoneTransform(i, SrtTransform.FromMatrix(_avatarAnimation.BoneTransforms[i]));
      }
    }
  }
}
#endif