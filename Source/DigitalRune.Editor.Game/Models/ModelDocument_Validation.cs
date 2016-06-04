// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Linq;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Editor.Models
{
    partial class ModelDocument
    {
        /// <summary>
        /// Checks the model for common mistakes and writes warnings.
        /// </summary>
        private void ValidateModelNode()
        {
            if (ModelNode == null)
                return;

            bool missingMesh = true;
            bool tooSmall = false;
            bool tooBig = false;
            bool tooManyBones = false;
            bool missingPositions = false;
            bool missingTexture = false;
            bool missingTextureCoordinates = false;
            bool missingNormals = false;
            bool missingTangentFrames = false;
            bool wrongAlpha = false;
            bool missingBoneIndices = false;
            bool missingBoneWeights = false;
            foreach (var meshNode in ModelNode.GetDescendants().OfType<MeshNode>())
            {
                missingMesh = false;

                var size = meshNode.Aabb.Extent.Length;
                if (size < 0.01)
                    tooSmall = true;
                else if (size > 100)
                    tooBig = true;

                if (meshNode.SkeletonPose != null && meshNode.SkeletonPose.Skeleton.NumberOfBones > 72)
                    tooManyBones = true;

                foreach (var submesh in meshNode.Mesh.Submeshes)
                {
                    var vertexElements = submesh.VertexBuffer.VertexDeclaration.GetVertexElements();
                    if (!vertexElements.Any(ve => ve.VertexElementUsage == VertexElementUsage.Position))
                        missingPositions = true;
                    if (!vertexElements.Any(ve => ve.VertexElementUsage == VertexElementUsage.TextureCoordinate))
                        missingTextureCoordinates = true;
                    if (!vertexElements.Any(ve => ve.VertexElementUsage == VertexElementUsage.Normal))
                        missingNormals = true;
                    if (!vertexElements.Any(ve => ve.VertexElementUsage == VertexElementUsage.Tangent))
                        missingTangentFrames = true;
                    if (!vertexElements.Any(ve => ve.VertexElementUsage == VertexElementUsage.Binormal))
                        missingTangentFrames = true;
                    if (meshNode.SkeletonPose != null)
                    {
                        if (!vertexElements.Any(ve => ve.VertexElementUsage == VertexElementUsage.BlendIndices))
                            missingBoneIndices = true;
                        if (!vertexElements.Any(ve => ve.VertexElementUsage == VertexElementUsage.BlendWeight))
                            missingBoneWeights = true;
                    }
                }

                foreach (var material in meshNode.Mesh.Materials)
                {
                    foreach (var effectBinding in material.EffectBindings)
                    {
                        if (effectBinding.ParameterBindings.Contains("Diffuse"))
                        {
                            var alphaBinding = effectBinding.ParameterBindings["Diffuse"] as ConstParameterBinding<Vector4>;
                            if (alphaBinding != null && alphaBinding.Value.W < 0.001f)
                                wrongAlpha = true;
                        }
                        else if (effectBinding.ParameterBindings.Contains("DiffuseColor"))
                        {
                            var alphaBinding = effectBinding.ParameterBindings["DiffuseColor"] as ConstParameterBinding<Vector4>;
                            if (alphaBinding != null && alphaBinding.Value.W < 0.001f)
                                wrongAlpha = true;
                        }
                        else if (effectBinding.ParameterBindings.Contains("Alpha"))
                        {
                            var alphaBinding = effectBinding.ParameterBindings["Alpha"] as ConstParameterBinding<float>;
                            if (alphaBinding != null && alphaBinding.Value < 0.001f)
                                wrongAlpha = true;
                        }

                        if (effectBinding.ParameterBindings.Contains("Texture"))
                        {
                            var textureBinding = effectBinding.ParameterBindings["Texture"] as ConstParameterBinding<Texture>;
                            if (textureBinding != null && textureBinding.Value == null)
                                missingTexture = true;
                            var texture2DBinding = effectBinding.ParameterBindings["Texture"] as ConstParameterBinding<Texture2D>;
                            if (texture2DBinding != null && texture2DBinding.Value == null)
                                missingTexture = true;
                        }
                        if (effectBinding.ParameterBindings.Contains("DiffuseTexture"))
                        {
                            var textureBinding = effectBinding.ParameterBindings["DiffuseTexture"] as ConstParameterBinding<Texture>;
                            if (textureBinding != null && textureBinding.Value == null)
                                missingTexture = true;
                            var texture2DBinding = effectBinding.ParameterBindings["DiffuseTexture"] as ConstParameterBinding<Texture2D>;
                            if (texture2DBinding != null && texture2DBinding.Value == null)
                                missingTexture = true;
                        }
                    }
                }
            }

            if (missingMesh)
                AddWarning("Model does not contain any meshes.");

            if (tooSmall)
                AddWarning("Model is very small. Scale it up in a 3D modeling tool or set a scale factor in DRMDL file!");
            else if (tooBig)
                AddWarning("Model is very big. Scale it down in a 3D modeling tool or set a scale factor in DRMDL file!");

            if (tooManyBones)
                AddWarning("Model uses too many bones for mesh skinning. Reduce number of bones to ≤ 72 in a 3D modeling tool!");

            if (missingPositions)
                AddWarning("Missing vertex positions. Correct the model in a 3D modeling tool!");
            if (missingNormals)
                AddWarning("Missing vertex normals. Correct the model in a 3D modeling tool!");
            if (missingTexture)
                AddWarning("Missing diffuse texture.");
            if (missingTextureCoordinates)
                AddWarning("Missing texture coordinates. Add texture coordinates in a 3D modeling tool!");
            if (missingTangentFrames)
                AddWarning("Missing tangent frames. Set GenerateTangentFrames in DRMDL file to true! (Optional for XNA BasicEffect. Required for advanced effects.)");
            if (missingBoneIndices || missingBoneWeights)
                AddWarning("Missing blend indices or blend weights for mesh skinning. Add blend indices and blend weights in a 3D modeling tool!");
            if (wrongAlpha)
                AddWarning("Alpha of mesh is 0 or almost 0.");
        }
    }
}
