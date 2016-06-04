#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;


namespace Samples
{
  // Controls the GodRayFilter.
  // When this game object is loaded, it finds the most important directional light
  // in the scene and uses this to control the GodRayFilter.
  public class GodRayObject : GameObject
  {
    private readonly IServiceLocator _services;
    private readonly GodRayFilter _godRayFilter;
    private readonly float _defaultScale;
    private LightNode _directionalLightNode;


    public GodRayObject(IServiceLocator services, GodRayFilter godRayFilter)
    {
      if (godRayFilter == null)
        throw new ArgumentNullException("godRayFilter");

      Name = "GodRay";
      _services = services;
      _godRayFilter = godRayFilter;
      _defaultScale = godRayFilter.Scale;
    }


    // OnLoad() is called when the GameObject is added to the IGameObjectService.
    protected override void OnLoad()
    {
      var scene = (SceneNode)_services.GetInstance<IScene>();

      // Find the most important directional light. 
      _directionalLightNode = scene.GetDescendants()
                                   .OfType<LightNode>()
                                   .Where(ln => ln.Light is DirectionalLight)
                                   .OrderBy(ln => ln.Priority)
                                   .LastOrDefault();

      if (_directionalLightNode != null)
      {
        // This light node defines the direction of the god rays. Handle the 
        // SceneChanged event to update the GodRayFilter when the light node is
        // updated.
        _directionalLightNode.SceneChanged += OnDirectionalLightNodeChanged;

        // First time initialization:
        OnDirectionalLightNodeChanged(null, null);
      }
    }


    // OnUnload() is called when the GameObject is removed from the IGameObjectService.
    protected override void OnUnload()
    {
      _godRayFilter.Scale = _defaultScale;

      if (_directionalLightNode != null)
      {
        _directionalLightNode.SceneChanged -= OnDirectionalLightNodeChanged;
        _directionalLightNode = null;
      }
    }


    private void OnDirectionalLightNodeChanged(object sender, SceneChangedEventArgs eventArgs)
    {
      _godRayFilter.Enabled = _directionalLightNode.IsEnabled;
      _godRayFilter.LightDirection = _directionalLightNode.PoseWorld.ToWorldDirection(Vector3F.Forward);
    }
  }
}
#endif