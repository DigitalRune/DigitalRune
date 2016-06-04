using System;
using System.Collections.Generic;
using System.Windows;
using DigitalRune.Windows;
using DigitalRune.Windows.Docking;


namespace WindowsThemesApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public IList<string> Themes { get; }
        public int SelectedThemeIndex
        {
            get { return _selectedThemeIndex; }
            set {
                if (SetProperty(ref _selectedThemeIndex, value))
                {
                    switch (value)
                    {
                        case 1:
                            MenuToUpperConverter.IsEnabled = false;
                            ThemeManager.ApplyTheme(
                                Application.Current,
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Theme.xaml", UriKind.Absolute)),
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Colors/System.xaml", UriKind.Absolute)));
                            break;
                        case 2:
                            MenuToUpperConverter.IsEnabled = true;
                            ThemeManager.ApplyTheme(
                                Application.Current,
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Theme.xaml", UriKind.Absolute)),
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Colors/Light.xaml", UriKind.Absolute)));
                            break;
                        case 3:
                            MenuToUpperConverter.IsEnabled = false;
                            ThemeManager.ApplyTheme(
                                Application.Current,
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Theme.xaml", UriKind.Absolute)),
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Colors/Gray.xaml", UriKind.Absolute)));
                            break;
                        case 4:
                            MenuToUpperConverter.IsEnabled = true;
                            ThemeManager.ApplyTheme(
                                Application.Current,
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Theme.xaml", UriKind.Absolute)),
                                new Theme("Modern", new Uri("pack://application:,,,/DigitalRune.Windows.Themes;component/Themes/Modern/Colors/Dark.xaml", UriKind.Absolute)));
                            break;
                        default:
                            MenuToUpperConverter.IsEnabled = false;
                            ThemeManager.ApplyTheme(Application.Current, null, null);
                            break;
                    }
                }
            }
        }
        private int _selectedThemeIndex;

        public DockControlViewModel DockControlViewModel { get; }


        public MainViewModel()
        {
            Themes = new List<string>
            {
                "Default",
                "System",
                "Light",
                "Gray",
                "Dark",
            };
            SelectedThemeIndex = 3;

            // Build dock layout directly.
            //var dockTabPaneViewModel = new DockTabPaneViewModel();
            //dockTabPaneViewModel.Items.Add(new SystemViewModel { DockState = DockState.Dock });
            //dockTabPaneViewModel.Items.Add(new SystemViewModel { DockState = DockState.Dock });
            //dockTabPaneViewModel.Items.Add(new DigitalRuneViewModel { DockState = DockState.Dock });
            //dockTabPaneViewModel.Items.Add(new DigitalRuneViewModel { DockState = DockState.Dock });
            //dockTabPaneViewModel.Items.Add(new PropertyGridViewModel { DockState = DockState.Dock });
            //dockTabPaneViewModel.Items.Add(new PropertyGridViewModel { DockState = DockState.Dock });
            //dockTabPaneViewModel.Items.Add(new ICSharpDevelopViewModel { DockState = DockState.Dock });
            //dockTabPaneViewModel.Items.Add(new ICSharpDevelopViewModel { DockState = DockState.Dock });

            //var dockAnchorPaneViewModel = new DockAnchorPaneViewModel
            //{
            //    ChildPane = dockTabPaneViewModel
            //};

            //DockControlViewModel = new DockControlViewModel(new DockStrategy())
            //{
            //    RootPane = dockAnchorPaneViewModel
            //};

            // Or use DockStrategy to do the same.
            var dockStrategy = new DockStrategy();
            DockControlViewModel = new DockControlViewModel(dockStrategy)
            {
                RootPane = new DockAnchorPaneViewModel(),
            };
            dockStrategy.Begin();
            dockStrategy.Dock(new SystemViewModel());
            dockStrategy.Dock(new SystemViewModel());
            dockStrategy.Dock(new DigitalRuneViewModel());
            dockStrategy.Dock(new DigitalRuneViewModel());
            dockStrategy.Dock(new PropertyGridViewModel());
            dockStrategy.Dock(new PropertyGridViewModel());
            dockStrategy.Dock(new ICSharpDevelopViewModel());
            dockStrategy.Dock(new ICSharpDevelopViewModel());
            dockStrategy.End();
        }
    }
}
