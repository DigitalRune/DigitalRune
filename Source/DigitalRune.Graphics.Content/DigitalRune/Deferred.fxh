//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Deferred.fxh
/// Functions for deferred rendering (e.g. G-buffer and light buffer access).
/// 
/// If you are creating the G-buffer, you need to define
///   #define CREATE_GBUFFER 1
/// before including Deferred.fxh.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_DEFERRED_FXH
#define DIGITALRUNE_DEFERRED_FXH

#ifndef DIGITALRUNE_ENCODING_FXH
#error "Encoding.fxh required. Please include Encoding.fxh before including Deferred.fxh."
#endif


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

#if CREATE_GBUFFER
DECLARE_UNIFORM_NORMALSFITTINGTEXTURE
#endif


/// Declares the uniform const for the view frustum far corners in view space.
/// \param[in] name   The name of the constant.
/// \remarks
/// Order of the corners: (top left, top right, bottom left, bottom right)
/// Usually you will call
///   DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);
#define DECLARE_UNIFORM_FRUSTUMCORNERS(name) float3 name[4]


/// Declares the uniform const for the view frustum info for reconstructing the
/// view space position from texture coordinates.
/// \param[in] name   The name of the constant.
/// \remarks
/// The const values are:
/// (Left / Near, Top / Near, (Right - Left) / Near, (Bottom - Top) / Near)
#define DECLARE_UNIFORM_FRUSTUMINFO(name) float4 name


/// Declares the uniform const for a G-buffer texture + sampler.
/// \param[in] name   The name of the texture constant.
/// \param[in] index  The index of the G-buffer.
/// \remarks
/// Example: To declare GBuffer0 and GBuffer0Sampler call
///   DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
/// Usually you will use
///  DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
///  DECLARE_UNIFORM_GBUFFER(GBuffer1, 1);
#define DECLARE_UNIFORM_GBUFFER(name, index) \
texture name : GBUFFER##index; \
sampler name##Sampler = sampler_state \
{ \
  Texture = <name>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MinFilter = POINT; \
  MagFilter = POINT; \
  MipFilter = NONE; \
}


/// Declares the uniform const for a light buffer texture + sampler.
/// \param[in] name   The name of the light buffer constant.
/// \param[in] index  The index of the light buffer.
/// \remarks
/// Example: To declare LightBuffer0 and LightBuffer0Sampler call
///   DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
/// Usually you will use
///  DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
///  DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer1, 1);
#define DECLARE_UNIFORM_LIGHTBUFFER(name, index) \
texture name : LIGHTBUFFER##index; \
sampler name##Sampler = sampler_state \
{ \
  Texture = <name>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MinFilter = POINT; \
  MagFilter = POINT; \
  MipFilter = NONE; \
}


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Gets the linear depth in the range [0,1] from a G-buffer 0 sample.
/// \param[in] gBuffer0    The G-buffer 0 value.
/// \return The linear depth in the range [0, 1].
float GetGBufferDepth(float4 gBuffer0)
{
  return abs(gBuffer0.x);
}


#if CREATE_GBUFFER
/// Stores the depth in the given G-buffer 0 value.
/// \param[in] depth          The linear depth in the range [0, 1].
/// \param[in] sceneNodeType  The scene node type info (1 = static, 0 = dynamic).
/// \param[in,out] gBuffer0   The G-buffer 0 value.
void SetGBufferDepth(float depth, float sceneNodeType, inout float4 gBuffer0)
{
  if (sceneNodeType)
  {
    // Static objects are encoded as positive values.
    gBuffer0 = depth;
  }
  else
  {
    // Dynamic objects are encoded as negative values.
    gBuffer0 = -depth;
  }
}
#endif


/// Gets the world space normal from a G-buffer 1 sample.
/// \param[in] gBuffer1    The G-buffer 1 value.
/// \return The normal in world space.
float3 GetGBufferNormal(float4 gBuffer1)
{
  return DecodeNormalBestFit((half4)gBuffer1);
  
  //float x = gBuffer1.r * 2 - 1;
  //float y = gBuffer1.g * 2 - 1;
  //return float3(x, y, sqrt(1 - x*x - y*y));
  
  //return normalize(DecodeNormalStereographic(gBuffer1));
  //return normalize(DecodeNormalSphereMap(gBuffer1));
}

#if CREATE_GBUFFER
/// Stores the world space normal in the given G-buffer 1 value.
/// \param[in] normal         The normal in world space.
/// \param[in,out] gBuffer1   The G-buffer 1 value.
void SetGBufferNormal(float3 normal, inout float4 gBuffer1)
{
  gBuffer1.rgb = EncodeNormalBestFit((half3)normal, NormalsFittingSampler);
  
  //gBuffer1.rgb = normal.xyz * 0.5f + 0.5f;
  
  // Note: GBuffer now encodes normal in world space. Does this work for these
  // encodings?
  //gBuffer1.rg = EncodeNormalStereographic(normal);
  //gBuffer1.rg = EncodeNormalSphereMap(normal);
}
#endif


