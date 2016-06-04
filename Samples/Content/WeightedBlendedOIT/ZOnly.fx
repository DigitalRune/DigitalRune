//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

float4 VS(float4 position : POSITION, float4x4 world) : SV_Position
{
  float4 positionWorld = mul(position, world);
  float4 positionView = mul(positionWorld, View);
  float4 positionProj = mul(positionView, Projection);
  
  return positionProj;
}


float4 VSNoInstancing(float4 position : POSITION) : SV_Position
{
  return VS(position, World);
}


float4 VSInstancing(float4 position : POSITION0,
                    float4 worldColumn0 : BLENDWEIGHT0,
                    float4 worldColumn1 : BLENDWEIGHT1,
                    float4 worldColumn2 : BLENDWEIGHT2) : SV_Position
{
  float4x4 worldTransposed =
  {
    worldColumn0,
    worldColumn1,
    worldColumn2,
    float4(0, 0, 0, 1)
  };
  float4x4 world = transpose(worldTransposed);
  
  return VS(position, world);
}

float4 PS() : COLOR0
{
  return float4(0, 0, 0, 0);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique Default
#if !MGFX
< string InstancingTechnique = "DefaultInstancing"; >
#endif
{
  pass
  {
#if !SM4
    // XNA
    VertexShader = compile vs_2_0 VSNoInstancing();
    PixelShader = compile ps_2_0 PS();
#else
    // MonoGame
    VertexShader = compile vs_4_0_level_9_1 VSNoInstancing();
    PixelShader = compile ps_4_0_level_9_1 PS();
#endif
  }
}


technique DefaultInstancing
{
  pass
  {
#if !SM4
    // XNA
    VertexShader = compile vs_3_0 VSInstancing();
    PixelShader = compile ps_3_0 PS();
#else
    // MonoGame
    VertexShader = compile vs_4_0 VSInstancing();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}
