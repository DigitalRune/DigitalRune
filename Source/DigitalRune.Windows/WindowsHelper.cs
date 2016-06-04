// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using DigitalRune.Linq;
using DigitalRune.Mathematics;
#if WP8
using System.Reflection;
using System.Threading;
#else
using System.Threading.Tasks;
#endif


namespace DigitalRune.Windows
{
    /// <summary>
    /// Static class that provides helper functions for WPF and Silverlight.
    /// </summary>  
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class WindowsHelper
    {
        // TODO: Add support for Windows Store apps.
        // Some of the necessary changes have already been made, but not all. Changes
        // are marked with NETFX_CORE.


        #region ----- Data Binding -----

        /*  MartinG: The following code is from a Microsoft Sample application - not sure if we need it.

        /// <summary>
        /// Creates a new <see cref="Binding"/> using <paramref name="bindingSource"/> as the
        /// <see cref="Binding.Source"/> and <paramref name="propertyPath"/> as the
        /// <see cref="Binding.Path"/>.
        /// </summary>
        /// <param name="bindingSource">
        /// The object to use as the new binding's <see cref="Binding.Source"/>.
        /// </param>
        /// <param name="propertyPath">
        /// The property path to use as the new binding's <see cref="Binding.Path"/>.
        /// </param>
        /// <returns>A new <see cref="Binding"/> object.</returns>
        public static Binding CreateOneWayBinding(this INotifyPropertyChanged bindingSource, string propertyPath)
        {
            return bindingSource.CreateOneWayBinding(propertyPath, null);
        }


        /// <summary>
        /// Creates a new <see cref="Binding"/> using <paramref name="bindingSource"/> as the <see cref="Binding.Source"/>,
        /// <paramref name="propertyPath"/> as the <see cref="Binding.Path"/>,
        /// and <paramref name="converter"/> as the <see cref="Binding.Converter"/>.
        /// </summary>
        /// <param name="bindingSource">The object to use as the new binding's <see cref="Binding.Source"/>.</param>
        /// <param name="propertyPath">The property path to use as the new binding's <see cref="Binding.Path"/>.</param>
        /// <param name="converter">The converter to use as the new binding's <see cref="Binding.Converter"/>.</param>
        /// <returns>A new <see cref="Binding"/> object.</returns>
        public static Binding CreateOneWayBinding(this INotifyPropertyChanged bindingSource, string propertyPath, IValueConverter converter)
        {
            Binding binding = new Binding();

            binding.Source = bindingSource;
            binding.Path = new PropertyPath(propertyPath);
            binding.Converter = converter;

            return binding;
        }
        */


        /// <summary>
        /// Creates a new <see cref="Binding"/> object that is a copy of another
        /// <see cref="Binding"/> object.
        /// </summary>
        /// <param name="binding">The <see cref="Binding"/> object to copy.</param>
        /// <returns>A new <see cref="Binding"/> object.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="binding"/> is <see langword="null"/>.
        /// </exception>
        public static Binding Clone(this Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            var clone = new Binding
            {
                BindsDirectlyToSource = binding.BindsDirectlyToSource,
                Converter = binding.Converter,
                ConverterParameter = binding.ConverterParameter,
                ConverterCulture = binding.ConverterCulture,
                Mode = binding.Mode,
                NotifyOnValidationError = binding.NotifyOnValidationError,
                Path = binding.Path,
                UpdateSourceTrigger = binding.UpdateSourceTrigger,
                ValidatesOnExceptions = binding.ValidatesOnExceptions,
            };

            if (binding.ElementName != null)
                clone.ElementName = binding.ElementName;
            else if (binding.RelativeSource != null)
                clone.RelativeSource = binding.RelativeSource;
            else
                clone.Source = binding.Source;

            return clone;
        }
        #endregion


        #region ----- Design Mode -----

        /// <summary>
        /// Gets a value indicating whether the application is running in design mode (in Microsoft
        /// Visual Studio or Microsoft Expression Blend).
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this application is in design mode; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <example>
        /// In C# (for example, within a view model):
        /// <code lang="csharp">
        /// <![CDATA[
        /// public MyViewModel()
        /// {
        ///   if (WindowsHelper.IsInDesignMode)
        ///   {
        ///     // Code runs in Visual Studio Designer oder Expression Blend.
        ///     // --> Create design time data.
        ///   }
        ///   else
        ///   {
        ///     // Code runs in runtime.
        ///     // --> Connect to DB, services, etc...
        ///   }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static bool IsInDesignMode
        {
            get
            {
                // Detect design mode:
                // See http://blogs.msdn.com/jnak/archive/2006/10/07/Detecting-Design-Mode.aspx and
                // http://blog.galasoft.ch/archive/2009/09/05/detecting-design-time-mode-in-wpf-and-silverlight.aspx.
                if (!_isInDesignMode.HasValue)
                {
#if NETFX_CORE
                    _isInDesignMode = DesignMode.DesignModeEnabled;
#elif SILVERLIGHT || WINDOWS_PHONE
                    _isInDesignMode = DesignerProperties.IsInDesignTool;
#else
                    _isInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement))
                                                                        .Metadata
                                                                        .DefaultValue;

                    if (!_isInDesignMode.Value)
                    {
                        var processName = Process.GetCurrentProcess().ProcessName.ToUpperInvariant();
                        if (processName.Contains("DEVENV")
                            || processName.Contains("XDESPROC") // VS 2012 XAML Designer
                            || processName.StartsWith("V", StringComparison.Ordinal) && processName.Contains("EXPRESS"))
                        {
                            _isInDesignMode = true;
                        }
                    }
#endif
                }

                return _isInDesignMode.Value;
            }
        }
        private static bool? _isInDesignMode;
        #endregion


        #region ----- Numeric -----

        /// <overloads>
        /// <summary>
        /// Determines whether two <see cref="Point"/>s are equal (regarding a given tolerance).
        /// </summary>
        /// </overloads>
        /// 
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
        public static bool AreEqual(Point value1, Point value2)
        {
            return Numeric.AreEqual(value1.X, value2.X)
                   && Numeric.AreEqual(value1.Y, value2.Y);
        }


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
        public static bool AreEqual(Size value1, Size value2)
        {
            return Numeric.AreEqual(value1.Width, value2.Width)
                   && Numeric.AreEqual(value1.Height, value2.Height);
        }


        /// <summary>
        /// Determines whether two <see cref="Point"/>s are equal (regarding a specific
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
        public static bool AreEqual(Point value1, Point value2, double epsilon)
        {
            return Numeric.AreEqual(value1.X, value2.X, epsilon)
                   && Numeric.AreEqual(value1.Y, value2.Y, epsilon);
        }


        /// <summary>
        /// Determines whether two <see cref="Size"/>s are equal (regarding a specific
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
        public static bool AreEqual(Size value1, Size value2, double epsilon)
        {
            return Numeric.AreEqual(value1.Width, value2.Width, epsilon)
                   && Numeric.AreEqual(value1.Height, value2.Height, epsilon);
        }


        /// <overloads>
        /// <summary>
        /// Clamps near-zero values to zero.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Clamps near-zero values to zero.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// (0, 0) if the value is nearly zero (within the tolerance <see cref="Numeric.EpsilonD"/>)
        /// or the original value otherwise.
        /// </returns>
        public static Point ClampToZero(Point value)
        {
            return new Point(
              Numeric.ClampToZero(value.X),
              Numeric.ClampToZero(value.Y));
        }


        /// <summary>
        /// Clamps near-zero values to zero.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// (0, 0) if the value is nearly zero (within the tolerance <see cref="Numeric.EpsilonD"/>)
        /// or the original value otherwise.
        /// </returns>
        public static Size ClampToZero(Size value)
        {
            return new Size(
              Numeric.ClampToZero(value.Width),
              Numeric.ClampToZero(value.Height));
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
        public static Point ClampToZero(Point value, double epsilon)
        {
            return new Point(
              Numeric.ClampToZero(value.X, epsilon),
              Numeric.ClampToZero(value.Y, epsilon));
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
        public static Size ClampToZero(Size value, double epsilon)
        {
            return new Size(
              Numeric.ClampToZero(value.Width, epsilon),
              Numeric.ClampToZero(value.Height, epsilon));
        }


        /// <overloads>
        /// <summary>
        /// Determines whether an object is zero (regarding the tolerance
        /// <see cref="Numeric.EpsilonD"/>).
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Determines whether a <see cref="Point"/> is (0, 0) (regarding the tolerance
        /// <see cref="Numeric.EpsilonD"/>).
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Point"/> is (0, 0) (within the
        /// tolerance); otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// A value is zero if |x| &lt; <see cref="Numeric.EpsilonD"/>.
        /// </remarks>
        public static bool IsZero(Point value)
        {
            return Numeric.IsZero(value.X)
                   && Numeric.IsZero(value.Y);
        }


        /// <summary>
        /// Determines whether a <see cref="Point"/> is (0, 0) (regarding the tolerance
        /// <see cref="Numeric.EpsilonD"/>).
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Point"/> is (0, 0) (within the
        /// tolerance); otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// A value is zero if |x| &lt; <see cref="Numeric.EpsilonD"/>.
        /// </remarks>
        public static bool IsZero(Size value)
        {
            return Numeric.IsZero(value.Width)
                   && Numeric.IsZero(value.Height);
        }


        /// <summary>
        /// Determines whether a <see cref="Point"/> is (0, 0) (regarding a specific tolerance).
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="epsilon">The tolerance value.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Point"/> is (0, 0) (within the
        /// tolerance); otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// A value is zero if |x| &lt; epsilon.
        /// </remarks>
        public static bool IsZero(Point value, float epsilon)
        {
            return Numeric.IsZero(value.X, epsilon)
                   && Numeric.IsZero(value.Y, epsilon);
        }


        /// <summary>
        /// Determines whether a <see cref="Size"/> is (0, 0) (regarding a specific tolerance).
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="epsilon">The tolerance value.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Size"/> is (0, 0) (within the
        /// tolerance); otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// A value is zero if |x| &lt; epsilon.
        /// </remarks>
        public static bool IsZero(Size value, float epsilon)
        {
            return Numeric.IsZero(value.Width, epsilon)
                   && Numeric.IsZero(value.Height, epsilon);
        }
        #endregion


        #region ----- LINQ to Visual Tree -----

        /// <summary>
        /// Determines whether the current <see cref="DependencyObject"/> is a visual ancestor of
        /// another <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="ancestor">The dependency object expected to be the visual ancestor.</param>
        /// <param name="descendant">The dependency object expected to be the visual descendant.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ancestor"/> is a visual ancestor of
        /// <paramref name="descendant"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="descendant"/> or <paramref name="ancestor"/> is <see langword="null"/>.
        /// </exception>
        public static bool IsVisualAncestorOf(this DependencyObject ancestor, DependencyObject descendant)
        {
            return descendant.IsVisualDescendantOf(ancestor);
        }


        /// <summary>
        /// Determines whether the current <see cref="DependencyObject"/> is a visual descendant of
        /// another <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="descendant">The dependency object expected to be the visual descendant.</param>
        /// <param name="ancestor">The dependency object expected to be the visual ancestor.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="descendant"/> is a visual descendant of
        /// <paramref name="ancestor"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="descendant"/> or <paramref name="ancestor"/> is <see langword="null"/>.
        /// </exception>
        public static bool IsVisualDescendantOf(this DependencyObject descendant, DependencyObject ancestor)
        {
            if (descendant == null)
                throw new ArgumentNullException(nameof(descendant));
            if (ancestor == null)
                throw new ArgumentNullException(nameof(ancestor));

            if (descendant == ancestor)
                return false;

            var parent = VisualTreeHelper.GetParent(descendant);
            while (parent != null)
            {
                if (parent == ancestor)
                    return true;

                parent = VisualTreeHelper.GetParent(parent);
            }

            return false;
        }


        /// <summary>
        /// Gets the visual parent of the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the visual parent for.
        /// </param>
        /// <returns>
        /// The visual parent of the <see cref="DependencyObject"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static DependencyObject GetVisualParent(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return VisualTreeHelper.GetParent(dependencyObject);
        }


        /// <summary>
        /// Gets the visual ancestors of the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the visual ancestors for.
        /// </param>
        /// <returns>The visual ancestors of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetVisualAncestors(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetAncestors(dependencyObject, VisualTreeHelper.GetParent);
        }


        /// <summary>
        /// Gets the <see cref="DependencyObject"/> and its visual ancestors.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the visual ancestors for.
        /// </param>
        /// <returns>The <see cref="DependencyObject"/> and its visual ancestors.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetSelfAndVisualAncestors(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetSelfAndAncestors(dependencyObject, VisualTreeHelper.GetParent);
        }


        /// <summary>
        /// Gets the visual root of the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the visual root for.
        /// </param>
        /// <returns>The visual root of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static DependencyObject GetVisualRoot(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetRoot(dependencyObject, VisualTreeHelper.GetParent);
        }


        /// <summary>
        /// Gets the visual children of the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the visual children for.
        /// </param>
        /// <returns>The visual children of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return GetVisualChildrenImpl(dependencyObject);
        }


        private static IEnumerable<DependencyObject> GetVisualChildrenImpl(DependencyObject dependencyObject)
        {
#if !SILVERLIGHT && !WINDOWS_PHONE
            FrameworkElement frameworkElement = dependencyObject as FrameworkElement;
            if (frameworkElement != null)
                frameworkElement.ApplyTemplate();
#endif

            int count = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (int i = 0; i < count; ++i)
            {
                yield return VisualTreeHelper.GetChild(dependencyObject, i);
            }
        }


        /// <overloads>
        /// <summary>
        /// Gets the visual descendants of the <see cref="DependencyObject"/>.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Gets the visual descendants of the <see cref="DependencyObject"/> using a depth-first
        /// search.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the visual descendants for.
        /// </param>
        /// <returns>
        /// The visual descendants of the <see cref="DependencyObject"/> using a depth-first search.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetDescendants(dependencyObject, GetVisualChildren, true);
        }


        /// <summary>
        /// Gets the visual descendants of the <see cref="DependencyObject"/> using either a depth-
        /// or a breadth-first search.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the visual descendants for.
        /// </param>
        /// <param name="depthFirst">
        /// If set to <see langword="true"/> then a depth-first search for descendants will be made;
        /// otherwise a breadth-first search will be made.
        /// </param>
        /// <returns>The visual descendants of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject dependencyObject, bool depthFirst)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetDescendants(dependencyObject, GetVisualChildren, depthFirst);
        }


        /// <overloads>
        /// <summary>
        /// Gets the visual subtree (the given <see cref="DependencyObject"/> and all of its
        /// descendants).
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Gets the visual subtree (the given <see cref="DependencyObject"/> and all of its
        /// descendants) using a depth-first search.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the visual descendants for.
        /// </param>
        /// <returns>
        /// The visual descendants of the <see cref="DependencyObject"/> using a depth-first search.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetVisualSubtree(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetSubtree(dependencyObject, GetVisualChildren, true);
        }


        /// <summary>
        /// Gets the visual subtree (the given <see cref="DependencyObject"/> and all of its
        /// descendants) using either a depth- or a breadth-first search.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> to get the visual descendants for.
        /// </param>
        /// <param name="depthFirst">
        /// If set to <see langword="true"/> then a depth-first search for descendants will be made;
        /// otherwise a breadth-first search will be made.
        /// </param>
        /// <returns>The visual descendants of the <see cref="DependencyObject"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<DependencyObject> GetVisualSubtree(this DependencyObject dependencyObject, bool depthFirst)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            return TreeHelper.GetSubtree(dependencyObject, GetVisualChildren, depthFirst);
        }
        #endregion


        #region ----- Change Notification for DependencyProperty -----

        /// <summary>
        /// Registers an event handler that is called when a given <see cref="DependencyProperty"/>
        /// is changed.
        /// </summary>
        /// <param name="element">
        /// The element that handles the event. (Only required in Silverlight - can be
        /// <see langword="null"/> in WPF. In Silverlight it must be of type
        /// <see cref="FrameworkElement"/> or derived. It can be the same as
        /// <paramref name="source"/>.)
        /// </param>
        /// <param name="source">
        /// The element the property is set for. (Must be of type <see cref="DependencyObject"/> or
        /// derived. Can be the same as <paramref name="element"/>.)
        /// </param>
        /// <param name="propertyName">
        /// The registered name of a dependency property or an attached property.
        /// </param>
        /// <param name="ownerType">
        /// The <see cref="Type"/> of the object that owns the property definition. (Can be
        /// <see langword="null"/> in Silverlight.)
        /// </param>
        /// <param name="targetType">
        /// The <see cref="Type"/> of the element the property is set for. For dependency
        /// properties, <paramref name="ownerType"/> and <paramref name="targetType"/> are the same
        /// type. For attached properties they usually differ. (Can be <see langword="null"/> in
        /// Silverlight.)
        /// </param>
        /// <param name="handler">The event handler.</param>
        /// <remarks>
        /// <para>
        /// In WPF the event handler is only called if the property is changed. In Silverlight the
        /// event handler is also called once immediately after it is registered. (Users might want
        /// to ignore the first call of <paramref name="handler"/> in Silverlight.)
        /// </para>
        /// <para>
        /// In Silverlight the event handler will receive <paramref name="source"/> as the sender
        /// and empty <see cref="EventArgs"/>. The original <see cref="EventArgs"/> are not passed
        /// to the event handler.
        /// </para>
        /// <para>
        /// <strong>Warning:</strong> The event handler is stored using a strong reference. The
        /// <paramref name="source"/> will keep the event handler alive.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// A required parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="propertyName"/> is empty.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "element")]
        [Obsolete("This method is obsolete because it may create a strong reference in WPF. Use the class BindablePropertyObserver instead.")]
        public static void RegisterPropertyChangedEventHandler(FrameworkElement element, DependencyObject source, string propertyName, Type ownerType, Type targetType, EventHandler handler)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));
            if (propertyName.Length == 0)
                throw new ArgumentException("The property name must not be empty.", nameof(propertyName));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));


