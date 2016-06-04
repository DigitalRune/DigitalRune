//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Terrain.fxh
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_TERRAIN_FXH
#define DIGITALRUNE_TERRAIN_FXH

#ifndef DIGITALRUNE_COMMON_FXH
#error "Common.fxh required. Please include Common.fxh before including Terrain.fxh."
#endif

#ifndef DIGITALRUNE_ENCODING_FXH
#error "Encoding.fxh required. Please include Encoding.fxh before including Terrain.fxh."
#endif

#ifndef MAX_ANISOTROPY
#define MAX_ANISOTROPY 8
#endif


//float4 Debug : DEBUG;


// Convert position to clipmap texture coordinates (but not texture atlas coordinates!)
float2 GetClipmapTexCoords(float2 positionXZ, float2 origin, float cellsPerLevel, float cellSize)
{
  float levelSize = cellsPerLevel * cellSize;
  return (positionXZ - origin) / levelSize;
}


// Converts a texture coordinate for an individual texture to the coordinate
// inside the clipmap texture atlas.
float2 GetAtlasTexCoords(float2 texCoord, int level, int numberOfLevels, int numberOfColumns)
{
  // We have to clamp texture coordinates manually.
  // (With a proper LOD bias, we should never sample outside [0, 1] and this line is only relevant
  // for the last level.)
  texCoord = clamp(texCoord, 0.0001, 0.9999);
  
#if SM4
  uint numberOfRows = ((uint)numberOfLevels - 1) / (uint)numberOfColumns + 1;
  uint row = (uint)((uint)level / (uint)numberOfColumns);
  uint column = (uint)level % (uint)numberOfColumns;
#else
  int numberOfRows = (numberOfLevels - 1.0f) / numberOfColumns + 1;
  int row = (int)(level / (float)numberOfColumns);
  // int column = level % numberOfColumns;  // Integer modulo creates garbage on DX9+AMD.
  float column = level - row * numberOfColumns;
#endif
  
  texCoord.x = (texCoord.x + column) / numberOfColumns;
  texCoord.y = (texCoord.y + row) / numberOfRows;
  return texCoord;
}


// Samples one level of the base clipmap texture using manual bilinear filtering.
float4 SampleBaseClipmap(sampler baseClipmapSampler, float2 origins[9], float cellSize, int cellsPerLevel,
                         int numberOfLevels, int numberOfColumns, float2 positionXZ, int level)
{
  // The vertex positions are in the centers of the level 0 texels. Since we might want to sample 
  // in the texel center of a different level, we first move the position back to the texel corner.
  positionXZ -= cellSize / 2;

  // Convert position to texture coordinates.
  float2 texCoord = GetClipmapTexCoords(positionXZ, origins[level], cellsPerLevel, cellSize * exp2(level));
  
  // The size of one texel in texture coordinates (in any single texture of the clipmap).
  float texelSize = 1.0 / cellsPerLevel;
  
  // Add a half texel to sample texel centers.
  // We do not sample exactly in the middle because then the bilinear sampling
  // would randomly sample the current and the next or the current and the
  // previous neighbor texel because of numerical errors in the texCoord.
  texCoord = texCoord + 0.49 * texelSize;
  
  float4 sample = float4(0, 0, 0, 0);
  
  float2 tc;
  
  // Simple point sampling - results in non-smooth terrain. Because of the
  // diamond mesh tesselation pattern or when a clipmap mesh level samples the texture
  // of the next level, some vertices sample between texels...
  //tc = GetAtlasTexCoords(texCoord, level, numberOfLevels, numberOfColumns);
  //sample = tex2Dlod(baseClipmapSampler, float4(tc, 0, 0));
  //return sample;
  
  // Manual bilinear interpolation.
  texCoord.xy -= 0.5 * texelSize;
  tc = GetAtlasTexCoords(texCoord, level, numberOfLevels, numberOfColumns);
  float4 s00 = tex2Dlod(baseClipmapSampler, float4(tc, 0, 0));
  
  tc = GetAtlasTexCoords(texCoord + float2(texelSize, 0), level, numberOfLevels, numberOfColumns);
  float4 s10 = tex2Dlod(baseClipmapSampler, float4(tc, 0, 0));
  
  tc = GetAtlasTexCoords(texCoord + float2(0, texelSize), level, numberOfLevels, numberOfColumns);
  float4 s01 = tex2Dlod(baseClipmapSampler, float4(tc, 0, 0));
  
  tc = GetAtlasTexCoords(texCoord + float2(texelSize, texelSize), level, numberOfLevels, numberOfColumns);
  float4 s11 = tex2Dlod(baseClipmapSampler, float4(tc, 0, 0));
  
  float2 lerps = frac(cellsPerLevel * texCoord.xy);
  sample = lerp(lerp(s00, s10, lerps.x), lerp(s01, s11, lerps.x), lerps.y);
  
  return sample;
}


