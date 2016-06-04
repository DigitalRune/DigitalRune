//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Common.fxh
/// Frequently used constants and functions.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_COMMON_FXH
#define DIGITALRUNE_COMMON_FXH


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

// The gamma value.
// Use 2.0 for approximate gamma (default) and 2.2 for exact gamma.
// See also comments in FromGamma/ToGamma().
#ifndef DR_GAMMA
#define DR_GAMMA 2.0
#endif


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The RGB weights to compute luminance with:
//    luminance = dot(color, LuminanceWeights);
// The color must be in linear space (not nonlinear sRGB values).
// Weights according to ITU Rec 601 (standard digital TV).
static const float3 LuminanceWeightsRec601 = float3(0.299, 0.587, 0.114);
// Weights according to ITU Rec 709 (HDTV). Same as sRGB.
static const float3 LuminanceWeightsRec709 = float3(0.2126, 0.7152, 0.0722);
static const float3 LuminanceWeights = LuminanceWeightsRec709;

static const float Pi = 3.1415926535897932384626433832795;

// A bias matrix that converts a projection space vector into texture space.
// (x, y) coordinates in projection space range from (-1, -1) at the bottom left
// to (1, 1) at the top right. For texturing the top left should be (0, 0) and
// the bottom right should be (1, 1).
static const float4x4 ProjectorBiasMatrix = { 0.5,    0, 0, 0,
                                                0, -0.5, 0, 0,
                                                0,    0, 1, 0,
                                              0.5,  0.5, 0, 1 };


/// Declares the uniform const for a jitter map texture + sampler.
/// \param[in] name   The name of the jitter map texture constant.
/// \remarks
/// Example: To declare JitterMap and JitterMapSampler call
///   DECLARE_UNIFORM_JITTERMAP(JitterMap);
#define DECLARE_UNIFORM_JITTERMAP(name) \
Texture2D name : JITTERMAP; \
sampler name##Sampler = sampler_state \
{ \
  Texture = <name>; \
  AddressU  = WRAP; \
  AddressV  = WRAP; \
  MinFilter = POINT; \
  MagFilter = POINT; \
  MipFilter = NONE; \
}


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Converts a color component given in non-linear gamma space to linear space.
/// \param[in] colorGamma  The color component in non-linear gamma space.
/// \return The color component in linear space.
float FromGamma(float colorGamma)
{
  // If DR_GAMMA is not 2, we might have to use pow(0.000001 + ..., ...).
  return pow(colorGamma, DR_GAMMA);
}


/// Converts a color given in non-linear gamma space to linear space.
/// \param[in] colorGamma  The color in non-linear gamma space.
/// \return The color in linear space.
float3 FromGamma(float3 colorGamma)
{
  // If DR_GAMMA is not 2, we might have to use pow(0.000001 + ..., ...).
  return pow(colorGamma, DR_GAMMA);
}


/// Converts a color component given in linear space to non-linear gamma space.
/// \param[in] colorLinear  The color component in linear space.
/// \return The color component in non-linear gamma space.
float ToGamma(float colorLinear)
{
  // If DR_GAMMA is not 2, we might have to use pow(0.000001 + ..., ...).
  return pow(colorLinear, 1 / DR_GAMMA);
}


/// Converts a color given in linear space to non-linear gamma space.
/// \param[in] colorLinear  The color in linear space.
/// \return The color in non-linear gamma space.
float3 ToGamma(float3 colorLinear)
{
  // If DR_GAMMA is not 2, we might have to use pow(0.000001 + ..., ...).
  return pow(colorLinear, 1 / DR_GAMMA);
}