#if SILVERLIGHT || WINDOWS_PHONE
            if (element == null)
                throw new ArgumentNullException("element");

            // In Silverlight we can use a workaround: 
            // - Create an attached dependency property with a PropertyChangedEventHandler.
            // - Bind the attached dependency property to the dependency property that should be monitored.

            var attachedProperty = DependencyProperty.RegisterAttached(
                propertyName + "_Attached",
                typeof(object),
                source.GetType(),
                new PropertyMetadata(new PropertyChangedCallback((s, e) => handler(source, EventArgs.Empty))));

            Binding binding = new Binding(propertyName) { Source = source };
            element.SetBinding(attachedProperty, binding);
#else
            if (ownerType == null)
                throw new ArgumentNullException(nameof(ownerType));
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            // In WPF we can register an event handler using the DependencyPropertyDescriptor.
            var descriptor = DependencyPropertyDescriptor.FromName(propertyName, ownerType, targetType);
            descriptor.AddValueChanged(source, handler);
#endif
        }
        #endregion


        #region ----- ItemsControl -----

#if SILVERLIGHT || WINDOWS_PHONE
        /// <summary>
        /// Gets the child of a panel that contains the specified element.
        /// </summary>
        /// <param name="panel">The panel.</param>
        /// <param name="element">An element of type <see cref="UIElement"/>.</param>
        /// <returns>
        /// The <see cref="UIElement"/> in <see cref="Panel.Children"/> collection of 
        /// <paramref name="panel"/> that contains <paramref name="element"/>. Returns 
        /// <see langword="null"/> if <paramref name="element"/> is <see langword="null"/> or not part 
        /// of a child.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="panel"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