// Main method for use in vertex shader.
void ComputeTerrainGeometry(sampler baseClipmapSampler, float2 origins[9], float cellSizeLevel0,
                            int cellsPerLevel, int numberOfLevels, int numberOfColumns, float levelBias,
                            float holeThreshold, float3 cameraPosition, float3 holePosition,
                            inout float3 position, out float3 normal, out float clod)
{
  // Position.y stores the clipmap level of each vertex.
  int lod = (int)position.y;
  position.y = 0;
  
  // The positions are relative to a cell size of 1. --> Scale them.
  position.xz *= cellSizeLevel0;
  
  // Clamp camera position to grid.
  float2 cameraPositionXZ = cameraPosition.xz;
  float cellSize = exp2(lod) * cellSizeLevel0;
  cameraPositionXZ = floor(cameraPositionXZ / cellSize) * cellSize;
  
  // Center geo-clipmap mesh at snapped camera position.
  position.xz += cameraPositionXZ;
  
  // Distance of vertex to unclamped(!) camera position in 2D.
  float2 cameraToVertex = position.xz - cameraPosition.xz;
  
  // Compute parameter which is 0 on the camera position and 1 at the border of the level 0 grid.
  float2 r = abs(cameraToVertex / (cellSizeLevel0 * cellsPerLevel / 2.0));
  
  // Continuous lod parameter.
  // Get max component.
  clod = max(r.x, r.y);
  
  // Limit min to avoid log(0).
  clod = max(clod, 0.1);   // 0.1 = 2^(-3.32)  --> ~-3 is the min lod level (which limits the bias).
  
  // Convert from "distance" to lod/mip level.
  clod = log2(clod) + 1;
  // Now, the parameter goes from ~-3 to max lod level.
  // clod reaches (n + 1) exactly at the border between level n and level n+1.
  
  // Add lod bias and clamp result.
  clod = clamp(clod + levelBias, 0, numberOfLevels - 1);
  // Now, clod reaches (n + 1) before the level border.
  
  // We sample 2 clipmap levels and use the clod fraction to lerp.
  int clodLow = (int)clod;
  int clodHigh = min(clodLow + 1, numberOfLevels - 1);
  float clodFrac = clod - clodLow;
  float4 sampleLow = SampleBaseClipmap(
    baseClipmapSampler, origins, cellSizeLevel0, cellsPerLevel, numberOfLevels, numberOfColumns,
    position.xz, clodLow);
  float4 sampleHigh = SampleBaseClipmap(
    baseClipmapSampler, origins, cellSizeLevel0, cellsPerLevel, numberOfLevels, numberOfColumns,
    position.xz, clodHigh);
  float4 sample = lerp(sampleLow, sampleHigh, clodFrac);
  
  // The base clipmap sample contains: height, normal x, normal z, hole
  
  position.y = sample.x;
  //position.y += lod;  // Separate LODs for debugging.
  
  // Standard normals.
  normal = float3(sample.y, 0, -sample.z);
  normal.y = sqrt(1 - normal.x * normal.x - normal.z * normal.z);
  
  // Partial derivative normals.
  //normal = float3(sample.y, 1, -sample.z);
  
  //normal = normalize(normal); // Normalization is done in pixel shader.
  
  // Holes are created by setting the vertex to a special invalid position.
  bool isHole = sample.w < holeThreshold;
  
  // We need to force a branch here, otherwise the Intel HD4000 creates distorted terrains for
  // some view directions - even if there are no holes.
  [branch]
  if (isHole)
    position = holePosition;
}


