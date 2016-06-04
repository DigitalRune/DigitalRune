// Renders the velocity into a velocity buffer.
//
// The velocity buffer contains the position change of each pixel:
// current position - position of last frame (using texture space coordinates).
//

float4x4 WorldViewProjection;
float4x4 LastWorldViewProjection;


struct VSInput
{
  float4 Position : POSITION;
};

struct VSOutput
{
  float4 PositionProj : TEXCOORD0;
  float4 LastPositionProj : TEXCOORD1;
  float4 Position : SV_Position;
};

struct PSInput
{
  float4 PositionProj : TEXCOORD0;
  float4 LastPositionProj : TEXCOORD1;
};


VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.PositionProj = mul(input.Position, WorldViewProjection); 
  output.LastPositionProj = mul(input.Position, LastWorldViewProjection);
  output.Position = output.PositionProj;
  return output;
}


float4 PS(PSInput input) : COLOR0
{
  // Homogenous divide:
  input.PositionProj /= input.PositionProj.w;
  input.LastPositionProj /= input.LastPositionProj.w;

  // Position change relative to the camera clip space.
  float2 delta = input.PositionProj.xy - input.LastPositionProj.xy;
  
  // Convert from clip space to texture space:
  // * 0.5 because clip space is in the range [-1, 1] and texture space in the range [0, 1].
  delta *= 0.5f;
  // Clip space y must be inverted for textures space.
  delta.y *= -1;                                   

  return float4(delta.xy, 0, 1);
}


technique 
{
  pass 
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
