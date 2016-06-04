//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ShadowMap.fx
/// Renders the model into the shadow map.
//
// ----- Cull Mode, Rendering front faces vs. back faces
// "CryEngine 3: Three Years of Work in Review, GPU PRO 3", recommends to use
// back faces for point lights. We do not use this because this does not work
// with thin alpha tested objects: Thick objects need a positive depth bias to
// move receiver into shadow. Thin alpha-tested objects need a negative depth
// bias. --> We always render front faces. Except, we render front and back
// faces for VSM due to low shadow map texel density (VSM is usually used for
// distant geometry.)
//
// ----- Planar vs. linear depth
// We render planar depth (= view position z value normalized to [0, 1]) for
// most lights.
// Only omnilights have to use linear depth (= distance from camera to pixel
// normalized to [0, 1]) because a cube map shadow has 6 different planar
// directions and this would complicate the shadow mask shader (the shader does
// not distinguish cube sides).
//
// ----- Pancaking
// Normally the depth is computed between the light's camera near and far plane.
// However, using pancaking we can make better use of the depth range. This is
// used for directional lights. The orthogonal projection ranges from
// -MinLightDirection to far. To compute shadow map depth we use near = 0,
// which means all pixels between -MinLightDirection and 0 are pancaked to depth
// 0.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Material.fxh"
#include "../Noise.fxh"
#include "../ShadowMap.fxh"


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

// Possible defines.
//#define ALPHA_TEST 1
//#define MORPHING 1
//#define SKINNING 1
//#define VSM_BIAS 1     // 1 to enable VSM bias to reduce acne.


//-----------------------------------------------------------------------------
// Static Constants
//-----------------------------------------------------------------------------

static const int DepthTypePlanar = 0;
static const int DepthTypeLinear = 1;

// The shadow map type.
static const int SMTypeDefault = 0; // Normal shadow map containing only the depth.
static const int SMTypeVsm = 1;     // Variance shadow map using unbiased second depth moment.


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;

// The camera is the light camera not the player's camera.
float CameraNear : CAMERANEAR;
float CameraFar : CAMERAFAR;

#if ALPHA_TEST
float ReferenceAlpha = 0.9f;
DECLARE_UNIFORM_DIFFUSETEXTURE
#endif

#if MORPHING
float MorphWeight0 : MORPHWEIGHT0;
float MorphWeight1 : MORPHWEIGHT1;
float MorphWeight2 : MORPHWEIGHT2;
float MorphWeight3 : MORPHWEIGHT3;
float MorphWeight4 : MORPHWEIGHT4;
#endif

#if SKINNING
float4x3 Bones[72];
#endif


//-----------------------------------------------------------------------------
// Structs
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION0;
#if ALPHA_TEST
  float2 TexCoord : TEXCOORD0;
#endif
#if MORPHING
  float3 MorphPosition0 : POSITION1;
  float3 MorphPosition1 : POSITION2;
  float3 MorphPosition2: POSITION3;
  float3 MorphPosition3 : POSITION4;
  float3 MorphPosition4 : POSITION5;
#endif
#if SKINNING
  uint4 BoneIndices : BLENDINDICES0;
  float4 BoneWeights : BLENDWEIGHT0;
#endif
};


struct VSOutput
{
  float3 DepthOrPositionView : TEXCOORD0;
#if ALPHA_TEST
  float2 TexCoord : TEXCOORD1;
#endif
  float4 Position : SV_Position;
};


struct PSInput
{
  float3 DepthOrPositionView : TEXCOORD0; // Stores depth in x, or position in xyz.
#if ALPHA_TEST
  float2 TexCoord : TEXCOORD1;
#endif
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, float4x4 world, int depthType, float near)
{
  float4 position = input.Position;
  
#if MORPHING
  // ----- Apply morph targets.
  position.xyz += MorphWeight0 * input.MorphPosition0;
  position.xyz += MorphWeight1 * input.MorphPosition1;
  position.xyz += MorphWeight2 * input.MorphPosition2;
  position.xyz += MorphWeight3 * input.MorphPosition3;
  position.xyz += MorphWeight4 * input.MorphPosition4;
#endif

#if SKINNING
  // ----- Apply skinning matrix.
  float4x3 skinningMatrix = 0;
  skinningMatrix += Bones[input.BoneIndices.x] * input.BoneWeights.x;
  skinningMatrix += Bones[input.BoneIndices.y] * input.BoneWeights.y;
  skinningMatrix += Bones[input.BoneIndices.z] * input.BoneWeights.z;
  skinningMatrix += Bones[input.BoneIndices.w] * input.BoneWeights.w;
  position.xyz = mul(position, skinningMatrix);
#endif
  
  // ----- Apply world, view, projection transformation.
  float4 positionWorld = mul(position, world);
  float4 positionView = mul(positionWorld, View);
  float4 positionProj = mul(positionView, Projection);
  
  // ----- Output
  VSOutput output = (VSOutput)0;
  output.Position = positionProj;
  if (depthType == DepthTypePlanar)
  {
    // Compute planar view space distance (normalized).
    output.DepthOrPositionView.x = (-positionView.z - near) / (CameraFar - near);
  }
  else
  {
    // Pass position in view space to pixel shader.
    output.DepthOrPositionView = positionView.xyz;
  }
#if ALPHA_TEST
  output.TexCoord = input.TexCoord;
#endif
  return output;
}


