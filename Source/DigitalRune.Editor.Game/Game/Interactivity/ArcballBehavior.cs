// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using DigitalRune.Graphics.Interop;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Windows;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Allows to orbit the camera around the origin by pressing the left mouse button and moving
    /// the mouse. (This behavior is also known as "arcball camera".)
    /// </summary>
    /// <remarks>
    /// The behavior takes exclusive control of the camera and cannot be used in conjunction with
    /// other navigation behaviors that control camera position and orientation (e.g.
    /// <see cref="KeyboardNavigationBehavior"/>, <see cref="MouseLookBehavior"/>, ...)! It can be
    /// used with the <see cref="MouseZoomBehavior"/>.
    /// </remarks>
    public class ArcballBehavior : Behavior<D3DImagePresentationTarget>
    {
        // References:
        // This orbit behavior is known as: arcball, Shoemake's arcball, trackball
        // http://www.codeproject.com/Articles/22484/Arcball-Module-in-C-Tao-OpenGL
        // http://rainwarrior.ca/dragon/arcball.html


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private CameraNode _cameraNode;
        private Vector3F _startVector;
        private Matrix44F _originalTransform = Matrix44F.Identity;
        private Matrix44F _originalViewMatrix;
        private Matrix44F _transform = Matrix44F.Identity;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(ArcballBehavior),
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
            typeof(ArcballBehavior),
            new FrameworkPropertyMetadata(null));


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


        /// <summary>
        /// Identifies the <see cref="CameraTarget"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CameraTargetProperty = DependencyProperty.Register(
            "CameraTarget",
            typeof(Vector3F),
            typeof(ArcballBehavior),
            new FrameworkPropertyMetadata(Vector3F.Zero, OnCameraTargetChanged));

        /// <summary>
        /// Gets or sets the center around which the camera is orbiting.
        /// This is a dependency property.
        /// </summary>
        /// <value>The center around which the camera is orbiting.</value>
        [Description("Gets or sets the center around which the camera is orbiting.")]
        [Category(Categories.Behavior)]
        public Vector3F CameraTarget
        {
            get { return (Vector3F)GetValue(CameraTargetProperty); }
            set { SetValue(CameraTargetProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the <see cref="CameraTarget"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnCameraTargetChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (ArcballBehavior)dependencyObject;
            Vector3F oldValue = (Vector3F)eventArgs.OldValue;
            Vector3F newValue = (Vector3F)eventArgs.NewValue;
            behavior.OnCameraTargetChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the <see cref="CameraTarget"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnCameraTargetChanged(Vector3F oldValue, Vector3F newValue)
        {
            //var cameraNode = CameraNode;
            //if (cameraNode != null)
            //    UpdateCamera(cameraNode);
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
            AssociatedObject.MouseDown += OnMouseDown;
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
            AssociatedObject.MouseDown -= OnMouseDown;
            base.OnDetaching();
        }


        private void OnMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (!eventArgs.Handled && eventArgs.ChangedButton == MouseButton.Left && IsEnabled)
                BeginOrbit(eventArgs);
        }


        private void BeginOrbit(MouseButtonEventArgs eventArgs)
        {
            if (Mouse.Captured != null && !Equals(Mouse.Captured, AssociatedObject))
            {
                // Mouse is already captured by another element.
                return;
            }

            var cameraNode = CameraNode;
            if (cameraNode == null)
            {
                // No camera found.
                return;
            }

            if (!AssociatedObject.CaptureMouse())
            {
                // Failed to capture mouse.
                return;
            }

            _cameraNode = cameraNode;
            _startVector = MapToSphere(eventArgs.GetPosition(AssociatedObject));
            _originalTransform.Minor = cameraNode.PoseWorld.Orientation.Inverse;
            _originalViewMatrix = cameraNode.View;

            AssociatedObject.PreviewKeyDown += OnKeyDown;
            AssociatedObject.LostMouseCapture += OnLostMouseCapture;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseUp += OnMouseUp;

            if (!AssociatedObject.IsKeyboardFocused)
            {
                // Focus element to receive keyboard events.
                AssociatedObject.Focus();
            }

            eventArgs.Handled = true;
        }


        private void EndOrbit(bool commit)
        {
            AssociatedObject.PreviewKeyDown -= OnKeyDown;
            AssociatedObject.LostMouseCapture -= OnLostMouseCapture;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseUp -= OnMouseUp;

            AssociatedObject.ReleaseMouseCapture();

            if (!commit)
            {
                // Operation canceled, revert camera pose.
                _cameraNode.View = _originalViewMatrix;
            }

            _cameraNode = null;
        }


        private void OnKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Escape)
            {
                // Cancel orbiting, revert camera pose.
                EndOrbit(false);
                eventArgs.Handled = true;
            }
        }


        private void OnLostMouseCapture(object sender, MouseEventArgs eventArgs)
        {
            // Cancel orbiting, revert camera pose.
            EndOrbit(false);
            eventArgs.Handled = true;
        }


        private void OnMouseUp(object sender, MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.ChangedButton == MouseButton.Left)
            {
                // End orbiting, commit camera pose.
                EndOrbit(true);
                eventArgs.Handled = true;
            }
        }


        private void OnMouseMove(object sender, MouseEventArgs eventArgs)
        {
            if (eventArgs.LeftButton == MouseButtonState.Pressed)
            {
                // Rotate view.
                Point mousePosition = eventArgs.GetPosition(AssociatedObject);
                Vector3F dragVector = MapToSphere(mousePosition);
                Matrix44F rotation = QuaternionF.CreateRotation(_startVector, dragVector).ToRotationMatrix44();
                _transform = rotation * _originalTransform;
                UpdateCamera(_cameraNode);
            }
        }


        // Maps the mouse position to an imaginary sphere (radius = 1).
        private Vector3F MapToSphere(Point mousePosition)
        {
            // Map mouse position from [0, size in pixels] to [-1.5, 1.5].
            Vector2F p;
            p.X = (float)mousePosition.X / ((float)AssociatedObject.ActualWidth - 1) * 3.0f - 1.5f;
            p.Y = -((float)mousePosition.Y / ((float)AssociatedObject.ActualHeight - 1) * 3.0f - 1.5f);

            // (Note: Since the viewport is usually rectangular, we do not actually map
            // to a sphere, but to an ellipsoid.)

            float lengthSquared = p.LengthSquared;
            if (lengthSquared > 1)
            {
                // The mouse position is outside of the sphere.
                // --> Map mouse position to point on sphere.
                p.Normalize();
                return new Vector3F(p.X, p.Y, 0);
            }

            // The mouse position is on the sphere.
            // --> Return point on sphere.
            return new Vector3F(p.X, p.Y, (float)Math.Sqrt(1 - lengthSquared));
        }


        private void UpdateCamera(CameraNode camera)
        {
            // Camera distance from CameraTarget.
            float distance = (camera.PoseWorld.Position - CameraTarget).Length;

            // Re-orthogonalize to remove numerical errors which could add up.
            var minor = _transform.Minor;
            minor.Orthogonalize();
            _transform.Minor = minor;

            // Create view matrix for look at origin (ignoring CameraTarget).
            camera.View = Matrix44F.CreateTranslation(0, 0, -distance) * _transform;

            // Apply translation to look at CameraTarget.
            var pose = camera.PoseWorld;
            pose.Position += CameraTarget;
            camera.PoseWorld = pose;
        }
        #endregion
    }
}
