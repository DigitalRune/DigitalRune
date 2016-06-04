//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file OcclusionCulling.fx
/// Implements occlusion culling using a hierarchical z-buffer (HZB).
/// See http://blog.selfshadow.com/publications/practical-visibility/
//
// Notes:
// Empty/Infinite shapes need to be handled by the caller.
// XNA EffectProcessor.DebugMode needs to be set to Optimize, otherwise the
// instruction limit of 512 instructions is exceeded.
//-----------------------------------------------------------------------------

#include "Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// Camera/light AABB.
float3 ClampAabbMinimum;    // The minimum of the camera/light AABB in world space.
float3 ClampAabbMaximum;    // The maximum of the camera/light AABB in world space.

// Camera settings.
float4x4 CameraViewProj;    // View-projection matrix of camera.
float CameraNear;           // Distance to near plane of camera.
float CameraFar;            // Distance to far plane of camera.
float3 CameraPosition;      // The camera position to calculate the LOD metric.
float NormalizationFactor;  // The normalization factor for the LOD metric.

// Light settings for shadow caster culling.
float4x4 LightViewProj;   // View-projection matrix of light. (Orthographic projection!)
float4x4 LightToCamera;   // Transformation from light clip space to camera clip space.

// Downsample: The size of the source texture.
// Query: The size of HZB level 0.
float2 HzbSize;

// Downsample: The size of the target texture.
// Query: The size of the render target storing the result.
float2 TargetSize;

// The size of the texture atlas storing the HZB levels.
float2 AtlasSize;

// Downsample: UV offset between texels in the source texture.
// Query: UV offset between texels in the HZB.
float2 TexelOffset;     // = 1 / AtlasSize
float2 HalfTexelOffset; // = 0.5 * TexelOffset

// The index of the max level in the HZB.
float MaxLevel;

