using System;
using System.Diagnostics;
using System.Windows.Forms;


namespace Samples
{
  internal static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      if (Debugger.IsAttached)
      {
        // The debugger is attached. The debugger will display any exception messages.

        // Run the XNA game.
        using (SampleGame game = new SampleGame())
          game.Run();
      }
      else
      {
        // The debugger is NOT attached. Use a MessageBox to display any exception messages.

        try
        {
          // Run the XNA game.
          using (SampleGame game = new SampleGame())
            game.Run();
        }
        catch (Exception exception)
        {
          MessageBox.Show(SampleHelper.GetExceptionMessage(exception), "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }
  }
}
