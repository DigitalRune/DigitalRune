// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DigitalRune.Graphics.Content.Pipeline;
using DigitalRune.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using DRPath = DigitalRune.Storages.Path;
using static System.FormattableString;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Creates .DRMDL/.DRMAT files (if required) and calls the <see cref="DRModelProcessor"/>.
    /// </summary>
    //[CLSCompliant(false)]
    [ContentProcessor(DisplayName = "GameModelProcessor")]
    internal class GameModelProcessor : DRModelProcessor
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // Input
        private NodeContent _input;
        //private ContentProcessorContext _context;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether the model description (*.DRMDL file) should be
        /// replaced by an automatically created file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> the model description (*.DRMDL file) should be replaced by an
        /// automatically created file; otherwise, <see langword="false"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DefaultValue(false)]
        [DisplayName("Recreate Model Description File")]
        [Description("If enabled, an existing .DRMDL file is replaced by default files when a new model is processed.")]
        public bool RecreateModelDescriptionFile { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the material definitions (*.DRMAT files) should
        /// be replaced by automatically created files.
        /// </summary>
        /// <value>
        /// <see langword="true"/> the material definitions (*.DRMAT files) should be replaced by
        /// automatically created DRMDL files; otherwise, <see langword="false"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DefaultValue(false)]
        [DisplayName("Recreate Material Description Files")]
        [Description("If enabled, existing .DRMAT files are replaced by default files when a new model is processed.")]
        public bool RecreateMaterialDefinitionFiles { get; set; }
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
                throw new ArgumentNullException(nameof(input));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _input = input;
            //_context = context;

            // Base class should not write .DRMDL/.DRMAT files.
            CreateMissingMaterialDefinition = false;
            CreateMissingModelDescription = false;

            CreateModelDescriptionAndMaterialDefinitions();

            return base.Process(input, context);
        }


        private void CreateModelDescriptionAndMaterialDefinitions()
        {
            string sourceFileName = Path.GetFullPath(_input.Identity.SourceFilename);
            string modelDescriptionFileName = Path.ChangeExtension(sourceFileName, "drmdl");
            bool modelDescriptionExists = File.Exists(modelDescriptionFileName);
            if (modelDescriptionExists && !(RecreateModelDescriptionFile || RecreateMaterialDefinitionFiles))
                return;

            // Create model description (.DRMDL file).
            var xDocument = new XDocument();

            var modelElement =
                new XElement("Model",
                    new XAttribute("File", Path.GetFileName(sourceFileName)),
                    new XAttribute("GenerateTangentFrames", "True"),
                    new XAttribute("PremultiplyVertexColors", "True"),
                    new XAttribute("RotationX", "0"),
                    new XAttribute("RotationY", "0"),
                    new XAttribute("RotationZ", "0"),
                    new XAttribute("Scale", "1"),
                    new XAttribute("SwapWindingOrder", "False"));

            xDocument.Add(modelElement);

            var materialContents = new HashSet<MaterialContent>();

            var meshContents = TreeHelper.GetSubtree(_input, node => node.Children)
                                         .OfType<MeshContent>();
            foreach (var meshContent in meshContents)
            {
                var meshElement =
                    new XElement("Mesh",
                        new XAttribute("Name", meshContent.Name));

                modelElement.Add(meshElement);

                foreach (var geometryContent in meshContent.Geometry)
                {
                    var materialContent = geometryContent.Material;
                    materialContents.Add(materialContent);

                    meshElement.Add(
                        new XElement("Submesh",
                            new XAttribute("Material", GetFileName(materialContent))));
                }
            }

            Save(xDocument, modelDescriptionFileName);

            // Create material definitions (.DRMAT files).
            string modelFolder = Path.GetDirectoryName(modelDescriptionFileName);
            foreach (var materialContent in materialContents)
                CreateMaterialDefinition(modelFolder, materialContent);
        }


        private void CreateMaterialDefinition(string modelFolder, MaterialContent materialContent)
        {
            string materialDescriptionFileName = Path.Combine(modelFolder, GetFileName(materialContent));
            bool materialDescriptionExists = File.Exists(materialDescriptionFileName);
            if (materialDescriptionExists && !RecreateMaterialDefinitionFiles)
                return;

            var basicMaterial = materialContent as BasicMaterialContent;

            float alpha = basicMaterial?.Alpha ?? 1.0f;
            bool isTransparent = alpha < 0.9999f; // || materialContent.Textures.ContainsKey("Transparency");

            Vector3 diffuseColor = basicMaterial?.DiffuseColor ?? new Vector3(1);
            Vector3 emissiveColor = basicMaterial?.EmissiveColor ?? new Vector3(0);
            Vector3 specularColor = basicMaterial?.SpecularColor ?? new Vector3(1);
            float specularPower = basicMaterial?.SpecularPower ?? 100;

            ExternalReference<TextureContent> diffuseTexture;
            materialContent.Textures.TryGetValue("Texture", out diffuseTexture);

            // Note: Open Asset Importer finds specular textures only if they are assigned to the
            // "SpecularColor" slot of the 3ds Max material and not if they are assigned to the
            // "SpecularLevel" slot.
            ExternalReference<TextureContent> specularTexture;
            materialContent.Textures.TryGetValue("Specular", out specularTexture);

            ExternalReference<TextureContent> normalTexture;
            materialContent.Textures.TryGetValue("Bump", out normalTexture);

            bool isSkinned = (MeshHelper.FindSkeleton(_input) != null);

            var materialElement = new XElement("Material");

            // Render pass "Default" or "AlphaBlend"
            {
                var passElement =
                    new XElement("Pass",
                        new XAttribute("Name", isTransparent ? "AlphaBlend" : "Default"),
                        new XAttribute("Effect", isSkinned ? "SkinnedEffect" : "BasicEffect"),
                        new XAttribute("Profile", "Any"));

                materialElement.Add(passElement);

                passElement.Add(
                    new XElement("Parameter",
                        new XAttribute("Name", "Alpha"),
                        new XAttribute("Value", alpha)));
                passElement.Add(
                    new XElement("Parameter",
                        new XAttribute("Name", "DiffuseColor"),
                        new XAttribute("Value", ToXmlValueString(diffuseColor))));
                passElement.Add(
                    new XElement("Parameter",
                        new XAttribute("Name", "EmissiveColor"),
                        new XAttribute("Value", ToXmlValueString(emissiveColor))));
                passElement.Add(
                    new XElement("Parameter",
                        new XAttribute("Name", "SpecularColor"),
                        new XAttribute("Value", ToXmlValueString(specularColor))));
                passElement.Add(
                    new XElement("Parameter", 
                        new XAttribute("Name", "SpecularPower"),
                        new XAttribute("Value", specularPower)));

                if (diffuseTexture != null)
                {
                    passElement.Add(
                        new XElement("Texture",
                            new XAttribute("Name", "Texture"),
                            new XAttribute("Format", "Dxt"),
                            new XAttribute("GenerateMipmaps", "True"),
                            new XAttribute("PremultiplyAlpha", "True"),
                            //new XAttribute("ReferenceAlpha", "True"),
                            new XAttribute("File", DRPath.GetRelativePath(modelFolder, diffuseTexture.Filename))));
                }
            }

            // Render pass "ShadowMap"
            if (!isTransparent)   // Alpha blended meshes do not cast shadows.
            {
                materialElement.Add(
                    new XElement("Pass", 
                        new XAttribute("Name", "ShadowMap"),
                        new XAttribute("Effect", "DigitalRune/Materials/ShadowMap" + (isSkinned ? "Skinned" : string.Empty)),
                        new XAttribute("Profile", "HiDef")));
            }

            // Render pass "GBuffer"
            if (!isTransparent)
            {
                var passElement =
                    new XElement("Pass",
                        new XAttribute("Name", "GBuffer"),
                        new XAttribute("Effect", "DigitalRune/Materials/GBuffer"
                                                 + (normalTexture != null ? "Normal" : string.Empty)
                                                 + (isSkinned ? "Skinned" : string.Empty)));
                passElement.Add(new XAttribute("Profile", "HiDef"));

                materialElement.Add(passElement);

                passElement.Add(
                    new XElement("Parameter",
                        new XAttribute("Name", "SpecularPower"),
                        new XAttribute("Value", specularPower)));

                if (normalTexture != null)
                {
                    passElement.Add(
                        new XElement("Texture",
                            new XAttribute("Name", "NormalTexture"),
                            new XAttribute("Format", "Normal"),
                            new XAttribute("GenerateMipmaps", "True"),
                            new XAttribute("File", DRPath.GetRelativePath(modelFolder, normalTexture.Filename))));
                }
            }

            // Render pass "Material"
            if (!isTransparent)
            {
                var passElement =
                    new XElement("Pass",
                        new XAttribute("Name", "Material"),
                        new XAttribute("Effect", "DigitalRune/Materials/Material" + (isSkinned ? "Skinned" : string.Empty)),
                        new XAttribute("Profile", "HiDef"));

                materialElement.Add(passElement);

                passElement.Add(
                    new XElement("Parameter",
                        new XAttribute("Name", "DiffuseColor"),
                        new XAttribute("Value", ToXmlValueString(diffuseColor))));
                passElement.Add(
                    new XElement("Parameter",
                        new XAttribute("Name", "SpecularColor"),
                        new XAttribute("Value", ToXmlValueString(specularColor))));

                if (diffuseTexture != null)
                {
                    passElement.Add(
                        new XElement("Texture",
                            new XAttribute("Name", "DiffuseTexture"),
                            new XAttribute("Format", "Dxt"),
                            new XAttribute("GenerateMipmaps", "True"),
                            new XAttribute("PremultiplyAlpha", "True"),
                            new XAttribute("File", DRPath.GetRelativePath(modelFolder, diffuseTexture.Filename))));
                }

                if (specularTexture != null)
                {
                    passElement.Add(
                        new XElement("Texture",
                            new XAttribute("Name", "SpecularTexture"),
                            new XAttribute("Format", "Dxt"),
                            new XAttribute("GenerateMipmaps", "True"),
                            new XAttribute("PremultiplyAlpha", "False"),
                            new XAttribute("File", DRPath.GetRelativePath(modelFolder, specularTexture.Filename))));
                }
            }

            Save(new XDocument(materialElement), materialDescriptionFileName);
        }


        /// <summary>
        /// Returns the string representation of the specified vector in a material definition
        /// (.DRMAT file).
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <returns>The string representation.</returns>
        private static string ToXmlValueString(Vector3 v)
        {
            return Invariant($"{v.X},{v.Y},{v.Z}");
        }


        //// Get property from opaque data dictionary.
        //private T GetProperty<T>(MaterialContent materialContent, string key, T defaultValue)
        //{
        //    object value;
        //    if (materialContent.OpaqueData.TryGetValue(key, out value))
        //        return (T)value;
        //    return defaultValue;
        //}


        /// <summary>
        /// Gets the file name of the material definition (.DRMAT file).
        /// </summary>
        /// <param name="materialContent">The material.</param>
        /// <returns>The file name.</returns>
        private static string GetFileName(MaterialContent materialContent)
        {
            return CleanFileName(materialContent.Name) + ".drmat";
        }


        /// <summary>
        /// Removes all invalid characters from a file name.
        /// </summary>
        /// <param name="fileName">The file name (including path).</param>
        /// <returns>The file name with all invalid characters replaced by '_'.</returns>
        private static string CleanFileName(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');

            // MonoGame does not like a few other characters:
            fileName = fileName.Replace('.', '_');
            fileName = fileName.Replace(' ', '_');
            fileName = fileName.Replace('#', '_');

            return fileName;
        }


        /// <summary>
        /// Saves the specified XML document.
        /// </summary>
        /// <param name="document">The XML document.</param>
        /// <param name="fileName">The file name (including path).</param>
        private void Save(XDocument document, string fileName)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    "
            };

            using (var writer = XmlWriter.Create(fileName, settings))
            {
                document.Save(writer);
            }
        }
        #endregion
    }
}
