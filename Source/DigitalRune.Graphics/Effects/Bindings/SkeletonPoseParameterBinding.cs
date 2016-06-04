// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if ANIMATION
using System;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Animation.Character;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Binds an <see cref="EffectParameter"/> to the skinning matrices of <see cref="SkeletonPose"/>
  /// of the current <see cref="MeshNode"/>.
  /// </summary>
  [DebuggerDisplay("{GetType().Name,nq}(Parameter = {Parameter.Name}, Value = {Value})")]
  public class SkeletonPoseParameterBinding : EffectParameterBinding
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the skeleton pose.
    /// </summary>
    /// <value>The skeleton pose.</value>
    public SkeletonPose Value { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="SkeletonPoseParameterBinding"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="SkeletonPoseParameterBinding"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or
    /// other special cases!)
    /// </summary>
    protected SkeletonPoseParameterBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SkeletonPoseParameterBinding"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    public SkeletonPoseParameterBinding(Effect effect, EffectParameter parameter)
      : base(effect, parameter)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override EffectParameterBinding CreateInstanceCore()
    {
      return new SkeletonPoseParameterBinding();
    }


    /// <inheritdoc/>
    protected override void CloneCore(EffectParameterBinding source)
    {
      // Clone EffectParameterBinding properties.
      base.CloneCore(source);

      // Clone SkeletonPoseParameterBinding properties.
      var sourceTyped = (SkeletonPoseParameterBinding)source;
      Value = sourceTyped.Value;
    }
    #endregion


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnUpdate(RenderContext context)
    {
      var meshNode = context.SceneNode as MeshNode;
      if (meshNode != null)
      {
        Value = meshNode.SkeletonPose;
        if (Value != null)
        {
          if (Value.Skeleton.NumberOfBones > Parameter.Elements.Count)
          {
            Value = null;

            var message = string.Format(
              CultureInfo.InvariantCulture,
              "Cannot update skeleton pose effect parameter binding: " +
              "The skeleton has {0} bones. The effect supports only {1} bones.",
              meshNode.SkeletonPose.Skeleton.NumberOfBones,
              Parameter.Elements.Count);
            throw new GraphicsException(message);
          }

          Value.Update();
        }
      }
      else
      {
        Value = null;
      }
    }


    /// <inheritdoc/>
    protected override void OnApply(RenderContext context)
    {
      if (Value != null)
        Parameter.SetValue(Value.SkinningMatricesXna);
    }
    #endregion
  }
}
#endif
