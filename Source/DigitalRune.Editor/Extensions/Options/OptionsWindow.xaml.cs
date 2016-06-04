// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Editor.Options
{
    /// <summary>
    /// The default Options view.
    /// </summary>
    internal partial class OptionsWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsWindow"/> class.
        /// </summary>
        public OptionsWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => TreeView.Focus();
        }
    }
}
