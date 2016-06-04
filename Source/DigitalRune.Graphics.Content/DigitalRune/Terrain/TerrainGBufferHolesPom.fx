//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file TerrainGBufferHolesPom.fx
/// Renders the "GBuffer" render pass of a terrain node with pixel-based holes
/// and parallax occlusion mapping.
//
//-----------------------------------------------------------------------------

#define PIXEL_HOLES 1
#define PARALLAX_MAPPING 1
#include "TerrainGBuffer.fx"
