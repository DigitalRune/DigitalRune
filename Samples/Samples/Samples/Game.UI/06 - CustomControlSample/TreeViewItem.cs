using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;


namespace Samples.Game.UI
{
  // A single item/node of a tree view.
  public class TreeViewItem : UIControl
  {
    // An internal stack panel is used to layout the child items.
    private StackPanel _panel;


    public override string VisualState
    {
      get
      {
        if (Items.Count == 0)
          return "Default";

        if (IsExpanded)
          return "Expanded";

        return "Collapsed";
      }
    }


    // The "label" of this item.
    public UIControl Header
    {
      get { return _header; }
      set
      {
        if (_header == value)
          return;

        _header = value;
        OnItemsChanged();
      }
    }
    private UIControl _header;


    // The child items.
    public NotifyingCollection<TreeViewItem> Items { get; private set; }


    // The TreeView control can be found by searching the visual tree.
    public TreeView TreeView
    {
      get { return UIHelper.GetAncestors(this).OfType<TreeView>().FirstOrDefault(); }
    }


    public static readonly int IsExpandedPropertyId = CreateProperty(
      typeof(TreeViewItem), "IsExpanded", GamePropertyCategories.Default, null, true,
      UIPropertyOptions.AffectsMeasure);

    public bool IsExpanded
    {
      get { return GetValue<bool>(IsExpandedPropertyId); }
      set { SetValue(IsExpandedPropertyId, value); }
    }


    public static readonly int IsSelectedPropertyId = CreateProperty(
      typeof(TreeViewItem), "IsSelected", GamePropertyCategories.Default, null, false,
      UIPropertyOptions.AffectsRender);

    public bool IsSelected
    {
      get { return GetValue<bool>(IsSelectedPropertyId); }
      set { SetValue(IsSelectedPropertyId, value); }
    }


    public TreeViewItem()
    {
      Style = "TreeViewItem";

      Items = new NotifyingCollection<TreeViewItem>(false, false);
      Items.CollectionChanged += (s, e) => OnItemsChanged();

      // Uncomment this to add a random background color for debugging.
      //Vector3F color = RandomHelper.Random.NextVector3F(0, 1);
      //Background = new Color(color.X, color.Y, color.Z, 0.4f);

      var isExpandedProperty = Properties.Get<bool>(IsExpandedPropertyId);
      isExpandedProperty.Changed += OnExpandedChanged;
    }


    private void OnItemsChanged()
    {
      // When Items is modified, rebuild the content of the stack panel.

      if (_panel == null)
        return;

      _panel.Children.Clear();

      _panel.Children.Add(Header);
      foreach (var item in Items)
        _panel.Children.Add(item);

      InvalidateMeasure();
    }


    protected override void OnLoad()
    {
      base.OnLoad();

      if (_panel == null)
      {
        // Create the internal stack panel.
        // The padding of this tree view item is used as the margin of the panel.
        _panel = new StackPanel
        {
          Margin = Padding,
        };

        // Whenever the padding is changed, the panel margin should be updated.
        var panelMargin = _panel.Properties.Get<Vector4F>(UIControl.MarginPropertyId);
        var padding = this.Properties.Get<Vector4F>(UIControl.PaddingPropertyId);
        padding.Changed += panelMargin.Change;

        // Add Items to the panel.
        _panel.Children.Add(Header);
        foreach (var item in Items)
          _panel.Children.Add(item);

        // The panel is the (only) visual child of the TreeView control.
        VisualChildren.Add(_panel);
      }
    }


    // Called when this control should handle input.
    protected override void OnHandleInput(InputContext context)
    {
      // Call base class. This will automatically handle the input of the visual children.
      base.OnHandleInput(context);

      if (!InputService.IsMouseOrTouchHandled && InputService.IsPressed(MouseButtons.Left, false))
      {
        // The mouse is not handled and the left button is pressed.

        var headerHeight = Header != null ? Header.ActualHeight : 0;
        if (context.MousePosition.X > ActualX && context.MousePosition.X < ActualX + Padding.X
            && context.MousePosition.Y > ActualY && context.MousePosition.Y < ActualY + headerHeight)
        {
          // The area left of the label was clicked. This is the area where a Expand/Collapse
          // icon is drawn. --> Switch between expanded and collapsed state.
          IsExpanded = !IsExpanded;

          InputService.IsMouseOrTouchHandled = true;
        }
        else
        {
          // If the mouse was over the Header, this control should be selected.
          if (Header != null && Header.IsMouseOver && TreeView != null)
            TreeView.SelectItem(this);
        }
      }
    }


    private void OnExpandedChanged(object sender, GamePropertyEventArgs<bool> eventArgs)
    {
      // Toggle the visibility of all child Items.
      foreach (var item in Items)
        item.IsVisible = eventArgs.NewValue;
    }
  }
}
