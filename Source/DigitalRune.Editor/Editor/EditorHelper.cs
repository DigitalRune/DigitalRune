// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using DigitalRune.ServiceLocation;
using DigitalRune.Windows;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using NLog;
using static System.FormattableString;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Provides helper method for the editor.
    /// </summary>
    public static class EditorHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        //--------------------------------------------------------------
        #region Collections
        //--------------------------------------------------------------

        /// <summary>
        /// Converts a sequence of <see cref="string"/>s to a <see cref="StringCollection"/>.
        /// </summary>
        /// <param name="texts">The sequence of <see cref="string"/>s.</param>
        /// <returns>The <see cref="StringCollection"/>.</returns>
        internal static StringCollection AsStringCollection(this IEnumerable<string> texts)
        {
            var stringCollection = new StringCollection();
            if (texts != null)
                foreach (var text in texts)
                    stringCollection.Add(text);

            return stringCollection;
        }


        /// <summary>
        /// Converts a <see cref="StringCollection"/> to a sequence of <see cref="string"/>s.
        /// </summary>
        /// <param name="stringCollection">The <see cref="StringCollection"/>.</param>
        /// <returns>The sequence of <see cref="string"/>s.</returns>
        internal static IEnumerable<string> AsEnumerable(this StringCollection stringCollection)
        {
            if (stringCollection == null)
                return Enumerable.Empty<string>();

            return stringCollection.Cast<string>();
        }


        /// <summary>
        /// Adds the <see cref="string"/>s to the collection.
        /// </summary>
        /// <param name="collection">The target collection.</param>
        /// <param name="stringCollection">The source collection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        internal static void AddRange(this ICollection<string> collection, StringCollection stringCollection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (stringCollection != null)
                foreach (var text in stringCollection)
                    collection.Add(text);
        }
        #endregion


        //--------------------------------------------------------------
        #region Configuration
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the name of the application from the assembly information of the entry assembly.
        /// </summary>
        /// <returns>The name of the application.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        internal static string GetDefaultApplicationName()
        {
            string applicationName;

            // Try to get the name from the 'Title' attribute of the entry assembly.
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)  // Can be null at design-time.
            {
                var titleAttribute = entryAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)
                                                  .Cast<AssemblyTitleAttribute>()
                                                  .FirstOrDefault();
                applicationName = titleAttribute?.Title;
                if (!string.IsNullOrEmpty(applicationName))
                    return applicationName;
            }

            // Get the name of the entry assembly.
            if (entryAssembly != null)
            {
                applicationName = entryAssembly.GetName().Name;
                if (!string.IsNullOrEmpty(applicationName))
                    return applicationName;
            }

            return "Unnamed Application";
        }


        /// <summary>
        /// Gets the name of the executable.
        /// </summary>
        /// <returns>The name of the executable.</returns>
        internal static string GetExecutableName()
        {
            return Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location);
        }


        /// <summary>
        /// Gets the folder that contains user settings.
        /// </summary>
        /// <param name="userLevel">
        /// The user level. (See
        /// <see cref="ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel)"/> for more
        /// info.)
        /// </param>
        /// <returns>The folder (path) where user settings are stored.</returns>
        public static string GetUserSettingsFolder(ConfigurationUserLevel userLevel)
        {
            try
            {
                var userConfiguration = ConfigurationManager.OpenExeConfiguration(userLevel);
                return Path.GetDirectoryName(userConfiguration.FilePath);
            }
            catch (ConfigurationException exception)
            {
                Logger.Warn("Could not retrieve folder where user settings are stored.");
                Logger.Debug(exception, "Exception in GetUserSettingsFolder.");
                return exception.Filename;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Resources
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the <see cref="BitmapSource"/> for a packed bitmap.
        /// </summary>
        /// <param name="uriString">
        /// The URI string of the image that contains all packed bitmaps.
        /// </param>
        /// <param name="x">The x-coordinate of the top-left corner of the source rectangle.</param>
        /// <param name="y">The y-coordinate of the top-left corner of the source rectangle.</param>
        /// <param name="width">The width of the source rectangle.</param>
        /// <param name="height">The height of the source rectangle.</param>
        /// <returns>The <see cref="BitmapSource"/> for the packed bitmap.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
        public static BitmapSource GetPackedBitmap(string uriString, int x, int y, int width, int height)
        {
            var bitmapImage = new BitmapImage(new Uri(uriString, UriKind.RelativeOrAbsolute));
            bitmapImage.Freeze();

            var croppedBitmap = new CroppedBitmap(bitmapImage, new Int32Rect(x, y, width, height));
            croppedBitmap.Freeze();

            return croppedBitmap;
        }


        /// <summary>
        /// Adds the specified resource dictionary to the application.
        /// </summary>
        /// <param name="resources">The resource dictionary.</param>
        public static void RegisterResources(ResourceDictionary resources)
        {
            Application.Current.Resources.MergedDictionaries.Add(resources);
        }


        /// <summary>
        /// Removes the specified resource dictionary from the application.
        /// </summary>
        /// <param name="resources">The resource dictionary.</param>
        public static void UnregisterResources(ResourceDictionary resources)
        {
            Application.Current.Resources.MergedDictionaries.Remove(resources);
        }
        #endregion


        //--------------------------------------------------------------
        #region Services
        //--------------------------------------------------------------

        /// <summary>
        /// Registers a view model.
        /// </summary>
        /// <param name="serviceContainer">The service container.</param>
        /// <param name="viewModelType">The type of the view model.</param>
        /// <param name="creationPolicy">
        /// The creation policy that specifies when and how the view will be instantiated.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceContainer"/> is <see langword="null"/>.
        /// </exception>
        public static void RegisterViewModel(this ServiceContainer serviceContainer, Type viewModelType, CreationPolicy creationPolicy = CreationPolicy.NonShared)
        {
            if (serviceContainer == null)
                throw new ArgumentNullException(nameof(serviceContainer));

            serviceContainer.Register(viewModelType, null, viewModelType, creationPolicy);
        }


        /// <summary>
        /// Unregisters a view model.
        /// </summary>
        /// <param name="serviceContainer">The service container.</param>
        /// <param name="viewModelType">The type of the view model.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceContainer"/> is <see langword="null"/>.
        /// </exception>
        public static void UnregisterViewModel(this ServiceContainer serviceContainer, Type viewModelType)
        {
            if (serviceContainer == null)
                throw new ArgumentNullException(nameof(serviceContainer));

            serviceContainer.Unregister(viewModelType, null);
        }


        /// <summary>
        /// Registers a view for a view model.
        /// </summary>
        /// <param name="serviceContainer">The service container.</param>
        /// <param name="viewModelType">The type of the view model.</param>
        /// <param name="view">The type of the view.</param>
        /// <param name="creationPolicy">
        /// The creation policy that specifies when and how the view will be instantiated.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceContainer"/> or <paramref name="viewModelType"/> is
        /// <see langword="null"/>.
        /// </exception>
        public static void RegisterView(this ServiceContainer serviceContainer, Type viewModelType, Type view, CreationPolicy creationPolicy = CreationPolicy.NonShared)
        {
            if (serviceContainer == null)
                throw new ArgumentNullException(nameof(serviceContainer));
            if (viewModelType == null)
                throw new ArgumentNullException(nameof(viewModelType));

            serviceContainer.Register(typeof(FrameworkElement), viewModelType.FullName, view, creationPolicy);
        }


        /// <summary>
        /// Unregisters a view for a view model.
        /// </summary>
        /// <param name="serviceContainer">The service container.</param>
        /// <param name="viewModelType">The type of the view model.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceContainer"/> or <paramref name="viewModelType"/> is
        /// <see langword="null"/>.
        /// </exception>
        public static void UnregisterView(this ServiceContainer serviceContainer, Type viewModelType)
        {
            if (serviceContainer == null)
                throw new ArgumentNullException(nameof(serviceContainer));
            if (viewModelType == null)
                throw new ArgumentNullException(nameof(viewModelType));

            serviceContainer.Unregister(typeof(FrameworkElement), viewModelType.FullName);
        }


        /// <summary>
        /// Throws a <seealso cref="ServiceNotFoundException"/> if the specified services is
        /// <see langword="null"/>.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="service">The service.</param>
        /// <returns>The service.</returns>
        /// <exception cref="ServiceNotFoundException">
        /// <paramref name="service"/> is <see langword="null"/>.
        /// </exception>
        public static T ThrowIfMissing<T>(this T service) where T : class
        {
            if (service == null)
            {
                var message = Invariant($"The service of type {typeof(T).Name} is missing.");
                throw new ServiceNotFoundException(message);
            }

            return service;
        }


        /// <summary>
        /// Logs a warning if the specified service is <see langword="null"/>.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="service">The service.</param>
        /// <returns>The service.</returns>
        public static T WarnIfMissing<T>(this T service) where T : class
        {
            if (service == null)
                Logger.Warn(CultureInfo.InvariantCulture, "The service of type {0} is missing.", typeof(T).Name);

            return service;
        }
        #endregion


        //--------------------------------------------------------------
        #region Misc
        //--------------------------------------------------------------

        /// <summary>
        /// Removes the '_' which is used to indicate access keys from the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text without '_'.</returns>
        internal static string FilterAccessKeys(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            int index = text.IndexOf('_');
            if (index >= 0)
                text = text.Remove(index, 1);

            return text;
        }


        /// <summary>
        /// Focuses the specified editor.
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <param name="dataContext">
        /// The data context of the UI element which should receive the focus.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dataContext"/> is <see langword="null"/>.
        /// </exception>
        public static void Focus(this IEditorService editor, object dataContext)
        {
            if (dataContext == null)
                throw new ArgumentNullException(nameof(dataContext));

            editor?.Services.GetInstance<IMessageBus>()?.Publish(new FocusMessage(dataContext));
        }


        /// <summary>
        /// Gets the <see cref="IEditorService"/> using the visual tree.
        /// </summary>
        /// <param name="element">
        /// A UI element in the <see cref="EditorWindow"/> or in a <see cref="FloatWindow"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IEditorService"/> or <see langword="null"/> if it could not be found.
        /// </returns>
        /// <remarks>
        /// A warning is logged when the editor could not be found.
        /// </remarks>
        public static IEditorService GetEditor(this DependencyObject element)
        {
            if (element == null)
                return null;

            var window = element as Window;
            if (window == null)
                window = Window.GetWindow(element);

            if (window != null)
            {
                var floatWindow = window as FloatWindow;
                if (floatWindow != null)
                    window = floatWindow.Owner;

                return window.DataContext as IEditorService;
            }

            foreach (var e in element.GetSelfAndVisualAncestors().OfType<FrameworkElement>())
            {
                var editor = e.DataContext as IEditorService;
                if (editor != null)
                    return editor;
            }

            Logger.Warn("Could not find editor view model in visual tree.");
            return null;
        }
        #endregion
    }
}
