// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Code completion data for functions of the shader language and the effect file.
    /// </summary>
    internal class FunctionCompletionData : NamedCompletionData
    {
        /// <summary>
        /// Gets the function signatures.
        /// </summary>
        /// <value>The function signatures.</value>
        public string[] Signatures { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionCompletionData"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="signatures">The signatures.</param>
        public FunctionCompletionData(string name, string description, string[] signatures)
            : base(name, description, MultiColorGlyphs.Method)
        {
            if (String.IsNullOrEmpty(description))
                throw new ArgumentNullException(nameof(description));
            if (signatures == null)
                throw new ArgumentNullException(nameof(signatures));

            Signatures = signatures;
        }
    }
}
