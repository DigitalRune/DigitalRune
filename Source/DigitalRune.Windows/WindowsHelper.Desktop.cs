// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using DigitalRune.Linq;
using DigitalRune.Mathematics;
using DigitalRune.Windows.Interop;


namespace DigitalRune.Windows
{
    partial class WindowsHelper
    {
        #region ----- LINQ to Logical Tree -----

        /// <summary>
        /// Gets the logical parent of the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical parent for.
        /// </param>
        /// <returns>The logical parent of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static DependencyObject GetLogicalParent(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            DependencyObject parent = LogicalTreeHelper.GetParent(dependencyObject);

            // If the current element is in a template, we have to use TemplatedParent.
            if (parent == null)
            {
                if (dependencyObject is Popup)
                    parent = ((Popup)dependencyObject).PlacementTarget;
                else if (dependencyObject is FrameworkElement)
                    parent = ((FrameworkElement)dependencyObject).TemplatedParent;
                else if (dependencyObject is FrameworkContentElement)
                    parent = ((FrameworkContentElement)dependencyObject).TemplatedParent;
            }

            return parent;
        }


        /// <summary>
        /// Gets the logical ancestors of the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical ancestors for.
        /// </param>
        /// <returns>The logical ancestors of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetLogicalAncestors(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetAncestors(dependencyObject, GetLogicalParent);
        }


        /// <summary>
        /// Gets the <see cref="DependencyObject"/> and its logical ancestors.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical ancestors for.
        /// </param>
        /// <returns>The <see cref="DependencyObject"/> and its logical ancestors.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetSelfAndLogicalAncestors(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetSelfAndAncestors(dependencyObject, GetLogicalParent);
        }


        /// <summary>
        /// Gets the logical root of the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical root for.
        /// </param>
        /// <returns>The logical root of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static DependencyObject GetLogicalRoot(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetRoot(dependencyObject, GetLogicalParent);
        }


        /// <summary>
        /// Gets the logical children of the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical children for.
        /// </param>
        /// <returns>The logical children of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetLogicalChildren(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return LogicalTreeHelper.GetChildren(dependencyObject).OfType<DependencyObject>();
        }


        /// <overloads>
        /// <summary>
        /// Gets the logical descendants of the <see cref="DependencyObject"/>.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Gets the logical descendants of the <see cref="DependencyObject"/> using depth-first
        /// search.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical descendants for.
        /// </param>
        /// <returns>
        /// The logical descendants of the <see cref="DependencyObject"/> using a depth-first
        /// search.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetLogicalDescendants(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetDescendants(dependencyObject, GetLogicalChildren, true);
        }


        /// <summary>
        /// Gets the logical descendants of the <see cref="DependencyObject"/> using either a depth-
        /// or a breadth-first search.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical descendants for.
        /// </param>
        /// <param name="depthFirst">
        /// If set to <see langword="true"/> then a depth-first search for descendants will be made;
        /// otherwise a breadth-first search will be made.
        /// </param>
        /// <returns>The logical descendants of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetLogicalDescendants(this DependencyObject dependencyObject, bool depthFirst)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetDescendants(dependencyObject, GetLogicalChildren, depthFirst);
        }


        /// <overloads>
        /// <summary>
        /// Gets the logical subtree (the given <see cref="DependencyObject"/> and all of its
        /// descendants).
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Gets the logical subtree (the given <see cref="DependencyObject"/> and all of its
        /// descendants) using depth-first search.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> that is the root of the logical subtree.
        /// </param>
        /// <returns>
        /// The <paramref name="dependencyObject"/> and all of its descendants using a depth-first
        /// search.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetLogicalSubtree(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetSubtree(dependencyObject, GetLogicalChildren, true);
        }


        /// <summary>
        /// Gets the logical subtree (the given <see cref="DependencyObject"/> and all of its
        /// descendants) using either a depth- or a breadth-first search.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the logical descendants for.
        /// </param>
        /// <param name="depthFirst">
        /// If set to <see langword="true"/> then a depth-first search for descendants will be made;
        /// otherwise a breadth-first search will be made.
        /// </param>
        /// <returns>The <paramref name="dependencyObject"/> and all of its descendants.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetLogicalSubtree(this DependencyObject dependencyObject, bool depthFirst)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetSubtree(dependencyObject, GetLogicalChildren, depthFirst);
        }
        #endregion


        /* NOTE: CloneUsingXaml is temporarily removed.
           The method is currently not used. To avoid any confusions in the API documentation it is
           commented out because it would be listed as extension method for every other type.
     
        /// <summary>
        /// Clones an object using XAML serialization rules.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to be cloned.</param>
        /// <returns>
        /// A clone of <paramref name="obj"/> where all public fields are cloned (deep copy). Fields or
        /// events are not copied and have their default values.
        /// </returns>
        public static Object CloneUsingXaml(this object obj)
        {
            // NOTE: This helper method is based on the tip provided by Mike Hillberg.
            // See http://blogs.msdn.com/mikehillberg/archive/2007/05/01/CloneWithXamlWriterXamlReader.aspx

            string xaml = XamlWriter.Save(obj);
            return XamlReader.Load(new XmlTextReader(new StringReader(xaml)));
        }
        */


        /// <summary>
        /// Gets location of the mouse cursor relative to the specified <see cref="Visual"/>.
        /// </summary>
        /// <param name="visual">The <see cref="Visual"/>.</param>
        /// <returns>The mouse position relative to <paramref name="visual"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="visual"/> is <see langword="null"/>.
        /// </exception>
        public static Point GetMousePosition(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));

            POINT pointNative = new POINT();
            Win32.GetCursorPos(ref pointNative);
            Point point = new Point(pointNative.X, pointNative.Y);

            // Convert mouse position from screen coordinates into local coordinates of visual.
            return visual.PointFromScreen(point);
        }


        #region ----- Aero Glass Frame (Windows 10 version) -----

        // References:
        // - http://withinrafael.com/adding-the-aero-glass-blur-to-your-windows-10-apps/


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttribData data);


        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttribData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public AccentFlags AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }


        [Flags]
        internal enum AccentFlags
        {
            DrawLeftBorder = 0x20,
            DrawTopBorder = 0x40,
            DrawRightBorder = 0x80,
            DrawBottomBorder = 0x100,
            DrawAllBorders = DrawLeftBorder | DrawTopBorder | DrawRightBorder | DrawBottomBorder
        }


        internal enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }


        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }


        /// <summary>
        /// Extends the aero glass frame into the client area of the <see cref="Window"/>. Creates
        /// a "sheet of glass" effect where the client area is rendered as a solid surface with no
        /// window border.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <param name="enable">
        /// <see langword="true"/> to enable the aero glass effect; otherwise,
        /// <see langword="false"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the glass frame was successfully extended; otherwise,
        /// <see langword="false"/>. The method can fail when the Desktop Window Manager (DWM) is
        /// disabled or not available.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="window"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DigitalRune.Windows.WindowsHelper.SetWindowCompositionAttribute(System.IntPtr,DigitalRune.Windows.WindowsHelper+WindowCompositionAttribData@)")]
        public static bool SetAeroGlass(this Window window, bool enable)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            if (Environment.OSVersion.Version.Major < 6)
            {
                // An OS older than Windows Vista.
                return false;
            }

            if (SystemParameters.HighContrast)
            {
                // Blur is not useful in high contrast mode.
                return false;
            }

            try
            {
                var windowHelper = new WindowInteropHelper(window);

                var accent = new AccentPolicy
                {
                    AccentState = enable ? AccentState.ACCENT_ENABLE_BLURBEHIND : AccentState.ACCENT_DISABLED,
                    //AccentFlags = AccentFlags.DrawAllBorders
                };

                var accentStructSize = Marshal.SizeOf(accent);
                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttribData
                {
                    Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                    SizeOfData = accentStructSize,
                    Data = accentPtr
                };

                SetWindowCompositionAttribute(windowHelper.Handle, ref data);

                Marshal.FreeHGlobal(accentPtr);
            }
            catch (DllNotFoundException)
            {
                return false;
            }

            return true;
        }
        #endregion


        #region ----- Numeric -----

        /// <summary>
        /// Determines whether two values are equal (regarding the tolerance
        /// <see cref="Numeric.EpsilonD"/>).
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>
        /// <see langword="true"/> if the specified values are equal (within the tolerance);
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// <strong>Important:</strong> When at least one component of the parameters is a
        /// <see cref="Double.NaN"/> the result is undefined. Such cases should be handled
        /// explicitly by the calling application.
        /// </remarks>
        public static bool AreEqual(Vector value1, Vector value2)
        {
            return Numeric.AreEqual(value1.X, value2.X)
                   && Numeric.AreEqual(value1.Y, value2.Y);
        }


        /// <summary>
        /// Determines whether two <see cref="Vector"/>s are equal (regarding a specific
        /// tolerance).
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="epsilon">The tolerance value.</param>
        /// <returns>
        /// <see langword="true"/> if the specified values are equal (within the tolerance);
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// <strong>Important:</strong> When at least one of the parameters is a
        /// <see cref="Double.NaN"/> the result is undefined. Such cases should be handled
        /// explicitly by the calling application.
        /// </remarks>
        public static bool AreEqual(Vector value1, Vector value2, double epsilon)
        {
            return Numeric.AreEqual(value1.X, value2.X, epsilon)
                   && Numeric.AreEqual(value1.Y, value2.Y, epsilon);
        }


        /// <summary>
        /// Clamps near-zero values to zero.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// (0, 0) if the value is nearly zero (within the tolerance <see cref="Numeric.EpsilonD"/>)
        /// or the original value otherwise.
        /// </returns>
        public static Vector ClampToZero(Vector value)
        {
            return new Vector(Numeric.ClampToZero(value.X), Numeric.ClampToZero(value.Y));
        }


        /// <summary>
        /// Clamps near-zero values to zero.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="epsilon">The tolerance value.</param>
        /// <returns>
        /// 0 if the value is nearly zero (within the tolerance <paramref name="epsilon"/>) or the
        /// original value otherwise.
        /// </returns>
        public static Vector ClampToZero(Vector value, double epsilon)
        {
            return new Vector(Numeric.ClampToZero(value.X, epsilon), Numeric.ClampToZero(value.Y, epsilon));
        }


        /// <summary>
        /// Determines whether a <see cref="Vector"/> is (0, 0) (regarding the tolerance
        /// <see cref="Numeric.EpsilonD"/>).
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Vector"/> is (0, 0) (within the
        /// tolerance); otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// A value is zero if |x| &lt; <see cref="Numeric.EpsilonD"/>.
        /// </remarks>
        public static bool IsZero(Vector value)
        {
            return Numeric.IsZero(value.X)
                   && Numeric.IsZero(value.Y);
        }


        /// <summary>
        /// Determines whether a <see cref="Vector"/> is (0, 0) (regarding a specific tolerance).
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="epsilon">The tolerance value.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Vector"/> is (0, 0) (within the
        /// tolerance); otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// A value is zero if |x| &lt; epsilon.
        /// </remarks>
        public static bool IsZero(Vector value, float epsilon)
        {
            return Numeric.IsZero(value.X, epsilon)
                   && Numeric.IsZero(value.Y, epsilon);
        }
        #endregion


        #region ----- Dispatcher -----

        /// <summary>
        /// Synchronously processes all window messages currently in the message queue. (Blocks
        /// until messages have been processed.)
        /// </summary>
        /// <remarks>
        /// In async methods (.NET 4.5) use 
        /// <c>await Dispatcher.Yield(DispatcherPriority.Background)</c> instead.
        /// </remarks>