/// Transforms a screen space position to standard projection space.
/// The half pixel offset for correct texture alignment is applied.
/// (Note: This half pixel offset is only necessary in DirectX 9.)
/// \param[in] position     The screen space position in "pixels".
/// \param[in] viewportSize The viewport size in pixels, e.g. (1280, 720).
/// \return The position in projection space.
float2 ScreenToProjection(float2 position, float2 viewportSize)
{
#if !SM4
  // Subtract a half pixel so that the edge of the primitive is between screen pixels.
  // Thus, the first texel lies exactly on the first pixel.
  // See also http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
  // for a good description of this DirectX 9 problem.
  position -= 0.5;
#endif
  
  // Now transform screen space coordinate into projection space.
  // Screen space: Left top = (0, 0), right bottom = (ScreenSize.x - 1, ScreenSize.y - 1).
  // Projection space: Left top = (-1, 1), right bottom = (1, -1).
  
  // Transform into the range [0, 1] x [0, 1].
  position /= viewportSize;
  // Transform into the range [0, 2] x [-2, 0]
  position *= float2(2, -2);
  // Transform into the range [-1, 1] x [1, -1].
  position -= float2(1, -1);
  
  return position;
}


/// Transforms a screen space position to standard projection space.
/// The half pixel offset for correct texture alignment is applied.
/// (Note: This half pixel offset is only necessary in DirectX 9.)
/// \param[in] position     The screen space position in "pixels".
/// \param[in] viewportSize The viewport size in pixels, e.g. (1280, 720).
/// \return The position in projection space.
float4 ScreenToProjection(float4 position, float2 viewportSize)
{
  position.xy = ScreenToProjection(position.xy, viewportSize);
  return position;
}


/// Transforms a position from standard projection space to screen space.
/// The half pixel offset for correct texture alignment is applied.
/// (Note: This half pixel offset is only necessary in DirectX 9.)
/// \param[in] position     The position in projection space.
/// \param[in] viewportSize The viewport size in pixels, e.g. (1280, 720).
/// \return The screen space position in texture coordinates ([0, 1] range).
float2 ProjectionToScreen(float4 position, float2 viewportSize)
{
  // Perspective divide:
  position.xy = position.xy / position.w;
  
  // Convert from range [-1, 1] x [1, -1] to [0, 1] x [0, 1].
  position.xy = float2(0.5, -0.5) * position.xy + float2(0.5, 0.5);
  
  // The position (0, 0) is the center of the first screen pixel. We have
  // to add half a texel to sample the center of the first texel.
#if !SM4
  position.xy += 0.5f / viewportSize;
#endif
  
  return position.xy;
}


/// Converts a clip space depth (normal depth buffer value) back to view space.
/// \param[in] depth  The clip space depth value.
/// \param[in] near   The distance of the camera near plane.
/// \param[in] far    The distance of the camera far plane.
/// \return The depth in view space in the range [near, far].
/// \remarks
/// The returned depth is positive and describes the planar distance from the
/// camera (if it were the z coordinate it would be negative).
/// (Note: This function only works for perspective projections.)
float ConvertDepthFromClipSpaceToViewSpace(float depth, float near, float far)
{
  // This operation inverts the perspective projection matrix and the
  // perspective divide.
  float q = far / (near - far);
  return near * q / (depth + q);
}


//-----------------------------------------------------------------------------
// Texture Mapping
//-----------------------------------------------------------------------------

/// Computes the current mip map level for a texture.
/// \param[in] texCoord      The texture coordinates.
/// \param[in] textureSize   The size of the texture in pixels (width, height).
/// \return The mip map level.
float MipLevel(float2 texCoord, float2 textureSize)
{
  // Note: This code is taken from Shader X5 - Practical Parallax Occlusion Mapping
  // which is similar to the DirectX 2009 Parallax Occlusion Mapping sample code.
  
  // Compute mip map level for texture.
  float2 scaledTexCoord = texCoord * textureSize;
  
  // Compute partial derivatives of the texture coordinates with respect to screen
  // coordinates. The derivatives are an approximation of the pixel's size in texture
  // space.
  float2 dxSize = ddx(scaledTexCoord);
  float2 dySize = ddy(scaledTexCoord);
  
  // Find max of change in u and v across quad: Compute du and dv magnitude across quad.
  float2 dTexCoord = dxSize * dxSize + dySize * dySize;
  float maxTexCoordDelta = max(dTexCoord.x, dTexCoord.y);
  
  // The mip map level k for a given compression value d is such that
  //   2^k <= d < 2^(k+1)
  // Or:
  //   k = floor(log2(d))
  
  // Compute the current mip map level.
  // (The factor 0.5 is effectively computing a square root before log.)
  return max(0.5 * log2(maxTexCoordDelta), 0);
}


