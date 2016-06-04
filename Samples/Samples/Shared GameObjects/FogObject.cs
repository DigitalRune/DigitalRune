using DigitalRune.Game;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Practices.ServiceLocation;


namespace Samples
{
  // Adds distance and height-based fog. Fog is disabled by default.
  public class FogObject : GameObject
  {
    private readonly IServiceLocator _services;


    public FogNode FogNode { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether the fog node is attached to the camera.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if fog node is attached to the camera; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Optionally, we can move the fog node with the camera node. If camera and 
    /// fog are independent, then the camera can fly up and "escape" the height-based 
    /// fog. If camera and fog move together, then the fog will always have the
    /// same height at the horizon (e.g. to hide the horizon).
    /// </remarks>
    public bool AttachToCamera
    {
      get { return _attachToCamera; }
      set
      {
        if (value == _attachToCamera)
          return;

        _attachToCamera = value;

        if (FogNode != null)
        {
          // Remove fog node from existing parent and re-add.
          if (FogNode.Parent != null)
            FogNode.Parent.Children.Remove(FogNode);

          AddFogNodeToScene();
        }
      }
    }
    private bool _attachToCamera;


    public FogObject(IServiceLocator services)
    {
      _services = services;
      Name = "Fog";
    }


    // OnLoad() is called when the GameObject is added to the IGameObjectService.
    protected override void OnLoad()
    {
      FogNode = new FogNode(new Fog())
      {
        IsEnabled = false,
        Name = "Fog",
      };

      AddFogNodeToScene();

      // Add GUI controls to the Options window.
      var sampleFramework = _services.GetInstance<SampleFramework>();
      var optionsPanel = sampleFramework.AddOptions("Game Objects");
      var panel = SampleHelper.AddGroupBox(optionsPanel, "FogObject");

      SampleHelper.AddCheckBox(
        panel,
        "Enable fog",
        FogNode.IsEnabled,
        isChecked => FogNode.IsEnabled = isChecked);

      SampleHelper.AddCheckBox(
        panel,
        "Attach to camera",
        AttachToCamera,
        isChecked => AttachToCamera = isChecked);

      SampleHelper.AddSlider(
        panel,
        "Fog ramp start",
        "F2",
        0,
        1000,
        FogNode.Fog.Start,
        value => FogNode.Fog.Start = value);

      SampleHelper.AddSlider(
        panel,
        "Fog ramp end",
        "F2",
        0,
        5000,
        FogNode.Fog.End,
        value => FogNode.Fog.End = value);

      SampleHelper.AddSlider(
        panel,
        "Density",
        "F2",
        0.0f,
        2,
        FogNode.Fog.Density,
        value => FogNode.Fog.Density = value);

      SampleHelper.AddSlider(
        panel,
        "Height falloff",
        "F2",
        -1,
        1,
        FogNode.Fog.HeightFalloff,
        value => FogNode.Fog.HeightFalloff = value);

      SampleHelper.AddSlider(
        panel,
        "Height Y",
        "F2",
        -100,
        100,
        FogNode.PoseWorld.Position.Y,
        value =>
        {
          var pose = FogNode.PoseWorld;
          pose.Position.Y = value;
          FogNode.PoseWorld = pose;
        });
    }


    private void AddFogNodeToScene()
    {
      var scene = _services.GetInstance<IScene>();
      if (!_attachToCamera)
      {
        scene.Children.Add(FogNode);
      }
      else
      {
        var cameraNode = ((Scene)scene).GetSceneNode("PlayerCamera");
        if (cameraNode.Children == null)
          cameraNode.Children = new SceneNodeCollection();

        cameraNode.Children.Add(FogNode);
      }
    }


    // OnUnload() is called when the GameObject is removed from the IGameObjectService.
    protected override void OnUnload()
    {
      FogNode.Parent.Children.Remove(FogNode);
      FogNode.Dispose(false);
      FogNode = null;
    }
  }
}
