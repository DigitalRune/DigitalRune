#if !WP7 && !WP8
using DigitalRune.Game;
using DigitalRune.Graphics.Effects;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using System;
using System.Linq;


namespace Samples.Graphics
{
  /// <summary>
  /// Manages many vegetation mesh instances in a grid.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Each <see cref="VegetationObject"/> manages instances of one mesh. The instances are managed
  /// in a 2D grid structure. Random instances are placed in each cell. One
  /// <see cref="MeshInstancingNode{T}"/> is created for each grid cell to store the instances.
  /// </para>
  /// <para>
  /// <see cref="MinDistance"/> and <see cref="MaxDistance"/> define which instances are visible.
  /// Usually, <see cref="MinDistance"/> is 0 and <see cref="MaxDistance"/> is greater than 0.
  /// That means that the mesh instances are visible near the camera and not visible beyond
  /// <see cref="MaxDistance"/>. If <see cref="MinDistance"/> is set to a value greater than 0,
  /// then the instances are not visible near the camera. They start to appear at
  /// <see cref="MinDistance"/>. This can be used to combine different mesh LODs. For example:
  /// One <see cref="VegetationObject"/> uses a detailed mesh and min distance = 0 and max
  /// distance = 20. A second <see cref="VegetationObject"/> uses a low-poly version of the mesh
  /// with min distance = 20 and max distance = 50.
  /// </para>
  /// <para>
  /// <strong>Fade in/out:</strong>
  /// The <see cref="VegetationObject"/> assumes that the meshes use the special vegetation shaders
  /// (see "DIGITALRUNE_FOLDER/Samples/Content/Vegetation/*Vegetation.fx"). These shaders
  /// use an alpha-test with a dither pattern ("screen door transparency") to fade meshes in/out.
  /// The effect parameter "LodDistances" controls the fade in/out distances.
  /// </para>
  /// <para>
  /// <strong>Culling:</strong>
  /// In <see cref="OnUpdate"/> the <see cref="VegetationObject"/> checks the camera position and
  /// only adds the <see cref="MeshInstancingNode{T}"/>s to the scene which are within the
  /// <see cref="MaxDistance"/>.
  /// </para>
  /// <para>
  /// <strong>Shadows:</strong>
  /// Currently, the shadows of one cell are completely visible or not visible. The vegetation
  /// shadow map effects do not yet implement a fade out effect.
  /// </para>
  /// </remarks>
  public class VegetationObject : GameObject
  {
    // Notes:
    // This vegetation object is very simple. In a real game you will want to choose
    // a better placement strategy for mesh instances depending on the terrain.

    private readonly IServiceLocator _services;
    private IScene _scene;
    private CameraObject _cameraObject;

    private readonly Mesh _mesh;
    private readonly int _numberOfInstancesPerCell;
    private readonly float _cellSize;
    private readonly int _numberOfCellsX;
    private readonly int _numberOfCellsZ;
    private readonly int _randomSeed;

    // One mesh instancing node per cell.
    private MeshInstancingNode<InstanceData>[,] _nodes;


    /// <summary>
    /// Gets or sets the minimum distance at which the vegetation meshes become visible.
    /// </summary>
    /// <value>The minimum distance.</value>
    /// <remarks>
    /// <see cref="MinDistance"/> (= inner radius) and <see cref="MaxDistance"/> (= outer radius)
    /// define a ring around the camera in which the vegetation meshes are visible. Outside this
    /// area the vegetation meshes are hidden.
    /// </remarks>
    public float MinDistance
    {
      get { return _minDistance; }
      set
      {
        _minDistance = value;
        UpdateLodDistances();
      }
    }
    private float _minDistance;


    /// <summary>
    /// Gets or sets the maximum distance up to which the meshes are visible.
    /// </summary>
    /// <value>The maximum distance.</value>
    /// <remarks>
    /// <see cref="MinDistance"/> (= inner radius) and <see cref="MaxDistance"/> (= outer radius)
    /// define a ring around the camera in which the vegetation meshes are visible. Outside this
    /// area the vegetation meshes are hidden.
    /// </remarks>
    public float MaxDistance
    {
      get { return _maxDistance; }
      set
      {
        _maxDistance = value;
        UpdateLodDistances();
      }
    }
    private float _maxDistance;


