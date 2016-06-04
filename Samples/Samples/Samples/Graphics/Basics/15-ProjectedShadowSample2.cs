using System;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Plane = DigitalRune.Geometry.Shapes.Plane;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to render planar projected shadows using the existing MeshRenderer,
a custom effect and custom effect parameter bindings.",
    @"This sample renders planar projected shadows like the ProjectShadowSample. However, this
sample adds support for projected shadows of skinned meshes.
The previous ProjectShadowSample uses a custom renderer, the ProjectedShadowRenderer, to create
shadows for MeshNodes. The renderer is simple, it needs only the existing XNA BasicEffect and can
render any MeshNode. However, the BasicEffect does not support mesh skinning or other special shader
effects.
This sample uses a different approach to create planar shadows:
A new render pass 'ProjectedShadow' is added.
A custom effect is used, see <Samples folder>\Content\DudeWithProjectedShadow\ProjectedShadowSkinned.fx.
The dude model in the folder <Samples folder>\Content\DudeWithProjectedShadow\ uses a special
materials (see *.drmat files) which support the 'ProjectedShadow' render pass.
For example, here is the material file for the dude's pants: Pants.drmat

  <?xml version=""1.0"" encoding=""utf-8""?>
  <Material>
    <Pass Name=""Default"" Effect=""SkinnedEffect"" Profile=""Any"">
      <Parameter Name=""DiffuseColor"" Value=""1,1,1"" />
      <Parameter Name=""SpecularColor"" Value=""0.1,0.1,0.1"" />
      <Parameter Name=""SpecularPower"" Value=""10"" />
      <Texture Name=""Texture"" File=""../Dude/pants.tga"" />
    </Pass>
    <Pass Name=""ProjectedShadow"" Effect=""ProjectedShadowSkinned.fx"" Profile=""Any"" />
  </Material>

This material tells the renderer to use the SkinnedEffect in the 'Default' render pass and the new
'ProjectedShadowSkinned' effect in the 'ProjectedShadow' pass. (Please note, the names of the render
passes can be chosen by the application and have no predefined meaning in DigitalRune Graphics.)

To render the projected shadows the existing MeshRenderer is used (no custom renderer needed):

  context.RenderPass = ""ProjectedShadow"";
  _meshRenderer.Render(query.SceneNodes, context);
  context.RenderPass = null;

The projected shadow effect (ProjectedShadowSkinned.fx) uses several effect parameters:

  float4x4 World;
  float4x4 ViewProjection;
  float4x3 Bones[72];
  float4x4 ShadowMatrix;
  float4 ShadowColor;

When the MeshRenderer renders the mesh nodes, it has to set these effect parameters. This is done
using effect parameter bindings. Effect parameter bindings are created when the model is loaded and
used by the MeshRenderer to set the effect parameters in each frame.
ConstParameterBindings supply a constant value for a parameter, e.g. the 'DiffuseColor' of the 'Default'
render pass is a constant value which is defined in the drmat file. DelegateParameterBindings use
a callback to compute the correct value before the mesh is rendered, e.g. the 'World' parameter is
automatically set to the current pose and scale of the mesh node in each frame.
DigitalRune Graphics knows common effect parameters, like 'DiffuseColor', 'World', 'Bones', etc. It
automatically creates effect parameter bindings for these parameters. (For unknown parameters it will
check the drmat file for a constant parameter value.)
We want to set 'ShadowMatrix' and 'ShadowColor' using callbacks. Therefore, this sample tells the
graphics service to create DelegateParameterBindings for these parameters.

