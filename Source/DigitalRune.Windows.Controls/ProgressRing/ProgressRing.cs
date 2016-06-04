#region ----- Copyright -----
/*
   This control is a modified version of the ProgressRing implemented in MahApps.Metro (see
   https://github.com/MahApps/MahApps.Metro) which is licensed under Ms-PL (see below).


    Microsoft Public License (Ms-PL)

    This license governs use of the accompanying software. If you use the software, you accept this 
    license. If you do not accept the license, do not use the software.

    1. Definitions
    The terms “reproduce,” “reproduction,” “derivative works,” and “distribution” have the same 
    meaning here as under U.S. copyright law.
    A “contribution” is the original software, or any additions or changes to the software.
    A “contributor” is any person that distributes its contribution under this license.
    “Licensed patents” are a contributor’s patent claims that read directly on its contribution.

    2. Grant of Rights
    (A) Copyright Grant- Subject to the terms of this license, including the license conditions and 
        limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
        copyright license to reproduce its contribution, prepare derivative works of its contribution, 
        and distribute its contribution or any derivative works that you create.
    (B) Patent Grant- Subject to the terms of this license, including the license conditions and 
        limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
        license under its licensed patents to make, have made, use, sell, offer for sale, import, 
        and/or otherwise dispose of its contribution in the software or derivative works of the 
        contribution in the software.

    3. Conditions and Limitations
    (A) No Trademark License- This license does not grant you rights to use any contributors’ name, 
        logo, or trademarks.
    (B) If you bring a patent claim against any contributor over patents that you claim are infringed 
        by the software, your patent license from such contributor to the software ends automatically.
    (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, 
        and attribution notices that are present in the software.
    (D) If you distribute any portion of the software in source code form, you may do so only under 
        this license by including a complete copy of this license with your distribution. If you 
        distribute any portion of the software in compiled or object code form, you may only do so 
        under a license that complies with this license.
    (E) The software is licensed “as-is.” You bear the risk of using it. The contributors give no 
        express warranties, guarantees or conditions. You may have additional consumer rights under 
        your local laws which this license cannot change. To the extent permitted under your local 
        laws, the contributors exclude the implied warranties of merchantability, fitness for a 
        particular purpose and non-infringement. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a control that indicates the progress of an ongoing operation in the form of dots
    /// rotating in a circle.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This control is a port of the Windows Runtime
    /// <see href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.controls.progressring">ProgressRing</see>.
    /// </para>
    /// <para>
    /// The dependency property <see cref="IsActive"/> starts and stops the animation. The property
    /// also sets the <see cref="Visibility"/> of the control.
    /// </para>
    /// </remarks>
    [TemplateVisualState(Name = "Large", GroupName = "SizeStates")]
    [TemplateVisualState(Name = "Small", GroupName = "SizeStates")]
    [TemplateVisualState(Name = "Inactive", GroupName = "ActiveStates")]
    [TemplateVisualState(Name = "Active", GroupName = "ActiveStates")]
    public class ProgressRing : Control
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // Defer actions until template is applied.
        private List<Action> _deferredActions = new List<Action>();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="IsActive"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive",
            typeof(bool),
            typeof(ProgressRing),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsActiveChanged));

        /// <summary>
        /// Gets or sets value indicating whether the operation is in progress.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the operation is in progress; otherwise,
        /// <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the operation is in progress.")]
        [Category(Categories.Default)]
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="IsLarge"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLargeProperty = DependencyProperty.Register(
            "IsLarge",
            typeof(bool),
            typeof(ProgressRing),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnIsLargeChanged));

        /// <summary>
        /// Gets or sets value indicating whether a sixth rotating dot should be rendered.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to show 6 dots; otherwise, <see langword="false"/> to show 5
        /// dots.
        /// </value>
        [Description("Gets or sets a value indicating whether a sixth rotating dot should be rendered.")]
        [Category(Categories.Default)]
        public bool IsLarge
        {
            get { return (bool)GetValue(IsLargeProperty); }
            set { SetValue(IsLargeProperty, Boxed.Get(value)); }
        }


        private static readonly DependencyPropertyKey MaxSideLengthPropertyKey = DependencyProperty.RegisterReadOnly(
            "MaxSideLength",
            typeof(double),
            typeof(ProgressRing),
            new FrameworkPropertyMetadata(Boxed.DoubleZero));

        /// <summary>
        /// Identifies the <see cref="MaxSideLength"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxSideLengthProperty = MaxSideLengthPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the size of the progress ring.
        /// This is a dependency property.
        /// </summary>
        /// <value>The size of the progress ring.</value>
        [Browsable(false)]
        public double MaxSideLength
        {
            get { return (double)GetValue(MaxSideLengthProperty); }
            private set { SetValue(MaxSideLengthPropertyKey, value); }
        }


        private static readonly DependencyPropertyKey EllipseDiameterPropertyKey = DependencyProperty.RegisterReadOnly(
            "EllipseDiameter",
            typeof(double),
            typeof(ProgressRing),
            new FrameworkPropertyMetadata(Boxed.DoubleZero));

        /// <summary>
        /// Identifies the <see cref="EllipseDiameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EllipseDiameterProperty = EllipseDiameterPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the dot diameter.
        /// This is a dependency property.
        /// </summary>
        /// <value>The dot diameter.</value>
        [Browsable(false)]
        public double EllipseDiameter
        {
            get { return (double)GetValue(EllipseDiameterProperty); }
            private set { SetValue(EllipseDiameterPropertyKey, value); }
        }


        private static readonly DependencyPropertyKey EllipseOffsetPropertyKey = DependencyProperty.RegisterReadOnly(
            "EllipseOffset",
            typeof(Thickness),
            typeof(ProgressRing),
            new FrameworkPropertyMetadata(default(Thickness)));

        /// <summary>
        /// Identifies the <see cref="EllipseOffset"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EllipseOffsetProperty = EllipseOffsetPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the dot offset.
        /// This is a dependency property.
        /// </summary>
        /// <value>The dot offset.</value>
        [Browsable(false)]
        public Thickness EllipseOffset
        {
            get { return (Thickness)GetValue(EllipseOffsetProperty); }
            private set { SetValue(EllipseOffsetPropertyKey, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="ProgressRing"/> class.
        /// </summary>
        static ProgressRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(typeof(ProgressRing)));
            VisibilityProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(OnVisibilityChanged));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressRing"/> class.
        /// </summary>
        public ProgressRing()
        {
            SizeChanged += OnSizeChanged;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnIsActiveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (ProgressRing)dependencyObject;
            control.UpdateActiveState();
        }


        private static void OnIsLargeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (ProgressRing)dependencyObject;
            control.UpdateLargeState();
        }


        private static void OnVisibilityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (ProgressRing)sender;
            var oldValue = (Visibility)eventArgs.OldValue;
            var newValue = (Visibility)eventArgs.NewValue;
            control.OnVisibilityChanged(oldValue, newValue);
        }


        private void OnVisibilityChanged(Visibility oldValue, Visibility newValue)
        {
            if (newValue != oldValue)
            {
                if (newValue != Visibility.Visible)
                {
                    // Set the value without overriding it's binding (if any).
                    SetCurrentValue(IsActiveProperty, false);
                }
                else
                {
                    // Don't forget to re-activate.
                    IsActive = true;
                }
            }
        }


        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            ExecuteOrDefer(() =>
            {
                double width = ActualWidth;
                MaxSideLength = width <= 20 ? 20 : width;
                EllipseDiameter = width / 8;

                // Apply an offset to move the dot from left/top to left/center of the Canvas.
                EllipseOffset = new Thickness(0, width / 2 - width / 16, 0, 0);
            });
        }


        private void ExecuteOrDefer(Action action)
        {
            if (_deferredActions != null)
                _deferredActions.Add(action);
            else
                action();
        }


        private void ExecuteDeferredActions()
        {
            if (_deferredActions != null)
                foreach (var action in _deferredActions)
                    action();

            _deferredActions = null;
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            UpdateLargeState();
            UpdateActiveState();
            base.OnApplyTemplate();
            ExecuteDeferredActions();
        }


        private void UpdateLargeState()
        {
            if (IsLarge)
                ExecuteOrDefer(() => VisualStateManager.GoToState(this, "Large", true));
            else
                ExecuteOrDefer(() => VisualStateManager.GoToState(this, "Small", true));
        }


        private void UpdateActiveState()
        {
            if (IsActive)
                ExecuteOrDefer(() => VisualStateManager.GoToState(this, "Active", true));
            else
                ExecuteOrDefer(() => VisualStateManager.GoToState(this, "Inactive", true));
        }
        #endregion
    }
}
