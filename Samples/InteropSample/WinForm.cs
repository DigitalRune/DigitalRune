using System;
using System.ComponentModel;
using System.Windows.Forms;
using DigitalRune.Graphics;


namespace InteropSample
{
  // A Windows Forms window with two presentation targets into which XNA graphics 
  // can be rendered.
  public partial class WinForm : Form
  {
    public IGraphicsService GraphicsServices { get; set; }


    public WinForm()
    {      
      InitializeComponent();
    }


    protected override void OnLoad(EventArgs eventArgs)
    {
      base.OnLoad(eventArgs);

      // Register presentation targets.
      if (GraphicsServices != null)
      {
        GraphicsServices.PresentationTargets.Add(PresentationTarget0);
        GraphicsServices.PresentationTargets.Add(PresentationTarget1);
      }
    }


    protected override void OnClosing(CancelEventArgs eventArgs)
    {
      // Unregister presentation targets.
      if (GraphicsServices != null)
      {
        GraphicsServices.PresentationTargets.Remove(PresentationTarget1);
        GraphicsServices.PresentationTargets.Remove(PresentationTarget0);        
      }

      base.OnClosing(eventArgs);
    }


    private void OnClearButtonClick(object sender, EventArgs eventArgs)
    {
      TextBox.Clear();
    }
  }
}
