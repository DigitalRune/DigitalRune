//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file MatcapNormalSkinned.fx
/// Renders an object by sampling a surface material ("material capture") using
/// the normal vector.
/// Supports:
/// - Matcap texture
/// - Normal map
/// - Mesh skinning (up to 72 bones)
//
//-----------------------------------------------------------------------------

#define NORMAL_MAP 1
#define SKINNING 1
#include "Matcap.fx"