/// Computes the current mip map level for a texture and anisotropic filtering.
/// \param[in] texCoord       The texture coordinates.
/// \param[in] textureSize    The size of the texture in pixels (width, height).
/// \param[in] maxAnisotropy  The max anistropy, e.g. 16 for 16xAniso.
/// \return The mip map level.
float MipLevel(float2 texCoord, float2 textureSize, float maxAnisotropy)
{
  // Note: This code is taken from Shader X5 - Practical Parallax Occlusion Mapping
  // which is similar to the DirectX 2009 Parallax Occlusion Mapping sample code.
  
  // Compute mip map level for texture.
  float2 scaledTexCoord = texCoord * textureSize;
  
  // Compute partial derivatives of the texture coordinates with respect to screen
  // coordinates. The derivatives are an approximation of the pixel's size in texture
  // space.
  float2 dxSize = ddx(scaledTexCoord);
  float2 dySize = ddy(scaledTexCoord);
  
  // According to OpenGL specs:
  ////vec2 ddx=dFdx(gl_TexCoord[0].xy);
  ////vec2 ddy=dFdy(gl_TexCoord[0].xy);
  ////float Px = length(ddx);
  ////float Py = length(ddy);
  float Px = length(dxSize);
  float Py = length(dySize);
  float Pmax = max(Px, Py);
  float Pmin = min(Px, Py);
  //float N = min(ceil(Pmax/Pmin), maxAnisotropy);   // OpenGL spec
  float N = min(Pmax/Pmin, maxAnisotropy);     // This actually works and makes more sense.
  float lambda = max(0, log2(Pmax / N));
  return lambda;
}


// Samples the given texture using manual bilinear filtering.
/// \param[in] textureSampler  The texture sampler (which uses POINT filtering).
/// \param[in] texCoord        The texture coordinates.
/// \param[in] textureSize     The size of the texture in pixels (width, height).
/// \return The texture sample.
float4 SampleLinear(sampler textureSampler, float2 texCoord, float2 textureSize)
{
  float2 texelSize = 1.0 / textureSize;
  texCoord -= 0.5 * texelSize;
  float4 s00 = tex2D(textureSampler, texCoord);
  float4 s10 = tex2D(textureSampler, texCoord + float2(texelSize.x, 0));
  float4 s01 = tex2D(textureSampler, texCoord + float2(0, texelSize.y));
  float4 s11 = tex2D(textureSampler, texCoord + texelSize);
  
  float2 texelpos = textureSize * texCoord;
  float2 lerps = frac(texelpos);
  return lerp(lerp(s00, s10, lerps.x), lerp(s01, s11, lerps.x), lerps.y);
}


// Samples the given texture using manual bilinear filtering.
/// \param[in] textureSampler  The texture sampler (which uses POINT filtering).
/// \param[in] texCoord        The texture coordinates for tex2Dlod.
/// \param[in] textureSize     The size of the texture in pixels (width, height).
/// \return The texture sample.
float4 SampleLinearLod(sampler textureSampler, float4 texCoord, float2 textureSize)
{
  // Texel size relative to the mip level:
  float2 texelSize = 1.0 / textureSize * exp2(texCoord.w);
  texCoord.xy -= 0.5 * texelSize;
  float4 s00 = tex2Dlod(textureSampler, float4(texCoord.xy, texCoord.z, texCoord.w));
  float4 s10 = tex2Dlod(textureSampler, float4(texCoord.xy + float2(texelSize.x, 0), texCoord.z, texCoord.w));
  float4 s01 = tex2Dlod(textureSampler, float4(texCoord.xy + float2(0, texelSize.y), texCoord.z, texCoord.w));
  float4 s11 = tex2Dlod(textureSampler, float4(texCoord.xy + texelSize, texCoord.z, texCoord.w));
  
  float2 texelpos = textureSize * texCoord.xy;
  float2 lerps = frac(texelpos);
  return lerp(lerp(s00, s10, lerps.x), lerp(s01, s11, lerps.x), lerps.y);
}

