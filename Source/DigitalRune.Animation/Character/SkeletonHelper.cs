// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;

#if XNA || MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Provides helper methods for working with skeletons.
  /// </summary>
  public static class SkeletonHelper
  {
#if XNA || MONOGAME
    /// <summary>
    /// Draws the skeleton bones, bone space axes and bone names for debugging. 
    /// (Only available in the XNA-compatible build.)
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="effect">
    /// A <see cref="BasicEffect"/> instance. The effect parameters <see cref="BasicEffect.World"/>,
    /// <see cref="BasicEffect.View"/>, and <see cref="BasicEffect.Projection"/> must be
    /// correctly initialized before this method is called.
    /// </param>
    /// <param name="axisLength">The visible length of the bone space axes.</param>
    /// <param name="spriteBatch">
    /// A <see cref="SpriteBatch"/>. Can be <see langword="null"/> to skip text rendering.
    /// </param>
    /// <param name="spriteFont">
    /// A <see cref="SpriteFont"/>. Can be <see langword="null"/> to skip text rendering.
    /// </param>
    /// <param name="color">The color for the bones and the bone names.</param>
    /// <remarks>
    /// <para>
    /// This method is available only in the XNA-compatible build of the DigitalRune.Animation.dll.
    /// </para>
    /// <para>
    /// This method draws the skeleton for debugging. It draws a line for each bone and the bone
    /// name. At the bone origin it draws 3 lines (red, green, blue) that visualize the bone
    /// space axes (x, y, z).
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose"/>, <paramref name="graphicsDevice"/> or 
    /// <paramref name="effect"/> is <see langword="null"/>.
    /// </exception>
    public static void DrawBones(this SkeletonPose skeletonPose, GraphicsDevice graphicsDevice,
      BasicEffect effect, float axisLength, SpriteBatch spriteBatch, SpriteFont spriteFont, Color color)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (effect == null)
        throw new ArgumentNullException("effect");

      var oldVertexColorEnabled = effect.VertexColorEnabled;
      effect.VertexColorEnabled = true;

      // No font, then we don't need the sprite batch.
      if (spriteFont == null)
        spriteBatch = null;

      if (spriteBatch != null)
        spriteBatch.Begin();

      List<VertexPositionColor> vertices = new List<VertexPositionColor>();

      var skeleton = skeletonPose.Skeleton;
      for (int i = 0; i < skeleton.NumberOfBones; i++)
      {
        string name = skeleton.GetName(i);
        SrtTransform bonePose = skeletonPose.GetBonePoseAbsolute(i);
        var translation = (Vector3)bonePose.Translation;
        var rotation = (Quaternion)bonePose.Rotation;
        
        int parentIndex = skeleton.GetParent(i);
        if (parentIndex >= 0)
        {
          // Draw line to parent joint representing the parent bone.
          SrtTransform parentPose = skeletonPose.GetBonePoseAbsolute(parentIndex);
          vertices.Add(new VertexPositionColor(translation, color));
          vertices.Add(new VertexPositionColor((Vector3)parentPose.Translation, color));
        }

        // Add three lines in Red, Green and Blue.
        vertices.Add(new VertexPositionColor(translation, Color.Red));
        vertices.Add(new VertexPositionColor(translation + Vector3.Transform(Vector3.UnitX, rotation) * axisLength, Color.Red));
        vertices.Add(new VertexPositionColor(translation, Color.Green));
        vertices.Add(new VertexPositionColor(translation + Vector3.Transform(Vector3.UnitY, rotation) * axisLength, Color.Green));
        vertices.Add(new VertexPositionColor(translation, Color.Blue));
        vertices.Add(new VertexPositionColor(translation + Vector3.Transform(Vector3.UnitZ, rotation) * axisLength, Color.Blue));

        // Draw name.
        if (spriteBatch != null && !string.IsNullOrEmpty(name))
        {
          // Compute the 3D position in view space. Text is rendered near drawn x axis.
          Vector3 textPosition = translation + Vector3.TransformNormal(Vector3.UnitX, bonePose) * axisLength * 0.5f;
          var textPositionWorld = Vector3.Transform(textPosition, effect.World);
          var textPositionView = Vector3.Transform(textPositionWorld, effect.View);

          // Check if the text is in front of the camera.
          if (textPositionView.Z < 0)
          {
            // Project text position to screen.
            Vector3 textPositionProjected = graphicsDevice.Viewport.Project(textPosition, effect.Projection, effect.View, effect.World);
            spriteBatch.DrawString(spriteFont, name + " " + i, new Vector2(textPositionProjected.X, textPositionProjected.Y), color);
          }
        }
      }

      if (spriteBatch != null)
        spriteBatch.End();

      // Draw axis lines in one batch.
      graphicsDevice.DepthStencilState = DepthStencilState.None;
      effect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices.ToArray(), 0, vertices.Count / 2);
      
      effect.VertexColorEnabled = oldVertexColorEnabled;
    }