VSOutput VSNoInstancing(VSInput input, uniform int depthType, uniform float near)
{
  return VS(input, World, depthType, near);
}


VSOutput VSInstancing(VSInput input,
                      float4 worldColumn0 : BLENDWEIGHT0,
                      float4 worldColumn1 : BLENDWEIGHT1,
                      float4 worldColumn2 : BLENDWEIGHT2,
                      float4 colorAndAlpha : BLENDWEIGHT3,
                      uniform int depthType,
                      uniform float near)
{
  float4x4 worldTransposed =
  {
    worldColumn0,
    worldColumn1,
    worldColumn2,
    float4(0, 0, 0, 1)
  };
  float4x4 world = transpose(worldTransposed);
  
  return VS(input, world, depthType, near);
}


float4 PS(PSInput input, uniform int depthType, uniform int smType) : COLOR
{
#if ALPHA_TEST
  float4 diffuse = tex2D(DiffuseSampler, input.TexCoord);
  clip(diffuse.a - ReferenceAlpha);
#endif
  
  float depth;
  if (depthType == DepthTypePlanar)
  {
    // Get normalized planar distance to near plane.
    depth = input.DepthOrPositionView.x;
  }
  else
  {
    // Compute normalized linear distance to camera.
    float3 positionView = input.DepthOrPositionView.xyz;
    depth = length(positionView) / CameraFar;
  }
  
  if (smType == SMTypeDefault)
  {
    return float4(depth.x, 0, 0, 1);
  }
  else if (smType == SMTypeVsm)
  {
#if VSM_BIAS
    bool useBias = true;
#else
    bool useBias = false;
#endif
    float2 moments = GetDepthMoments(depth, useBias);
    return float4(moments.x, moments.y, 0, 1);
  }
}


// TODO: MonoGame does not support parameters like this:
//   VertexShader = compile vs_2_0 VSNoInstancing(DepthTypePlanar);
// Therefore we need to add functions without uniform parameters.
VSOutput VSNoInstancingPlanar(VSInput input) { return VSNoInstancing(input, DepthTypePlanar, CameraNear); }
VSOutput VSNoInstancingPlanarPancaking(VSInput input) { return VSNoInstancing(input, DepthTypePlanar, 0); }
VSOutput VSNoInstancingLinear(VSInput input) { return VSNoInstancing(input, DepthTypeLinear, CameraNear); }
VSOutput VSInstancingPlanar(VSInput input, float4 worldColumn0 : BLENDWEIGHT0, float4 worldColumn1 : BLENDWEIGHT1, float4 worldColumn2 : BLENDWEIGHT2, float4 colorAndAlpha : BLENDWEIGHT3)
{
  return VSInstancing(input, worldColumn0, worldColumn1, worldColumn2, colorAndAlpha, DepthTypePlanar, CameraNear);
}
VSOutput VSInstancingPlanarPancaking(VSInput input, float4 worldColumn0 : BLENDWEIGHT0, float4 worldColumn1 : BLENDWEIGHT1, float4 worldColumn2 : BLENDWEIGHT2, float4 colorAndAlpha : BLENDWEIGHT3)
{
  return VSInstancing(input, worldColumn0, worldColumn1, worldColumn2, colorAndAlpha, DepthTypePlanar, 0);
}
VSOutput VSInstancingLinear(VSInput input, float4 worldColumn0 : BLENDWEIGHT0, float4 worldColumn1 : BLENDWEIGHT1, float4 worldColumn2 : BLENDWEIGHT2, float4 colorAndAlpha : BLENDWEIGHT3)
{
  return VSInstancing(input, worldColumn0, worldColumn1, worldColumn2, colorAndAlpha, DepthTypeLinear, CameraNear);
}

