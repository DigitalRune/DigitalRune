//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ForwardMorphSkinned.fx
/// Renders a model in a single pass ("forward rendering").
/// Supports:
/// - Morphing (up to 5 morph targets)
/// - Mesh skinning (up to 72 bones)
//
//-----------------------------------------------------------------------------

#define PREMULTIPLIED_ALPHA 1       // Diffuse texture uses premultiplied alpha.
#define DIFFUSE_TEXTURE 1           // Enable diffuse texture.
#define OPACITY_TEXTURE 1           // Enable opacity texture (stored in A channel of diffuse texture).
#define SPECULAR_TEXTURE 1          // Enable specular texture.
#define MORPHING 1                  // Enable morph targets.
#define SKINNING 1                  // Enable mesh skinning.

#include "Forward.fx"
