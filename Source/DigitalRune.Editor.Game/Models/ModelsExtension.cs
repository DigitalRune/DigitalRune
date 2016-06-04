// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Linq;
using System.Windows;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Game;
using DigitalRune.Editor.Game.Properties;
using DigitalRune.Windows.Docking;
using NLog;


namespace DigitalRune.Editor.Models
{
    /// <summary>
    /// Provides functions for 3D model documents.
    /// </summary>
    public sealed class ModelsExtension : EditorExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IDocumentService _documentService;
        private ResourceDictionary _resourceDictionary;
        private ModelDocumentFactory _textureDocumentFactory;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        internal bool UseDigitalRuneGraphics
        {
            get { return Settings.Default.ModelDocumentUsesDigitalRune; }
            set { Settings.Default.ModelDocumentUsesDigitalRune = value; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelsExtension"/> class.
        /// </summary>
        public ModelsExtension()
        {
            Logger.Debug("Initializing ModelsExtension.");
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        protected override void OnStartup()
        {
            Editor.Extensions.OfType<GameExtension>().FirstOrDefault().ThrowIfMissing();
            _documentService = Editor.Services.GetInstance<IDocumentService>().ThrowIfMissing();

            AddDataTemplates();
            AddDocumentFactories();
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            RemoveDocumentFactories();
            RemoveDataTemplates();

            _documentService = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor.Game;component/Models/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
            Application.Current.Resources.MergedDictionaries.Add(_resourceDictionary);
        }


        private void RemoveDataTemplates()
        {
            Application.Current.Resources.MergedDictionaries.Remove(_resourceDictionary);
            _resourceDictionary = null;
        }


        private void AddDocumentFactories()
        {
            _textureDocumentFactory = new ModelDocumentFactory(Editor);
            _documentService.Factories.Add(_textureDocumentFactory);
        }


        private void RemoveDocumentFactories()
        {
            _documentService.Factories.Remove(_textureDocumentFactory);
            _textureDocumentFactory = null;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            return null;
        }
        #endregion
    }
}
