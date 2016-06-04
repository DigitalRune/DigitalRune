//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ForwardTwoSided.fx
/// Renders a model in a single pass ("forward rendering").
/// Supports:
/// - Diffuse color/texture
/// - Specular color/texture
/// - Specular power
/// - Alpha blending
/// - Two-sided materials
//
//-----------------------------------------------------------------------------

#define TWO_SIDED 1
#define PREMULTIPLIED_ALPHA 1   // Diffuse texture uses premultiplied alpha.
#define DIFFUSE_TEXTURE 1           // Enable diffuse texture.
#define OPACITY_TEXTURE 1           // Enable opacity texture (stored in A channel of diffuse texture).
#define SPECULAR_TEXTURE 1          // Enable specular texture.

#include "Forward.fx"
