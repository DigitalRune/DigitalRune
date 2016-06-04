// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Text;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides the descriptions effects using the <i>DirectX Standard Annotations and Semantics 
  /// (DXSAS)</i> version 0.8.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class Dxsas08EffectInterpreter : IEffectInterpreter
  {
    // DXSAS semantics are case-insensitive!
    private static readonly Dictionary<string, Func<EffectParameter, int, EffectParameterDescription>> Semantics =
      new Dictionary<string, Func<EffectParameter, int, EffectParameterDescription>>(StringComparer.OrdinalIgnoreCase)
      {
        // Bounding Shapes
        //{ "BOUNDINGBOXMAX",     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.BoundingBoxMax, i, EffectParameterHint.PerInstance) },
        //{ "BOUNDINGBOXMIN",     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.BoundingBoxMin, i, EffectParameterHint.PerInstance) },
        //{ "BOUNDINGBOXSIZE",    (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.BoundingBoxSize, i, EffectParameterHint.PerInstance) },
        //{ "BOUNDINGCENTER",     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.BoundingCenter, i, EffectParameterHint.PerInstance) },
        //{ "BOUNDINGSPHERESIZE", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.BoundingSphereRadius, i, EffectParameterHint.PerInstance) },
        //{ "BOUNDINGSPHEREMIN",  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.BoundingSphereRadius, i, EffectParameterHint.PerInstance) },
        //{ "BOUNDINGSPHEREMAX",  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.BoundingSphereRadius, i, EffectParameterHint.PerInstance) },

        // Camera
        { "CAMERAPOSITION", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.CameraPosition, i, EffectParameterHint.Global) },

        // Lights
        { "AMBIENT",       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.AmbientLight, i, EffectParameterHint.Local) },
        { "LIGHTPOSITION", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.PointLightPosition, i, EffectParameterHint.Local) },
        { "DIRECTION",     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightDirection, i, EffectParameterHint.Local) },
        { "FALLOFFANGLE",  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.SpotlightCutoffAngle, i, EffectParameterHint.Local) },
                           // Yes! The SAS falloff angle is actually the cutoff angle!

        // Material
        { "DIFFUSE",       (p, i) => GetColorOrTexture(p, i, DefaultEffectParameterSemantics.DiffuseColor, DefaultEffectParameterSemantics.DiffuseTexture) },
        { "DIFFUSEMAP",    (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.DiffuseTexture, i, EffectParameterHint.Material) },
        { "SPECULAR",      (p, i) => GetColorOrTexture(p, i, DefaultEffectParameterSemantics.SpecularColor, DefaultEffectParameterSemantics.SpecularTexture) },
        { "SPECULARMAP",   (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.SpecularTexture, i, EffectParameterHint.Material) },
        { "SPECULARPOWER", (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.SpecularPower, i, EffectParameterHint.Material) },
        { "EMISSIVE",      (p, i) => GetColorOrTexture(p, i, DefaultEffectParameterSemantics.EmissiveColor, DefaultEffectParameterSemantics.EmissiveTexture) },
        { "OPACITY",       (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.Opacity, i, EffectParameterHint.Material) },
        { "NORMAL",        (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.NormalTexture, i, EffectParameterHint.Material) },
        //{ "HEIGHT",        (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.Height, i, EffectParameterHint.Material) },
        //{ "REFRACTION",    (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.Refraction, i, EffectParameterHint.Material) },
        //{ "TEXTUREMATRIX", (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.TextureMatrix, i, EffectParameterHint.Material) },

        // Environment
        { "ENVIRONMENT",       (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.EnvironmentMap, i, EffectParameterHint.Local) },
        { "ENVMAP",            (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.EnvironmentMap, i, EffectParameterHint.Local) },
        //{ "ENVIRONMENTNORMAL", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.EnvironmentNormalMap, i, EffectParameterHint.Local) },

        // Misc
        //{ "RANDOM", (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.RandomValue, i, EffectParameterHint.PerInstance) },
                      // Could also be another sort hint?

        // Render Properties
        //{ "RENDERTARGETDIMENSIONS", (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.RenderTargetSize, i, EffectParameterHint.Global) },
        { "VIEWPORTPIXELSIZE",      (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.ViewportSize, i, EffectParameterHint.Global) },

        // Simulation
        //{ "TIME",        (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.Time, i, EffectParameterHint.Global) },
        //{ "LASTTIME",    (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.LastTime, i, EffectParameterHint.Global) },
        { "ELAPSEDTIME", (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.ElapsedTime, i, EffectParameterHint.Global)},

        // World, View, Projection
        { "POSITION", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.Position, i, EffectParameterHint.PerInstance) },

        { "WORLD",                 (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.World, i, EffectParameterHint.PerInstance) },
        { "WORLDINVERSE",          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldInverse, i, EffectParameterHint.PerInstance) },
        { "WORLDTRANSPOSE",        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldTranspose, i, EffectParameterHint.PerInstance) },
        { "WORLDINVERSETRANSPOSE", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldInverseTranspose, i, EffectParameterHint.PerInstance) },

        { "VIEW",                 (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.View, i, EffectParameterHint.Global) },
        { "VIEWINVERSE",          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewInverse, i, EffectParameterHint.Global) },
        { "VIEWTRANSPOSE",        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewTranspose, i, EffectParameterHint.Global) },
        { "VIEWINVERSETRANSPOSE", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewInverseTranspose, i, EffectParameterHint.Global) },

        { "PROJECTION",                 (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.Projection, i, EffectParameterHint.Global) },
        { "PROJECTIONINVERSE",          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectionInverse, i, EffectParameterHint.Global) },
        { "PROJECTIONTRANSPOSE",        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectionTranspose, i, EffectParameterHint.Global) },
        { "PROJECTIONINVERSETRANSPOSE", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ProjectionInverseTranspose, i, EffectParameterHint.Global) },

        { "WORLDVIEW",                 (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldView, i, EffectParameterHint.PerInstance) },
        { "WORLDVIEWINVERSE",          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewInverse, i, EffectParameterHint.PerInstance) },
        { "WORLDVIEWTRANSPOSE",        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewTranspose, i, EffectParameterHint.PerInstance) },
        { "WORLDVIEWINVERSETRANSPOSE", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewInverseTranspose, i, EffectParameterHint.PerInstance) },

        { "VIEWPROJECTION",                 (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewProjection, i, EffectParameterHint.Global) },
        { "VIEWPROJECTIONINVERSE",          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewProjectionInverse, i, EffectParameterHint.Global) },
        { "VIEWPROJECTIONTRANSPOSE",        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewProjectionTranspose, i, EffectParameterHint.Global) },
        { "VIEWPROJECTIONINVERSETRANSPOSE", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.ViewProjectionInverseTranspose, i, EffectParameterHint.Global) },

        { "WORLDVIEWPROJECTION",                 (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewProjection, i, EffectParameterHint.PerInstance) },
        { "WORLDVIEWPROJECTIONINVERSE",          (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewProjectionInverse, i, EffectParameterHint.PerInstance) },
        { "WORLDVIEWPROJECTIONTRANSPOSE",        (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewProjectionTranspose, i, EffectParameterHint.PerInstance) },
        { "WORLDVIEWPROJECTIONINVERSETRANSPOSE", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewProjectionInverseTranspose, i, EffectParameterHint.PerInstance) },
      };

    // In the following all semantics from DXSAS 0.8 are listed that are not supported.
    // (Remark: 'Not supported' means only that there are no EffectParameterBindings setup
    // automatically. - But of course they can be set manually in the application.)
    //
    // The following are DirectX specific, but never actually used:
    //  JOINT                           Object space to frame-joint space (float4).
    //  JOINTWORLD                      The joint world matrix (float4x4).
    //  JOINTWORLDINVERSE               The inverse joint world matrix (float4x4).
    //  JOINTWORLDTRANSPOSE             The transpose of the joint world matrix (float4x4).
    //  JOINTWORLDINVERSETRANSPOSE      The transpose of the inverse joint world matrix (float4x4).
    //  JOINTWORLDVIEW                  The joint world-view matrix (float4x4).
    //  JOINTWORLDVIEWINVERSE           The inverse world-view matrix (float4x4).
    //  JOINTWORLDVIEWTRANSPOSE         The transpose of the joint world-view matrix (float4x4).
    //  JOINTWORLDVIEWINVERSETRANSPOSE  The transpose of the inverse joint world-view matrix (float4x4).
    // 
    // The following semantics are not supported by DigitalRune Graphics because improved light models are used:
    //  ATTENUATION           The light attenuation as float3 (constant factor, linear factor, quadratic factor).
    //  CONSTANTATTENUATION   The constant light attenuation factor (float).
    //  LINEARATTENUATION     The linear light attenuation factor (float).
    //  QUADRATICATTENUATION  The quadratic light attenuation factor (float).
    //  FALLOFFEXPONENT       The spotlight falloff exponent (float).

    // The following semantics that are specific to NVIDIA (see http://developer.nvidia.com/object/using_sas.html) 
    // and SAS Scripting is not supported:
    //  STANDARDSGLOBAL
    //  RENDERCOLORTARGET
    //  RENDERDEPTHSTENCILTARGET
    //  RENDERTARGETCLIPPING
    //  RESETPULSE
    //  FXCOMPOSER_RESETPULSE
    //  VIEWPORTRATIO
    //  MOUSEPOSITION
    //  LEFTMOUSEDOWN
    //  RIGHTMOUSEDOWN
    //  ANIMATIONTIME
    //  ANIMATIONTICK
    //
    // The following semantics are design-time specific and are not supported at runtime:
    //  UNITSSCALE


    private static EffectParameterDescription GetColorOrTexture(EffectParameter parameter, int index, string colorSemantic, string textureSemantic)
    {
      if (parameter.ParameterClass == EffectParameterClass.Vector
          && parameter.ParameterType == EffectParameterType.Single)
      {
        // Color
        return new EffectParameterDescription(parameter, colorSemantic, index, EffectParameterHint.Material);
      }

      if (parameter.ParameterClass == EffectParameterClass.Object
          && parameter.ParameterType == EffectParameterType.Texture || parameter.ParameterType == EffectParameterType.Texture2D)
      {
        // Texture
        return new EffectParameterDescription(parameter, textureSemantic, index, EffectParameterHint.Material);
      }

      return null;
    }


    /// <inheritdoc/>
    public EffectTechniqueDescription GetDescription(Effect effect, EffectTechnique technique)
    {
      return null;
    }


    /// <inheritdoc/>
    public EffectParameterDescription GetDescription(Effect effect, EffectParameter parameter)
    {
      if (parameter == null)
        throw new ArgumentNullException("parameter");

      string semantic = parameter.Semantic;
      if (semantic == null)
        return null;

      // Split semantic "Direction12" into "Direction" and 12.
      int index;
      semantic.SplitTextAndNumber(out semantic, out index);

      if (semantic.Length > 0)
      {
        // Convert DXSAS semantic to standard semantic.
        Func<EffectParameter, int, EffectParameterDescription> createDescription;
        if (Semantics.TryGetValue(semantic, out createDescription))
          return createDescription(parameter, index);
      }

      return null;
    }
  }
}
