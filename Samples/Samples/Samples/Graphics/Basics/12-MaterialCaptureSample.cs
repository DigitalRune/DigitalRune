// The matcap textures Samples/Content/Matcap/mc*.jpg are not included in this release.
//#if !WP7 && !WP8
//using DigitalRune.Graphics;
//using DigitalRune.Graphics.Effects;
//using DigitalRune.Graphics.Rendering;
//using DigitalRune.Graphics.SceneGraph;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;


//namespace Samples.Graphics
//{
//  [Sample(SampleCategory.Graphics,
//    @"This samples shows how to use a material capture shader to render a model.",
//    @"A material capture texture contains an image of a shaded sphere. This textures is mapped 
//onto a model using view-space environment mapping. This is very useful for 3D modeling tools 
//that want to display a 3D mesh without dynamic lights and without textures. The effect is 
//inexpensive and looks great when the camera does not rotate. This is also known as ""MatCap"". 
//There are many free MatCap textures available on the internet. The textures used in this 
//sample are available under terms of the GNU General Purpose License (GPL).",
//    12)]
//  [Controls(@"Sample
//  Press <Up Arrow>/<Down Arrow> to switch texture.")]
//  class MaterialCaptureSample : Sample
//  {
//    // The name format of the matcap texture content.
//    private const string MatcapTexturePath = "Matcaps/mc{0:00}";

//    // The number of matcap textures.
//    private const int NumberOfMatcapTextures = 24;

//    private readonly CameraObject _cameraObject;
//    private readonly Scene _scene;
//    private readonly ModelNode _model;
//    private readonly MeshRenderer _meshRenderer;
//    private readonly DebugRenderer _debugRenderer;

//    private int _matcapTextureIndex = 1;


//    public MaterialCaptureSample(Microsoft.Xna.Framework.Game game)
//      : base(game)
//    {
//      SampleFramework.IsMouseVisible = false;
//      var delegateGraphicsScreen = new DelegateGraphicsScreen(GraphicsService)
//      {
//        RenderCallback = Render,
//      };
//      GraphicsService.Screens.Insert(0, delegateGraphicsScreen);

//      // Add a custom game object which controls the camera.
//      _cameraObject = new CameraObject(Services);
//      GameObjectService.Objects.Add(_cameraObject);

//      // Create a new empty scene.
//      _scene = new Scene();

//      // Add the camera node to the scene.
//      _scene.Children.Add(_cameraObject.CameraNode);

//      _model = ContentManager.Load<ModelNode>("Dude/Dude").Clone();

//      // Add the model to the scene.
//      _scene.Children.Add(_model);

//      // The dude model does not use a matcap shader. Following code replaces the 
//      // dude model's materials with a matcap shader.
//      var matcapMaterial = ContentManager.Load<Material>("Matcaps/MatcapNormalSkinned");
//      var meshNode = (MeshNode)_model.Children[0];
//      for (int i = 0; i < meshNode.Mesh.Materials.Count; i++)
//      {
//        // Replace original material with the matcap material.
//        var oldMaterial = meshNode.Mesh.Materials[i];
//        var newMaterial = matcapMaterial.Clone();
//        newMaterial.Name = oldMaterial.Name;
//        meshNode.Mesh.Materials[i] = newMaterial;

//        // The original material uses a normal map in the "GBuffer" pass. 
//        // The matcap shader should use the same normal map.
//        var oldNormalTextureBinding = oldMaterial["GBuffer"].ParameterBindings["NormalTexture"];
//#if MONOGAME
//        // In MonoGame the effect parameter is of type Texture2D, but in XNA it is Texture.
//        var normalTexture = ((EffectParameterBinding<Texture2D>)oldNormalTextureBinding).Value;
//#else
//        var normalTexture = ((EffectParameterBinding<Texture>)oldNormalTextureBinding).Value;
//#endif
//        newMaterial["Default"].Set("NormalTexture", normalTexture);
//      }

//      // We have replaced the material of the mesh. The MeshNode caches several material
//      // parameters. We must force the MeshNode to load the new material info.
//      meshNode.Mesh = meshNode.Mesh;

//      _meshRenderer = new MeshRenderer();

//      var spriteFont = UIContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
//      _debugRenderer = new DebugRenderer(GraphicsService, spriteFont);

//      UpdateMatcapTexture();
//    }


//    public override void Update(GameTime gameTime)
//    {
//      // Switch matcap texture if <Up>/<Down> are pressed.
//      var currentMatcapTextureIndex = _matcapTextureIndex;
//      if (InputService.IsPressed(Keys.Up, true))
//        _matcapTextureIndex++;
//      if (InputService.IsPressed(Keys.Down, true))
//        _matcapTextureIndex--;
//      if (currentMatcapTextureIndex != _matcapTextureIndex)
//      {
//        if (_matcapTextureIndex > NumberOfMatcapTextures)
//          _matcapTextureIndex = 1;
//        if (_matcapTextureIndex < 1)
//          _matcapTextureIndex = NumberOfMatcapTextures;

//        UpdateMatcapTexture();
//      }

//      _scene.Update(gameTime.ElapsedGameTime);

//      base.Update(gameTime);
//    }


//    private void UpdateMatcapTexture()
//    {
//      // Load texture.
//      var texture = ContentManager.Load<Texture2D>(string.Format(MatcapTexturePath, _matcapTextureIndex));

//      // Replace texture in all materials.
//      var meshNode = (MeshNode)_model.Children[0];
//      foreach (var material in meshNode.Mesh.Materials)
//        material["Default"].Set("MatcapTexture", texture);

//      // Visualize texture for debugging.
//      _debugRenderer.Clear();
//      _debugRenderer.DrawTexture(texture, new Rectangle(20, 120, 200, 200));
//    }


//    private void Render(RenderContext context)
//    {
//      context.CameraNode = _cameraObject.CameraNode;
//      context.Scene = _scene;

//      var graphicsDevice = context.GraphicsService.GraphicsDevice;
//      graphicsDevice.Clear(new Color(40, 40, 40, 255));

//      graphicsDevice.DepthStencilState = DepthStencilState.Default;
//      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
//      graphicsDevice.BlendState = BlendState.Opaque;

//      var query = _scene.Query<CameraFrustumQuery>(context.CameraNode, context);

//      context.RenderPass = "Default";
//      _meshRenderer.Render(query.SceneNodes, context);
//      context.RenderPass = null;

//      _debugRenderer.Render(context);

//      context.Scene = null;
//      context.CameraNode = null;
//    }


//    protected override void Dispose(bool disposing)
//    {
//      if (disposing)
//      {
//        // IMPORTANT: Dispose scene nodes if they are no longer needed!
//        _scene.Dispose(false);  // Disposes current and all descendant nodes.

//        _meshRenderer.Dispose();
//        _debugRenderer.Dispose();

//        // Unload content.
//        // We have modified the material of the a mesh. These changes should not
//        // affect other samples. Therefore, we unload the assets. The next sample
//        // will reload them with default values.)
//        ContentManager.Unload();
//      }

//      base.Dispose(disposing);
//    }
//  }
//}
//#endif