// Samples the detail height in the 3rd detail clipmap.
float SampleDetailHeight(sampler2D heightSampler, float2 texCoord, float2 offset,
                         int cellsPerLevel, int border, float borderInTexCoord,
                         int level, int numberOfLevels, int numberOfColumns)
{
  // Toroidal clipmap wrapping.
  texCoord = frac(texCoord + offset);
  
  // Consider border for texture filtering.
  texCoord = (texCoord * cellsPerLevel) / (cellsPerLevel + 2 * border) + borderInTexCoord;
  
  // Convert to texture atlas.
  texCoord = GetAtlasTexCoords(texCoord, level, numberOfLevels, numberOfColumns);
  
  return tex2Dlod(heightSampler, float4(texCoord, 0, 0)).y;
}


// Computes parallax occlusion mapping (see Parallax.fxh for more details).
// Returns (texCoord delta, height, shadow strength)
// viewDirection and lightDirection do not need to be normalized!
float4 ParallaxOcclusionMapping(float2 texCoord, sampler2D heightSampler,
                                float3 viewDirection, float heightScale, float heightBias, float mipLevel,
                                int lodThreshold, int minNumberOfSamples, int maxNumberOfSamples, float3 lightDirection,
                                float shadowScale, int shadowSamples, float shadowFalloff, float shadowStrength,
                                float2 offset, int cellsPerLevel, int border, float borderInTexCoord,
                                int level, int numberOfLevels, int numberOfColumns)
{
  float height = 0;
  float shadow = 0;
  float2 originalTexCoord = texCoord;
  if (mipLevel < (float) lodThreshold && heightScale > 0)
  {
    // Limit tangent space vector. Vectors can get extreme at grazing angles, especially
    // with bad interpolated normals. (tan(18°) ~ 1 / 3)
    if (length(viewDirection.xy) > 3 * abs(viewDirection.z))
      viewDirection.z = length(viewDirection.xy) / 3;
    if (length(lightDirection.xy) > 3 * abs(lightDirection.z))
      lightDirection.z = length(lightDirection.xy) / 3;
    
    viewDirection = normalize(viewDirection);
    lightDirection = normalize(lightDirection);
    
    // Compute furthest amount of parallax displacement. We get this displacement if the view
    // vector hits the bottom (height = 0) of the height map.
    float2 parallaxOffset = viewDirection.xy / abs(viewDirection.z) * heightScale;
    
    // Compute a dynamic number of steps for the linear search. Depending on the viewing angle
    // we lerp between minNumberOfSamples and maxNumberOfSamples.
    int numberOfSteps = (int) lerp(maxNumberOfSamples, minNumberOfSamples, abs(viewDirection.z));
    
#if !SM4
    // XNA compiler problems make it hard to compile shader for arbitrary loop limit. :-(
    numberOfSteps = 4;
#endif
    
    // Size of a step (relative to 1).
    float stepSize = 1.0 / (float) numberOfSteps;
    
    // Size of a step in texture coordinates.
    float2 texCoordStep = stepSize * parallaxOffset;
    
    // Because of the height bias we must change the start texture coordinate.
    float2 texCoordBias = -parallaxOffset * (1 + heightBias / heightScale);
    
    // The texture coordinate where we begin sampling the height texture.
    float2 currentTexCoord = texCoord + texCoordBias;
    float currentBound = 1.0;
    
    float2 pt1 = 0;
    float2 pt2 = 0;
    
    // Find hit of view ray against height vield.
    float previousHeight = 1;
    int stepIndex = 0;
    //[loop]       // TODO: Use [loop] attribute. - But this gives a "cannot unroll" error.
    while (stepIndex < numberOfSteps)
    {
      currentTexCoord += texCoordStep;
      
      // Sample height map which in this case is stored in the R channel.
      float currentHeight = SampleDetailHeight(
        heightSampler, currentTexCoord, offset, cellsPerLevel, border, borderInTexCoord,
        level, numberOfLevels, numberOfColumns);
      
      currentBound -= stepSize;
      
      if (currentHeight > currentBound)
      {
        pt1 = float2(currentBound, currentHeight );
        pt2 = float2(currentBound + stepSize, previousHeight);
        
        // Abort loop.
        stepIndex = numberOfSteps + 1;
      }
      else
      {
        stepIndex++;
        previousHeight = currentHeight;
      }
    }
    
    // Assume that the height is linear between two samples and compute the
    // height of the hit between the last two samples.
    float hitHeight;    // = height where view ray hits height field in range [0, 1].
    float delta2 = pt2.x - pt2.y;
    float delta1 = pt1.x - pt1.y;
    float denominator = delta2 - delta1;
    
    // Avoid division by zero.
    if (denominator == 0.0f)
      hitHeight = 0.0f;
    else
      hitHeight = (pt1.x * delta2 - pt2.x * delta1) / denominator;
    
    // Convert relative height to absolute height.
    height = heightBias + hitHeight * heightScale;
    
    // Correct texture coordinate.
    texCoord = texCoord + parallaxOffset * (1 - hitHeight) + texCoordBias;
    
    // Experimental soft self-shadowing:
    // From the hit position, sample to the light using fixed steps.
    lightDirection = lightDirection * shadowScale;
    float shadowHeight = hitHeight;
    [loop]
    for (int i = 1; i <= shadowSamples; i++)
    {
      float h = SampleDetailHeight(heightSampler, float2(texCoord - lightDirection.xy * i / shadowSamples),
                                   offset, cellsPerLevel, border, borderInTexCoord, level, numberOfLevels, numberOfColumns);
      
      // Reduce influence of distant samples.
      h = lerp(h, 0, shadowFalloff * i / shadowSamples);
      
      // Remember max height.
      shadowHeight = max(shadowHeight, h);
    }
    
    // The max found (height - hitHeight) is a measure of how much the pixel
    // is occluded.
    shadow = saturate((shadowHeight - hitHeight) * shadowStrength * heightScale);
    //shadow *= shadow;
    
    // Within the last LOD level, lerp POM influence to 0.
    if (mipLevel > (float)(lodThreshold - 1))
    {
      texCoord = lerp(texCoord, originalTexCoord, frac(mipLevel));
      shadow = lerp(shadow, 0, frac(mipLevel));
    }
  }
  
  return float4(texCoord - originalTexCoord, height, shadow);
}


