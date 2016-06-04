//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file OccluderSkinned.fx
/// Renders the model as an occluder into the occlusion buffer.
/// Supports:
/// - Mesh skinning (up to 72 bones)
//
//-----------------------------------------------------------------------------

#define SKINNING 1
#include "Occluder.fx"
