//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file SkyObject.fx
/// Renders a textured quad into the sky and two glows. The quad is shaded
/// to render "moon phases".
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"

//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 ViewProjection : VIEWPROJECTION;

float3 Up;       // The billboard up axis in world space.
float3 Right;    // The billboard right axis in world space.
float3 Normal;   // The billboard normal in world space.

float3 SunDirection;  // Direction to the sun.
float3 SunLight;

// AmbientLight.a is used for the Alpha of the whole object.
float4 AmbientLight;
#define Alpha AmbientLight.a

texture ObjectTexture;
sampler ObjectSampler = sampler_state
{
  Texture = <ObjectTexture>;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
  AddressU = CLAMP;
  AddressV = CLAMP;
};

float4 TextureParameters;
#define TextureCenter TextureParameters.xy
#define OneOverTextureHalfExtent TextureParameters.zw

// The amount of wrap lighting [0, 1] and the smoothness [0, 1].
float2 LightWrapSmoothness;
#define LightWrap LightWrapSmoothness.x
#define LightSmoothness LightWrapSmoothness.y

// Two Phong highlights (RGB = color and A = exponent).
float4 Glow0;
float4 Glow1;


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSOutputObject
{
  float2 TexCoord : TEXCOORD0;
  float4 Position : SV_Position;
};

struct PSInputObject
{
  float2 TexCoord : TEXCOORD0;
};


struct VSOutputGlow
{
  float2 TexCoord : TEXCOORD0;
  float3 PositionWorld : TEXCOORD1;
  float4 Position : SV_Position;
};

struct PSInputGlow
{
  float2 TexCoord : TEXCOORD0;
  float3 PositionWorld : TEXCOORD1;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutputObject VSObject(in float4 position : POSITION,
                        in float2 texCoord : TEXCOORD)
{
  VSOutputObject output = (VSOutputObject)0;
  output.Position = mul(position, ViewProjection).xyww;
  output.TexCoord = texCoord;
  
  return output;
}


float4 PSObject(PSInputObject input, bool correctGamma)
{
  // Compute a normal for a sphere in the image space. z is pointing out.
  float nx = (input.TexCoord.x - TextureCenter.x) * OneOverTextureHalfExtent.x;
  float ny = (input.TexCoord.y - TextureCenter.y) * OneOverTextureHalfExtent.y;
  float nz = sqrt(1 - nx * nx - ny * ny);
  
  // Compute a normal in world space.
  float3 normal = nx * Right - ny * Up + nz * Normal;
  
  // Sample texture.
  float4 color = tex2D(ObjectSampler, input.TexCoord);
  color.rgb = FromGamma(color.rgb);
  
  // Compute diffuse factor N.L with wrap lighting.
  float nDotLWrapped = saturate((dot(SunDirection, normal) + LightWrap) / (1 + LightWrap));
  
  // Smooth the wrap lighting to avoid harsh light/dark borders.
  nDotLWrapped = lerp(nDotLWrapped, smoothstep(0, 1, nDotLWrapped), LightSmoothness);
  
  // Modulate texture with light.
  float3 light = AmbientLight.rgb + SunLight * nDotLWrapped;
  color.rgb *= light;
  
  if (correctGamma)
    color.rgb = ToGamma(color.rgb);
  
  // Add alpha (premultiply).
  color *= Alpha;
  
  return color;
}
float4 PSObjectLinear(PSInputObject input) : COLOR { return PSObject(input, false); }
float4 PSObjectGamma(PSInputObject input) : COLOR  { return PSObject(input, true); }


VSOutputGlow VSGlow(in float4 position : POSITION,
                    in float2 texCoord : TEXCOORD)
{
  VSOutputGlow output = (VSOutputGlow)0;
  output.Position = mul(position, ViewProjection).xyww;
  output.TexCoord = texCoord;
  output.PositionWorld = -position.xyz; // Negate here instead of in PS.
  
  return output;
}


float4 PSGlow(PSInputGlow input, bool correctGamma)
{
  float3 toCamera = normalize(input.PositionWorld);
  
  // Sum up two phong highlights.
  float4 color = float4(0, 0, 0, 0);
  color.rgb = Glow0.rgb * pow(max(0.000001, dot(Normal, toCamera)), Glow0.a);
  color.rgb += Glow1.rgb * pow(max(0.000001, dot(Normal, toCamera)), Glow1.a);
  
  // Distance in the range [0, 1] from billboard center.
  float d = 2 * length((input.TexCoord.xy - float2(0.5, 0.5)));
  // Attenuate the glow with the curve 1 - x² to get values of 0 at the billboard border.
  color.rgb *= max(0, smoothstep(1, 0, d)).xxx;
  
  if (correctGamma)
    color.rgb = ToGamma(color.rgb);
  
  return color;
}
float4 PSGlowLinear(PSInputGlow input) : COLOR { return PSGlow(input, false); }
float4 PSGlowGamma(PSInputGlow input) : COLOR  { return PSGlow(input, true); }


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_2_0
#define PSTARGET ps_2_0
#else
#define VSTARGET vs_4_0_level_9_1
#define PSTARGET ps_4_0_level_9_1
#endif

technique
{
  pass ObjectLinear     // Object, not gamma corrected
  {
    VertexShader = compile VSTARGET VSObject();
    PixelShader = compile PSTARGET PSObjectLinear();
  }
  pass ObjectGamma      // Object, gamma corrected
  {
    VertexShader = compile VSTARGET VSObject();
    PixelShader = compile PSTARGET PSObjectGamma();
  }
  
  pass GlowLinear     // Glow, not gamma corrected
  {
    VertexShader = compile VSTARGET VSGlow();
    PixelShader = compile PSTARGET PSGlowLinear();
  }
  pass GlowGamma      // Glow, gamma corrected
  {
    VertexShader = compile VSTARGET VSGlow();
    PixelShader = compile PSTARGET PSGlowGamma();
  }
}
