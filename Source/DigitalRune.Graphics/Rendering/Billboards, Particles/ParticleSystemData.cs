// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if PARTICLES
using System;
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Particles;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Caches render data for a particle system instance. (Data will be stored in 
  /// <see cref="DigitalRune.Particles.ParticleSystem.RenderData"/>.)
  /// </summary>
  /// <remarks>
  /// The render data of each particle system is stored in 
  /// <see cref="DigitalRune.Particles.ParticleSystem.RenderData"/>. The render data of nested 
  /// particle systems is additionally stored in <see cref="NestedRenderData"/> of the root particle
  /// system. This is necessary because the <see cref="BillboardRenderer"/> cannot access nested 
  /// particle system, if the particle systems are updated concurrently on a different thread.
  /// </remarks>
  internal sealed class ParticleSystemData
  {
    // TODO: Improve performance by using unsafe code.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The frame in which the ParticleSystemData was updated the last time.
    public int Frame;

    // The root particle system and all descendant particle systems.
    // (Only set if this is the root particle system!)
    public List<ParticleSystemData> NestedRenderData;


    #region ----- Particle Parameters -----

    // Uniform particle parameters:
    public IParticleParameter TextureParameter;
    public IParticleParameter<float> AlphaTestParameter;
    public IParticleParameter<BillboardOrientation> BillboardOrientationParameter;
    public IParticleParameter<int> DrawOrderParameter;
    public IParticleParameter<bool> IsDepthSortedParameter;

    // For ribbons:
    public IParticleParameter<ParticleType> TypeParameter;
    //public IParticleParameter<bool> StartsAtOriginParameter; // Not yet implemented.
    public IParticleParameter<int> TextureTilingParameter;

    // Uniform or varying particle parameters:
    public IParticleParameter<float> NormalizedAgeParameter;
    public IParticleParameter<Vector3F> PositionParameter;
    public IParticleParameter<Vector3F> NormalParameter;
    public IParticleParameter<Vector3F> AxisParameter;
    public IParticleParameter<float> SizeParameter;
    public IParticleParameter<float> SizeXParameter;
    public IParticleParameter<float> SizeYParameter;
    public IParticleParameter<float> AngleParameter;
    public IParticleParameter<Vector3F> ColorParameter;
    public IParticleParameter<float> AlphaParameter;
    public IParticleParameter<float> AnimationTimeParameter;
    public IParticleParameter<float> BlendModeParameter;
    public IParticleParameter<float> SoftnessParameter;
    #endregion

    #region ----- Cached Values -----

    // The pose relative to the root particle system.
    public Pose Pose;
    public ParticleReferenceFrame ReferenceFrame;

    // Uniform parameters:
    public PackedTexture Texture;
    public float AlphaTest;
    public BillboardOrientation BillboardOrientation;
    public int DrawOrder; // For state sorting. Particle systems with higher value are draw on top.
    public bool IsDepthSorted;
    public float Softness;

    // For ribbons:
    public bool IsRibbon;
    //public bool StartsAtOrigin; // Not yet implemented.
    public int TextureTiling;

    // The particle data for rendering.
    public ArrayList<Particle> Particles;
    #endregion

    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystemData" /> class.
    /// </summary>
    /// <param name="particleSystem">The particle system.</param>
    public ParticleSystemData(ParticleSystem particleSystem)
    {
      RequeryParameters(particleSystem);
      Particles = new ArrayList<Particle>(Math.Max(particleSystem.NumberOfActiveParticles, 8));
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    public void RequeryParameters(ParticleSystem particleSystem)
    {
      var parameters = particleSystem.Parameters;

      // Uniform particle parameters.
      TextureParameter = parameters.GetUnchecked<PackedTexture>(ParticleParameterNames.Texture);
      if (TextureParameter == null)
        TextureParameter = parameters.GetUnchecked<Texture2D>(ParticleParameterNames.Texture);
      AlphaTestParameter = parameters.Get<float>(ParticleParameterNames.AlphaTest);
      BillboardOrientationParameter = parameters.Get<BillboardOrientation>(ParticleParameterNames.BillboardOrientation);
      DrawOrderParameter = parameters.Get<int>(ParticleParameterNames.DrawOrder);
      IsDepthSortedParameter = parameters.Get<bool>(ParticleParameterNames.IsDepthSorted);
      TypeParameter = parameters.Get<ParticleType>(ParticleParameterNames.Type);
      TextureTilingParameter = parameters.Get<int>(ParticleParameterNames.TextureTiling);

      // Uniform or varying particle parameters.
      NormalizedAgeParameter = parameters.Get<float>(ParticleParameterNames.NormalizedAge);
      PositionParameter = parameters.Get<Vector3F>(ParticleParameterNames.Position);
      NormalParameter = parameters.Get<Vector3F>(ParticleParameterNames.Normal);
      AxisParameter = parameters.Get<Vector3F>(ParticleParameterNames.Axis);
      SizeParameter = parameters.Get<float>(ParticleParameterNames.Size);
      SizeXParameter = parameters.Get<float>(ParticleParameterNames.SizeX);
      SizeYParameter = parameters.Get<float>(ParticleParameterNames.SizeY);
      AngleParameter = parameters.Get<float>(ParticleParameterNames.Angle);
      ColorParameter = parameters.Get<Vector3F>(ParticleParameterNames.Color);
      AlphaParameter = parameters.Get<float>(ParticleParameterNames.Alpha);
      AnimationTimeParameter = parameters.Get<float>(ParticleParameterNames.AnimationTime);
      BlendModeParameter = parameters.Get<float>(ParticleParameterNames.BlendMode);
      SoftnessParameter = parameters.Get<float>(ParticleParameterNames.Softness);
      //StartsAtOriginParameter = parameters.Get<bool>(ParticleParameterNames.StartsAtOrigin);
    }


    /// <summary>
    /// Updates the render data of the <see cref="ParticleSystem" />. Nested particle systems are
    /// ignored.
    /// </summary>
    /// <param name="particleSystem">The particle system.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    public void Update(ParticleSystem particleSystem)
    {
      Particles.Clear();

      var numberOfParticles = particleSystem.NumberOfActiveParticles;
      if (numberOfParticles == 0)
      {
        // Clear texture reference to allow garbage collection.
        // (Only relevant, if user switches texture.)
        Texture = null;
        return;
      }

      // Pose (relative to root particle system)
      var parent = particleSystem.Parent;
      if (parent == null)
      {
        Pose = Pose.Identity;
      }
      else
      {
        // Collect all poses except for the root particle system pose.
        Pose = particleSystem.Pose;
        while (parent.Parent != null)
        {
          Pose = parent.Pose * Pose;
          parent = parent.Parent;
        }
      }

      // ReferenceFrame
      ReferenceFrame = particleSystem.ReferenceFrame;

      // ----- Uniform particle parameters
      // Texture
      var packedTextureParameter = TextureParameter as IParticleParameter<PackedTexture>;
      if (packedTextureParameter != null)
      {
        Texture = packedTextureParameter.DefaultValue;
      }
      else
      {
        var textureParameter = TextureParameter as IParticleParameter<Texture2D>;
        if (textureParameter != null)
        {
          var texture = textureParameter.DefaultValue;
          if (texture != null)
          {
            if (Texture == null || Texture.TextureAtlas != texture)
              Texture = new PackedTexture(texture);
          }
          else
          {
            Texture = null;
          }
        }
      }

      // Particles are not rendered without a texture.
      if (Texture == null)
        return;

      float aspectRatio = 1.0f;
      if (Texture != null)
      {
        var texture = Texture.TextureAtlas;
        float textureAspectRatio = (float)texture.Width / texture.Height;
        Vector2F texCoordTopLeft = Texture.Offset;
        Vector2F texCoordBottomRight = Texture.Offset + (Texture.Scale / new Vector2F(Texture.NumberOfColumns, Texture.NumberOfRows));
        aspectRatio = textureAspectRatio * (texCoordBottomRight.X - texCoordTopLeft.X) / (texCoordBottomRight.Y - texCoordTopLeft.Y);
      }

      // AlphaTest
      AlphaTest = (AlphaTestParameter != null) ? AlphaTestParameter.DefaultValue : 0.0f;

      // BillboardOrientation
      BillboardOrientation = (BillboardOrientationParameter != null) ? BillboardOrientationParameter.DefaultValue : BillboardOrientation.ViewPlaneAligned;

      // DrawOrder
      DrawOrder = (DrawOrderParameter != null) ? DrawOrderParameter.DefaultValue : 0;

      // IsDepthSorted 
      IsDepthSorted = (IsDepthSortedParameter != null) ? IsDepthSortedParameter.DefaultValue : false;

      // Softness
      Softness = (SoftnessParameter != null) ? SoftnessParameter.DefaultValue : 0;
      if (Numeric.IsNaN(Softness))
        Softness = -1;

      // ParticleType (particles vs. ribbons)
      IsRibbon = (TypeParameter != null) ? (TypeParameter.DefaultValue == ParticleType.Ribbon) : false;

      // StartsAtOrigin
      //StartsAtOrigin = (StartsAtOriginParameter != null) ? StartsAtOriginParameter.DefaultValue : false;

      // TextureTiling
      TextureTiling = (TextureTilingParameter != null) ? TextureTilingParameter.DefaultValue : 0;

      // ----- Varying particle parameters
      Particles.AddRange(numberOfParticles); // Values are set below.

      var targetArray = Particles.Array;

      // Determine default size of particles. If one dimension is missing, calculate the
      // missing value using the aspect ratio of the texture.
      Vector2F size = Vector2F.One;
      if (SizeParameter != null)
        size = new Vector2F(SizeParameter.DefaultValue);
      if (SizeXParameter != null)
        size.X = SizeXParameter.DefaultValue;
      if (SizeYParameter != null)
        size.Y = SizeYParameter.DefaultValue;
      if (SizeParameter == null && SizeXParameter != null && SizeYParameter == null)
        size.Y = size.X / aspectRatio;
      if (SizeParameter == null && SizeXParameter == null && SizeYParameter != null)
        size.X = size.Y * aspectRatio;

      // Initialize particles with default values.
      var defaultParticle = new Particle
      {
        Position = (PositionParameter != null) ? PositionParameter.DefaultValue : new Vector3F(),
        Normal = (NormalParameter != null) ? NormalParameter.DefaultValue : Vector3F.UnitZ,
        Axis = (AxisParameter != null) ? AxisParameter.DefaultValue : Vector3F.Up,
        Size = size,
        Angle = (AngleParameter != null) ? AngleParameter.DefaultValue : 0.0f,
        Color = (ColorParameter != null) ? ColorParameter.DefaultValue : Vector3F.One,
        Alpha = (AlphaParameter != null) ? AlphaParameter.DefaultValue : 1.0f,
        BlendMode = (BlendModeParameter != null) ? BlendModeParameter.DefaultValue : 1.0f,

        // AnimationTime is initialized with NormalizedAge below.
      };

      for (int i = 0; i < numberOfParticles; i++)
        targetArray[i] = defaultParticle;

      int startIndex = particleSystem.ParticleStartIndex;
      int totalCount = numberOfParticles;
      int count0 = totalCount;
      int endIndex0 = startIndex + count0;
      int endIndex1 = 0;
      if (endIndex0 > particleSystem.MaxNumberOfParticles)
      {
        count0 = particleSystem.MaxNumberOfParticles - startIndex;
        endIndex0 = particleSystem.MaxNumberOfParticles;
        endIndex1 = numberOfParticles - count0;
      }

      // NormalizedAge
      if (NormalizedAgeParameter != null && !NormalizedAgeParameter.IsUniform)
      {
        var sourceArray = NormalizedAgeParameter.Values;
        for (int sourceIndex = startIndex, targetIndex = 0; sourceIndex < endIndex0; sourceIndex++, targetIndex++)
        {
          targetArray[targetIndex].IsAlive = (sourceArray[sourceIndex] < 1.0f);
          targetArray[targetIndex].AnimationTime = sourceArray[sourceIndex];
        }

        for (int sourceIndex = 0, targetIndex = count0; sourceIndex < endIndex1; sourceIndex++, targetIndex++)
        {
          targetArray[targetIndex].IsAlive = (sourceArray[sourceIndex] < 1.0f);
          targetArray[targetIndex].AnimationTime = sourceArray[sourceIndex];
        }
      }

      // Position
      if (PositionParameter != null && !PositionParameter.IsUniform)
      {
        var sourceArray = PositionParameter.Values;
        for (int sourceIndex = startIndex, targetIndex = 0; sourceIndex < endIndex0; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Position = sourceArray[sourceIndex];

        for (int sourceIndex = 0, targetIndex = count0; sourceIndex < endIndex1; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Position = sourceArray[sourceIndex];
      }

      // Normal
      if (NormalParameter != null && !NormalParameter.IsUniform)
      {
        var sourceArray = NormalParameter.Values;
        for (int sourceIndex = startIndex, targetIndex = 0; sourceIndex < endIndex0; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Normal = sourceArray[sourceIndex];

        for (int sourceIndex = 0, targetIndex = count0; sourceIndex < endIndex1; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Normal = sourceArray[sourceIndex];
      }

      // Axis
      if (AxisParameter != null && !AxisParameter.IsUniform)
      {
        var sourceArray = AxisParameter.Values;
        for (int sourceIndex = startIndex, targetIndex = 0; sourceIndex < endIndex0; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Axis = sourceArray[sourceIndex];

        for (int sourceIndex = 0, targetIndex = count0; sourceIndex < endIndex1; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Axis = sourceArray[sourceIndex];
      }

      // Size
      if (SizeParameter != null && !SizeParameter.IsUniform)
      {
        var sourceArray = SizeParameter.Values;
        for (int sourceIndex = startIndex, targetIndex = 0; sourceIndex < endIndex0; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Size = new Vector2F(sourceArray[sourceIndex]);

        for (int sourceIndex = 0, targetIndex = count0; sourceIndex < endIndex1; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Size = new Vector2F(sourceArray[sourceIndex]);
      }
      if (SizeXParameter != null && !SizeXParameter.IsUniform)
      {
        var sourceArray = SizeXParameter.Values;
        for (int sourceIndex = startIndex, targetIndex = 0; sourceIndex < endIndex0; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Size.X = sourceArray[sourceIndex];

        for (int sourceIndex = 0, targetIndex = count0; sourceIndex < endIndex1; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Size.X = sourceArray[sourceIndex];
      }
      if (SizeYParameter != null && !SizeYParameter.IsUniform)
      {
        var sourceArray = SizeYParameter.Values;
        for (int sourceIndex = startIndex, targetIndex = 0; sourceIndex < endIndex0; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Size.Y = sourceArray[sourceIndex];

        for (int sourceIndex = 0, targetIndex = count0; sourceIndex < endIndex1; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Size.Y = sourceArray[sourceIndex];
      }
      if (SizeParameter == null && SizeXParameter != null && !SizeXParameter.IsUniform && SizeYParameter == null)
      {
        for (int i = 0; i < numberOfParticles; i++)
          targetArray[i].Size.Y = targetArray[i].Size.X / aspectRatio;
      }
      if (SizeParameter == null && SizeXParameter == null && SizeYParameter != null && !SizeYParameter.IsUniform)
      {
        for (int i = 0; i < numberOfParticles; i++)
          targetArray[i].Size.X = targetArray[i].Size.Y * aspectRatio;
      }

      // Angle
      if (AngleParameter != null && !AngleParameter.IsUniform)
      {
        var sourceArray = AngleParameter.Values;
        for (int sourceIndex = startIndex, targetIndex = 0; sourceIndex < endIndex0; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Angle = sourceArray[sourceIndex];

        for (int sourceIndex = 0, targetIndex = count0; sourceIndex < endIndex1; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Angle = sourceArray[sourceIndex];
      }

      // Color
      if (ColorParameter != null && !ColorParameter.IsUniform)
      {
        var sourceArray = ColorParameter.Values;
        for (int sourceIndex = startIndex, targetIndex = 0; sourceIndex < endIndex0; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Color = sourceArray[sourceIndex];

        for (int sourceIndex = 0, targetIndex = count0; sourceIndex < endIndex1; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Color = sourceArray[sourceIndex];
      }

      // Alpha
      if (AlphaParameter != null && !AlphaParameter.IsUniform)
      {
        var sourceArray = AlphaParameter.Values;
        for (int sourceIndex = startIndex, targetIndex = 0; sourceIndex < endIndex0; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Alpha = sourceArray[sourceIndex];

        for (int sourceIndex = 0, targetIndex = count0; sourceIndex < endIndex1; sourceIndex++, targetIndex++)
          targetArray[targetIndex].Alpha = sourceArray[sourceIndex];
      }

      // AnimationTime
      if (AnimationTimeParameter != null)
      {
        // AnimationTime has been initialized with NormalizedAge for automatic animations. 
        // But the "AnimationTime" parameter is set explicitly!
        if (AnimationTimeParameter.IsUniform)
        {
          float animationTime = AnimationTimeParameter.DefaultValue;
          for (int i = 0; i < numberOfParticles; i++)
            targetArray[i].AnimationTime = animationTime;
        }
        else
        {
          var sourceArray = AnimationTimeParameter.Values;
          for (int sourceIndex = startIndex, targetIndex = 0; sourceIndex < endIndex0; sourceIndex++, targetIndex++)
            targetArray[targetIndex].AnimationTime = sourceArray[sourceIndex];

          for (int sourceIndex = 0, targetIndex = count0; sourceIndex < endIndex1; sourceIndex++, targetIndex++)
            targetArray[targetIndex].AnimationTime = sourceArray[sourceIndex];
        }
      }

      // BlendMode
      if (BlendModeParameter != null && !BlendModeParameter.IsUniform)
      {
        var sourceArray = BlendModeParameter.Values;
        for (int sourceIndex = startIndex, targetIndex = 0; sourceIndex < endIndex0; sourceIndex++, targetIndex++)
          targetArray[targetIndex].BlendMode = sourceArray[sourceIndex];

        for (int sourceIndex = 0, targetIndex = count0; sourceIndex < endIndex1; sourceIndex++, targetIndex++)
          targetArray[targetIndex].BlendMode = sourceArray[sourceIndex];
      }
    }
    #endregion
  }
}
#endif
