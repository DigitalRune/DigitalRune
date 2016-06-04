using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample shows how to use the XNA DualTextureEffect.",
    "",
    22)]
  public class DualTextureEffectSample : BasicSample
  {
    public DualTextureEffectSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3F(8, 6, 8), ConstantsF.PiOver4, -0.4f);

      // Load the dual-textured model. This model is processed using the DigitalRune 
      // Model Processor - not the default XNA model processor!
      // In the folder that contains model_lightmap.fbx, there are several XML files (*.drmdl and *.drmat) 
      // which define the materials of the model. These material description files are 
      // automatically processed by the DigitalRune Model Processor. Please browse 
      // to the content folder and have a look at the *.drmdl and *.drmat files.
      // The model itself is a tree of scene nodes. This model contains several 
      // mesh nodes for the floor plane and the cubes.
      ModelNode modelNode = ContentManager.Load<ModelNode>("DualTextured/model_lightmap");

      // The XNA ContentManager manages a single instance of each model. We clone 
      // the models, to get a copy that we can modify without changing the original 
      // instance. - This is good practice, even if we do not currently modify the 
      // model in this sample.
      modelNode = modelNode.Clone();

      modelNode.ScaleLocal = new Vector3F(0.5f);

      // Add the model to the scene.
      GraphicsScreen.Scene.Children.Add(modelNode);
    }
  }
}
