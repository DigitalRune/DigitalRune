// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Code completion data for keywords of the shader language.
    /// </summary>
    internal class KeywordCompletionData : NamedCompletionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeywordCompletionData"/> class.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        public KeywordCompletionData(string keyword)
            : base(keyword, null, null)
        {
        }
    }
}
