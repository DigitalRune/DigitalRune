using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DigitalRune.Windows.Controls;

namespace WindowsControlsApp
{
    public partial class ExplorerViewTest
    {
        public ExplorerViewTest()
        {
            InitializeComponent();
        }


        private void OnButtonClick(object sender, RoutedEventArgs eventArgs)
        {
            var explorerView = WallpaperListView.View as ExplorerView;
            if (explorerView != null)
            {
                int viewMode = (int)explorerView.Mode;
                viewMode = (viewMode + 1) % ((int)ExplorerViewMode.ExtraLargeIcons + 1);
                explorerView.Mode = (ExplorerViewMode)viewMode;
            }
        }


        private void OnListViewMouseWheel(object sender, MouseWheelEventArgs eventArgs)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                var explorerView = WallpaperListView.View as ExplorerView;
                if (explorerView != null)
                {
                    if (eventArgs.Delta > 0)
                    {
                        int steps = eventArgs.Delta / Mouse.MouseWheelDeltaForOneLine;
                        for (int i = 0; i < steps; i++)
                            explorerView.IncreaseScale();
                    }
                    else if (eventArgs.Delta < 0)
                    {
                        int steps = -eventArgs.Delta / Mouse.MouseWheelDeltaForOneLine;
                        for (int i = 0; i < steps; i++)
                            explorerView.DecreaseScale();
                    }
                }
                eventArgs.Handled = true;
            }
        }


        #region ----- Column Sorting -----

        GridViewColumnHeader _lastHeaderClicked;
        ListSortDirection _lastSortDirection = ListSortDirection.Ascending;

        private void OnGridViewColumnHeaderClick(object sender, RoutedEventArgs eventArgs)
        {
            var headerClicked = eventArgs.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    ListSortDirection direction;

                    if (headerClicked != _lastHeaderClicked)
                        direction = ListSortDirection.Ascending;
                    else
                    {
                        if (_lastSortDirection == ListSortDirection.Ascending)
                            direction = ListSortDirection.Descending;
                        else
                            direction = ListSortDirection.Ascending;
                    }

                    string header = headerClicked.Column.Header as string;
                    Sort(header, direction);

                    if (direction == ListSortDirection.Ascending)
                        headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    else
                        headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowDown"] as DataTemplate;

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                        _lastHeaderClicked.Column.HeaderTemplate = null;

                    _lastHeaderClicked = headerClicked;
                    _lastSortDirection = direction;
                }
            }
        }


        private void Sort(string sortBy, ListSortDirection direction)
        {
            // TODO: Alphanumeric sorting is used. Columns containing numbers (e.g. file size) are 
            // not sorted correctly.
            var dataView = CollectionViewSource.GetDefaultView(WallpaperListView.ItemsSource);
            dataView.SortDescriptions.Clear();
            var sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
        #endregion
    }


    public class Wallpaper
    {
        public BitmapImage Image { get; set; }
        public string Name { get; set; }
        public string Dimensions { get; set; }
        public string Size { get; set; }
    }


    public class WallpaperCollection : ObservableCollection<Wallpaper>
    {
        public WallpaperCollection()
        {
            LoadWallpapers();
        }


        private void LoadWallpapers()
        {
            var wallpaperDirectory = new DirectoryInfo(Path.Combine(Environment.SystemDirectory, @"..\Web\Wallpaper"));

            // Recursively collect all wallpaper images.
            AddWallpapers(wallpaperDirectory);
        }


        private void AddWallpapers(DirectoryInfo directory)
        {
            foreach (var fileInfo in directory.GetFiles())
            {
                if (string.Equals(fileInfo.Extension, ".JPG", StringComparison.InvariantCultureIgnoreCase))
                {
                    var wallpaper = new Wallpaper();
                    wallpaper.Name = fileInfo.Name;
                    wallpaper.Size = (fileInfo.Length / 1024) + " KB";
                    wallpaper.Image = new BitmapImage(new Uri(fileInfo.FullName));
                    wallpaper.Dimensions = wallpaper.Image.PixelWidth + " x " + wallpaper.Image.PixelHeight;

                    Add(wallpaper);
                }
            }

            foreach (var subDirectory in directory.GetDirectories())
            {
                AddWallpapers(subDirectory);
            }
        }
    }
}
