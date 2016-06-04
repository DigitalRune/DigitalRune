//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file MaterialAlphaTest.fx
/// Combines the decal material (colors, textures) with the light buffer data.
/// Supports:
/// - Diffuse color/texture
/// - Specular color/texture
/// - Alpha test (mask stored in alpha channel of diffuse texture)
//
//-----------------------------------------------------------------------------

#define ALPHA_TEST 1
#include "Material.fx"
