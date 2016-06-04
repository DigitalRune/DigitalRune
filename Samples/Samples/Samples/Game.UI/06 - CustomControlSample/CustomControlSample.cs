#if WINDOWS
using System.Windows.Forms;
#endif
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using MouseButtons = DigitalRune.Game.Input.MouseButtons;


namespace Samples.Game.UI
{
  [Sample(SampleCategory.GameUI,
    @"This sample shows how to create a TreeView control.",
    @"When double-click on a tree view item is detected, a message box is shown.

The DigitalRune Blog contains a detailed description for this project; see section
'Creating a Tree View Control'.",
    6)]
  public class CustomControlSample : Sample
  {
    private readonly UIScreen _uiScreen;

    // The tree view item that was selected in the last frame.
    private TreeViewItem _lastSelectedItem;


    public CustomControlSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add a DelegateGraphicsScreen as the first graphics screen to the graphics
      // service. This lets us do the rendering in the Render method of this class.
      var graphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, graphicsScreen);

      // Create a theme. A theme defines the images of the controls and the default values of 
      // the UI control properties.
      Theme theme = ContentManager.Load<Theme>("UI Themes/BlendBlue/Theme");

      // Create a renderer that uses the theme information. We do not use the default UIRenderer
      // class, instead we use our own MyUIRenderer that adds rendering of tree view items.
      var renderer = new MyUIRenderer(Game, theme);

      // Create the screen. A screen is the root of the UI control hierarchy.
      _uiScreen = new UIScreen("SampleUIScreen", renderer);

      // Screens must be added to the UI service to be updated each frame.
      UIService.Screens.Add(_uiScreen);

      // Add a tree view control to the screen.
      var padding = new Vector4F(0, 2, 4, 2);
      var treeView = new TreeView
      {
        X = 10,
        Y = 60,
        Items =
          {
            new TreeViewItem
            {
              Header = new TextBlock { Text = "Item 1", Padding = padding, },
              UserData = "1",
            },
            new TreeViewItem
            {
              Header = new TextBlock { Text = "Item 2", Padding = padding, },
              UserData = "2",
              Items =
              {
                new TreeViewItem
                {
                  Header = new TextBlock { Text = "Item 2.1", Padding = padding, },
                  UserData = "2.1",
                },
                new TreeViewItem
                {
                  Header = new TextBlock { Text = "Item 2.2", Padding = padding, },
                  UserData = "2.2",
                  Items =
                  {
                    new TreeViewItem
                    {
                      Header = new TextBlock { Text = "Item 2.2.1", Padding = padding, },
                      UserData = "2.2.1",
                    },
                    new TreeViewItem
                    {
                      Header = new TextBlock { Text = "Item 2.2.2", Padding = padding, },
                      UserData = "2.2.2",
                    },
                  }
                },
                new TreeViewItem
                {
                  Header = new TextBlock { Text = "Item 2.3", Padding = padding, },
                  UserData = "2.3",
                },
              }
            },
            new TreeViewItem
            {
              Header = new TextBlock { Text = "Item 3 (The quick brown fox jumps over the lazy dog.)", Padding = padding, },
              UserData = "3",
              Items =
              {
                new TreeViewItem
                {
                  Header = new TextBlock { Text = "Item 3.1", Padding = padding, },
                  UserData = "3.1",
                },
                new TreeViewItem
                {
                  Header = new TextBlock { Text = "Item 3.2", Padding = padding, },
                  UserData = "3.2",
                },
                new TreeViewItem
                {
                  Header = new Image { Texture = theme.Textures["UITexture"].Texture, SourceRectangle = new Rectangle(214, 66, 16, 16), Padding = padding, },
                  UserData = "3.3",
                },
                new TreeViewItem
                {
                  Header = new TextBlock { Text = "Item 3.4", Padding = padding, },
                  UserData = "3.4",
                },
              }
            },
          }
      };
      _uiScreen.Children.Add(treeView);

      // Handle InputProcessed events.
      treeView.InputProcessed += OnTreeViewInputProcessed;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Remove UIScreen from UI service.
        UIService.Screens.Remove(_uiScreen);
      }

      base.Dispose(disposing);
    }


    // Called after the tree view control has processed device input.
    private void OnTreeViewInputProcessed(object sender, InputEventArgs eventArgs)
    {
      var treeView = (TreeView)sender;
      var selectedItem = treeView.SelectedItem;
      var inputService = treeView.InputService;

      if (!inputService.IsMouseOrTouchHandled               // Mouse was not handled before?
          && inputService.IsDoubleClick(MouseButtons.Left)  // Double-click detected?
          && selectedItem == _lastSelectedItem              // Both clicks were on the same item?
          && selectedItem != null                           // Any item is selected?
          && selectedItem.IsMouseOver)                      // The mouse is over the selected item?
      {
        // A tree view item was double-clicked. Show a message box with the data of the item.
        // This is only for debugging, so we simply use the Windows Forms MessageBox.
#if WINDOWS
        MessageBox.Show((string)selectedItem.UserData);
#endif
      }

      // Remember the selected item. This is necessary because we must be able to check if both
      // clicks of the double-click were on the same item. When a tree view item is clicked,
      // it is selected. Clicking two different items in rapid succession should not count as
      // a double-click.
      _lastSelectedItem = selectedItem;
    }


    private void Render(RenderContext context)
    {
      _uiScreen.Draw(context.DeltaTime);
    }
  }
}
