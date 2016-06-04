// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  // A light query for global lights.
  internal sealed class GlobalLightQuery : ISceneQuery
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public SceneNode ReferenceNode { get; private set; }


    /// <summary>
    /// Gets the ambient lights.
    /// </summary>
    /// <value>The ambient lights.</value>
    public List<LightNode> AmbientLights { get; private set; }


    /// <summary>
    /// Gets the directional lights.
    /// </summary>
    /// <value>The directional lights.</value>
    public List<LightNode> DirectionalLights { get; private set; }


    /// <summary>
    /// Gets the image-based lights.
    /// </summary>
    /// <value>The image-based lights.</value>
    public List<LightNode> ImageBasedLights { get; private set; }

    
    /// <summary>
    /// Gets other lights that did not fit into any of the predefined categories
    /// (<see cref="AmbientLights"/>, <see cref="DirectionalLights"/>, etc.).
    /// </summary>
    /// <value>The other lights.</value>
    public List<LightNode> OtherLights { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalLightQuery"/> class.
    /// </summary>
    public GlobalLightQuery()
    {
      // Create collections for caching the light nodes.
      AmbientLights = new List<LightNode>();
      DirectionalLights = new List<LightNode>();
      ImageBasedLights = new List<LightNode>();
      OtherLights = new List<LightNode>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public void Reset()
    {
      ReferenceNode = null;
      AmbientLights.Clear();
      DirectionalLights.Clear();
      ImageBasedLights.Clear();
      OtherLights.Clear();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public void Set(SceneNode referenceNode, IList<SceneNode> nodes, RenderContext context)
    {
      Reset();
      ReferenceNode = referenceNode;

      int numberOfNodes = nodes.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode != null)
        {
          Debug.Assert(lightNode.ActualIsEnabled, "Scene query contains disabled nodes.");

          if (lightNode.Light is AmbientLight)
          {
            AmbientLights.Add(lightNode);
          }
          else if (lightNode.Light is DirectionalLight)
          {
            DirectionalLights.Add(lightNode);
          }
          else if (lightNode.Light is PointLight)
          {
            // Point lights cannot be global.
          }
          else if (lightNode.Light is Spotlight)
          {
            // Spotlights cannot be global.
          }
          else if (lightNode.Light is ProjectorLight)
          {
            // Projector lights cannot be global.
          }
          else if (lightNode.Light is ImageBasedLight)
          {
            if (lightNode.Shape is InfiniteShape)
              ImageBasedLights.Add(lightNode);
          }
          else
          {
            OtherLights.Add(lightNode);
          }

          // Sort by estimated light contribution.
          lightNode.SortTag = lightNode.GetLightContribution(Vector3F.Zero, 0.7f);

          // Or simpler: Sort light nodes by distance.
          // (We use distance², because it is faster.)
          //float distance = (referencePosition - lightNode.PoseWorld.Position).LengthSquared; 
          //lightNode.SortTag = distance;
        }
      }

      // Sort lights.
      AmbientLights.Sort(DescendingLightNodeComparer.Instance);
      DirectionalLights.Sort(DescendingLightNodeComparer.Instance);
      ImageBasedLights.Sort(DescendingLightNodeComparer.Instance);
      OtherLights.Sort(DescendingLightNodeComparer.Instance);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    internal List<LightNode> GetLights<T>() where T : Light
    {
      Type type = typeof(T);
      if (type == typeof(AmbientLight))
        return AmbientLights;
      if (type == typeof(DirectionalLight))
        return DirectionalLights;
      if (type == typeof(ImageBasedLight))
        return ImageBasedLights;

      string message = String.Format(CultureInfo.InvariantCulture, "Type of light \"{0}\" is not supported by GlobalLightQuery.", type);
      throw new GraphicsException(message);
    }
    #endregion
  }
}
