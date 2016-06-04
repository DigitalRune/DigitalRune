// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.Specialized;
using DigitalRune.Animation.Character;
using DigitalRune.Editor.Outlines;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Windows.Controls;
using DigitalRune.Windows.Themes;
using Microsoft.Xna.Framework.Graphics;
using Material = DigitalRune.Graphics.Material;
using Mesh = DigitalRune.Graphics.Mesh;
using Path = System.IO.Path;


namespace DigitalRune.Editor.Models
{
    partial class ModelDocument
    {
        //--------------------------------------------------------------
        #region Constant
        //--------------------------------------------------------------

        private const string ToolTipGeneral = "Select an item to show additional information in Properties window.";
        private const string ToolTipSceneNode = "Select node to show in Properties window and highlight in 3D view.\n(Highlighting is only visible when animations are stopped.)";
        private const string ToolTipAssimpNode = "This tree represents the data imported by the Assimp library.\nSelect node to show in Properties window.";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        internal Outline Outline;
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

        private void UpdateOutline()
        {
            if (Outline == null)
            {
                Outline = new Outline();
                Outline.SelectedItems.CollectionChanged += OnOutlineSelectedItemsChanged;
            }

            Outline.RootItems.Clear();

            if (State != ModelDocumentState.Loaded)
                return;

            var root = new OutlineItem
            {
                Text = Path.GetFileName(Uri.LocalPath),
                Icon = MultiColorGlyphs.Model,
                Children = new OutlineItemCollection(),
                IsSelected = true,
                ToolTip = ToolTipGeneral,
            };
            Outline.RootItems.Add(root);

            if (ModelNode != null)
                root.Children.Add(CreateOutlineItem(ModelNode));
            else if (Model != null)
                root.Children.Add(CreateOutlineItem(Model));

            if (_assimpScene != null)
                root.Children.Add(CreateOutlineItem(_assimpScene));

            if (_documentService.ActiveDocument == this)
            {
                // This document is active and can control tool windows.
                if (_outlineService != null)
                    _outlineService.Outline = Outline;
            }
        }


        private void OnOutlineSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            // The selection has changed. --> Update Properties window.

            if (_propertiesService == null)
                return;
            if (_outlineService == null)
                return;

            // Do nothing if the Outline window is displaying a different outline.
            if (_outlineService.Outline != Outline)
                return;

            // Per default, we show the general model info.
            _currentPropertySource = _modelPropertySource;

            // If something in the Outline window is selected, we show the selected item.
            if (Outline.SelectedItems.Count > 0)
            {
                var selectedItem = Outline.SelectedItems[0];
                var userData = selectedItem.UserData;
                if (userData != null)
                {
                    _currentPropertySource = PropertyGridHelper.CreatePropertySource(
                        selectedItem.UserData,
                        selectedItem.Text);
                }
            }

            _propertiesService.PropertySource = _currentPropertySource;
        }


        private static OutlineItem CreateOutlineItem(SceneNode sceneNode)
        {
            var item = new OutlineItem
            {
                Text = $"{sceneNode.GetType().Name} \"{sceneNode.Name}\"",
                Icon = MultiColorGlyphs.SceneNode,
                Children = new OutlineItemCollection(),
                ToolTip = ToolTipSceneNode,
                UserData = sceneNode,
            };

            if (sceneNode is MeshNode)
                item.Icon = MultiColorGlyphs.Mesh;

            if (sceneNode.Children != null)
                foreach (var child in sceneNode.Children)
                    if (child != null)
                        item.Children.Add(CreateOutlineItem(child));

            var mesh = (sceneNode as MeshNode)?.Mesh;
            if (mesh != null)
                item.Children.Add(CreateOutlineItem(mesh));

            return item;
        }


        private static OutlineItem CreateOutlineItem(Mesh mesh)
        {
            var item = new OutlineItem
            {
                Text = $"Mesh \"{mesh.Name}\"",
                Icon = MultiColorGlyphs.Mesh,
                Children = new OutlineItemCollection(),
                ToolTip = ToolTipSceneNode,
                UserData = mesh,
            };

            foreach (var submesh in mesh.Submeshes)
                item.Children.Add(CreateOutlineItem(submesh));

            foreach (var material in mesh.Materials)
                item.Children.Add(CreateOutlineItem(material));

            if (mesh.Skeleton != null)
                item.Children.Add(CreateOutlineItem(mesh.Skeleton));

            if (mesh.Animations != null)
                foreach (var animation in mesh.Animations)
                    item.Children.Add(CreateOutlineItem(animation));

            if (mesh.Occluder != null)
                item.Children.Add(CreateOutlineItem(mesh.Occluder));

            return item;
        }


        private static OutlineItem CreateOutlineItem(Submesh submesh)
        {
            var item = new OutlineItem
            {
                Text = "Submesh",
                Icon = MultiColorGlyphs.Mesh,
                ToolTip = ToolTipSceneNode,
                UserData = submesh,
            };
            return item;
        }


