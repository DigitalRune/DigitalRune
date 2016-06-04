//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file MaterialMorphSkinned.fx
/// Combines the material of a model (e.g. textures) with the light buffer data.
/// Supports:
/// - Diffuse color/texture
/// - Specular color/texture
/// - Morphing (up to 5 morph targets)
/// - Mesh skinning (up to 72 bones)
//
//-----------------------------------------------------------------------------

#define MORPHING 1
#define SKINNING 1
#include "Material.fx"
