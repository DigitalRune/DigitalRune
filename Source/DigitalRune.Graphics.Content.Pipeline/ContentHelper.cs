// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DigitalRune.Collections;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Provides helper methods for content processing.
  /// </summary>
  internal static class ContentHelper
  {
    /// <summary>
    /// Visualizes the tree structure of the specified content node.
    /// </summary>
    /// <param name="node">The content node.</param>
    /// <param name="context">Contains any required custom process parameters.</param>
    public static void PrintContentTree(NodeContent node, ContentProcessorContext context)
    {
      var stringBuilder = new StringBuilder();
      PrintContentTree(node, stringBuilder);
      context.Logger.LogImportantMessage(stringBuilder.ToString());
    }


    private static void PrintContentTree(NodeContent node, StringBuilder stringBuilder, int indentation = 0)
    {
      if (stringBuilder == null)
        throw new ArgumentNullException("stringBuilder");

      if (node == null)
        return;

      stringBuilder.Append('\t', indentation);
      stringBuilder.AppendFormat("\"{0}\" ({1})\n", node.Name, node.GetType());

      indentation++;
      var mesh = node as MeshContent;
      if (mesh != null)
      {
        foreach (var geometry in mesh.Geometry)
        {
          stringBuilder.Append('\t', indentation);
          stringBuilder.AppendFormat("\"{0}\" ({1})\n", geometry.Name, geometry.GetType());
        }

        stringBuilder.AppendLine();
      }

      foreach (var child in node.Children)
        PrintContentTree(child, stringBuilder, indentation);
    }


    /// <summary>
    /// Determines whether the type of the given object is a valid effect parameter type.
    /// </summary>
    /// <param name="effectParameter">The effect parameter.</param>
    /// <returns>
    /// <see langword="true"/> if the type is valid; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValidTypeForEffectParameter(object effectParameter)
    {
      if (effectParameter == null)
        throw new ArgumentNullException("effectParameter");

      // Valid types: int, bool, string, Matrix, Vector2, etc. and arrays of these types.
      // Except: string arrays are not supported.

      // Determine the type of the parameter
      Type elementType = effectParameter.GetType();
      if (elementType.IsArray && elementType != typeof(string[]))
        elementType = elementType.GetElementType();

      if ((elementType == typeof(int))
          || (elementType == typeof(bool))
          || (elementType == typeof(string))
          || (elementType == typeof(float))
          || (elementType == typeof(Matrix))
          || (elementType == typeof(Quaternion))
          || (elementType == typeof(Vector2))
          || (elementType == typeof(Vector3))
          || (elementType == typeof(Vector4))
          || (elementType == typeof(Matrix22F))
          || (elementType == typeof(Matrix33F))
          || (elementType == typeof(Matrix44F))
          || (elementType == typeof(QuaternionF))
          || (elementType == typeof(Vector2F))
          || (elementType == typeof(Vector3F))
          || (elementType == typeof(Vector4F)))
      {
        return true;
      }

      return false;
    }


    /// <summary>
    /// Copies the properties of a <see cref="ContentItem"/>.
    /// </summary>
    /// <param name="source">The source item.</param>
    /// <param name="target">The target item.</param>
    public static void CopyContentItem(ContentItem source, ContentItem target)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      if (target == null)
        throw new ArgumentNullException("target");

      target.Name = source.Name;
      target.Identity = source.Identity;
      foreach (var entry in source.OpaqueData)
        target.OpaqueData.Add(entry.Key, entry.Value);
    }


    /// <summary>
    /// Copies the properties of a <see cref="MaterialContent"/>.
    /// </summary>
    /// <param name="source">The source item.</param>
    /// <param name="target">The target item.</param>
    public static void CopyMaterialContent(MaterialContent source, MaterialContent target)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      if (target == null)
        throw new ArgumentNullException("target");

      // Copy properties of ContentItem.
      CopyContentItem(source, target);

      // Copy MaterialContent.Textures.
      foreach (var entry in source.Textures)
        target.Textures.Add(entry.Key, entry.Value);
    }


    /// <summary>
    /// Tries to locate the specified file.
    /// </summary>
    /// <param name="path">
    /// The name of the file. May include a relative or absolute path.
    /// </param>
    /// <param name="identity">The content identity.</param>
    /// <returns>The full path and file name.</returns>
    public static string FindFile(string path, ContentIdentity identity)
    {
      if (path == null)
        throw new ArgumentNullException("path");

      // ---- 1. Check whether path is a valid absolute path.
      if (Path.IsPathRooted(path) && File.Exists(path))
        return Path.GetFullPath(path);

      if (identity == null)
        throw new ArgumentNullException("identity");

      // ----- 2. Check whether path is relative to asset.
      string folder = Path.GetDirectoryName(identity.SourceFilename) ?? String.Empty;
      
      // Try relative path.
      string relativeFilename = Path.Combine(folder, path);
      if (File.Exists(relativeFilename))
        return Path.GetFullPath(relativeFilename);

      // Try file name only.
      string fileName = Path.GetFileName(path);
      relativeFilename = Path.Combine(folder, fileName);
      if (File.Exists(relativeFilename))
        return Path.GetFullPath(relativeFilename);

      // ----- 3. Check whether path is relative to the current directory.
      if (File.Exists(path))
        return Path.GetFullPath(path);

      if (File.Exists(fileName))
        return Path.GetFullPath(fileName);

      string message = String.Format(CultureInfo.InvariantCulture, "File \"{0}\" not found.", path);
      throw new InvalidContentException(message, identity);
    }


    /// <summary>
    /// Gets the MonoGame platform identifier.
    /// </summary>
    /// <returns>
    /// The name of the MonoGame platform (e.g. "IOS"), or <see langword="null"/> or 
    /// <see cref="String.Empty"/> if this is an XNA project.
    /// </returns>
    internal static string GetMonoGamePlatform()
    {
      return Environment.GetEnvironmentVariable("MONOGAME_PLATFORM", EnvironmentVariableTarget.User);
    }


    // The regular expression used in ParseSceneNodeName().
    private static Regex _regexSceneNodeName;


    /// <summary>
    /// Parses the name of the scene node.
    /// </summary>
    /// <param name="originalName">
    /// The original name of the scene node. Example: "MeshXyz_LOD2".
    /// </param>
    /// <param name="name">The name of the scene node. Example: "MeshXyz"</param>
    /// <param name="lod">The LOD level. Example: 2</param>
    internal static void ParseSceneNodeName(string originalName, out string name, out int? lod)
    {
      name = originalName;
      lod = null;

      if (originalName != null)
      {
        // Use regular expression to capture "Name" and "LOD".
        // Example: 
        //   "Name123_LOD456_xxx"
        //   Name = "Name123"
        //   LOD = "456"
        if (_regexSceneNodeName == null)
          _regexSceneNodeName = new Regex(@"(?<Name>.+)_LOD(?<LOD>\d+).*", RegexOptions.IgnoreCase);

        var match = _regexSceneNodeName.Match(originalName);
        if (match.Success)
        {
          int i;
          lod = int.TryParse(match.Groups["LOD"].Value, out i) ? i : (int?)null;
          name = match.Groups["Name"].Value;
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Determines whether the specified node represents a morph target.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified node represents a morph target.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>
    /// <see langword="true"/> if the mesh represents a morph target; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    internal static bool IsMorphTarget(NodeContent node)
    {
      string name;
      return IsMorphTarget(node, out name);
    }


    /// <summary>
    /// Determines whether the specified node represents a morph target.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="name">The name of the morph target.</param>
    /// <returns>
    /// <see langword="true"/> if the mesh represents a morph target; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    internal static bool IsMorphTarget(NodeContent node, out string name)
    {
      // Acceptable morph target names are:
      //  "MORPH_name"
      //  "MORPHTARGET_name"
      //  "BLENDSHAPE_name"
      //  "name_MORPH"
      //  "name_MORPHTARGET"
      //  "name_BLENDSHAPE"
      //
      // Comparison is case-insensitive.
      // The prefix/suffix ("MORPH") and the separator ('_') will be cut from the
      // morph target name.
      var mesh = node as MeshContent;
      if (mesh != null)
      {
        name = mesh.Name;
        if (name != null)
        {
          string[] patterns = { "MORPH", "MORPHTARGET", "BLENDSHAPE"};
          char separator = '_';

          // Check prefix.
          foreach (var pattern in patterns)
          {
            if (name.Length > pattern.Length + 1)
            {
              if (name.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) && name[pattern.Length] == separator)
              {
                name = name.Substring(pattern.Length + 1);
                return true;
              }
            }
          }

          // Check postfix.
          foreach (var pattern in patterns)
          {
            if (name.Length > pattern.Length + 1)
            {
              if (name.EndsWith(pattern, StringComparison.OrdinalIgnoreCase)
                  && name[name.Length - pattern.Length - 1] == separator)
              {
                name = name.Substring(0, name.Length - pattern.Length - 1);
                return true;
              }
            }
          }
        }
      }

      name = null;
      return false;
    }


    /// <summary>
    /// Determines whether the specified mesh represents an occluder.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <returns>
    /// <see langword="true"/> if the mesh represents an occluder; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    internal static bool IsOccluder(MeshContent mesh)
    {
      // Acceptable occluder names are:
      //  "*OCCLUDER*"
      //  "*OCCLUSION*"
      // where * means zero or more characters.
      // Comparison is case-insensitive.
      string name = mesh.Name;
      if (name != null)
      {
        const string pattern0 = "OCCLUDER";
        const string pattern1 = "OCCLUSION";
        if (name.Length >= pattern0.Length)
        {
          if (name.IndexOf(pattern0, StringComparison.OrdinalIgnoreCase) >= 0
              || name.IndexOf(pattern1, StringComparison.OrdinalIgnoreCase) >= 0)
          {
            return true;
          }
        }
      }

      return false;
    }


    /// <overloads>
    /// <summary>
    /// Determines whether the specified content is a skinned mesh.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified node is a skinned mesh.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="node"/> is a skinned mesh; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsSkinned(NodeContent node)
    {
      var mesh = node as MeshContent;
      if (mesh != null && mesh.Geometry.Any(IsSkinned))
        return true;

      return false;
    }


    /// <summary>
    /// Determines whether the specified geometry is a skinned mesh.
    /// </summary>
    /// <param name="geometry">The <see cref="GeometryContent"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="geometry"/> is a skinned mesh; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsSkinned(GeometryContent geometry)
    {
      return geometry.Vertices
                     .Channels
                     .OfType<VertexChannel<BoneWeightCollection>>()
                     .Any();
    }


    /// <summary>
    /// Converts a <see cref="BasicMaterialContent"/> to an equivalent
    /// <see cref="SkinnedMaterialContent"/>.
    /// </summary>
    /// <param name="material">The <see cref="BasicMaterialContent"/>.</param>
    /// <returns>The <see cref="SkinnedMaterialContent"/>.</returns>
    public static SkinnedMaterialContent ConvertToSkinnedMaterial(MaterialContent material)
    {
      var skinnedMaterial = material as SkinnedMaterialContent;
      if (skinnedMaterial != null)
        return skinnedMaterial;

      var basicMaterial = material as BasicMaterialContent;
      if (basicMaterial != null)
      {
        skinnedMaterial = new SkinnedMaterialContent
        {
          Name = basicMaterial.Name,
          Identity = basicMaterial.Identity,
          Alpha = basicMaterial.Alpha,
          DiffuseColor = basicMaterial.DiffuseColor,
          EmissiveColor = basicMaterial.EmissiveColor,
          SpecularColor = basicMaterial.SpecularColor,
          SpecularPower = basicMaterial.SpecularPower,
          WeightsPerVertex = null
        };
        skinnedMaterial.Textures.AddRange(basicMaterial.Textures);
        return skinnedMaterial;
      }

      return null;
    }
  }
}
