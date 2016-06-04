using DigitalRune.Geometry;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;


namespace Samples.Particles
{
  // An start value effector that initializes a particle parameter with random positions on a
  // triangle mesh. The start positions are transformed by the particle system's pose.
  public class StartOnMeshEffector : ParticleEffector
  {
    private IParticleParameter<Vector3F> _parameter;

    [ParticleParameter(ParticleParameterUsage.Out)]
    public string Parameter { get; set; }

    public ITriangleMesh Mesh { get; set; }


    public StartOnMeshEffector()
    {
      Parameter = ParticleParameterNames.Position;
    }


    #region ----- Cloning -----

    // Creates an instance of this class.
    protected override ParticleEffector CreateInstanceCore()
    {
      return new StartOnMeshEffector();
    }


    // Copy members of the given effector.
    protected override void CloneCore(ParticleEffector source)
    {
      base.CloneCore(source);

      var sourceTyped = (StartOnMeshEffector)source;
      Parameter = sourceTyped.Parameter;
      Mesh = sourceTyped.Mesh;
    }
    #endregion


    protected override void OnRequeryParameters()
    {
      _parameter = ParticleSystem.Parameters.Get<Vector3F>(Parameter);
    }


    protected override void OnInitialize()
    {
      // If Parameter is a uniform particle parameter, we initialize it here:
      if (_parameter != null && _parameter.Values == null)
      {
        // The Parameter is initialized with a random mesh position. If the reference frame
        // is World, we need to apply the particle system's pose to transform the position
        // to world space.

        if (ParticleSystem.ReferenceFrame == ParticleReferenceFrame.World)
        {
          // Apply translation of particle system.
          var pose = ParticleSystem.GetPoseWorld();
          _parameter.DefaultValue = pose.ToWorldPosition(GetRandomPositionOnMesh());
        }
        else
        {
          _parameter.DefaultValue = GetRandomPositionOnMesh();
        }
      }
    }


    protected override void OnUninitialize()
    {
      _parameter = null;
    }


    protected override void OnInitializeParticles(int startIndex, int count, object emitter)
    {
      if (_parameter == null)
        return;

      var positions = _parameter.Values;
      if (positions == null)
      {
        // Parameter is a uniform. Uniform parameters are handled in OnInitialize().
        return;
      }

      // If Parameter is a varying particle parameter, we initialize it here.
      // Again we have to apply the particle system's pose if the reference frame is World:
      if (ParticleSystem.ReferenceFrame == ParticleReferenceFrame.World)
      {
        var pose = ParticleSystem.GetPoseWorld();
        if (pose != Pose.Identity)
        {
          for (int i = startIndex; i < startIndex + count; i++)
            positions[i] = pose.ToWorldPosition(GetRandomPositionOnMesh());

          return;
        }
      }

      // The reference frame is Local, or the particle system Pose is the identity.
      for (int i = startIndex; i < startIndex + count; i++)
        positions[i] = GetRandomPositionOnMesh();
    }


    private Vector3F GetRandomPositionOnMesh()
    {
      if (Mesh == null)
        return new Vector3F();

      int numberOfTriangles = Mesh.NumberOfTriangles;

      // Get a random triangle.
      int randomIndex = ParticleSystem.Random.NextInteger(0, numberOfTriangles - 1);
      var triangle = Mesh.GetTriangle(randomIndex);

      // Get random point on the triangle using barycentric coordinates.
      float u = ParticleSystem.Random.NextFloat(0, 1);
      float v = ParticleSystem.Random.NextFloat(0, 1);
      float w = 1 - u - v;
      return triangle.Vertex0 * u + triangle.Vertex1 * v + triangle.Vertex2 * w;
    }
  }
}
