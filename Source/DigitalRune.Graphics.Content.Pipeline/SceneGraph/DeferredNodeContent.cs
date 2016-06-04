// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Linq;
using DigitalRune.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Represents a placeholder for a <see cref="NodeContent"/> which hasn't been imported yet.
  /// </summary>
  /// <remarks>
  /// Call <see cref="Import"/> to load the actual <see cref="NodeContent"/>.
  /// </remarks>
  public class DeferredNodeContent : NodeContent
  {
    /// <summary>
    /// Gets or sets the model description.
    /// </summary>
    /// <value>The model description.</value>
    internal ModelDescription ModelDescription { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredNodeContent"/> class.
    /// </summary>
    /// <param name="modelDescription">The model description.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="modelDescription"/> is <see langword="null"/>.
    /// </exception>
    internal DeferredNodeContent(ModelDescription modelDescription)
    {
      if (modelDescription == null)
        throw new ArgumentNullException("modelDescription");

      Transform = Matrix.Identity;
      ModelDescription = modelDescription;
    }


    /// <summary>
    /// Imports the asset.
    /// </summary>
    /// <param name="context">Contains any required custom process parameters.</param>
    public void Import(ContentProcessorContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");
      if (string.IsNullOrWhiteSpace(ModelDescription.FileName))
        throw new InvalidContentException("The attribute 'File' is not set in the model description (.drmdl file).", Identity);

      var fileName = ContentHelper.FindFile(ModelDescription.FileName, Identity);
      var asset = new ExternalReference<NodeContent>(fileName);
      var node = context.BuildAndLoadAsset<NodeContent, NodeContent>(asset, null, null, ModelDescription.Importer);

      // BuildAndLoadAsset does not return root node in MonoGame.
      while (node.Parent != null)
        node = node.Parent;

      if (node.GetType() == typeof(NodeContent))
      {
        // Root node is of type NodeContent.
        // --> Copy root node content and children.
        Name = node.Name;
        Transform = node.Transform;
        Animations.AddRange(node.Animations);
        OpaqueData.AddRange(node.OpaqueData);

        var children = node.Children.ToArray();
        node.Children.Clear(); // Clear parents.
        Children.AddRange(children);
      }
      else
      {
        // Root node is a derived type.
        // --> Add node as child.
        Children.Add(node);
      }
    }
  }
}
