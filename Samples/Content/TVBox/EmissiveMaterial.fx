float4x4 WorldView : WORLDVIEW;
float4x4 Projection : PROJECTION;
float CameraFar : CAMERAFAR;
float2 ViewportSize : VIEWPORTSIZE;

float3 DiffuseColor : DIFFUSECOLOR;
float3 SpecularColor : SPECULARCOLOR;
float Exposure;
texture EmissiveTexture : EMISSIVETEXTURE;
sampler EmissiveSampler = sampler_state
{
  Texture = <EmissiveTexture>;
  MAGFILTER = LINEAR;
  MINFILTER = LINEAR;
  MIPFILTER = LINEAR;
  AddressU = WRAP;
  AddressV = WRAP;
};

// Light buffer 0 stores the diffuse lighting.
texture LightBuffer0 : LIGHTBUFFER0;
sampler LightBuffer0Sampler = sampler_state
{
  Texture = <LightBuffer0>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = POINT;
  MagFilter = POINT;
  MipFilter = NONE;
};

// Light buffer 1 stores the specular lighting.
texture LightBuffer1 : LIGHTBUFFER1;
sampler LightBuffer1Sampler = sampler_state
{
  Texture = <LightBuffer1>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = POINT;
  MagFilter = POINT;
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
  float2 TexCoord : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  float4 positionView = mul(input.Position, WorldView);
  float4 position = mul(positionView, Projection);

  VSOutput output = (VSOutput)0;
  output.TexCoord = input.TexCoord;
  output.PositionProj = position;
  output.Position = position;
  return output;
}


float4 PS(float2 texCoord	: TEXCOORD0,
          float4 positionProj : TEXCOORD1) : COLOR0
{
  float3 emissiveMap = tex2D(EmissiveSampler, texCoord).rgb;
  
  // Convert from Gamma to linear space.
  emissiveMap = emissiveMap * emissiveMap;
  
  // Get the screen space texture coordinate for this position.
  float2 texCoordScreen = positionProj.xy / positionProj.w;
  texCoordScreen.xy = (float2(texCoordScreen.x, -texCoordScreen.y) + 1) * 0.5f;
  texCoordScreen.xy += 0.5f / ViewportSize;
  
  float3 diffuseLight = tex2D(LightBuffer0Sampler, texCoordScreen).rgb;
  float3 specularLight = tex2D(LightBuffer1Sampler, texCoordScreen).rgb;
  
  return float4(DiffuseColor * diffuseLight + SpecularColor * specularLight.rgb + emissiveMap * Exposure, 1);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_2_0
#define PSTARGET ps_2_0
#else
#define VSTARGET vs_4_0_level_9_1
#define PSTARGET ps_4_0_level_9_1
#endif

technique
{
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
