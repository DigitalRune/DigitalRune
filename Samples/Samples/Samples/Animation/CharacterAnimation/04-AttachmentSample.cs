using System;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    "This sample shows how to attach a weapon model to the hand bone of the marine model.",
    "",
    54)]
  public class AttachmentSample : CharacterAnimationSample
  {
    private readonly MeshNode _meshNode;

    // The model that will be attached to a bone.
    private readonly ModelNode _weaponModelNode;


    public AttachmentSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var modelNode = ContentManager.Load<ModelNode>("Marine/PlayerMarine");
      _meshNode = modelNode.GetSubtree().OfType<MeshNode>().First().Clone();
      SampleHelper.EnablePerPixelLighting(_meshNode);
      GraphicsScreen.Scene.Children.Add(_meshNode);

      // Play a looping 'Idle' animation.
      var animations = _meshNode.Mesh.Animations;
      var idleAnimation = animations["Idle"];
      var loopingAnimation = new AnimationClip<SkeletonPose>(idleAnimation)
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };
      var animationController = AnimationService.StartAnimation(loopingAnimation, (IAnimatableProperty)_meshNode.SkeletonPose);
      animationController.UpdateAndApply();
      animationController.AutoRecycle();

      // Add weapon model to the scene graph under the node of the marine mesh.
      _weaponModelNode = ContentManager.Load<ModelNode>("Marine/Weapon/WeaponMachineGun").Clone();
      _meshNode.Children = new SceneNodeCollection();
      _meshNode.Children.Add(_weaponModelNode);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // Update pose of weapon.
      // The offset of the weapon origin to the hand bone origin (in bone space)
      Pose offset = Pose.Identity;
      // Get hand bone index.
      int handBoneIndex = _meshNode.Mesh.Skeleton.GetIndex("R_Hand2");
      // The hand bone position in model space
      var bonePose = _meshNode.SkeletonPose.GetBonePoseAbsolute(handBoneIndex);

      // Update SceneNode.LastPoseWorld (required for optional effects, like motion blur).
      _weaponModelNode.SetLastPose(true);

      // The weapon model node is a child of the marine mesh node. That means,
      // we can change _weaponModelNode.PoseWorld to set the pose in world space
      // or _weaponModelNode.PoseLocal to set the pose relative to mesh:
      _weaponModelNode.PoseLocal = (Pose)bonePose * offset;
    }
  }
}
