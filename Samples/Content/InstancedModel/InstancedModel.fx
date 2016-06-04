// This effect is based on:
//-----------------------------------------------------------------------------
// InstancedModel.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Camera settings.
float4x4 World;
float4x4 View;
float4x4 Projection;

// This sample uses a simple Lambert lighting model.
float3 DirectionalLightDirection : DIRECTIONALLIGHTDIRECTION = normalize(float3(-1, -0, -1));
float3 DirectionalLightDiffuse : DIRECTIONALLIGHTDIFFUSE = 1.25;
float3 AmbientLight = 0.25;

float3 InstanceColor : INSTANCECOLOR = float3(1, 1, 1);
float InstanceAlpha : INSTANCEALPHA = 1;

texture DiffuseTexture : DIFFUSETEXTURE;
sampler DiffuseSampler = sampler_state
{
  Texture = <DiffuseTexture>;
};


struct VertexShaderInput
{
  float4 Position : POSITION0;
  float3 Normal : NORMAL0;
  float2 TextureCoordinate : TEXCOORD0;
};


struct VertexShaderOutput
{
  float4 Color : COLOR0;
  float2 TextureCoordinate : TEXCOORD0;
  float4 Position : SV_Position;
};


struct PixelShaderInput
{
  float4 Color : COLOR0;
  float2 TextureCoordinate : TEXCOORD0;
#if SM4
  float4 VPos : SV_Position;
#else
  float2 VPos : VPOS;
#endif
};


// Vertex shader helper function shared between the two techniques.
VertexShaderOutput VertexShaderCommon(VertexShaderInput input, float4x4 instanceTransform,
                                      float3 instanceColor, float instanceAlpha)
{
  VertexShaderOutput output;

  // Apply the world and camera matrices to compute the output position.
  float4 worldPosition = mul(input.Position, instanceTransform);
  float4 viewPosition = mul(worldPosition, View);
  output.Position = mul(viewPosition, Projection);

  // Compute lighting, using a simple Lambert model.
  float3 worldNormal = mul(input.Normal, (float3x3)instanceTransform);
  
  float diffuseAmount = max(-dot(worldNormal, DirectionalLightDirection), 0);
  
  float3 lightingResult = saturate(diffuseAmount * DirectionalLightDiffuse + AmbientLight);
  
  output.Color = float4(lightingResult * instanceColor, instanceAlpha);
  
  // Copy across the input texture coordinate.
  output.TextureCoordinate = input.TextureCoordinate;

  return output;
}


// When instancing is disabled we take the world transform from an effect parameter.
VertexShaderOutput NoInstancingVertexShader(VertexShaderInput input)
{
  return VertexShaderCommon(input, World, InstanceColor, InstanceAlpha);
}


// Hardware instancing reads the per-instance world transform from a secondary vertex stream.
// BlendWeight0-2 are used for the World matrix. The 4th column of the matrix is always (0, 0, 0, 1).
// BlendWeight3 is used for InstanceColor and InstanceAlpha.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input, 
                                                  float4 worldColumn0 : BLENDWEIGHT0,
                                                  float4 worldColumn1 : BLENDWEIGHT1,
                                                  float4 worldColumn2 : BLENDWEIGHT2,
                                                  float4 colorAndAlpha : BLENDWEIGHT3)
{
  float4x4 worldTransposed = 
  {
    worldColumn0, 
    worldColumn1, 
    worldColumn2, 
    float4(0, 0, 0, 1)
  };
  float4x4 world = transpose(worldTransposed);
  
  return VertexShaderCommon(input, world, colorAndAlpha.rgb, colorAndAlpha.a);
}


/// Returns a value in the range [0, 1] of a regular 4x4 dither pattern.
/// \param[in] p   The (x, y) position in pixels.
/// \return A value in the range [0, 1].
float Dither4x4(float2 p)
{
  static const float4x4 DitherMatrix =
  {
     1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
    13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
     4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
    16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
  };
  return DitherMatrix[p.x % 4][p.y % 4];
}

// Both techniques share this same pixel shader.
float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
  // Screen-door transparency
  clip(input.Color.a - Dither4x4(input.VPos.xy));
  
  return tex2D(DiffuseSampler, input.TextureCoordinate) * input.Color;
}


// For rendering without instancing.
technique Default
#if !MGFX
<
  // There is an equivalent of this technique that supports hardware instancing.
  string InstancingTechnique = "DefaultInstancing";
>
#endif
{
  pass 
  {
#if !SM4
    VertexShader = compile vs_3_0 NoInstancingVertexShader();
    PixelShader = compile ps_3_0 PixelShaderFunction();
#else
    VertexShader = compile vs_4_0 NoInstancingVertexShader();
    PixelShader = compile ps_4_0 PixelShaderFunction();
#endif
  }
}


// Hardware instancing technique.
technique DefaultInstancing
{
  pass 
  {
#if !SM4
    VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
    PixelShader = compile ps_3_0 PixelShaderFunction();
#else
    VertexShader = compile vs_4_0 HardwareInstancingVertexShader();
    PixelShader = compile ps_4_0 PixelShaderFunction();
#endif
  }
}
