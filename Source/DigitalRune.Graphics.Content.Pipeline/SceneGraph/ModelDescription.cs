// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DigitalRune.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  // The model description is stored in an XML file. The XML file has the same
  // name as the model asset. Example: "Dude.fbx" --> "Dude.xml" or "Dude.drmdl"
  // If the XML file is missing, the model is built using the materials included
  // in the model file ("local materials").

  internal class ModelDescription : ContentItem
  {
    public string FileName { get; set; }
    public string Importer { get; set; }
    public float RotationX { get; set; }
    public float RotationY { get; set; }
    public float RotationZ { get; set; }
    public float Scale { get; set; }
    public bool GenerateTangentFrames { get; set; }
    public bool SwapWindingOrder { get; set; }
    public bool AabbEnabled { get; set; }
    public Vector3 AabbMinimum { get; set; }
    public Vector3 AabbMaximum { get; set; }
    public bool PremultiplyVertexColors { get; set; }
    public List<MeshDescription> Meshes { get; set; }
    public AnimationDescription Animation { get; set; }
    public float MaxDistance { get; set; }


    /// <summary>
    /// Prevents a default instance of the <see cref="ModelDescription"/> class from being created.
    /// </summary>
    private ModelDescription()
    {
    }


    /// <summary>
    /// Loads the model description (XML file).
    /// </summary>
    /// <param name="sourceFileName">The .</param>
    /// <param name="context">Contains any required custom process parameters.</param>
    /// <param name="createIfMissing">
    /// If set to <see langword="true"/> the model description (.drmdl file) will be created
    /// automatically if it is missing.
    /// </param>
    /// <returns>The model description, or <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sourceFileName"/> or <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidContentException">
    /// The model description (.drmdl file) is invalid.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public static ModelDescription Load(string sourceFileName, ContentPipelineContext context, bool createIfMissing)
    {
      if (sourceFileName == null)
        throw new ArgumentNullException("sourceFileName");
      if (sourceFileName.Length == 0)
        throw new ArgumentException("File name must not be empty.", "sourceFileName");
      if (context == null)
        throw new ArgumentNullException("context");

      string fileName = Path.ChangeExtension(sourceFileName, "drmdl");
      if (!File.Exists(fileName))
      {
        // Also try with extension xml, which was used before drmdl.
        fileName = Path.ChangeExtension(sourceFileName, "xml");
        if (!IsModelDescriptionFile(fileName))
        {
          fileName = Path.ChangeExtension(sourceFileName, "drmdl");

          if (!createIfMissing)
          {
            context.Logger.LogImportantMessage(
              "The model description file \"{0}\" is missing. Using default settings.",
              Path.GetFileName(fileName));

            return null;
          }

          // Create default model description file.
          try
          {
            using (var stream = File.CreateText(fileName))
            {
              stream.Write(Properties.Resources.DefaultModelDescription, Path.GetFileName(sourceFileName));
            }
          }
          catch (Exception exception)
          {
            context.Logger.LogImportantMessage(
              "Automatic creation of model description \"{0}\" failed. Using default settings.\nException: {1}",
              fileName, exception.ToString());

            return null;
          }
        }
      }

      context.AddDependency(fileName);

      var modelDescription = new ModelDescription { Identity = new ContentIdentity(fileName) };

      XDocument document;
      try
      {
        document = XDocument.Load(fileName, LoadOptions.SetLineInfo);
      }
      catch (Exception exception)
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Could not load '{0}': {1}", fileName, exception.Message);
        throw new InvalidContentException(message, modelDescription.Identity);
      }

      var modelElement = document.Root;
      if (modelElement == null || modelElement.Name != "Model")
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Root element \"<Model>\" is missing in XML.");
        throw new InvalidContentException(message, modelDescription.Identity);
      }

      // Model attributes.
      modelDescription.Name = (string)modelElement.Attribute("Name") ?? Path.GetFileNameWithoutExtension(fileName);
      modelDescription.FileName = (string)modelElement.Attribute("File") ?? (string)modelElement.Attribute("FileName");
      modelDescription.Importer = (string)modelElement.Attribute("Importer");
      modelDescription.RotationX = (float?)modelElement.Attribute("RotationX") ?? 0.0f;
      modelDescription.RotationY = (float?)modelElement.Attribute("RotationY") ?? 0.0f;
      modelDescription.RotationZ = (float?)modelElement.Attribute("RotationZ") ?? 0.0f;
      modelDescription.Scale = (float?)modelElement.Attribute("Scale") ?? 1.0f;
      modelDescription.GenerateTangentFrames = (bool?)modelElement.Attribute("GenerateTangentFrames") ?? false;
      modelDescription.SwapWindingOrder = (bool?)modelElement.Attribute("SwapWindingOrder") ?? false;
      modelDescription.PremultiplyVertexColors = (bool?)modelElement.Attribute("PremultiplyVertexColors") ?? true;
      modelDescription.MaxDistance = (float?)modelElement.Attribute("MaxDistance") ?? 0.0f;

      var aabbMinimumAttribute = modelElement.Attribute("AabbMinimum");
      var aabbMaximumAttribute = modelElement.Attribute("AabbMaximum");
      if (aabbMinimumAttribute != null && aabbMaximumAttribute != null)
      {
        modelDescription.AabbEnabled = true;
        modelDescription.AabbMinimum = aabbMinimumAttribute.ToVector3(Vector3.Zero, modelDescription.Identity);
        modelDescription.AabbMaximum = aabbMaximumAttribute.ToVector3(Vector3.One, modelDescription.Identity);
      }

      // Mesh elements.
      modelDescription.Meshes = new List<MeshDescription>();
      foreach (var meshElement in modelElement.Elements("Mesh"))
      {
        var meshDescription = new MeshDescription();
        meshDescription.Name = (string)meshElement.Attribute("Name") ?? string.Empty;
        meshDescription.GenerateTangentFrames = (bool?)meshElement.Attribute("GenerateTangentFrames") ?? modelDescription.GenerateTangentFrames;
        meshDescription.MaxDistance = (float?)meshElement.Attribute("MaxDistance") ?? modelDescription.MaxDistance;
        meshDescription.LodDistance = (float?)meshElement.Attribute("LodDistance") ?? 0.0f;

        meshDescription.Submeshes = new List<SubmeshDescription>();
        foreach (var submeshElement in meshElement.Elements("Submesh"))
        {
          var submeshDescription = new SubmeshDescription();
          submeshDescription.GenerateTangentFrames = (bool?)meshElement.Attribute("GenerateTangentFrames") ?? meshDescription.GenerateTangentFrames;
          submeshDescription.Material = (string)submeshElement.Attribute("Material");

          meshDescription.Submeshes.Add(submeshDescription);
        }

        modelDescription.Meshes.Add(meshDescription);
      }

      // Animations element.
      var animationsElement = modelElement.Element("Animations");
      if (animationsElement != null)
      {
        var animationDescription = new AnimationDescription();
        animationDescription.MergeFiles = (string)animationsElement.Attribute("MergeFiles");
        animationDescription.Splits = AnimationSplitter.ParseAnimationSplitDefinitions(animationsElement, modelDescription.Identity, context);
        animationDescription.ScaleCompression = (float?)animationsElement.Attribute("ScaleCompression") ?? -1;
        animationDescription.RotationCompression = (float?)animationsElement.Attribute("RotationCompression") ?? -1;
        animationDescription.TranslationCompression = (float?)animationsElement.Attribute("TranslationCompression") ?? -1;
        animationDescription.AddLoopFrame = (bool?)animationsElement.Attribute("AddLoopFrame");

        modelDescription.Animation = animationDescription;
      }

      return modelDescription;
    }


    /// <summary>
    /// Checks whether the imported model matches the model description.
    /// </summary>
    /// <param name="input">The root node content.</param>
    /// <param name="context">Contains any required custom process parameters.</param>
    public void Validate(NodeContent input, ContentProcessorContext context)
    {
      foreach (var meshDescription in Meshes)
      {
        // Check if there is a mesh for this mesh name.
        if (!string.IsNullOrEmpty(meshDescription.Name)
            && TreeHelper.GetSubtree(input, n => n.Children)
                         .OfType<MeshContent>()
                         .All(mc => mc.Name != meshDescription.Name))
        {
          context.Logger.LogWarning(
            null, input.Identity,
            "Model description (.drmdl file) contains description for mesh '{0}' which was not found in the asset.",
            meshDescription.Name);
        }
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private static bool IsModelDescriptionFile(string fileName)
    {
      try
      {
        XDocument document = XDocument.Load(fileName, LoadOptions.SetLineInfo);
        if (document.Root != null && document.Root.Name == "Model")
          return true;
      }
      catch
      {
      }

      return false;
    }


    public MeshDescription GetMeshDescription(string name)
    {
      if (name == null)
        name = string.Empty;

      // Search for mesh description by name.
      var meshDescription = Meshes.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
      if (meshDescription != null)
        return meshDescription;

      // Search for mesh description without a name. Use as fallback.
      meshDescription = Meshes.FirstOrDefault(m => string.IsNullOrEmpty(m.Name));

      return meshDescription;
    }
  }
}
