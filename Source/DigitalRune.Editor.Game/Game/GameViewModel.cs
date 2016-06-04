// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Graphics;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Displays a rotating triangle and debug information.
    /// </summary>
    internal class GameViewModel : EditorDockTabItemViewModel
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static int _nextId = 0;
        private readonly IEditorService _editor;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a <see cref="GameViewModel"/> instance that can be used at design-time.
        /// </summary>
        /// <value>
        /// A <see cref="GameViewModel"/> instance that can be used at design-time.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static GameViewModel DesignInstance
        {
            get { return new GameViewModel(null); }
        }


        /// <summary>
        /// Gets or sets the graphics screens to be rendered.
        /// </summary>
        /// <value>The graphics screens to be rendered.</value>
        public IList<GraphicsScreen> GraphicsScreens
        {
            get { return _graphicsScreens; }
            private set { SetProperty(ref _graphicsScreens, value); }
        }
        private IList<GraphicsScreen> _graphicsScreens;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="GameViewModel"/> class.
        /// </summary>
        /// <param name="editor">The editor. Can be <see langword="null"/> at design-time.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> is <see langword="null"/>.
        /// </exception>
        public GameViewModel(IEditorService editor)
        {
            if (editor == null && !WindowsHelper.IsInDesignMode)
                throw new ArgumentNullException(nameof(editor));

            _editor = editor;

            DisplayName = "3D Scene";
            DockId = "Scene" + _nextId++;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            if (eventArgs.Opened)
            {
                // Initialize graphics screens.
                var graphicsService = _editor.Services.GetInstance<IGraphicsService>().ThrowIfMissing();
                GraphicsScreens = new GraphicsScreen[]
                {
                    new TriangleGraphicsScreen(graphicsService),
                    new DebugGraphicsScreen(_editor.Services) { ShowTitleSafeArea = true }
                };
            }

            base.OnActivated(eventArgs);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            if (eventArgs.Closed)
            {
                // Dispose graphics screens.
                foreach (var graphicsScreen in GraphicsScreens)
                    graphicsScreen.SafeDispose();

                GraphicsScreens = null;
            }

            base.OnDeactivated(eventArgs);
        }
        #endregion
    }
}