texture HzbTexture;
sampler HzbSampler = sampler_state
{
  Texture = <HzbTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

texture LightHzb;
sampler LightHzbSampler = sampler_state
{
  Texture = <LightHzb>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

// ---- Debug Visualization
float DebugLevel;     // The HZB level to visualize.
float3 DebugMinimum;  // The AABB minimum of the object to visualize.
float3 DebugMaximum;  // The AABB maximum of the object to visualize.

static const float4 ColorOrange = float4(1, 0.5, 0, 1); // Used to show screen-space bounds of AABB.
static const float4 ColorGreen = float4(0, 1, 0, 1);    // Used to show HZB texels covered by AABB.


//-----------------------------------------------------------------------------
// Input, output
//-----------------------------------------------------------------------------

struct VSInputOccluder
{
  float4 Position : POSITION;
};

struct VSOutputOccluder
{
  float2 DepthProj : TEXCOORD;   // The clip space depth (z, w)
  float4 Position : SV_Position;
};

struct PSInputOccluder
{
  float2 DepthProj : TEXCOORD;
};


struct VSInputDrawQuad
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
};

struct VSOutputDrawQuad
{
  float2 TexCoord : TEXCOORD;
  float4 Position : SV_Position;
};


struct VSInputQuery
{
  float2 Pixel : POSITION0;       // The pixel address (x, y) of the result.
  float3 Minimum : TEXCOORD0;     // The minimum of the AABB in world space.
  float3 Maximum : TEXCOORD1;     // The maximum of the AABB in world space.
  float3 Position : TEXCOORD2;    // The position of the scene node in world space.
  float3 Scale : TEXCOORD3;       // The scale of the scene node.
  float MaxDistance : TEXCOORD4;  // The maximum distance for distance culling.
};

struct VSOutputQuery
{
  float3 Minimum : TEXCOORD0;
  float3 Maximum : TEXCOORD1;
  float2 Distances : TEXCOORD2;   // (viewNormalizedDistance, maxDistance)
  float4 Position : SV_Position;
};

struct PSInputQuery
{
  float3 Minimum : TEXCOORD0;
  float3 Maximum : TEXCOORD1;
  float2 Distances : TEXCOORD2;
};


struct VSInputVisualizeObject
{
  float4 Position : POSITION0;
  float2 TexCoord : TEXCOORD0;
};

struct VSOutputVisualizeObject
{
  float2 TexCoord : TEXCOORD0;
#if !SM4
  float One : TEXCOORD1;
#endif
  float4 Position : SV_Position;
};

struct PSInputVisualObject
{
  float2 TexCoord : TEXCOORD0;
#if !SM4
  float One : TEXCOORD1;
#endif
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutputOccluder VSOccluder(VSInputOccluder input)
{
  VSOutputOccluder output = (VSOutputOccluder)0;
  output.Position = mul(input.Position, CameraViewProj);
  output.DepthProj = output.Position.zw;
  return output;
}


float4 PSOccluder(PSInputOccluder input) : COLOR
{
  // DepthProj stores z and w in clip space.
  // Homogeneous divide gives non-linear depth in z-buffer.
  float depth = input.DepthProj.x / input.DepthProj.y;
  return float4(depth, depth, depth, 1);
}


VSOutputDrawQuad VSDrawQuad(VSInputDrawQuad input)
{
  VSOutputDrawQuad output = (VSOutputDrawQuad)0;
  output.Position = ScreenToProjection(input.Position, TargetSize);
  output.TexCoord = input.TexCoord;
  return output;
}


float4 PSDownsample(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get 2x2 samples and return the maximum depth.
  float2 o = HalfTexelOffset;
  float4 s0 = tex2Dlod(HzbSampler, float4(texCoord + float2(-o.x, -o.y), 0, 0));
  float4 s1 = tex2Dlod(HzbSampler, float4(texCoord + float2( o.x, -o.y), 0, 0));
  float4 s2 = tex2Dlod(HzbSampler, float4(texCoord + float2(-o.x,  o.y), 0, 0));
  float4 s3 = tex2Dlod(HzbSampler, float4(texCoord + float2( o.x,  o.y), 0, 0));
  return max(max(s0, s1), max(s2, s3));
}


float4 PSCopy(float2 texCoord : TEXCOORD0) : COLOR0
{
  return tex2Dlod(HzbSampler, float4(texCoord, 0, 0));
}


/// Clamps the specified AABB to camera/light AABB.
/// \param[inout] minimum   The minimum of the AABB in world space.
/// \param[inout] maximum   The maximum of the AABB in world space.
/// \remarks
/// Necessary for infinite objects, but also helps with very large objects!
void ClampAabb(inout float3 minimum, inout float3 maximum)
{
  minimum = max(minimum, min(maximum, ClampAabbMinimum));
  maximum = min(maximum, max(minimum, ClampAabbMaximum));
}


/// Gets the view-normalized distance (= LOD distance).
/// \param[in]  position  The position world space.
/// \param[in]  scale     The scale of the scene node.
/// \return     The view-normalized distance.
float GetViewNormalizedDistance(float3 position, float3 scale)
{
  float d = distance(CameraPosition, position);
  
  // Make distance independent of current FOV and scale.
  d *= NormalizationFactor;
  
  // We can assume that all scale factors are positive. But since abs() is considered
  // free on most GPUs, let's play it safe.
  scale = abs(scale);
  
  d /= max(max(scale.x, scale.y), scale.z);
  
  return d;
}


/// Determines minimum/maximum and if outside viewing frustum. (For perspective projections!)
/// \param[in]     p           The position in clip space.
/// \param[in]     isFirst     True if this is the first point.
/// \param[in,out] minimumNdc  The minimum position in normalized device coordinates (NDC).
/// \param[in,out] maximumNdc  The maximum position in normalized device coordinates (NDC).
/// \param[in,out] allMinOut   True if outside of the frustum (left, bottom, front).
/// \param[in,out] allMaxOut   True if outside of the frustum (right, top, back).
void GetMinMax(float4 p, bool isFirst, inout float3 minimumNdc, inout float3 maximumNdc, inout bool3 allMinOut, inout bool3 allMaxOut)
{
  // Frustum culling: Compare position with frustum clipping bounds.
  bool3 minOut = p.xyz < float3(-p.w, -p.w, 0);
  bool3 maxOut = p.xyz > float3(p.w, p.w, p.w);
  allMinOut = isFirst ? minOut : allMinOut && minOut;
  allMaxOut = isFirst ? maxOut : allMaxOut && maxOut;
  
  float3 pNdc;
  //if (p.w <= 0) // Point behind camera position? (Potential division-by-zero issues?)
  if (minOut.z)   // Point behind camera near?
  {
    // Position is behind camera. To avoid back projections assume that object
    // is visible and return point at near plane.
    pNdc = float3(0, 0, 0);
  }
  else
  {
    // Homogeneous divide.
    pNdc = p.xyz / p.w;
  }
  
  // Determine minimum and maximum in normalized device coordinates (NDC).
  minimumNdc = isFirst ? pNdc : min(minimumNdc, pNdc);
  maximumNdc = isFirst ? pNdc : max(maximumNdc, pNdc);
}


/// Gets the bounds of the specified AABB relative to the viewport. (For perspective projections!)
/// \param[in]  minimum   The minimum of the AABB in world space.
/// \param[in]  maximum   The maximum of the AABB in world space.
/// \param[in]  viewProj  The perspective view-projection matrix.
/// \param[out] visible   True if the AABBs is inside viewing frustum.
/// \param[out] bounds    The bounds relative to viewport: (left, top, right, bottom)
/// \param[out] minDepth  The min depth of the AABB.
void GetBounds(float3 minimum, float3 maximum, float4x4 viewProj, out bool visible, out float4 bounds, out float minDepth)
{
  bool3 allMinOut;
  bool3 allMaxOut;
  float3 minimumNdc;
  float3 maximumNdc;
  
  // Transform AABB minimum to clip space.
  float4 v0 = mul(float4(minimum, 1), viewProj);
  GetMinMax(v0, true, minimumNdc, maximumNdc, allMinOut, allMaxOut);
  
  // Transform AABB extent to clip space and calculate remaining 7 vertices.
  float3 extent = maximum - minimum;
  float4 e0 = extent.x * viewProj[0];
  float4 e1 = extent.y * viewProj[1];
  float4 e2 = extent.z * viewProj[2];
  
  float4 v1 = v0 + e0;
  GetMinMax(v1, false, minimumNdc, maximumNdc, allMinOut, allMaxOut);
  
  float4 v2 = v0 + e1;
  GetMinMax(v2, false, minimumNdc, maximumNdc, allMinOut, allMaxOut);
  
  float4 v3 = v0 + e2;
  GetMinMax(v3, false, minimumNdc, maximumNdc, allMinOut, allMaxOut);
  
  float4 v4 = v1 + e1;
  GetMinMax(v4, false, minimumNdc, maximumNdc, allMinOut, allMaxOut);
  
  float4 v5 = v1 + e2;
  GetMinMax(v5, false, minimumNdc, maximumNdc, allMinOut, allMaxOut);
  
  float4 v6 = v2 + e2;
  GetMinMax(v6, false, minimumNdc, maximumNdc, allMinOut, allMaxOut);
  
  float4 v7 = v4 + e2;
  GetMinMax(v7, false, minimumNdc, maximumNdc, allMinOut, allMaxOut);
  
  if (any(allMinOut || allMaxOut))
  {
    // AABB is outside the viewing frustum.
    visible = false;
    bounds = float4(0, 0, 0, 0);
    minDepth = 1;
  }
  else
  {
    // AABB is inside the viewing frustum.
    visible = true;
    
    // Convert from range [-1, 1] x [1, -1] to [0, 1] x [0, 1].
    bounds.xy = float2(0.5, -0.5) * float2(minimumNdc.x, maximumNdc.y) + float2(0.5, 0.5);
    bounds.zw = float2(0.5, -0.5) * float2(maximumNdc.x, minimumNdc.y) + float2(0.5, 0.5);
    minDepth = minimumNdc.z;
    
    // Clip bounds to viewport.
    bounds = saturate(bounds);
  }
}


/// Determines minimum/maximum and if outside viewing frustum. (For orthographic projections!)
/// \param[in]     p          The position in clip space.
/// \param[in]     isFirst     True if this is the first point.
/// \param[in,out] minimum    The minimum position in clip space.
/// \param[in,out] maximum    The maximum position in clip space.
/// \param[in,out] allMinOut  True if outside of the frustum (left, bottom, front).
/// \param[in,out] allMaxOut  True if outside of the frustum (right, top, back).
void GetMinMaxOrtho(float3 p, bool isFirst, inout float3 minimumClip, inout float3 maximumClip, inout bool3 allMinOut, inout bool3 allMaxOut)
{
  bool3 minOut = p < float3(-1, -1, 0);
  bool3 maxOut = p > float3(1, 1, 1);
  allMinOut = isFirst ? minOut : allMinOut && minOut;
  allMaxOut = isFirst ? maxOut : allMaxOut && maxOut;
  
  // Determine minimum and maximum in clip space.
  minimumClip = isFirst ? p : min(minimumClip, p);
  maximumClip = isFirst ? p : max(maximumClip, p);
}


/// Gets the bounds of the object in clip space and relative to the viewport. (For orthographic projections!)
/// \param[in]  minimum     The minimum of the shadow caster AABB in world space.
/// \param[in]  maximum     The maximum of the shadow caster AABB in world space.
/// \param[in]  viewProj    The light view-projection matrix. (Orthographic projection!)
/// \param[out] visible     True if the AABBs is inside viewing frustum.
/// \param[out] minimumClip The minimum of the shadow caster AABB in light clip space.
/// \param[out] maximumClip The maximum of the shadow caster AABB in light clip space.
/// \param[out] bounds      The bounds relative to viewport: (left, top, right, bottom)
void GetBoundsOrtho(float3 minimum, float3 maximum, float4x4 viewProj, out bool visible, out float3 minimumClip, out float3 maximumClip, out float4 bounds)
{
  // Note: w = 1, no homogeneous divide necessary for orthographic projections!
  bool3 allMinOut;
  bool3 allMaxOut;
  
  // Transform AABB minimum to clip space.
  float3 v0 = mul(float4(minimum, 1), viewProj).xyz;
  GetMinMaxOrtho(v0, true, minimumClip, maximumClip, allMinOut, allMaxOut);
  
  // Transform AABB extent to clip space and calculate remaining 7 vertices.
  float3 extent = maximum - minimum;
  float3 e0 = extent.x * viewProj[0].xyz;
  float3 e1 = extent.y * viewProj[1].xyz;
  float3 e2 = extent.z * viewProj[2].xyz;
  
  float3 v1 = v0 + e0;
  GetMinMaxOrtho(v1, false, minimumClip, maximumClip, allMinOut, allMaxOut);
  
  float3 v2 = v0 + e1;
  GetMinMaxOrtho(v2, false, minimumClip, maximumClip, allMinOut, allMaxOut);
  
  float3 v3 = v0 + e2;
  GetMinMaxOrtho(v3, false, minimumClip, maximumClip, allMinOut, allMaxOut);
  
  float3 v4 = v1 + e1;
  GetMinMaxOrtho(v4, false, minimumClip, maximumClip, allMinOut, allMaxOut);
  
  float3 v5 = v1 + e2;
  GetMinMaxOrtho(v5, false, minimumClip, maximumClip, allMinOut, allMaxOut);
  
  float3 v6 = v2 + e2;
  GetMinMaxOrtho(v6, false, minimumClip, maximumClip, allMinOut, allMaxOut);
  
  float3 v7 = v4 + e2;
  GetMinMaxOrtho(v7, false, minimumClip, maximumClip, allMinOut, allMaxOut);
  
  if (any(allMinOut || allMaxOut))
  {
    // AABB is outside the viewing frustum.
    visible = false;
    bounds = float4(0, 0, 0, 0);
  }
  else
  {
    // AABB is inside the viewing frustum.
    visible = true;
    
    // Convert from range [-1, 1] x [1, -1] to [0, 1] x [0, 1].
    bounds.xy = float2(0.5, -0.5) * float2(minimumClip.x, maximumClip.y) + float2(0.5, 0.5);
    bounds.zw = float2(0.5, -0.5) * float2(maximumClip.x, minimumClip.y) + float2(0.5, 0.5);
    
    // Clip bounds to viewport.
    bounds = saturate(bounds);
  }
}


/// Determines the optimal HZB level.
/// \param[in] bounds   The bounds relative to viewport: (left, top, right, bottom)
/// \param[in] hzbSize  The size of HZB level 0.
/// \return    The optimal HZB level.
float GetLevel(float4 bounds, float2 hzbSize)
{
  // Calculate HZB level.
  float4 boundsTexel = bounds * hzbSize.xyxy;
  float2 size = boundsTexel.zw - boundsTexel.xy;
  
  // For 2x2 HZB depth comparisons.
  //float level = ceil(log2(max(size.x, size.y)));
  
  // For 4x4 HZB depth comparisons.
  float level = ceil(log2(max(size.x, size.y)) - 1);
  
  // Texel footprint for the lower (finer-grained) level.
  float lowerLevel = max(level - 1, 0);
  float2 scale = exp2(-lowerLevel);
  float2 a = floor(boundsTexel.xy * scale);
  float2 b = ceil(boundsTexel.zw * scale);
  float2 dims = b - a;
  
  // Use the lower level if we only touch <= 2 texels in both dimensions.
  //if (dims.x <= 2 && dims.y <= 2)
  //  level = lowerLevel;
  
  // Use the lower level if we only touch <= 4 texels in both dimensions.
  if (dims.x <= 4 && dims.y <= 4)
    level = lowerLevel;
  
  return level;
}


/// Gets the LOD in the texture atlas.
/// \param[in]  level         The LOD index.
/// \param[out] levelScale    The scale of the LOD in the texture atlas.
/// \param[out] levelOffset   The Offset of the LOD in the texture atlas.
/// \param[out] levelClamp    The clamp region of the LOD: (left, top, right, bottom)
void GetLevelInAtlas(float level, out float2 levelScale, out float2 levelOffset, out float4 levelClamp)
{
  if (level <= 0)
  {
    // Level 0
    levelScale.x = 1;
    levelScale.y = 2.0 / 3.0;
    levelOffset.x = 0;
    levelOffset.y = 0;
  }
  else
  {
    // Levels 1 ... MaxLevel
    level = min(level, MaxLevel);
    float scale = exp2(-level);
    levelScale.x = scale;
    levelScale.y = scale * 2.0 / 3.0;
    levelOffset.x = 1 - 2 * scale;
    levelOffset.y = 2.0 / 3.0;
  }
  
  // Contract border by a half texel to avoid sampling outside the level.
  float2 halfTexelOffset = TexelOffset / 2;
  levelClamp.xy = levelOffset + halfTexelOffset;
  levelClamp.zw = levelOffset + levelScale - halfTexelOffset;
}


/// Gets the bounds of the texels that cover the object in the texture atlas.
/// \param[in]  bounds      The bounds of the object relative to viewport: (left, top, right, bottom)
/// \param[in]  level       The LOD index.
/// \param[out] levelClamp  The clamp region of the current LOD.
/// \return     The bounds (texture coordinates).
float4 GetBoundsInAtlas(float4 bounds, float level, out float4 levelClamp)
{
  // Get level in texture atlas.
  float2 levelScale;
  float2 levelOffset;
  GetLevelInAtlas(level, levelScale, levelOffset, levelClamp);
  
  // Get bounds in texture atlas.
  bounds = bounds * levelScale.xyxy + levelOffset.xyxy;
  
  // Expand bounds to cover whole texels.
  bounds.xy = floor(bounds.xy * AtlasSize) * TexelOffset;
  bounds.zw =  ceil(bounds.zw * AtlasSize) * TexelOffset;
  
  return bounds;
}


/// Gets the maximum value of a 4-component vector.
/// \param[in] v  The vector.
/// \return    The maximum value.
float max4(float4 v)
{
  // Note: Xbox 360 has max4 intrinsic!
  return max(max(v.x, v.y), max(v.z, v.w));
}


/// Gets the maximum depth from the HZB.
/// \param[in] hzbSampler  The HZB texture sampler.
/// \param[in] bounds      The bounds in the HZB texture.
/// \return    The maximum depth.
float GetMaxDepth(sampler hzbSampler, float4 bounds)
{
  float level = GetLevel(bounds, HzbSize);
  
  // ----- Fixed 2x2 HZB depth comparisons (using mipmaps).
  //float4 samples;
  //samples.x = tex2Dlod(HzbSampler, float4(bounds.xy, 0, level)).x;
  //samples.y = tex2Dlod(HzbSampler, float4(bounds.zy, 0, level)).x;
  //samples.z = tex2Dlod(HzbSampler, float4(bounds.xw, 0, level)).x;
  //samples.w = tex2Dlod(HzbSampler, float4(bounds.zw, 0, level)).x;
  //float maxDepth = max4(samples);
  
  // ----- Up to 4x4 HZB depth comparisons (using texture atlas).
  float4 levelClamp;
  bounds = GetBoundsInAtlas(bounds, level, levelClamp);
  float maxDepth = 0;
  for (float v = bounds.y + HalfTexelOffset.y; v < bounds.w; v += TexelOffset.y)
  {
    for (float u = bounds.x + HalfTexelOffset.x; u < bounds.z; u += TexelOffset.x)
    {
      float2 t = float2(u, v);
      
      // Clamp texture coordinates to avoid sampling outside the level.
      t = clamp(t, levelClamp.xy, levelClamp.zw);
      
      float depth = tex2Dlod(hzbSampler, float4(t, 0, 0)).x;
      maxDepth = max(maxDepth, depth);
    }
  }
  
  return maxDepth;
}


VSOutputQuery VSQuery(VSInputQuery input)
{
  VSOutputQuery output = (VSOutputQuery)0;
  
  // Convert pixel position to clip space.
  float2 position = input.Pixel;
  
#if SM4
  // DirectX 10+: Add half-pixel offset.
  position += float2(0.5, 0.5);
#endif
  
  // Transform into the range [0, 1] x [0, 1].
  position /= TargetSize;
  // Transform into the range [0, 2] x [-2, 0]
  position *= float2(2, -2);
  // Transform into the range [-1, 1] x [1, -1].
  position -= float2(1, -1);
  
  output.Position = float4(position, 0, 1);
  
  // Clamp AABB to reasonable size.
  ClampAabb(input.Minimum, input.Maximum);
  output.Minimum = input.Minimum;
  output.Maximum = input.Maximum;
  
  // Pass view-normalized distance and max distance for distance culling.
  float viewNormalizedDistance = GetViewNormalizedDistance(input.Position, input.Scale);
  output.Distances.x = viewNormalizedDistance;
  output.Distances.y = (input.MaxDistance > 0)
                       ? input.MaxDistance        // Distance culling enabled.
                       : viewNormalizedDistance;  // Distance culling disabled.
  
  return output;
}


float4 PSQuery(PSInputQuery input) : COLOR0
{
  float3 minimum = input.Minimum;
  float3 maximum = input.Maximum;
  
  // Distance culling.
  float viewNormalizedDistance = input.Distances.x;
  float maxDistance = input.Distances.y;
  bool visible = (viewNormalizedDistance <= maxDistance);
  
  if (!visible)
    return -viewNormalizedDistance;
  
  // Project AABB and get bounds relative to viewport.
  float4 bounds;    // (left, top, right, bottom) in the range [0, 1].
  float minDepth;   // The z-buffer depth in the range [0, 1].
  GetBounds(minimum, maximum, CameraViewProj, visible, bounds, minDepth);
  
  if (!visible)
    return -viewNormalizedDistance; // AABB is outside viewing frustum.
  
  float maxDepth = GetMaxDepth(HzbSampler, bounds);
  visible = (minDepth <= maxDepth);
  
  if (visible)
    return viewNormalizedDistance;
  else
    return -viewNormalizedDistance;
}


// Shadow caster culling:
// - Cull shadow caster in light HZB.
// - Determine shadow volume extent.
// - Cull shadow volume in camera HZB.
float4 PSQueryShadowCaster(PSInputQuery input, uniform const bool progressiveCulling) : COLOR0
{
  float3 minimum = input.Minimum;
  float3 maximum = input.Maximum;
  
  // ----- Pass 0: Distance culling.
  float viewNormalizedDistance = input.Distances.x;
  float maxDistance = input.Distances.y;
  bool visible = (viewNormalizedDistance <= maxDistance);
  
  if (!visible)
    return -viewNormalizedDistance;
  
  // ----- Pass 1: Frustum/Occlusion test in light HZB.
  float3 minimumClip, maximumClip;
  float4 bounds;
  GetBoundsOrtho(minimum, maximum, LightViewProj, visible, minimumClip, maximumClip, bounds);
  
  if (!visible)
    return -viewNormalizedDistance; // Shadow caster is outside light frustum.
  
  float maxDepth = GetMaxDepth(LightHzbSampler, bounds);
  visible = (minimumClip.z <= maxDepth);
  
  if (!visible)
    return -viewNormalizedDistance; // Shadow caster is occluded in light HZB.
  
  // Estimate extent of the shadow volume:
  if (!progressiveCulling || maximumClip.z >= maxDepth)
  {
    // Conservative shadow caster culling (maxDepth obviously wrong):
    // Assume that shadow volume extends to the edge of the light space. (Safe.)
    maximumClip.z = 1;
  }
  else
  {
    // Progressive shadow caster culling:
    // The light HZB is used to get the depth of the shadow volume.
    maximumClip.z = maxDepth;
  }
  
  // ----- Pass 2: Frustum/Occlusion test in camera HZB.
  // Transform shadow volume from light clip space to camera clip space
  // and get bounds relative to viewport.
  float minDepth;
  GetBounds(minimumClip, maximumClip, LightToCamera, visible, bounds, minDepth);
  
  if (!visible)
    return -viewNormalizedDistance; // Shadow volume is outside camera frustum.
  
  maxDepth = GetMaxDepth(HzbSampler, bounds);
  visible = (minDepth <= maxDepth);
  
  if (visible)
    return viewNormalizedDistance;
  else
    return -viewNormalizedDistance;
}


float4 PSQueryShadowCaster0(PSInputQuery input) : COLOR0 { return PSQueryShadowCaster(input, false); }
float4 PSQueryShadowCaster1(PSInputQuery input) : COLOR0 { return PSQueryShadowCaster(input, true); }


/// Gets the linear depth from the non-linear z-buffer value.
/// \param[in] z  The z-buffer value.
/// \return The linear depth [0, 1] were 0 is at near and 1 at far plane.
float GetLinearDepth(float z)
{
  float n = CameraNear;
  float f = CameraFar;
  
  return n / (z * n + (1 - z) * f);
}


float4 VisualizeCameraHzb(float2 texCoord : TEXCOORD0, float level)
{
  // Get level in texture atlas.
  float2 levelScale;
  float2 levelOffset;
  float4 levelClamp;
  GetLevelInAtlas(level, levelScale, levelOffset, levelClamp);
  
  // Correct texture coordinates.
  texCoord = texCoord * levelScale + levelOffset;
  
  // Clamp texture coordinates to avoid sampling outside the level.
  texCoord = clamp(texCoord, levelClamp.xy, levelClamp.zw);
  
  // Sample HZB.
  float z = tex2Dlod(HzbSampler, float4(texCoord, 0, 0)).x;
  z = GetLinearDepth(z);
  clip(0.9999 - z);
  
  // Apply gamma curve, otherwise depth differences are hardly noticable.
  z = sqrt(z);
  
  return float4(z, z, z, 1);
}


float4 PSVisualizeCameraHzb(float2 texCoord : TEXCOORD0) : COLOR0
{
  return VisualizeCameraHzb(texCoord, DebugLevel);
}


VSOutputVisualizeObject VSVisualizeObject(VSInputVisualizeObject input)
{
  VSOutputVisualizeObject output = (VSOutputVisualizeObject)0;
  output.Position = ScreenToProjection(input.Position, TargetSize);
  output.TexCoord = input.TexCoord;
#if !SM4
  output.One = 1; // Workaround for FXC bug.
#endif
  return output;
}


float4 PSVisualizeObject(PSInputVisualObject input) : COLOR0
{
  float2 texCoord = input.TexCoord;
#if !SM4
  // Workaround for FXC bug (XNA only):
  // Multiply shader constants with 1 from vertex shader. Otherwise, the FXC tries
  // to pull the entire code into the preshader. Preshaders can't be disabled in
  // XNA and a bug in the FXC causes an error.
  float3 minimum = DebugMinimum * input.One;
  float3 maximum = DebugMaximum * input.One;
#else
  float3 minimum = DebugMinimum;
  float3 maximum = DebugMaximum;
#endif
  
  // Clamp AABB to reasonable size.
  ClampAabb(minimum, maximum);
  
  // Project AABB and get bounds relative to viewport.
  bool visible;     // Result of frustum culling.
  float4 bounds;    // (left, top, right, bottom) in the range [0, 1].
  float minDepth;   // The z-buffer depth in the range [0, 1].
  GetBounds(minimum, maximum, CameraViewProj, visible, bounds, minDepth);
  
  // Visualize query.
  float level = GetLevel(bounds, HzbSize);
  
  float4 levelClamp;
  float4 boundsAtlas = GetBoundsInAtlas(bounds, level, levelClamp);
  
  float2 levelScale;
  float2 levelOffset;
  GetLevelInAtlas(level, levelScale, levelOffset, levelClamp);
  float2 texCoordAtlas = texCoord * levelScale + levelOffset;
  
  if (visible && all(boundsAtlas.xy <= texCoordAtlas) && all(texCoordAtlas <= boundsAtlas.zw))
  {
    if (all(bounds.xy <= texCoord) && all(texCoord <= bounds.zw))
    {
      // Screen-space bounds of AABB.
      return ColorOrange;
    }
    else
    {
      // HZB texels covered by AABB.
      return ColorGreen;
    }
  }
  else
  {
    // Visualize HZB at query level.
    return VisualizeCameraHzb(texCoord, level);
  }
}


float4 VisualizeLightHzb(float2 texCoord : TEXCOORD0, float level)
{
  // Get level in texture atlas.
  float2 levelScale;
  float2 levelOffset;
  float4 levelClamp;
  GetLevelInAtlas(level, levelScale, levelOffset, levelClamp);
  
  // Correct texture coordinates.
  texCoord = texCoord * levelScale + levelOffset;
  
  // Clamp texture coordinates to avoid sampling outside the level.
  texCoord = clamp(texCoord, levelClamp.xy, levelClamp.zw);
  
  // Sample HZB.
  float z = tex2Dlod(LightHzbSampler, float4(texCoord, 0, 0)).x;
  //clip(0.9999 - z);
  
  // Apply gamma curve, otherwise depth differences are hardly noticable.
  z = sqrt(z);
  
  return float4(z, z, z, 1);
}


float4 PSVisualizeLightHzb(float2 texCoord : TEXCOORD0) : COLOR0
{
  return VisualizeLightHzb(texCoord, DebugLevel);
}


float4 PSVisualizeShadowCaster(PSInputVisualObject input) : COLOR0
{
  float2 texCoord = input.TexCoord;
#if !SM4
  // Workaround for FXC bug (XNA only):
  // Multiply shader constants with 1 from vertex shader. Otherwise, the FXC tries
  // to pull the entire code into the preshader. Preshaders can't be disabled in
  // XNA and a bug in the FXC causes an error.
  float3 minimum = DebugMinimum * input.One;
  float3 maximum = DebugMaximum * input.One;
#else
  float3 minimum = DebugMinimum;
  float3 maximum = DebugMaximum;
#endif
  
  // Clamp AABB to reasonable size.
  ClampAabb(minimum, maximum);
  
  // Get bounds in light HZB.
  bool visible;
  float3 minimumClip, maximumClip;
  float4 bounds;
  GetBoundsOrtho(minimum, maximum, LightViewProj, visible, minimumClip, maximumClip, bounds);
  
  // Visualize query.
  float level = GetLevel(bounds, HzbSize);
  
  float4 levelClamp;
  float4 boundsAtlas = GetBoundsInAtlas(bounds, level, levelClamp);
  
  float2 levelScale;
  float2 levelOffset;
  GetLevelInAtlas(level, levelScale, levelOffset, levelClamp);
  float2 texCoordAtlas = texCoord * levelScale + levelOffset;
  
  if (visible && all(boundsAtlas.xy <= texCoordAtlas) && all(texCoordAtlas <= boundsAtlas.zw))
  {
    if (all(bounds.xy <= texCoord) && all(texCoord <= bounds.zw))
    {
      // Screen-space bounds of AABB.
      return ColorOrange;
    }
    else
    {
      // HZB texels covered by AABB.
      return ColorGreen;
    }
  }
  else
  {
    // Visualize HZB at query level.
    return VisualizeLightHzb(texCoord, level);
  }
}


float4 PSVisualizeShadowVolume(PSInputVisualObject input) : COLOR0
{
  float2 texCoord = input.TexCoord;
#if !SM4
  // Workaround for FXC bug (XNA only):
  // Multiply shader constants with 1 from vertex shader. Otherwise, the FXC tries
  // to pull the entire code into the preshader. Preshaders can't be disabled in
  // XNA and a bug in the FXC causes an error.
  float3 minimum = DebugMinimum * input.One;
  float3 maximum = DebugMaximum * input.One;
#else
  float3 minimum = DebugMinimum;
  float3 maximum = DebugMaximum;
#endif
  
  // Clamp AABB to reasonable size.
  ClampAabb(minimum, maximum);
  
  // Get bounds of shadow caster in light HZB.
  bool visible;
  float3 minimumClip, maximumClip;
  float4 bounds;
  GetBoundsOrtho(minimum, maximum, LightViewProj, visible, minimumClip, maximumClip, bounds);
  
  // Get bounds of shadow volume.
  float maxDepth = GetMaxDepth(LightHzbSampler, bounds);
  if (maximumClip.z >= maxDepth)
    maximumClip.z = 1; // See comments in PSQueryShadowCaster.
  else
    maximumClip.z = maxDepth;
  
  // Get bounds of shadow volume in camera HZB.
  bool visible2;
  float minDepth;
  GetBounds(minimumClip, maximumClip, LightToCamera, visible2, bounds, minDepth);
  visible = visible && visible2;
  
  // Visualize query.
  float level = GetLevel(bounds, HzbSize);
  
  float4 levelClamp;
  float4 boundsAtlas = GetBoundsInAtlas(bounds, level, levelClamp);
  
  float2 levelScale;
  float2 levelOffset;
  GetLevelInAtlas(level, levelScale, levelOffset, levelClamp);
  float2 texCoordAtlas = texCoord * levelScale + levelOffset;
  
  if (visible && all(boundsAtlas.xy <= texCoordAtlas) && all(texCoordAtlas <= boundsAtlas.zw))
  {
    if (all(bounds.xy <= texCoord) && all(texCoord <= bounds.zw))
    {
      // Screen-space bounds of AABB.
      return ColorOrange;
    }
    else
    {
      // HZB texels covered by AABB.
      return ColorGreen;
    }
  }
  else
  {
    // Visualize HZB at query level.
    return VisualizeCameraHzb(texCoord, level);
  }
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


technique Occluder
{
  pass
  {
    VertexShader = compile VSTARGET VSOccluder();
    PixelShader = compile PSTARGET PSOccluder();
  }
}


technique Downsample
{
  pass
  {
    VertexShader = compile VSTARGET VSDrawQuad();
    PixelShader = compile PSTARGET PSDownsample();
  }
}


technique Copy
{
  pass
  {
    VertexShader = compile VSTARGET VSDrawQuad();
    PixelShader = compile PSTARGET PSCopy();
  }
}


technique Query
{
  pass Object
  {
    VertexShader = compile VSTARGET VSQuery();
    PixelShader = compile PSTARGET PSQuery();
  }
  pass ShadowCaster_Conservative
  {
    VertexShader = compile VSTARGET VSQuery();
    PixelShader = compile PSTARGET PSQueryShadowCaster0();
  }
  pass ShadowCaster_Progressive
  {
    VertexShader = compile VSTARGET VSQuery();
    PixelShader = compile PSTARGET PSQueryShadowCaster1();
  }
}


technique Visualize
{
  pass CameraHzb
  {
    VertexShader = compile VSTARGET VSDrawQuad();
    PixelShader = compile PSTARGET PSVisualizeCameraHzb();
  }
  pass Object
  {
    VertexShader = compile VSTARGET VSVisualizeObject();
    PixelShader = compile PSTARGET PSVisualizeObject();
  }
  pass LightHzb
  {
    VertexShader = compile VSTARGET VSDrawQuad();
    PixelShader = compile PSTARGET PSVisualizeLightHzb();
  }
  pass ShadowCaster
  {
    VertexShader = compile VSTARGET VSVisualizeObject();
    PixelShader = compile PSTARGET PSVisualizeShadowCaster();
  }
  pass ShadowVolume
  {
    VertexShader = compile VSTARGET VSVisualizeObject();
    PixelShader = compile PSTARGET PSVisualizeShadowVolume();
  }
}
