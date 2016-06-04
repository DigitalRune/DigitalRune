// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using DigitalRune.Geometry;
using DigitalRune.Graphics.Interop;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Windows;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Allows to control the viewing direction by pressing the left mouse button and moving the
    /// mouse.
    /// </summary>
    /// <remarks>
    /// The mouse controls yaw (heading) and pitch (attitude) of the camera. The roll (bank) is
    /// fixed.
    /// </remarks>
    public class MouseLookBehavior : Behavior<D3DImagePresentationTarget>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private CameraNode _cameraNode;
        private Point _lastMousePosition;
        private Matrix33F _originalOrientation;
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
            typeof(MouseLookBehavior),
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
            typeof(MouseLookBehavior),
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
        /// Identifies the <see cref="Speed"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SpeedProperty = DependencyProperty.Register(
            "Speed",
            typeof(double),
            typeof(MouseLookBehavior),
            new FrameworkPropertyMetadata(0.005));

        /// <summary>
        /// Gets or sets the rotation speed.
        /// This is a dependency property.
        /// </summary>
        /// <value>The rotation speed determines how fast the camera rotates.</value>
        [Description("Gets or sets the zoom speed.")]
        [Category(Categories.Behavior)]
        public double Speed
        {
            get { return (double)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

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
                BeginLooking(eventArgs);
        }


        private void BeginLooking(MouseButtonEventArgs eventArgs)
        {
            if (Mouse.Captured != null && Mouse.Captured != AssociatedObject)
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

            // Try to capture mouse input.
            if (!AssociatedObject.CaptureMouse())
            {
                // Failed to capture mouse.
                return;
            }

            _cameraNode = cameraNode;
            _lastMousePosition = eventArgs.GetPosition(AssociatedObject);
            _originalOrientation = cameraNode.PoseWorld.Orientation;

            AssociatedObject.PreviewKeyDown += OnKeyDown;
            AssociatedObject.LostMouseCapture += OnLostMouseCapture;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseUp += OnMouseUp;

            if (!AssociatedObject.IsKeyboardFocused)
            {
                // Focus view element to receive keyboard events.
                AssociatedObject.Focus();
            }

            eventArgs.Handled = true;
        }


        private void EndLooking(bool commit)
        {
            AssociatedObject.PreviewKeyDown -= OnKeyDown;
            AssociatedObject.LostMouseCapture -= OnLostMouseCapture;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseUp -= OnMouseUp;

            AssociatedObject.ReleaseMouseCapture();

            if (!commit)
            {
                // Operation canceled, revert camera orientation.
                Pose pose = _cameraNode.PoseWorld;
                pose.Orientation = _originalOrientation;
                _cameraNode.PoseWorld = pose;
            }

            _cameraNode = null;
        }


        private void OnKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Escape)
            {
                // Cancel looking, revert camera orientation.
                EndLooking(false);
                eventArgs.Handled = true;
            }
        }


        private void OnLostMouseCapture(object sender, MouseEventArgs eventArgs)
        {
            // Cancel looking, revert camera orientation.
            EndLooking(false);
            eventArgs.Handled = true;
        }


        private void OnMouseUp(object sender, MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.ChangedButton == MouseButton.Left)
            {
                // End looking, commit camera orientation.
                EndLooking(true);
                eventArgs.Handled = true;
            }
        }


        private void OnMouseMove(object sender, MouseEventArgs eventArgs)
        {
            Point mousePosition = eventArgs.GetPosition(AssociatedObject);
            Vector delta = mousePosition - _lastMousePosition;
            UpdateOrientation(delta);
            _lastMousePosition = mousePosition;
        }


        private void UpdateOrientation(Vector delta)
        {
            Pose pose = _cameraNode.PoseWorld;

            // Get previous yaw and pitch angles.
            float yaw, pitch;
            GetYawPitch(pose.Orientation, out yaw, out pitch);

            // Apply delta.
            float speed = (float)Speed;
            yaw -= (float)delta.X * speed;
            pitch -= (float)delta.Y * speed;

            // Set new camera orientation.
            pose.Orientation = Matrix33F.CreateRotationY(yaw) * Matrix33F.CreateRotationX(pitch);
            _cameraNode.PoseWorld = pose;
        }


        private static void GetYawPitch(Matrix33F matrix, out float yaw, out float pitch)
        {
            yaw = (float)Math.Atan2(-matrix.M20, matrix.M00);
            pitch = (float)Math.Asin(-matrix.M12);
        }
        #endregion
    }
}
