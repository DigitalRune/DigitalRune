// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  partial class DRModelProcessor
  {
    /// <summary>
    /// Sets a <see cref="SkinnedMaterialContent"/> for all skinned meshes.
    /// </summary>
    private void SetSkinnedMaterial()
    {
      var swappedMaterials = new Dictionary<MaterialContent, SkinnedMaterialContent>();
      var defaultMaterial = new SkinnedMaterialContent() { Identity = _input.Identity };
      SetSkinnedMaterial(_input, swappedMaterials, defaultMaterial);
    }


    private static void SetSkinnedMaterial(NodeContent node, Dictionary<MaterialContent, SkinnedMaterialContent> swappedMaterials, SkinnedMaterialContent defaultMaterial)
    {
      var mesh = node as MeshContent;
      if (mesh != null)
      {
        foreach (var geometry in mesh.Geometry)
        {
          if (!ContentHelper.IsSkinned(geometry))
            continue;

          if (geometry.Material == null)
          {
            // Happens if the model is exported without a material. (XNA only!)
            geometry.Material = defaultMaterial;
            continue;
          }

          SkinnedMaterialContent skinnedMaterial;
          if (!swappedMaterials.TryGetValue(geometry.Material, out skinnedMaterial))
          {
            // Convert BasicMaterialContent to SkinnedMaterialContent.
            skinnedMaterial = ContentHelper.ConvertToSkinnedMaterial(geometry.Material);
            if (skinnedMaterial == null)
              continue;

            swappedMaterials[geometry.Material] = skinnedMaterial;
          }

          geometry.Material = skinnedMaterial;
        }
      }

      foreach (var child in node.Children)
        SetSkinnedMaterial(child, swappedMaterials, defaultMaterial);
    }


    private string GetExternalMaterial(MeshContent mesh, GeometryContent geometry)
    {
      if (_modelDescription != null)
      {
        var meshDescription = _modelDescription.GetMeshDescription(mesh.Name);
        if (meshDescription != null)
        {
          int index = mesh.Geometry.IndexOf(geometry);
          if (0 <= index && index < meshDescription.Submeshes.Count)
            return meshDescription.Submeshes[index].Material;
        }
      }

      // Fallback:
      // The model description does not define a material file. Try to use the texture name
      // as a fallback.
      if (geometry != null && geometry.Material != null && geometry.Material.Textures.ContainsKey("Texture"))
      {
        string textureFile = geometry.Material.Textures["Texture"].Filename;
        string materialFile = Path.ChangeExtension(textureFile, ".drmat");

        if (File.Exists(materialFile))
          return materialFile;
      }

      return null;
    }


    /// <summary>
    /// Creates the missing material definition.
    /// </summary>
    /// <param name="material">The material.</param>
    /// <returns>
    /// <see langword="true"/> if the material definition was successfully created; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    private bool CreateMaterialDefinition(MaterialContent material)
    {
      if (!CreateMissingMaterialDefinition)
        return false;

      if (material == null)
        return false;

      ExternalReference<TextureContent> texture;
      if (!material.Textures.TryGetValue("Texture", out texture) || texture == null)
        return false;

      var textureFileName = GetTextureFileName(texture);
      if (string.IsNullOrEmpty(textureFileName))
        return false;

      string content;
      if (material is BasicMaterialContent)
      {
        var basicMaterial = (BasicMaterialContent)material;
        float alpha = basicMaterial.Alpha ?? 1;
        Vector3 diffuseColor = basicMaterial.DiffuseColor ?? new Vector3(1, 1, 1);
        Vector3 emissiveColor = basicMaterial.EmissiveColor ?? new Vector3(0, 0, 0);
        Vector3 specularColor = basicMaterial.SpecularColor ?? new Vector3(1, 1, 1);
        float specularPower = basicMaterial.SpecularPower ?? 16;
        bool vertexColorEnabled = basicMaterial.VertexColorEnabled ?? false;

        content = string.Format(
          CultureInfo.InvariantCulture,
          @"<?xml version=""1.0"" encoding=""utf-8""?>

<!-- This file was generated automatically by the DigitalRune Model Content Processor. -->

<Material>
  <Pass Name=""Default"" Effect=""BasicEffect"" Profile=""Any"">
    <Parameter Name=""Alpha"" Value=""{0}"" />
    <Parameter Name=""DiffuseColor"" Value=""{1},{2},{3}"" />
    <Parameter Name=""EmissiveColor"" Value=""{4},{5},{6}"" />
    <Parameter Name=""SpecularColor"" Value=""{7},{8},{9}"" />
    <Parameter Name=""SpecularPower"" Value=""{10}"" />
    <Parameter Name=""VertexColorEnabled"" Value=""{11}"" />
    <Texture Name=""Texture"" File=""{12}"" />
  </Pass>

<!--  Add other application-dependent passes here, for example:
  <Pass Name=""ShadowMap"" Effect=""DigitalRune/Materials/ShadowMap"" Profile=""HiDef""/>
  <Pass Name=""GBuffer"" Effect=""DigitalRune/Materials/GBufferNormal"" Profile=""HiDef"">
    <Parameter Name=""SpecularPower"" Value=""{10}"" />
    <Texture Name=""NormalTexture"" Format=""Normal"" File=""normal.tga"" />
  </Pass>
  <Pass Name=""Material"" Effect=""DigitalRune/Materials/Material"" Profile=""HiDef"">
    <Parameter Name=""DiffuseColor"" Value=""{1},{2},{3}"" />
    <Parameter Name=""SpecularColor"" Value=""{7},{8},{9}"" />
    <Texture Name=""DiffuseTexture"" File=""{12}"" />
    <Texture Name=""SpecularTexture"" File=""specular.tga"" />
  </Pass>
-->

<!-- See the DigitalRune Graphics documentation for a full list of material definition options. -->

</Material>",
          alpha,
          diffuseColor.X, diffuseColor.Y, diffuseColor.Z,
          emissiveColor.X, emissiveColor.Y, emissiveColor.Z,
          specularColor.X, specularColor.Y, specularColor.Z,
          specularPower,
          vertexColorEnabled,
          Path.GetFileName(textureFileName));
      }
      else if (material is SkinnedMaterialContent)
      {
        var skinnedMaterial = (SkinnedMaterialContent)material;
        float alpha = skinnedMaterial.Alpha ?? 1;
        Vector3 diffuseColor = skinnedMaterial.DiffuseColor ?? new Vector3(1, 1, 1);
        Vector3 emissiveColor = skinnedMaterial.EmissiveColor ?? new Vector3(0, 0, 0);
        Vector3 specularColor = skinnedMaterial.SpecularColor ?? new Vector3(1, 1, 1);
        float specularPower = skinnedMaterial.SpecularPower ?? 16;
        float weightsPerVertex = skinnedMaterial.WeightsPerVertex ?? 4;

        content = string.Format(
          CultureInfo.InvariantCulture,
          @"<?xml version=""1.0"" encoding=""utf-8""?>

<!-- This file was generated automatically by the DigitalRune Model Content Processor. -->

<Material>
  <Pass Name=""Default"" Effect=""SkinnedEffect"" Profile=""Any"">
    <Parameter Name=""Alpha"" Value=""{0}"" />
    <Parameter Name=""DiffuseColor"" Value=""{1},{2},{3}"" />
    <Parameter Name=""EmissiveColor"" Value=""{4},{5},{6}"" />
    <Parameter Name=""SpecularColor"" Value=""{7},{8},{9}"" />
    <Parameter Name=""SpecularPower"" Value=""{10}"" />
    <Parameter Name=""WeightsPerVertex"" Value=""{11}"" />
    <Texture Name=""Texture"" File=""{12}"" />
  </Pass>

<!--  Add other application-dependent passes here, for example:
  <Pass Name=""ShadowMap"" Effect=""DigitalRune/Materials/ShadowMapSkinned"" Profile=""HiDef""/>
  <Pass Name=""GBuffer"" Effect=""DigitalRune/Materials/GBufferNormalSkinned"" Profile=""HiDef"">
    <Parameter Name=""SpecularPower"" Value=""{10}"" />
    <Texture Name=""NormalTexture"" Format=""Normal"" File=""normal.tga"" />
  </Pass>
  <Pass Name=""Material"" Effect=""DigitalRune/Materials/MaterialSkinned"" Profile=""HiDef"">
    <Parameter Name=""DiffuseColor"" Value=""{1},{2},{3}"" />
    <Parameter Name=""SpecularColor"" Value=""{7},{8},{9}"" />
    <Texture Name=""DiffuseTexture"" File=""{12}"" />
    <Texture Name=""SpecularTexture"" File=""specular.tga"" />
  </Pass>
-->

<!-- See the DigitalRune Graphics documentation for a full list of material definition options. -->

</Material>",
          alpha,
          diffuseColor.X, diffuseColor.Y, diffuseColor.Z,
          emissiveColor.X, emissiveColor.Y, emissiveColor.Z,
          specularColor.X, specularColor.Y, specularColor.Z,
          specularPower,
          weightsPerVertex,
          Path.GetFileName(textureFileName));
      }
      else if (material is AlphaTestMaterialContent)
      {
        var alphaTestMaterial = (AlphaTestMaterialContent)material;
        float alpha = alphaTestMaterial.Alpha ?? 1;
        Vector3 diffuseColor = alphaTestMaterial.DiffuseColor ?? new Vector3(1, 1, 1);
        float referenceAlpha = alphaTestMaterial.ReferenceAlpha ?? 0.95f;
        bool vertexColorEnabled = alphaTestMaterial.VertexColorEnabled ?? false;

        content = string.Format(
          CultureInfo.InvariantCulture,
          @"<?xml version=""1.0"" encoding=""utf-8""?>

<!-- This file was generated automatically by the DigitalRune Model Content Processor. -->

<Material>
  <Pass Name=""Default"" Effect=""AlphaTestEffect"" Profile=""Any"">
    <Parameter Name=""Alpha"" Value=""{0}"" />
    <Parameter Name=""DiffuseColor"" Value=""{1},{2},{3}"" />
    <Parameter Name=""ReferenceAlpha"" Value=""{4}"" />
    <Parameter Name=""VertexColorEnabled"" Value=""{5}"" />
    <Texture Name=""Texture"" File=""{6}"" />
  </Pass>

<!--  Add other application-dependent passes here, for example:
  <Pass Name=""ShadowMap"" Effect=""DigitalRune/Materials/ShadowMapAlphaTest"" Profile=""HiDef"">
    <Parameter Name=""ReferenceAlpha"" Value=""{4}"" />
    <Texture Name=""DiffuseTexture"" ReferenceAlpha=""{4}"" ScaleAlphaToCoverage=""True"" File=""{6}"" />
  </Pass>
  <Pass Name=""GBuffer"" Effect=""DigitalRune/Materials/GBufferAlphaTestNormal"" Profile=""HiDef"">
    <Parameter Name=""SpecularPower"" Value=""16"" />
    <Parameter Name=""ReferenceAlpha"" Value=""{4}"" />
    <Texture Name=""DiffuseTexture"" ReferenceAlpha=""{4}"" ScaleAlphaToCoverage=""True"" File=""{6}"" />
    <Texture Name=""NormalTexture"" Format=""Normal"" File=""normal.tga"" />
  </Pass>
  <Pass Name=""Material"" Effect=""DigitalRune/Materials/MaterialAlphaTest"" Profile=""HiDef"">
    <Parameter Name=""DiffuseColor"" Value=""{1},{2},{3}"" />
    <Parameter Name=""SpecularColor"" Value=""1,1,1"" />
    <Parameter Name=""ReferenceAlpha"" Value=""{4}"" />
    <Texture Name=""DiffuseTexture"" ReferenceAlpha=""{4}"" ScaleAlphaToCoverage=""True"" File=""{6}"" />
    <Texture Name=""SpecularTexture"" File=""specular.tga"" />
  </Pass>
-->

<!-- See the DigitalRune Graphics documentation for a full list of material definition options. -->

</Material>",
          alpha,
          diffuseColor.X, diffuseColor.Y, diffuseColor.Z,
          referenceAlpha,
          vertexColorEnabled,
          Path.GetFileName(textureFileName));
      }
      else if (material is DualTextureMaterialContent)
      {
        var dualTextureMaterial = (DualTextureMaterialContent)material;
        float alpha = dualTextureMaterial.Alpha ?? 1;
        Vector3 diffuseColor = dualTextureMaterial.DiffuseColor ?? new Vector3(1, 1, 1);
        bool vertexColorEnabled = dualTextureMaterial.VertexColorEnabled ?? false;
        string texture2 = GetTextureFileName(dualTextureMaterial.Texture2) ?? string.Empty;

        content = string.Format(
          CultureInfo.InvariantCulture,
          @"<?xml version=""1.0"" encoding=""utf-8""?>

<!-- This file was generated automatically by the DigitalRune Model Content Processor. -->

<Material>
  <Pass Name=""Default"" Effect=""DualTextureEffect"" Profile=""Any"">
    <Parameter Name=""Alpha"" Value=""{0}"" />
    <Parameter Name=""DiffuseColor"" Value=""{1},{2},{3}"" />
    <Parameter Name=""VertexColorEnabled"" Value=""{4}"" />
    <Texture Name=""Texture"" File=""{5}"" />
    <Texture Name=""Texture2"" File=""{6}"" />
  </Pass>

<!--  Add other application-dependent passes here, for example:
  <Pass Name=""ShadowMap"" Effect=""DigitalRune/Materials/ShadowMap"" Profile=""HiDef""/>
  <Pass Name=""GBuffer"" Effect=""DigitalRune/Materials/GBufferNormal"" Profile=""HiDef"">
    <Parameter Name=""SpecularPower"" Value=""16"" />
    <Texture Name=""NormalTexture"" Format=""Normal"" File=""normal.tga"" />
  </Pass>
  <Pass Name=""Material"" Effect=""DigitalRune/Materials/Material"" Profile=""HiDef"">
    <Parameter Name=""DiffuseColor"" Value=""{1},{2},{3}"" />
    <Parameter Name=""SpecularColor"" Value=""1,1,1"" />
    <Texture Name=""DiffuseTexture"" File=""{5}"" />
    <Texture Name=""SpecularTexture"" File=""specular.tga"" />
  </Pass>
-->

<!-- See the DigitalRune Graphics documentation for a full list of material definition options. -->

</Material>",
          alpha,
          diffuseColor.X, diffuseColor.Y, diffuseColor.Z,
          vertexColorEnabled,
          Path.GetFileName(textureFileName),
          Path.GetFileName(texture2));
      }
      else if (material is EnvironmentMapMaterialContent)
      {
        var environmentMapMaterial = (EnvironmentMapMaterialContent)material;
        float alpha = environmentMapMaterial.Alpha ?? 1;
        Vector3 diffuseColor = environmentMapMaterial.DiffuseColor ?? new Vector3(1, 1, 1);
        Vector3 emissiveColor = environmentMapMaterial.EmissiveColor ?? new Vector3(0, 0, 0);
        string environmentMap = GetTextureFileName(environmentMapMaterial.EnvironmentMap) ?? string.Empty;
        float environmentMapAmount = environmentMapMaterial.EnvironmentMapAmount ?? 1;
        Vector3 environmentMapSpecular = environmentMapMaterial.EnvironmentMapSpecular ?? Vector3.Zero;
        float fresnelFactor = environmentMapMaterial.FresnelFactor ?? 1;
        content = string.Format(
          CultureInfo.InvariantCulture,
          @"<?xml version=""1.0"" encoding=""utf-8""?>

<!-- This file was generated automatically by the DigitalRune Model Content Processor. -->

<Material>
  <Pass Name=""Default"" Effect=""EnvironmentMapEffect"" Profile=""Any"">
    <Parameter Name=""Alpha"" Value=""{0}"" />
    <Parameter Name=""DiffuseColor"" Value=""{1},{2},{3}"" />
    <Parameter Name=""EmissiveColor"" Value=""{4},{5},{6}"" />
    <Parameter Name=""EnvironmentMapAmount"" Value=""{7}"" />
    <Parameter Name=""EnvironmentMapSpecular"" Value=""{8},{9},{10}"" />
    <Parameter Name=""FresnelFactor"" Value=""{11}"" />
    <Texture Name=""Texture"" File=""{12}"" />
    <Texture Name=""EnvironmentMap"" File=""{13}"" />
  </Pass>

<!--  Add other application-dependent passes here, for example: 
  <Pass Name=""ShadowMap"" Effect=""DigitalRune/Materials/ShadowMap"" Profile=""HiDef""/>
  <Pass Name=""GBuffer"" Effect=""DigitalRune/Materials/GBufferNormal"" Profile=""HiDef"">
    <Parameter Name=""SpecularPower"" Value=""16"" />
    <Texture Name=""NormalTexture"" Format=""Normal"" File=""normal.tga"" />
  </Pass>
  <Pass Name=""Material"" Effect=""DigitalRune/Materials/Material"" Profile=""HiDef"">
    <Parameter Name=""DiffuseColor"" Value=""{1},{2},{3}"" />
    <Parameter Name=""SpecularColor"" Value=""1,1,1"" />
    <Texture Name=""DiffuseTexture"" File=""{12}"" />
    <Texture Name=""SpecularTexture"" File=""specular.tga"" />
  </Pass>
-->

<!-- See the DigitalRune Graphics documentation for a full list of material definition options. -->

</Material>",
          alpha,
          diffuseColor.X, diffuseColor.Y, diffuseColor.Z,
          emissiveColor.X, emissiveColor.Y, emissiveColor.Z,
          environmentMapAmount,
          environmentMapSpecular.X, environmentMapSpecular.Y, environmentMapSpecular.Z,
          fresnelFactor,
          Path.GetFileName(textureFileName),
          Path.GetFileName(environmentMap));
      }
      else
      {
        // Unknown type of MaterialContent.
        return false;
      }

      // Write .drmat file.
      var fileName = Path.ChangeExtension(textureFileName, "drmat");
      try
      {
        using (var stream = File.CreateText(fileName))
        {
          stream.Write(content);
        }
      }
      catch (Exception exception)
      {
        _context.Logger.LogImportantMessage(
          "Automatic creation of material definition \"{0}\" failed.\nException: {1}",
          fileName, exception.ToString());

        return false;
      }

      return true;
    }


    private string GetTextureFileName(ExternalReference<TextureContent> texture)
    {
      try
      {
        return ContentHelper.FindFile(texture.Filename, _input.Identity);
      }
      catch (InvalidContentException)
      {
        // Referenced texture file not found.
        // No problem - use file name, but strip path.
        return Path.GetFileName(texture.Filename);
      }
    }


    // Add missing materials. Split materials if necessary and update "VertexColorEnabled" flag.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void PrepareMaterials()
    {
      // Note: The material may be defined in an external XML file. In this case
      // the local material is ignored!

      // Table that stores a list of GeometryContent objects for each material.
      var geometriesPerMaterial = new Dictionary<MaterialContent, List<GeometryContent>>();

      // GeometryContent objects that have no material assigned are stored in this list.
      List<GeometryContent> geometriesWithoutMaterial = null;

      var meshes = _model.GetSubtree()
                         .OfType<DRMeshNodeContent>()
                         .Select(mi => mi.InputMesh);

      foreach (var mesh in meshes)
      {
        // Sort geometries into geometriesPerMaterial and geometriesWithoutMaterial.
        foreach (GeometryContent geometry in mesh.Geometry)
        {
          if (GetExternalMaterial(mesh, geometry) != null)
          {
            // ----- External material.
            // The material is defined in external XML file.
            // Ignore local material.
            continue;
          }

          // If desired, create a material definition XML file from a MaterialContent.
          if (CreateMaterialDefinition(geometry.Material))
            continue;

          // Uncomment the line below to log a warning message if the material definition 
          // (.drmat file) is missing.
          _context.Logger.LogWarning(
            null, _input.Identity,
            "No material definition (.drmat file) found for mesh {0}, submesh index {1}. Using material stored in model.",
            mesh.Name, mesh.Geometry.IndexOf(geometry));

          // ----- Local material.
          MaterialContent material = geometry.Material;
          if (material != null)
          {
            // Add the 'geometry' to the list of GeometryContent objects that use 'material'.
            List<GeometryContent> geometryList;
            if (!geometriesPerMaterial.TryGetValue(material, out geometryList))
            {
              // Material is not yet stored in table, add entry.
              geometryList = new List<GeometryContent>();
              geometriesPerMaterial.Add(material, geometryList);
            }

            geometryList.Add(geometry);
          }
          else
          {
            if (geometriesWithoutMaterial == null)
              geometriesWithoutMaterial = new List<GeometryContent>();

            geometriesWithoutMaterial.Add(geometry);
          }
        }
      }

      // Process all GeometryContent objects that use a material.
      foreach (var geometryList in geometriesPerMaterial.Values)
        PrepareMaterial(geometryList);

      // Process all GeometryContent objects that do not use a material.
      if (geometriesWithoutMaterial != null)
        PrepareMaterial(geometriesWithoutMaterial);
    }


    private static void PrepareMaterial(List<GeometryContent> geometries)
    {
      var material = geometries[0].Material;

      // Assign default material if material is null.
      if (material == null)
      {
        material = new BasicMaterialContent();
        foreach (GeometryContent geometry in geometries)
          geometry.Material = material;
      }

      // All effects, except the stock EnvironmentMap and SkinnedMaterial effect 
      // can have vertex colors.
      if (!(material is EnvironmentMapMaterialContent) && !(material is SkinnedMaterialContent))
        SetVertexColorEnabled(geometries);
    }


    // Checks if the geometries have a Color vertex channel and updates the VertexColorEnabled 
    // flag in the opaque material data. If necessary, the material is cloned to create one 
    // material with and one without vertex colors.
    private static void SetVertexColorEnabled(List<GeometryContent> geometries)
    {
      var material = geometries[0].Material;

      bool geometryWithVertexColorsFound = false;
      bool geometryWithoutVertexColorsFound = false;
      foreach (GeometryContent geometry in geometries)
      {
        if (geometry.Vertices.Channels.Contains(VertexChannelNames.Color(0)))
          geometryWithVertexColorsFound = true;
        else
          geometryWithoutVertexColorsFound = true;
      }

      if (geometryWithVertexColorsFound && geometryWithoutVertexColorsFound)
      {
        // There are some geometries that have a vertex color and some that don't.
        // --> Create a copy of the current material. One material is used for geometry 
        // with vertex colors enabled. The other for the geometry where vertex colors 
        // are disabled.
        MaterialContent clonedMaterial = (MaterialContent)Activator.CreateInstance(material.GetType());
        ContentHelper.CopyMaterialContent(material, clonedMaterial);
        material.OpaqueData["VertexColorEnabled"] = true;
        clonedMaterial.OpaqueData["VertexColorEnabled"] = false;
        foreach (GeometryContent geometry in geometries)
        {
          if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Color(0)))
            geometry.Material = clonedMaterial;
        }
      }
      else
      {
        if (geometryWithVertexColorsFound)
          material.OpaqueData["VertexColorEnabled"] = true;
        else
          material.OpaqueData.Remove("VertexColorEnabled");
      }
    }


    /// <summary>
    /// Builds the material.
    /// </summary>
    /// <param name="material">
    /// The external material (<see cref="string"/>) or the local material 
    /// (<see cref="MaterialContent"/>).
    /// </param>
    /// <returns>
    /// The processed material.
    /// </returns>
    private object BuildMaterial(object material)
    {
      object convertedMaterial;
      if (!_materials.TryGetValue(material, out convertedMaterial))
      {
        string name = material as string;
        if (name != null)
        {
          // Build external material (XML file).
          string fileName = ContentHelper.FindFile(name, _input.Identity);
          var reference = new ExternalReference<DRMaterialContent>(fileName);
          convertedMaterial = OnBuildMaterial(reference, _context);
        }
        else
        {
          // Build local material.
          convertedMaterial = OnConvertMaterial((MaterialContent)material, _context);
        }
      }

      return convertedMaterial;
    }


    /// <summary>
    /// Called by the framework when a material asset needs to be built.
    /// </summary>
    /// <param name="material">The material asset.</param>
    /// <param name="context">The context of this processor.</param>
    /// <returns>A reference to the processed material.</returns>
    /// <remarks>
    /// This method is called when the model uses a material defined in an XML file. 
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual ExternalReference<DRMaterialContent> OnBuildMaterial(ExternalReference<DRMaterialContent> material, ContentProcessorContext context)
    {
      return context.BuildAsset<DRMaterialContent, DRMaterialContent>(material, typeof(DRMaterialProcessor).Name, null, typeof(DRMaterialImporter).Name, null);
    }


    /// <summary>
    /// Called by the framework when the <see cref="MaterialContent"/> property of a 
    /// <see cref="GeometryContent"/> object is encountered in the input node collection.
    /// </summary>
    /// <param name="material">The input material content.</param>
    /// <param name="context">The context of this processor.</param>
    /// <returns>The converted material content.</returns>
    /// <remarks>
    /// This method is <strong>not</strong> called when the model uses a material defined in an 
    /// external XML file. The method <see cref="OnBuildMaterial"/> is called for external 
    /// materials.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual DRMaterialContent OnConvertMaterial(MaterialContent material, ContentProcessorContext context)
    {
      //var processorParameters = new OpaqueDataDictionary
      //{
      //  { "ColorKeyColor", ColorKeyColor },
      //  { "ColorKeyEnabled", ColorKeyEnabled },
      //  { "GenerateMipmaps", GenerateMipmaps },
      //  { "PremultiplyTextureAlpha", PremultiplyTextureAlpha },
      //  { "ResizeTexturesToPowerOfTwo", ResizeTexturesToPowerOfTwo },
      //  { "TextureFormat", TextureFormat },
      //  { "DefaultEffectType", DefaultEffectType },
      //  { "DefaultEffectFile", DefaultEffectFile },
      //};
      return context.Convert<MaterialContent, DRMaterialContent>(material, typeof(DRMaterialProcessor).Name/*, processorParameters*/);
    }
  }
}
