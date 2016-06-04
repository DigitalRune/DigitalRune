// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Code completion data for variables.
    /// </summary>
    internal class VariableCompletionData : NamedCompletionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableCompletionData"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public VariableCompletionData(string name)
            : base(name, null, MultiColorGlyphs.Field)
        {
        }
    }
}
