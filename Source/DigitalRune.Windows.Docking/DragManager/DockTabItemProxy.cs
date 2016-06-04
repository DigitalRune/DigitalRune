// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;


namespace DigitalRune.Windows.Docking
{
    internal class DockTabItemProxy : IDockTabItem
    {
        public IDockTabItem Item { get; }

        public DockState DockState
        {
            get { return DockState.Hide; }
            set { Debug.Fail("DockTabItemProxies.DockState should never be changed."); }
        }

        // Wrap original properties.
        public DockState LastDockState
        {
            get { return Item.LastDockState; }
            set { Item.LastDockState = value; }
        }

        public GridLength DockWidth
        {
            get { return Item.DockWidth; }
            set { Item.DockWidth = value; }
        }

        public GridLength DockHeight
        {
            get { return Item.DockHeight; }
            set { Item.DockHeight = value; }
        }

        public bool IsPersistent
        {
            get { return Item.IsPersistent; }
        }

        public DateTime LastActivation
        {
            get { return Item.LastActivation; }
            set { Item.LastActivation = value; }
        }

        public double AutoHideWidth
        {
            get { return Item.AutoHideWidth; }
            set { Item.AutoHideWidth = value; }
        }

        public double AutoHideHeight
        {
            get { return Item.AutoHideHeight; }
            set { Item.AutoHideHeight = value; }
        }

        public object Icon
        {
            get { return Item.Icon; }
        }

        public string Title
        {
            get { return Item.Title; }
        }

        public string DockId
        {
            get { return Item.DockId; }
        }


        public DockTabItemProxy(IDockTabItem item)
        {
            Debug.Assert(item != null);

            Item = item;
        }
    }
}
