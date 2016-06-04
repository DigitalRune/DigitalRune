// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;
using DigitalRune.Editor.Options;
using DigitalRune.Editor.Properties;
using ICSharpCode.AvalonEdit;


namespace DigitalRune.Editor.Text
{
    partial class TextExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private MergeableNodeCollection<OptionsPageViewModel> _optionsNodes;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public TextEditorOptions Options { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void AddOptions()
        {
            _optionsNodes = new MergeableNodeCollection<OptionsPageViewModel>
            {
                new MergeableNode<OptionsPageViewModel> { Content = new TextEditorOptionsPageViewModel(this) }
            };

            var optionsService = Editor.Services.GetInstance<IOptionsService>().WarnIfMissing();
            optionsService?.OptionsNodeCollections.Add(_optionsNodes);
        }


        private void RemoveOptions()
        {
            if (_optionsNodes == null)
                return;

            var optionsService = Editor.Services.GetInstance<IOptionsService>().WarnIfMissing();
            optionsService?.OptionsNodeCollections.Remove(_optionsNodes);
            _optionsNodes = null;
        }


        private void LoadOptions()
        {
            Options.Set(Settings.Default.TextEditorOptions ?? new TextEditorOptions());
        }


        private void SaveOptions()
        {
            Settings.Default.TextEditorOptions = Options;
        }
        #endregion
    }
}