    /// <summary>
    /// Gets or sets a value indicating whether the vegetation meshes cast shadows.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the vegetation meshes cast shadows; otherwise,
    /// <see langword="false"/>.
    /// </value>
    public bool CastsShadows
    {
      get { return _castsShadows; }
      set { _castsShadows = value; }
    }
    private bool _castsShadows = true;


    /// <summary>
    /// Initializes a new instance of the <see cref="VegetationObject"/> class.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <param name="mesh">The vegetation mesh.</param>
    /// <param name="numberOfInstancesPerCell">The number of instances per cell.</param>
    /// <param name="cellSize">The size of a cell.</param>
    /// <param name="numberOfCellsX">The number of cells in x direction.</param>
    /// <param name="numberOfCellsZ">The number of cells in z direction.</param>
    /// <param name="randomSeed">A random seed.</param>
    public VegetationObject(IServiceLocator services, Mesh mesh, int numberOfInstancesPerCell,
                            float cellSize, int numberOfCellsX, int numberOfCellsZ, int randomSeed)
    {
      if (services == null)
        throw new ArgumentNullException("services");
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      _services = services;
      _mesh = mesh;
      _numberOfInstancesPerCell = numberOfInstancesPerCell;
      _cellSize = cellSize;
      _numberOfCellsX = numberOfCellsX;
      _numberOfCellsZ = numberOfCellsZ;
      _randomSeed = randomSeed;

      _minDistance = 0;
      _maxDistance = 50;
    }


    protected override void OnLoad()
    {
      _scene = _services.GetInstance<IScene>();

      // Get a bounding shape for the cells. We use a box with the cell size and
      // make it bigger by some arbitrary values. The boxes must be bigger because
      // mesh instances can be placed on the cell boundary and when they are animated
      // tree branches can sway outside the cell bounds.
      var meshAabb = _mesh.BoundingShape.GetAabb();
      float meshWidth = new Vector2F(meshAabb.Extent.X, meshAabb.Extent.Z).Length * 1.5f;
      float meshHeight = meshAabb.Extent.Y * 1.7f;
      var boxShape = new BoxShape(_cellSize + meshWidth, meshHeight, _cellSize + meshWidth);

      // Create one MeshInstancingNode per cell and add random instances.
      _nodes = new MeshInstancingNode<InstanceData>[_numberOfCellsX, _numberOfCellsZ];
      float xOrigin = -(_numberOfCellsX * _cellSize) / 2;
      float zOrigin = -(_numberOfCellsZ * _cellSize) / 2;
      var random = new Random(_randomSeed);
      for (int x = 0; x < _numberOfCellsX; x++)
      {
        for (int z = 0; z < _numberOfCellsZ; z++)
        {
          var instances = new InstanceData[_numberOfInstancesPerCell];
          for (int i = 0; i < instances.Length; i++)
          {
            Vector3F scale = new Vector3F(random.NextFloat(0.5f, 1.5f));
            Pose pose = new Pose(new Vector3F(xOrigin + x * _cellSize + random.NextFloat(0, _cellSize),
                                              0,
                                              zOrigin + z * _cellSize + random.NextFloat(0, _cellSize)),
                                 Matrix33F.CreateRotationY(random.NextFloat(0, 10)));
            Vector4F color = new Vector4F(1);

            instances[i] = new InstanceData(scale, pose, color);
          }

          _nodes[x, z] = new MeshInstancingNode<InstanceData>(_mesh, instances)
          {
            PoseLocal = new Pose(new Vector3F(xOrigin + (0.5f + x) * _cellSize, 
                                              boxShape.WidthY / 2, 
                                              zOrigin + (0.5f + z) * _cellSize)),
            Shape = boxShape,
            CastsShadows = _castsShadows,
          };
          _scene.Children.Add(_nodes[x, z]);
        }
      }

      UpdateLodDistances();

      // ----- Add GUI controls to the Options window.
      var sampleFramework = _services.GetInstance<SampleFramework>();
      var optionsPanel = sampleFramework.AddOptions("Game Objects");
      var panel = SampleHelper.AddGroupBox(optionsPanel, "VegetationObject " + Name);

      SampleHelper.AddSlider(
        panel,
        "Min distance",
        "F2",
        0,
        100,
        MinDistance,
        value => MinDistance = value);

      SampleHelper.AddSlider(
        panel,
        "Max distance",
        "F2",
        0,
        100,
        MaxDistance,
        value => MaxDistance = value);
    }


