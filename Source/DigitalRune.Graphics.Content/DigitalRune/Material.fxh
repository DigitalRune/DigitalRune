//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Material.fxh
/// Functions/macros for material properties of rendered objects.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_MATERIAL_FXH
#define DIGITALRUNE_MATERIAL_FXH


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

/// Declares the uniform const for the diffuse texture: DiffuseTexture and DiffuseSampler.
#define DECLARE_UNIFORM_DIFFUSETEXTURE \
texture DiffuseTexture : DIFFUSETEXTURE; \
sampler DiffuseSampler = sampler_state \
{ \
  Texture = <DiffuseTexture>; \
  AddressU = WRAP; \
  AddressV = WRAP; \
  MINFILTER = ANISOTROPIC; \
  MAGFILTER = LINEAR; \
  MIPFILTER = LINEAR; \
};


/// Declares the uniform const for the specular texture: SpecularTexture and SpecularSampler.
#define DECLARE_UNIFORM_SPECULARTEXTURE \
texture SpecularTexture : SPECULARTEXTURE; \
sampler SpecularSampler = sampler_state \
{ \
  Texture = <SpecularTexture>; \
  AddressU = WRAP; \
  AddressV = WRAP; \
  MINFILTER = ANISOTROPIC; \
  MAGFILTER = LINEAR; \
  MIPFILTER = LINEAR; \
};


/// Declares the uniform const for normal mapping: NormalTexture and NormalSampler.
#define DECLARE_UNIFORM_NORMALTEXTURE \
texture NormalTexture : NORMALTEXTURE; \
sampler NormalSampler = sampler_state \
{ \
  Texture = <NormalTexture>; \
  AddressU = WRAP; \
  AddressV = WRAP; \
  MINFILTER = ANISOTROPIC; \
  MAGFILTER = LINEAR; \
  MIPFILTER = LINEAR; \
};


/// Declares the uniform const for height texture (for parallax mapping): HeightTexture and HeightSampler.
#define DECLARE_UNIFORM_HEIGHTTEXTURE \
texture HeightTexture : HEIGHTTEXTURE; \
sampler HeightSampler = sampler_state \
{ \
  Texture = <HeightTexture>; \
  AddressU = WRAP; \
  AddressV = WRAP; \
  MINFILTER = ANISOTROPIC; \
  MAGFILTER = LINEAR; \
  MIPFILTER = LINEAR; \
};

#endif
