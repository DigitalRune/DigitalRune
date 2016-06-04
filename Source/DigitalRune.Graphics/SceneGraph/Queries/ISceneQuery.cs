// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a query that can be executed against a scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A scene query returns all scene nodes that touch a given reference node. Queries can be 
  /// performed by calling <see cref="IScene.Query{T}"/> of a <see cref="IScene"/>. Here are a few 
  /// examples of scene queries: 
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// A <see cref="CameraFrustumQuery"/> gets all scene nodes within the camera frustum. The 
  /// reference node in this query is (usually) a camera node.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// A <see cref="LightQuery"/> gets all lights that shine light on the reference node.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// A <see cref="ShadowCasterQuery"/> gets all shadow casters near a light source. The reference
  /// node in this query is the light node.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// Scene nodes where <see cref="SceneNode.IsEnabled"/> is <see langword="false"/> are ignored and
  /// do not show up in the query results.
  /// </para>
  /// <para>
  /// <strong>Notes to Implementors:</strong> Classes that implement <see cref="ISceneQuery"/> must 
  /// have a parameterless constructor.
  /// </para>
  /// </remarks>
  /// <example>
  /// <para>
  /// The following examples demonstrates how to create a scene query that collects 
  /// <see cref="MeshNode"/>s.
  /// </para>
  /// <code lang="csharp" title="MeshQuery">
  /// <![CDATA[
  /// using System.Collections.Generic;
  /// using DigitalRune.Graphics;
  /// using DigitalRune.Graphics.SceneGraph;
  ///
  /// namespace Samples
  /// {
  ///   public class MeshQuery : ISceneQuery
  ///   {
  ///     public SceneNode ReferenceNode { get; private set; }
  ///     public List<SceneNode> Meshes { get; private set; }
  ///
  ///     public MeshQuery()
  ///     {
  ///       Meshes = new List<SceneNode>();
  ///     }
  ///
  ///     public void Reset()
  ///     {
  ///       ReferenceNode = null;
  ///       Meshes.Clear();
  ///     }
  ///
  ///     public void Set(SceneNode referenceNode, IList<SceneNode> nodes, RenderContext context)
  ///     {
  ///       Reset();
  ///       ReferenceNode = referenceNode;
  ///
  ///       for (int i = 0; i < nodes.Count; i++)
  ///       {
  ///         var node = nodes[i];
  ///         if (node is MeshNode)
  ///           Meshes.Add(node);
  ///       }
  ///     }
  ///   }
  /// }
  /// ]]>
  /// </code>
  /// <para>
  /// The query can, for example, be used to get all meshes within the camera frustum.
  /// <code lang="csharp">
  /// <![CDATA[
  /// ISceneQuery query = myScene.Query<MeshQuery>(cameraNode, renderContext);
  /// ]]>
  /// </code>
  /// </para>
  /// <para>
  /// <strong>Distance Culling:</strong><br/>
  /// The following example shows how to implement a scene query that performs <i>distance 
  /// culling</i> of scene nodes.
  /// </para>
  /// <code lang="csharp" title="Scene Query with Distance Culling">
  /// <![CDATA[
  /// using System.Collections.Generic;
  /// using DigitalRune.Graphics;
  /// using DigitalRune.Graphics.SceneGraph;
  /// using DigitalRune.Mathematics;
  /// 
  /// namespace Samples
  /// {
  ///   public class SceneQueryWithDistanceCulling : ISceneQuery
  ///   {
  ///     public SceneNode ReferenceNode { get; private set; }
  ///     public List<SceneNode> Nodes { get; private set; }
  ///
  ///     public SceneQueryWithDistanceCulling()
  ///     {
  ///       Nodes = new List<SceneNode>();
  ///     }
  ///
  ///     public void Reset()
  ///     {
  ///       ReferenceNode = null;
  ///       Nodes.Clear();
  ///     }
  ///
  ///     public void Set(SceneNode referenceNode, IList<SceneNode> nodes, RenderContext context)
  ///     {
  ///       Reset();
  ///       ReferenceNode = referenceNode;
  ///
  ///       var cameraNode = context.LodCameraNode;
  ///       if (cameraNode == null)
  ///         throw new GraphicsException("LOD camera node needs to be set in render context.");
  /// 
  ///       for (int i = 0; i < nodes.Count; i++)
  ///       {
  ///         var node = nodes[i];
  ///
  ///         // Calculate view-normalized distance of scene node.
  ///         float distance = GraphicsHelper.GetViewNormalizedDistance(node, cameraNode);
  ///         distance *= cameraNode.LodBias * context.LodBias;
  /// 
  ///         // Distance Culling: Check whether scene node is within MaxDistance.
  ///         if (Numeric.IsPositiveFinite(node.MaxDistance) && distance >= node.MaxDistance)
  ///           continue;   // Ignore scene node.
  ///         
  ///         Nodes.Add(node);
  ///       }
  ///     }
  ///   }
  /// }
  /// ]]>
  /// </code>
  /// <para>
  /// <strong>Level of Detail:</strong><br/>
  /// Level of detail (see <see cref="LodGroupNode"/>) needs to be evaluated by the scene query. The 
  /// following example implements distance culling and LOD selection.
  /// </para>
  /// <code lang="csharp" title="Scene Query with Level Of Detail">
  /// <![CDATA[
  /// using System.Collections.Generic;
  /// using DigitalRune.Graphics;
  /// using DigitalRune.Graphics.SceneGraph;
  /// using DigitalRune.Mathematics;
  /// 
  /// namespace Samples
  /// {
  ///   public class SceneQueryWithLod : ISceneQuery
  ///   {
  ///     public SceneNode ReferenceNode { get; private set; }
  ///     public List<SceneNode> Nodes { get; private set; }
  ///
  ///     public SceneQueryWithLod()
  ///     {
  ///       Nodes = new List<SceneNode>();
  ///     }
  ///
  ///     public void Reset()
  ///     {
  ///       ReferenceNode = null;
  ///       Nodes.Clear();
  ///     }
  ///
  ///     public void Set(SceneNode referenceNode, IList<SceneNode> nodes, RenderContext context)
  ///     {
  ///       Reset();
  ///       ReferenceNode = referenceNode;
  ///
  ///       if (context.LodCameraNode == null)
  ///         throw new GraphicsException("LOD camera node needs to be set in render context.");
  /// 
  ///       for (int i = 0; i < nodes.Count; i++)
  ///         AddNode(nodes[i], context);
  ///     }
  /// 
  ///     private void AddNode(SceneNode node, RenderContext context)
  ///     {
  ///       var cameraNode = context.LodCameraNode;
  /// 
  ///       // Calculate view-normalized distance.
  ///       float distance = GraphicsHelper.GetViewNormalizedDistance(node, cameraNode);
  ///       distance *= cameraNode.LodBias * context.LodBias;
  /// 
  ///       // Distance Culling: Check whether scene node is within MaxDistance.
  ///       if (Numeric.IsPositiveFinite(node.MaxDistance) && distance >= node.MaxDistance)
  ///         return;   // Ignore scene node.
  /// 
  ///       var lodGroupNode = node as LodGroupNode;
  ///       if (lodGroupNode != null)
  ///       {
  ///         // Evaluate LOD group.
  ///         var lodSelection = lodGroupNode.SelectLod(context, distance);
  ///         AddSubtree(lodSelection.Current, context);
  ///       }
  ///       else
  ///       {
  ///         Nodes.Add(node);
  ///       }
  ///     }
