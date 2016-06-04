//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file TerrainMaterialPom.fx
/// Renders the "Material" render pass of a terrain node with vertex-based holes
/// and parallax occlusion mapping.
//
//-----------------------------------------------------------------------------

#ifndef XBOX
#define PARALLAX_MAPPING 1
#endif
#include "TerrainMaterial.fx"