#else
        /// <summary>
        /// Gets the child of a panel that contains the specified element.
        /// </summary>
        /// <param name="panel">The panel.</param>
        /// <param name="element">
        /// In WPF: An element of type <see cref="Visual"/> or
        /// <see cref="FrameworkContentElement"/>. 
        /// In Silverlight: An element of type <see cref="UIElement"/>.
        /// </param>
        /// <returns>
        /// The <see cref="UIElement"/> in <see cref="Panel.Children"/> collection of
        /// <paramref name="panel"/> that contains <paramref name="element"/>. Returns
        /// <see langword="null"/> if <paramref name="element"/> is <see langword="null"/> or not
        /// part of a child.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="panel"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
#endif
        public static UIElement GetChildContainingElement(this Panel panel, DependencyObject element)
        {
            if (panel == null)
                throw new ArgumentNullException(nameof(panel));
            if (element == null)
                throw new ArgumentNullException(nameof(element));

#if !SILVERLIGHT && !WINDOWS_PHONE
            // Make sure that element is of type Visual.
            if (!(element is Visual))
                element = element.GetLogicalAncestors().OfType<Visual>().FirstOrDefault();

            Debug.Assert(element == null || element is Visual);
#endif

            if (element != null)
            {
                foreach (UIElement child in panel.Children)
                    if (child != null && child.IsAncestorOf(element))
                        return child;
            }

            return null;
        }


        /// <summary>
        /// Gets the <see cref="Panel"/> of an <see cref="ItemsControl"/> that hosts the items.
        /// </summary>
        /// <param name="itemsControl">The <see cref="ItemsControl"/>.</param>
        /// <returns>The items host of <paramref name="itemsControl"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="itemsControl"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static Panel GetItemsPanel(this ItemsControl itemsControl)
        {
            if (itemsControl == null)
                throw new ArgumentNullException(nameof(itemsControl));

            // First, make sure all elements are loaded.
            itemsControl.ApplyTemplate();

            // Get internal member ItemsControl.ItemsHost using reflection.
            return (Panel)typeof(ItemsControl).InvokeMember(
                "ItemsHost", BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance,
                null, itemsControl, null, CultureInfo.InvariantCulture);

            //// Try to get item panel from item container.
            //if (itemsControl.Items.Count > 0)
            //{
            //    var itemContainer = itemsControl.ItemContainerGenerator.ContainerFromIndex(0);
            //    if (itemContainer != null)
            //        return VisualTreeHelper.GetParent(itemContainer) as Panel;
            //}

            //// Search visual tree.
            //return itemsControl.GetVisualDescendants()
            //                   .OfType<Panel>()
            //                   .FirstOrDefault(panel => ItemsControl.GetItemsOwner(panel) == itemsControl);
        }


