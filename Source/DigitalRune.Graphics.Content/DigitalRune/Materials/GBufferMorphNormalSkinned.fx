//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file GBufferMorphNormalSkinned.fx
/// Renders the model into the G-buffer.
/// Supports:
/// - Specular power
/// - Normal map
/// - Morphing (up to 5 morph targets)
/// - Mesh skinning (up to 72 bones)
//
//-----------------------------------------------------------------------------

#define NORMAL_MAP 1
#define MORPHING 1
#define SKINNING 1
#include "GBuffer.fx"
