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
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Windows;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Allows to orbit the camera around the origin by pressing the left mouse button and moving
    /// the mouse. (This behavior is also known as "turntable" navigation.)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The behavior takes exclusive control of the camera and cannot be used in conjunction with
    /// other navigation behaviors that control camera position and orientation (e.g.
    /// <see cref="KeyboardNavigationBehavior"/>, <see cref="MouseLookBehavior"/>, ...)! It can be
    /// used with the <see cref="MouseZoomBehavior"/>.
    /// </para>
    /// <para>
    /// "Turntable" (<see cref="TurntableBehavior"/>) and "arcball" (<see cref="ArcballBehavior"/>)
    /// are the most common navigation modes for orbiting the camera around a target.
    /// "Turntable" is the default mode in many 3D modeling tools.
    /// </para>
    /// <para>
    /// According to user studies [1], "turntable" is the preferred navigation mode (highest
    /// performance and user satisfaction). However, [2] has found no significant differences
    /// between different navigation modes.
    /// </para>
    /// <para>
    /// <strong>References:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// [1] Ragnar Bade, Felix Ritter, and Bernhard Preim: <i>Usability comparison of mouse-based
    /// interaction techniques for predictable 3d rotation.</i> In Proceedings of the 5th
    /// international conference on Smart Graphics (SG'05)
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// [2] Yao Jun Zhao, Dmitri Shuralyov, and Wolfgang Stuerzlinger: <i>Comparison of multiple 3D
    /// rotation methods.</i> VECIMS, page 13-17. IEEE, (2011)
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public class TurntableBehavior : Behavior<D3DImagePresentationTarget>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private CameraNode _cameraNode;
        private Matrix44F _originalViewMatrix;

        private float _upDirection;
        private Point _lastMousePosition;
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
            typeof(TurntableBehavior),
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
            typeof(TurntableBehavior),
            new FrameworkPropertyMetadata(null));


        /// <summary>
        /// Gets or sets the camera node. This is a dependency property.
        /// </summary>
        /// <value>The camera node.</value>
        [Description("Gets or sets the camera node.")]
        [Category(Categories.Default)]
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
            typeof(TurntableBehavior),
            new PropertyMetadata(Vector3F.Zero, OnCameraTargetChanged));

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


        /// <summary>
        /// Identifies the <see cref="Speed"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SpeedProperty = DependencyProperty.Register(
            "Speed",
            typeof(double),
            typeof(TurntableBehavior),
            new FrameworkPropertyMetadata(ConstantsD.TwoPi / 2000));

        /// <summary>
        /// Gets or sets the rotation speed mode.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The rotation speed in radian per pixel. The default value is
        /// 2 * Pi rad / 2000 px, which means a movement of 2000 pixel causes a 360° rotation.
        /// </value>
        [Description("Gets or sets the rotation speed.")]
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
        /// Called when the <see cref="CameraTarget"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnCameraTargetChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (TurntableBehavior)dependencyObject;
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
            _originalViewMatrix = cameraNode.View;

            _lastMousePosition = eventArgs.GetPosition(AssociatedObject);

            // The rotation direction around the world up axis depends on the initial
            // camera orientation. The direction is set when the manipulation starts
            // and is fixed during the manipulation. (This is necessary to avoid instant
            // switches when the camera turns upside down.)
            Vector3F up = _cameraNode.PoseWorld.ToWorldDirection(Vector3F.Up);
            _upDirection = Math.Sign(up.Y);

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
            var mousePosition = eventArgs.GetPosition(AssociatedObject);

            if (eventArgs.LeftButton == MouseButtonState.Pressed)
            {
                var target = CameraTarget;
                float speed = (float)Speed;
                Vector delta = mousePosition - _lastMousePosition;

                // Rotation around y-axis in world space
                float angle0 = (float)delta.X * speed * _upDirection;
                Pose rotation0 = new Pose(Matrix33F.CreateRotationY(angle0));

                // Rotation around x-axis in view space.
                float angle1 = (float)delta.Y * speed;
                Pose rotation1 = new Pose(Matrix33F.CreateRotationX(angle1));
                float distance = (CameraTarget - _cameraNode.PoseWorld.Position).Length;

                // Variant #1: Set camera pose.
                var pose = new Pose(target)
                           * rotation0.Inverse
                           * new Pose(-target)
                           * _cameraNode.PoseWorld
                           * new Pose(new Vector3F(0, 0, -distance))
                           * rotation1.Inverse
                           * new Pose(new Vector3F(0, 0, distance));

                // Re-orthogonalize to remove numerical errors which could add up.
                var orientation = pose.Orientation;
                orientation.Orthogonalize();
                pose.Orientation = orientation;

                CameraNode.PoseWorld = pose;
            }

            _lastMousePosition = mousePosition;
        }
        #endregion
    }
}
