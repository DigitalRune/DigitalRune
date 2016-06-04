#if KINECT
using System;
using System.Linq;
using DigitalRune.Animation.Character;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;


namespace Samples.Kinect
{
  // This game component manages the Kinect sensor. Kinect is initialized. Two 
  // player bodies are tracked. 
  // The body data is stored in SkeletonPose instances (SkeletonPoseA and SkeletonPoseB). 
  // These SkeletonPose instances are different from usual SkeletonPose instance: Normally, most 
  // bone transformations contain only rotations and no translations. But the Kinect skeleton 
  // data consists of translations (= joint positions). Rotation info of Kinect is currently
  // not used - it is not required for the motion retargeting.
  public class KinectWrapper : GameComponent
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // Describes a bone in the skeleton pose.
    private struct BoneInfo
    {
      public JointType JointType;
      public string Name;
      public int BoneIndex;
      public int ParentIndex;

      public BoneInfo(JointType jointType, int boneIndex, int parentIndex)
      {
        JointType = jointType;
        Name = jointType.ToString();
        BoneIndex = boneIndex;
        ParentIndex = parentIndex;
      }
    }
    #endregion
    

    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    // We represent the Kinect data with following bone hierarchy:
    private readonly BoneInfo[] Bones =
    {
      new BoneInfo(JointType.SpineBase,      0, -1),
      new BoneInfo(JointType.SpineMid,       1,  0),
      new BoneInfo(JointType.SpineShoulder,  2,  1),
      new BoneInfo(JointType.Neck,           3,  2),
      new BoneInfo(JointType.Head,           4,  3),
      new BoneInfo(JointType.ShoulderLeft,   5,  2),
      new BoneInfo(JointType.ElbowLeft,      6,  5),
      new BoneInfo(JointType.WristLeft,      7,  6),
      new BoneInfo(JointType.HandLeft,       8,  7),
      new BoneInfo(JointType.HandTipLeft,    9,  8),
      new BoneInfo(JointType.ThumbLeft,     10,  7),
      new BoneInfo(JointType.ShoulderRight, 11,  2),
      new BoneInfo(JointType.ElbowRight,    12, 11),
      new BoneInfo(JointType.WristRight,    13, 12),
      new BoneInfo(JointType.HandRight,     14, 13),
      new BoneInfo(JointType.HandTipRight,  15, 14),
      new BoneInfo(JointType.ThumbRight,    16, 13),
      new BoneInfo(JointType.HipLeft,       17,  0),
      new BoneInfo(JointType.KneeLeft,      18, 17),
      new BoneInfo(JointType.AnkleLeft,     19, 18),
      new BoneInfo(JointType.FootLeft,      20, 19),
      new BoneInfo(JointType.HipRight,      21, 0 ),
      new BoneInfo(JointType.KneeRight,     22, 21),
      new BoneInfo(JointType.AnkleRight,    23, 22),
      new BoneInfo(JointType.FootRight,     24, 23),
    };
    #endregion
    


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Microsoft.Kinect types:
    private KinectSensor _kinectSensor;
    private BodyFrameReader _bodyFrameReader;
    private Body[] _bodies;
    private Body _bodyA;
    private Body _bodyB;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    // Is the Kinect sensor running?
    public bool IsRunning
    {
      get { return _kinectSensor != null && _kinectSensor.IsAvailable; }
    }


    // Is the first player currently being tracked?
    public bool IsTrackedA
    {
      get { return _bodyA != null && _bodyA.IsTracked; }
    }


    // Is the second player currently being tracked?
    public bool IsTrackedB
    {
      get { return _bodyB != null && _bodyB.IsTracked; }
    }


    // The SkeletonPose that stores Kinect data for the first player.
    public SkeletonPose SkeletonPoseA { get; private set; }


    // The SkeletonPose that stores Kinect data for the second player.
    public SkeletonPose SkeletonPoseB { get; private set; }


    // An offset that is applied to the Kinect joint positions.
    public Vector3F Offset { get; set; }