#if NET45
        [Obsolete("In .NET 4.5 use 'await Dispatcher.Yield(DispatcherPriority.Background)' instead of DoEvents().")]
#endif
        public static void DoEvents()
        {
            // See blog post: http://kentb.blogspot.com/2008/04/dispatcher-frames.html
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        }


        ///// <summary>
        ///// Asynchronously processes all window messages currently in the message queue.
        ///// </summary>
        ///// <returns>A task that represents the asynchronous operation.</returns>
        //public static Task DoEventsAsync()
        //{
        //    var tcs = new TaskCompletionSource<object>();
        //    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => tcs.SetResult(null)));
        //    return tcs.Task;
        //}
        #endregion


        #region ----- Routed Events -----

        /// <summary>
        /// Raise a routed event on a target <see cref="UIElement"/> or
        /// <see cref="ContentElement"/>.
        /// </summary>
        /// <param name="target">
        /// The <see cref="UIElement"/> or <see cref="ContentElement"/> on which to raise the event.
        /// </param>
        /// <param name="eventArgs">
        /// The <see cref="RoutedEventArgs"/> to use when raising the event.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        public static void RaiseEvent(DependencyObject target, RoutedEventArgs eventArgs)
        {
            if (target is UIElement)
                (target as UIElement).RaiseEvent(eventArgs);
            else if (target is ContentElement)
                (target as ContentElement).RaiseEvent(eventArgs);
        }


        /// <summary>
        /// Adds an event handler for a routed event to a target <see cref="UIElement"/> or
        /// <see cref="ContentElement"/>.
        /// </summary>
        /// <param name="element">
        /// The <see cref="UIElement"/> or <see cref="ContentElement"/> that listens to the event.
        /// </param>
        /// <param name="routedEvent">The routed event that will be handled.</param>
        /// <param name="eventHandler">The event handler to be added.</param>
        public static void AddHandler(DependencyObject element, RoutedEvent routedEvent, Delegate eventHandler)
        {
            var uiElement = element as UIElement;
            if (uiElement != null)
            {
                uiElement.AddHandler(routedEvent, eventHandler);
            }
            else
            {
                var contentElement = element as ContentElement;
                if (contentElement != null)
                {
                    contentElement.AddHandler(routedEvent, eventHandler);
                }
            }
        }


        /// <summary>
        /// Removes an event handler for a routed event from a target <see cref="UIElement"/> or
        /// <see cref="ContentElement"/>.
        /// </summary>
        /// <param name="element">
        /// The <see cref="UIElement"/> or <see cref="ContentElement"/> that listens to the event.
        /// </param>
        /// <param name="routedEvent">The routed event that will no longer be handled.</param>
        /// <param name="eventHandler">The event handler to be removed.</param>
        public static void RemoveHandler(DependencyObject element, RoutedEvent routedEvent, Delegate eventHandler)
        {
            var uiElement = element as UIElement;
            if (uiElement != null)
            {
                uiElement.RemoveHandler(routedEvent, eventHandler);
            }
            else
            {
                var contentElement = element as ContentElement;
                if (contentElement != null)
                {
                    contentElement.RemoveHandler(routedEvent, eventHandler);
                }
            }
        }
        #endregion


        #region ----- Window ShowIcon/CanMinimize/CanMaximize Properties -----

        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.WindowsHelper.ShowIcon"/> attached
        /// dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a value indicating whether the window icon is visible in the caption bar.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the window icon is visible; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.RegisterAttached(
            "ShowIcon",
            typeof(bool),
            typeof(WindowsHelper),
            new PropertyMetadata(Boxed.BooleanTrue, OnShowIconChanged));

        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.WindowsHelper.ShowIcon"/>
        /// attached property from a given <see cref="Window"/> object.
        /// </summary>
        /// <param name="window">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.WindowsHelper.ShowIcon"/> attached
        /// property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="window"/> is <see langword="null"/>.
        /// </exception>
        public static bool GetShowIcon(DependencyObject window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            return (bool)window.GetValue(ShowIconProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.WindowsHelper.ShowIcon"/>
        /// attached property to a given <see cref="Window"/> object.
        /// </summary>
        /// <param name="window">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="window"/> is <see langword="null"/>.
        /// </exception>
        public static void SetShowIcon(DependencyObject window, bool value)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            window.SetValue(ShowIconProperty, Boxed.Get(value));
        }


        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.WindowsHelper.CanMinimize"/> attached
        /// dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a value indicating whether the window has a minimize button.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the window has a minimize button; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty CanMinimizeProperty = DependencyProperty.RegisterAttached(
            "CanMinimize",
            typeof(bool),
            typeof(WindowsHelper),
            new PropertyMetadata(Boxed.BooleanTrue, OnCanMinimizeChanged));

        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.WindowsHelper.CanMinimize"/>
        /// attached property from a given <see cref="Window"/> object.
        /// </summary>
        /// <param name="window">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.WindowsHelper.CanMinimize"/> attached
        /// property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="window"/> is <see langword="null"/>.
        /// </exception>
        public static bool GetCanMinimize(DependencyObject window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            return (bool)window.GetValue(CanMinimizeProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.WindowsHelper.CanMinimize"/>
        /// attached property to a given <see cref="Window"/> object.
        /// </summary>
        /// <param name="window">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="window"/> is <see langword="null"/>.
        /// </exception>
        public static void SetCanMinimize(DependencyObject window, bool value)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            window.SetValue(CanMinimizeProperty, Boxed.Get(value));
        }


        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.WindowsHelper.CanMaximize"/> attached
        /// dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a value indicating whether the window has a maximize button.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the window has a maximize button; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty CanMaximizeProperty = DependencyProperty.RegisterAttached(
          "CanMaximize",
          typeof(bool),
          typeof(WindowsHelper),
          new PropertyMetadata(Boxed.BooleanTrue, OnCanMaximizeChanged));

        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.WindowsHelper.CanMaximize"/>
        /// attached property from a given <see cref="Window"/> object.
        /// </summary>
        /// <param name="window">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.WindowsHelper.CanMaximize"/> attached
        /// property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="window"/> is <see langword="null"/>.
        /// </exception>
        public static bool GetCanMaximize(DependencyObject window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            return (bool)window.GetValue(CanMaximizeProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.WindowsHelper.CanMaximize"/>
        /// attached property to a given <see cref="Window"/> object.
        /// </summary>
        /// <param name="window">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="window"/> is <see langword="null"/>.
        /// </exception>
        public static void SetCanMaximize(DependencyObject window, bool value)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            window.SetValue(CanMaximizeProperty, Boxed.Get(value));
        }


        /// <summary>
        /// Called when the <see cref="P:DigitalRune.Windows.WindowsHelper.ShowIcon"/> property
        /// changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnShowIconChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            bool newValue = (bool)eventArgs.NewValue;
            var window = dependencyObject as Window;
            if (window == null)
                return;

            if (window.IsLoaded)
            {
                if (newValue)
                    ShowWindowIcon(window);
                else
                    HideWindowIcon(window);
            }
            else
            {
                EventHandler handler = null;
                handler = (s, e) =>
                {
                    window.SourceInitialized -= handler;

                    if (newValue)
                        ShowWindowIcon(window);
                    else
                        HideWindowIcon(window);
                };
                window.SourceInitialized += handler;
            }
        }


        /// <summary>
        /// Called when the <see cref="P:DigitalRune.Windows.WindowsHelper.CanMinimize"/> property
        /// changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnCanMinimizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            bool newValue = (bool)eventArgs.NewValue;
            var window = dependencyObject as Window;
            if (window == null)
                return;

            if (window.IsLoaded)
            {
                if (newValue)
                    ShowMinimizeBox(window);
                else
                    HideMinimizeBox(window);
            }
            else
            {
                EventHandler handler = null;
                handler = (s, e) =>
                          {
                              window.SourceInitialized -= handler;

                              if (newValue)
                                  ShowMinimizeBox(window);
                              else
                                  HideMinimizeBox(window);
                          };
                window.SourceInitialized += handler;
            }
        }


        /// <summary>
        /// Called when the <see cref="P:DigitalRune.Windows.WindowsHelper.CanMaximize"/> property
        /// changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnCanMaximizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            bool newValue = (bool)eventArgs.NewValue;
            var window = dependencyObject as Window;
            if (window == null)
                return;

            if (window.IsLoaded)
            {
                if (newValue)
                    ShowMaximizeBox(window);
                else
                    HideMaximizeBox(window);
            }
            else
            {
                EventHandler handler = null;
                handler = (s, e) =>
                          {
                              window.SourceInitialized -= handler;

                              if (newValue)
                                  ShowMaximizeBox(window);
                              else
                                  HideMaximizeBox(window);
                          };
                window.SourceInitialized += handler;
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        private static void ShowWindowIcon(Window window)
        {
            // Not implemented.
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        private static void HideWindowIcon(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int value = Win32.GetWindowLong(hwnd, GetWindowLongIndex.GWL_EXSTYLE);
            Win32.SetWindowLong(hwnd, GetWindowLongIndex.GWL_EXSTYLE, (int)(value | WindowStylesEx.WS_EX_DLGMODALFRAME));

            // Update the window's non-client area to reflect the changes.
            //Win32.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_FRAMECHANGED);

            // Fix for dialog windows. See http://stackoverflow.com/questions/18580430/hide-the-icon-from-a-wpf-window.
            Win32.SendMessage(hwnd, WindowMessages.WM_SETICON, new IntPtr(1), IntPtr.Zero);
            Win32.SendMessage(hwnd, WindowMessages.WM_SETICON, IntPtr.Zero, IntPtr.Zero);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        private static void ShowMinimizeBox(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int value = Win32.GetWindowLong(hwnd, GetWindowLongIndex.GWL_STYLE);
            Win32.SetWindowLong(hwnd, GetWindowLongIndex.GWL_STYLE, (int)(value | WindowStyles.WS_MINIMIZEBOX));
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        private static void HideMinimizeBox(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int value = Win32.GetWindowLong(hwnd, GetWindowLongIndex.GWL_STYLE);
            Win32.SetWindowLong(hwnd, GetWindowLongIndex.GWL_STYLE, (int)(value & ~WindowStyles.WS_MINIMIZEBOX));
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        private static void ShowMaximizeBox(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int value = Win32.GetWindowLong(hwnd, GetWindowLongIndex.GWL_STYLE);
            Win32.SetWindowLong(hwnd, GetWindowLongIndex.GWL_STYLE, (int)(value | WindowStyles.WS_MAXIMIZEBOX));
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        private static void HideMaximizeBox(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int value = Win32.GetWindowLong(hwnd, GetWindowLongIndex.GWL_STYLE);
            Win32.SetWindowLong(hwnd, GetWindowLongIndex.GWL_STYLE, (int)(value & ~WindowStyles.WS_MAXIMIZEBOX));
        }
        #endregion


        #region ----- SelectOnMouseDown Attached Property -----

        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.WindowsHelper.SelectOnMouseDown"/>
        /// attached dependency property.
        /// </summary>
        /// 
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a value indicating whether the <strong>Selector.IsSelected</strong>
        /// attached property should be set when a mouse button is pressed over the element.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to set the <strong>Selector.IsSelected</strong> attached property
        /// whenever a mouse button is pressed over the element; otherwise, <see langword="false"/>.
        /// The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This attached dependency property can be set on any <see cref="UIElement"/>. Typically
        /// it is set on <see cref="ListBoxItem"/>s. If the property is set to
        /// <see langword="true"/>, then the <strong>Selector.IsSelected</strong> attached property
        /// will be set whenever a mouse button is pressed down over the element. This is usually
        /// needed when a <see cref="ListBoxItem"/> contains an interactive element, like a
        /// <see cref="TextBox"/> because the contained element handles the mouse and the list box
        /// item is not automatically selected. Setting the
        /// <see cref="P:DigitalRune.Windows.WindowsHelper.SelectOnMouseDown"/> attached property
        /// makes sure that the list box item is properly selected.
        /// </remarks>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty SelectOnMouseDownProperty = DependencyProperty.RegisterAttached(
          "SelectOnMouseDown", typeof(bool), typeof(WindowsHelper),
          new PropertyMetadata(Boxed.BooleanFalse, OnSelectOnMouseDownChanged));


        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.WindowsHelper.SelectOnMouseDown"/>
        /// attached property from a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.WindowsHelper.SelectOnMouseDown"/>
        /// attached property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static bool GetSelectOnMouseDown(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return (bool)obj.GetValue(SelectOnMouseDownProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.WindowsHelper.SelectOnMouseDown"/>
        /// attached property to a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static void SetSelectOnMouseDown(DependencyObject obj, bool value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.SetValue(SelectOnMouseDownProperty, Boxed.Get(value));
        }


        private static void OnSelectOnMouseDownChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = dependencyObject as UIElement;
            if (element == null)
                return;

            if ((bool)eventArgs.NewValue)
            {
                element.PreviewMouseDown += OnPreviewMouseDown;
            }
            else
            {
                element.PreviewMouseDown -= OnPreviewMouseDown;
            }
        }


        private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            var element = sender as UIElement;
            if (element == null)
                return;

            Selector.SetIsSelected(element, true);
        }
        #endregion


        #region ----- Pixel Snapping -----

        /// <summary>
        /// Gets the size of a device pixel (screen) in device-independent pixels (WPF).
        /// </summary>
        /// <param name="visual">The reference visual.</param>
        /// <returns>The size of a device pixel given in device-independent pixels.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="visual"/> is <see langword="null"/>.
        /// </exception>
        public static Size GetPixelSize(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));

            Size size = new Size(1, 1);
            var presentationSource = PresentationSource.FromVisual(visual);
            if (presentationSource != null && presentationSource.CompositionTarget != null)
            {
                Matrix matrix = presentationSource.CompositionTarget.TransformFromDevice;
                size = new Size(matrix.M11, matrix.M22);
            }

            return size;
        }


        /// <overloads>
        /// <summary>
        /// Rounds specified value in device-independent pixels to the next whole number of device
        /// pixels.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Rounds specified value in device-independent pixels to the next whole number of device
        /// pixels.
        /// </summary>
        /// <param name="value">The value in device-independent pixels.</param>
        /// <param name="pixelSize">
        /// The size of a device pixel. See <see cref="GetPixelSize"/>.
        /// </param>
        /// <returns>The rounded value in device-independent pixels.</returns>
        /// <remarks>
        /// <para>
        /// Graphical elements in WPF can look "blurry" on screen due to anti-aliasing and filtering
        /// when the logical positions (device-independent pixels) do not align with the physical
        /// pixels on the target device pixels. The <strong>RoundToDevicePixels</strong>-methods can
        /// be used to align logical positions with physical pixels.
        /// </para>
        /// <para>
        /// The sizes and boundaries of elements can be snapped to full device pixels using
        /// <see cref="RoundToDevicePixels(double,double)"/>. This method rounds the specified
        /// values to the closest whole device pixels.
        /// </para>
        /// <para>
        /// Horizontal and vertical lines that are drawn in WPF need to be centered on device
        /// pixels. The method <see cref="RoundToDevicePixelsCenter(double,double)"/> snaps the
        /// specified values to device pixel centers.
        /// </para>
        /// <para>
        /// <see cref="RoundToDevicePixelsEven"/> and <see cref="RoundToDevicePixelsOdd"/> can be
        /// used in special cases. <see cref="RoundToDevicePixelsOdd"/> can be used to round the
        /// size of an element to an odd number of device pixels. This is, for example, useful if
        /// a 1-pixel line should be drawn in the center of the element.
        /// </para>
        /// <para>
        /// <strong>Transformations:</strong> The <strong>RoundToDevicePixels</strong>-methods do
        /// not take any layout/render transformation of the element into account. These need to be
        /// handled by the caller. In most cases pixel snapping can be disabled while transformation
        /// are active.
        /// </para>
        /// </remarks>
        public static double RoundToDevicePixels(double value, double pixelSize)
        {
            // Convert device-independent value (WPF) to device value (screen).
            value = value / pixelSize;

            // Round to whole device pixels.
            value = Math.Round(value, MidpointRounding.AwayFromZero);

            // Convert device value (screen) to device-independent value (WPF).
            return pixelSize * value;
        }


        /// <summary>
        /// Aligns the specified point with the closest device pixel boundary.
        /// </summary>
        /// <param name="point">The point in device-independent pixels.</param>
        /// <param name="pixelSize">
        /// The size of a device pixel. See <see cref="GetPixelSize"/>.
        /// </param>
        /// <returns>The aligned point in device-independent pixels.</returns>
        /// <inheritdoc cref="RoundToDevicePixels(double,double)"/>
        public static Point RoundToDevicePixels(Point point, Size pixelSize)
        {
            point.X = RoundToDevicePixels(point.X, pixelSize.Width);
            point.Y = RoundToDevicePixels(point.Y, pixelSize.Height);
            return point;
        }


        /// <summary>
        /// Aligns the specified rectangle with the closest device pixel boundary.
        /// </summary>
        /// <param name="rectangle">The rectangle in device-independent pixels.</param>
        /// <param name="pixelSize">
        /// The size of a device pixel. See <see cref="GetPixelSize"/>.
        /// </param>
        /// <returns>The aligned rectangle in device-independent pixels.</returns>
        /// <inheritdoc cref="RoundToDevicePixels(double,double)"/>
        public static Rect RoundToDevicePixels(Rect rectangle, Size pixelSize)
        {
            rectangle.X = RoundToDevicePixels(rectangle.X, pixelSize.Width);
            rectangle.Y = RoundToDevicePixels(rectangle.Y, pixelSize.Height);
            rectangle.Width = RoundToDevicePixels(rectangle.Width, pixelSize.Width);
            rectangle.Height = RoundToDevicePixels(rectangle.Height, pixelSize.Height);
            return rectangle;
        }


        /// <overloads>
        /// <summary>
        /// Aligns the specified position with the center of the closest device pixel.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Aligns the specified position with the center of the closest device pixel.
        /// </summary>
        /// <param name="position">The position in device-independent pixels.</param>
        /// <param name="pixelSize">
        /// The size of a device pixel. See <see cref="GetPixelSize"/>.
        /// </param>
        /// <returns>The aligned position in device-independent pixels.</returns>
        /// <inheritdoc cref="RoundToDevicePixels(double,double)"/>
        public static double RoundToDevicePixelsCenter(double position, double pixelSize)
        {
            // Convert device-independent position (WPF) to device position (screen).
            position = position / pixelSize;

            // Device pixels:
            //
            //  |     |     |
            // -+-----+-----+-
            //  0     1     2
            //
            // [0, 1[ ... snaps to 0.5.
            // [1, 2[ ... snaps to 1.5.

            position = Math.Round(position + 0.5, MidpointRounding.AwayFromZero) - 0.5;

            // Convert device position (screen) to device-independent position (WPF).
            return pixelSize * position;
        }


        /// <summary>
        /// Aligns the specified point with the center of the closest device pixel.
        /// </summary>
        /// <param name="point">The point in device-independent pixels.</param>
        /// <param name="pixelSize">
        /// The size of a device pixel. See <see cref="GetPixelSize"/>.
        /// </param>
        /// <returns>The aligned point in device-independent pixels.</returns>
        /// <inheritdoc cref="RoundToDevicePixels(double,double)"/>
        public static Point RoundToDevicePixelsCenter(Point point, Size pixelSize)
        {
            point.X = RoundToDevicePixelsCenter(point.X, pixelSize.Width);
            point.Y = RoundToDevicePixelsCenter(point.Y, pixelSize.Height);
            return point;
        }


        /// <summary>
        /// Aligns the specified rectangle with the center of the closest device pixels.
        /// </summary>
        /// <param name="rectangle">The rectangle in device-independent pixels.</param>
        /// <param name="pixelSize">
        /// The size of a device pixel. See <see cref="GetPixelSize"/>.
        /// </param>
        /// <returns>The aligned rectangle in device-independent pixels.</returns>
        /// <inheritdoc cref="RoundToDevicePixels(double,double)"/>
        public static Rect RoundToDevicePixelsCenter(Rect rectangle, Size pixelSize)
        {
            rectangle.X = RoundToDevicePixelsCenter(rectangle.X, pixelSize.Width);
            rectangle.Y = RoundToDevicePixelsCenter(rectangle.Y, pixelSize.Height);
            rectangle.Width = RoundToDevicePixels(rectangle.Width, pixelSize.Width);
            rectangle.Height = RoundToDevicePixels(rectangle.Height, pixelSize.Height);
            return rectangle;
        }


        /// <summary>
        /// Rounds specified value in device-independent pixels to the next even number of device
        /// pixels.
        /// </summary>
        /// <param name="value">The value in device-independent pixels.</param>
        /// <param name="pixelSize">
        /// The size of a device pixel. See <see cref="GetPixelSize"/>.
        /// </param>
        /// <returns>The rounded value in device-independent pixels.</returns>
        /// <inheritdoc cref="RoundToDevicePixels(double,double)"/>
        public static double RoundToDevicePixelsEven(double value, double pixelSize)
        {
            return RoundToDevicePixels(value, 2 * pixelSize);
        }


        /// <summary>
        /// Rounds specified value in device-independent pixels to the next odd number of device
        /// pixels.
        /// </summary>
        /// <param name="value">The value in device-independent pixels.</param>
        /// <param name="pixelSize">
        /// The size of a device pixel. See <see cref="GetPixelSize"/>.
        /// </param>
        /// <returns>The rounded value in device-independent pixels.</returns>
        /// <inheritdoc cref="RoundToDevicePixels(double,double)"/>
        public static double RoundToDevicePixelsOdd(double value, double pixelSize)
        {
            return RoundToDevicePixels(value + pixelSize, 2 * pixelSize) - pixelSize;
        }
        #endregion


        #region ----- Misc -----

        /// <summary>
        /// Activates the window and makes sure the window is in the foreground.
        /// </summary>
        /// <param name="window">The window. Can be <see langword="null"/>.</param>
        /// <remarks>
        /// The method <see cref="Window.Activate"/> of the <see cref="Window"/> class should 
        /// activate a window and bring it to the foreground. However, this does not always work.
        /// Sometimes the application icon in the task bar flashes, but the windows stays inactive.
        /// This method uses a few tricks to make sure that the window lands in the foreground.
        /// </remarks>
        public static void ActivateWindow(Window window)
        {
            if (window == null)
                return;

            // Bring application to foreground:
            // Calling Window.Activate() immediately or deferred using Dispatcher.BeginInvoke 
            // does not work. The icon in the task bar flashes, but the windows stays inactive.
            //_shell.Window.Activate();
            //_shell.Window.Dispatcher.BeginInvoke(new Action(() => _shell.Window.Activate()));
            // Following calls should make sure that the window lands in the foreground:
            // (from http://stackoverflow.com/questions/257587/bring-a-window-to-the-front-in-wpf)
            if (!window.IsVisible)
                window.Show();

            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;

            window.Activate();
            window.Topmost = true;
            window.Topmost = false;
            window.Focus();
        }
        #endregion
    }
}
