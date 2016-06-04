// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Editor.Options;
using DigitalRune.Windows.Framework;
using NLog;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Represents the options that allows the user to change the settings of the shader editor.
    /// </summary>
    internal class ShaderOptionsPageViewModel : OptionsPageViewModel
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ShaderExtension _shaderExtension;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a <see cref="ShaderOptionsPageViewModel"/> instance that can be used at
        /// design-time.
        /// </summary>
        /// <value>
        /// a <see cref="ShaderOptionsPageViewModel"/> instance that can be used at design-time.
        /// </value>
        internal static ShaderOptionsPageViewModel DesignInstance
        {
            get { return new ShaderOptionsPageViewModel(); }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to compile effects with FXC.EXE.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if compile effects with FXC.EXE; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsFxcEnabled
        {
            get { return _isFxcEnabled; }
            set { SetProperty(ref _isFxcEnabled, value); }
        }
        private bool _isFxcEnabled = true;


        /// <summary>
        /// Gets or sets a value indicating whether to compile effects with MonoGame.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if compile effects with MonoGame; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsMonoGameEnabled
        {
            get { return _isMonoGameEnabled; }
            set { SetProperty(ref _isMonoGameEnabled, value); }
        }
        private bool _isMonoGameEnabled = true;


        /// <summary>
        /// Gets or sets a value indicating whether to compile effects with XNA.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if compile effects with XNA; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsXnaEnabled
        {
            get { return _isXnaEnabled; }
            set { SetProperty(ref _isXnaEnabled, value); }
        }
        private bool _isXnaEnabled = true;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        private ShaderOptionsPageViewModel()
            : base("Shader Editor")
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderOptionsPageViewModel"/> class.
        /// </summary>
        /// <param name="shaderExtension">The shader extension.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="shaderExtension"/> is <see langword="null"/>.
        /// </exception>
        public ShaderOptionsPageViewModel(ShaderExtension shaderExtension)
            : base("Shader Editor")
        {
            if (shaderExtension == null)
                throw new ArgumentNullException(nameof(shaderExtension));

            _shaderExtension = shaderExtension;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            IsFxcEnabled = _shaderExtension.IsFxcEffectProcessorEnabled;
            IsMonoGameEnabled = _shaderExtension.IsMonoGameEffectProcessorEnabled;
            IsXnaEnabled = _shaderExtension.IsXnaEffectProcessorEnabled;

            base.OnActivated(eventArgs);
        }


        /// <inheritdoc/>
        protected override void OnApply()
        {
            _shaderExtension.IsFxcEffectProcessorEnabled = IsFxcEnabled;
            _shaderExtension.IsMonoGameEffectProcessorEnabled = IsMonoGameEnabled;
            _shaderExtension.IsXnaEffectProcessorEnabled = IsXnaEnabled;
        }
        #endregion
    }
}
