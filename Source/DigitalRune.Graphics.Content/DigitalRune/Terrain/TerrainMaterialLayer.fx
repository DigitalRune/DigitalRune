//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file TerrainMaterialLayer.fx
/// Renders a tiling texture into the terrain detail clipmap.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"


// Very important: Add a negative LOD bias.
// Crisp noisy terrain materials are much better than smooth materials!
// When the terrain is rendered, the clipmaps will be smoothed again.
#define MATERIAL_MIPLODBIAS -1

// Warning: gradient-based operations must be moved out of flow control...
// Is caused by GetNormalDxt5nm, which is called with manually computed gradients!?
#pragma warning(disable : 4121)


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// ----- General Constants
// The viewport size in pixels.
float2 ViewportSize;
float3 LodCameraPosition;

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

float TriplanarTightening = 0.5;

float TintStrength;
texture TintTexture;
sampler TintSampler = sampler_state
{
  Texture = <TintTexture>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = ANISOTROPIC;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
};

float BlendThreshold;
float BlendRange;
float BlendHeightInfluence;
float BlendNoiseInfluence;
float4 BlendTextureChannelMask;
texture BlendTexture;
sampler BlendSampler = sampler_state
{
  Texture = <BlendTexture>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = ANISOTROPIC;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
};

float NoiseTileSize;
texture NoiseMap;
sampler NoiseMapSampler = sampler_state
{
  Texture = <NoiseMap>;
  AddressU  = WRAP;
  AddressV  = WRAP;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = NONE;
};

float TerrainHeightMin;
float TerrainHeightMax;
float TerrainHeightBlendRange;

// Terrain slope limits in radians.
float TerrainSlopeMin;
float TerrainSlopeMax;
float TerrainSlopeBlendRange;

// ----- Terrain parameters.
// Tile origin and size
float2 TerrainTileOrigin;
float2 TerrainTileSize;
// Tile height texture and size in texels
float2 TerrainTileHeightTextureSize;
texture TerrainTileHeightTexture;
sampler TerrainTileHeightSampler = sampler_state
{
  Texture = <TerrainTileHeightTexture>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = POINT;    // TODO: We could use LINEAR here for cases where CellSizes differ?
  MagFilter = POINT;
  MipFilter = POINT;
};
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


//-----------------------------------------------------------------------------
// Shaders
//-----------------------------------------------------------------------------

void VS(in float2 inPositionScreen : POSITION0,     // Screen space xy position.
        in float2 inPositionWorldXZ : TEXCOORD0,    // World space xz position
        out float2 texCoordTerrain : TEXCOORD0,     // Texture coordinates for terrain tile
        out float2 texCoordNoise : TEXCOORD1,       // Texture coordinates for noise texture
        out float2 positionWorldXZ : TEXCOORD2,     // World space xz position
        out float4 positionProj : SV_Position)      // Projection space position
{
  positionProj.xy = ScreenToProjection(inPositionScreen, ViewportSize);
  positionProj.z = 0;
  positionProj.w = 1;
  
  // Convert world space position (X, Z) to texture coordinates.
  texCoordTerrain = (inPositionWorldXZ - TerrainTileOrigin) / TerrainTileSize;
  
  // The terrain tile origin and the height texture origin differ by half a texel because the
  // tile starts in the center of the first texel.
  texCoordTerrain += 0.5 / TerrainTileHeightTextureSize;
  
  texCoordNoise = inPositionWorldXZ / NoiseTileSize;
  positionWorldXZ = inPositionWorldXZ;
}


