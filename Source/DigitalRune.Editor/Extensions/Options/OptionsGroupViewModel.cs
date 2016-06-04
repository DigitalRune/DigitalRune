// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Editor.Options
{
    /// <summary>
    /// Represents a node that groups other option pages.
    /// </summary>
    /// <remarks>
    /// An <see cref="OptionsGroupViewModel"/> will be rendered as a tree view item with children in
    /// the Options dialog. When the tree view item is clicked, the options page of the first
    /// child is shown in the Options dialog.
    /// </remarks>
    public sealed class OptionsGroupViewModel : OptionsPageViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsGroupViewModel"/> class.
        /// </summary>
        /// <param name="name">The name of the options group.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        public OptionsGroupViewModel(string name)
            : base(name)
        {
        }


        /// <inheritdoc/>
        protected override void OnApply()
        {
        }
    }
}
