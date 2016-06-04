//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file GBufferNormalTransparent.fx
/// Renders the model into the G-buffer.
/// Supports:
/// - Specular power
/// - Normal map
/// - Screen-door transparency
// 
//-----------------------------------------------------------------------------

#define TRANSPARENT 1
#define NORMAL_MAP 1
#include "GBuffer.fx"
