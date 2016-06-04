// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Interop;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Windows;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Updates the camera's field of view (FOV) when the size of the presentation target changes.
    /// </summary>
    public class CameraFieldOfViewBehavior : Behavior<D3DImagePresentationTarget>
    {
        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(CameraFieldOfViewBehavior),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue));

        /// <summary>
        /// Gets or sets a value indicating whether this behavior is enabled.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the behavior is enabled; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether this behavior is enabled.")]
        [Category(Categories.Common)]
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="CameraNode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CameraNodeProperty = DependencyProperty.Register(
            "CameraNode",
            typeof(CameraNode),
            typeof(CameraFieldOfViewBehavior),
            new FrameworkPropertyMetadata(null, OnCameraNodeChanged));


        /// <summary>
        /// Gets or sets the camera node.
        /// This is a dependency property.
        /// </summary>
        /// <value>The camera node.</value>
        [Description("Gets or sets the camera node.")]
        [Category(Categories.Behavior)]
        public CameraNode CameraNode
        {
            get { return (CameraNode)GetValue(CameraNodeProperty); }
            set { SetValue(CameraNodeProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnCameraNodeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (CameraFieldOfViewBehavior)dependencyObject;
            behavior.UpdateFieldOfView();
        }


        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the <see cref="Behavior{T}.AssociatedObject"/>.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SizeChanged += OnSizeChanged;
        }


        /// <summary>
        /// Called when the <see cref="Behavior{T}"/> is about to detach from the 
        /// <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        /// <remarks>
        /// When this method is called, detaching can not be canceled. The 
        /// <see cref="Behavior{T}.AssociatedObject"/> is still set.
        /// </remarks>
        protected override void OnDetaching()
        {
            AssociatedObject.SizeChanged -= OnSizeChanged;
            base.OnDetaching();
        }


        private void OnSizeChanged(object sender, SizeChangedEventArgs eventArgs)
        {
            if (eventArgs.Handled)
                return;

            UpdateFieldOfView();
        }


        private void UpdateFieldOfView()
        {
            if (!IsEnabled)
                return;

            var cameraNode = CameraNode;
            if (cameraNode == null)
                return;

            float aspectRatio = (float)AssociatedObject.ActualWidth / (float)AssociatedObject.ActualHeight;
            var projection = cameraNode.Camera.Projection;

            var orthographicProjection = projection as OrthographicProjection;
            if (orthographicProjection != null)
            {
                // Orthographic camera.
                float height = orthographicProjection.Height;
                float width = height * aspectRatio;
                orthographicProjection.Set(width, height);
                return;
            }

            var perspectiveProjection = projection as PerspectiveProjection;
            if (perspectiveProjection != null)
            {
                // Perspective camera.
                float height = perspectiveProjection.Height;
                float width = height * aspectRatio;
                perspectiveProjection.Set(width, height);
            }
        }
        #endregion
    }
}
