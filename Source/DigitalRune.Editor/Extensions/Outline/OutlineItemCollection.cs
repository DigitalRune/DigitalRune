// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Windows;


namespace DigitalRune.Editor.Outlines
{
    /// <summary>
    /// Represents a collection of <see cref="OutlineItem"/>s.
    /// </summary>
    public class OutlineItemCollection : ObservableChildCollection<OutlineItem, OutlineItem>
    {
        /// <summary>
        /// Gets the parent which owns this child collection.
        /// </summary>
        /// <value>The parent.</value>
        public new OutlineItem Parent
        {
            get { return base.Parent; }
            protected internal set { base.Parent = value; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="OutlineItemCollection"/> class.
        /// </summary>
        public OutlineItemCollection() : base(null)
        {
        }


        /// <summary>
        /// Gets the parent of an object.
        /// </summary>
        /// <param name="child">The child object.</param>
        /// <returns>The parent of <paramref name="child"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override OutlineItem GetParent(OutlineItem child)
        {
            return child.Parent;
        }


        /// <summary>
        /// Sets the parent of the given object.
        /// </summary>
        /// <param name="child">The child object.</param>
        /// <param name="parent">The parent to set.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void SetParent(OutlineItem child, OutlineItem parent)
        {
            child.Parent = parent;
        }
    }
}
