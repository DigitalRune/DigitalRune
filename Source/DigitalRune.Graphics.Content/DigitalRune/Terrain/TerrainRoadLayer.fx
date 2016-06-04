//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file TerrainRoadLayer.fx
/// Renders a road mesh into the terrain detail clipmap.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"


// Very important: Add a negative LOD bias.
// Crisp noisy terrain materials are much better than smooth materials!
// When the terrain is rendered, the clipmaps will be smoothed again.
#define MATERIAL_MIPLODBIAS -1


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// ----- General Constants
// The viewport size in pixels.
float2 ViewportSize;

// Clipmap levels where the material fades in/out.
int FadeInStart;
int FadeInEnd;
int FadeOutStart;
int FadeOutEnd;

// ----- Material parameters
float TileSize;
float3 DiffuseColor;
float3 SpecularColor;
float SpecularPower;
float Alpha;
// Diffuse (RGB) + Alpha (A)
texture DiffuseTexture : DIFFUSETEXTURE;
sampler DiffuseSampler = sampler_state
{
  Texture = <DiffuseTexture>;
  AddressU = WRAP;
  AddressV = WRAP;
  MINFILTER = ANISOTROPIC;
  MAGFILTER = LINEAR;
  MIPFILTER = LINEAR;
#if SM4
  MIPLODBIAS = MATERIAL_MIPLODBIAS;
#else
  MIPMAPLODBIAS = MATERIAL_MIPLODBIAS;
#endif
};
// Specular (RGB)
texture SpecularTexture : SPECULARTEXTURE;
sampler SpecularSampler = sampler_state
{
  Texture = <SpecularTexture>;
  AddressU = WRAP;
  AddressV = WRAP;
  MINFILTER = ANISOTROPIC;
  MAGFILTER = LINEAR;
  MIPFILTER = LINEAR;
#if SM4
  MIPLODBIAS = MATERIAL_MIPLODBIAS;
#else
  MIPMAPLODBIAS = MATERIAL_MIPLODBIAS;
#endif
};
texture NormalTexture : NORMALTEXTURE;
sampler NormalSampler = sampler_state
{
  Texture = <NormalTexture>;
  AddressU = WRAP;
  AddressV = WRAP;
  MINFILTER = ANISOTROPIC;
  MAGFILTER = LINEAR;
  MIPFILTER = LINEAR;
#if SM4
  MIPLODBIAS = MATERIAL_MIPLODBIAS;
#else
  MIPMAPLODBIAS = MATERIAL_MIPLODBIAS;
#endif
};
float HeightTextureScale = 1;
float HeightTextureBias;
texture HeightTexture : HEIGHTTEXTURE;
sampler HeightSampler = sampler_state
{
  Texture = <HeightTexture>;
  AddressU = WRAP;
  AddressV = WRAP;
  MINFILTER = ANISOTROPIC;
  MAGFILTER = LINEAR;
  MIPFILTER = LINEAR;
#if SM4
  MIPLODBIAS = MATERIAL_MIPLODBIAS;
#else
  MIPMAPLODBIAS = MATERIAL_MIPLODBIAS;
#endif
};


// The fade-out ranges of the road borders in texture coordinates.
// BorderBlendRange contains the values for the 4 sides of the road: (left, start, right, end)
float4 BorderBlendRange;

// The length of the road in world space.
float RoadLength;

// ----- Terrain parameters.
// Tile origin and size
float2 TerrainTileOrigin;
float2 TerrainTileSize;
float2 TerrainTileNormalTextureSize;
texture TerrainTileNormalTexture;
sampler TerrainTileNormalSampler = sampler_state
{
  Texture = <TerrainTileNormalTexture>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
};

// Parameters of the target clipmap.
float TerrainClipmapLevel;
float2 TerrainClipmapOffsetWorld;   // World space position of the toroidal wrap offset.
float2 TerrainClipmapOffsetScreen;  // Screen space position of the toroidal wrap offset.
float TerrainClipmapCellSize;


//-----------------------------------------------------------------------------
// Shaders
//-----------------------------------------------------------------------------