// Samples the given texture using manual trilinear filtering. (Not fully tested!!!)
/// \param[in] textureSampler  The texture sampler (which uses POINT filtering).
/// \param[in] texCoord        The texture coordinates for tex2Dlod.
/// \param[in] textureSize     The size of the texture in pixels (width, height).
/// \return The texture sample.
float4 SampleTrilinear(sampler textureSampler, float2 texCoord, float2 textureSize)
{
  float mipLevel = MipLevel(texCoord, textureSize);
  float minMipLevel = (int)mipLevel;
  float maxMipLevel = minMipLevel + 1;
  float lerpParameter = frac(mipLevel);
  
  return lerp(SampleLinearLod(textureSampler, float4(texCoord.xy, 0, minMipLevel), textureSize),
              SampleLinearLod(textureSampler, float4(texCoord.xy, 0, maxMipLevel), textureSize),
              lerpParameter);
}


/// Helps to visualize the resolution of a texture for debugging. This function
/// can be used to draw grid lines between the texels or to draw a checkerboard
/// pattern.
/// \param[in] texCoord    The current texture coordinate.
/// \param[in] textureSize The size of the texture as float2(width, height).
/// \param[in] gridSize    The relative size of the grid lines in percent, for
///                        instance 0.02. If this value is 0 or negative a
///                        checkerboard pattern is created instead of a grid.
///  \return  0 if a dark pixel should be drawn for this texture coordinate;
///           otherwise 1.
float VisualizeTextureGrid(float2 texCoord, float2 textureSize, float gridSize)
{
  if (gridSize > 0)
  {
    float2 t = frac(texCoord * textureSize);
    return (t.x < gridSize || t.x > 1 - gridSize)
           || (t.y < gridSize || t.y > 1 - gridSize);
  }
  else
  {
    int2 t = int2(texCoord * textureSize);
    return (t.x % 2 == 0 && t.y % 2 == 0)
           || (t.x % 2 == 1 && t.y % 2 == 1);
  }
}


//-----------------------------------------------------------------------------
// Normal Mapping
//-----------------------------------------------------------------------------

/// Gets the normal vector from a standard normal texture (no special encoding).
/// \param[in] normalSampler The sampler for the normal texture.
/// \param[in] texCoord      The texture coordinates.
/// \return The normalized normal.
float3 GetNormal(sampler normalSampler, float2 texCoord)
{
  float3 normal = tex2D(normalSampler, texCoord).xyz * 255/128 - 1;
  normal = normalize(normal);
  return normal;
}


/// Gets the normal vector from a normal texture which uses DXT5nm encoding.
/// \param[in] normalSampler The sampler for the normal texture.
/// \param[in] texCoord      The texture coordinates.
/// \return The normalized normal.
float3 GetNormalDxt5nm(sampler normalSampler, float2 texCoord)
{
  float3 normal;
  normal.xy = tex2D(normalSampler, texCoord).ag * 255/128 - 1;
  normal.z = sqrt(1.0 - dot(normal.xy, normal.xy));
  return normal;
}


