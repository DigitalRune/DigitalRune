// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows.Media;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Provides category strings for the <see cref="CategoryAttribute"/> that can be used when
    /// documenting control properties.
    /// </summary>
    /// <remarks>
    /// Recommendations for assigning the <see cref="CategoryAttribute"/> to properties:
    /// <list type="bullet">
    /// <item>
    /// Choose the category that describes the purpose of the property best.
    /// </item>
    /// <item>
    /// If the property is available on different entities, consider choosing the
    /// <see cref="Common"/> category.
    /// </item>
    /// <item>
    /// In all other cases, choose the <see cref="Default"/> category.
    /// </item>
    /// </list>
    /// </remarks>
    public static class Categories
    {
        /// <summary>
        /// Properties related to available actions.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Action"/>
        public const string Action = "Action";


        /// <summary>
        /// Properties related to asynchronous operations.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Asynchronous"/>
        public const string Asynchronous = "Asynchronous";


        /// <summary>
        /// Properties related to how an entity appears.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Appearance"/>
        public const string Appearance = "Appearance";


        /// <summary>
        /// Properties related to how an entity acts.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Behavior"/>
        public const string Behavior = "Behavior";


        /// <summary>
        /// Properties of type <see cref="Brush"/>.
        /// </summary>
        public const string Brushes = "Brushes";


        /// <summary>
        /// Properties common among different entities.
        /// </summary>
        public const string Common = "Common Properties";


        /// <summary>
        /// Properties related to data and data source management.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Data"/>
        public const string Data = "Data";


#if SILVERLIGHT || WINDOWS_PHONE
        /// <summary>
        /// Properties that are grouped in a default category.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Default"/>
        public const string Default = "Default";
#else
        /// <summary>
        /// Properties that are grouped in a default category.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Default"/>
        public const string Default = "Misc";
#endif


        /// <summary>
        /// Properties that are available only at design time.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Design"/>
        public const string Design = "Design";


#if SILVERLIGHT || WINDOWS_PHONE
        /// <summary>
        /// Properties related to drag-and-drop operations.
        /// </summary>
        /// <seealso cref="CategoryAttribute.DragDrop"/>
        public const string DragDrop = "DragDrop";
#else
        /// <summary>
        /// Properties related to drag-and-drop operations.
        /// </summary>
        /// <seealso cref="CategoryAttribute.DragDrop"/>
        public const string DragDrop = "Drag Drop";
#endif

        /// <summary>
        /// Properties related to focus.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Format"/>
        public const string Focus = "Focus";


        /// <summary>
        /// Properties related to formatting.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Format"/>
        public const string Format = "Format";


        /// <summary>
        /// Properties related to the keyboard.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Key"/>
        public const string Key = "Key";


        /// <summary>
        /// Properties related to layout.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Layout"/>
        public const string Layout = "Layout";


        /// <summary>
        /// Properties related to the mouse.
        /// </summary>
        /// <seealso cref="CategoryAttribute.Mouse"/>
        public const string Mouse = "Mouse";


        /// <summary>
        /// Properties of type <see cref="System.Windows.Media.Transform"/>.
        /// </summary>
        public const string Transform = "Transform";


#if SILVERLIGHT || WINDOWS_PHONE
        /// <summary>
        /// Properties related to the window style of top-level forms.
        /// </summary>
        /// <seealso cref="CategoryAttribute.WindowStyle"/>
        public const string WindowStyle = "WindowStyle"; 
#else
        /// <summary>
        /// Properties related to the window style of top-level forms.
        /// </summary>
        /// <seealso cref="CategoryAttribute.WindowStyle"/>
        public const string WindowStyle = "Window Style";
#endif
    }
}