/// Gets the specular power from the given G-buffer samples.
/// \param[in] gBuffer0    The G-buffer 0 value.
/// \param[in] gBuffer1    The G-buffer 1 value.
/// \return The specular power.
float GetGBufferSpecularPower(float4 gBuffer0, float4 gBuffer1)
{
  return DecodeSpecularPower(gBuffer1.a);
}

/// Stores the given specular power in the G-buffer.
/// \param[in] specularPower  The specular power.
/// \param[in,out] gBuffer0   The G-buffer 0 value.
/// \param[in,out] gBuffer1   The G-buffer 1 value.
void SetGBufferSpecularPower(float specularPower, inout float4 gBuffer0, inout float4 gBuffer1)
{
  gBuffer1.a = EncodeSpecularPower(specularPower);
}

/// Gets the diffuse light value from the given light buffer samples.
/// \param[in] lightBuffer0   The light buffer 0 value.
/// \param[in] lightBuffer1   The light buffer 1 value.
/// \return The diffuse light value.
float3 GetLightBufferDiffuse(float4 lightBuffer0, float4 lightBuffer1)
{
  return lightBuffer0.xyz;
}


/// Gets the specular light value from the given light buffer samples.
/// \param[in] lightBuffer0   The light buffer 0 value.
/// \param[in] lightBuffer1   The light buffer 1 value.
/// \return The specular light value.
float3 GetLightBufferSpecular(float4 lightBuffer0, float4 lightBuffer1)
{
  return lightBuffer1.xyz;
}


/// Gets the index of the given texture corner.
/// \param[in] texCoord The texture coordinate of one of the texture corners.
///                     Allowed values are (0, 0), (1, 0), (0, 1), and (1, 1).
/// \return The index of the texture corner.
/// \retval 0   left, top
/// \retval 1   right, top
/// \retval 2   left, bottom
/// \retval 3   right, bottom
float GetCornerIndex(in float2 texCoord)
{
  return texCoord.x + (texCoord.y * 2);
}


struct VSFrustumRayInput
{
  float4 Position : POSITION0;
  float2 TexCoord : TEXCOORD0;    // The texture coordinate of one of the texture corners.
                                  // Allowed values are (0, 0), (1, 0), (0, 1), and (1, 1).
};

struct VSFrustumRayOutput
{
  float2 TexCoord : TEXCOORD0;    // The texture coordinates of the vertex.
  float3 FrustumRay : TEXCOORD1;
  float4 Position : SV_Position;
};

/// A vertex shader that also converts the position from screen space for clip space and computes
/// the frustum ray for this vertex.
/// \param[in] input            The vertex data (see VSFrustumRayInput).
/// \param[in] viewportSize     The viewport size in pixels.
/// \param[in] frustumCorners   See constant FrustumCorners above.
VSFrustumRayOutput VSFrustumRay(VSFrustumRayInput input,
                                uniform const float2 viewportSize,
                                uniform const float3 frustumCorners[4])
{
  float4 position = input.Position;
  float2 texCoord = input.TexCoord;
  
  position.xy /= viewportSize;
  
  texCoord.xy = position.xy;
  
  // Instead of subtracting the 0.5 pixel offset from the position, we add
  // it to the texture coordinates - because frustumRay is associated with
  // the position output.
#if !SM4
  texCoord.xy += 0.5f / viewportSize;
#endif
  
  position.xy = position.xy * float2(2, -2) - float2(1, -1);
  
  VSFrustumRayOutput output = (VSFrustumRayOutput)0;
  output.Position = position;
  output.TexCoord = texCoord;
  output.FrustumRay = frustumCorners[GetCornerIndex(input.TexCoord)];
  
  return output;
}


/// Reconstructs the position in view space.
/// \param[in]  texCoord    The texture coordinates of the current pixel.
/// \param[in]  depth       The depth [0, Far] of the current pixel in view space.
/// \param[in]  frustumInfo The frustum info. See DECLARE_UNIFORM_FRUSTUMINFO().
/// \return The position in view space.
float3 GetPositionView(float2 texCoord, float depth, float4 frustumInfo)
{
  float3 frustumRay = float3(frustumInfo.x + texCoord.x * frustumInfo.z,
                             frustumInfo.y + texCoord.y * frustumInfo.w,
                             -1);
  return depth * frustumRay;
}


/// Reconstructs the normal from the given position.
/// \param[in]  positionView  The position in.
/// \return The normal in view space.
/// \remarks
/// The function returns the normal of the same space as the given position
/// (e.g. world space or view space).
float3 DeriveNormal(float3 position)
{
  return normalize(cross(ddy(position), ddx(position)));
}
#endif
