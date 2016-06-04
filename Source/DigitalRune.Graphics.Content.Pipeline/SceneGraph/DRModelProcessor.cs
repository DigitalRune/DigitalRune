// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#if ANIMATION
using DigitalRune.Animation.Character;
#endif


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Processes a game asset mesh to a model content that is optimal for runtime.
  /// </summary>
  [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
  [ContentProcessor(DisplayName = "Model - DigitalRune Graphics")]
  public partial class DRModelProcessor : ContentProcessor<NodeContent, DRModelNodeContent>
  {
    // Notes:
    // - Mesh instancing: 
    //   The XNA content pipeline does not directly support instanced meshes. If instanced
    //   meshes are added in the future, we have to modify the DRModelProcessor to make 
    //   sure that instanced meshes are not processed twice (DRMeshNodeContent.InputMesh 
    //   can be shared.)
    // - Name "DRModelProcessor":
    //   XNA will show an error if it finds two processors with the same name in a content 
    //   project. Therefore, this processor cannot be simply named "ModelProcessor".

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Input
    private NodeContent _input;
    private ContentProcessorContext _context;

    // Optional model description (.drmdl file).
    private ModelDescription _modelDescription;

    // The model (= root node of the scene).
    private DRModelNodeContent _model;

    // Skeleton and animations
    private BoneContent _rootBone;
#if ANIMATION
    private Skeleton _skeleton;
    private Dictionary<string, SkeletonKeyFrameAnimation> _animations;
#endif

    // Vertex and index buffers
    private List<VertexBufferContent> _vertexBuffers;     // One vertex buffer for each VertexDeclaration.
    private IndexCollection _indices;                     // One index buffer for everything.
    private VertexBufferContent _morphTargetVertexBuffer; // One vertex buffer for all morph targets.
    int[][] _vertexReorderMaps;                           // Vertex reorder map to match morph target with base mesh.

    // Processed Materials:
    // If material is defined in external XML file:
    //   Key: string
    //   Value: ExternalReference<DRMaterialContent>
    // If local material is used:
    //   Key: MaterialContent
    //   Value: DRMaterialContent
    private Dictionary<object, object> _materials;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the value of the <strong>Create Missing Model Description</strong> 
    /// processor parameter.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a default model description file (*.drmdl) should be created if no
    /// model description file could be found.
    /// </value>
    [DefaultValue(true)]
    [DisplayName("Create Missing Model Description")]
    [Description("If enabled, a default model description file (*.drmdl) is created when no user-defined file was found.")]
    public bool CreateMissingModelDescription
    {
      get { return _createMissingModelDescription; }
      set { _createMissingModelDescription = value; }
    }
    private bool _createMissingModelDescription = true;


    /// <summary>
    /// Gets or sets the value of the <strong>Create Missing Material Definition</strong> 
    /// processor parameter.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if material description file (*.drmat) should be created for
    /// submeshes if no material definition file could be found.
    /// </value>
    [DefaultValue(true)]
    [DisplayName("Create Missing Material Definition")]
    [Description("If enabled, material definition files (*.drmat) are created when no user-defined file was found.")]
    public bool CreateMissingMaterialDefinition
    {
      get { return _createMissingMaterialDefinition; }
      set { _createMissingMaterialDefinition = value; }
    }
    private bool _createMissingMaterialDefinition = true;


    /*
    /// <summary>
    /// Gets or sets the value of the <strong>X Axis Rotation</strong> processor parameter.
    /// </summary>
    /// <value>The amount of rotation, in degrees, around the x-axis.</value>
    [DefaultValue(0f)]
    [DisplayName("X Axis Rotation")]
    [Description("Rotates the model a specified number of degrees around the x-axis.")]
    public virtual float RotationX
    {
      get { return _rotationX; }
      set { _rotationX = value; }
    }
    private float _rotationX;


    /// <summary>
    /// Gets or sets the value of the <strong>Y Axis Rotation</strong> processor parameter.
    /// </summary>
    /// <value>The amount of rotation, in degrees, around the y-axis.</value>
    [DefaultValue(0f)]
    [DisplayName("Y Axis Rotation")]
    [Description("Rotates the model a specified number of degrees around the y-axis.")]
    public virtual float RotationY
    {
      get { return _rotationY; }
      set { _rotationY = value; }
    }
    private float _rotationY;


    /// <summary>
    /// Gets or sets the value of the <strong>Z Axis Rotation</strong> processor parameter.
    /// </summary>
    /// <value>The amount of rotation, in degrees, around the z-axis.</value>
    [DefaultValue(0f)]
    [DisplayName("Z Axis Rotation")]
    [Description("Rotates the model a specified number of degrees around the z-axis.")]
    public virtual float RotationZ
    {
      get { return _rotationZ; }
      set { _rotationZ = value; }
    }
    private float _rotationZ;


    /// <summary>
    /// Gets or sets the value of the <strong>Scale</strong> processor parameter.
    /// </summary>
    /// <value>The scaling factor to be applied.</value>
    [DefaultValue(1f)]
    [DisplayName("Scale")]
    [Description("Scales the model uniformly along all three axes.")]
    public virtual float Scale
    {
      get { return _scale; }
      set { _scale = value; }
    }
    private float _scale = 1f;
    */
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Converts mesh content to model content.
    /// </summary>
    /// <param name="input">The root node content.</param>
    /// <param name="context">Contains any required custom process parameters.</param>
    /// <returns>The model content.</returns>
    public override DRModelNodeContent Process(NodeContent input, ContentProcessorContext context)
    {
      if (input == null)
        throw new ArgumentNullException("input");
      if (context == null)
        throw new ArgumentNullException("context");

      // The content processor may write text files. We want to use invariant culture number formats.
      // TODO: Do not set Thread.CurrentThread.CurrentCulture. Make sure that all read/write operations explicitly use InvariantCulture.
      var originalCulture = Thread.CurrentThread.CurrentCulture;
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

      try
      {
        // Uncomment this to launch and attach a debugger.
        //System.Diagnostics.Debugger.Launch();

        // Uncomment this to visualize the content tree.
        //ContentHelper.PrintContentTree(input, context);

        _context = context;

        var delayedNode = input as DeferredNodeContent;
        if (delayedNode != null)
        {
          // Model description was imported.
          _modelDescription = delayedNode.ModelDescription;

          // Load the model.
          delayedNode.Import(context);
          _input = input;
        }
        else
        {
          // The model was imported.
          _input = input;

          // Load the model description.
          var wrappedContext = new ContentPipelineContext(context);
          if (input.Identity != null && input.Identity.SourceFilename != null)
            _modelDescription = ModelDescription.Load(input.Identity.SourceFilename, wrappedContext, CreateMissingModelDescription);
        }

        if (_modelDescription != null)
          _modelDescription.Validate(_input, _context);

        ValidateInput();

        // Try to find skeleton root bone.
        _rootBone = MeshHelper.FindSkeleton(input);
        if (_rootBone != null)
        {
#if ANIMATION
          MergeAnimationFiles();
#endif
          BakeTransforms(input);
          TransformModel();
#if ANIMATION
          BuildSkeleton();
          BuildAnimations();
#endif
          SetSkinnedMaterial();
        }
        else
        {
          TransformModel();
        }

        BuildSceneGraph();
        PrepareMaterials();
        BuildMeshes();
        BuildOccluders();
        CombineLodGroups();
        ValidateOutput();

        _model.Name = Path.GetFileNameWithoutExtension(context.OutputFilename);
      }
      finally
      {
        // Clean up.
        Thread.CurrentThread.CurrentCulture = originalCulture;
      }

      return _model;
    }


    /// <summary>
    /// Bakes all node transforms of all skinned meshes into the geometry so that each node's
    /// transform is Identity. (Only bones and morph targets keep their transforms.)
    /// </summary>
    /// <param name="node">The node.</param>
    private static void BakeTransforms(NodeContent node)
    {
      if (node is BoneContent)
        return;
      if (ContentHelper.IsMorphTarget(node))
        return;

      if (ContentHelper.IsSkinned(node))
      {
        // Bake all transforms in this subtree.
        BakeAllTransforms(node);
      }
      else
      {
        // Bake transforms of skinned meshes.
        foreach (NodeContent child in node.Children)
          BakeTransforms(child);
      }
    }


    /// <summary>
    /// Bakes all node transforms in the specified subtree into the mesh geometry so that each
    /// node's transform is Identity. (Only bones and morph targets keep their transforms.)
    /// </summary>
    /// <param name="node">The node.</param>
    private static void BakeAllTransforms(NodeContent node)
    {
      if (node is BoneContent)
        return;
      if (ContentHelper.IsMorphTarget(node))
        return;

      if (node.Transform != Matrix.Identity)
      {
        MeshHelper.TransformScene(node, node.Transform);
        node.Transform = Matrix.Identity;
      }

      foreach (NodeContent child in node.Children)
        BakeAllTransforms(child);
    }


    private void TransformModel()
    {
      // Use MeshHelper to transform the whole scene node tree.
      if (_modelDescription != null)
      {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        if (_modelDescription.RotationX != 0f
            || _modelDescription.RotationY != 0f
            || _modelDescription.RotationZ != 0f
            || _modelDescription.Scale != 1f)
        {
          Matrix rotationZ = Matrix.CreateRotationZ(MathHelper.ToRadians(_modelDescription.RotationZ));
          Matrix rotationX = Matrix.CreateRotationX(MathHelper.ToRadians(_modelDescription.RotationX));
          Matrix rotationY = Matrix.CreateRotationY(MathHelper.ToRadians(_modelDescription.RotationY));
          Matrix transform = rotationZ * rotationX * rotationY * Matrix.CreateScale(_modelDescription.Scale);
          MeshHelper.TransformScene(_input, transform);
        }
        // ReSharper restore CompareOfFloatsByEqualityOperator
      }
    }
    #endregion
  }
}
