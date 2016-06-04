// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Game;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Windows.Themes;
using Microsoft.Xna.Framework.Graphics;
using Path = System.IO.Path;


namespace DigitalRune.Editor.Models
{
    /// <summary>
    /// Handles 3d model documents (i.e. FBX, X, DAE, XNB files).
    /// </summary>
    internal class ModelDocumentFactory : DocumentFactory
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly IEditorService _editor;
        private readonly DocumentType _processedModelDocumentType;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelDocumentFactory" /> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        public ModelDocumentFactory(IEditorService editor)
            : base("Model Viewer")
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            _editor = editor;

            // ----- Initialize supported document types.
            var modelDocumentType = new DocumentType(
                name: "3D model",
                factory: this,
                icon: MultiColorGlyphs.Model,
                fileExtensions: new[]
                {
                    ".dae", ".fbx", ".x"
                },
                isCreatable: false,
                isLoadable: true,
                isSavable: false,
                priority: 10);

            _processedModelDocumentType = new DocumentType(
                name: "3D model, processed",
                factory: this,
                icon: MultiColorGlyphs.Model,
                fileExtensions: new[]
                {
                    ".xnb"
                },
                isCreatable: false,
                isLoadable: true,
                isSavable: false,
                priority: 10);

            DocumentTypes = new[]
            {
                modelDocumentType,
                _processedModelDocumentType,
            };

        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public override DocumentType GetDocumentType(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var extension = Path.GetExtension(uri.LocalPath);
            if (string.Compare(extension, ".XNB", StringComparison.OrdinalIgnoreCase) != 0)
                return base.GetDocumentType(uri);

            if (CanLoadXnb(uri))
                return _processedModelDocumentType;

            return null;
        }


        private bool CanLoadXnb(Uri uri)
        {
            // Try to load the asset and check if we get a model.
            string fileName = uri.LocalPath;
            string directoryName = Path.GetDirectoryName(fileName);

            try
            {
                var monoGameService = _editor.Services.GetInstance<IMonoGameService>().ThrowIfMissing();
                var result = monoGameService.LoadXnb(directoryName, fileName, cacheResult: true);

                // Do not dispose result. IMonoGameService caches the asset.

                var modelNode = result.Asset as ModelNode;
                var model = result.Asset as Model;
                return modelNode != null || model != null;
            }
            catch (Exception)
            {
                // Asset could not be loaded.
            }

            return false;
        }


        /// <inheritdoc/>
        protected override Document OnCreate(DocumentType documentType)
        {
            return new ModelDocument(_editor, documentType);
        }
        #endregion
    }
}
