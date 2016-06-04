// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using DigitalRune.Geometry;
using DigitalRune.Graphics.Interop;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Windows;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Allows to zoom the camera using the mouse wheel.
    /// </summary>
    /// <remarks>
    /// Press SHIFT to zoom the camera, CONTROL to zoom the camera slower.
    /// </remarks>
    public class MouseZoomBehavior : Behavior<D3DImagePresentationTarget>
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
            typeof(MouseZoomBehavior),
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
            typeof(MouseZoomBehavior),
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
            typeof(MouseZoomBehavior),
            new FrameworkPropertyMetadata(0.5));

        /// <summary>
        /// Gets or sets the zoom speed.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The zoom speed determines the distance the camera is moved when the mouse wheel is
        /// rotated by one increment. Set a negative value to invert the zoom direction.
        /// </value>
        [Description("Gets or sets the zoom speed.")]
        [Category(Categories.Behavior)]
        public double Speed
        {
            get { return (double)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="CameraTarget"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CameraTargetProperty = DependencyProperty.Register(
            "CameraTarget",
            typeof(Vector3F),
            typeof(MouseZoomBehavior),
            new FrameworkPropertyMetadata(Vector3F.Zero));

        /// <summary>
        /// Gets or sets the center around which the camera is orbiting.
        /// This is a dependency property.
        /// </summary>
        /// <value>The center around which the camera is orbiting.</value>
        /// <remarks>
        /// <see cref="MinDistance"/> and <see cref="MaxDistance"/> limit the distance of the camera
        /// to the <see cref="CameraTarget"/>. The distance limits can be disabled by setting
        /// <see cref="MinDistance"/> and/or <see cref="MaxDistance"/> to NaN.
        /// <see cref="CameraTarget"/> only needs to be set if a distance limit is enabled.
        /// </remarks>
        [Description("Gets or sets the center around which the camera is orbiting.")]
        [Category(Categories.Behavior)]
        public Vector3F CameraTarget
        {
            get { return (Vector3F)GetValue(CameraTargetProperty); }
            set { SetValue(CameraTargetProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MinDistance"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinDistanceProperty = DependencyProperty.Register(
            "MinDistance",
            typeof(double),
            typeof(MouseZoomBehavior),
            new FrameworkPropertyMetadata(Boxed.DoubleNaN));

        /// <summary>
        /// Gets or sets the minimum distance between the camera and the camera target.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The minimum distance between the camera and the camera target.
        /// The default value is NaN, which means the min distance limit is disabled.
        /// </value>
        /// <inheritdoc cref="CameraTarget"/>
        [Description("Gets or sets the minimum distance between the camera and the camera target.")]
        [Category(Categories.Behavior)]
        public double MinDistance
        {
            get { return (double)GetValue(MinDistanceProperty); }
            set { SetValue(MinDistanceProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MaxDistance"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxDistanceProperty = DependencyProperty.Register(
            "MaxDistance",
            typeof(double),
            typeof(MouseZoomBehavior),
            new FrameworkPropertyMetadata(Boxed.DoubleNaN));

        /// <summary>
        /// Gets or sets the maximum distance between the camera and the camera target.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The maximum distance between the camera and the camera target.
        /// The default value is NaN, which means the max distance limit is disabled.
        /// </value>
        /// <inheritdoc cref="CameraTarget"/>
        [Description("Gets or sets the maximum distance between the camera and the camera target.")]
        [Category(Categories.Behavior)]
        public double MaxDistance
        {
            get { return (double)GetValue(MaxDistanceProperty); }
            set { SetValue(MaxDistanceProperty, value); }
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
            AssociatedObject.MouseWheel += OnMouseWheel;
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
            AssociatedObject.MouseWheel -= OnMouseWheel;
            base.OnDetaching();
        }


        private void OnMouseWheel(object sender, MouseWheelEventArgs eventArgs)
        {
            if (eventArgs.Handled || !IsEnabled)
                return;

            var cameraNode = CameraNode;
            if (cameraNode == null)
                return;

            float modifier = 1;
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                modifier *= 10;
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                modifier /= 10;

            float increments = (float)eventArgs.Delta / Mouse.MouseWheelDeltaForOneLine;
            float distanceChange = (float)Speed * increments * modifier;

            // Move camera in forward direction.
            Pose pose = cameraNode.PoseWorld;
            Vector3F forward = pose.ToWorldDirection(Vector3F.Forward);
            pose.Position += forward * distanceChange;

            // Apply min/max distance limits.
            if (Numeric.IsFinite(MinDistance) || Numeric.IsFinite(MaxDistance))
            {
                var cameraToTarget = CameraTarget - pose.Position;
                float cameraDistance = cameraToTarget.Length;

                // cameraDistance is negative if the target is behind the camera.
                if (Vector3F.Dot(cameraToTarget, forward) < 0)
                    cameraDistance = -cameraDistance;

                // Normally, cameraToTarget should be equal to forward and the min/max
                // limits should only be used with an orbiting camera. However, if the user
                // makes strange use of the distance limits, it is safer to use 
                // cameraToTarget (and only fall back to forward if necessary).
                if (!cameraToTarget.TryNormalize())
                    cameraToTarget = forward;

                // Following conditions are safe to use with NaN.
                if (cameraDistance < MinDistance)
                    cameraDistance = (float)MinDistance;
                if (cameraDistance > MaxDistance)
                    cameraDistance = (float)MaxDistance;

                pose.Position = CameraTarget - cameraToTarget * cameraDistance;
            }

            cameraNode.PoseWorld = pose;
            eventArgs.Handled = true;
        }
        #endregion
    }
}
