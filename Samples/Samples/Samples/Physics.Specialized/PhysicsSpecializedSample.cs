using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Physics
{
  // The PhysicsSpecializedSample represents a base class for character controller,
  // and vehicle samples. In Update() it draws all rigid bodies of the physics simulation
  // and debug information using the DebugRenderer.
  [Controls(@"Debug Rendering of Physics
  Press <C>, <B>, <I> to render Contacts, Bounding Boxes or Simulation Islands.
  Press <L> to render sleeping bodies in a different color.
  Press <M> to toggle wire frame mode")]
  public abstract class PhysicsSpecializedSample : BasicSample
  {
    private bool _drawWireFrame;
    private bool _showSleeping;
    private bool _drawContacts;
    private bool _drawBoundingBoxes;
    private bool _drawIslands;


    protected PhysicsSpecializedSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.White;
      GraphicsScreen.DrawReticle = true;
      SetCamera(new Vector3F(0, 2, 10), 0, 0);
    }


    public override void Update(GameTime gameTime)
    {
      // Toggle between wireframe and normal mode if <M> is pressed.
      if (InputService.IsPressed(Keys.M, true))
        _drawWireFrame = !_drawWireFrame;

      // If <L> is pressed render the sleeping (inactive) bodies in a different color.
      if (InputService.IsPressed(Keys.L, true))
        _showSleeping = !_showSleeping;

      // Visualize contacts if <C> is pressed.
      if (InputService.IsPressed(Keys.C, true))
        _drawContacts = !_drawContacts;

      // When contact drawing is enabled, we must make sure that the contact information
      // is up-to-date after Simulation.Update().
      Simulation.Settings.SynchronizeCollisionDomain = _drawContacts;

      // Visualize axis-aligned bounding boxes if <B> is pressed.
      if (InputService.IsPressed(Keys.B, true))
        _drawBoundingBoxes = !_drawBoundingBoxes;

      // Visualize simulation islands if <I> is pressed.
      if (InputService.IsPressed(Keys.I, true))
        _drawIslands = !_drawIslands;

      // ----- Draw rigid bodies using the DebugRenderer of the graphics screen.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      foreach (var body in Simulation.RigidBodies)
      {
        // To skip automatic drawing of bodies, the sub-classes can set the UserData
        // property to "NoDraw".
        if (body.UserData is string && (string)body.UserData == "NoDraw")
          continue;

        var color = Color.Gray;
        // Draw static and, optionally, sleeping bodies with different colors.
        if (body.MotionType == MotionType.Static || _showSleeping && body.IsSleeping)
          color = Color.LightGray;

        debugRenderer.DrawObject(body, color, _drawWireFrame, false);
      }

      // Draw contacts.
      if (_drawContacts)
        debugRenderer.DrawContacts(Simulation.CollisionDomain.ContactSets, 0.1f, Color.DarkOrange, true);

      // Draw AABBs.
      if (_drawBoundingBoxes)
      {
        foreach (CollisionObject collisionObject in Simulation.CollisionDomain.CollisionObjects)
        {
          if (collisionObject.Enabled)
            debugRenderer.DrawAabb(collisionObject.GeometricObject.Aabb, Pose.Identity, new Color(0x80, 0, 1), false);
        }
      }

      // Draw simulation islands.
      if (_drawIslands)
        DrawIslands();
    }


    // Visualizes the simulation islands of a simulation.
    private void DrawIslands()
    {
      var debugRenderer = GraphicsScreen.DebugRenderer;

      // Loop over all simulation islands.
      var islands = Simulation.IslandManager.Islands;
      for (int i = 0; i < islands.Count; i++)
      {
        // Compute AABB of island.
        var island = islands[i];
        Aabb aabb = island.RigidBodies[0].Aabb;
        var rigidBodies = island.RigidBodies;
        for (int j = 0; j < rigidBodies.Count; j++)
          aabb.Grow(rigidBodies[j].Aabb);

        // Draw AABB of island.
        debugRenderer.DrawAabb(aabb, Pose.Identity, Color.Yellow, false);
      }
    }
  }
}
