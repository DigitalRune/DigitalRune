// This post-processing effect distorts the image to create a simple underwater feeling.


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float2 ViewportSize;
float Time;

texture SourceTexture;
sampler SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
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

VSOutput VSPostProcess(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = input.Position;
  output.TexCoord = input.TexCoord;
  
#if !SM4
  // Apply half pixel offset.
  // See also http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
  output.Position.xy -= 0.5;
#endif

  // Now transform screen space coordinate into projection space.
  // Screen space: Left top = (0, 0), right bottom = (ScreenSize.x - 1, ScreenSize.y - 1).
  // Projection space: Left top = (-1, 1), right bottom = (1, -1).

  // Transform into the range [0, 1] x [0, 1].
  output.Position.xy /= ViewportSize;
  // Transform into the range [0, 2] x [-2, 0]
  output.Position.xy *= float2(2, -2);
  // Transform into the range [-1, 1] x [1, -1].
  output.Position.xy -= float2(1, -1);
  
  return output;
}



float4 PSPostProcess(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Calculate a pertubation for the texture coordinates
  float2 perturbation;
  perturbation.x = sin(6.28 * frac(Time / 3) + 20 * texCoord.x);
  perturbation.y = cos(6.28 * frac(Time / 3) + 20 * texCoord.y);
  perturbation *= 0.005;

  // Perturb the texture coordinates. Fall off to zero on the borders to avoid
  // artifacts on edges.
  perturbation.x *= saturate(0.8 - (2.0 * texCoord.x - 1.0) * (2.0 * texCoord.x - 1.0));
  perturbation.y *= saturate(0.8 - (2.0 * texCoord.y - 1.0) * (2.0 * texCoord.y - 1.0));
  
  return tex2D(SourceSampler, texCoord + perturbation);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique 
{
  pass 
  {
#if !SM4
    VertexShader = compile vs_3_0 VSPostProcess();
    PixelShader = compile ps_3_0 PSPostProcess();
#else
    VertexShader = compile vs_4_0 VSPostProcess();
    PixelShader = compile ps_4_0 PSPostProcess();
#endif
  }
}
