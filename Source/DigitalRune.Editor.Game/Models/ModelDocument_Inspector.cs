// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using DigitalRune.Editor.Documents;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Windows.Controls;
using static System.FormattableString;


namespace DigitalRune.Editor.Models
{
    partial class ModelDocument
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly ComponentResourceKey TextBlockKey = new ComponentResourceKey(typeof(PropertyGrid), "TextBlock");
        private static readonly ComponentResourceKey OpenLinkKey = new ComponentResourceKey(typeof(PropertyGrid), "OpenLink");

        // The basic property source for the model.
        private PropertySource _modelPropertySource;
        private readonly List<AnimationPropertyViewModel> _animationPropertyViewModels = new List<AnimationPropertyViewModel>();

        // The current property source displayed in the Properties window.
        // Per default, it is _modelPropertySource. But it can also represent another outline item.
        private PropertySource _currentPropertySource;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void UpdateProperties()
        {
            if (_modelPropertySource == null)
                _modelPropertySource = new PropertySource();

            _currentPropertySource = _modelPropertySource;

            _modelPropertySource.Name = this.GetName();
            _modelPropertySource.TypeName = "3D Model";
            _modelPropertySource.Properties.Clear();

            if (State != ModelDocumentState.Loaded)
                return;

            // Try to load drmdl file.
            string drmdlFile = Path.ChangeExtension(Uri.LocalPath, "drmdl");
            if (UseDigitalRuneGraphics && File.Exists(drmdlFile))
            {
                _modelPropertySource.Properties.Add(new CustomProperty
                {
                    Category = "General",
                    Name = "Model description",
                    Value = drmdlFile,
                    //Description = ,
                    PropertyType = typeof(string),
                    DataTemplateKey = OpenLinkKey,
                    CanReset = false,
                    IsReadOnly = true,
                });
            }

            int numberOfVertices = 0;
            int numberOfPrimitives = 0;
            if (UseDigitalRuneGraphics)
            {
                foreach (var mesh in ModelNode.GetDescendants().OfType<MeshNode>().Select(mn => mn.Mesh))
                {
                    foreach (var submesh in mesh.Submeshes)
                    {
                        numberOfVertices += submesh.VertexCount;
                        numberOfPrimitives += submesh.PrimitiveCount;
                    }
                }
            }
            else
            {
                foreach (var meshPart in Model.Meshes.SelectMany(m => m.MeshParts))
                {
                    numberOfVertices += meshPart.NumVertices;
                    numberOfPrimitives += meshPart.PrimitiveCount;
                }
            }

            _modelPropertySource.Properties.Add(new CustomProperty
            {
                Category = "General",
                Name = "Triangles",
                Value = numberOfPrimitives,
                //Description = ,
                PropertyType = typeof(int),
                DataTemplateKey = TextBlockKey,
                CanReset = false,
                IsReadOnly = true,
            });

            _modelPropertySource.Properties.Add(new CustomProperty
            {
                Category = "General",
                Name = "Vertices",
                Value = numberOfVertices,
                //Description = ,
                PropertyType = typeof(int),
                DataTemplateKey = TextBlockKey,
                CanReset = false,
                IsReadOnly = true,
            });

            // Try to load drmat files.
            if (UseDigitalRuneGraphics)
            {
                string drmdlFolder = Path.GetDirectoryName(drmdlFile);
                var drmatFiles = new HashSet<string>();
                var textures = new HashSet<string>();
                var xDocument = XDocument.Load(drmdlFile);
                foreach (var submeshElement in xDocument.Descendants("Submesh"))
                {
                    var materialAttribute = submeshElement.Attributes("Material").FirstOrDefault();
                    if (materialAttribute == null)
                        continue;

                    string drmatFile = GetAbsolutePath(drmdlFolder, materialAttribute.Value);
                    if (!File.Exists(drmatFile))
                        continue;

                    drmatFiles.Add(drmatFile);
                    string drmatFolder = Path.GetDirectoryName(drmatFile);

                    // Collect all referenced texture filenames.
                    var drmatXDocument = XDocument.Load(drmatFile);
                    foreach (var textureElement in drmatXDocument.Descendants("Texture"))
                    {
                        var fileAttribute = textureElement.Attributes("File").FirstOrDefault();
                        if (fileAttribute == null)
                            continue;

                        string textureFile = GetAbsolutePath(drmatFolder, fileAttribute.Value);
                        textures.Add(textureFile);
                    }
                }

                int i = 0;
                foreach (var drmatFile in drmatFiles)
                {
                    _modelPropertySource.Properties.Add(new CustomProperty
                    {
                        Category = "Materials",
                        Name = Invariant($"Material {i}"),
                        Value = drmatFile,
                        //Description = ,
                        PropertyType = typeof(string),
                        DataTemplateKey = OpenLinkKey,
                        CanReset = false,
                        IsReadOnly = true,
                    });
                    i++;
                }

                i = 0;
                foreach (var texture in textures)
                {
                    _modelPropertySource.Properties.Add(new CustomProperty
                    {
                        Category = "Textures",
                        Name = Invariant($"Texture {i}"),
                        Value = texture,
                        //Description = ,
                        PropertyType = typeof(string),
                        DataTemplateKey = OpenLinkKey,
                        CanReset = false,
                        IsReadOnly = true,
                    });
                    i++;
                }

                _animationPropertyViewModels.Clear();
                if (HasAnimations)
                {
                    var mesh = ModelNode.GetDescendants().OfType<MeshNode>().First().Mesh;
                    foreach (var entry in mesh.Animations)
                    {
                        var animationPropertyViewModel = new AnimationPropertyViewModel(this)
                        {
                            Name = entry.Key,
                            Animation = entry.Value,
                        };
                        _animationPropertyViewModels.Add(animationPropertyViewModel);

                        _modelPropertySource.Properties.Add(new CustomProperty
                        {
                            Category = "Animations",
                            Name = "\"" + entry.Key + "\"",
                            Value = animationPropertyViewModel,
                            //Description = ,
                            PropertyType = typeof(AnimationPropertyViewModel),
                            CanReset = false,
                            IsReadOnly = true,
                        });
                    }
                }
            }

            if (_documentService.ActiveDocument == this)
            {
                // This document is active and can control tool windows.
                if (_propertiesService != null)
                    _propertiesService.PropertySource = _currentPropertySource;
            }
        }


        /// <summary>
        /// Tries to get the absolute path for the specified file.
        /// </summary>
        /// <param name="folder">The current folder.</param>
        /// <param name="fileName">The absolute or relative path and name of the file.</param>
        /// <returns>The absolute path and name of the file.</returns>
        private static string GetAbsolutePath(string folder, string fileName)
        {
            if (!Path.IsPathRooted(fileName) && !string.IsNullOrEmpty(folder))
                fileName = Path.Combine(folder, fileName);

            return fileName;
        }
        #endregion
    }
}
