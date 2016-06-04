// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Draws a border of a menu popup that has a gap where the parent menu item is. To be used in
    /// control template for top-level menu item with children.
    /// </summary>
    public sealed class MenuGapBorder : Border
    {
        private Grid _grid;


        /// <summary>
        /// Initializes a new instance of the <see cref="MenuGapBorder"/> class.
        /// </summary>
        public MenuGapBorder()
        {
            CreateOpacityMask();

            // Update OpacityMask everytime popup is shown.
            IsVisibleChanged += (s, e) => UpdateOpacityMask();
            LayoutUpdated += (s, e) => UpdateOpacityMask();
        }


        private void CreateOpacityMask()
        {
            // Opacity mask is a VisualBrush consisting of a Grid:
            //          column 0   column1      column2
            //        +----------+-------------+-------------------+
            // row 0  | opaque   | transparent | opaque            |
            //        +          +-------------+                   +
            // row 1  | opaque   | opaque      | opaque            |
            //        +----------+-------------+-------------------+

            _grid = new Grid();
            var columnDefinition0 = new ColumnDefinition { Width = new GridLength(1) };
            var columnDefinition1 = new ColumnDefinition { Width = new GridLength(0) };
            var columnDefinition2 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            _grid.ColumnDefinitions.Add(columnDefinition0);
            _grid.ColumnDefinitions.Add(columnDefinition1);
            _grid.ColumnDefinitions.Add(columnDefinition2);

            var rowDefinition0 = new RowDefinition { Height = new GridLength(1) };
            var rowDefinition1 = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            _grid.RowDefinitions.Add(rowDefinition0);
            _grid.RowDefinitions.Add(rowDefinition1);

            var rectangle0 = new Rectangle { Fill = Brushes.Black };
            Grid.SetRow(rectangle0, 0);
            Grid.SetColumn(rectangle0, 0);
            Grid.SetRowSpan(rectangle0, 2);

            var rectangle1 = new Rectangle { Fill = Brushes.Black };
            Grid.SetRow(rectangle1, 1);
            Grid.SetColumn(rectangle1, 1);

            var rectangle2 = new Rectangle { Fill = Brushes.Black };
            Grid.SetRow(rectangle2, 0);
            Grid.SetColumn(rectangle2, 2);
            Grid.SetRowSpan(rectangle2, 2);

            _grid.Children.Add(rectangle0);
            _grid.Children.Add(rectangle1);
            _grid.Children.Add(rectangle2);

            OpacityMask = new VisualBrush(_grid);
        }


        private void UpdateOpacityMask()
        {
            if (!IsVisible)
                return;

            var menuItem = TemplatedParent as MenuItem;
            if (menuItem == null)
                return;

            // Transform origin of parent menu item to popup. (Different HWND!)
            var p = new Point();
            p = menuItem.PointToScreen(p);
            p = PointFromScreen(p);

            var borderThickness = BorderThickness;
            var size = RenderSize;

            if (p.X < 0 || p.Y > 0)
            {
                // Invalid position or popup above MenuItem.
                // --> No gap in border.
                _grid.Width = size.Width;
                _grid.Height = size.Height;
                _grid.ColumnDefinitions[0].Width = new GridLength(Math.Max(0, p.X) + borderThickness.Left);
                _grid.ColumnDefinitions[1].Width = new GridLength(0);
                _grid.RowDefinitions[0].Height = new GridLength(borderThickness.Top);
            }
            else
            {
                // Popup below MenuItem.
                // --> Set gap in border.
                _grid.Width = size.Width;
                _grid.Height = size.Height;
                _grid.ColumnDefinitions[0].Width = new GridLength(p.X + borderThickness.Left);
                _grid.ColumnDefinitions[1].Width = new GridLength(menuItem.RenderSize.Width - borderThickness.Left - borderThickness.Right);
                _grid.RowDefinitions[0].Height = new GridLength(borderThickness.Top);
            }
        }
    }
}
