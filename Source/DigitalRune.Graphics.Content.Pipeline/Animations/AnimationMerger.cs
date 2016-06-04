// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Merges the animations of several animation files (e.g. .fbx) into a given NodeContent.
  /// </summary>
  /// <remarks>
  /// See http://blogs.msdn.com/b/shawnhar/archive/2010/06/18/merging-animation-files.aspx for more
  /// information.
  /// </remarks>
  internal static class AnimationMerger
  {
    /// <summary>
    /// Merges the specified animation files to the specified animation dictionary.
    /// </summary>
    /// <param name="animationFiles">
    /// The animation files as a string separated by semicolon (relative to the folder of the model 
    /// file). For example: "run.fbx;jump.fbx;turn.fbx".
    /// </param>
    /// <param name="animationDictionary">The animation dictionary.</param>
    /// <param name="contentIdentity">The content identity.</param>
    /// <param name="context">The content processor context.</param>
    public static void Merge(string animationFiles, AnimationContentDictionary animationDictionary,
                             ContentIdentity contentIdentity, ContentProcessorContext context)
    {
      if (string.IsNullOrEmpty(animationFiles))
        return;

      // Get path of the model file.
      var files = animationFiles.Split(';', ',')
                                .Select(s => s.Trim())
                                .Where(s => !string.IsNullOrEmpty(s));
      foreach (string file in files)
      {
        MergeAnimation(file, animationDictionary, contentIdentity, context);
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    private static void MergeAnimation(string animationFile, AnimationContentDictionary animationDictionary,
                                       ContentIdentity contentIdentity, ContentProcessorContext context)
    {
      if (string.IsNullOrEmpty(animationFile))
        return;
      if (animationDictionary == null)
        throw new ArgumentNullException("animationDictionary");
      if (context == null)
        throw new ArgumentNullException("context");

      // Use content pipeline to build the asset.
      animationFile = ContentHelper.FindFile(animationFile, contentIdentity);
      NodeContent mergeModel = context.BuildAndLoadAsset<NodeContent, NodeContent>(new ExternalReference<NodeContent>(animationFile), null);

      // Find the skeleton.
      BoneContent mergeRoot = MeshHelper.FindSkeleton(mergeModel);
      if (mergeRoot == null)
      {
        context.Logger.LogWarning(null, contentIdentity, "Animation model file '{0}' has no root bone. Cannot merge animations.", animationFile);
        return;
      }

      // Merge all animations of the skeleton root node.
      foreach (string animationName in mergeRoot.Animations.Keys)
      {
        if (animationDictionary.ContainsKey(animationName))
        {
          context.Logger.LogWarning(null, contentIdentity, "Replacing animation '{0}' with merged animation from '{1}'.", animationName, animationFile);
          animationDictionary[animationName] = mergeRoot.Animations[animationName];
        }
        else
        {
          context.Logger.LogImportantMessage("Merging animation '{0}' from '{1}'.", animationName, animationFile);
          animationDictionary.Add(animationName, mergeRoot.Animations[animationName]);
        }
      }
    }
  }
}
