// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Appears over the <see cref="DockControl"/> or a <see cref="DockTabPane"/> when a
    /// <see cref="DockTabItem"/> is dragged and visualizes the areas where the user can dock the
    /// window. (Base implementation.)
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default, the <see cref="DockControl"/> uses two types of
    /// <see cref="DockIndicatorOverlay"/>s: <see cref="BorderIndicators"/> that shows dock
    /// indicators at the borders and <see cref="PaneIndicators"/> that shows dock indicators inside
    /// a <see cref="DockTabPane"/>.
    /// </para>
    /// <para>
    /// The control template of the <see cref="BorderIndicators"/> and <see cref="PaneIndicators"/>
    /// can be styled. The control template needs to contain certain template parts
    /// ("PART_DockLeft", "PART_DockRight", etc.). The template parts need to be of type
    /// <see cref="DockIndicator"/>. When the user drags a <see cref="DockTabItem"/> the
    /// <see cref="DockIndicatorOverlay"/> performs a hit test against these parts to select one of
    /// the dock positions.
    /// </para>
    /// </remarks>
    [TemplatePart(Name = "PART_DockLeft", Type = typeof(DockIndicator))]
    [TemplatePart(Name = "PART_DockRight", Type = typeof(DockIndicator))]
    [TemplatePart(Name = "PART_DockTop", Type = typeof(DockIndicator))]
    [TemplatePart(Name = "PART_DockBottom", Type = typeof(DockIndicator))]
    [TemplatePart(Name = "PART_DockInside", Type = typeof(DockIndicator))]
    [TemplateVisualState(GroupName = "DockStates", Name = "None")]
    [TemplateVisualState(GroupName = "DockStates", Name = "Left")]
    [TemplateVisualState(GroupName = "DockStates", Name = "Right")]
    [TemplateVisualState(GroupName = "DockStates", Name = "Top")]
    [TemplateVisualState(GroupName = "DockStates", Name = "Bottom")]
    [TemplateVisualState(GroupName = "DockStates", Name = "Inside")]
    public abstract class DockIndicatorOverlay : DockOverlay
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // _dockIndicators[(int)DockPosition] = DockIndicator
        private readonly DockIndicator[] _dockIndicators = new DockIndicator[6];

        private bool _fadeOutAnimationCompleted;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="AllowDockLeft"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowDockLeftProperty = DependencyProperty.Register(
            "AllowDockLeft",
            typeof(bool),
            typeof(DockIndicatorOverlay),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnAllowedDockPositionChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the it is allowed to dock to the left.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to enable docking to the left; otherwise, 
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the it is allowed to dock to the left.")]
        [Category(Categories.Default)]
        public bool AllowDockLeft
        {
            get { return (bool)GetValue(AllowDockLeftProperty); }
            set { SetValue(AllowDockLeftProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="AllowDockRight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowDockRightProperty = DependencyProperty.Register(
            "AllowDockRight",
            typeof(bool),
            typeof(DockIndicatorOverlay),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnAllowedDockPositionChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the it is allowed to dock to the right.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to enable docking to the right; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the it is allowed to dock to the right.")]
        [Category(Categories.Default)]
        public bool AllowDockRight
        {
            get { return (bool)GetValue(AllowDockRightProperty); }
            set { SetValue(AllowDockRightProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="AllowDockTop"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowDockTopProperty = DependencyProperty.Register(
            "AllowDockTop",
            typeof(bool),
            typeof(DockIndicatorOverlay),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnAllowedDockPositionChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the it is allowed to dock to the top.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to enable docking to the top; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the it is allowed to dock to the top.")]
        [Category(Categories.Default)]
        public bool AllowDockTop
        {
            get { return (bool)GetValue(AllowDockTopProperty); }
            set { SetValue(AllowDockTopProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="AllowDockBottom"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowDockBottomProperty = DependencyProperty.Register(
            "AllowDockBottom",
            typeof(bool),
            typeof(DockIndicatorOverlay),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnAllowedDockPositionChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the it is allowed to dock to the bottom.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to enable docking to the bottom; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the it is allowed to dock to the bottom.")]
        [Category(Categories.Default)]
        public bool AllowDockBottom
        {
            get { return (bool)GetValue(AllowDockBottomProperty); }
            set { SetValue(AllowDockBottomProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="AllowDockInside"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowDockInsideProperty = DependencyProperty.Register(
            "AllowDockInside",
            typeof(bool),
            typeof(DockIndicatorOverlay),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnAllowedDockPositionChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the it is allowed to dock inside the
        /// <see cref="DockTabPane"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to enable docking in the center; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the it is allowed to dock inside the DockTabPane.")]
        [Category(Categories.Default)]
        public bool AllowDockInside
        {
            get { return (bool)GetValue(AllowDockInsideProperty); }
            set { SetValue(AllowDockInsideProperty, Boxed.Get(value)); }
        }


        private static readonly DependencyPropertyKey ResultPropertyKey = DependencyProperty.RegisterReadOnly(
            "Result",
            typeof(DockPosition),
            typeof(DockIndicatorOverlay),
            new FrameworkPropertyMetadata(DockPosition.None, OnResultChanged));

        /// <summary>
        /// Identifies the <see cref="Result"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ResultProperty = ResultPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a value indicating which dock indicator is selected.
        /// This is a dependency property.
        /// </summary>
        [Browsable(false)]
        public DockPosition Result
        {
            get { return (DockPosition)GetValue(ResultProperty); }
            private set { SetValue(ResultPropertyKey, value); }
        }


        /// <summary>
        /// Identifies the <see cref="FadeInAnimation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FadeInAnimationProperty = DependencyProperty.Register(
            "FadeInAnimation",
            typeof(Storyboard),
            typeof(DockIndicatorOverlay),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the animation that is played when the <see cref="DockIndicatorOverlay"/>
        /// appears. This is a dependency property.
        /// </summary>
        /// <value>
        /// A <see cref="Storyboard"/> that is played when the <see cref="DockIndicatorOverlay"/> is
        /// pops up. The default value is <see langword="null"/>.
        /// </value>
        [Description("Gets or sets the animation that is played when the DockIndicatorOverlay appears.")]
        [Category(Categories.Default)]
        public Storyboard FadeInAnimation
        {
            get { return (Storyboard)GetValue(FadeInAnimationProperty); }
            set { SetValue(FadeInAnimationProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="FadeOutAnimation"/> dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "FadeOut")]
        public static readonly DependencyProperty FadeOutAnimationProperty = DependencyProperty.Register(
            "FadeOutAnimation",
            typeof(Storyboard),
            typeof(DockIndicatorOverlay),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the animation that is played when the <see cref="DockIndicatorOverlay"/>
        /// is closed. This is a dependency property.
        /// </summary>
        /// <value>
        /// A <see cref="Storyboard"/> that is played when the <see cref="DockIndicatorOverlay"/> is
        /// closed. The default value is <see langword="null"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "FadeOut")]
        [Description("Gets or sets the animation that is played when the DockIndicatorOverlay disappears.")]
        [Category(Categories.Default)]
        public Storyboard FadeOutAnimation
        {
            get { return (Storyboard)GetValue(FadeOutAnimationProperty); }
            set { SetValue(FadeOutAnimationProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="DockIndicatorOverlay"/> class.
        /// </summary>
        /// <param name="target">
        /// The target element over which the indicators should appear. (Typically the
        /// <see cref="DockControl"/> or one of its <see cref="DockTabPane"/>s.)
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="target"/> is <see langword="null"/>.
        /// </exception>
        protected DockIndicatorOverlay(FrameworkElement target) : base(target)
        {
            Loaded += OnLoaded;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnAllowedDockPositionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var dockIndicatorOverlay = (DockIndicatorOverlay)dependencyObject;
            dockIndicatorOverlay.UpdateDockIndicators();
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal 
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Remove all previously stored DockIndicators.
            Array.Clear(_dockIndicators, 0, _dockIndicators.Length);

            base.OnApplyTemplate();

            // Get the new DockIndicators from the control template.
            _dockIndicators[(int)DockPosition.Left] = GetTemplateChild("PART_DockLeft") as DockIndicator;
            _dockIndicators[(int)DockPosition.Right] = GetTemplateChild("PART_DockRight") as DockIndicator;
            _dockIndicators[(int)DockPosition.Top] = GetTemplateChild("PART_DockTop") as DockIndicator;
            _dockIndicators[(int)DockPosition.Bottom] = GetTemplateChild("PART_DockBottom") as DockIndicator;
            _dockIndicators[(int)DockPosition.Inside] = GetTemplateChild("PART_DockInside") as DockIndicator;

            UpdateDockIndicators();
            UpdateVisualStates(false);
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            UpdateDockIndicators();

            // Start fade-in animation as soon as overlay is loaded.
            FadeInAnimation?.Begin(this, true);
        }


        /// <summary>
        /// Raises the <see cref="Window.Closing"/> event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="CancelEventArgs"/> that contains the event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            var fadeOutAnimation = FadeOutAnimation;
            if (!_fadeOutAnimationCompleted && fadeOutAnimation != null)
            {
                // Stop fade-in animation.
                FadeInAnimation?.Stop(this);

                // Start fade-out animation and prevent window from closing until 
                // fade-out animation has finished.
                Storyboard clonedAnimation = fadeOutAnimation.Clone();
                clonedAnimation.Completed += OnFadeOutAnimationCompleted;
                clonedAnimation.Begin(this);
                e.Cancel = true;
            }
        }


        private void OnFadeOutAnimationCompleted(object sender, EventArgs eventArgs)
        {
            // Closing of the window is deferred until the fade-out animation has finished.
            _fadeOutAnimationCompleted = true;
            Close();
        }


        private void UpdateDockIndicators()
        {
            var allowDockLeft = AllowDockLeft;
            var allowDockRight = AllowDockRight;
            var allowDockTop = AllowDockTop;
            var allowDockBottom = AllowDockBottom;
            var allowDockInside = AllowDockInside;
            if (allowDockLeft || allowDockRight || allowDockTop || allowDockBottom || allowDockInside)
            {
                // At least one DockPosition is allowed.
                Visibility = Visibility.Visible;
                UpdateDockIndicator(DockPosition.Left, allowDockLeft);
                UpdateDockIndicator(DockPosition.Right, allowDockRight);
                UpdateDockIndicator(DockPosition.Top, allowDockTop);
                UpdateDockIndicator(DockPosition.Bottom, allowDockBottom);
                UpdateDockIndicator(DockPosition.Inside, allowDockInside);
            }
            else
            {
                // No docking allowed. Hide entire overlay window.
                Visibility = Visibility.Collapsed;
            }
        }


        private void UpdateDockIndicator(DockPosition position, bool isEnabled)
        {
            var dockIndicator = _dockIndicators[(int)position];
            if (dockIndicator != null)
                dockIndicator.IsEnabled = isEnabled;
        }


        private static void OnResultChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var dockIndicatorOverlay = (DockIndicatorOverlay)dependencyObject;
            dockIndicatorOverlay.SelectDockIndicator();
        }


        private void SelectDockIndicator()
        {
            var result = Result;
            for (int i = 0; i < _dockIndicators.Length; i++)
            {
                var dockIndicator = _dockIndicators[i];
                if (dockIndicator != null)
                    dockIndicator.IsSelected = (result == (DockPosition)i);
            }
        }


        private void UpdateVisualStates(bool useTransitions)
        {
            switch (Result)
            {
                case DockPosition.None:
                    VisualStateManager.GoToState(this, "None", useTransitions);
                    break;
                case DockPosition.Left:
                    VisualStateManager.GoToState(this, "Left", useTransitions);
                    break;
                case DockPosition.Right:
                    VisualStateManager.GoToState(this, "Right", useTransitions);
                    break;
                case DockPosition.Top:
                    VisualStateManager.GoToState(this, "Top", useTransitions);
                    break;
                case DockPosition.Bottom:
                    VisualStateManager.GoToState(this, "Bottom", useTransitions);
                    break;
                case DockPosition.Inside:
                    VisualStateManager.GoToState(this, "Inside", useTransitions);
                    break;
            }
        }


        /// <summary>
        /// Clears the current hit-test result.
        /// </summary>
        public void ClearResult()
        {
            ClearValue(ResultPropertyKey);
            UpdateVisualStates(true);
        }


        /// <summary>
        /// Tests whether the mouse hits one of the <see cref="UIElement"/>s.
        /// </summary>
        /// <returns>A result that indicates which <see cref="UIElement"/> was hit.</returns>
        public DockPosition HitTest()
        {
            var result = DockPosition.None;
            for (int i = 0; i < _dockIndicators.Length; i++)
            {
                var dockIndicator = _dockIndicators[i];
                if (dockIndicator != null && dockIndicator.IsEnabled)
                {
                    bool hit = DockHelper.HitTest(dockIndicator);
                    if (hit)
                    {
                        result = (DockPosition)i;
                        break;
                    }
                }
            }

            Result = result;
            UpdateVisualStates(true);
            return result;
        }
        #endregion
    }
}
