namespace WpfInteropSample2
{
  public partial class MainWindow
  {
    private MyGame _game;


    public MainWindow()
    {
      InitializeComponent();

      // Create the MyGame instance, which implements the "game loop" and takes care 
      // of 3D rendering.
      _game = new MyGame();

      // Load the "game level" consisting of some 3D models, lights, etc.
      TestLevel.Create();
    }
  }
}
