// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Describes a region of an DirectX Effect or CgFX file.
    /// </summary>
    internal enum ShaderRegion
    {
        /// <summary>Unknown code region.</summary>
        Unknown,
        /// <summary>The default region.</summary>
        Default,
        /// <summary>The outer scope of the effect file.</summary>
        Global,
        /// <summary>A structure (e.g. "struct", "cbuffer") or interface.</summary>
        StructureOrInterface,
        /// <summary>A shader code block.</summary>
        Code,
        /// <summary>A line comment (<c>// ...</c>).</summary>
        LineComment,
        /// <summary>A block comment (<c>/* ... */</c>).</summary>
        BlockComment,
        /// <summary>A string (e.g. <c>"Hello World"</c>).</summary>
        String,
        /// <summary>A character literal (e.g. <c>'a'</c>).</summary>
        CharacterLiteral,
        /// <summary>An assembler block (<c>asm { ... }</c>).</summary>
        Assembler,
        /// <summary>A DirectX 9 or Cg sampler state block.</summary>
        SamplerState,
        /// <summary>A state block.</summary>
        StateBlock,
        /// <summary>A technique or a pass.</summary>
        TechniqueOrPass,
        /// <summary>An annotation (<c>&lt; ... &gt;</c>).</summary>
        Annotation,
        /// <summary>A DirectX 10 blend state block.</summary>
        BlendState10,
        /// <summary>A DirectX 10 depth-stencil state block.</summary>
        DepthStencilState10,
        /// <summary>A DirectX 10 rasterizer state block.</summary>
        RasterizerState10,
        /// <summary>A DirectX 10 sampler state block.</summary>
        SamplerState10,
        /// <summary>A DirectX 10 technique or a pass.</summary>
        TechniqueOrPass10,
    }
}
