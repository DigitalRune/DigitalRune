using DigitalRune.Animation.Character;
using DigitalRune.Mathematics.Algebra;


namespace Samples.Kinect
{
  // This class filters the bone rotations of a SkeletonPose using a first order low-pass filter.
  // This removes jitter in the bone animations. The strength of the filter effect depends
  // on TimeConstant. Larger values make the animations smoother but also less responsive. Large
  // TimeConstant values can introduce a significant lag and make the animation unresponsive.
  // The used filter is a first order IIR filter.
  //
  // The filter is associated with a specific SkeletonPose. If Update() is called, the 
  // current bone rotations are updated to filtered values.
  internal class SkeletonPoseFilter
  {
    // Two arrays with 4 entries (= 4 quaternion components) per bone.
    private float[] _newValues;
    private float[] _filteredValues;
    
    
    // The filtered SkeletonPose.
    public SkeletonPose SkeletonPose { get; private set; }


    // The time constant (in seconds). The default is 0.05 s.
    public float TimeConstant { get; set; }


    public SkeletonPoseFilter(SkeletonPose skeletonPose)
    {
      SkeletonPose = skeletonPose;      
      TimeConstant = 0.05f;
    }


    // Updates the bone rotations of SkeletonPose and sets them to filtered values.
    public void Update(float deltaTime)
    {
      int numberOfBones = SkeletonPose.Skeleton.NumberOfBones;

      if (_filteredValues == null)
      {
        // First time initialization.
        _newValues = new float[numberOfBones * 4];
        _filteredValues = new float[numberOfBones * 4];
        SkeletonPoseToArray(_filteredValues);
      }
      else
      {
        // Copy current bone rotations to array.
        SkeletonPoseToArray(_newValues);

        // Average the old values and the current values. 
        // See http://en.wikipedia.org/wiki/Low-pass_filter for an explanation.
        float weight1 = deltaTime / (deltaTime + TimeConstant);
        float weight2 = 1 - weight1;
        for (int i = 0; i < numberOfBones * 4; i++)
          _filteredValues[i] = _newValues[i] * weight1 + _filteredValues[i] * weight2;

        ArrayToSkeletonPose(_filteredValues); 
      }      
    }


    // Copies the quaternions of all bones to the specified array.
    private void SkeletonPoseToArray(float[] values)
    {
      var numberOfBones = SkeletonPose.Skeleton.NumberOfBones;
      for (int i = 0; i < numberOfBones; i++)
      {
        QuaternionF quaternion = SkeletonPose.GetBoneTransform(i).Rotation;
        values[i * 4 + 0] = quaternion.W;
        values[i * 4 + 1] = quaternion.X;
        values[i * 4 + 2] = quaternion.Y;
        values[i * 4 + 3] = quaternion.Z;
      }
    }


    // Initializes the bone rotations using the quaternions of the specified array.
    private void ArrayToSkeletonPose(float[] values)
    {
      var numberOfBones = SkeletonPose.Skeleton.NumberOfBones;
      for (int i = 0; i < numberOfBones; i++)
      {
        QuaternionF quaternion = new QuaternionF(
          values[i * 4 + 0],
          values[i * 4 + 1],
          values[i * 4 + 2],
          values[i * 4 + 3]);

        // The quaternions were filtered using component-wise linear interpolation. This
        // is only an approximation which denormalizes the quaternions. 
        // --> Renormalize the quaternions.
        quaternion.TryNormalize();

        // Exchange the rotation in the bone transform.
        var boneTransform = SkeletonPose.GetBoneTransform(i);
        boneTransform.Rotation = quaternion;
        SkeletonPose.SetBoneTransform(i, boneTransform);
      }
    }
  }
}