    // A scale factor that is applied to the Kinect joint positions.
    public Vector3F Scale { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public KinectWrapper(Game game)
      : base(game)
    {
      // one sensor is currently supported
      _kinectSensor = KinectSensor.GetDefault();

      // open the reader for the body frames
      _bodyFrameReader = _kinectSensor.BodyFrameSource.OpenReader();

      // open the sensor
      _kinectSensor.Open();

      _bodyFrameReader.FrameArrived += OnKinectBodyFrameArrived;

      Offset = new Vector3F(0, 0.3f, 0);
      Scale = new Vector3F(1, 1, 1);

      // Create a skeleton that defines the bone hierarchy and rest pose.
      var skeleton = new Skeleton(
        Bones.Select(b => b.ParentIndex).ToArray(),                         // Parent indices
        Bones.Select(b => b.Name).ToArray(),                                // Bone names
        Enumerable.Repeat(SrtTransform.Identity, Bones.Length).ToArray());  // Bind poses = all Identity transforms

      // Create a SkeletonPose for each player. 
      SkeletonPoseA = SkeletonPose.Create(skeleton);
      SkeletonPoseB = SkeletonPose.Create(skeleton);
    }


    protected override void Dispose(bool disposing)
    {
      // Clean up.
      try
      {
        if (_bodyFrameReader != null)
        {
          _bodyFrameReader.Dispose();
          _bodyFrameReader = null;
        }

        if (_kinectSensor != null)
        {
          _kinectSensor.Close();
          _kinectSensor = null;
        }
      }
      catch (Exception)
      {
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Called when Kinect has new body data.
    private void OnKinectBodyFrameArrived(object sender, BodyFrameArrivedEventArgs eventArgs)
    {
      bool dataReceived = false;

      using (var bodyFrame = eventArgs.FrameReference.AcquireFrame())
      {
        if (bodyFrame != null)
        {
          if (_bodies == null)
            _bodies = new Body[bodyFrame.BodyCount];

          // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
          // As long as those body objects are not disposed and not set to null in the array,
          // those body objects will be re-used.
          bodyFrame.GetAndRefreshBodyData(_bodies);
          dataReceived = true;
        }
      }

      if (dataReceived)
      {
        // Get a tracked body for player A.
        if (_bodyA == null || !_bodyA.IsTracked)
        {
          foreach(var body in _bodies)
          {
            if (body != null && body.IsTracked && body != _bodyB)
            {
              _bodyA = body;
              break;
            }
          }
        }

        // Get a tracked body for player A.
        if (_bodyB == null || !_bodyB.IsTracked)
        {
          foreach (var body in _bodies)
          {
            if (body != null && body.IsTracked && body != _bodyA)
            {
              _bodyB = body;
              break;
            }
          }
        }

        // Update the SkeletonPose from the Kinect body data.
        UpdateKinectSkeletonPose(_bodyA, SkeletonPoseA);
        UpdateKinectSkeletonPose(_bodyB, SkeletonPoseB);
      }
    }


    // Update the skeleton pose using the data from Kinect. 
    private void UpdateKinectSkeletonPose(Body body, SkeletonPose skeletonPose)
    {
      if (body == null || !body.IsTracked)
        return;

      for (int i = 0; i < skeletonPose.Skeleton.NumberOfBones; i++)
      {
        var joint = Bones[i].JointType;
        if (body.Joints[joint].TrackingState != TrackingState.NotTracked)
        {
          // The joint position in "Kinect space".
          var kinectPosition = body.Joints[joint].Position;

          // Convert Kinect joint position to a Vector3F.
          // z is negated because in XNA the camera forward vectors is -z, but the Kinect
          // forward vector is +z. 
          Vector3F position = new Vector3F(kinectPosition.X, kinectPosition.Y, -kinectPosition.Z);

          // Apply scale and offset.
          position = position * Scale + Offset;

          var orientation = QuaternionF.Identity;   // TODO: Use orientations of body.JointOrientations.

          skeletonPose.SetBonePoseAbsolute(i, new SrtTransform(orientation, position));
        }
      }
    }
    #endregion
  }
}
#endif