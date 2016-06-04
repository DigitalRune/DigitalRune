// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Code completion for types of the shader language and the effect file.
    /// </summary>
    internal class TypeCompletionData : NamedCompletionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeCompletionData"/> class.
        /// </summary>
        /// <param name="name">The name of the type.</param>
        public TypeCompletionData(string name)
            : base(name, null, MultiColorGlyphs.Struct)
        {
        }
    }
}
