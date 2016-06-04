// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Game;
using DigitalRune.Windows.Themes;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Editor.Textures
{
    /// <summary>
    /// Handles image and texture documents (e.g. JPEG, PNG, DDS files).
    /// </summary>
    internal class TextureDocumentFactory : DocumentFactory
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly IEditorService _editor;
        private readonly DocumentType _processedTextureDocumentType;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureDocumentFactory" /> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        public TextureDocumentFactory(IEditorService editor)
            : base("Texture Viewer")
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            _editor = editor;

            // ----- Initialize supported document types.
            // Textures
            var textureDocumentType = new DocumentType(
                name: "Texture",
                factory: this,
                icon: MultiColorGlyphs.Image,
                fileExtensions: new[]
                {
                    ".bmp", ".dds", ".gif", ".ico", ".jpg", ".jpeg", ".png", ".tga", ".tif",
                    ".jxr", ".hdp", ".wdp" // HD Photo
                },
                isCreatable: false,
                isLoadable: true,
                isSavable: false,
                priority: 10);

            _processedTextureDocumentType = new DocumentType(
                name: "Texture, processed",
                factory: this,
                icon: MultiColorGlyphs.Image,
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
                textureDocumentType,
                _processedTextureDocumentType,
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
                return _processedTextureDocumentType;

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

                var texture2D = result.Asset as Texture2D;
                // TODO: Add support for Texture3D and TextureCube.

                return texture2D != null;
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
            return new TextureDocument(_editor, documentType);
        }
        #endregion
    }
}
