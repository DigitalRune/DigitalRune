// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;


namespace DigitalRune.Editor.Textures
{
    /// <summary>
    /// Draws a single texture.
    /// </summary>
    /// <remarks>
    /// A custom effect is used to draw a checkerboard pattern, apply a color transform (e.g. to 
    /// show individual color channels), apply scale and offset, etc.
    /// </remarks>
    internal class TextureGraphicsScreen : GraphicsScreen //, IDisposable
    {
        // TODO: Use pixel shader to show channels.
        //
        //  R G B A   Pixel shader output
        //  *         (r, r, r, 1)
        //    *       (g, g, g, 1)
        //      *     (b, b, b, 1)
        //        *   (a, a, a, 1)
        //  * * * *   (r, g, b, a)  Add option: premultiplied or non-premultiplied?


        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private static readonly Vector4 CheckerColor0 = new Vector4(0.25f);
        private static readonly Vector4 CheckerColor1 = new Vector4(0.5f);
        private static readonly Vector2 CheckerCellSize = new Vector2(8);
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly Effect _effect;
        private readonly EffectParameter _parameterViewportSize;
        private readonly EffectParameter _parameterSourceTexture;
        private readonly EffectParameter _parameterCheckerColor0;
        private readonly EffectParameter _parameterCheckerColor1;
        private readonly EffectParameter _parameterCheckerCount;
        private readonly EffectParameter _parameterInputGamma;
        private readonly EffectParameter _parameterOutputGamma;
        private readonly EffectParameter _parameterColorTransform;
        private readonly EffectParameter _parameterColorOffset;
        private readonly EffectParameter _parameterMipLevel;

        //private readonly SpriteBatch _spriteBatch;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        ///// <summary>
        ///// Gets a value indicating whether this instance has been disposed of.
        ///// </summary>
        ///// <value>
        ///// <see langword="true"/> if this instance has been disposed of; otherwise,
        ///// <see langword="false"/>.
        ///// </value>
        //public bool IsDisposed { get; private set; }


        public Texture2D Texture2D { get; set; }
        public Vector2F Offset { get; set; }
        public float Scale { get; set; }
        public float InputGamma { get; set; }
        public float OutputGamma { get; set; }
        public Matrix44F ColorTransform { get; set; }
        public Vector4F ColorOffset { get; set; }
        public bool IsPremultiplied { get; set; }
        public float MipLevel { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureGraphicsScreen"/> class.
        /// </summary>
        /// <param name="graphicsService">The graphics service.</param>
        public TextureGraphicsScreen(IGraphicsService graphicsService)
          : base(graphicsService)
        {
            IsPremultiplied = false;
            _effect = graphicsService.Content.Load<Effect>("DigitalRune.Editor.Game/Effects/TextureViewer");
            _parameterViewportSize = _effect.Parameters["ViewportSize"];
            _parameterSourceTexture = _effect.Parameters["SourceTexture"];
            _parameterCheckerColor0 = _effect.Parameters["CheckerColor0"];
            _parameterCheckerColor1 = _effect.Parameters["CheckerColor1"];
            _parameterCheckerCount = _effect.Parameters["CheckerCount"];
            _parameterInputGamma = _effect.Parameters["InputGamma"];
            _parameterOutputGamma = _effect.Parameters["OutputGamma"];
            _parameterColorTransform = _effect.Parameters["ColorTransform"];
            _parameterColorOffset = _effect.Parameters["ColorOffset"];
            _parameterMipLevel = _effect.Parameters["MipLevel"];

            //_spriteBatch = graphicsService.GetSpriteBatch();
        }


        ///// <summary>
        ///// Releases all resources used by an instance of the <see cref="TextureGraphicsScreen"/> class.
        ///// </summary>
        ///// <remarks>
        ///// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in
        ///// <see langword="true"/>, and then suppresses finalization of the instance.
        ///// </remarks>
        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}


        ///// <summary>
        ///// Releases the unmanaged resources used by an instance of the <see cref="TextureGraphicsScreen"/> class
        ///// and optionally releases the managed resources.
        ///// </summary>
        ///// <param name="disposing">
        ///// <see langword="true"/> to release both managed and unmanaged resources;
        ///// <see langword="false"/> to release only unmanaged resources.
        ///// </param>
        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!IsDisposed)
        //    {
        //        if (disposing)
        //        {
        //        }

        //        IsDisposed = true;
        //    }
        //}
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnUpdate(TimeSpan deltaTime)
        {
        }


        /// <inheritdoc/>
        protected override void OnRender(RenderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var graphicsDevice = GraphicsService.GraphicsDevice;

            // Clear screen to gray.
            graphicsDevice.Clear(new Color(96, 96, 96));
            
            // Draw texture.
            if (Texture2D != null)
            {
                // Snap to pixels.
                int x = (int)Math.Round(Offset.X, MidpointRounding.AwayFromZero);
                int y = (int)Math.Round(Offset.Y, MidpointRounding.AwayFromZero);
                int width = (int)Math.Round(Texture2D.Width * Scale, MidpointRounding.AwayFromZero);
                int height = (int)Math.Round(Texture2D.Height * Scale, MidpointRounding.AwayFromZero);
                Vector2 offset = new Vector2(x, y);
                Vector2 size = new Vector2(width, height);

                // Use linear sampling for minification. Use point sampling for magnification.
                double pixelScale = Scale * Math.Pow(2, MipLevel);
                var samplerState = (pixelScale >= 1) ? SamplerState.PointClamp : SamplerState.LinearClamp;
                graphicsDevice.SamplerStates[0] = samplerState;

                var viewport = context.Viewport;
                _parameterViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
                _parameterSourceTexture.SetValue(Texture2D);

                _parameterCheckerColor0.SetValue(CheckerColor0);
                _parameterCheckerColor1.SetValue(CheckerColor1);
                _parameterCheckerCount.SetValue(size / CheckerCellSize);

                _parameterInputGamma.SetValue(InputGamma);
                _parameterOutputGamma.SetValue(OutputGamma);
                _parameterColorTransform.SetValue((Matrix)ColorTransform);
                _parameterColorOffset.SetValue((Vector4)ColorOffset);
                _parameterMipLevel.SetValue(MipLevel);

                int techniqueIndex = 0;
                int passIndex = IsPremultiplied ? 1 : 0;
                _effect.Techniques[techniqueIndex].Passes[passIndex].Apply();

                graphicsDevice.DrawQuad(new Rectangle(x, y, width, height));

                _parameterSourceTexture.SetValue((Texture)null);
            }
        }
        #endregion
    }
}
