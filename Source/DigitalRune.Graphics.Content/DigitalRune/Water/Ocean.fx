//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Ocean.fx
/// Computes the ocean wave spectrum and performs inverse Fast Fourier Transform
/// (FFT).
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// ----- General
// The viewport size in pixels.
float Size;

// ----- Spectrum
float4 SpectrumParameters;
#define PatchSize SpectrumParameters.x
#define Gravity SpectrumParameters.y
#define FftTime SpectrumParameters.z
#define Height SpectrumParameters.w

// ----- FFT
float ButterflyIndex;    // Determines the row of the butterfly texture.
//bool IsLastPass;
//float LastPassScale;    // Scale factor for the last forward FFT pass 1/N^2; otherwise, 1.
float Choppiness;

Texture2D ButterflyTexture;
sampler ButterflySampler = sampler_state
{
  Texture = <ButterflyTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

// Each source texture contains two complex images to perform up to 4 different FFTs.
// Image0 = SourceTexture0.xy, Image1 = SourceTexture0.zw
// Image2 = SourceTexture1.xy, Image3 = SourceTexture1.zw
Texture2D SourceTexture0;
sampler SourceSampler0 = sampler_state
{
  Texture = <SourceTexture0>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

Texture2D SourceTexture1;
sampler SourceSampler1 = sampler_state
{
  Texture = <SourceTexture1>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
};

struct VSOutput
{
  float2 TexCoord : TEXCOORD;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, float2(Size, Size));
  output.TexCoord = input.TexCoord;
  return output;
}


float4 VSSpectrum(VSInput input) : SV_Position
{
  return ScreenToProjection(input.Position, float2(Size, Size));
}


// Get texcoord to sample for frequency/position p in h0 texture.
float2 GetH0TexCoord(float2 p, float n)
{
  // layout of h0: -n/2, ..., -1, 0, 1, ..., n/2
  float2 index = p + float2(n / 2, n / 2);
  
  // Convert to texcoord.
  // size of h0 is n+1
  // We add 0.5 to sample texel centers.
  return (index + float2(0.5, 0.5)) / (n + 1);
}


float2 GetK(float2 p)
{
  return 2.0 * Pi * p / PatchSize;
}


void PSSpectrum(
#if !MGFX
  float4 vPos : VPOS,
#else
  float4 vPos : SV_Position,
#endif
  out float4 color0 : COLOR0,
  out float4 color1 : COLOR1)
{
  int n = Size;
#if MGFX
  vPos.xy = (int2)vPos;
#endif
  
  // vPos is the index in the FFT spectral image is
  
  // Get frequency/position x of current pixel.
  // The spectral image uses this layout (same as on CPU):
  // 0, 1, 2, ..., n/2-1, -n/2, -n/2+1, ..., -1
  // TODO: Use different layout than CPU to simplify index computations!?
  float2 p = vPos.xy;
  if (vPos.x >= n / 2)  // Second half?
    p.x = vPos.x - n;
  if (vPos.y >= n / 2)
    p.y = vPos.y - n;
  
  float2 k = GetK(p);
  float kLength = length(k);
  float omega = sqrt(Gravity * kLength);  // We could cache omega in a texture like h0.
  
  float c = cos(omega * FftTime);
  float s = sin(omega * FftTime);
  
  // Sample h0(x, y) and h0(-x, -y).
  float2 h0XY = tex2Dlod(SourceSampler0, float4(GetH0TexCoord(p, n), 0, 0)).xy * Height;
  float2 h0NegXNegY = tex2Dlod(SourceSampler0, float4(GetH0TexCoord(-p, n), 0, 0)).xy * Height;
  
  float2 h = float2((h0XY.x + h0NegXNegY.x) * c - (h0XY.y + h0NegXNegY.y) * s,
                    (h0XY.x - h0NegXNegY.x) * s + (h0XY.y - h0NegXNegY.y) * c);
  
  float2 N = float2(-k.x * h.y - k.y * h.x,
                    k.x * h.x - k.y * h.y);
  
  // Avoid division by zero.
  kLength = max(1e-8f, kLength);
  
  k /= kLength;
  float2 D = float2(k.x * h.y + k.y * h.x,
                    -k.x * h.x + k.y * h.y);
  
  color0 = float4(h, D);     // Target 0 stores spectrum for h and D.
  color1 = float4(N, 0, 0);  // Target 1 stores spectrum for N.
}


// Perform one FFT butterfly pass.
void PSFft(float2 texCoord : TEXCOORD0, out float4 color0 : COLOR0, out float4 color1 : COLOR1, uniform bool isHorizontalPass)
{
  float2 butterflyTexCoord = isHorizontalPass ? float2(texCoord.x, ButterflyIndex)
                                              : float2(texCoord.y, ButterflyIndex);
  float4 indexWeight = tex2Dlod(ButterflySampler, float4(butterflyTexCoord, 0, 0));
  float2 i = indexWeight.xy;     // Scrambling coords.
  float2 w = indexWeight.zw;     // Weights.
  
  float2 evenTexCoord;
  float2 oddTexCoord;
  if (isHorizontalPass)
  {
    evenTexCoord = float2(i.x, texCoord.y);
    oddTexCoord = float2(i.y, texCoord.y);
  }
  else
  {
    evenTexCoord = float2(texCoord.x, i.x);
    oddTexCoord = float2(texCoord.x, i.y);
  }
  
  float4 e = tex2Dlod(SourceSampler0, float4(evenTexCoord, 0, 0));  // Even samples for 2 images
  float4 o = tex2Dlod(SourceSampler0, float4(oddTexCoord, 0, 0));   // Odd samples for 2 images
  
  // Odd must be multiplied by complex weight W^k.
  o = float4(w.x * o.x - w.y * o.y,
             w.y * o.x + w.x * o.y,
             w.x * o.z - w.y * o.w,
             w.y * o.z + w.x * o.w);
  
  float4 result0 = e + o;
  
  e = tex2Dlod(SourceSampler1, float4(evenTexCoord, 0, 0));  // Even
  o = tex2Dlod(SourceSampler1, float4(oddTexCoord, 0, 0));   // Odd
  
  // Odd must be multiplied by weight W^k.
  o = float4(w.x * o.x - w.y * o.y,
             w.y * o.x + w.x * o.y,
             w.x * o.z - w.y * o.w,
             w.y * o.z + w.x * o.w);
  
  float4 result1 = e + o;
  
  //if (!IsLastPass)
  //{
  color0 = result0;
  color1 = result1;
  return;
  //}
  
  // For normal forward FFT...
  //result0 *= LastPassScale;
  //result1 *= LastPassScale;
  
  // In last pass we could collect the actually needed results, e.g. ignoring imaginary values...
}


void PSFftHorizontal(float2 texCoord : TEXCOORD0, out float4 color0 : COLOR0, out float4 color1 : COLOR1)
{
  PSFft(texCoord, color0, color1, true);
}


void PSFftVertical(float2 texCoord : TEXCOORD0, out float4 color0 : COLOR0, out float4 color1 : COLOR1)
{
  PSFft(texCoord, color0, color1, false);
}


// Perform one final FFT vertical butterfly pass for displacement texture.
float4 PSFftDisplacement(float2 texCoord : TEXCOORD0) : COLOR0
{
  float2 butterflyTexCoord = float2(texCoord.y, ButterflyIndex);
  float4 indexWeight = tex2Dlod(ButterflySampler, float4(butterflyTexCoord, 0, 0));
  float2 i = indexWeight.xy;     // Scrambling coords.
  float2 w = indexWeight.zw;     // Weights.
  
  float2 evenTexCoord = float2(texCoord.x, i.x);
  float2 oddTexCoord = float2(texCoord.x, i.y);
  
  float4 e = tex2Dlod(SourceSampler0, float4(evenTexCoord, 0, 0));  // Even samples for 2 images
  float4 o = tex2Dlod(SourceSampler0, float4(oddTexCoord, 0, 0));   // Odd samples for 2 images
  
  // Odd must be multiplied by complex weight W^k.
  o = float4(w.x * o.x - w.y * o.y,
             w.y * o.x + w.x * o.y,
             w.x * o.z - w.y * o.w,
             w.y * o.z + w.x * o.w);
  
  float4 result0 = e + o;
  
  // Collect results in ocean displacement map.
  float height = result0.x;
  float displacementX = -result0.z * Choppiness;
  float displacementY = -result0.w * Choppiness;
  
  return float4(displacementX, height, displacementY, 1);
}


// Perform one final vertical FFT butterfly pass for normal texture.
float4 PSFftNormal(float2 texCoord : TEXCOORD0) : COLOR0
{
  float2 butterflyTexCoord = float2(texCoord.y, ButterflyIndex);
  float4 indexWeight = tex2Dlod(ButterflySampler, float4(butterflyTexCoord, 0, 0));
  float2 i = indexWeight.xy;     // Scrambling coords.
  float2 w = indexWeight.zw;     // Weights.
  
  float2 evenTexCoord = float2(texCoord.x, i.x);
  float2 oddTexCoord = float2(texCoord.x, i.y);
  
  float4 e = tex2Dlod(SourceSampler0, float4(evenTexCoord, 0, 0));  // Even samples for 2 images
  float4 o = tex2Dlod(SourceSampler0, float4(oddTexCoord, 0, 0));   // Odd samples for 2 images
  
  // Odd must be multiplied by complex weight W^k.
  o = float4(w.x * o.x - w.y * o.y,
             w.y * o.x + w.x * o.y,
             w.x * o.z - w.y * o.w,
             w.y * o.z + w.x * o.w);
  
  float4 result0 = e + o;
  
  // Collect results in ocean normal map.
  float3 normal = float3(-result0.x, -result0.y, 0);
  normal.z = sqrt(1 - normal.x * normal.x - normal.y * normal.y);
  
  return float4(normal * 0.5 + 0.5, 1);
}

// Used to copy normal from Vector4 texture to Color texture.
float4 PSCopyNormal(float2 texCoord : TEXCOORD0) : COLOR0
{
  return tex2Dlod(SourceSampler0, float4(texCoord, 0, 0));
}


//--------------------------------------------------------
// Techniques
//--------------------------------------------------------

#if !SM4
#define VSTARGET vs_3_0
#define PSTARGET ps_3_0
#else
#define VSTARGET vs_4_0
#define PSTARGET ps_4_0
#endif


technique
{
  pass Spectrum
  {
    VertexShader = compile VSTARGET VSSpectrum();
    PixelShader = compile PSTARGET PSSpectrum();
  }
  
  pass FftHorizontal
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSFftHorizontal();
  }
  
  pass FftVertical
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSFftVertical();
  }
  
  pass FinalFftDisplacementPass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSFftDisplacement();
  }
  
  pass FinalFftNormalPass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSFftNormal();
  }
  
  pass CopyNormal  // Pass to copy normal map.
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCopyNormal();
  }
}
