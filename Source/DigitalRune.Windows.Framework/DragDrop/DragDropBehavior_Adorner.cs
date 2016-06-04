// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Documents;


namespace DigitalRune.Windows.Framework
{
    public partial class DragDropBehavior
    {
        // The drag adorner shows a representation of the data near the mouse while dragging.
        private DragAdorner _dragAdorner;

        // The drop adorner renders an insertion indicator line over items controls.
        private DropAdorner _dropAdorner;


        #region ----- Drag Adorner -----

        // Creates or updates the DragAdorner. 
        private void UpdateDragAdorner(Point mousePosition)
        {
            // Create new adorner if necessary.
            if (_dragAdorner == null && _dropCommandParameter.Data != null)
            {
                _dragAdorner = new DragAdorner(AssociatedObject)
                {
                    Data = _dropCommandParameter.Data,
                    DataTemplate = DragTemplate,
                };

                var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                adornerLayer?.Add(_dragAdorner);
            }

            // Update the position of the adorner.
            UpdateDragAdornerPosition(mousePosition);


            // Update data. 
            // (The drop commands can change it to alter the visualization while dropping.)
            if (_dragAdorner != null)
                _dragAdorner.Data = _dropCommandParameter.Data;
        }


        // Update the adorner position. 
        // (Is called by UpdateDragAdorner but can also be called directly.)
        private void UpdateDragAdornerPosition(Point mousePosition)
        {
            // Set location relative to mouse cursor.
            _dragAdorner?.SetOffsets(mousePosition.X, mousePosition.Y);
        }


        /// <summary>
        /// Removes the <see cref="DragAdorner"/>.
        /// </summary>
        private void RemoveDragAdorner()
        {
            if (_dragAdorner != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(_dragAdorner.AdornedElement);
                adornerLayer?.Remove(_dragAdorner);

                _dragAdorner = null;
            }
        }
        #endregion


        #region ----- Drop Adorner -----

        // Creates or updates the DropAdorner.
        private void UpdateDropAdorner(FrameworkElement adornedElement, bool isVertical, bool insertAfter)
        {
            // Remove the old adorner if adorner is shown on wrong item container.
            if (_dropAdorner != null && _dropAdorner.AdornedElement != adornedElement)
                RemoveDropAdorner();

            if (adornedElement == null)
                return;

            // Create new adorner if necessary.
            if (_dropAdorner == null)
            {
                // We need to get the AdornerLayer of the ItemContainer and not the ItemsControl.
                // The ItemsControl could contain a ScrollContentPresenter with its own
                // AdornerLayer. If we would use the AdornerLayer of the Window, then the drop
                // adorner could render over the scroll bar of the ItemsControl.
                _dropAdorner = new DropAdorner(isVertical, insertAfter, adornedElement);
                var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
                adornerLayer?.Add(_dropAdorner);
            }

            // Update the position of the adorner.
            if (_dropAdorner != null)
            {
                _dropAdorner.InsertAfter = insertAfter;
                _dropAdorner.InvalidateVisual();
            }
        }

        
        private void RemoveDropAdorner()
        {
            if (_dropAdorner != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(_dropAdorner.AdornedElement);
                adornerLayer?.Remove(_dropAdorner);

                _dropAdorner = null;
            }
        }
        #endregion
    }
}