#if !SILVERLIGHT
        /// <summary>
        /// Ensures that the item containers of an <see cref="ItemsControl"/> have been
        /// generated.
        /// </summary>
        /// <param name="itemsControl">The <see cref="ItemsControl"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the item containers are available; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// An items panel is required to generate item containers. The method automatically tries
        /// to build the template's visual tree if it is missing or incomplete. A return value of 
        /// <see langword="false"/> usually indicates that the visual tree is not available at this
        /// point.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="itemsControl"/> is <see langword="null"/>.
        /// </exception>
        public static bool EnsureItemContainers(this ItemsControl itemsControl)
        {
            if (itemsControl == null)
                throw new ArgumentNullException(nameof(itemsControl));

#if !WINDOWS_PHONE
            if (itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                return true;
#endif

            // Find the items host.
            var panel = itemsControl.GetItemsPanel();

            // Querying the UIElementCollection generates the item containers!
            return panel != null && panel.Children != null;
        }
#endif
        #endregion


        #region ----- Dispatcher -----

        /// <summary>
        /// Gets or sets the dispatcher associated with the UI thread.
        /// </summary>
        /// <value>
        /// The <see cref="Dispatcher"/> associated with the UI thread, or <see langword="null"/> if
        /// there is currently no dispatcher associated with the UI thread. A custom dispatcher can
        /// be set to overwrite default dispatcher. (Setting a value of <see langword="null"/>
        /// resets the property to the default dispatcher.)
        /// </value>
        /// <seealso cref="CheckAccess"/>
        /// <seealso cref="CheckBeginInvokeOnUI"/>
        /// <seealso cref="BeginInvokeOnUI"/>
        /// <seealso cref="InvokeOnUI"/>
#if NETFX_CORE
        public static CoreDispatcher Dispatcher
#else
        public static Dispatcher Dispatcher
#endif
        {
            get
            {
                // Note: Background threads should never occur at design-time.
                if (_dispatcher == null && !IsInDesignMode)
                {
#if NETFX_CORE
                    if (Window.Current != null)
                        _dispatcher = Window.Current.Dispatcher;
#elif SILVERLIGHT || WINDOWS_PHONE
                    if (Deployment.Current != null)
                        _dispatcher = Deployment.Current.Dispatcher;
#else
                    if (Application.Current != null)
                        _dispatcher = Application.Current.Dispatcher;
#endif
                }

                return _dispatcher;
            }
            set { _dispatcher = value; }
        }
#if NETFX_CORE
        private static CoreDispatcher _dispatcher;
#else
        private static Dispatcher _dispatcher;
#endif


        /// <summary>
        /// Determines whether the calling thread is the UI thread.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the calling thread is the UI thread; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <seealso cref="Dispatcher"/>
        /// <seealso cref="CheckBeginInvokeOnUI"/>
        /// <seealso cref="BeginInvokeOnUI"/>
        /// <seealso cref="InvokeOnUI"/>
        public static bool CheckAccess()
        {
            var dispatcher = Dispatcher;
#if NETFX_CORE
            return dispatcher == null || dispatcher.HasThreadAccess;
#else
            return dispatcher == null || dispatcher.CheckAccess();
#endif
        }



        /// <overloads>
        /// <summary>
        /// If necessary, executes the specified action asynchronously on the UI thread. (If the
        /// method is called on the UI thread the action is executed immediately.)
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// If necessary, executes the specified action asynchronously on the UI thread. (If the
        /// method is called on the UI thread the action is executed immediately.)
        /// </summary>
        /// <param name="action">The action that should be executed on the UI thread.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Dispatcher"/>
        /// <seealso cref="CheckAccess"/>
        /// <seealso cref="BeginInvokeOnUI"/>
        /// <seealso cref="InvokeOnUI"/>
        public static void CheckBeginInvokeOnUI(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var dispatcher = Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
#if NETFX_CORE
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
#else
                dispatcher.BeginInvoke(action);
#endif
            }
        }


