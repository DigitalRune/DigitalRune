using System;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples shows how to manually apply effect bindings and render a mesh without the 
MeshRenderer class.",
    @"",
    1000)]
  public class ManualMeshRenderSample : Sample
  {
    private readonly CameraObject _cameraObject;

    // A DigitalRune model.
    private readonly ModelNode _model;
    private Scene _scene; 


    public ManualMeshRenderSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      var delegateGraphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, delegateGraphicsScreen);

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      GameObjectService.Objects.Add(_cameraObject);

      _scene = new Scene();
      SceneSample.InitializeDefaultXnaLights(_scene);

      // For advanced users: Set this flag if you want to analyze the imported opaque data of
      // effect bindings.
      EffectBinding.KeepOpaqueData = true;

      _model = ContentManager.Load<ModelNode>("Dude/Dude").Clone();
      var meshNode = _model.GetSubtree().OfType<MeshNode>().First();
      meshNode.ScaleLocal = new Vector3F(1, 2, 1);
      var mesh = meshNode.Mesh;
      var timeline = new TimelineClip(mesh.Animations.Values.First())
      {
        Duration = TimeSpan.MaxValue,
        LoopBehavior = LoopBehavior.Cycle,
      };
      AnimationService.StartAnimation(timeline, (IAnimatableProperty)meshNode.SkeletonPose);
    }


    private void Render(RenderContext context)
    {
      // Set the current camera node in the render context. This info is used 
      // by the renderers.
      context.CameraNode = _cameraObject.CameraNode;
      context.Scene = _scene;

      var device = context.GraphicsService.GraphicsDevice;

      device.Clear(Color.CornflowerBlue);

      device.DepthStencilState = DepthStencilState.Default;
      device.RasterizerState = RasterizerState.CullCounterClockwise;
      device.BlendState = BlendState.Opaque;

      context.RenderPass = "Default";
      foreach (var meshNode in _model.GetSubtree().OfType<MeshNode>())
      {
        context.SceneNode = meshNode;

        foreach (var submesh in meshNode.Mesh.Submeshes)
        {
          var materialIndex = submesh.MaterialIndex;
          var materialInstance = meshNode.MaterialInstances[materialIndex];
          var material = materialInstance.Material;
          var materialInstanceBinding = materialInstance[context.RenderPass];
          var materialBinding = material[context.RenderPass];
          var effect = materialBinding.Effect;

          context.MaterialBinding = materialBinding;
          context.MaterialInstanceBinding = materialInstanceBinding;

          // Update and apply global bindings.
          foreach (var binding in effect.GetParameterBindings())
          {
            if (binding.Description.Hint == EffectParameterHint.Global)
            {
              binding.Update(context);
              binding.Apply(context);
            }
          }

          // Update and apply material bindings.
          foreach (var binding in materialBinding.ParameterBindings)
          {
            binding.Update(context);
            binding.Apply(context);
          }

          // Update and apply local and per-instance bindings.
          foreach (var binding in materialInstanceBinding.ParameterBindings)
          {
            if (binding.Description.Hint != EffectParameterHint.PerPass)
            {
              binding.Update(context);
              binding.Apply(context);
            }
          }

          // Select and apply effect technique.
          var techniqueBinding = materialInstanceBinding.TechniqueBinding;
          techniqueBinding.Update(context);
          var technique = techniqueBinding.GetTechnique(effect, context);
          effect.CurrentTechnique = technique;

          // Select and apply effect passes.
          var passBinding = techniqueBinding.GetPassBinding(technique, context);
          foreach (var pass in passBinding)
          {
            // Update and apply per-pass bindings.
            foreach (var binding in materialInstanceBinding.ParameterBindings)
            {
              if (binding.Description.Hint == EffectParameterHint.PerPass)
              {
                binding.Update(context);
                binding.Apply(context);
              }
            }

            pass.Apply();
            submesh.Draw();
          }


          context.MaterialBinding = null;
          context.MaterialInstanceBinding = null;
        }
      }
      context.SceneNode = null;
      context.RenderPass = null;

      // Clean up.
      context.CameraNode = null;
      context.Scene = null;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // IMPORTANT: Always dispose scene nodes if they are no longer needed!
        _model.Dispose(false);  // Note: This statement disposes only our local clone.
                                // The original instance is still available in the 
                                // ContentManager.
      }

      base.Dispose(disposing);
    }
  }
}