void VS(in float2 positionWorld : POSITION0,        // World space xz position
        inout float2 texCoordRoad : TEXCOORD0,      // Road texture coordinates
        out float2 texCoordTerrain : TEXCOORD1,     // Terrain tile textue coordinates
        out float4 positionProj : SV_Position)      // Projection space position
{
  // Convert world space position to screen space position of current target clipmap level.
  float2 positionScreen = TerrainClipmapOffsetScreen + (positionWorld - TerrainClipmapOffsetWorld) / TerrainClipmapCellSize;
  
  // Convert screen space position to projection space.
  positionProj.xy = ScreenToProjection(positionScreen, ViewportSize);
  positionProj.z = 0;
  positionProj.w = 1;
  
  // Convert world space position (X, Z) to texture coordinates.
  texCoordTerrain = (positionWorld - TerrainTileOrigin) / TerrainTileSize;
  
  // The terrain tile origin and the height texture origin differ by half a texel because the
  // tile starts in the center of the first texel.
  texCoordTerrain += 0.5 / TerrainTileNormalTextureSize;
  
  // The road texture tiles in the road direction.
  texCoordRoad.y /= TileSize;
}


void PS(in float2 texCoordRoad : TEXCOORD0,
        in float2 texCoordTerrain : TEXCOORD1,
        out float4 color0 : COLOR0,
        out float4 color1 : COLOR1,
        out float4 color2 : COLOR2)
{
  // Dummy value which we use to circumvent DX9 HLSL compiler preshader bug.
#if !SM4
  float dummy = texCoordTerrain.x * 0.00000001f;
#else
  float dummy = 0;
#endif
  
  //color0 = float4(0.5, 0.5, 0.4, 1);
  //color1 = float4(1, 1, 1, 1);
  //color2 = float4(1, 1, 1, 1);
  //return;
  
  // Sample terrain normal texture of this tile.
  float3 terrainNormal = GetNormal(TerrainTileNormalSampler, texCoordTerrain).rgb;
  
  // Convert the green-up normal map normal to world space.
  terrainNormal = float3(terrainNormal.x, terrainNormal.z, -terrainNormal.y);
  
  float4 diffuseMap = tex2D(DiffuseSampler, texCoordRoad);
  float4 specularMap = tex2D(SpecularSampler, texCoordRoad);
  float3 normal = GetNormalDxt5nm(NormalSampler, texCoordRoad);
  float height = tex2D(HeightSampler, texCoordRoad).x * HeightTextureScale + HeightTextureBias;
  
  // Undo premultiplied alpha:
  diffuseMap.rgb /= diffuseMap.a;
  
  // Convert from gamma to linear space.
  float3 diffuse = FromGamma(diffuseMap.rgb);
  float3 specular = FromGamma(specularMap.rgb);
  
  // Compute alpha.
  float fadeInWeight = saturate((TerrainClipmapLevel + 1.0 - FadeInStart) / (FadeInEnd + 1.0 - FadeInStart) + dummy);
  float fadeOutWeight = 1 - saturate((TerrainClipmapLevel + 1.0 - FadeOutStart) / (FadeOutEnd + 1.0 - FadeOutStart) + dummy);
  
  float alpha = Alpha * diffuseMap.a * fadeInWeight * fadeOutWeight;
  if (BorderBlendRange.x > 0)
    alpha *= saturate(texCoordRoad.x / BorderBlendRange.x);
  if (BorderBlendRange.y > 0)
    alpha *= saturate((texCoordRoad.y) / BorderBlendRange.y);
  if (BorderBlendRange.z > 0)
    alpha *= saturate((1 - texCoordRoad.x) / BorderBlendRange.z);
  if (BorderBlendRange.w > 0)
    alpha *= saturate((RoadLength / TileSize - texCoordRoad.y) / BorderBlendRange.w);
  
  // Convert normal from tangent to world space.
  float3 tangent = normalize(cross(terrainNormal, float3(0, 0, 1)));
  float3 binormal = cross(tangent, terrainNormal);
  normal = terrainNormal * normal.z + tangent * normal.x - binormal * normal.y;
  
  // Convert normal from [-1, 1] range to [0, 1] range.
  normal = (normal + 1.0) * 128.0 / 255.0;
  
  // (normal.x, normal.z, specular exponent, hole alpha)
  color0 = float4(normal.x, normal.z, EncodeSpecularPower(SpecularPower), 1) * alpha;
  
  // (diffuse rgb, alpha)
  color1 = float4(ToGamma(DiffuseColor * diffuse), 1) * alpha;
  
  // (specular rgb, alpha)
  //color2 = float4(ToGamma(SpecularColor * specular), 1) * alpha;
  
  // (specular intensity, height, -, alpha)
  color2 = float4(ToGamma(SpecularColor * specular).g, height, 0, 1) * alpha;
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_3_0
#define PSTARGET ps_3_0
#else
#define VSTARGET vs_4_0
#define PSTARGET ps_4_0
#endif

technique
{
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
