#if !WP7 && !WP8
using System;
using DigitalRune.Geometry;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows a simple scene rendered with deferred lighting.",
    @"The class DeferredGraphicsScreen implements a deferred lighting render pipeline.
This pipeline supports: 
  - lights and shadows, 
  - screen-space ambient occlusion (SSAO),
  - high dynamic range (HDR) lighting, 
  - sky rendering, 
  - particle systems with soft particles and low-resolution offscreen rendering,
  - post-processing, 
  - and more...
The intermediate render targets (G-buffer, light buffer, shadow masks) can be
visualized for debugging and understanding. (Press <F4> to show Options window.)
Beginners can use this graphics screen as it is. Advanced developers can adapt 
the render pipeline to their needs.
Have a look at the source code comments of the DeferredGraphicsScreen for more details.",
    101)]
  public class DeferredLightingSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;


    public DeferredLightingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, _graphicsScreen);

      // Add a game object which adds some GUI controls for the deferred graphics 
      // screen to the Options window.
      GameObjectService.Objects.Add(new DeferredGraphicsOptionsObject(Services));

      Services.Register(typeof(DebugRenderer), null, _graphicsScreen.DebugRenderer);
      Services.Register(typeof(IScene), null, _graphicsScreen.Scene);

      // Add gravity and damping to the physics simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new StaticSkyObject(Services)); // Skybox + some lights.
      GameObjectService.Objects.Add(new GroundObject(Services));

      // Add a god ray post-process filter and a game object which updates the god ray directions.
      var godRayFilter = new GodRayFilter(GraphicsService)
      {
        Intensity = new Vector3F(1.0f),
        NumberOfSamples = 12,
        NumberOfPasses = 2,
        Softness = 1,
      };
      _graphicsScreen.PostProcessors.Add(godRayFilter);
      GameObjectService.Objects.Add(new GodRayObject(Services, godRayFilter));

      GameObjectService.Objects.Add(new DudeObject(Services));
      GameObjectService.Objects.Add(new DynamicObject(Services, 1));
      GameObjectService.Objects.Add(new DynamicObject(Services, 2));
      GameObjectService.Objects.Add(new DynamicObject(Services, 3));
      GameObjectService.Objects.Add(new DynamicObject(Services, 4));
      GameObjectService.Objects.Add(new DynamicObject(Services, 5));
      GameObjectService.Objects.Add(new DynamicObject(Services, 6));
      GameObjectService.Objects.Add(new DynamicObject(Services, 7));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new FogObject(Services));
      GameObjectService.Objects.Add(new CampfireObject(Services));

      // The LavaBalls class controls all lava ball instances.
      var lavaBalls = new LavaBallsObject(Services);
      GameObjectService.Objects.Add(lavaBalls);

      // Create a lava ball instance.
      lavaBalls.Spawn();

      // Add a few palm trees.
      Random random = new Random(12345);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-3, -8), 0, random.NextFloat(0, -5));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree", scale, new Pose(position, orientation)));
      }
    }


    public override void Update(GameTime gameTime)
    {
      // This sample clears the debug renderer each frame.
      _graphicsScreen.DebugRenderer.Clear();
    }


/* ----- How to add a new model to the project for use with the deferred graphics screen -----
    Here is a list of steps.
    Add the model to the content project:
      a. Right-click the content project and select Add | Existing Item.... 
      b. Browse for the model file (e.g. a FBX or X file) and click Add. 
      Process the model with the new model processor:
      c. Select the model in the content project.
      d. Right-click the model and select Properties to open the Properties window.
      e. In the Properties window search for the property Content Processor and select Model - 
         DigitalRune Graphics from the combo box.
    Now, build the project. This automatically creates .drmdl and .drmat files, in case they do not 
    yet exist. (Or you can simply copy and rename .drmdl and .drmat files from an existing model.) 
    In the Windows Explorer go to the folder containing the model. It is time to edit the .drmdl and 
    .drmat files: The .drmdl file defines the preprocessing settings of the model. 
    Example: MetalGrateBox.drmdl
      <?xml version="1.0" encoding="utf-8"?>
      <Model File="MetalGrateBox.fbx" GenerateTangentFrames="True">
        <Mesh Name="Box">
          <Submesh Material="Metal_Grate.drmat" />
        </Mesh>
      </Model>

    The .drmat file defines the material.
    Example: Metal_Grate.drmat
      <?xml version="1.0" encoding="utf-8"?>
      <Material>
        <Pass Name="Default" Effect="BasicEffect" Profile="Any">
          <Parameter Name="DiffuseColor" Value="1,1,1" />
          <Parameter Name="SpecularColor" Value="1,1,1" />
          <Parameter Name="SpecularPower" Value="100" />
          <Texture Name="Texture" File="Metal_Grate_diffuse.png" />
        </Pass>
        <Pass Name="ShadowMap" Effect="DigitalRune/Materials/ShadowMap" Profile="HiDef" />
        <Pass Name="GBuffer" Effect="DigitalRune/Materials/GBufferNormal" Profile="HiDef">
          <Parameter Name="SpecularPower" Value="100" />
          <Texture Name="NormalTexture" Format="NormalInvertY" File="Metal_Grate_normal.png"/>
        </Pass>
        <Pass Name="Material" Effect="DigitalRune/Materials/Material" Profile="HiDef">
          <Parameter Name="DiffuseColor" Value="1,1,1" />
          <Parameter Name="SpecularColor" Value="1,1,1" />
          <Texture Name="DiffuseTexture" File="Metal_Grate_diffuse.png" />
          <Texture Name="SpecularTexture" File="Metal_Grate_specular.png" />
        </Pass>
      </Material>

    The rendering is defined in DeferredGraphicsScreen.cs. The sample uses the following passes: 
    "ShadowMap", "GBuffer", "Material". If one of these passes is missing in the .drmat file, it 
    won't be rendered properly. A list of all effects can be find in the DigitalRune Documentation
    (User Documentation | DigitalRune Graphics | Effects and Materials | Predefined Effects).
    Next, load the model and add it to the scene. You can add it directly
      ModelNode myModel = game.Content.Load<ModelNode>("MyModel").Clone();
      scene.Children.Add(myModel);
    Or, you can use the StaticObject, which is defined in the sample:
      gameObjectService.Objects.Add(new StaticObject("MyModel", 1.0f, Pose.Identity));
  */
  }
}
#endif