using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Graphics;


namespace InteropSample
{
  // A WPF window with two presentation targets into which 3D graphics can be rendered.
  // The window also handles some mouse events to demonstrate that these are working.
  public partial class WpfWindow
  {
    public IGraphicsService GraphicsService { get; set; }


    public WpfWindow()
    {
      InitializeComponent();
      Loaded += OnLoaded;
    }


    private void OnLoaded(object sender, RoutedEventArgs eventArgs)
    {
      // Register render targets.
      if (GraphicsService != null)
      {
        GraphicsService.PresentationTargets.Add(PresentationTarget0);
        GraphicsService.PresentationTargets.Add(PresentationTarget1);
      }
    }


    protected override void OnClosing(CancelEventArgs eventArgs)
    {
      // Unregister render targets.
      if (GraphicsService != null)
      {
        GraphicsService.PresentationTargets.Remove(PresentationTarget1);
        GraphicsService.PresentationTargets.Remove(PresentationTarget0);
      }

      base.OnClosing(eventArgs);
    }


    protected override void OnPreviewMouseDown(MouseButtonEventArgs eventArgs)
    {
      TextBox.Text += "Window.PreviewMouseDown\n";
      base.OnPreviewMouseDown(eventArgs);
    }


    protected override void OnMouseDown(MouseButtonEventArgs eventArgs)
    {
      TextBox.Text += "Window.MouseDown\n";
      base.OnMouseDown(eventArgs);
    }


    protected override void OnMouseWheel(MouseWheelEventArgs eventArgs)
    {
      TextBox.Text += "Window.MouseWheel\n";
      base.OnMouseWheel(eventArgs);
    }


    private void OnClearButtonClicked(object sender, RoutedEventArgs eventArgs)
    {
      TextBox.Clear();
    }
  }
}
