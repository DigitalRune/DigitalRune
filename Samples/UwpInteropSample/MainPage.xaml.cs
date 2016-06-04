namespace UwpInteropSample
{
  public sealed partial class MainPage
  {
    private MyGame _game;


    public MainPage()
    {
      InitializeComponent();

      // Create the MyGame instance, which implements the "game loop" and takes care of 3D rendering.
      _game = new MyGame();
    }
  }
}
