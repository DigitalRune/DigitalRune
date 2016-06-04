using System;
using System.Diagnostics;


namespace Samples
{
  internal static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
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
        // The debugger is NOT attached. Use the ExceptionGame to display any 
        // exception messages. (This is the only method to display the message on 
        // the Xbox. On Windows we could use a Windows MessageBox.)

        try
        {
          // Run the XNA game.
          using (SampleGame game = new SampleGame())
            game.Run();
        }
        catch (Exception exception)
        {
          using (var game = new ExceptionGame())
          {
            game.Exception = exception;
            game.Run();
          }
        }
      }
    }
  }
}
