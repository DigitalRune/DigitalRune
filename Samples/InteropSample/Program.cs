using System;


namespace InteropSample
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]         // <-- Required for WPF interop!
    static void Main(string[] args)
    {
      using (Game1 game = new Game1())
      {
        game.Run();
      }
    }
  }
}
