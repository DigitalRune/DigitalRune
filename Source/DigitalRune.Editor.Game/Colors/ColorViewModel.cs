// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Media;
using DigitalRune.Editor.Game.Properties;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Colors
{
    /// <summary>
    /// Represents the Color window.
    /// </summary>
    /// <remarks>
    /// The Color window listens for <see cref="Color"/>s on the message bus and automatically
    /// broadcasts the new color.
    /// </remarks>
    internal class ColorViewModel : EditorDockTabItemViewModel
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        internal const string DockIdString = "ColorPicker";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly IMessageBus _messageBus;

        // Keep strong reference to message bus subscription.
        // ReSharper disable once NotAccessedField.Local
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private IDisposable _subscription;
        private bool _acceptColors;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a <see cref="ColorViewModel"/> instance that can be used at design-time.
        /// </summary>
        /// <value>A <see cref="ColorViewModel"/> instance that can be used at design-time.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static ColorViewModel DesignInstance
        {
            get
            {
                return new ColorViewModel(null)
                {
                    _oldColor = System.Windows.Media.Colors.DarkBlue,
                    _color = System.Windows.Media.Colors.Blue,
                };
            }
        }


        /// <summary>
        /// Gets or sets the old color.
        /// </summary>
        /// <value>The old color.</value>
        public Color OldColor
        {
            get { return _oldColor; }
            set { SetProperty(ref _oldColor, value); }
        }
        private Color _oldColor;


        /// <summary>
        /// Gets or sets the new color.
        /// </summary>
        /// <value>The new color.</value>
        public Color Color
        {
            get { return _color; }
            set
            {
                if (SetProperty(ref _color, value))
                    SendColor(value);
            }
        }
        private Color _color;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorViewModel" /> class.
        /// </summary>
        /// <param name="editor">The editor. Can be <see langword="null"/> at design-time.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> is <see langword="null"/>.
        /// </exception>
        public ColorViewModel(IEditorService editor)
        {
            DisplayName = "Color";
            DockId = DockIdString;
            //Icon = MultiColorGlyphs.ColorPalette;
            IsPersistent = true;

            _oldColor = Settings.Default.OldColor;
            _color = Settings.Default.NewColor;

            // Listen for Color messages on the message bus.
            _messageBus = editor?.Services.GetInstance<IMessageBus>().WarnIfMissing();
            if (_messageBus != null)
                _subscription = _messageBus.Listen<Color>().Subscribe(ReceiveColor);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            if (eventArgs.Closed)
            {
                // Save last used colors.
                Settings.Default.OldColor = OldColor;
                Settings.Default.NewColor = Color;
            }

            base.OnDeactivated(eventArgs);
        }


        private void ReceiveColor(Color color)
        {
            if (!_acceptColors)
                return;

            OldColor = color;
            Color = color;
        }


        private void SendColor(Color color)
        {
            if (_messageBus == null)
                return;

            _acceptColors = false;
            _messageBus.Publish(color);
            _acceptColors = true;
        }
        #endregion
    }
}