float4 PSPlanarDefault(PSInput input) : COLOR { return PS(input, DepthTypePlanar, SMTypeDefault); }
float4 PSPlanarVsm(PSInput input) : COLOR { return PS(input, DepthTypePlanar, SMTypeVsm); }
float4 PSLinearDefault(PSInput input) : COLOR { return PS(input, DepthTypeLinear, SMTypeDefault); }


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET_2_0 vs_2_0
#define PSTARGET_2_0 ps_2_0
#define VSTARGET_3_0 vs_3_0
#define PSTARGET_3_0 ps_3_0
#else
#define VSTARGET_2_0 vs_4_0_level_9_1
#define PSTARGET_2_0 ps_4_0_level_9_1
#define VSTARGET_3_0 vs_4_0_level_9_3
#define PSTARGET_3_0 ps_4_0_level_9_3
#endif


#if !SKINNING && !MORPHING
#define SUPPORTS_INSTANCING 1
#endif

technique Default
#if !MGFX && SUPPORTS_INSTANCING   // TODO: Add Annotation support to MonoGame.
< string InstancingTechnique = "DefaultInstancing"; >
#endif
{
  pass
  {
#if ALPHA_TEST
    CullMode = NONE;
#else
    CullMode = CCW;
#endif
    VertexShader = compile VSTARGET_2_0 VSNoInstancingPlanar();
    PixelShader = compile PSTARGET_2_0 PSPlanarDefault();
  }
}

#if SUPPORTS_INSTANCING
technique DefaultInstancing
{
  pass
  {
#if ALPHA_TEST
    CullMode = NONE;
#else
    CullMode = CCW;
#endif
    VertexShader = compile VSTARGET_3_0 VSInstancingPlanar();
    PixelShader = compile PSTARGET_3_0 PSPlanarDefault();
  }
}
#endif


technique Directional
#if !MGFX && SUPPORTS_INSTANCING
< string InstancingTechnique = "DirectionalInstancing"; >
#endif
{
  pass
  {
#if ALPHA_TEST
    CullMode = NONE;
#else
    CullMode = CCW;
#endif
    VertexShader = compile VSTARGET_2_0 VSNoInstancingPlanarPancaking();
    PixelShader = compile PSTARGET_2_0 PSPlanarDefault();
  }
}

#if SUPPORTS_INSTANCING
technique DirectionalInstancing
{
  pass
  {
#if ALPHA_TEST
    CullMode = NONE;
#else
    CullMode = CCW;
#endif
    VertexShader = compile VSTARGET_3_0 VSInstancingPlanarPancaking();
    PixelShader = compile PSTARGET_3_0 PSPlanarDefault();
  }
}
#endif


technique DirectionalVsm
#if !MGFX && SUPPORTS_INSTANCING
< string InstancingTechnique = "DirectionalVsmInstancing"; >
#endif
{
  pass
  {
    //CullMode = NONE;  // Set in C# code
    VertexShader = compile VSTARGET_3_0 VSNoInstancingPlanarPancaking();
    PixelShader = compile PSTARGET_3_0 PSPlanarVsm();
  }
}

#if SUPPORTS_INSTANCING
technique DirectionalVsmInstancing
{
  pass
  {
    //CullMode = NONE; // Set in C# code.
    VertexShader = compile VSTARGET_3_0 VSInstancingPlanarPancaking();
    PixelShader = compile PSTARGET_3_0 PSPlanarVsm();
  }
}
#endif


technique Omnidirectional
#if !MGFX && SUPPORTS_INSTANCING
< string InstancingTechnique = "OmnidirectionalInstancing"; >
#endif
{
  pass
  {
#if ALPHA_TEST
    CullMode = NONE;
#else
    CullMode = CCW;
#endif
    VertexShader = compile VSTARGET_2_0 VSNoInstancingLinear();
    PixelShader = compile PSTARGET_2_0 PSLinearDefault();
  }
}

#if SUPPORTS_INSTANCING
technique OmnidirectionalInstancing
{
  pass
  {
#if ALPHA_TEST
    CullMode = NONE;
#else
    CullMode = CCW;
#endif
    VertexShader = compile VSTARGET_3_0 VSInstancingLinear();
    PixelShader = compile PSTARGET_3_0 PSLinearDefault();
  }
}
#endif
