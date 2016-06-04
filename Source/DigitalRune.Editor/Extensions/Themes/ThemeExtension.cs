// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using DigitalRune.Collections;
using DigitalRune.Editor.Options;
using DigitalRune.Editor.Properties;
using DigitalRune.Windows;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using NLog;
using static System.FormattableString;


namespace DigitalRune.Editor.Themes
{
    /// <summary>
    /// Provides support for switching UI themes.
    /// </summary>
    public sealed class ThemeExtension : EditorExtension, IThemeService
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IMessageBus _messageBus;
        private ResourceDictionary _resourceDictionary;
        private ResourceDictionary _windowChromeResourceDictionary;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        private MergeableNodeCollection<ICommandItem> _toolBarNodes;
        private MergeableNodeCollection<OptionsPageViewModel> _optionsNodes;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public IEnumerable<string> Themes { get; }


        /// <inheritdoc/>
        public string Theme
        {
            get { return _theme; }
            set
            {
                if (_theme == value)
                    return;

                try
                {
                    Logger.Info(CultureInfo.InvariantCulture, "Applying theme \"{0}\"", value);

                    switch (value)
                    {
                        case "System":
                            MenuToUpperConverter.IsEnabled = false;
                            ThemeManager.ApplyTheme(
                                Application.Current,
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Theme.xaml", UriKind.Absolute)),
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Colors/System.xaml", UriKind.Absolute)));
                            break;
                        case "Light":
                            MenuToUpperConverter.IsEnabled = true;
                            ThemeManager.ApplyTheme(
                                Application.Current,
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Theme.xaml", UriKind.Absolute)),
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Colors/Light.xaml", UriKind.Absolute)));
                            break;
                        case "Gray":
                            MenuToUpperConverter.IsEnabled = false;
                            ThemeManager.ApplyTheme(
                                Application.Current,
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Theme.xaml", UriKind.Absolute)),
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Colors/Gray.xaml", UriKind.Absolute)));
                            break;
                        case "Dark":
                            MenuToUpperConverter.IsEnabled = true;
                            ThemeManager.ApplyTheme(
                                Application.Current,
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Theme.xaml", UriKind.Absolute)),
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Colors/Dark.xaml", UriKind.Absolute)));
                            break;
                        default:
                            throw new EditorException(Invariant($"Cannot change theme: Theme '{value}' is not registered in the ThemeExtension."));
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "Unable to change theme.");
                    throw;
                }

                _theme = value;

                OnThemeChanged(EventArgs.Empty);
            }
        }
        private string _theme;


        /// <summary>
        /// Occurs when the UI theme was changed.
        /// </summary>
        /// <seealso cref="Theme"/>
        public event EventHandler<EventArgs> ThemeChanged;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeExtension"/> class.
        /// </summary>
        public ThemeExtension()
        {
            Themes = new ReadOnlyCollection<string>(new[]
            {
                "System",
                "Light",
                "Gray",
                "Dark",
            });

            Theme = "Gray";
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Raises the <see cref="ThemeChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        ///// <remarks>
        ///// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnThemeChanged"/> in a
        ///// derived class, be sure to call the base class's <see cref="OnThemeChanged"/> method so
        ///// that registered delegates receive the event.
        ///// </remarks>
        private void OnThemeChanged(EventArgs eventArgs)
        {
            ThemeChanged?.Invoke(this, eventArgs);

            // Broadcast message over message bus.
            _messageBus?.Publish(new ThemeMessage(Theme));
        }


        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            Editor.Services.Register(typeof(IThemeService), null, this);
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            _messageBus = Editor.Services.GetInstance<IMessageBus>().WarnIfMissing();

            AddDataTemplates();
            AddWindowChrome();

            LoadTheme();

            AddCommands();
            AddMenus();
            AddToolBars();
            AddOptions();
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            RemoveOptions();
            RemoveToolBars();
            RemoveMenus();
            RemoveCommands();

            SaveTheme();

            RemoveDataTemplates();
            RemoveWindowChrome();

            _messageBus = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.Unregister(typeof(IThemeService), null);
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Extensions/Themes/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
            EditorHelper.RegisterResources(_resourceDictionary);
        }


        private void RemoveDataTemplates()
        {
            EditorHelper.UnregisterResources(_resourceDictionary);
            _resourceDictionary = null;
        }


        private void AddWindowChrome()
        {
            _windowChromeResourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/WindowChrome.xaml", UriKind.RelativeOrAbsolute) };
            EditorHelper.RegisterResources(_windowChromeResourceDictionary);
        }


        private void RemoveWindowChrome()
        {
            EditorHelper.UnregisterResources(_windowChromeResourceDictionary);
            _windowChromeResourceDictionary = null;
        }


        private void AddCommands()
        {
            CommandItems.Add(new ThemeCommandItem(this));
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();
        }


        private void AddMenus()
        {
            _menuNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("ToolsGroup", "_Tools"),
                    new MergeableNode<ICommandItem>(CommandItems["Theme"], new MergePoint(MergeOperation.InsertBefore, "ShowOptions"), MergePoint.Append)),
            };

            Editor.MenuNodeCollections.Add(_menuNodes);
        }


        private void RemoveMenus()
        {
            Editor.MenuNodeCollections.Remove(_menuNodes);
            _menuNodes = null;
        }


        private void AddToolBars()
        {
            _toolBarNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("StandardGroup", "Standard"),
                    new MergeableNode<ICommandItem>(CommandItems["Theme"])),
            };

            Editor.ToolBarNodeCollections.Add(_toolBarNodes);
        }


        private void RemoveToolBars()
        {
            Editor.ToolBarNodeCollections.Remove(_toolBarNodes);
            _toolBarNodes = null;
        }


        private void AddOptions()
        {
            _optionsNodes = new MergeableNodeCollection<OptionsPageViewModel>
            {
                new MergeableNode<OptionsPageViewModel> { Content = new ThemeOptionsPageViewModel(this) }
            };

            var optionsService = Editor.Services.GetInstance<IOptionsService>().WarnIfMissing();
            optionsService?.OptionsNodeCollections.Add(_optionsNodes);
        }


        private void RemoveOptions()
        {
            if (_optionsNodes == null)
                return;

            var optionsService = Editor.Services.GetInstance<IOptionsService>().WarnIfMissing();
            optionsService?.OptionsNodeCollections.Remove(_optionsNodes);
            _optionsNodes = null;
        }


        private void LoadTheme()
        {
            Logger.Debug("Loading WPF theme.");

            string theme = Settings.Default.Theme;
            if (string.IsNullOrEmpty(theme))
                return;

            if (Themes.Contains(theme))
            {
                Theme = theme;
            }
            else
            {
                Logger.Warn("Invalid theme settings. Using defaults instead.");
            }
        }


        private void SaveTheme()
        {
            Logger.Debug(CultureInfo.InvariantCulture, "Saving theme \"{0}\".", Theme);
            Settings.Default.Theme = Theme;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            return null;
        }
        #endregion
    }
}