    protected override void OnUnload()
    {
      // Remove scene nodes from scene and dispose them.
      foreach (var node in _nodes)
      {
        if (node.Parent != null)
          node.Parent.Children.Remove(node);

        node.Dispose(false);
      }

      _nodes = null;
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      if (_cameraObject == null)
      {
        // Get the camera game object.
        var gameObjectService = _services.GetInstance<IGameObjectService>();
        _cameraObject = gameObjectService.Objects.OfType<CameraObject>().First();
      }

      // Create an AABB with the camera position and add the MaxDistance.
      var cameraPosition = _cameraObject.CameraNode.PoseWorld.Position;
      var cameraAabb = new Aabb(cameraPosition, cameraPosition);
      cameraAabb.Minimum -= new Vector3F(MaxDistance);
      cameraAabb.Maximum += new Vector3F(MaxDistance);

      // Add all scene nodes which are within the MaxDistance to the scene.
      // Remove the other scene nodes.
      // TODO: This code performs brute-force checks. We can make this a lot faster
      // when the nodes are in a simple 2D grid structure.
      // TODO: It is not necessary to make these checks each frame. It is sufficient
      // to update the scene nodes when the camera has moved a certain distance.
      // TODO: Add a hysteresis.
      for (int x = 0; x < _nodes.GetLength(0); x++)
      {
        for (int z = 0; z < _nodes.GetLength(0); z++)
        {
          bool isVisible = GeometryHelper.HaveContact(cameraAabb, _nodes[x, z].Aabb);
          if (isVisible)
          {
            if (_nodes[x, z].Parent == null)
              _scene.Children.Add(_nodes[x, z]);

            _nodes[x, z].CastsShadows = CastsShadows;
          }
          else
          {
            if (_nodes[x, z].Parent != null)
              _scene.Children.Remove(_nodes[x, z]);
          }

          _nodes[x, z].IsEnabled = isVisible;
        }
      }
    }


    /// <summary>
    /// Updates the "LodDistances" effect parameters of the mesh instancing nodes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method assumes that "LodDistances" is a "per instance" parameter and therefore it
    /// expects the effect parameter in MeshInstancingNode.MaterialInstances and not in
    /// MeshInstancingNode.Mesh.Materials. 
    /// </para>
    /// <para>
    /// In the effects of this sample "LodDistances" is a a "per instance" parameter because
    /// the LOD meshes of the BirdnestPlant model use the same material. Since each LOD mesh
    /// must use different LOD distances, we cannot store the parameter in the shared material.
    /// </para>
    /// </remarks>
    private void UpdateLodDistances()
    {
      if (_nodes == null)
        return;

      foreach (var node in _nodes)
      {
        foreach (var materialInstance in node.MaterialInstances)
        {
          foreach (var effectBinding in materialInstance.EffectBindings)
          {
            if (effectBinding.ParameterBindings.Contains("LodDistances"))
            {
              const float transitionRange = 1;
              var lodDistancesBinding = (ConstParameterBinding<Vector3>)effectBinding.ParameterBindings["LodDistances"];
              lodDistancesBinding.Value = new Vector3(MinDistance, MaxDistance, transitionRange);
            }
          }
        }
      }
    }
  }
}
#endif
