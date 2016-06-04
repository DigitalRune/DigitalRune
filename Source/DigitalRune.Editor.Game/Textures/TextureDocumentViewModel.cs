// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows.Input;
using DigitalRune.Editor.Documents;
using DigitalRune.Mathematics;
using DigitalRune.Windows.Framework;
using static System.FormattableString;


namespace DigitalRune.Editor.Textures
{
    internal class TextureDocumentViewModel : DocumentViewModel
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the zoom level.
        /// </summary>
        /// <value>The zoom level. The default value is 1.0.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public double Zoom
        {
            get { return _zoom; }
            set { SetProperty(ref _zoom, value); }
        }
        private double _zoom = 1.0;


        /// <summary>
        /// Gets or sets a value indicating whether the red channel is rendered.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if red channel is rendered; otherwise, <see langword="false"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool EnableRedChannel
        {
            get { return _enableRedChannel; }
            set
            {
                if (SetProperty(ref _enableRedChannel, value))
                    OnChannelChanged(ColorChannels.Red, value);
            }
        }
        private bool _enableRedChannel;


        /// <summary>
        /// Gets or sets a value indicating whether the green channel is rendered.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if green channel is rendered; otherwise, <see langword="false"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool EnableGreenChannel
        {
            get { return _enableGreenChannel; }
            set
            {
                if (SetProperty(ref _enableGreenChannel, value))
                    OnChannelChanged(ColorChannels.Green, value);
            }
        }
        private bool _enableGreenChannel;


        /// <summary>
        /// Gets or sets a value indicating whether the blue channel is rendered.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if blue channel is rendered; otherwise, <see langword="false"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool EnableBlueChannel
        {
            get { return _enableBlueChannel; }
            set
            {
                if (SetProperty(ref _enableBlueChannel, value))
                    OnChannelChanged(ColorChannels.Blue, value);
            }
        }
        private bool _enableBlueChannel;


        /// <summary>
        /// Gets or sets a value indicating whether the alpha channel is rendered.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if alpha channel is rendered; otherwise, <see langword="false"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool EnableAlphaChannel
        {
            get { return _enableAlphaChannel; }
            set { SetProperty(ref _enableAlphaChannel, value); }
        }
        private bool _enableAlphaChannel;


        public bool IsRedChannelSupported { get; }
        public bool IsGreenChannelSupported { get; }
        public bool IsBlueChannelSupported { get; }
        public bool IsAlphaChannelSupported { get; }


        // IsPremultiplied is data-bound to the presentation target.
        // AlphaBlendTypeIndex and AlphaBlendTypeIndex are used for the combo box.
        // IsPremultiplied and AlphaBlendTypeIndex represent the same info.
        public bool IsPremultiplied
        {
            get { return _isPremultiplied; }
            private set { SetProperty(ref _isPremultiplied, value); }
        }
        private bool _isPremultiplied;

        public int AlphaBlendTypeIndex
        {
            get { return _alphaBlendTypeIndex; }
            set
            {
                if (SetProperty(ref _alphaBlendTypeIndex, value))
                {
                    IsPremultiplied = (AlphaBlendTypeIndex == 1);
                }
            }
        }
        private int _alphaBlendTypeIndex;

        public string[] AlphaBlendTypes { get; }


        public int MipLevel
        {
            get { return _mipLevel; }
            set { SetProperty(ref _mipLevel, value); }
        }
        private int _mipLevel;


        public string[] MipLevelNames { get; }


        public DelegateCommand<object> ChangeMipLevelCommand { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureDocumentViewModel" /> class.
        /// </summary>
        /// <param name="document">The TextureDocument.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        public TextureDocumentViewModel(TextureDocument document)
            : base(document)
        {
            // Disable unsupported color channels.
            var channels = TextureHelper.GetColorChannels(document.Texture2D.Format);
            if ((channels & ColorChannels.Red) != 0)
            {
                EnableRedChannel = true;
                IsRedChannelSupported = true;
            }
            if ((channels & ColorChannels.Green) != 0)
            {
                EnableGreenChannel = true;
                IsGreenChannelSupported = true;
            }
            if ((channels & ColorChannels.Blue) != 0)
            {
                EnableBlueChannel = true;
                IsBlueChannelSupported = true;
            }
            if ((channels & ColorChannels.Alpha) != 0)
            {
                EnableAlphaChannel = true;
                IsAlphaChannelSupported = true;
            }

            AlphaBlendTypes = new[] { "Straight alpha", "Premultiplied alpha" };

            MipLevelNames = new string[document.Texture2D.LevelCount];
            for (int i = 0; i < document.Texture2D.LevelCount; i++)
                MipLevelNames[i] = Invariant($"Mip level {i}");

            ChangeMipLevelCommand = new DelegateCommand<object>(ChangeMipLevel);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnChannelChanged(ColorChannels colorChannel, bool newValue)
        {
            // If Shift is pressed, we toggle RGB together.
            if (colorChannel != ColorChannels.Alpha
                && (Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                EnableRedChannel = newValue;
                EnableGreenChannel = newValue;
                EnableBlueChannel = newValue;
            }
        }


        private void ChangeMipLevel(object parameter)
        {
            int levelChange = ObjectHelper.ConvertTo<int>(parameter, CultureInfo.InvariantCulture);
            MipLevel = MathHelper.Clamp(
                MipLevel + levelChange,
                0,
                ((TextureDocument)Document).Texture2D.LevelCount - 1);
        }
        #endregion
    }
}