See source code for more details.",
    15)]
  public class ProjectedShadowSample2 : Sample
  {
    // To avoid overdraw: Only draw if stencil buffer contains 0, write 1 to stencil buffer.
    private readonly DepthStencilState StencilNoOverdraw = new DepthStencilState
    {
      DepthBufferEnable = true,
      DepthBufferWriteEnable = false,
      StencilEnable = true,
      StencilPass = StencilOperation.Replace,
      StencilFunction = CompareFunction.Greater,
      ReferenceStencil = 1
    };


    private readonly CameraObject _cameraObject;

    private readonly Scene _scene;

    private AnimationController _animationController;

    private readonly MeshRenderer _meshRenderer;

    private readonly LightNode _mainDirectionalLightNode;
    private float _lightAngle;

    private Vector4 _shadowColor;
    private Matrix _shadowMatrix;


    public ProjectedShadowSample2(Microsoft.Xna.Framework.Game game)
    : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      var delegateGraphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, delegateGraphicsScreen);

      // The dude model uses a new ProjectedShadowSkinned.fx effect. This effect contains new 
      // parameters 'ShadowMatrix' and 'ShadowColor' which are not yet supported. When an mesh is 
      // loaded via the content manager, effect bindings are automatically created. This is done
      // by effect interpreters and effect binders. The graphics service uses several predefined
      // effect interpreter and binder classes to support the most common effect parameters. E.g.
      // the SceneEffectInterpreter and SceneEffectBinder handle parameters like 'World', 'View',
      // 'ViewProjection', 'CameraPosition', 'FogColor', etc. (see also class 
      // SceneEffectParameterSemantics).
      // We can add new effect interpreters/binders or we can add an entry to an existing 
      // interpreter/binder. Let's add entries to the standard SceneEffectInterpreter which creates 
      // meta-data for the new parameters:
      var sceneEffectInterpreter = GraphicsService.EffectInterpreters.OfType<SceneEffectInterpreter>().First();
      sceneEffectInterpreter.ParameterDescriptions.Add(
        "ShadowMatrix",
        (parameter, index) => new EffectParameterDescription(parameter, "ShadowMatrix", index, EffectParameterHint.Global));
      sceneEffectInterpreter.ParameterDescriptions.Add(
        "ShadowColor",
        (parameter, index) => new EffectParameterDescription(parameter, "ShadowColor", index, EffectParameterHint.Global));

      // Add entries to the standard SceneEffectBinder which create DelegateParameterBindings for 
      // the new parameters. The delegate bindings use callback methods to compute the parameter
      // value.
      var sceneEffectBinder = GraphicsService.EffectBinders.OfType<SceneEffectBinder>().First();
      sceneEffectBinder.MatrixBindings.Add(
        "ShadowMatrix",
        (effect, parameter, data) => new DelegateParameterBinding<Matrix>(effect, parameter, GetShadowMatrix));
      sceneEffectBinder.Vector4Bindings.Add(
        "ShadowColor",
        (effect, parameter, data) => new DelegateParameterBinding<Vector4>(effect, parameter, GetShadowColor));

      // Create a new empty scene.
      _scene = new Scene();
      Services.Register(typeof(IScene), null, _scene);

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      _cameraObject.ResetPose(new Vector3F(-2, 2, 2), -ConstantsF.PiOver4, -0.4f);
      GameObjectService.Objects.Add(_cameraObject);

      // Add a default light setup (ambient light + 3 directional lights).
      var defaultLightsObject = new DefaultLightsObject(Services);
      GameObjectService.Objects.Add(defaultLightsObject);

      // Get the main directional light.
      _mainDirectionalLightNode = ((LightNode)_scene.GetSceneNode("KeyLight"));

      // Add a ground plane model to the scene graph.
      var grid = ContentManager.Load<ModelNode>("Ground/Ground").Clone();
      grid.ScaleLocal = new Vector3F(0.3f);
      _scene.Children.Add(grid);

      // Add a dude model to the scene graph.
      var dude = ContentManager.Load<ModelNode>("DudeWithProjectedShadow/Dude").Clone();
      dude.PoseWorld = new Pose(Matrix33F.CreateRotationY(ConstantsF.Pi));
      SampleHelper.EnablePerPixelLighting(dude);
      _scene.Children.Add(dude);

      // Start walk animation.
      StartDudeAnimation(dude);

      // Create the renderers.
      _meshRenderer = new MeshRenderer();

      _shadowColor = new Vector4(0, 0, 0, 0.4f);
    }


    // A callback for a DelegateParameterBinding which provides the shadow matrix.
    private Matrix GetShadowMatrix(DelegateParameterBinding<Matrix> delegateParameterBinding, RenderContext renderContext)
    {
      return _shadowMatrix;
    }


    // A callback for a DelegateParameterBinding which provides the shadow color.
    private Vector4 GetShadowColor(DelegateParameterBinding<Vector4> delegateParameterBinding, RenderContext renderContext)
    {
      return _shadowColor;
    }


    private void StartDudeAnimation(ModelNode dude)
    {
      // The dude model contains a single mesh node.
      var meshNode = (MeshNode)dude.Children[0];

      // The imported animation data (skeleton and animations) is stored with the mesh.
      var animations = meshNode.Mesh.Animations;

      // The MeshNodes of skinned models has a SkeletonPose which can be animated.
      // Let's start the first animation.
      var timeline0 = new TimelineClip(animations.Values.First())
      {
        LoopBehavior = LoopBehavior.Cycle, // Loop animation...
        Duration = TimeSpan.MaxValue,      // ...forever.
      };
      _animationController = AnimationService.StartAnimation(timeline0, (IAnimatableProperty)meshNode.SkeletonPose);
      _animationController.UpdateAndApply();
    }


    public override void Update(GameTime gameTime)
    {
      // Move the directional light in a circle.
      float deltaTimeF = (float)gameTime.ElapsedGameTime.TotalSeconds;
      _lightAngle += 0.3f * deltaTimeF;
      var position = QuaternionF.CreateRotationY(_lightAngle).Rotate(new Vector3F(6, 6, 0));

      // Make the light look at the world space origin.
      var lightTarget = Vector3F.Zero;
      var lookAtMatrix = Matrix44F.CreateLookAt(position, lightTarget, Vector3F.Up);

      // A look-at matrix is the inverse of a normal world or pose matrix.
      _mainDirectionalLightNode.PoseWorld =
        new Pose(lookAtMatrix.Translation, lookAtMatrix.Minor).Inverse;

      // Compute shadow matrix for the new light direction.
      var lightRayDirection = (lightTarget - position);
      _shadowMatrix = ProjectedShadowRenderer.CreateShadowMatrix(
        new Plane(new Vector3F(0, 1, 0), 0.01f), new Vector4F(-lightRayDirection, 0));

      // Update the scene - this must be called once per frame.
      _scene.Update(gameTime.ElapsedGameTime);

      base.Update(gameTime);
    }


    private void Render(RenderContext context)
    {
      // Set render context info.
      context.CameraNode = _cameraObject.CameraNode;
      context.Scene = _scene;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.CornflowerBlue);

      // Frustum culling: Get all scene nodes which overlap the view frustum.
      var query = _scene.Query<CameraFrustumQuery>(context.CameraNode, context);

      // Default render states for opaque meshes.
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;
      graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

      // Render the meshes using the "Default" render pass.
      context.RenderPass = "Default";
      _meshRenderer.Render(query.SceneNodes, context);
      context.RenderPass = null;

      // Render state for planar projected shadows.
      // We use stencil to avoid overdraw (which is a problem for transparent shadows).
      graphicsDevice.DepthStencilState = StencilNoOverdraw;
      // Cull back faces as usual.
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      // Use a alpha blending, because we shadows are usually transparent.
      graphicsDevice.BlendState = BlendState.AlphaBlend;

      // Render the meshes using the "ProjectedShadow" render pass.
      context.RenderPass = "ProjectedShadow";
      _meshRenderer.Render(query.SceneNodes, context);
      context.RenderPass = null;

      // Clean up.
      context.Scene = null;
      context.CameraNode = null;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Remove effect interpreter/binder entries for ShadowMatrix and ShadowColor.
        var sceneEffectInterpreter = GraphicsService.EffectInterpreters.OfType<SceneEffectInterpreter>().First();
        sceneEffectInterpreter.ParameterDescriptions.Remove("ShadowMatrix");
        sceneEffectInterpreter.ParameterDescriptions.Remove("ShadowColor");
        var sceneEffectBinder = GraphicsService.EffectBinders.OfType<SceneEffectBinder>().First();
        sceneEffectBinder.MatrixBindings.Remove("ShadowMatrix");
        sceneEffectBinder.Vector4Bindings.Remove("ShadowColor");

        // Unload content.
        // (When the DudeWithProjectedShadow model was loaded, the effect bindings for ShadowMatrix
        // and ShadowColor where initialized to use callbacks of this class. The next time a 
        // DudeWithProjectedShadow is loaded via the content manager, it will reuse the same cached
        // mesh, material and effect bindings. Since this class is being removed, the callbacks are
        // now invalid and we must make sure that the next time a DudeWithProjectedShadow model is 
        // loaded, a new instance with new effect parameter bindings is created.)
        ContentManager.Unload();

        _animationController.Stop();
        _animationController.Recycle();

        // IMPORTANT: Dispose scene nodes if they are no longer needed!
        _scene.Dispose(false);  // Disposes current and all descendant nodes.

        // Dispose renderers.
        _meshRenderer.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
