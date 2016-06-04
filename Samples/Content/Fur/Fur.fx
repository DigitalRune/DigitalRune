//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Fur.fx
/// Fur rendering using "shells".
//
// To render fur/hair/grass/etc. the object is rendered in several passes. In
// each pass the object is extruded a bit more in the direction of the normal
// vectors. A random value texture is used to define where hairs are placed.
// Shell pixels of hairs are drawn; shell pixels between hairs are transparent.
//
// The first shell is always drawn completely. All other passes draw only
// the hairs. The second pass sets the cull mode to NONE. The last pass restores
// the default cull mode (CCW).
//
// This effect is best used in the forward rendering "AlphaBlend" pass. Alpha
// blending helps to hide undersampling artifacts a bit.
//
// For the lighting, the result of the light accumulation buffers are used.
// Therefore, the object has to be drawn in the opaque render passes.
// The disadvantage of this lighting trick is that the lighting is incorrect if
// the base of a hair is occluded by another object or outside the viewport.
//
// This effect uses a fixed number of passes/shells. This way, we do not need 
// any special C# code. The effect is completely controlled by this effect.
// Alternatively, we could use a single pass with an annotation and a custom 
// EffectTechniqueBinding which checks the annotations and executes the
// pass several times. However, this requires a new C# EffectTechniqueBinding
// class. This alternative method is shown in the TechniqueBindingSample.
//
//-----------------------------------------------------------------------------

#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Encoding.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Material.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Deferred.fxh"


//--------------------------------------------------------
// Constants
//--------------------------------------------------------

// The number of layers/shells.
static const int NumberOfLayers = 28;

// The index of the current pass.
int PassIndex : PASSINDEX;

float4x4 World : WORLD;
float4x4 ViewProjection : VIEWPROJECTION;
float2 ViewportSize : VIEWPORTSIZE;

// The max length of an individual hair.
float MaxFurLength = 0.04;

// A displacement vector (in world space) that is used to displace the shells. 
// Can be used to bend the hairs, e.g. to fake gravity.
float3 FurDisplacement : FURDISPLACEMENT;

// The hair density: 0 = no hairs, 1 = every pixel is a hair.
float FurDensity = 0.5f;

// Inner shells are rendered with darker color to fake self shadowing.
// 0 = no self shadows, 1 = the first shell is black.
float SelfShadowStrength = 0.7;

// A texture map with random values. 
Texture2D JitterMap: JITTERMAP;
sampler JitterMapSampler = sampler_state
{
  Texture = <JitterMap>;
  AddressU  = WRAP;
  AddressV  = WRAP;
  MipFilter = LINEAR;
  MinFilter = LINEAR;
  
  // (Using POINT, we get rectangular hair. Using LINEAR, we get hairs that
  // look more organic. like grass leafs.
  MagFilter = LINEAR;
};

// The JitterMapScale determines the thickness of the hairs.
float JitterMapScale = 0.02;

// The material.
float3 DiffuseColor : DIFFUSECOLOR;
float3 SpecularColor : SPECULARCOLOR;
DECLARE_UNIFORM_DIFFUSETEXTURE      // Diffuse (RGB) + Alpha (A)
DECLARE_UNIFORM_SPECULARTEXTURE     // Specular (RGB) + Emissive (A)
float Alpha = 0.5;

// The light accumulation buffers for diffuse and specular lighting.
DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer1, 1);


//-----------------------------------------------------------------------------
// Input, output
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float3 Normal : NORMAL;
  float2 TexCoord : TEXCOORD;
};


struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
  float4 Position : SV_Position;
};


struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input) 
{
  VSOutput output = (VSOutput)0;
  output.TexCoord = input.TexCoord;

  // The projected position without displacement. (Used to sample the light buffer.)
  float4 positionWorld = mul(input.Position, World);
  output.PositionProj = mul(positionWorld, ViewProjection);
  
  // A value between 0 and 1 proportional to the pass index.
  float layer = float(PassIndex + 1) / NumberOfLayers;
  
  // Displace the vertex along the normal.
  float3 layerPositionLocal = input.Position.xyz + MaxFurLength * layer * input.Normal;
  float4 layerPositionWorld = mul(float4(layerPositionLocal,1), World);
    
  // Apply the displacement vector. Outer shells are displaced more by using a
  // non-linear curve.
  layerPositionWorld.xyz += FurDisplacement * pow(layer, 3);
      
  output.Position = mul(layerPositionWorld, ViewProjection);
   
  return output;
}


float4 PS(PSInput input) : COLOR
{
  float layer = float(PassIndex) / (NumberOfLayers - 1);
  
  float alpha = 1;
  if (PassIndex > 0)  // Shell 0 is always drawn completely.
  {
    // Clip pixels which are not on hairs.
    float random = tex2D(JitterMapSampler, input.TexCoord / JitterMapScale).r;
    float offset = 1 - FurDensity;
    float furVisibility = (layer <= (random - offset) / FurDensity);
    clip(furVisibility - 0.0001);
    alpha = Alpha;
  }
  
  // Get material.
  float4 diffuseMap = tex2D(DiffuseSampler, input.TexCoord);
  float4 specularMap = tex2D(SpecularSampler, input.TexCoord);
  float3 diffuse = FromGamma(diffuseMap.rgb);
  float3 specular = FromGamma(specularMap.rgb);
  
  // Get the screen space texture coordinate for this position.
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  
  // Get lighting from light accumulation buffers.
  float4 lightBuffer0Sample = tex2D(LightBuffer0Sampler, texCoordScreen);
  float4 lightBuffer1Sample = tex2D(LightBuffer1Sampler, texCoordScreen);
  float3 diffuseLight = GetLightBufferDiffuse(lightBuffer0Sample, lightBuffer1Sample);
  float3 specularLight = GetLightBufferSpecular(lightBuffer0Sample, lightBuffer1Sample);
  
  float4 result = float4(
    DiffuseColor * diffuse * diffuseLight + SpecularColor * specular * specularLight, 
    alpha);
  
  // Make inner shells darker and outer shells brighter to fake self shadows.
  result.xyz *= lerp(1 - SelfShadowStrength, 1 + SelfShadowStrength, layer);
  
  // We use pre-multiplied alpha-blending.
  result.xyz *= alpha;

  return result;
}


#if !SM4
  #define VSTARGET vs_2_0
  #define PSTARGET ps_2_0
#else
  #define VSTARGET vs_4_0_level_9_1
  #define PSTARGET ps_4_0_level_9_1
#endif   

technique
{
  pass 
  {
    CullMode = NONE;
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }

  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }

  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }

  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass 
  {
    CullMode = CCW;
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