#endif


    //public static void RotateBoneWorld(this SkeletonPose SkeletonPose, int boneIndex, QuaternionF rotation, Matrix44F world)
    //{
    //  QuaternionF worldRotation = QuaternionF.CreateRotation(world.Minor);
    //  RotateBoneAbsolute(SkeletonPose, boneIndex, worldRotation.Conjugated * rotation);
    //}


    // TODO: This method should really be called RotateBoneLocalAnimated?
    ///// <summary>
    ///// Rotates bone where the rotation is given in the bone space. 
    ///// </summary>
    ///// <param name="skeletonPose">The skeleton pose.</param>
    ///// <param name="boneIndex">The index of the bone.</param>
    ///// <param name="rotation">The rotation in bone space.</param>
    ///// <exception cref="ArgumentNullException">
    ///// <paramref name="skeletonPose" /> is <see langword="null"/>.
    ///// </exception>
    //public static void RotateBoneLocal(this SkeletonPose skeletonPose, int boneIndex, QuaternionF rotation)
    //{
    //  if (skeletonPose == null)
    //    throw new ArgumentNullException("skeletonPose");

    //  var boneTransform = skeletonPose.GetBoneTransform(boneIndex);
    //  boneTransform.Rotation = boneTransform.Rotation * rotation;
    //  skeletonPose.SetBoneTransform(boneIndex, boneTransform);
    //}


    /// <summary>
    /// Rotates a bone where the rotation is given in model space.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <param name="rotation">The rotation in model space.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose" /> is <see langword="null"/>.
    /// </exception>
    public static void RotateBoneAbsolute(this SkeletonPose skeletonPose, int boneIndex, QuaternionF rotation)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      var skeleton = skeletonPose.Skeleton;
      var boneTransform = skeletonPose.GetBoneTransform(boneIndex);
      var bindPoseRelative = skeleton.GetBindPoseRelative(boneIndex);
      var parentIndex = skeleton.GetParent(boneIndex);
      var parentBonePoseAbsolute = skeletonPose.GetBonePoseAbsolute(parentIndex);

      // Solving these equations (using only the rotations):
      // rotation * bonePoseAbsolute = bonePoseAbsoluteNew
      // bonePoseAbsolute = parentBonePoseAbsolute * bindPoseRelative * boneTransform.
      // ...

      // Rotation relative to bone bind pose space (using similarity transformation).
      var rotationRelative = bindPoseRelative.Rotation.Conjugated
                             * parentBonePoseAbsolute.Rotation.Conjugated
                             * rotation
                             * parentBonePoseAbsolute.Rotation
                             * bindPoseRelative.Rotation;

      // The final rotation is: First rotate into bone bind pose space, then apply rotation.
      boneTransform.Rotation = rotationRelative * boneTransform.Rotation;

      // So many multiplications, numerical errors adds up quickly in iterative IK algorithms...
      boneTransform.Rotation.Normalize();
      
      skeletonPose.SetBoneTransform(boneIndex, boneTransform);
    }



    /// <summary>
    /// Sets the bone rotation of a bone so that it matches the given rotation in model space.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <param name="rotation">The rotation in model space.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose" /> is <see langword="null"/>.
    /// </exception>
    public static void SetBoneRotationAbsolute(this SkeletonPose skeletonPose, int boneIndex, QuaternionF rotation)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      var skeleton = skeletonPose.Skeleton;
      var bindPoseRelative = skeleton.GetBindPoseRelative(boneIndex);
      var parentIndex = skeleton.GetParent(boneIndex);
      var parentBonePoseAbsolute = (parentIndex >= 0) ? skeletonPose.GetBonePoseAbsolute(parentIndex) : SrtTransform.Identity;

      // Solving this equation (using only rotations): 
      // rotation = parentBonePoseAbsolute * bindPoseRelative * rotationRelative;
      // rotationRelative = boneTransform.

      var rotationRelative = bindPoseRelative.Rotation.Conjugated
                             * parentBonePoseAbsolute.Rotation.Conjugated
                             * rotation;
      
      rotationRelative.Normalize();

      var boneTransform = skeletonPose.GetBoneTransform(boneIndex);
      boneTransform.Rotation = rotationRelative;
      skeletonPose.SetBoneTransform(boneIndex, boneTransform);
    }


    /// <summary>
    /// Sets the bone transform to create a desired pose in model space.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <param name="bonePoseAbsolute">The bone pose in model space.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose" /> is <see langword="null"/>.
    /// </exception>
    public static void SetBonePoseAbsolute(this SkeletonPose skeletonPose, int boneIndex, SrtTransform bonePoseAbsolute)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      var skeleton = skeletonPose.Skeleton;
      var bindPoseRelative = skeleton.GetBindPoseRelative(boneIndex);
      var parentIndex = skeleton.GetParent(boneIndex);
      var parentBonePoseAbsolute = (parentIndex >= 0) ? skeletonPose.GetBonePoseAbsolute(parentIndex) : SrtTransform.Identity;

      // Solving this equation: 
      // bonePoseAbsolute = parentBonePoseAbsolute * bindPoseRelative * BoneTransform;

      var boneTransform = bindPoseRelative.Inverse * parentBonePoseAbsolute.Inverse * bonePoseAbsolute;

      boneTransform.Rotation.Normalize();

      skeletonPose.SetBoneTransform(boneIndex, boneTransform);
    }



    /// <summary>
    /// Determines whether the given bone indices form a valid bone chain.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="ancestorBoneIndex">Index of the start bone (root of the chain). Can be -1.</param>
    /// <param name="childBoneIndex">Index of the end bone (tip of the chain). Must not be -1.</param>
    /// <returns>
    /// <see langword="true"/> if bone indices describe a valid chain; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method checks it the start bone is an ancestor of the end bone.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose" /> is <see langword="null"/>.
    /// </exception>
    public static bool IsAncestor(this SkeletonPose skeletonPose, int ancestorBoneIndex, int childBoneIndex)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      if (ancestorBoneIndex > childBoneIndex)
        return false;

      if (childBoneIndex < 0)
        return false;

      if (ancestorBoneIndex < 0)
        return true;

      var skeleton = skeletonPose.Skeleton;
      int boneIndex = skeleton.GetParent(childBoneIndex);
      while (boneIndex >= 0)
      {
        if (boneIndex == ancestorBoneIndex)
          return true;

        boneIndex = skeleton.GetParent(boneIndex);
      }
      
      // Parent not found in the chain.
      return false;
    }


    /// <summary>
    /// Determines whether the given bone indices form a valid bone chain.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="ancestorBoneIndex">Index of the start bone (root of the chain). Can be -1.</param>
    /// <param name="childBoneIndex">Index of the end bone (tip of the chain). Must not be -1.</param>
    /// <returns>
    /// <see langword="true"/> if bone indices describe a valid chain; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method checks it the start bone is an ancestor of the end bone.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose" /> is <see langword="null"/>.
    /// </exception>
    public static bool IsAncestorOrSelf(this SkeletonPose skeletonPose, int ancestorBoneIndex, int childBoneIndex)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      if (ancestorBoneIndex > childBoneIndex)
        return false;

      if (childBoneIndex < 0)
        return false;

      if (ancestorBoneIndex < 0)
        return true;

      var skeleton = skeletonPose.Skeleton;
      int boneIndex = childBoneIndex;
      while (boneIndex >= 0)
      {
        if (boneIndex == ancestorBoneIndex)
          return true;

        boneIndex = skeleton.GetParent(boneIndex);
      }

      // Parent not found in the chain.
      return false;
    }


    /// <summary>
    /// Gets the bone indices of a bone chain.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="startBoneIndex">Index of the start bone (root of the chain). Can be -1.</param>
    /// <param name="endBoneIndex">Index of the end bone (tip of the chain). Must not be -1.</param>
    /// <param name="boneIndices">
    /// A list where the bone indices should be stored. Must not be <see langword="null"/>. 
    /// The list is cleared before the new bones are added.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose"/> or <paramref name="boneIndices"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="startBoneIndex"/> and <paramref name="endBoneIndex"/> do not form a valid
    /// bone chain.
    /// </exception>
    public static void GetChain(this SkeletonPose skeletonPose, int startBoneIndex, int endBoneIndex, List<int> boneIndices)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");
      if (boneIndices == null)
        throw new ArgumentNullException("boneIndices");

      if (!IsAncestorOrSelf(skeletonPose, startBoneIndex, endBoneIndex))
        throw new ArgumentException("startBoneIndex and endBoneIndex do not form a valid bone chain.");

      boneIndices.Clear();

      var skeleton = skeletonPose.Skeleton;
      int boneIndex = endBoneIndex;
      while (boneIndex >= 0)
      {
        boneIndices.Add(boneIndex);

        if (boneIndex == startBoneIndex)
          break;

        boneIndex = skeleton.GetParent(boneIndex);
      }

      boneIndices.Reverse();
    }


    /// <summary>
    /// Counts the number of bones in a bone chain.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="startBoneIndex">Index of the start bone (root of the chain). Can be -1.</param>
    /// <param name="endBoneIndex">Index of the end bone (tip of the chain). Must not be -1.</param>
    /// <returns>The number of bones in the chain; or 0 if the chain is invalid.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose" /> is <see langword="null"/>.
    /// </exception>
    public static int GetNumberOfBones(this SkeletonPose skeletonPose, int startBoneIndex, int endBoneIndex)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      if (!IsAncestorOrSelf(skeletonPose, startBoneIndex, endBoneIndex))
        return 0;

      var skeleton = skeletonPose.Skeleton;
      int numberOfBones = 0;
      int boneIndex = endBoneIndex;
      while (boneIndex >= 0)
      {
        numberOfBones++;

        if (boneIndex == startBoneIndex)
          break;

        boneIndex = skeleton.GetParent(boneIndex);
      }

      return numberOfBones;
    }


    /// <overloads>
    /// <summary>
    /// Resets bone transforms.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Resets the bone transforms of all bones in a bone chain.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="startBoneIndex">Index of the start bone (root of the chain). Can be -1.</param>
    /// <param name="endBoneIndex">Index of the end bone (tip of the chain). Must not be -1.</param>
    /// <returns>The number of bones in the chain; or 0 if the chain is invalid.</returns>
    /// <remarks>
    /// If a bone transform is reset, it is set to the <see cref="SrtTransform.Identity"/>
    /// transform. If all bone transforms of a skeleton are reset, then the skeleton is in its
    /// bind pose.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose" /> is <see langword="null"/>.
    /// </exception>
    public static void ResetBoneTransforms(this SkeletonPose skeletonPose, int startBoneIndex, int endBoneIndex)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      var skeleton = skeletonPose.Skeleton;
      int boneIndex = endBoneIndex;
      while (boneIndex >= 0)
      {
        skeletonPose.SetBoneTransform(boneIndex, SrtTransform.Identity);

        if (boneIndex == startBoneIndex)
          break;

        boneIndex = skeleton.GetParent(boneIndex);
      }
    }


    /// <summary>
    /// Resets the bone transform components (scale, rotation or translation) of all bones in a 
    /// bone chain. 
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="startBoneIndex">Index of the start bone (root of the chain). Can be -1.</param>
    /// <param name="endBoneIndex">Index of the end bone (tip of the chain). Must not be -1.</param>
    /// <param name="resetScale">If set to <see langword="true"/>, the scale is reset.</param>
    /// <param name="resetRotation">If set to <see langword="true"/>, the rotation is reset.</param>
    /// <param name="resetTranslation">If set to <see langword="true"/>, the translation is reset.</param>
    /// <remarks>
    /// If a bone transform is reset, it is set to the <see cref="SrtTransform.Identity"/>
    /// transform. If all bone transforms of a skeleton are reset, then the skeleton is in its
    /// bind pose.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// 	<paramref name="skeletonPose"/> is <see langword="null"/>.
    /// </exception>
    public static void ResetBoneTransforms(this SkeletonPose skeletonPose, int startBoneIndex, 
      int endBoneIndex, bool resetScale, bool resetRotation, bool resetTranslation)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      var skeleton = skeletonPose.Skeleton;
      int boneIndex = endBoneIndex;
      while (boneIndex >= 0)
      {
        var boneTransform = skeletonPose.GetBoneTransform(boneIndex);
        if (resetScale)
          boneTransform.Scale = Vector3F.One;
        if (resetRotation)
          boneTransform.Rotation = QuaternionF.Identity;
        if (resetTranslation)
          boneTransform.Translation = Vector3F.Zero;

        skeletonPose.SetBoneTransform(boneIndex, boneTransform);

        if (boneIndex == startBoneIndex)
          break;

        boneIndex = skeleton.GetParent(boneIndex);
      }
    }


    /// <summary>
    /// Resets the bone transform components (scale, rotation or translation) of all bones in a 
    /// bone subtree.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="boneIndex">
    /// Index of the root bone of the subtree. Must not be negative.
    /// </param>
    /// <param name="resetScale">If set to <see langword="true"/>, the scale is reset.</param>
    /// <param name="resetRotation">If set to <see langword="true"/>, the rotation is reset.</param>
    /// <param name="resetTranslation">If set to <see langword="true"/>, the translation is reset.</param>
    /// <remarks>
    /// If a bone transform is reset, it is set to the <see cref="SrtTransform.Identity"/>
    /// transform. If all bone transforms of a skeleton are reset, then the skeleton is in its
    /// bind pose.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="boneIndex"/> is negative.</exception>
    public static void ResetBoneTransformsInSubtree(this SkeletonPose skeletonPose, int boneIndex, bool resetScale, bool resetRotation, bool resetTranslation)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      if (boneIndex < 0)
        throw new ArgumentOutOfRangeException("boneIndex");

      var boneTransform = skeletonPose.GetBoneTransform(boneIndex);
      if (resetScale)
        boneTransform.Scale = Vector3F.One;
      if (resetRotation)
        boneTransform.Rotation = QuaternionF.Identity;
      if (resetTranslation)
        boneTransform.Translation = Vector3F.Zero;

      skeletonPose.SetBoneTransform(boneIndex, boneTransform);

      var skeleton = skeletonPose.Skeleton;
      for (int i = 0; i < skeleton.GetNumberOfChildren(boneIndex); i++)
        ResetBoneTransformsInSubtree(skeletonPose, skeleton.GetChild(boneIndex, i), resetScale, resetRotation, resetTranslation);
    }


    /// <summary>
    /// Copies the bone transforms from skeleton pose to another skeleton pose.
    /// </summary>
    /// <param name="source">The <see cref="SkeletonPose"/> from which the bone transforms are copied.</param>
    /// <param name="target">The <see cref="SkeletonPose"/> to which the bone transforms are copied.</param>
    /// <remarks>
    /// Copying a <see cref="SkeletonPose"/> using this method is faster than manually copying all
    /// bone transforms.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="source"/> and <paramref name="target"/> belong to different skeletons and
    /// <paramref name="target"/> has more bones than <paramref name="source"/>.
    /// </exception>
    public static void Copy(SkeletonPose source, SkeletonPose target)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      if (target == null)
        throw new ArgumentNullException("target");

      if (target != source)
      {
        var sourceTransforms = source.BoneTransforms;
        var targetTransforms = target.BoneTransforms;
        Array.Copy(sourceTransforms, 0, targetTransforms, 0, targetTransforms.Length);
        target.Invalidate();
      }
    }
  }
}
