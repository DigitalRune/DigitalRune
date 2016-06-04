using System;
using System.Windows;
using DigitalRune.Windows.Framework;


namespace ScreenConductionApp
{
    public class ViewLocator : IViewLocator
    {
        public FrameworkElement GetView(object viewModel, DependencyObject parent = null, object context = null)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (viewModel is MainWindowViewModel)
            {
                return new MainWindow();
            }
            if (viewModel is WindowViewModel)
            {
                // Show empty window.
                return null;
            }
            if (viewModel is DialogViewModel && "Window".Equals(context))
            {
                return new DialogView();
            }
            if (viewModel is DialogViewModel && "UserControl".Equals(context))
            {
                return new UserControlDialogView();
            }
            if (viewModel is SaveChangesViewModel)
            {
                return new SaveChangesView();
            }

            return null;
        }
    }
}
