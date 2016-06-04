using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to play sounds for collisions, rolling or sliding bodies.",
    "",
    30)]
  public class SoundSample : PhysicsSample
  {
    // Notes:
    // In this sample "bang" sounds are played for collisions. Only a limited number of bang
    // sound effect instances can be active at one time. If there are many collisions in one frame
    // then a single hit sound is played in the average position.
    // A "rolling" sound is played for rolling spheres. Only one sound is played for all rolling
    // contacts. The sound is looped and the sound emitter is at the average rolling contact
    // position.
    // A "scratching" sound is played for sliding objects. Only one sound is played for all 
    // sliding objects.The sound is looped and the sound emitter is at the average sliding contact
    // position.
    //
    // Possible Improvements:
    // - Have several different "bang" sounds and play a random sound for each collision. 
    // - Have different sound for heavy and light collisions.
    // - If there are many active collisions, play a cascade sound (= a sound of several colliding 
    //   objects): Single "bang" sounds can quickly lead to a "machine-gun"-like sound. In such
    //   cases the bangs should be replaced by one more interesting sound.
    // - When a body is grabbed with the mouse and pushed into a static body, the grabbed body
    //   becomes unstable and several bangs are played - this case should be detected and 
    //   repetitive sounds should be avoided.
    // For a good sound design, a lot of more should be done. Good sound design is difficult.

    // The speed with which sound parameters (e.g. volume, pitch) are changed. 
    // (We do not allow instant changes of sound parameters because that would sound unnatural.)
    private const float MaxSoundChangeSpeed = 3f;

    // Only sounds within MaxDistance will be played.
    private const float MaxDistance = 20;

    // Contact forces below MinHitForce do not make a sound.
    private const float MinHitForce = 20000;


    private AudioListener _listener;
    private CameraNode _cameraNode;

    // A "bang" sound for hits.
    private SoundEffect _hitSound;
    // Up to 5 instances can be active.
    private SoundEffectInstance[] _hitSoundInstances = new SoundEffectInstance[5];
    private AudioEmitter[] _hitEmitters = new AudioEmitter[5];
    private float _timeSinceLastHitSound;

    // A scratch sound.
    private SoundEffect _scratchSound;
    private SoundEffectInstance _scratchSoundInstance;
    private AudioEmitter _scratchEmitter;

    // A sound of a rolling object.
    private SoundEffect _rollSound;
    private SoundEffectInstance _rollSoundInstance;
    private AudioEmitter _rollEmitter;


    public SoundSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      InitializePhysics();
      InitializeAudio();
    }


    private void InitializePhysics()
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a ground plane.
      RigidBody groundPlane = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        Name = "GroundPlane",
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(groundPlane);

      // Add walls.
      RigidBody wall0 = new RigidBody(new BoxShape(10, 2, 0.5f))
      {
        Name = "Wall0",
        MotionType = MotionType.Static,
        Pose = new Pose(new Vector3F(0, 1, -5))
      };
      Simulation.RigidBodies.Add(wall0);

      RigidBody wall1 = new RigidBody(new BoxShape(10, 2, 0.5f))
      {
        Name = "Wall1",
        MotionType = MotionType.Static,
        Pose = new Pose(new Vector3F(0, 1, 5))
      };
      Simulation.RigidBodies.Add(wall1);

      RigidBody wall2 = new RigidBody(new BoxShape(0.5f, 2, 10))
      {
        Name = "Wall2",
        MotionType = MotionType.Static,
        Pose = new Pose(new Vector3F(-5, 1, 0))
      };
      Simulation.RigidBodies.Add(wall2);

      RigidBody wall3 = new RigidBody(new BoxShape(0.5f, 2, 10))
      {
        Name = "Wall3",
        MotionType = MotionType.Static,
        Pose = new Pose(new Vector3F(5, 1, 0))
      };
      Simulation.RigidBodies.Add(wall3);

      // Add sphere.
      RigidBody sphere = new RigidBody(new SphereShape(0.4f))
      {
        Name = "Sphere",
        Pose = new Pose(new Vector3F(1, 1, 1)),
      };
      Simulation.RigidBodies.Add(sphere);

      // Add a stack of boxes.
      const int numberOfBoxes = 3;
      const float boxSize = 0.8f;
      BoxShape boxShape = new BoxShape(boxSize, boxSize, boxSize);

      // Optional: Use a small overlap between boxes to improve the stability.
      float overlap = Simulation.Settings.Constraints.AllowedPenetration * 0.5f;
      Vector3F position = new Vector3F(0, boxSize / 2 - overlap, 0);
      for (int i = 0; i < numberOfBoxes; i++)
      {
        RigidBody box = new RigidBody(boxShape)
        {
          Name = "Box" + i,
          Pose = new Pose(position),
        };
        Simulation.RigidBodies.Add(box);
        position.Y += boxSize - overlap;
      }
    }


    private void InitializeAudio()
    {
      // The camera defines the position of the audio listener.
      _listener = new AudioListener();
      _cameraNode = GraphicsScreen.CameraNode;

      // Set a distance scale that is suitable for our demo.
      SoundEffect.DistanceScale = 10;

      // ----- Load sounds, create instances and emitters.
      _hitSound = ContentManager.Load<SoundEffect>("Audio/Hit");
      for (int i = 0; i < _hitSoundInstances.Length; i++)
      {
        _hitSoundInstances[i] = _hitSound.CreateInstance();
        // Change pitch. Our instance sounds better this way.
        _hitSoundInstances[i].Pitch = -1;
        _hitEmitters[i] = new AudioEmitter();
      }

      _scratchSound = ContentManager.Load<SoundEffect>("Audio/Scratch");
      _scratchSoundInstance = _scratchSound.CreateInstance();
      _scratchEmitter = new AudioEmitter();

      _rollSound = ContentManager.Load<SoundEffect>("Audio/Roll");
      _rollSoundInstance = _rollSound.CreateInstance();
      _rollEmitter = new AudioEmitter();

      // The hit sounds are instant sounds. The scratch and rolling sounds are looped.
      _scratchSoundInstance.IsLooped = true;
      _rollSoundInstance.IsLooped = true;
    }


    public override void Update(GameTime gameTime)
    {
      // Size of the current time step.
      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      // Update the position of the audio listener.
      _listener.Forward = (Vector3)_cameraNode.PoseWorld.ToWorldDirection(Vector3F.Forward);
      _listener.Position = (Vector3)_cameraNode.PoseWorld.Position;

      // In the loop below, we investigate the current contacts of the simulation. We collect
      // information about the number of contacts, the accumulated impulse, contact speeds, 
      // and positions.
      int numberOfHits = 0;
      Vector3F hitCenter = Vector3F.Zero;
      float hitForce = 0;

      int numberOfScratchingContacts = 0;
      Vector3F scratchCenter = Vector3F.Zero;
      float scratchSpeed = 0;
      float scratchImpulse = 0;

      int numberOfRollingContacts = 0;
      Vector3F rollCenter = Vector3F.Zero;
      float rollSpeed = 0;

      // Now, loop over all contact constraints of the simulation and collection information.
      for (int i = 0; i < Simulation.ContactConstraints.Count; i++)
      {
        // The ContactConstraint describes the dynamics properties. The Contact describes the
        // geometrical properties (positions, normal vector, etc.).
        var contactConstraint = Simulation.ContactConstraints[i];
        var contact = contactConstraint.Contact;

        // Only consider contacts near the camera.
        if ((contact.Position - _cameraNode.PoseWorld.Position).LengthSquared > MaxDistance * MaxDistance)
          continue;

        // The normal vector of the contact.
        var normal = contact.Normal;
        // The relative velocity of the contact points.
        var relativeVelocity = contactConstraint.BodyA.GetVelocityOfWorldPoint(contact.PositionAWorld) - contactConstraint.BodyB.GetVelocityOfWorldPoint(contact.PositionBWorld);
        // The relative velocity in normal vector direction.
        var relativeNormalVelocity = Vector3F.Dot(relativeVelocity, normal);
        // The relative tangential velocity.
        var relativeTangentVelocity = (relativeVelocity - normal * relativeNormalVelocity).Length;
        // The contact force (force = impulse / time).
        var force = contactConstraint.LinearConstraintImpulse.Length / deltaTime;

        if (contact.Lifetime < deltaTime && Math.Abs(relativeNormalVelocity) > 0.0001f && force > MinHitForce)
        {
          // ----- Colliding Contact:
          // - The contact is less than 1 frame old.
          // - The velocity along the contact normal is significant.
          // - The force is significant.

          numberOfHits++;
          hitCenter += contact.Position;
          hitForce += force;
          continue;
        }

        // The relative angular velocity of the bodies.
        var relativeAngularVelocity = contactConstraint.BodyA.AngularVelocity - contactConstraint.BodyB.AngularVelocity;
        // The rolling velocity is the angular velocity minus any components in normal vector
        // direction. (A rotation around the normal vector is a twist and should not play
        // the rolling sound.)
        var rollingVelocity = (relativeAngularVelocity - Vector3F.ProjectTo(relativeAngularVelocity, normal)).Length;
        if ((contactConstraint.BodyA.Shape is SphereShape || contactConstraint.BodyB.Shape is SphereShape)
            && rollingVelocity > 1)
        {
          // ----- Rolling Contact:
          // We only play the rolling contact for spheres (not for "rolling" boxes) and only
          // if the rolling velocity is significantly greater than 0.

          numberOfRollingContacts++;
          rollCenter += contact.Position;
          rollSpeed += rollingVelocity;
          continue;
        }

        if (Math.Abs(relativeNormalVelocity) < 0.01f && Math.Abs(relativeTangentVelocity) > 1)
        {
          // ----- Scratching Contact:
          // The relative normal velocity is near 0 and the tangential velocity is significantly
          // greater than 0.

          // Ignore "light" contacts.
          if (force < 10)
            continue;

          numberOfScratchingContacts++;
          scratchCenter += contact.Position;
          scratchSpeed += relativeTangentVelocity;
          scratchImpulse += force;
        }
      }

      // We do not play hit sounds too frequently. That would create an unnatural "machine-gun" 
      // sound and we do not want to have too many active sound effect instances.
      _timeSinceLastHitSound += deltaTime;
      if (numberOfHits > 0 && _timeSinceLastHitSound > 0.1f)
      {
        // ----- Play hit sounds.

        // Find a not playing hit sound effect instance.
        int index = -1;
        for (int i = 0; i < _hitSoundInstances.Length; i++)
        {
          if (_hitSoundInstances[i].State != SoundState.Playing)
          {
            index = i;
            break;
          }
        }

        if (index != -1)
        {
          // Set the sound emitter to the average hit position.
          _hitEmitters[index].Position = (Vector3)hitCenter / numberOfHits;

          // Make the volume proportional to the collision force.
          var newVolume = (hitForce - MinHitForce) / (200000 - MinHitForce);
          _hitSoundInstances[index].Volume = Math.Min(newVolume, 0.4f);

          // Play 3D sound.
          _hitSoundInstances[index].Apply3D(_listener, _hitEmitters[index]);
          _hitSoundInstances[index].Play();
          _timeSinceLastHitSound = 0;
        }
      }

      if (numberOfRollingContacts > 0)
      {
        // ----- Play rolling sound.

        // Set the sound emitter to the average rolling contact position.
        var currentPosition = (Vector3F)_rollEmitter.Position;
        var newPosition = rollCenter / numberOfRollingContacts;
        var change = newPosition - currentPosition;
        // If the sound is already playing, then we only change the position gradually.
        if (_rollSoundInstance.State == SoundState.Playing && change.Length > 1f)
          change.Length = 0.1f;
        _rollEmitter.Position = (Vector3)(currentPosition + change);

        // Fade to target volume.
        ChangeVolume(_rollSoundInstance, 0.1f, deltaTime);
        // We set a pitch proportional to the average roll speed.
        ChangePitch(_rollSoundInstance, rollSpeed / numberOfRollingContacts / 20 - 2, deltaTime);


        // Play 3D sound.
        _rollSoundInstance.Apply3D(_listener, _rollEmitter);
        _rollSoundInstance.Play();
      }
      else
      {
        // No rolling contacts --> turn off roll sound.
        ChangeVolume(_rollSoundInstance, 0, deltaTime);
        if (_rollSoundInstance.Volume == 0)
          _rollSoundInstance.Stop();
      }

      if (numberOfScratchingContacts > 0)
      {
        // ----- Play scratching sound.

        var currentPosition = (Vector3F)_scratchEmitter.Position;
        var newPosition = scratchCenter / numberOfScratchingContacts;
        var change = newPosition - currentPosition;
        if (_scratchSoundInstance.State == SoundState.Playing && change.Length > 1f)
          change.Length = 0.1f;
        _scratchEmitter.Position = (Vector3)(currentPosition + change);

        ChangeVolume(_scratchSoundInstance, scratchImpulse / 25000, deltaTime);
        ChangePitch(_scratchSoundInstance, scratchSpeed / numberOfScratchingContacts / 40 - 1f, deltaTime);

        _scratchSoundInstance.Apply3D(_listener, _scratchEmitter);
        _scratchSoundInstance.Play();
      }
      else
      {
        // No scratching --> turn off scratch sound.
        ChangeVolume(_scratchSoundInstance, 0, deltaTime);
        if (_scratchSoundInstance.Volume == 0)
          _scratchSoundInstance.Stop();
      }

      base.Update(gameTime);
    }


    // Continually changes the volume, avoiding sudden changes.
    private void ChangeVolume(SoundEffectInstance sound, float targetVolume, float deltaTime)
    {
      // Limit the volume change to get a fade-in/out effect instead of sudden changes. 
      float change = targetVolume - sound.Volume;
      float changeLimit = MaxSoundChangeSpeed * deltaTime;
      change = MathHelper.Clamp(change, -changeLimit, changeLimit);

      // Set the new volume. The volume value must stay within [0, 1].
      sound.Volume = MathHelper.Clamp(sound.Volume + change, 0, 1);
    }


    // Continually changes the pitch. Similar to ChangeVolume().
    private void ChangePitch(SoundEffectInstance sound, float targetPitch, float deltaTime)
    {
      float change = targetPitch - sound.Pitch;
      float changeLimit = MaxSoundChangeSpeed * deltaTime;
      change = MathHelper.Clamp(change, -changeLimit, changeLimit);

      // Set new pitch value. The pitch must stay within [-1, 1].
      sound.Pitch = MathHelper.Clamp(sound.Volume + change, -1, 1);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Stop all sounds.
        _hitSound = null;
        for (int i = 0; i < _hitSoundInstances.Length; i++)
          _hitSoundInstances[i].Stop();

        _rollSoundInstance.Stop();
        _scratchSoundInstance.Stop();
      }

      base.Dispose(disposing);
    }
  }
}
