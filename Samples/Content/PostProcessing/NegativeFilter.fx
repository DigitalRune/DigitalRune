// A simple post-processing effect that inverts colors.

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
  // Get original color.
  float4 color = tex2D(SourceSampler, texCoord);
  
  // Linearly interpolate between original and inverted color.
  color.rgb = lerp(color.rgb, 1 - color.rgb, Strength);
    
  return color;
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