#if !NETFX_CORE
        /// <summary>
        /// If necessary, executes the specified action asynchronously on the UI thread. (If the
        /// method is called on the UI thread the action is executed immediately.)
        /// </summary>
        /// <param name="action">The action that should be executed on the UI thread.</param>
        /// <param name="priority">
        /// The priority, relative to the other pending operations in the
        /// <see cref="System.Windows.Threading.Dispatcher"/> event queue, the specified method is
        /// invoked.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Dispatcher"/>
        /// <seealso cref="CheckAccess"/>
        /// <seealso cref="BeginInvokeOnUI"/>
        /// <seealso cref="InvokeOnUI"/>
        public static void CheckBeginInvokeOnUI(Action action, DispatcherPriority priority)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var dispatcher = Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.BeginInvoke(action, priority);
            }
        }
#endif


        /// <summary>
        /// Executes the specified action asynchronously on the UI thread.
        /// </summary>
        /// <param name="action">The action that should be executed on the UI thread.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Dispatcher"/>
        /// <seealso cref="CheckAccess"/>
        /// <seealso cref="CheckBeginInvokeOnUI"/>
        /// <seealso cref="InvokeOnUI"/>
        public static void BeginInvokeOnUI(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var dispatcher = Dispatcher;
            if (dispatcher == null)
            {
                action();
            }
            else
            {
#if NETFX_CORE
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
#else
                dispatcher.BeginInvoke(action);
#endif
            }
        }


