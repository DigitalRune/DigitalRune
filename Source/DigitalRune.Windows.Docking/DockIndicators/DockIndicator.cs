// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents an area inside a <see cref="DockIndicatorOverlay"/> where the user can dock a
    /// <see cref="DockTabItem"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="DockIndicatorOverlay"/> pops up when the users starts dragging a
    /// <see cref="DockTabItem"/>. The overlay automatically appears over the
    /// <see cref="DockControl"/>, <see cref="DockTabPane"/>s and <see cref="FloatWindow"/>s when
    /// the user is dragging a <see cref="DockTabItem"/>. It indicates where the user can drop the
    /// dragged window. A <see cref="DockIndicatorOverlay"/> usually has several
    /// <see cref="DockIndicator"/>s that indicate the dock positions, such as "Dock left", "Dock
    /// right", "Dock inside", etc. The <see cref="DockIndicator"/> itself has no special functions
    /// other than that the user can drop something on it.
    /// </para>
    /// </remarks>
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Selected")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    public class DockIndicator : ContentControl
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="IsSelected"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected",
            typeof(bool),
            typeof(DockIndicator),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, OnStateChanged));

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DockIndicator"/> is currently
        /// selected. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="DockIndicator"/> is selected; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This property is set by the parent <see cref="DockIndicatorOverlay"/> when the user has
        /// selected this dock position. The property is usually used in the
        /// <see cref="Control.Template"/> to highlight the control when it is selected.
        /// </remarks>
        [Description("Gets or sets a value indicating whether this dock position is currently selected.")]
        [Category(Categories.Default)]
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, Boxed.Get(value)); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="DockIndicator"/> class.
        /// </summary>
        static DockIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockIndicator), new FrameworkPropertyMetadata(typeof(DockIndicator)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockIndicator"/> class.
        /// </summary>
        public DockIndicator()
        {
            IsEnabledChanged += OnStateChanged;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualStates(false);
        }


        private static void OnStateChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            var dockIndicator = (DockIndicator)sender;
            dockIndicator.UpdateVisualStates(true);
        }


        private void UpdateVisualStates(bool useTransitions)
        {
            if (IsEnabled)
            {
                if (IsSelected)
                    VisualStateManager.GoToState(this, "Selected", useTransitions);
                else
                    VisualStateManager.GoToState(this, "Normal", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
            }
        }
        #endregion
    }
}
