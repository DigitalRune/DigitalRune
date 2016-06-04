using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


// Note:
// The ViewModelLocator is only used to demonstrate the common view model locator pattern. 
// In this application the ViewModelLocator is only used at design-time in MainWindow.xaml.
// The ViewModelLocator can be removed, if services are registered in AppBootstrapper.cs and 
// if static properties (e.g. MainWindowViewModel.DesignInstance) are used at design-time.


namespace ScreenConductionApp
{
    /// <summary>
    /// Provides references to view models and allows them to be used in bindings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="ViewModelLocator"/> instance is usually created in the application resources
    /// (App.xaml):
    /// </para>
    /// <code lang="XAML">
    /// <![CDATA[
    /// <local:ViewModelLocator x:Key="Locator" d:IsDataSource="True" />
    /// ]]>
    /// </code>
    /// <para>
    /// This allows to access the view models in user controls:
    /// </para>
    /// <code lang="XAML">
    /// <![CDATA[
    /// DataContext="{Binding Source={StaticResource Locator}, Path=Main}"
    /// ]]>
    /// </code>
    /// </remarks>
    public class ViewModelLocator
    {
        public IWindowService WindowService { get; set; }


        public ViewModelLocator()
        {
            // Create service container.
            // ...

            if (WindowsHelper.IsInDesignMode)
            {
                // Create design time services.
                // ...
            }
            else
            {
                // Create run time services.
                // ...
            }

            // Register view models in service container.
            // ...
        }


        public MainWindowViewModel Main
        {
            get { return new MainWindowViewModel(WindowService); }
        }
    }
}