#if !NETFX_CORE
        /// <overloads>
        /// <summary>
        /// Executes the specified action asynchronously on the UI thread.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Executes the specified action asynchronously on the UI thread.
        /// </summary>
        /// <param name="action">The action that should be executed on the UI thread.</param>
        /// <param name="priority">
        /// The priority, relative to the other pending operations in the
        /// <see cref="System.Windows.Threading.Dispatcher"/> event queue, the specified method is
        /// invoked.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Dispatcher"/>
        /// <seealso cref="CheckAccess"/>
        /// <seealso cref="CheckBeginInvokeOnUI"/>
        /// <seealso cref="InvokeOnUI"/>
        public static void BeginInvokeOnUI(Action action, DispatcherPriority priority)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var dispatcher = Dispatcher;
            if (dispatcher == null)
            {
                action();
            }
            else
            {
                dispatcher.BeginInvoke(action, priority);
            }
        }
#endif


#if !WP8
        /// <summary>
        /// Executes the specified action asynchronously on the UI thread.
        /// </summary>
        /// <param name="action">The action that should be executed on the UI thread.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        private static Task RunAsync(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var dispatcher = Dispatcher;
#if NETFX_CORE
            return dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AsTask();
#elif NET45
            return dispatcher.InvokeAsync(action).Task;
