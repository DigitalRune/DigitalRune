// Compile on command line
//
//  "%DXSDK_DIR%\Utilities\Bin\x86\fxc.exe" /T ps_2_0 /Fo "ImageEffect.ps" "ImageEffect.fx"
//
// or with pre-build event in project
//
//  "%DXSDK_DIR%\Utilities\Bin\x86\fxc.exe" /T ps_2_0 /Fo "$(ProjectDir)Resources\ImageEffect.ps" "$(ProjectDir)Resources\ImageEffect.fx"

sampler2D InputSampler : register(s0);
float4 Color : register(c0);
float Opacity : register(c1);
float Saturation : register(c2);

static const float3 LuminanceWeights = float3(0.2126, 0.7152, 0.0722);


float4 main(float2 uv : TEXCOORD) : COLOR 
{
  float4 color = tex2D(InputSampler, uv);
  
  // Saturation
  color.rgb = lerp(dot(color.rgb, LuminanceWeights), color.rgb, Saturation);
  
  // Tint color
  color *= Color;
  
  // Opacity
  color *= Opacity;
  
  return color;
}