// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
#if ANIMATION
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Animation.Character;
#endif


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a 3D model composed of multiple <see cref="SceneNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="ModelNode"/> represents a 3D model. The complexity of a model can vary from a 
  /// simple <see cref="MeshNode"/> to a complex hierarchical tree of <see cref="MeshNode"/>s, 
  /// <see cref="LightNode"/>s, and other types of <see cref="SceneNode"/>s. The 
  /// <see cref="ModelNode"/> basically represents a "mini scene" stored under a single scene node. 
  /// - <see cref="ModelNode"/> is the root node of this scene.
  /// </para>
  /// <para>
  /// <strong>Loading and Cloning a Model:</strong><br/>
  /// A model can be loaded from an asset that has been preprocessed by the XNA content pipeline. 
  /// For example:
  /// <code lang="csharp">
  /// <![CDATA[
  /// ModelNode myModel = Content.Load<ModelNode>("dude");
  /// ]]>
  /// </code>
  /// Repeated calls of <c>Content.Load&lt;ModelNode&gt;("dude")</c> will return the same object 
  /// instance. (The model asset needs to be preprocessed by using the 
  /// <strong>DRModelProcessor</strong> which is listed as 
  /// <strong>"Model - DigitalRune Graphics"</strong> in the Visual Studio Properties window.)
  /// </para>
  /// <para>
  /// The model node can be inserted in a <see cref="IScene"/> for rendering. The model node cannot 
  /// be inserted multiple times into a scene. Also, it cannot be inserted into multiple scenes. 
  /// </para>
  /// <para>
  /// To render a model multiple times in a scene the model needs to be cloned by calling 
  /// <see cref="Clone"/>:
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Clone the entire model.
  /// ModelNode clonedModel = myModel.Clone();
  /// ]]>
  /// </code>
  /// <para>
  /// <see cref="Clone"/> duplicates the <see cref="ModelNode"/> and its descendant nodes. The data 
  /// objects referenced by the scene nodes, such as <see cref="Mesh"/> or <see cref="Light"/>, are 
  /// not duplicated. The original model and the cloned model will references the same data objects.
  /// Only the instance information which is responsible for positioning the objects in the scene 
  /// will be duplicated.)
  /// </para>
  /// <para>
  /// <strong>Adding a Model to a scene:</strong><br/>
  /// To render a model, it needs to be added to a scene. <see cref="ModelNode"/> is derived from 
  /// <see cref="SceneNode"/>, hence it can be attached to any node of a scene. For example, the 
  /// following code inserts a model node under the root node of a scene.
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Insert model into scene.
  /// myScene.Children.Add(myModel);
  /// ]]>
  /// </code>
  /// </para>
  /// </remarks>
  public class ModelNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    internal void OnAssetLoaded(object sender, EventArgs eventArgs)
    {
#if ANIMATION
      // Create MeshNode.SkeletonPoses for all mesh.Skeletons. 
      // (Skeletons can be shared and for each skeleton we create only one SkeletonPose.)
      Dictionary<Skeleton, SkeletonPose> skeletons = new Dictionary<Skeleton,SkeletonPose>();
      foreach (var meshNode in this.GetSubtree().OfType<MeshNode>())
      {
        var skeleton = meshNode.Mesh.Skeleton;
        if (skeleton != null)
        {
          // Get existing skeleton pose or create a new one.
          SkeletonPose skeletonPose;
          if (!skeletons.TryGetValue(skeleton, out skeletonPose))
          {
            skeletonPose = SkeletonPose.Create(skeleton);
            skeletons.Add(skeleton, skeletonPose);
          }

          meshNode.SkeletonPose = skeletonPose;
        }
      }
#endif
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new ModelNode Clone()
    {
      return (ModelNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new ModelNode();
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

#if ANIMATION
      // Each clone of a MeshNode creates its own SkeletonPose clone, but
      // if the SkeletonPoses are shared in the source, then the clone
      // should also use shared SkeletonPoses.
      if (Children != null && Children.Count > 0)
      {
        var originalMeshNodes = source.GetDescendants().OfType<MeshNode>().ToArray();
        var clonedMeshNodes = this.GetDescendants().OfType<MeshNode>().ToArray();

        // Dictionary of original skeleton poses and their clones.
        var skeletonPoses = new Dictionary<SkeletonPose, SkeletonPose>();

        for (int i = 0; i < originalMeshNodes.Length; i++)
        {
          var sourceNode = originalMeshNodes[i];
          var clonedNode = clonedMeshNodes[i];

          if (sourceNode.SkeletonPose == null)
            continue;

          SkeletonPose clonedSkeletonPose;
          if (!skeletonPoses.TryGetValue(sourceNode.SkeletonPose, out clonedSkeletonPose))
          {
             // Register existing clone (which was created in base.CloneCore()).
            skeletonPoses.Add(sourceNode.SkeletonPose, clonedNode.SkeletonPose);
          }
          else
          {
            // Use the shared skeleton pose in the clone.
            clonedNode.SkeletonPose = clonedSkeletonPose;
          }
        }
      }
#endif
    }
    #endregion

    #endregion
  }
}
