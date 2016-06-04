// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace DigitalRune.Windows.Docking
{
    [TemplatePart(Name = "PART_AutoHideBarLeft", Type = typeof(AutoHideBar))]
    [TemplatePart(Name = "PART_AutoHideBarRight", Type = typeof(AutoHideBar))]
    [TemplatePart(Name = "PART_AutoHideBarTop", Type = typeof(AutoHideBar))]
    [TemplatePart(Name = "PART_AutoHideBarBottom", Type = typeof(AutoHideBar))]
    [TemplatePart(Name = "PART_AutoHidePanel", Type = typeof(Panel))]
    partial class DockControl
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private AutoHideBar _leftAutoHideBar;
        private AutoHideBar _rightAutoHideBar;
        private AutoHideBar _topAutoHideBar;
        private AutoHideBar _bottomAutoHideBar;
        private AutoHideBar[] _autoHideBars = Array.Empty<AutoHideBar>();
        private Panel _autoHidePanel;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
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
            _contentPresenter = null;

            foreach (var autoHideBar in _autoHideBars)
                autoHideBar.TargetArea = null;

            _leftAutoHideBar = null;
            _rightAutoHideBar = null;
            _topAutoHideBar = null;
            _bottomAutoHideBar = null;
            _autoHideBars = null;

            if (_autoHidePanel != null)
            {
                _autoHidePanel.Children.Clear();
                _autoHidePanel = null;
            }

            base.OnApplyTemplate();

            _contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
            _autoHidePanel = GetTemplateChild("PART_AutoHidePanel") as Panel;
            _leftAutoHideBar = GetTemplateChild("PART_AutoHideBarLeft") as AutoHideBar;
            _rightAutoHideBar = GetTemplateChild("PART_AutoHideBarRight") as AutoHideBar;
            _topAutoHideBar = GetTemplateChild("PART_AutoHideBarTop") as AutoHideBar;
            _bottomAutoHideBar = GetTemplateChild("PART_AutoHideBarBottom") as AutoHideBar;

            var autoHideBars = new List<AutoHideBar>(4);
            if (_leftAutoHideBar != null)
            {
                _leftAutoHideBar.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(IDockControl.AutoHideLeft)));
                autoHideBars.Add(_leftAutoHideBar);
            }
            if (_rightAutoHideBar != null)
            {
                _rightAutoHideBar.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(IDockControl.AutoHideRight)));
                autoHideBars.Add(_rightAutoHideBar);
            }
            if (_topAutoHideBar != null)
            {
                _topAutoHideBar.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(IDockControl.AutoHideTop)));
                autoHideBars.Add(_topAutoHideBar);
            }
            if (_bottomAutoHideBar != null)
            {
                _bottomAutoHideBar.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(IDockControl.AutoHideBottom)));
                autoHideBars.Add(_bottomAutoHideBar);
            }

            _autoHideBars = autoHideBars.ToArray();

            foreach (var autoHideBar in _autoHideBars)
                autoHideBar.TargetArea = _autoHidePanel;
        }


        private void ShowAutoHidePane(IDockTabPane dockTabPane, IDockTabItem dockTabItem)
        {
            CloseAutoHidePanesExcept(dockTabPane);
            foreach (var autoHideBar in _autoHideBars)
            {
                var autoHidePane = autoHideBar.ShowAutoHidePane(dockTabPane, dockTabItem, true);
                if (autoHidePane != null)
                    break;
            }
        }


        /// <summary>
        /// Closes all open <see cref="AutoHidePane"/>s.
        /// </summary>
        internal void CloseAutoHidePanes()
        {
            foreach (var autoHideBar in _autoHideBars)
                autoHideBar.CloseAutoHidePanes();
        }


        /// <summary>
        /// Closes all open <see cref="AutoHidePane"/>s except one.
        /// </summary>
        /// <param name="dockTabPane">
        /// The content of the <see cref="AutoHidePane"/> that should stay open.
        /// </param>
        internal void CloseAutoHidePanesExcept(IDockTabPane dockTabPane)
        {
            foreach (var autoHideBar in _autoHideBars)
                autoHideBar.CloseAutoHidePanesExcept(dockTabPane);
        }
        #endregion
    }
}
