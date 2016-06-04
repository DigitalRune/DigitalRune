// A distortion post-processing effect.

// The size of the viewport in pixels.
uniform const float2 ViewportSize : VIEWPORTSIZE = float2(1280, 720);

// The strength of the effect in the range [0, 1].
uniform const float Strength = 1;

// The texture containing the original image.
uniform const texture SourceTexture : SOURCETEXTURE;
uniform const sampler SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  MipFilter = NONE;
};

// A texture containing the distortion offsets (R, G).
// No distortion where Alpha is 0.
uniform const texture DistortionTexture : SOURCETEXTURE;
uniform const sampler DistortionSampler = sampler_state
{
  Texture = <DistortionTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  MipFilter = NONE;
};


// Poisson disk kernel for optional blur.
// (n = 5, min distance = 0,9):
//#define NumberOfPoissonSamples 5
//const float2 PoissonKernel[NumberOfPoissonSamples] =
//{
//  float2(0.0f, 0.0f),
//  float2(0.2350235f, -0.9049428f),
//  float2(-0.9714093f, -0.05954546f),
//  float2(0.9876884f, -0.08089168f),
//  float2(-0.1840318f, 0.9780183f),
//};


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


// Converts a position from screen space to projection space.
// See ScreenToProjection() in Common.fxh for a detailed description.
VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = input.Position;
  output.TexCoord = input.TexCoord;
#if !SM4
  output.Position.xy -= 0.5;
#endif
  output.Position.xy /= ViewportSize;
  output.Position.xy *= float2(2, -2);
  output.Position.xy -= float2(1, -1);
  return output;
}


float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get distortion vector.
  float4 distortion = tex2D(DistortionSampler, texCoord);
  
  // R and G contain the distortion offset. The signed offset needs to be unpacked.
  // Alpha is 0 where we do not want any distortion.
  float2 offset = Strength * distortion.a * (2 * distortion.rg - 1);
  
  // Return scene pixel of distorted position.
  // If we use this, then the distortion might sample objects which are actually
  // in front of the distortion.
  //return tex2D(SourceSampler, texCoord + offset);
  // To avoid this, we sample the distortion strength at the distorted position
  // and use this as scale for the distortion offset.
  float mask = tex2D(DistortionSampler, texCoord + offset).a;
  return tex2D(SourceSampler, texCoord + offset * mask);
  
  // We could also blur the distorted scene:
  //float4 color = 0;
  //float blurScale = 0.002f * distortion.a;
  //for (int i = 0; i < NumberOfPoissonSamples; i++)
  //{
  //  float mask = tex2D(DistortionSampler, texCoordDistorted + blurScale * PoissonKernel[i]).a;
  //  color += tex2D(SourceSampler, texCoord + offset * mask + blurScale * PoissonKernel[i]);
  //}
  //return color / NumberOfPoissonSamples;
}


technique Technique0
{
  pass Pass0
  {
#if !SM4
    VertexShader = compile vs_2_0 VS();
    PixelShader = compile ps_2_0 PS();
#else
    VertexShader = compile vs_4_0_level_9_1 VS();
    PixelShader = compile ps_4_0_level_9_1 PS();
#endif
  }
}
