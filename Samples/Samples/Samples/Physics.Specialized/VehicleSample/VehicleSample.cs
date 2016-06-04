#if !ANDROID && !IOS   // Cannot read from vertex buffer in MonoGame/OpenGLES.
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics.Specialized
{
  [Sample(SampleCategory.PhysicsSpecialized,
    @"This sample shows how to implement vehicle physics.",
    @"A controllable car is created using a ray-car method where each wheel is implemented
by a short ray that senses the ground. The car supports suspension with damping, wheel
friction and sliding, etc.",
    50)]
  public class VehicleSample : PhysicsSpecializedSample
  {
    public VehicleSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a game object which loads the test obstacles.
      GameObjectService.Objects.Add(new VehicleLevelObject(Services));

      // Add a game object which controls a vehicle.
      var vehicleObject = new VehicleObject(Services);
      GameObjectService.Objects.Add(vehicleObject);

      // Add a camera that is attached to chassis of the vehicle.
      var vehicleCameraObject = new VehicleCameraObject(vehicleObject.Vehicle.Chassis, Services);
      GameObjectService.Objects.Add(vehicleCameraObject);
      GraphicsScreen.CameraNode = vehicleCameraObject.CameraNode;
    }
  }
}
#endif