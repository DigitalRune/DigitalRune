// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Code completion data for macros of the shader language.
    /// </summary>
    internal class MacroCompletionData : NamedCompletionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MacroCompletionData"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        public MacroCompletionData(string name, string description)
            : base(name, description, MultiColorGlyphs.Macro)
        {
        }
    }
}
