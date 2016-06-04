// A simple post-processing effect that creates a vignette effect.

// The size of the viewport in pixels.
float2 ViewportSize : VIEWPORTSIZE = float2(1280, 720);

// Scale defines the shape and size of the vignette effect.
float2 Scale = 1;

// Power defines the vignette curve.
// 1 .... linear brightness falloff
// >1 ... non-linear brightness falloff
float Power = 1;

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
  float4 color = tex2D(SourceSampler, texCoord);

  // Get scaled distance to screen center.
  float radius = length((texCoord - 0.5) * 2 / Scale);
  // Compute a vignette strength proportional to the distance and using a non-linear curve.
  float vignette = pow(abs(0.0001 + radius), Power);
    
  return color * saturate(1 - vignette);
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