/// Gets the normal vector from a normal texture which uses DXT5nm encoding.
/// \param[in] normalSampler The sampler for the normal texture.
/// \param[in] texCoord      The texture coordinates.
/// \param[in] ddxTexCoord   ddx(texCoord)
/// \param[in] ddyTexCoord   ddx(texCoord)
/// \return The normalized normal.
float3 GetNormalDxt5nm(sampler normalSampler, float2 texCoord, float2 ddxTexCoord, float2 ddyTexCoord)
{
  float3 normal;
  normal.xy = tex2Dgrad(normalSampler, texCoord, ddxTexCoord, ddyTexCoord).ag * 255/128 - 1;
  normal.z = sqrt(1.0 - dot(normal.xy, normal.xy));
  return normal;
}


/// Computes the cotangent frame.
/// \param[in] n   The (normalized) normal vector.
/// \param[in] p   The position.
/// \param[in] uv  The texture coordinates.
/// \return The cotangent frame.
/// \remarks
/// For reference see http://www.thetenthplanet.de/archives/1180.
/// Example: To convert a normal n from a normal map to world space.
///  float3x3 cotangentFrame = CotangentFrame(normalWorld, positionWorld, texCoord);
///  n.y = -n.y;   // Invert y for our standard "green-up" normal maps.
///  nWorld = mul(n, cotangentFrame);
float3x3 CotangentFrame(float3 n, float3 p, float2 uv)
{
  // Get edge vectors of the pixel triangle.
  float3 dp1 = ddx(p);
  float3 dp2 = ddy(p);
  float2 duv1 = ddx(uv);
  float2 duv2 = ddy(uv);
  
  // ----- Original
  // Solve the linear system.
  //float3x3 M = float3x3(dp1, dp2, cross(dp1, dp2));
  //float3x3 inverseM = Invert(M);
  //float3 T = mul(inverseM, float3(duv1.x, duv2.x, 0));
  //float3 B = mul(inverseM, float3(duv1.y, duv2.y, 0));
  
  // ----- Optimized
  float3 dp2perp = cross(n, dp2);
  float3 dp1perp = cross(dp1, n);
  float3 t = dp2perp * duv1.x + dp1perp * duv2.x;
  float3 b = dp2perp * duv1.y + dp1perp * duv2.y;
  
  // Construct a scale-invariant frame.
  float invmax = rsqrt(max(dot(t, t), dot(b, b)));
  return float3x3(t * invmax, b * invmax, n);
}


//-----------------------------------------------------------------------------
// Util
//-----------------------------------------------------------------------------

/// Creates a right-handed, look-at matrix.
/// \param[in] eyePosition     Eye position (position of the viewer).
/// \param[in] targetPosition  The point where the viewer is looking at.
/// \param[in] upVector        The up-vector of the viewer.
/// \return The look-at matrix.
float4x4 CreateLookAt(float3 eyePosition, float3 targetPosition, float3 upVector)
{
  float3 zAxis = normalize(eyePosition - targetPosition);
  float3 xAxis = normalize(cross(upVector, zAxis));
  float3 yAxis = cross(zAxis, xAxis);
  
  float4x4 view =
  {
    xAxis.x, yAxis.x, zAxis.x, 0,
    xAxis.y, yAxis.y, zAxis.y, 0,
    xAxis.z, yAxis.z, zAxis.z, 0,
    -dot(xAxis, eyePosition), -dot(yAxis, eyePosition), -dot(zAxis, eyePosition), 1
  };
  
  return view;
}


/// Computes an orthnormal base for the given vector.
/// \param[in] n        A normalized vector.
/// \param[out] b1      A normalized vector orthogonal to n.
/// \param[out] b2      A normalized vector orthogonal to n and b1.
void GetOrthonormals(float3 n, out float3 b1, out float3 b2)
{
  // This method was presented in
  // "Building an Orthonormal Basis from a 3D Unit Vector Without Normalization"
  // http://orbit.dtu.dk/fedora/objects/orbit:113874/datastreams/file_75b66578-222e-4c7d-abdf-f7e255100209/content
  
  if(n.z < -0.9999999) // Handle the singularity.
  {
    b1 = float3(0, -1, 0);
    b2 = float3(-1, 0, 0);
    return;
  }
  
  float a = 1 / (1 + n.z);
  float b = -n.x * n.y * a ;
  b1 = float3(1 - n.x * n.x * a, b, -n.x);
  b2 = float3(b , 1 - n.y * n.y * a, -n.y);
}


