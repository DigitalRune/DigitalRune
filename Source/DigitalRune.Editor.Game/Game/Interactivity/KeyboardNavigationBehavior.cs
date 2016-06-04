// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using DigitalRune.Game.Timing;
using DigitalRune.Geometry;
using DigitalRune.Graphics.Interop;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Windows;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Allows to move the camera using the WASD and RF keys.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use the following keys control the camera:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Key</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item><term>W</term><description>Forward</description></item>
    /// <item><term>A</term><description>Left</description></item>
    /// <item><term>S</term><description>Backward</description></item>
    /// <item><term>D</term><description>Right</description></item>
    /// <item><term>R</term><description>Up ("Raise")</description></item>
    /// <item><term>F</term><description>Down ("Fall")</description></item>
    /// </list>
    /// <para>
    /// Press SHIFT to move the camera, CONTROL to move the camera slower.
    /// </para>
    /// </remarks>
    public class KeyboardNavigationBehavior : Behavior<D3DImagePresentationTarget>
    {
        // Notes:
        // We could use the WPF input events, but currently we use polling.
        // We use the game timer to poll the input device. (DispatcherTimer is too irregular.)


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private GameExtension _gameExtension;
        private IGameTimer _gameTimer;
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
            typeof(KeyboardNavigationBehavior),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnIsEnabledChanged));

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
            set { SetValue(IsEnabledProperty, Boxed.BooleanTrue); }
        }


        /// <summary>
        /// Identifies the <see cref="CameraNode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CameraNodeProperty = DependencyProperty.Register(
            "CameraNode",
            typeof(CameraNode),
            typeof(KeyboardNavigationBehavior),
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
            typeof(KeyboardNavigationBehavior),
            new FrameworkPropertyMetadata(5.0));


        /// <summary>
        /// Gets or sets the camera speed.
        /// This is a dependency property.
        /// </summary>
        /// <value>The camera speed.</value>
        [Description("Gets or sets the camera speed.")]
        [Category(Categories.Behavior)]
        public double Speed
        {
            get { return (double)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
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

            if (WindowsHelper.IsInDesignMode)
                return;

            AssociatedObject.GotFocus += OnFocusChanged;
            AssociatedObject.LostFocus += OnFocusChanged;
            AssociatedObject.MouseDown += OnMouseDown;
            AssociatedObject.KeyDown += OnKeyDown;

            UpdateTimer();
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
            AssociatedObject.GotFocus -= OnFocusChanged;
            AssociatedObject.LostFocus -= OnFocusChanged;
            AssociatedObject.MouseDown -= OnMouseDown;
            AssociatedObject.KeyDown -= OnKeyDown;

            base.OnDetaching();

            UpdateTimer();
            _gameTimer = null;
        }


        private static void OnIsEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (KeyboardNavigationBehavior)dependencyObject;
            behavior.UpdateTimer();
        }


        private void OnFocusChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            UpdateTimer();
        }


        private void UpdateTimer()
        {
            if (WindowsHelper.IsInDesignMode)
                return;

            if (_gameTimer == null)
            {
                if (AssociatedObject.IsLoaded)
                {
                    var editor = EditorHelper.GetEditor(AssociatedObject).ThrowIfMissing();
                    _gameTimer = editor.Services.GetInstance<IGameTimer>().ThrowIfMissing();
                    _gameExtension = editor.Extensions.OfType<GameExtension>().FirstOrDefault().ThrowIfMissing();
                }
                else
                {
                    AssociatedObject.Loaded += (s, e) => UpdateTimer();
                    return;
                }
            }

            // Start/stop timer based on current state.
            var view = AssociatedObject;
            if (view != null && view.IsFocused && IsEnabled)
                _gameExtension.GameLoopNewFrame += OnTick;
            else
                _gameExtension.GameLoopNewFrame -= OnTick;
        }


        private void OnMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            // Focus view element to receive keyboard input.
            AssociatedObject.Focus();
        }


        private void OnTick(object sender, EventArgs eventArgs)
        {
            if (!AssociatedObject.IsKeyboardFocused)
                return;

            // Poll keyboard and update camera.
            bool wDown = Keyboard.IsKeyDown(Key.W);
            bool aDown = Keyboard.IsKeyDown(Key.A);
            bool sDown = Keyboard.IsKeyDown(Key.S);
            bool dDown = Keyboard.IsKeyDown(Key.D);
            bool rDown = Keyboard.IsKeyDown(Key.R);
            bool fDown = Keyboard.IsKeyDown(Key.F);
            if (!wDown && !aDown && !sDown && !dDown && !rDown && !fDown)
                return;

            var cameraNode = CameraNode;
            if (cameraNode == null)
                return;

            float dt = (float)_gameTimer.DeltaTime.TotalSeconds;

            // Get direction vectors from camera.
            Pose pose = cameraNode.PoseWorld;
            Vector3F forward = pose.ToWorldDirection(Vector3F.Forward);
            Vector3F right = pose.ToWorldDirection(Vector3F.Right);
            Vector3F up = pose.ToWorldDirection(Vector3F.Up);

            // Determine navigation direction.
            Vector3F direction = Vector3F.Zero;
            if (wDown)
                direction += forward;
            if (aDown)
                direction -= right;
            if (sDown)
                direction -= forward;
            if (dDown)
                direction += right;
            if (rDown)
                direction += up;
            if (fDown)
                direction -= up;

            float modifier = 1;
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                modifier *= 10;
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                modifier /= 10;

            // Move camera.
            direction.TryNormalize();
            Vector3F offset = direction * (float)Speed * dt * modifier;
            pose.Position += offset;
            cameraNode.PoseWorld = pose;
        }


        private void OnKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (!IsEnabled)
                return;

            if (eventArgs.Key == Key.W
                || eventArgs.Key == Key.A
                || eventArgs.Key == Key.S
                || eventArgs.Key == Key.D
                || eventArgs.Key == Key.R
                || eventArgs.Key == Key.F)
            {
                // Set event only to handled. The keys are polled in OnTick.
                eventArgs.Handled = true;
            }
        }
        #endregion
    }
}
