// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Code completion data for constants (HLSL constants, effect state values, etc.).
    /// </summary>
    /// <remarks>
    /// <see cref="ConstantCompletionData"/> does not provide a description, because most constants 
    /// have different usages and meanings.
    /// </remarks>
    internal class ConstantCompletionData : NamedCompletionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantCompletionData"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ConstantCompletionData(string name)
            : base(name, null, MultiColorGlyphs.Enum)
        {
        }
    }
}
