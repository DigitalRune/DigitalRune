using System;
using DigitalRune.Animation.Character;
using DigitalRune.Mathematics.Algebra;


namespace Samples.Animation
{
  // A bone-based jiggle-physics effect.
  // This calls controls a single bone of a SkeletonPose and makes it jiggle.
  public class BoneJiggler
  {
    // How it works:
    // The bone jiggler tracks a "fixed point" on the bone determined by Offset.
    // A particle is connected to this bone using a spring. 
    // The bone orientation is then computed between the "bone origin to fixed point" and the
    // "bone origin to particle" vectors.


    // The fixed point on the bone in model space.
    private Vector3F _fixedPointPosition;
    private Vector3F _fixedPointVelocity;

    // The particle connected to the fixed point (in model space).
    private Vector3F _particlePosition;
    private Vector3F _particleVelocity;


    public SkeletonPose SkeletonPose { get; private set; }

    public int BoneIndex { get; private set; }

    // The offset of the "fixed point" in bone space.
    public Vector3F Offset { get; set; }

    // Spring and damping strengths.
    public float Spring { get; set; }
    public float Damping { get; set; }


    public BoneJiggler(SkeletonPose skeletonPose, int boneIndex, Vector3F offset)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");
      if (boneIndex < 0 || boneIndex > skeletonPose.Skeleton.NumberOfBones)
        throw new ArgumentOutOfRangeException("boneIndex");
      if (offset.IsNumericallyZero)
        throw new ArgumentException("Parameter offset must not be a zero vector.");

      SkeletonPose = skeletonPose;

      BoneIndex = boneIndex;
      Offset = offset;

      Spring = 100f;
      Damping = 1f;

      Reset();
    }


    public void Reset()
    {
      _fixedPointPosition = new Vector3F(float.NaN);
      _fixedPointVelocity = new Vector3F(float.NaN);

      _particlePosition = new Vector3F(float.NaN);
      _particleVelocity = new Vector3F(float.NaN);
    }


    public void Update(float deltaTime, Matrix44F world)
    {
      if (deltaTime <= 0)
        return;

      // Reset bone transform.
      SkeletonPose.SetBoneTransform(BoneIndex, SrtTransform.Identity);

      // Get new fixed point position in world space.
      var bonePoseAbsolute = SkeletonPose.GetBonePoseAbsolute(BoneIndex);
      var bonePoseWorld = world * bonePoseAbsolute;
      var fixedPointPosition = bonePoseWorld.TransformPosition(Offset);

      // If we haven't set the fixed point position before, then store the position 
      // and we are done.
      if (_fixedPointPosition.IsNaN)
      {
        _fixedPointPosition = fixedPointPosition;
        return;
      }

      // New position and velocity of fixed point.
      _fixedPointVelocity = (fixedPointPosition - _fixedPointPosition) / deltaTime;
      _fixedPointPosition = fixedPointPosition;

      // If the particle position was not set before, then we only store the current values.
      // The real work starts in the next frame.
      if (_particlePosition.IsNaN)
      {
        _particlePosition = _fixedPointPosition;
        _particleVelocity = _fixedPointVelocity;
        return;
      }

      // Compute the spring force between the particle and the fixed point.
      var force = Spring * (_fixedPointPosition - _particlePosition) + Damping * (_fixedPointVelocity - _particleVelocity);

      // Update velocity and position of the particle using symplectic Euler.
      _particleVelocity = _particleVelocity + force * deltaTime;
      _particlePosition = _particlePosition + _particleVelocity * deltaTime;

      // Convert particle position back to bone space.
      var particleLocal = bonePoseWorld.Inverse.TransformPosition(_particlePosition);

      // Create rotation between the fixed point vector and the particle vector.
      var boneTransform = new SrtTransform(QuaternionF.CreateRotation(Offset, particleLocal));
      SkeletonPose.SetBoneTransform(BoneIndex, boneTransform);
    }
  }
}