// Main method for use in vertex shader.
void ComputeTerrainMaterial(sampler materialSampler0, sampler materialSampler1, sampler materialSampler2,
                            float2 origins[9], float cellSizes[9], int cellsPerLevel, int numberOfLevels, int numberOfColumns,
                            float levelBias, float2 offsets[9], float fadeRange, float enableAnisotropicFiltering,
                            float3 positionWorld, float3 viewDirectionTangent, float3 lightDirectionTangent,
                            int parallaxMinNumberOfSamples, int parallaxMaxNumberOfSamples,
                            float parallaxHeightScale, float parallaxHeightBias, int parallaxLodThreshold,
                            float parallaxNumberOfShadowSamples, float parallaxShadowScale,
                            float parallaxShadowFalloff, float parallaxShadowStrength,
                            out float3 normal, out float specularPower, out float3 diffuse,
                            out float alpha, out float3 specular)
{
  //  normal = float3(0.1, 1, 0.1);
  //  specularPower = 10;
  //  diffuse = (0.3).xxx;
  //  alpha = 1;
  //  specular = 1;
  //  return;
  
  // Hardcoded 8 texel border for 16x anisotropic filtering.
  float border = 8;
  float borderInTexCoord = border / (float)cellsPerLevel;
  cellsPerLevel -= 2 * border;
  
  float2 positionXZ = positionWorld.xz;
  float2 texCoordLevel0 = GetClipmapTexCoords(positionXZ, origins[0], cellsPerLevel, cellSizes[0]);
  float2 ddxLevel0 = ddx(texCoordLevel0);
  float2 ddyLevel0 = ddy(texCoordLevel0);
  
  // Compute standard mip level.
  float maxAnisotropy = 1 + (MAX_ANISOTROPY - 1) * enableAnisotropicFiltering;
  float mipLevel = max(0, MipLevel(texCoordLevel0, cellsPerLevel, maxAnisotropy) + levelBias);
  //mipLevel = max(0, MipLevel(texCoordLevel0, cellsPerLevel) + levelBias);
  
  // This mip level corresponds to this cell size.
  float mipCellSize = cellSizes[0] * exp2(mipLevel);
  
  // Find clipmap level which matches the cell size of the mip level.
  // (If clipmaps always double the cell size, then this code would be unnecessary.)
  int mipLevelFloor = numberOfLevels - 1;   // Integer part.
  float mipLevelFrac = 0;                   // Fraction part.
  mipLevel = numberOfLevels - 1;
  [loop]
  for (int i = 0; i < numberOfLevels - 1; i++)
  {
    float cellSize = cellSizes[i];
    float nextCellSize = cellSizes[i + 1];
    if (nextCellSize > mipCellSize)
    {
      mipLevelFloor = i;
      
      // Simple linear lerp based on cell size.
      //mipLevelFrac = (mipCellSize - cellSize) / (nextCellSize - cellSize);
      
      // Mip level is actual logrithmic:
      //mipLevelFrac = (log2(mipCellSize) - log2(cellSize)) / (log2(nextCellSize) - log2(cellSize));
      // In other words:
      mipLevelFrac = log2(mipCellSize / cellSize) / log2(nextCellSize / cellSize);
      
      break;
    }
  }
  
  mipLevel = mipLevelFloor + mipLevelFrac;
  
  // Find clipmap levels which contains the current pixel.
  int level = 0;
  float2 texCoord0 = texCoordLevel0;
  float2 p2 = abs(texCoord0 - 0.5) * 2;
  float p = max(p2.x, p2.y);  // p is 0 in the clipmap texture center and 1 at the border.
  [loop]
  for (level = 1; p >= 1 && level < numberOfLevels; level++)
  {
    texCoord0 = GetClipmapTexCoords(positionXZ, origins[level], cellsPerLevel, cellSizes[level]);
    p2 = abs(texCoord0 - 0.5) * 2;
    p = max(p2.x, p2.y);
  }
  level--;
  
  // Compute lerp parameter from p to lerp between current and next clipmap level.
  float texLerp = saturate((p - (1 - fadeRange)) / fadeRange);
  
  // If the mip level is higher than the current terrain level, then use the
  // mip level and its fraction for the lerp.
  if (mipLevelFloor > level)
  {
    level = mipLevelFloor;
    texLerp = mipLevelFrac;
    texCoord0 = GetClipmapTexCoords(positionXZ, origins[level], cellsPerLevel, cellSizes[level]);
  }
  else if (mipLevelFloor == level)
  {
    texLerp = max(texLerp, mipLevelFrac);
  }
  
  //texLerp = 0;
  
  // Now we know the two clipmap levels which we need to sample and lerp:
  int nextLevel = min(level + 1, numberOfLevels - 1);
  
  float2 texCoord1 = GetClipmapTexCoords(positionXZ, origins[nextLevel], cellsPerLevel, cellSizes[nextLevel]);
  
  // Parallax occlusion mapping.
  // We raymarch only the next level. (Ideally, we would raymarch level and nextLevel and lerp.)
  float shadow = 0;
  [branch]
  if (parallaxHeightScale > 0)
  {
    float4 pom = ParallaxOcclusionMapping(
      texCoord1, materialSampler2,
      viewDirectionTangent,
      parallaxHeightScale / (cellsPerLevel * cellSizes[nextLevel]),  // The height scale converted to texcoords.
      parallaxHeightBias / (cellsPerLevel * cellSizes[nextLevel]),
      mipLevel,  // We use clipmap level instead of mip level.
      parallaxLodThreshold, parallaxMinNumberOfSamples, parallaxMaxNumberOfSamples,
      lightDirectionTangent,
      parallaxShadowScale / (cellsPerLevel * cellSizes[nextLevel]),
      parallaxNumberOfShadowSamples, parallaxShadowFalloff, parallaxShadowStrength,
      offsets[nextLevel], cellsPerLevel, border, borderInTexCoord,
      nextLevel, numberOfLevels, numberOfColumns);
    
    texCoord0 += pom.xy / cellSizes[level] * cellSizes[nextLevel];
    texCoord1 += pom.xy;
    shadow = pom.w;
  }
  
  // Remember if we are near a border. (Only for debugging.)
  bool isBorder = false;
  if (texCoord0.x < 0.005 || texCoord0.y < 0.005 || texCoord0.x > 0.995 || texCoord0.y > 0.995)
    isBorder = true;
  
  // Toroidal clipmap wrapping
  texCoord0 = frac(texCoord0 + offsets[level]);
  texCoord1 = frac(texCoord1 + offsets[nextLevel]);
  
  // Consider border for texture filtering.
  texCoord0 = (texCoord0 * cellsPerLevel) / (cellsPerLevel + 2 * border) + borderInTexCoord;
  texCoord1 = (texCoord1 * cellsPerLevel) / (cellsPerLevel + 2 * border) + borderInTexCoord;
  
  // Convert to texture atlas.
  texCoord0 = GetAtlasTexCoords(texCoord0, level, numberOfLevels, numberOfColumns);
  texCoord1 = GetAtlasTexCoords(texCoord1, nextLevel, numberOfLevels, numberOfColumns);
  
  // Without anisotropic filtering.
  //float4 material0 = lerp(
  //  tex2Dlod(materialSampler0, float4(texCoord0, 0, 0)),
  //  tex2Dlod(materialSampler0, float4(texCoord1, 0, 0)),
  //  texLerp);
  //float4 material1 = lerp(
  //  tex2Dlod(materialSampler1, float4(texCoord0, 0, 0)),
  //  tex2Dlod(materialSampler1, float4(texCoord1, 0, 0)),
  //  texLerp);
  //float4 material2 = lerp(
  //  tex2Dlod(materialSampler2, float4(texCoord0, 0, 0)),
  //  tex2Dlod(materialSampler2, float4(texCoord1, 0, 0)),
  //  texLerp);
  
  // We need gradients for anisotropic filtering.
  float2 ddx0 = ddxLevel0 * cellSizes[0] / cellSizes[level];
  float2 ddy0 = ddyLevel0 * cellSizes[0] / cellSizes[level];
  float2 ddx1 = ddxLevel0 * cellSizes[0] / cellSizes[nextLevel];
  float2 ddy1 = ddyLevel0 * cellSizes[0] / cellSizes[nextLevel];
  
  // Sample next 2 clipmap levels and lerp between them.
  // material0 = (world space normal x, world space normal z, specular exponent, hole)
  float4 material0 = lerp(
    tex2Dgrad(materialSampler0, texCoord0, ddx0, ddy0),
    tex2Dgrad(materialSampler0, texCoord1, ddx1, ddy1),
    texLerp);
  // material1 = (diffuse rgb, -)
  float4 material1 = lerp(
    tex2Dgrad(materialSampler1, texCoord0, ddx0, ddy0),
    tex2Dgrad(materialSampler1, texCoord1, ddx1, ddy1),
    texLerp);
  // material2 = (specular intensity, detail height, - , -)
  float4 material2 = lerp(
    tex2Dgrad(materialSampler2, texCoord0, ddx0, ddy0),
    tex2Dgrad(materialSampler2, texCoord1, ddx1, ddy1),
    texLerp);
  
  normal = float3(material0.x, 0, material0.y);
  normal.xz = normal.xz * 255.0/128.0 - 1.0;
  // Reconstruct normal.y.
  // (Note: Through encoding and interpolation x*x + z*z can be greater than 1.
  // sqrt() of negative value is undefined. --> Use saturate() to clamp values.)
#pragma warning(disable : 4008)  // Floating point division by zero?!
  normal.y = sqrt(saturate(1 - normal.x * normal.x - normal.z * normal.z));
#pragma warning(enable : 4008)
  
  specularPower = DecodeSpecularPower(material0.z);
  alpha = material0.a; // = hole flag
  diffuse = FromGamma(material1.rgb) * (1 - shadow);
  specular = FromGamma(material2.rrr) * (1 - shadow);
  
  
//#if !PIXEL_HOLES && !PARALLAX_MAPPING
//  // Draw a red line at the border for debugging.
//  if (isBorder)
//    diffuse = float3(1, 0, 0);
//  // Draw a green line to visiualize the fade start.
//  if (texLerp > 0.001 && texLerp < 0.05)
//    diffuse = float3(0, 1, 0);
  //// To visualize mip levels.
  //float m = MipLevel2(texCoordLevel0, cellsPerLevel, maxAnisotropy);
  //diffuse = FromHsv(float3(((int)m) / 7.0, 1, 1)) * (m %1);
//#endif
}
#endif
