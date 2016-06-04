// A simple post-processing effect that creates a .

// The size of the viewport in pixels.
float2 ViewportSize : VIEWPORTSIZE = float2(1280, 720);

// Power defines the distortion curve.
// 1 .... distortion grows linearly with distance from screen center
// >1 ... non-linear distortion
float Power = 2;

// The max distortion for R, G and B.
// Example: 0.01 = 1% distortion at screen border.
// (Distortion in screen center is always 0.)
float3 Distortion = -0.02;
//float3 QuadraticDistortion = 0;
//float3 CubicDistortion = 0;

// The texture containing the original image.
texture SourceTexture : SOURCETEXTURE;
sampler SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  MipFilter = NONE;
};


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


// Inverts the RGB color.
float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Convert texcoords to range [-1, 1].
  texCoord = 2 * texCoord - 1;
  
  float aspect = ViewportSize.x / ViewportSize.y;
  float radiusSquared = aspect * aspect * texCoord.x * texCoord.x + texCoord.y * texCoord.y;
  float radius = sqrt(radiusSquared);
  
  // Compute distortion factor for each channel.
  float3 f = 1 + Distortion * pow(abs(0.0001 + radius), Power);
  
  // Or use SynthEyes Lens Distortion Algorithm (http://www.ssontech.com/content/lensalg.htm)
  //float3 f = 1 + radiusSquared * (QuadraticDistortion + CubicDistortion * radius);
  
  // Get distorted coordinates for each color channel and convert back to [0, 1].
  float2 texCoordR = (f.r * texCoord + 1) * 0.5;
  float2 texCoordG = (f.g * texCoord + 1) * 0.5;
  float2 texCoordB = (f.b * texCoord + 1) * 0.5;
  
   float4 color;
   color.r = tex2D(SourceSampler, texCoordR).r;
   color.ga = tex2D(SourceSampler, texCoordG).ga;  // We use the alpha value from this sample.
   color.b = tex2D(SourceSampler, texCoordB).b;
   
   return color;
}


technique Technique0
{
  pass Pass0
  {
#if !SM4
    VertexShader = compile vs_3_0 VS();
    PixelShader = compile ps_3_0 PS();
#else
    VertexShader = compile vs_4_0 VS();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}
