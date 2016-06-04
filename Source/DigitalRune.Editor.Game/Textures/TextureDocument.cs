// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Game;
using DigitalRune.Editor.Properties;
using DigitalRune.Graphics;
using DigitalRune.Windows.Controls;
using Microsoft.Xna.Framework.Graphics;
using Path = System.IO.Path;


namespace DigitalRune.Editor.Textures
{
    /// <summary>
    /// Represents a texture.
    /// </summary>
    /// <remarks>
    /// The texture is disposed when the document is disposed.
    /// </remarks>
    internal class TextureDocument : Document
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly IGraphicsService _graphicsService;
        private readonly IMonoGameService _monoGameService;
        private readonly IPropertiesService _propertiesService;

        private PropertySource _propertySource;

        private MonoGameContent _monoGameContent;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the 2D texture.
        /// </summary>
        /// <value>The 2D texture.</value>
        public Texture2D Texture2D
        {
            get { return _texture2D; }
            set
            {
                if (SetProperty(ref _texture2D, value))
                    UpdateProperties();
            }
        }
        private Texture2D _texture2D;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureDocument"/> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <param name="documentType">The type of the document.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> or <paramref name="documentType"/> is <see langword="null"/>.
        /// </exception>
        internal TextureDocument(IEditorService editor, DocumentType documentType)
          : base(editor, documentType)
        {
            _graphicsService = editor.Services.GetInstance<IGraphicsService>().ThrowIfMissing();
            _monoGameService = editor.Services.GetInstance<IMonoGameService>().ThrowIfMissing();
            _propertiesService = editor.Services.GetInstance<IPropertiesService>().WarnIfMissing();

            Editor.ActiveDockTabItemChanged += OnEditorDockTabItemChanged;
        }


        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_propertiesService != null && _propertiesService.PropertySource == _propertySource)
                        _propertiesService.PropertySource = null;

                    Editor.ActiveDockTabItemChanged -= OnEditorDockTabItemChanged;

                    Texture2D?.Dispose();
                    _monoGameContent?.Dispose();
                }

                // Release unmanaged resources.
            }

            base.Dispose(disposing);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override DocumentViewModel OnCreateViewModel()
        {
            return new TextureDocumentViewModel(this);
        }


        /// <inheritdoc/>
        protected override void OnLoad()
        {
            Texture2D?.Dispose();
            Texture2D = null;
            _monoGameContent?.Dispose();
            _monoGameContent = null;

            // TODO: Asynchronously load textures.
            string fileName = Uri.LocalPath;
            string extension = Path.GetExtension(fileName);
            bool isXnb = string.Compare(extension, ".XNB", StringComparison.OrdinalIgnoreCase) == 0;
            if (isXnb)
            {
                try
                {
                    string directoryName = Path.GetDirectoryName(fileName);
                    _monoGameContent = _monoGameService.LoadXnb(directoryName, fileName, cacheResult: false);
                    Texture2D = (Texture2D)_monoGameContent.Asset;
                }
                catch
                {
                    Texture2D?.Dispose();
                    Texture2D = null;
                    _monoGameContent?.Dispose();
                    _monoGameContent = null;

                    throw;
                }
            }
            else
            {
                Texture2D = TextureHelper.LoadTexture(_graphicsService, Uri.LocalPath);
            }

            UpdateProperties();
        }


        /// <inheritdoc/>
        protected override void OnSave()
        {
            throw new NotImplementedException();
        }


        private void OnEditorDockTabItemChanged(object sender, EventArgs eventArgs)
        {
            // One of our view models was activated. --> Show our properties.
            var documentViewModel = Editor.ActiveDockTabItem as TextureDocumentViewModel;
            if (documentViewModel != null && documentViewModel.Document == this)
                if (_propertiesService != null)
                    _propertiesService.PropertySource = _propertySource;
        }


        private void UpdateProperties()
        {
            if (_propertySource == null)
                _propertySource = new PropertySource();

            _propertySource.Name = (IsUntitled) ? UntitledName : Path.GetFileName(Uri.LocalPath);
            _propertySource.TypeName = "Texture";

            var textBlockKey = new ComponentResourceKey(typeof(PropertyGrid), "TextBlock");

            _propertySource.Properties.Clear();
            _propertySource.Properties.Add(new CustomProperty
            {
                Category = "Misc",
                Name = "File name",
                Value = this.GetName(),
                Description = "The filename of the image.",
                PropertyType = typeof(string),
                //DataTemplateKey = textBlockKey,
                CanReset = false,
                IsReadOnly = true,
            });
            _propertySource.Properties.Add(new CustomProperty
            {
                Category = "Misc",
                Name = "Size",
                Value = Texture2D?.Width,
                Description = "Image width in pixels.",
                PropertyType = typeof(int),
                DataTemplateKey = textBlockKey,
                CanReset = false,
                IsReadOnly = true,
            });
            _propertySource.Properties.Add(new CustomProperty
            {
                Category = "Misc",
                Name = "Height",
                Value = Texture2D?.Height,
                Description = "Image height in pixels.",
                PropertyType = typeof(int),
                DataTemplateKey = textBlockKey,
                CanReset = false,
                IsReadOnly = true,
            });
            _propertySource.Properties.Add(new CustomProperty
            {
                Category = "Misc",
                Name = "Format",
                Value = Texture2D?.Format,
                Description = null,
                PropertyType = typeof(SurfaceFormat),
                DataTemplateKey = textBlockKey,
                CanReset = false,
                IsReadOnly = true,
            });
            _propertySource.Properties.Add(new CustomProperty
            {
                Category = "Misc",
                Name = "Mipmap levels",
                Value = Texture2D?.LevelCount,
                Description = null,
                PropertyType = typeof(int),
                DataTemplateKey = textBlockKey,
                CanReset = false,
                IsReadOnly = true,
            });
        }
        #endregion
    }
}
