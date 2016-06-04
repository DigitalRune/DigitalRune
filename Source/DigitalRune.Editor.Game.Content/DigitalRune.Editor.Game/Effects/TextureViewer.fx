// The size of the viewport in pixels.
float2 ViewportSize : VIEWPORTSIZE = float2(1280, 720);

// Checker pattern.
float4 CheckerColor0 = float4(0.136, 0.136, 0.136, 1);
float4 CheckerColor1 = float4(0.319, 0.319, 0.319, 1);
float2 CheckerCount = float2(1280 / 8, 720 / 8);

float InputGamma = 2.2;
float OutputGamma = 2.2;

float4x4 ColorTransform = float4x4(1, 0, 0, 0,
                                   0, 1, 0, 0,
                                   0, 0, 1, 0,
                                   0, 0, 0, 1);
float4 ColorOffset = float4(0, 0, 0, 0);

float MipLevel;

// The texture containing the original image.
uniform const texture SourceTexture : SOURCETEXTURE;
uniform const sampler SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
};


void VS(inout float2 texCoord : TEXCOORD0,
        inout float4 position : SV_POSITION)
{
  // Convert position from screen space to projection space.
#if !SM4
  position.xy -= 0.5;
#endif
  position.xy /= ViewportSize;
  position.xy *= float2(2, -2);
  position.xy -= float2(1, -1);
}


float4 Checker(float2 texCoord)
{
  int2 pattern = (int2)((texCoord * CheckerCount) % 2);
  if (pattern.x == pattern.y)
    return CheckerColor0;
  else
    return CheckerColor1;
}


float4 PS(float2 texCoord : TEXCOORD0, bool isPremultipliedAlpha) : COLOR0
{
  float4 background = Checker(texCoord);
  float4 color = tex2Dlod(SourceSampler, float4(texCoord.xy, 0, MipLevel));
  
  // Convert from sRGB (gamma) to linear space.
  color.rgb = pow(abs(color.rgb), InputGamma);
  
  // Color transformation
  color = mul(color, ColorTransform) + ColorOffset;
  
  // Alpha blending
  float4 finalColor;
  if (isPremultipliedAlpha)
  {
    // Premultiplied alpha
    finalColor.rgb = color.rgb + background.rgb * (1 - color.a);
    finalColor.a = 1;
  }
  else
  {
    // Straight alpha
    finalColor.rgb = color.rgb * color.a + background.rgb * (1 - color.a);
    finalColor.a = 1;
  }
  
  // Gamma correction (when not using sRGB formats)
  finalColor.rgb = pow(abs(finalColor.rgb), 1 / OutputGamma);
  
  return finalColor;
}
float4 PSPremultipliedAlpha(float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord, true); }
float4 PSStraightAlpha(float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord, false); }


technique
{
  pass StraightAlpha
  {
#if !SM4
    VertexShader = compile vs_3_0 VS();
    PixelShader = compile ps_3_0 PSStraightAlpha();
#else
    VertexShader = compile vs_4_0 VS();
    PixelShader = compile ps_4_0 PSStraightAlpha();
#endif
  }

  pass PremultipliedAlpha
  {
#if !SM4
    VertexShader = compile vs_3_0 VS();
    PixelShader = compile ps_3_0 PSPremultipliedAlpha();
#else
    VertexShader = compile vs_4_0 VS();
    PixelShader = compile ps_4_0 PSPremultipliedAlpha();
#endif
  }
}
