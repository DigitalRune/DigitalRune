// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Returns the lights that affect a specific scene node.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="LightQuery"/> can be executed against a scene by calling 
  /// <see cref="IScene.Query{T}"/>. The query can be used to get all <see cref="LightNode"/>s that
  /// affect a certain reference node in the scene. The reference node is typically the scene node 
  /// that is currently being rendered.
  /// </para>
  /// <para>
  /// The light nodes are grouped by type (ambient lights, directional lights, etc.). The lights
  /// within each group are usually sorted descending by the approximate light contribution on the 
  /// reference node. That means, lights that have a strong influence on the reference node are 
  /// listed before lights that have a weak influence.
  /// </para>
  /// </remarks>
  public class LightQuery : ISceneQuery
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Reference position for calculating light contribution.
    private Vector3F? _referencePosition;

    // LOD computations:
    private Vector3F _cameraPosition;
    private float _lodBiasOverYScale;

    // Clip geometry tests:
    private CollisionObject _sphereCollisionObject;
    private CollisionObject _clipCollisionObject;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public SceneNode ReferenceNode { get; private set; }


    /// <summary>
    /// Gets the ambient lights.
    /// </summary>
    /// <value>The ambient lights.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<LightNode> AmbientLights { get; private set; }


    /// <summary>
    /// Gets the directional lights.
    /// </summary>
    /// <value>The directional lights.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<LightNode> DirectionalLights { get; private set; }


    /// <summary>
    /// Gets the point lights.
    /// </summary>
    /// <value>The point lights.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<LightNode> PointLights { get; private set; }


    /// <summary>
    /// Gets the spotlights.
    /// </summary>
    /// <value>The spotlights.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<LightNode> Spotlights { get; private set; }


    /// <summary>
    /// Gets the projector lights.
    /// </summary>
    /// <value>The projector lights.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<LightNode> ProjectorLights { get; private set; }


    /// <summary>
    /// Gets the image-based lights.
    /// </summary>
    /// <value>The image-based lights.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<LightNode> ImageBasedLights { get; private set; }


    /// <summary>
    /// Gets other lights that did not fit into any of the predefined categories
    /// (<see cref="AmbientLights"/>, <see cref="DirectionalLights"/>, etc.).
    /// </summary>
    /// <value>The other lights.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<LightNode> OtherLights { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="LightQuery"/> class.
    /// </summary>
    public LightQuery()
    {
      // Create collections for caching the light nodes.
      AmbientLights = new List<LightNode>();
      DirectionalLights = new List<LightNode>();
      PointLights = new List<LightNode>();
      Spotlights = new List<LightNode>();
      ProjectorLights = new List<LightNode>();
      ImageBasedLights = new List<LightNode>();
      OtherLights = new List<LightNode>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public void Reset()
    {
      ReferenceNode = null;
      _referencePosition = null;

      DirectionalLights.Clear();
      AmbientLights.Clear();
      PointLights.Clear();
      Spotlights.Clear();
      ProjectorLights.Clear();
      ImageBasedLights.Clear();
      OtherLights.Clear();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public void Set(SceneNode referenceNode, IList<SceneNode> nodes, RenderContext context)
    {
      Reset();
      ReferenceNode = referenceNode;

      // We use the origin of the reference node as the reference position for determining
      // the light contribution. Alternatively we could use the closest point in/on model AABB,
      // but this requires valid AABBs, is more expensive, etc.
      if (referenceNode != null)
        _referencePosition = referenceNode.PoseWorld.Position;

      int numberOfNodes = nodes.Count;

#if DEBUG
      for (int i = 0; i < numberOfNodes; i++)
        Debug.Assert(nodes[i].ActualIsEnabled, "Scene query contains disabled nodes.");
#endif

      if (context.LodCameraNode == null)
      {
        // ----- No LOD
        for (int i = 0; i < numberOfNodes; i++)
        {
          var lightNode = nodes[i] as LightNode;
          if (lightNode != null)
            AddNode(lightNode);
        }
      }
      else
      {
        // ----- LOD
        // Get values for LOD computations.
        var cameraNode = context.LodCameraNode;
        _cameraPosition = cameraNode.PoseLocal.Position;
        _lodBiasOverYScale = 1 / Math.Abs(cameraNode.Camera.Projection.ToMatrix44F().M11)
                             * cameraNode.LodBias * context.LodBias;

        // Add nodes and evaluate LOD groups.
        for (int i = 0; i < numberOfNodes; i++)
          AddNodeWithLod(nodes[i], context);
      }

      // Sort lights.
      if (referenceNode != null)
      {
        AmbientLights.Sort(DescendingLightNodeComparer.Instance);
        DirectionalLights.Sort(DescendingLightNodeComparer.Instance);
        PointLights.Sort(DescendingLightNodeComparer.Instance);
        Spotlights.Sort(DescendingLightNodeComparer.Instance);
        ProjectorLights.Sort(DescendingLightNodeComparer.Instance);
        ImageBasedLights.Sort(DescendingLightNodeComparer.Instance);
        OtherLights.Sort(DescendingLightNodeComparer.Instance);
      }
    }


    private void AddNode(LightNode lightNode)
    {
      Debug.Assert(lightNode.ActualIsEnabled, "Scene query contains disabled nodes.");

      // Ignore light if the reference position is not in the clip geometry.
      if (lightNode.Clip != null && _referencePosition.HasValue)
      {
        bool haveContact = HaveContact(lightNode.Clip, _referencePosition.Value);

        if (lightNode.InvertClip == haveContact)
          return;
      }

      // ----- Sort by estimated light contribution.

      // All lights except IBL will be sorted by contribution. 
      // Note: If we have no reference node, it still makes sense to sort the objects
      // by their contribution. We choose to get the light contribution at the position
      // of each light.
      Vector3F position = _referencePosition ?? lightNode.PoseWorld.Position;
      //lightNode.SortTag = lightNode.GetLightContribution(position, 0.7f);

      // Or simpler: Sort light nodes by distance. --> Use for image-based lights.
      // (We use distance², because it is faster.)
      //float distance = (position - lightNode.PoseWorld.Position).LengthSquared; 
      //lightNode.SortTag = -distance;   // minus because we use descending sort.
      
      if (lightNode.Light is AmbientLight)
      {
        AmbientLights.Add(lightNode);
        lightNode.SortTag = lightNode.GetLightContribution(position, 0.7f);
      }
      else if (lightNode.Light is DirectionalLight)
      {
        DirectionalLights.Add(lightNode);
        lightNode.SortTag = lightNode.GetLightContribution(position, 0.7f);
      }
      else if (lightNode.Light is PointLight)
      {
        PointLights.Add(lightNode);
        lightNode.SortTag = lightNode.GetLightContribution(position, 0.7f);
      }
      else if (lightNode.Light is Spotlight)
      {
        Spotlights.Add(lightNode);
        lightNode.SortTag = lightNode.GetLightContribution(position, 0.7f);
      }
      else if (lightNode.Light is ProjectorLight)
      {
        ProjectorLights.Add(lightNode);
        lightNode.SortTag = lightNode.GetLightContribution(position, 0.7f);
      }
      else if (lightNode.Light is ImageBasedLight)
      {
        ImageBasedLights.Add(lightNode);
        float distance = (position - lightNode.PoseWorld.Position).LengthSquared; 
        lightNode.SortTag = -distance;  // minus because we use descending sort.
      }
      else
      {
        OtherLights.Add(lightNode);
        lightNode.SortTag = lightNode.GetLightContribution(position, 0.7f);
      }
    }


    private bool HaveContact(IGeometricObject clipGeometry, Vector3F position)
    {
      // Use a shared collision detection instance.
      var collisionDetection = SceneHelper.CollisionDetection;

      if (_sphereCollisionObject == null)
      {
        // First time initializations.
        var sphereGeometricObject = TestGeometricObject.Create();
        sphereGeometricObject.Shape = new SphereShape(0);
        _sphereCollisionObject = new CollisionObject(sphereGeometricObject);
        _clipCollisionObject = new CollisionObject(TestGeometricObject.Create());
      }

      var _sphereGeometricObject = (TestGeometricObject)_sphereCollisionObject.GeometricObject;
      _sphereGeometricObject.Pose = new Pose(position);

      _clipCollisionObject.GeometricObject = clipGeometry;
      var result = collisionDetection.HaveContact(_sphereCollisionObject, _clipCollisionObject);
      _clipCollisionObject.GeometricObject = null;

      return result;
    }


    private void AddNodeWithLod(SceneNode node, RenderContext context)
    {
      var lightNode = node as LightNode;
      var lodGroupNode = node as LodGroupNode;
      bool isLightNode = (lightNode != null);
      bool isLodGroupNode = (lodGroupNode != null);
      if (!isLightNode && !isLodGroupNode)
        return;

      bool hasMaxDistance = Numeric.IsPositiveFinite(node.MaxDistance);
      float distance = 0;
      if (hasMaxDistance || isLodGroupNode)
      {
        Debug.Assert(
          node.ScaleWorld.X > 0 && node.ScaleWorld.Y > 0 && node.ScaleWorld.Z > 0,
          "Assuming that all scale factors are positive.");

        // Determine view-normalized distance between scene node and camera node.
        distance = (node.PoseWorld.Position - _cameraPosition).Length;
        distance *= _lodBiasOverYScale;
        distance /= node.ScaleWorld.LargestComponent;
      }

      // Distance Culling: Only handle nodes that are within MaxDistance.
      if (hasMaxDistance && distance >= node.MaxDistance)
        return;   // Ignore scene node.

      if (isLodGroupNode)
      {
        // Evaluate LOD group.
        var lodSelection = lodGroupNode.SelectLod(context, distance);
        AddSubtree(lodSelection.Current, context);
      }
      else
      {
        AddNode(lightNode);
      }
    }


    private void AddSubtree(SceneNode node, RenderContext context)
    {
      if (node.IsEnabled)
      {
        AddNodeWithLod(node, context);
        if (node.Children != null)
          foreach (var childNode in node.Children)
            AddSubtree(childNode, context);
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    internal List<LightNode> GetLights<T>() where T : Light
    {
      Type type = typeof(T);
      if (type == typeof(AmbientLight))
        return AmbientLights;
      if (type == typeof(DirectionalLight))
        return DirectionalLights;
      if (type == typeof(PointLight))
        return PointLights;
      if (type == typeof(Spotlight))
        return Spotlights;
      if (type == typeof(ProjectorLight))
        return ProjectorLights;
      if (type == typeof(ImageBasedLight))
        return ImageBasedLights;

      string message = String.Format(CultureInfo.InvariantCulture, "Type of light \"{0}\" is not supported by LightQuery.", type);
      throw new GraphicsException(message);
    }
    #endregion
  }
}
