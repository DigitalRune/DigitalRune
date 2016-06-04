//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file TerrainMaterialHolesPom.fx
/// Renders the "Material" render pass of a terrain node with pixel-based holes
/// and parallax occlusion mapping.
//
//-----------------------------------------------------------------------------

#ifndef XBOX
#define PIXEL_HOLES 1
#define PARALLAX_MAPPING 1
#endif
#include "TerrainMaterial.fx"
