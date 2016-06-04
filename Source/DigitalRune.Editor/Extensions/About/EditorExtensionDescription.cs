// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.About
{
    /// <summary>
    /// Describes an editor extension.
    /// </summary>
    public class EditorExtensionDescription
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }


        /// <summary>
        /// Gets or sets the icon of this extension.
        /// </summary>
        /// <value>The icon of this extension. The default is a generic extension icon.</value>
        public object Icon { get; set; }


        /// <summary>
        /// Gets or sets the name of the extension.
        /// </summary>
        /// <value>The name of the extension.</value>
        public string Name { get; set; }


        /// <summary>
        /// Gets or sets the version number of the extension.
        /// </summary>
        /// <value>The version number of the extension. For example: "1.0.0.0".</value>
        public string Version { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="EditorExtensionDescription"/> class.
        /// </summary>
        public EditorExtensionDescription()
        {
            Icon = MultiColorGlyphs.Plugin;
        }
    }
}
