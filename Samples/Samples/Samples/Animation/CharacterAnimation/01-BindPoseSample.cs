using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    @"This sample renders the Dude and the PlayerMarine model in their bind poses.",
    @"The skeleton is drawn for debugging. This sample is useful to analyze the skeletons, check
bone name, bone indices and bone coordinate systems.",
    51)]
  public class BindPoseSample : CharacterAnimationSample
  {
    // The mesh node of the dude.
    private readonly MeshNode _dudeMeshNode;

    // The marine mesh node.
    private readonly MeshNode _marineMeshNode;


    public BindPoseSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Load dude model node.
      // This model uses the DigitalRune Model Processor. Several XML files (*.drmdl 
      // and *.drmat) in the folder of dude.fbx define the materials and other properties. 
      // The DigitalRune Model Processor also imports the animations of the dude model
      // and the *.drmdl can be used to specify how animations should be processed in 
      // the content pipeline.
      // The *.drmat files define the used effects and effect parameters. The effects 
      // must support mesh skinning.
      var sharedDudeModelNode = ContentManager.Load<ModelNode>("Dude/Dude");

      // Clone the dude model because objects returned by the ContentManager
      // are shared instances, and we do not want manipulate or animate this shared instance.
      var dudeModelNode = sharedDudeModelNode.Clone();

      // The loaded dude model is a scene graph which consists of a ModelNode
      // which has a single MeshNode as its child.
      _dudeMeshNode = (MeshNode)dudeModelNode.Children[0];
      // We could also get the MeshNode by name:
      _dudeMeshNode = (MeshNode)dudeModelNode.GetSceneNode("him");
      // Or using a more general LINQ query:
      _dudeMeshNode = dudeModelNode.GetSubtree().OfType<MeshNode>().First();

      // Set the world space position and orientation of the dude.
      _dudeMeshNode.PoseLocal = new Pose(new Vector3F(-1f, 0, 0));

      // The imported Mesh of the Dude has a Skeleton, which defines the bone hierarchy.
      var skeleton = _dudeMeshNode.Mesh.Skeleton;

      // The imported MeshNode has a SkeletonPose, which defines the current animation pose
      // (transformations of the bones). The default skeleton pose is the bind pose 
      // where all bone transformations are set to an identity transformation (no scale, 
      // no rotation, no translation).
      var skeletonPose = _dudeMeshNode.SkeletonPose;

      // Load the marine model:
      var marineModelNode = ContentManager.Load<ModelNode>("Marine/PlayerMarine").Clone();
      _marineMeshNode = marineModelNode.GetSubtree().OfType<MeshNode>().First();
      _marineMeshNode.PoseLocal = new Pose(new Vector3F(1f, 0, 0));

      // Enable per-pixel lighting.
      SampleHelper.EnablePerPixelLighting(_dudeMeshNode);
      SampleHelper.EnablePerPixelLighting(_marineMeshNode);

      // Add the to the scene graph, so that they are drawn by the graphics screen.
      // We can add the ModelNodes directly to the scene.
      //GraphicsScreen.Scene.Children.Add(dudeModelNode);
      //GraphicsScreen.Scene.Children.Add(marineModelNode);
      // Alternatively, we can detach the MeshNodes from their parent nodes and
      // add them directly to the scene graph. The parent ModelNodes basically empty
      // nodes, which are only used to load and group other nodes.
      _dudeMeshNode.Parent.Children.Remove(_dudeMeshNode);
      GraphicsScreen.Scene.Children.Add(_dudeMeshNode);
      _marineMeshNode.Parent.Children.Remove(_marineMeshNode);
      GraphicsScreen.Scene.Children.Add(_marineMeshNode);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // Visualize bones for debugging using the DigitalRune Graphics DebugRenderer.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      debugRenderer.DrawSkeleton(_dudeMeshNode, 0.1f, Color.Orange, true);
      debugRenderer.DrawSkeleton(_marineMeshNode, 0.1f, Color.Orange, true);
    }


    /* If you want to modify the debug rendering or if you want to know how it works, 
       here is a DrawBones() method that produces a similar result as DebugRenderer.DrawSkeleton().
     
    /// <summary>
    /// Draws the skeleton bones, bone space axes and bone names for debugging. 
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="effect">
    /// An initialized basic effect instance. BasicEffect.World, BasicEffect.View and 
    /// BasicEffect.Projection must be correctly initialized before this method is called.
    /// </param>
    /// <param name="axisLength">The visible length of the bone space axes.</param>
    /// <param name="spriteBatch"> A SpriteBatch. Can be null to skip text rendering.  </param>
    /// <param name="spriteFont"> A SpriteFont. Can be null to skip text rendering.  </param>
    /// <param name="color">The color for the bones and the bone names.</param>
    private static void DrawBones(SkeletonPose skeletonPose, GraphicsDevice graphicsDevice,
      BasicEffect effect, float axisLength, SpriteBatch spriteBatch, SpriteFont spriteFont,
      Color color)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (effect == null)
        throw new ArgumentNullException("effect");
 
      var oldVertexColorEnabled = effect.VertexColorEnabled;
      effect.VertexColorEnabled = true;
 
      // No font, then we don't need the sprite batch.
      if (spriteFont == null)
        spriteBatch = null;
 
      if (spriteBatch != null)
        spriteBatch.Begin();
 
      List<VertexPositionColor> vertices = new List<VertexPositionColor>();
 
      var skeleton = skeletonPose.Skeleton;
      for (int i = 0; i < skeleton.NumberOfBones; i++)
      {
        // Data of bone i:
        string name = skeleton.GetName(i);
        SrtTransform bonePose = skeletonPose.GetBonePoseAbsolute(i);
        var translation = (Vector3)bonePose.Translation;
        var rotation = (Quaternion)bonePose.Rotation;
 
        // Draw line to parent joint representing the parent bone.
        int parentIndex = skeleton.GetParent(i);
        if (parentIndex >= 0)
        {
          SrtTransform parentPose = skeletonPose.GetBonePoseAbsolute(parentIndex);
          vertices.Add(new VertexPositionColor(translation, color));
          vertices.Add(new VertexPositionColor((Vector3)parentPose.Translation, color));
        }
 
        // Add three lines in Red, Green and Blue that visualize the bone space.
        vertices.Add(new VertexPositionColor(translation, Color.Red));
        vertices.Add(new VertexPositionColor(
          translation + Vector3.Transform(Vector3.UnitX, rotation) * axisLength, Color.Red));
        vertices.Add(new VertexPositionColor(translation, Color.Green));
        vertices.Add(new VertexPositionColor(
          translation + Vector3.Transform(Vector3.UnitY, rotation) * axisLength, Color.Green));
        vertices.Add(new VertexPositionColor(translation, Color.Blue));
        vertices.Add(new VertexPositionColor(
          translation + Vector3.Transform(Vector3.UnitZ, rotation) * axisLength, Color.Blue));
 
        // Draw name.
        if (spriteBatch != null && !string.IsNullOrEmpty(name))
        {
          // Compute the 3D position in view space. Text is rendered near drawn x axis.
          Vector3 textPosition = translation + Vector3.TransformNormal(Vector3.UnitX, bonePose)
                                 * axisLength * 0.5f;
          var textPositionWorld = Vector3.Transform(textPosition, effect.World);
          var textPositionView = Vector3.Transform(textPositionWorld, effect.View);
 
          // Check if the text is in front of the camera.
          if (textPositionView.Z < 0)
          {
            // Project text position to screen.
            Vector3 textPositionProjected = graphicsDevice.Viewport.Project(
              textPosition, effect.Projection, effect.View, effect.World);
 
            spriteBatch.DrawString(spriteFont, name + " " + i,
              new Vector2(textPositionProjected.X, textPositionProjected.Y), color);
          }
        }
      }
 
      if (spriteBatch != null)
        spriteBatch.End();
 
      // Draw axis lines in one batch.
      graphicsDevice.DepthStencilState = DepthStencilState.None;
      effect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices.ToArray(), 0,
        vertices.Count / 2);
 
      effect.VertexColorEnabled = oldVertexColorEnabled;
    }
    */
  }
}
