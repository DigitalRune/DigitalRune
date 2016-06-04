// A simple post-processing effect that creates a vignette effect.

// The size of the viewport in pixels.
float2 ViewportSize : VIEWPORTSIZE = float2(1280, 720);

float Strength = 1;

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
  
  // Change color in every second row.
  if (floor(texCoord.y * ViewportSize.y) % 2)
  {
    float luminance = dot(color.rgb, float3(0.2126, 0.7152, 0.0722));
    
    // Keep luminance below 1. This allows to make even white pixels black by
    // using a very large strength (>> 1).
    luminance = min(0.999, luminance);
    
    color.rgb = lerp(color.rgb, color.rgb * luminance, Strength);
  }

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
