// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Text;
using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Code completion data for effect states.
    /// </summary>
    internal class StateCompletionData : NamedCompletionData
    {
        private static readonly string[] EmptyArray = new string[0];


        /// <summary>
        /// Gets the allowed values for this effect state.
        /// </summary>
        /// <value>The allowed values (or an empty array if the state accepts arbitrary values).</value>
        public string[] AllowedValues { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="StateCompletionData"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        public StateCompletionData(string text, string description)
            : this(text, description, null, false)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="StateCompletionData"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="allowedValues">The allowed values.</param>
        public StateCompletionData(string text, string description, string[] allowedValues)
            : this(text, description, allowedValues, false)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="StateCompletionData"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="requiresIndex">
        /// If set to <see langword="true"/> the effect state requires an index (e.g. "AlphaOp[0] = ...").
        /// </param>
        public StateCompletionData(string text, string description, bool requiresIndex)
            : this(text, description, null, requiresIndex)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="StateCompletionData"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="allowedValues">The allowed values.</param>
        /// <param name="requiresIndex">
        /// If set to <see langword="true"/> the effect state requires an index (e.g. "AlphaOp[0] = ...").
        /// </param>
        public StateCompletionData(string text, string description, string[] allowedValues, bool requiresIndex)
            : base(text, description, MultiColorGlyphs.Field)
        {
            AllowedValues = allowedValues ?? EmptyArray;

            Text = requiresIndex ? Text + "[n]" : Text;
            Content = Text;

            // ----- Build description.
            StringBuilder descriptionBuilder = new StringBuilder();
            descriptionBuilder.Append(description);

            if (requiresIndex)
            {
                descriptionBuilder.AppendLine();
                descriptionBuilder.Append("Requires index.");
            }

            if (allowedValues != null && allowedValues.Length > 0)
            {
                descriptionBuilder.AppendLine();
                descriptionBuilder.AppendLine("Allowed values: ");
                descriptionBuilder.Append("  ");
                descriptionBuilder.Append(allowedValues[0]);
                for (int i = 1; i < allowedValues.Length; i++)
                {
                    descriptionBuilder.Append(",  ");  // Two spaces after ',' to make it more readable.
                    if ((i % 2) == 0)
                    {
                        // Add a line break every 4th value
                        descriptionBuilder.AppendLine();
                        descriptionBuilder.Append("  ");
                    }
                    descriptionBuilder.Append(allowedValues[i]);
                }
            }
            Description = descriptionBuilder.ToString();
        }
    }
}