/// Computes where a ray hits a sphere (which is centered at the origin).
/// \param[in]  rayOrigin    The start position of the ray.
/// \param[in]  rayDirection The normalized direction of the ray.
/// \param[in]  radius       The radius of the sphere.
/// \param[out] enter        The ray parameter where the ray enters the sphere.
///                          0 if the ray is already in the sphere.
/// \param[out] exit         The ray parameter where the ray exits the sphere.
/// \return  0 or a positive value if the ray hits the sphere. A negative value
///          if the ray does not touch the sphere.
float HitSphere(float3 rayOrigin, float3 rayDirection, float radius, out float enter, out float exit)
{
  // Solve the equation:  ||rayOrigin + distance * rayDirection|| = r
  //
  // This is a straight-forward quadratic equation:
  //   ||O + d * D|| = r
  //   =>  (O + d * D)² = r²  where V² means V.V
  //   =>  d² * D² + 2 * d * (O.D) + O² - r² = 0
  // D² is 1 because the rayDirection is normalized.
  //   =>  d = -O.D + sqrt((O.D)² - O² + r²)
  
  float OD = dot(rayOrigin, rayDirection);
  float OO = dot(rayOrigin, rayOrigin);
  float radicand = OD * OD - OO + radius * radius;
  enter = max(0, -OD - sqrt(radicand));
  exit = -OD + sqrt(radicand);
  
  return radicand;  // If radicand is negative then we do not have a result - no hit.
}


/// Inverts the specified 3x3 matrix.
/// \param[in] m     The 3x3 matrix to be inverted.
/// \return The inverse of the matrix.
float3x3 Invert(float3x3 m)
{
  float det = determinant(m); // = dot(cross(m[0], m[1]), m[2]);
  float3x3 t = transpose(m);
  float3x3 adjugate = float3x3(cross(t[1], t[2]),
                               cross(t[2], t[0]),
                               cross(t[0], t[1]));
  return adjugate / det;
}


/// Checks if the given vector elements are all in the range [min, max].
/// \param[in] x    The vector that should be checked.
/// \param[in] min  The minimal allowed range.
/// \param[in] max  The maximal allowed range.
/// \return True if all elements of x are in the range [min, max].
bool IsInRange(float3 x, float min, float max)
{
  return all(clamp(x, min, max) == x);
}


/// Checks if the given vector elements are all in the range [min, max].
/// \param[in] x    The vector that should be checked.
/// \param[in] min  The minimal allowed range.
/// \param[in] max  The maximal allowed range.
/// \return True if all elements of x are in the range [min, max].
bool IsInRange(float4 x, float min, float max)
{
  return all(clamp(x, min, max) == x);
}


/// Returns a linear interpolation betwenn 0 and 1 if x is in the range [min, max].
/// This does the same as the HLSL intrinsic function smoothstep() - but without
/// a smooth curve.
///  min  The minimum range of the x parameter.
///  max  The maximum range of the x parameter.
///  x    The specified value to be interpolated.
///  Returns 0 if x is less than min;
///  1 if x is greater than max;
///  otherwise, a value between 0 and 1 if x is in the range [min, max].
float LinearStep(float min, float max, float x)
{
  float y = (x - min) / (max - min);
  return clamp(y, 0, 1);
}


/// Calculates the logarithm for a given y and base, such that base^x = y.
/// param[in] base    The base of the logarithm.
/// param[in] y       The number of which to calculate the logarithm.
/// \return The logarithm of y.
float Log(float base, float y)
{
  return log(y) / log(base);
}
#endif
