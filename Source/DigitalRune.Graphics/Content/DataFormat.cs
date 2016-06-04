// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Defines a graphics resource data format.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The first constants (<see cref="Unknown"/> to <see cref="B4G4R4A4_UNORM"/>) match the
  /// Direct3D 11 constants (DXGI_FORMAT_*). It is possible to cast this values to/from
  /// <strong>SharpDX.DXGI.Format</strong>.
  /// </para>
  /// <para>
  /// The constants above <see cref="B4G4R4A4_UNORM"/> are platform-specific and may not be
  /// supported in Direct3D.
  /// </para>
  /// </remarks>
  internal enum DataFormat
  {
    // Disable warning: "Missing XML comment..."
#pragma warning disable 1591

                                // From dxgiformat.h
    Unknown,                    // DXGI_FORMAT_UNKNOWN = 0
    R32G32B32A32_TYPELESS,      // DXGI_FORMAT_R32G32B32A32_TYPELESS = 1
    R32G32B32A32_FLOAT,         // DXGI_FORMAT_R32G32B32A32_FLOAT = 2
    R32G32B32A32_UINT,          // DXGI_FORMAT_R32G32B32A32_UINT = 3
    R32G32B32A32_SINT,          // DXGI_FORMAT_R32G32B32A32_SINT = 4,
    R32G32B32_TYPELESS,         // DXGI_FORMAT_R32G32B32_TYPELESS = 5,
    R32G32B32_FLOAT,            // DXGI_FORMAT_R32G32B32_FLOAT = 6
    R32G32B32_UINT,             // DXGI_FORMAT_R32G32B32_UINT = 7
    R32G32B32_SINT,             // DXGI_FORMAT_R32G32B32_SINT = 8
    R16G16B16A16_TYPELESS,      // DXGI_FORMAT_R16G16B16A16_TYPELESS = 9
    R16G16B16A16_FLOAT,         // DXGI_FORMAT_R16G16B16A16_FLOAT = 10,
    R16G16B16A16_UNORM,         // DXGI_FORMAT_R16G16B16A16_UNORM = 11,
    R16G16B16A16_UINT,          // DXGI_FORMAT_R16G16B16A16_UINT = 12,
    R16G16B16A16_SNORM,         // DXGI_FORMAT_R16G16B16A16_SNORM = 13,
    R16G16B16A16_SINT,          // DXGI_FORMAT_R16G16B16A16_SINT = 14,
    R32G32_TYPELESS,            // DXGI_FORMAT_R32G32_TYPELESS = 15,
    R32G32_FLOAT,               // DXGI_FORMAT_R32G32_FLOAT = 16,
    R32G32_UINT,                // DXGI_FORMAT_R32G32_UINT = 17,
    R32G32_SINT,                // DXGI_FORMAT_R32G32_SINT = 18,
    R32G8X24_TYPELESS,          // DXGI_FORMAT_R32G8X24_TYPELESS = 19,
    D32_FLOAT_S8X24_UINT,       // DXGI_FORMAT_D32_FLOAT_S8X24_UINT = 20,
    R32_FLOAT_X8X24_TYPELESS,   // DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS = 21,
    X32_TYPELESS_G8X24_UINT,    // DXGI_FORMAT_X32_TYPELESS_G8X24_UINT = 22,
    R10G10B10A2_TYPELESS,       // DXGI_FORMAT_R10G10B10A2_TYPELESS = 23,
    R10G10B10A2_UNORM,          // DXGI_FORMAT_R10G10B10A2_UNORM = 24,
    R10G10B10A2_UINT,           // DXGI_FORMAT_R10G10B10A2_UINT = 25,
    R11G11B10_FLOAT,            // DXGI_FORMAT_R11G11B10_FLOAT = 26,
    R8G8B8A8_TYPELESS,          // DXGI_FORMAT_R8G8B8A8_TYPELESS = 27,
    R8G8B8A8_UNORM,             // DXGI_FORMAT_R8G8B8A8_UNORM = 28,
    R8G8B8A8_UNORM_SRGB,        // DXGI_FORMAT_R8G8B8A8_UNORM_SRGB = 29,
    R8G8B8A8_UINT,              // DXGI_FORMAT_R8G8B8A8_UINT = 30,
    R8G8B8A8_SNORM,             // DXGI_FORMAT_R8G8B8A8_SNORM = 31,
    R8G8B8A8_SINT,              // DXGI_FORMAT_R8G8B8A8_SINT = 32,
    R16G16_TYPELESS,            // DXGI_FORMAT_R16G16_TYPELESS = 33,
    R16G16_FLOAT,               // DXGI_FORMAT_R16G16_FLOAT = 34,
    R16G16_UNORM,               // DXGI_FORMAT_R16G16_UNORM = 35,
    R16G16_UINT,                // DXGI_FORMAT_R16G16_UINT = 36,
    R16G16_SNORM,               // DXGI_FORMAT_R16G16_SNORM = 37,
    R16G16_SINT,                // DXGI_FORMAT_R16G16_SINT = 38,
    R32_TYPELESS,               // DXGI_FORMAT_R32_TYPELESS = 39,
    D32_FLOAT,                  // DXGI_FORMAT_D32_FLOAT = 40,
    R32_FLOAT,                  // DXGI_FORMAT_R32_FLOAT = 41,
    R32_UINT,                   // DXGI_FORMAT_R32_UINT = 42,
    R32_SINT,                   // DXGI_FORMAT_R32_SINT = 43,
    R24G8_TYPELESS,             // DXGI_FORMAT_R24G8_TYPELESS = 44,
    D24_UNORM_S8_UINT,          // DXGI_FORMAT_D24_UNORM_S8_UINT = 45,
    R24_UNORM_X8_TYPELESS,      // DXGI_FORMAT_R24_UNORM_X8_TYPELESS = 46,
    X24_TYPELESS_G8_UINT,       // DXGI_FORMAT_X24_TYPELESS_G8_UINT = 47,
    R8G8_TYPELESS,              // DXGI_FORMAT_R8G8_TYPELESS = 48,
    R8G8_UNORM,                 // DXGI_FORMAT_R8G8_UNORM = 49,
    R8G8_UINT,                  // DXGI_FORMAT_R8G8_UINT = 50,
    R8G8_SNORM,                 // DXGI_FORMAT_R8G8_SNORM = 51,
    R8G8_SINT,                  // DXGI_FORMAT_R8G8_SINT = 52,
    R16_TYPELESS,               // DXGI_FORMAT_R16_TYPELESS = 53,
    R16_FLOAT,                  // DXGI_FORMAT_R16_FLOAT = 54,
    D16_UNORM,                  // DXGI_FORMAT_D16_UNORM = 55,
    R16_UNORM,                  // DXGI_FORMAT_R16_UNORM = 56,
    R16_UINT,                   // DXGI_FORMAT_R16_UINT = 57,
    R16_SNORM,                  // DXGI_FORMAT_R16_SNORM = 58,
    R16_SINT,                   // DXGI_FORMAT_R16_SINT = 59,
    R8_TYPELESS,                // DXGI_FORMAT_R8_TYPELESS = 60,
    R8_UNORM,                   // DXGI_FORMAT_R8_UNORM = 61,
    R8_UINT,                    // DXGI_FORMAT_R8_UINT = 62,
    R8_SNORM,                   // DXGI_FORMAT_R8_SNORM = 63,
    R8_SINT,                    // DXGI_FORMAT_R8_SINT = 64,
    A8_UNORM,                   // DXGI_FORMAT_A8_UNORM = 65,
    R1_UNORM,                   // DXGI_FORMAT_R1_UNORM = 66,
    R9G9B9E5_SHAREDEXP,         // DXGI_FORMAT_R9G9B9E5_SHAREDEXP = 67,
    R8G8_B8G8_UNORM,            // DXGI_FORMAT_R8G8_B8G8_UNORM = 68,
    G8R8_G8B8_UNORM,            // DXGI_FORMAT_G8R8_G8B8_UNORM = 69,
    BC1_TYPELESS,               // DXGI_FORMAT_BC1_TYPELESS = 70,
    BC1_UNORM,                  // DXGI_FORMAT_BC1_UNORM = 71,
    BC1_UNORM_SRGB,             // DXGI_FORMAT_BC1_UNORM_SRGB = 72,
    BC2_TYPELESS,               // DXGI_FORMAT_BC2_TYPELESS = 73,
    BC2_UNORM,                  // DXGI_FORMAT_BC2_UNORM = 74,
    BC2_UNORM_SRGB,             // DXGI_FORMAT_BC2_UNORM_SRGB = 75,
    BC3_TYPELESS,               // DXGI_FORMAT_BC3_TYPELESS = 76,
    BC3_UNORM,                  // DXGI_FORMAT_BC3_UNORM = 77,
    BC3_UNORM_SRGB,             // DXGI_FORMAT_BC3_UNORM_SRGB = 78,
    BC4_TYPELESS,               // DXGI_FORMAT_BC4_TYPELESS = 79,
    BC4_UNORM,                  // DXGI_FORMAT_BC4_UNORM = 80,
    BC4_SNORM,                  // DXGI_FORMAT_BC4_SNORM = 81,
    BC5_TYPELESS,               // DXGI_FORMAT_BC5_TYPELESS = 82,
    BC5_UNORM,                  // DXGI_FORMAT_BC5_UNORM = 83,
    BC5_SNORM,                  // DXGI_FORMAT_BC5_SNORM = 84,
    B5G6R5_UNORM,               // DXGI_FORMAT_B5G6R5_UNORM = 85,
    B5G5R5A1_UNORM,             // DXGI_FORMAT_B5G5R5A1_UNORM = 86,
    B8G8R8A8_UNORM,             // DXGI_FORMAT_B8G8R8A8_UNORM = 87,
    B8G8R8X8_UNORM,             // DXGI_FORMAT_B8G8R8X8_UNORM = 88,
    R10G10B10_XR_BIAS_A2_UNORM, // DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM = 89,
    B8G8R8A8_TYPELESS,          // DXGI_FORMAT_B8G8R8A8_TYPELESS = 90,
    B8G8R8A8_UNORM_SRGB,        // DXGI_FORMAT_B8G8R8A8_UNORM_SRGB = 91,
    B8G8R8X8_TYPELESS,          // DXGI_FORMAT_B8G8R8X8_TYPELESS = 92,
    B8G8R8X8_UNORM_SRGB,        // DXGI_FORMAT_B8G8R8X8_UNORM_SRGB = 93,
    BC6H_TYPELESS,              // DXGI_FORMAT_BC6H_TYPELESS = 94,
    BC6H_UF16,                  // DXGI_FORMAT_BC6H_UF16 = 95,
    BC6H_SF16,                  // DXGI_FORMAT_BC6H_SF16 = 96,
    BC7_TYPELESS,               // DXGI_FORMAT_BC7_TYPELESS = 97,
    BC7_UNORM,                  // DXGI_FORMAT_BC7_UNORM = 98,
    BC7_UNORM_SRGB,             // DXGI_FORMAT_BC7_UNORM_SRGB = 99,
    AYUV,                       // DXGI_FORMAT_AYUV = 100
    Y410,                       // DXGI_FORMAT_Y410 = 101
    Y416,                       // DXGI_FORMAT_Y416 = 102
    NV12,                       // DXGI_FORMAT_NV12 = 103
    P010,                       // DXGI_FORMAT_P010 = 104
    P016,                       // DXGI_FORMAT_P016 = 105
    Y420_OPAQUE,                // DXGI_FORMAT_420_OPAQUE = 106
    YUY2,                       // DXGI_FORMAT_YUY2 = 107
    Y210,                       // DXGI_FORMAT_Y210 = 108
    Y216,                       // DXGI_FORMAT_Y216 = 109
    NV11,                       // DXGI_FORMAT_NV11 = 110
    AI44,                       // DXGI_FORMAT_AI44 = 111
    IA44,                       // DXGI_FORMAT_IA44 = 112
    P8,                         // DXGI_FORMAT_P8 = 113
    A8P8,                       // DXGI_FORMAT_A8P8 = 114
    B4G4R4A4_UNORM,             // DXGI_FORMAT_B4G4R4A4_UNORM = 115,

    // Xbox One platform specific types:
    R10G10B10_7E3_A2_FLOAT,         // XBOX_DXGI_FORMAT_R10G10B10_7E3_A2_FLOAT DXGI_FORMAT = 116
    R10G10B10_6E4_A2_FLOAT,         // XBOX_DXGI_FORMAT_R10G10B10_6E4_A2_FLOAT DXGI_FORMAT = 117
    D16_UNORM_S8_UINT,              // XBOX_DXGI_FORMAT_D16_UNORM_S8_UINT DXGI_FORMAT = 118
    R16_UNORM_X8_TYPELESS,          // XBOX_DXGI_FORMAT_R16_UNORM_X8_TYPELESS DXGI_FORMAT = 119
    X16_TYPELESS_G8_UINT,           // XBOX_DXGI_FORMAT_X16_TYPELESS_G8_UINT DXGI_FORMAT = 120
    R10G10B10_SNORM_A2_UNORM = 189, // XBOX_DXGI_FORMAT_R10G10B10_SNORM_A2_UNORM DXGI_FORMAT(189)
    R4G4_UNORM = 190,               // XBOX_DXGI_FORMAT_R4G4_UNORM = 190

    // JPEG Hardware decode formats (DXGI 1.4)
    P208 = 130,                 // WIN10_DXGI_FORMAT_P208 = 130
    V208 = 131,                 // WIN10_DXGI_FORMAT_V208 = 131
    V408 = 132,                 // WIN10_DXGI_FORMAT_V408 = 132

    // ----- Non-DirectX formats:
    // PowerVR texture compression (iOS and some Android devices that use PowerVR GPUs):
    PVRTCI_2bpp_RGB = 1000,
    PVRTCI_4bpp_RGB,
    PVRTCI_2bpp_RGBA,
    PVRTCI_4bpp_RGBA,

    // Ericcson texture Compression (Android)
    // https://www.khronos.org/registry/gles/extensions/OES/OES_compressed_ETC1_RGB8_texture.txt
    ETC1 = 1004,

    // ATI texture compression (Android)
    // https://www.khronos.org/registry/gles/extensions/AMD/AMD_compressed_ATC_texture.txt
    ATC_RGB = 1005,             // ATC_RGB_AMD, Q_FORMAT_ATITC_RGB
    ATC_RGBA_EXPLICIT_ALPHA,    // ATC_RGBA_EXPLICIT_ALPHA_AMD, Q_FORMAT_ATITC_RGBA
    ATC_RGBA_INTERPOLATED_ALPHA // ATC_RGBA_INTERPOLATED_ALPHA_AMD, Q_FORMAT_ATC_RGBA_INTERPOLATED_ALPHA


#pragma warning restore 1591
  }
}