/// 
  ///     private void AddSubtree(SceneNode node, RenderContext context)
  ///     {
  ///       if (node.IsEnabled)
  ///       {
  ///         AddNode(node, context);
  /// 
  ///         if (node.Children != null)
  ///         foreach (var childNode in node.Children)
  ///           AddSubtree(childNode, context);
  ///       }
  ///     }
  ///   }
  /// }
  /// ]]>
  /// </code>
  /// </example>
  /// <seealso cref="IScene"/>
  /// <seealso cref="Scene"/>
  /// <seealso cref="SceneNode"/>
  public interface ISceneQuery
  {
    /// <summary>
    /// Gets the reference node.
    /// </summary>
    /// <value>The reference node.</value>
    SceneNode ReferenceNode { get; }


    /// <summary>
    /// Resets this query.
    /// </summary>
    /// <remarks>
    /// <see cref="ReferenceNode"/> is set to <see langword="null"/>, and any cached results are 
    /// cleared.
    /// </remarks>
    void Reset();


    /// <summary>
    /// Sets the query result. 
    /// </summary>
    /// <param name="referenceNode">The reference node; can be <see langword="null"/>.</param>
    /// <param name="nodes">
    /// The scene nodes that touch the reference node. (Note to caller: <paramref name="nodes"/>
    /// should not contain disabled scene nodes!)
    /// </param>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// This method is called by the scene to store the result of the query.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
    void Set(SceneNode referenceNode, IList<SceneNode> nodes, RenderContext context);
  }
}
