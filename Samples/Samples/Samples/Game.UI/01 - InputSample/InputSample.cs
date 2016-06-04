using Microsoft.Xna.Framework;


namespace Samples.Game.UI
{
  [Sample(SampleCategory.GameUI,
    @"This sample shows how to handle input from mouse, keyboard and gamepad.",
    @"The sample renders 3 rectangles. The rectangles can be moved and the color of the rectangles 
can be changed. The top rectangle will always handle input first. The lower rectangle will not
react to input if the top rectangle has already handled the input.",
    1)]
  [Controls(@"Sample
  Use <Left Mouse> or <Left Thumbstick> to move rectangle.
  Press <Left Shoulder>/<Right Shoulder> on gamepad to select other rectangle.
  Press <Space> on keyboard or <A> on gamepad to change color of the top rectangle.")]
  public class InputSample : BasicSample
  {
    public InputSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;

      // Add 3 simple game objects which draw rectangles and demonstrate input handling.
      GameObjectService.Objects.Add(new RectangleObject(Services));
      GameObjectService.Objects.Add(new RectangleObject(Services));
      GameObjectService.Objects.Add(new RectangleObject(Services));
    }


    public override void Update(GameTime gameTime)
    {
      GraphicsScreen.DebugRenderer2D.Clear();
    }
  }
}
