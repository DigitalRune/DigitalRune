// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Renders a scene using forward rendering.
    /// </summary>
    public class BasicGraphicsScreen : GraphicsScreen
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly MeshRenderer _meshRenderer;
        private readonly BillboardRenderer _billboardRenderer;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the scene.
        /// </summary>
        /// <value>The scene.</value>
        public Scene Scene { get; }


        /// <summary>
        /// Gets or sets the active camera node. (Does not need to included in <see cref="Scene"/>.)
        /// </summary>
        /// <value>The active camera node.</value>
        public CameraNode CameraNode { get; set; }


        /// <summary>
        /// Gets or sets a MonoGame <see cref="Model"/> which is rendered in addition to the
        /// DigitalRune <see cref="Scene"/>.
        /// </summary>
        /// <value>
        /// A MonoGame <see cref="Model"/> which is rendered in addition to the DigitalRune
        /// <see cref="Scene"/>.
        /// </value>
        public Model Model { get; set; }


        /// <summary>
        /// Gets a <see cref="DebugRenderer"/> for drawing debugging information.
        /// </summary>
        /// <value>The <see cref="DebugRenderer"/>.</value>
        public DebugRenderer DebugRenderer { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicGraphicsScreen"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="services"/> is <see langword="null"/>.
        /// </exception>
        public BasicGraphicsScreen(IServiceLocator services)
            : base(services?.GetInstance<IGraphicsService>())
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            _meshRenderer = new MeshRenderer();
            _billboardRenderer = new BillboardRenderer(GraphicsService, 2048);

            var contentManager = services.GetInstance<ContentManager>();
            var spriteFont = contentManager.Load<SpriteFont>("DigitalRune.Editor.Game/Fonts/DejaVuSans");
            DebugRenderer = new DebugRenderer(GraphicsService, spriteFont);

            Scene = new Scene();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnUpdate(TimeSpan deltaTime)
        {
            Scene.Update(deltaTime);
        }


        /// <inheritdoc/>
        protected override void OnRender(RenderContext context)
        {
            context.Scene = Scene;
            context.CameraNode = CameraNode;

            var graphicsDevice = GraphicsService.GraphicsDevice;

            // Clear background.
            graphicsDevice.Clear(Color.CornflowerBlue);

            if (context.CameraNode != null)
            {

                // Frustum Culling: Get all scene nodes that intersect the camera frustum.
                var query = Scene.Query<CameraFrustumQuery>(context.CameraNode, context);

                // Set render state.
                graphicsDevice.DepthStencilState = DepthStencilState.Default;
                graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                graphicsDevice.BlendState = BlendState.Opaque;
                graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

                // Use a MeshRenderer to render all MeshNodes that are in the camera frustum.
                // We use the shader effects and effect parameters for the render pass named 
                // "Default" (see the material (.drmat) files of the assets).
                context.RenderPass = "Default";
                _meshRenderer.Render(query.SceneNodes, context);
                context.RenderPass = null;

                // Draw XNA model.
                if (Model != null)
                {
                    Matrix view = (Matrix)CameraNode.View;
                    Matrix projection = CameraNode.Camera.Projection;
                    Model.Draw(Matrix.Identity, view, projection);
                }

                // Render alpha-blended (transparent) meshes.
                graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                graphicsDevice.BlendState = BlendState.AlphaBlend;
                graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
                context.RenderPass = "AlphaBlend";
                _meshRenderer.Render(query.SceneNodes, context);
                context.RenderPass = null;

                // Set the render states for alpha blended objects.
                graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                graphicsDevice.RasterizerState = RasterizerState.CullNone;
                graphicsDevice.BlendState = BlendState.NonPremultiplied;
                graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                // The BillboardRenderer renders BillboardNodes and ParticleSystemNodes.
                _billboardRenderer.Render(query.SceneNodes, context);
            }

            // Render debug info.
            DebugRenderer.Render(context);

            context.CameraNode = null;
            context.Scene = null;
        }
        #endregion
    }
}
