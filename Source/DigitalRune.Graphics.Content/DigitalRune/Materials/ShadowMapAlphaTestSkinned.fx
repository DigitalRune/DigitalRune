//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ShadowMapAlphaTestSkinned.fx
/// Renders the model into the shadow map.
/// Supports:
/// - Alpha test (mask stored in alpha channel of diffuse texture)
/// - Mesh skinning (up to 72 bones)
//
//-----------------------------------------------------------------------------

#define ALPHA_TEST 1
#define SKINNING 1
#include "ShadowMap.fx"