#else
            // Legacy implementation for .NET 4 and Silverlight 5:
            var tcs = new TaskCompletionSource<object>();
            Action wrapper = () =>
                             {
                                 try
                                 {
                                     action();
                                     tcs.SetResult(null);
                                 }
                                 catch (Exception ex)
                                 {
                                     tcs.SetException(ex);
                                 }
                             };
            dispatcher.BeginInvoke(wrapper);
            return tcs.Task;
#endif
        }
#endif


        /// <summary>
        /// Executes the specified action synchronously on the UI thread. (The method blocks until
        /// the action has been executed.)
        /// </summary>
        /// <param name="action">The action that should be executed on the UI thread.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Dispatcher"/>
        /// <seealso cref="CheckAccess"/>
        /// <seealso cref="CheckBeginInvokeOnUI"/>
        /// <seealso cref="BeginInvokeOnUI"/>
        public static void InvokeOnUI(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var dispatcher = Dispatcher;
            if (dispatcher == null
#if NETFX_CORE
                || dispatcher.HasThreadAccess()
#else
                || dispatcher.CheckAccess()
#endif
)
            {
                action();
            }
            else
            {
#if WP8
                // Legacy implementation Windows Phone 7:
                var waitHandle = new ManualResetEvent(false);
                Exception exception = null;
                dispatcher.BeginInvoke(() =>
                                       {
                                         try
                                         {
                                           action();
                                         }
                                         catch (Exception ex)
                                         {
                                           exception = ex;
                                         }
                                         finally
                                         {
                                           waitHandle.Set();
                                         }
                                       });
                waitHandle.WaitOne();
        
                if (exception != null)
                    throw new TargetInvocationException("An error occurred while dispatching a call to the UI thread.", exception);
#else
                RunAsync(action).Wait();
#endif
            }
        }
#endregion
    }
}
