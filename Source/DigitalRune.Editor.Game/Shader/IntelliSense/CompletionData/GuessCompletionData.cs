// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Code completion data for a guessed identifier.
    /// </summary>
    internal class GuessCompletionData : NamedCompletionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GuessCompletionData"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public GuessCompletionData(string name)
            : base(name, null, MultiColorGlyphs.Guess)
        {
        }
    }
}
