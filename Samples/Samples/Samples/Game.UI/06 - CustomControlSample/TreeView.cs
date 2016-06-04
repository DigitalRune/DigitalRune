using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;


namespace Samples.Game.UI
{
  public class TreeView : UIControl
  {
    // An internal stack panel is used to layout the Items.
    private StackPanel _panel;


    public NotifyingCollection<TreeViewItem> Items { get; private set; }

    public TreeViewItem SelectedItem { get; private set; }


    public TreeView()
    {
      Style = "TreeView";

      Items = new NotifyingCollection<TreeViewItem>(false, false);
      Items.CollectionChanged += (s, e) => OnItemsChanged();
    }


    private void OnItemsChanged()
    {
      // When Items is modified, rebuild the content of the stack panel.

      if (_panel == null)
        return;

      _panel.Children.Clear();

      foreach (var item in Items)
        _panel.Children.Add(item);

      InvalidateMeasure();
    }


    protected override void OnLoad()
    {
      base.OnLoad();

      if (_panel == null)
      {
        // Create and initialize the internal stack panel.
        _panel = new StackPanel();

        foreach (var item in Items)
          _panel.Children.Add(item);

        // The panel is the (only) visual child of the TreeView control.
        VisualChildren.Add(_panel);
      }
    }


    public void SelectItem(TreeViewItem item)
    {
      SelectedItem = item;

      // Update IsSelected flags of all TreeViewItems. 
      foreach (var descendant in UIHelper.GetDescendants(this).OfType<TreeViewItem>())
        descendant.IsSelected = (descendant == item);
    }
  }
}