        private static OutlineItem CreateOutlineItem(Material material)
        {
            var item = new OutlineItem
            {
                Text = $"Material \"{material.Name}\"",
                Icon = MultiColorGlyphs.Texture,
                ToolTip = ToolTipSceneNode,
                UserData = material,
            };
            return item;
        }


        private static OutlineItem CreateOutlineItem(KeyValuePair<string, SkeletonKeyFrameAnimation> animation)
        {
            var item = new OutlineItem
            {
                Text = $"Animation \"{animation.Key}\"",
                Icon = MultiColorGlyphs.Animation,
                UserData = animation,
            };
            return item;
        }


        private static OutlineItem CreateOutlineItem(Skeleton skeleton)
        {
            var item = new OutlineItem
            {
                Text = $"Skeleton \"{skeleton.Name}\"",
                Icon = MultiColorGlyphs.Skeleton,
                IsExpanded = false,
                Children = new OutlineItemCollection(),
                UserData = skeleton,
            };

            for (int bone = 0; bone < skeleton.NumberOfBones; bone++)
                if (skeleton.GetParent(bone) == -1)
                    item.Children.Add(CreateOutlineItem(skeleton, bone));

            return item;
        }


        private static OutlineItem CreateOutlineItem(Skeleton skeleton, int boneIndex)
        {
            var item = new OutlineItem
            {
                Text = $"Bone {boneIndex} \"{skeleton.GetName(boneIndex)}\"",
                Icon = MultiColorGlyphs.Bone,
                Children = new OutlineItemCollection(),
                UserData = boneIndex,
            };

            for (int bone = 0; bone < skeleton.NumberOfBones; bone++)
                if (skeleton.GetParent(bone) == boneIndex)
                    item.Children.Add(CreateOutlineItem(skeleton, bone));

            return item;
        }


        private static OutlineItem CreateOutlineItem(Occluder occluder)
        {
            var item = new OutlineItem
            {
                Text = "Occluder",
            };
            return item;
        }


        private OutlineItem CreateOutlineItem(Model model)
        {
            var item = new OutlineItem
            {
                Text = $"Model \"{Path.GetFileNameWithoutExtension(Uri.LocalPath)}\"",
                Icon = MultiColorGlyphs.Model,
                Children = new OutlineItemCollection(),
                UserData = model,
            };

            if (model.Root != null)
                item.Children.Add(CreateOutlineItem(model.Root));

            return item;
        }


        private static OutlineItem CreateOutlineItem(ModelBone bone)
        {
            var item = new OutlineItem
            {
                Text = $"ModelBone \"{bone.Name}\"",
                Icon = MultiColorGlyphs.Bone,
                Children = new OutlineItemCollection(),
                UserData = bone,
            };

            foreach (var mesh in bone.Meshes)
                item.Children.Add(CreateOutlineItem(mesh));

            foreach (var child in bone.Children)
                item.Children.Add(CreateOutlineItem(child));

            return item;
        }


        private static OutlineItem CreateOutlineItem(ModelMesh mesh)
        {
            var item = new OutlineItem
            {
                Text = $"ModelMesh \"{mesh.Name}\"",
                Icon = MultiColorGlyphs.Mesh,
                Children = new OutlineItemCollection(),
                UserData = mesh,
            };

            foreach (var meshPart in mesh.MeshParts)
                item.Children.Add(CreateOutlineItem(meshPart));

            return item;
        }


        private static OutlineItem CreateOutlineItem(ModelMeshPart meshPart)
        {
            var item = new OutlineItem
            {
                Text = "ModelMeshPart",
                Icon = MultiColorGlyphs.Mesh,
                UserData = meshPart,
            };

            return item;
        }


        private static OutlineItem CreateOutlineItem(Assimp.Scene scene)
        {
            var item = new OutlineItem
            {
                Text = "Assimp scene",
                Icon = MultiColorGlyphs.SceneNode,
                Children = new OutlineItemCollection(),
                ToolTip = ToolTipAssimpNode,
                UserData = scene,
                IsExpanded = false,
            };

            item.Children.Add(CreateOutlineItem(scene.RootNode));

            return item;
        }


        private static OutlineItem CreateOutlineItem(Assimp.Node node)
        {
            var item = new OutlineItem
            {
                Text = node.Name,
                Icon = node.HasMeshes ? MultiColorGlyphs.Mesh : MultiColorGlyphs.SceneNode,
                Children = new OutlineItemCollection(),
                ToolTip = ToolTipAssimpNode,
                UserData = node,
            };

            if (node.HasChildren)
                foreach (var child in node.Children)
                    item.Children.Add(CreateOutlineItem(child));

            return item;
        }
        #endregion
    }
}
