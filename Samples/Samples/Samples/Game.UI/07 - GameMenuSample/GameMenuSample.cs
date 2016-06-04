#if !WP7 && !WP8
using DigitalRune.Game.UI;
using Microsoft.Xna.Framework;


namespace Samples.Game.UI
{
  [Sample(SampleCategory.GameUI,
    @"This sample shows how to create menus that can be used with the mouse and the gamepad.",
    @"You need a gamepad for this sample.
This sample shows
- how to switch between standard game parts (start screen, main menu, game screen with menu),
- how to handle gamepad disconnects,
- how to create a main menu,
- how to create an options dialog,
- how to create a pause menu,
- how to add a debugging screen,
- how to use a debug console with a custom command. 
  (Open console with TAB key or ChatPadGreen button.)

Note: The GameMenuSample and the GameStatesSample solve a similar problem. The GameMenuSample uses
GameComponents to represents the game states. The GameStatesSample uses a single GameComponent
with a StateMachine.",
    7)]
  [Controls(@"Sample
  Press <Tab> or <ChatPadGreen> to display debug console.")]
  public class GameMenuSample : Sample
  {
    public GameMenuSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Sample requires gamepad.
      SampleFramework.IsMouseVisible = false;

      // Add a game component that shows a debug console when a special key is pressed.
      Game.Components.Add(new DebuggingComponent(game, Services));

      // Add the start screen. The start screen will load main menu component.
      Game.Components.Add(new StartScreenComponent(game, Services));
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Remove the game components of this sample:
        for (int i = Game.Components.Count - 1; i >= 0; i--)
        {
          var component = Game.Components[i];
          if (component is DebuggingComponent
              || component is StartScreenComponent
              || component is MainMenuComponent
              || component is MyGameComponent)
          {
            Game.Components.RemoveAt(i);
            ((GameComponent)component).Dispose();
          }
        }

        // Remove UI screen added by StartScreenComponent.
        UIService.Screens.Remove("SampleUI");
      }

      base.Dispose(disposing);
    }
  }
}
#endif