void PS(in float2 texCoordTerrain : TEXCOORD0,
        in float2 texCoordNoise : TEXCOORD1,
        in float2 positionWorldXZ : TEXCOORD2,
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
  float epsilon = 0.0001f;
  
  //color0 = float4(0.5, 0.5, 0.4, 1);
  //color1 = float4(1, 1, 1, 1);
  //color2 = float4(1, 1, 1, 1);
  //return;
  
  // ----- Sample terrain tile textures
  // Sample linear without mipmaps. We could do manual trilinear sampling but this did not look better.
  // TODO: Without trilinear sampling this might be unstable when the detail clipmap resolution is super low.
  // (Note: Terrain.fxh makes sure to always sample correct texel centers of each clipmap level
  // because mipmaps are usually computed using a special 3x3 filter. We ignore this here.)
  float terrainHeight = SampleLinear(TerrainTileHeightSampler, texCoordTerrain, TerrainTileHeightTextureSize).r;
  float3 terrainNormal = GetNormal(TerrainTileNormalSampler, texCoordTerrain).rgb;
  
  // Convert the green-up normal map normal to world space.
  terrainNormal = float3(terrainNormal.x, terrainNormal.z, -terrainNormal.y);
  
  float3 tintMap = FromGamma(tex2D(TintSampler, texCoordTerrain).rgb);
  tintMap = lerp(1, tintMap, TintStrength);
  
  float blendWeight = dot(BlendTextureChannelMask, tex2D(BlendSampler, texCoordTerrain));
  
  // ----- Sample noise
  // Two octaves hardcoded.
  float4 noiseValue = tex2D(NoiseMapSampler, texCoordNoise) * 2 - 1;
  noiseValue += 0.4 * (tex2D(NoiseMapSampler, texCoordNoise * 7) * 2 - 1);
  
  // ----- Sample detail texture with triplanar mapping
  float3 w = float3(0, 1, 0);
  if (TriplanarTightening >= 0)
  {
    // Weights for triplanar mapping:
    w = abs(terrainNormal);
    // Tighten up the blending zone
    w = (w - TriplanarTightening);
    w = max(w, 0);
    // Force weights to sum to 1.0 (very important!)
    w /= (w.x + w.y + w.z);
  }
  
  float3 position = float3(positionWorldXZ.x, terrainHeight, positionWorldXZ.y);
  float2 texCoordXZ = position.xz / TileSize;
  float2 texCoordYZ = position.yz / TileSize;
  float2 texCoordXY = position.xy / TileSize;
  float2 ddxXZ = ddx(texCoordXZ);
  float2 ddyXZ = ddy(texCoordXZ);
  float2 ddxYZ = ddx(texCoordYZ);
  float2 ddyYZ = ddy(texCoordYZ);
  float2 ddxXY = ddx(texCoordXY);
  float2 ddyXY = ddy(texCoordXY);
  
  float4 diffuseMap = 0;
  float4 specularMap = 0;
  float3 normal = 0;
  float height = 0;
  // TODO: Test if branching performance is better than without any branches.
  [branch]
  if (w.x > 0.0001f)
  {
    diffuseMap += tex2Dgrad(DiffuseSampler, texCoordYZ, ddxYZ, ddyYZ) * w.x;
    specularMap += tex2Dgrad(SpecularSampler, texCoordYZ, ddxYZ, ddyYZ) * w.x;
    normal += GetNormalDxt5nm(NormalSampler, texCoordYZ, ddxYZ, ddyYZ) * w.x;
    height += tex2Dgrad(HeightSampler, texCoordYZ, ddxYZ, ddyYZ).x * w.x;
  }
  [branch]
  if (w.y > 0.0001f)
  {
    diffuseMap += tex2Dgrad(DiffuseSampler, texCoordXZ, ddxXZ, ddyXZ) * w.y;
    specularMap += tex2Dgrad(SpecularSampler, texCoordXZ, ddxXZ, ddyXZ) * w.y;
    normal += GetNormalDxt5nm(NormalSampler, texCoordXZ, ddxXZ, ddyXZ) * w.y;
    height += tex2Dgrad(HeightSampler, texCoordXZ, ddxXZ, ddyXZ).x * w.y;
  }
  [branch]
  if (w.z > 0.0001f)
  {
    diffuseMap += tex2Dgrad(DiffuseSampler, texCoordXY, ddxXY, ddyXY) * w.z;
    specularMap += tex2Dgrad(SpecularSampler, texCoordXY, ddxXY, ddyXY) * w.z;
    normal += GetNormalDxt5nm(NormalSampler, texCoordXY, ddxXY, ddyXY) * w.z;
    height += tex2Dgrad(HeightSampler, texCoordXY, ddxXY, ddyXY).x * w.z;
  }
  
  float unscaledHeight = height;
  height = height * HeightTextureScale + HeightTextureBias;
    
  // Undo premultiplied alpha:
  diffuseMap.rgb /= diffuseMap.a;
  
  // Convert from gamma to linear space.
  float3 diffuse = FromGamma(diffuseMap.rgb);
  float3 specular = FromGamma(specularMap.rgb);
  
  // ----- Compute alpha
  // Add noise to blend threshold
  float blendThreshold = BlendThreshold + noiseValue.r * BlendNoiseInfluence;
  // Bring height to [-1, 1] range and add it to the blend weight.
  blendWeight +=  (unscaledHeight * 2 - 1) * BlendHeightInfluence;
  // Do blending only in the given BlendRange around the threshold.
  float blendStart = saturate(blendThreshold - BlendRange / 2.0);
  float blendEnd = saturate(blendThreshold + BlendRange / 2.0);
  blendWeight = saturate((blendWeight - blendStart) / (blendEnd - blendStart + epsilon));
  
  float fadeInWeight = saturate((TerrainClipmapLevel + 1.0 - FadeInStart) / (FadeInEnd + 1.0 - FadeInStart) + dummy);
  float fadeOutWeight = 1 - saturate((TerrainClipmapLevel + 1.0 - FadeOutStart) / (FadeOutEnd + 1.0 - FadeOutStart) + dummy);
  
  float heightFadeInWeight = saturate((terrainHeight - (TerrainHeightMin - TerrainHeightBlendRange / 2.0))
                                      / (TerrainHeightBlendRange + epsilon));
  float heightFadeOutWeight = 1 - saturate((terrainHeight - (TerrainHeightMax - TerrainHeightBlendRange / 2.0))
                                           / (TerrainHeightBlendRange + epsilon));
  float slopeFadeInWeight = saturate((acos(terrainNormal.y) - (TerrainSlopeMin - TerrainSlopeBlendRange / 2.0))
                                     / (TerrainSlopeBlendRange + epsilon));
  float slopeFadeOutWeight = 1 - saturate((acos(terrainNormal.y) - (TerrainSlopeMax - TerrainSlopeBlendRange / 2.0))
                                          / (TerrainSlopeBlendRange + epsilon));
  
  float alpha = Alpha * diffuseMap.a * blendWeight
                * fadeInWeight * fadeOutWeight
                * heightFadeInWeight * heightFadeOutWeight
                * slopeFadeInWeight * slopeFadeOutWeight;
  
  // ----- Convert normal from tangent to world space.
  float3 tangent = normalize(cross(terrainNormal, float3(0, 0, 1)));
  float3 binormal = cross(tangent, terrainNormal);
  normal = terrainNormal * normal.z + tangent * normal.x - binormal * normal.y;
  
  // Convert normal from [-1, 1] range to [0, 1] range.
  normal = (normal + 1.0) * 128.0 / 255.0;
  
  // (normal.x, normal.z, specular exponent, hole alpha)
  color0 = float4(normal.x, normal.z, EncodeSpecularPower(SpecularPower), 1) * alpha;
  
  // (diffuse rgb, alpha)
  color1 = float4(ToGamma(DiffuseColor * diffuse * tintMap), 1) * alpha;
  
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
