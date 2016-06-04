//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file GBufferAlphaTest.fx
/// Renders the model into the G-buffer.
/// Supports:
/// - Specular power
/// - Alpha test (mask stored in alpha channel of diffuse texture)
//
//-----------------------------------------------------------------------------

#define ALPHA_TEST 1
#include "GBuffer.fx"
