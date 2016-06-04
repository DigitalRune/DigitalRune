using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;


namespace DigitalRune.Game.Tests.GameObjects
{
  [TestFixture]
  public class GameObjectTest
  {
    private bool _wasCalled = false;   // True if an event handler was called. (Used by some test methods.)


    [SetUp]
    public void SetUp()
    {
      GameObject.PropertyMetadata.Clear();
      GameObject.NextPropertyId = 0;

      GameObject.EventMetadata.Clear();
      GameObject.NextEventId = 0;

      GamePropertyMetadata<float>.Properties.Clear();
      GamePropertyMetadata<string>.Properties.Clear();
      GamePropertyMetadata<bool>.Properties.Clear();

      GameEventMetadata<EventArgs>.Events.Clear();
      GameEventMetadata<GamePropertyEventArgs<float>>.Events.Clear();
    }


    [Test]
    public void CreateProperty()
    {
      Assert.AreEqual(0, GameObject.GetPropertyMetadata().Count());

      Assert.Throws(typeof(ArgumentNullException), () => GameObject.CreateProperty<float>(null, "", "", 0));
      Assert.Throws(typeof(ArgumentException), () => GameObject.CreateProperty<float>("", "", "", 0));

      GameObject.CreateProperty<float>("X", "Layout", "blah", 0f);
      GameObject.CreateProperty<string>("X", "Layout", "blah", string.Empty);
      GameObject.CreateProperty<string>("X", "Layout2", "XXX", string.Empty);     // Redefinition is ignored.
      GameObject.CreateProperty<bool>("IsEnabled", null, null, false);

      Assert.AreEqual(3, GameObject.GetPropertyMetadata().Count());
      Assert.IsTrue(GameObject.GetPropertyMetadata().Any(p => p.Name == "X"));
      Assert.IsTrue(GameObject.GetPropertyMetadata().Any(p => p.Name == "IsEnabled"));

      Assert.AreEqual("X", GameObject.GetPropertyMetadata<string>(1).Name);
      Assert.AreEqual("Layout", GameObject.GetPropertyMetadata<string>(1).Category);
      Assert.AreEqual("blah", GameObject.GetPropertyMetadata<string>(1).Description);
      Assert.AreEqual("", GameObject.GetPropertyMetadata<string>(1).DefaultValue);


      Assert.IsNull(GameObject.GetPropertyMetadata<float>(-1));
      Assert.IsNull(GameObject.GetPropertyMetadata<float>(100));
      Assert.IsNull(GameObject.GetPropertyMetadata<float>(null));
      Assert.IsNull(GameObject.GetPropertyMetadata<float>(""));
      Assert.IsNull(GameObject.GetPropertyMetadata<float>("Blah"));
    }


    [Test]
    public void Properties()
    {
      GameObject.CreateProperty<float>("X", "Layout", "blah", 0);
      GameObject.CreateProperty<string>("X", "Layout", "blah", string.Empty);
      GameObject.CreateProperty<bool>("IsEnabled", "Layout", "blah", false);

      var a = new GameObject("A");

      Assert.Throws(typeof(ArgumentException), () => a.GetValue<float>("Not defined"));

      Assert.AreEqual(0, a.GetValue<float>("X"));

      var p = a.Properties.Get<float>("X");
      Assert.AreEqual("X", p.Name);
      Assert.IsFalse(p.HasLocalValue);
      Assert.AreEqual(0, p.Value);

      p.Metadata.DefaultValue = 1;
      Assert.AreEqual(1, p.Value);

      p.Value = 2;
      Assert.AreEqual(2, p.Value);

      ((IGameProperty)p).Value = 3.0f;
      Assert.AreEqual(3, ((IGameProperty)p).Value);
      Assert.AreEqual(3, p.Value);
    }


    [Test]
    public void IGamePropertyMetadata()
    {
      var m = (IGamePropertyMetadata)GameObject.CreateProperty<float>("X", "Layout", "blah", 0);

      Assert.AreEqual(0, m.DefaultValue);

      m.DefaultValue = 10f;
      Assert.AreEqual(10, m.DefaultValue);
    }


    [Test]
    public void AddRemoveProperty()
    {
      GameObject.CreateProperty<float>("X", "Layout", "blah", 0);
      GameObject.CreateProperty<string>("X", "Layout", "blah", string.Empty);
      GameObject.CreateProperty<bool>("IsEnabled", "Layout", "blah", false);

      var a = new GameObject();

      Assert.AreEqual(0, a.Properties.Count());

      Assert.Throws(typeof(ArgumentException), () => a.Properties.Add<bool>("Not defined"));

      a.Properties.Add<string>("X");
      a.Properties.Add<string>("X");
      
      Assert.AreEqual(1, a.Properties.Count());

      a.Properties.Add<bool>(2);

      Assert.AreEqual(2, a.Properties.Count());

      a.Properties.Remove<string>("X");

      Assert.AreEqual(1, a.Properties.Count());
    }


    [Test]
    public void Template()
    {
      GameObject.CreateProperty<float>("X", "Layout", "blah", 0);
      GameObject.CreateProperty<float>("Y", "Layout", "blah", 0);
      GameObject.CreateProperty<float>("Z", "Layout", "blah", 0);
      GameObject.CreateProperty<float>("W", "Layout", "blah", 0);

      var t = new GameObject();

      t.SetValue("X", 10.0f);
      t.SetValue("Y", 20.0f);
      t.SetValue("Z", 30.0f);

      var a = new GameObject();

      Assert.AreEqual(0, a.GetValue<float>("X"));

      a.Template = t;

      Assert.AreEqual(10, a.GetValue<float>("X"));
      Assert.AreEqual(20, a.GetValue<float>("Y"));
      Assert.AreEqual(30, a.GetValue<float>("Z"));

      Assert.IsFalse(a.Properties.Get<float>("X").HasLocalValue);
      Assert.IsFalse(a.Properties.Get<float>("Y").HasLocalValue);
      Assert.IsFalse(a.Properties.Get<float>("Z").HasLocalValue);

      a.SetValue("Y", 999.0f);

      Assert.AreEqual(10, a.GetValue<float>("X"));
      Assert.AreEqual(999, a.GetValue<float>("Y"));
      Assert.AreEqual(30, a.GetValue<float>("Z"));

      Assert.IsFalse(a.Properties.Get<float>("X").HasLocalValue);
      Assert.IsTrue(a.Properties.Get<float>("Y").HasLocalValue);
      Assert.IsFalse(a.Properties.Get<float>("Z").HasLocalValue);

      a.Properties.Get<float>("Y").Reset();

      Assert.AreEqual(10, a.GetValue<float>("X"));
      Assert.AreEqual(20, a.GetValue<float>("Y"));
      Assert.AreEqual(30, a.GetValue<float>("Z"));

      Assert.IsFalse(a.Properties.Get<float>("X").HasLocalValue);
      Assert.IsFalse(a.Properties.Get<float>("Y").HasLocalValue);
      Assert.IsFalse(a.Properties.Get<float>("Z").HasLocalValue);

      // Enumerate properties.
      Assert.AreEqual(3, ((IEnumerable)a.Properties).Cast<IGameProperty>().Count());

      _wasCalled = false;
      a.TemplateChanged += (s, e) => _wasCalled = true;

      a.Template = t;
      Assert.IsFalse(_wasCalled);

      a.Template = null;
      Assert.IsTrue(_wasCalled);
    }
    

    [Test]
    public void NotifyPropertyChanged()
    {
      GameObject.CreateProperty<float>("X", "Layout", "blah", 0);

      var a = new GameObject();

      ((INotifyPropertyChanged)a).PropertyChanged += OnPropertyChanged;
      ((INotifyPropertyChanged)a).PropertyChanged += (s, e) => Assert.AreEqual(a, s);
      ((INotifyPropertyChanged)a).PropertyChanged += (s, e) => Assert.AreEqual("X", e.PropertyName);

      a.SetValue("X", 11f);

      Assert.IsTrue(_wasCalled);

      // Test de-registering event handler
      _wasCalled = false;
      ((INotifyPropertyChanged)a).PropertyChanged -= OnPropertyChanged;

      a.SetValue("X", 12f);

      Assert.IsFalse(_wasCalled);
    }


    private void OnPropertyChanged(object sender, EventArgs e)
    {
      _wasCalled = true;
    }


    [Test]
    public void PropertyChanged()
    {
      GameObject.CreateProperty<float>("X", "Layout", "blah", 1);

      var a = new GameObject();
      var p = a.Properties.Get<float>("X");

      _wasCalled = false;
      float oldValue = 1;
      float newValue = 11;
      p.Changed += OnPropertyChanged;
      p.Changed += (s, e) => Assert.AreEqual(a, s);
      p.Changed += (s, e) => Assert.AreEqual(p, e.Property);
      p.Changed += (s, e) => Assert.AreEqual(oldValue, e.OldValue);
      p.Changed += (s, e) => Assert.AreEqual(newValue, e.NewValue);
      p.Changed += (s, e) => Assert.AreEqual(newValue, e.CoercedValue);

      a.SetValue("X", 11f);

      Assert.IsTrue(_wasCalled);

      // Setting an equal value does not trigger event.
      _wasCalled = false;
      oldValue = 11;
      a.SetValue("X", 11f);
      Assert.IsFalse(_wasCalled);

      // Test de-registration of event handler.
      _wasCalled = false;
      p.Changed -= OnPropertyChanged;
      newValue = 5;
      p.Value = 5;
      Assert.IsFalse(_wasCalled);
    }


    [Test]
    public void PropertyChanging()
    {
      GameObject.CreateProperty<float>("X", "Layout", "blah", 1);

      var a = new GameObject();
      var p = a.Properties.Get<float>("X");

      _wasCalled = false;
      float oldValue = 1;
      float newValue = 11;
      p.Changing += OnPropertyChanged;
      p.Changing += (s, e) => e.CoercedValue = 10;
      p.Changing += (s, e) => Assert.AreEqual(a, s);
      p.Changing += (s, e) => Assert.AreEqual(p, e.Property);
      p.Changing += (s, e) => Assert.AreEqual(oldValue, e.OldValue);
      p.Changing += (s, e) => Assert.AreEqual(newValue, e.NewValue);
      p.Changing += (s, e) => Assert.LessOrEqual(e.CoercedValue, 10);

      a.SetValue("X", 11f);

      Assert.IsTrue(_wasCalled);
      Assert.AreEqual(10, p.Value);

      // Test de-registration of event handler.
      _wasCalled = false;
      p.Changing -= OnPropertyChanged;
      oldValue = 10;
      newValue = 5;
      p.Value = 5;
      Assert.IsFalse(_wasCalled);
    }


    [Test]
    public void PropertyConnection()
    {
      GameObject.CreateProperty<float>("X", "Layout", "blah", 1);
      GameObject.CreateProperty<float>("Y", "Layout", "blah", 1);

      var a = new GameObject();
      var b = new GameObject();

      var aX = a.Properties.Get<float>("X");
      var bY = b.Properties.Get<float>("Y");
      aX.Changed += bY.Change;

      aX.Value = 22;
      Assert.AreEqual(22, b.GetValue<float>("Y"));

      aX.Changed -= bY.Change;
      aX.Value = 23;
      Assert.AreEqual(22, b.GetValue<float>("Y"));
    }


    [Test]
    public void PropertyParse()
    {
      GameObject.CreateProperty<float>("X", "Layout", "blah", 1);

      var a = new GameObject();
      var p = a.Properties.Get<float>(0);

      p.Parse("1e5");
      Assert.AreEqual(1e5f, p.Value);
    }


    [Test]
    public void EventMetadata()
    {
      Assert.AreEqual(0, GameObject.GetEventMetadata().Count());

      Assert.Throws(typeof(ArgumentNullException), () => GameObject.CreateEvent(null, null, null, EventArgs.Empty));
      Assert.Throws(typeof(ArgumentException), () => GameObject.CreateEvent("", null, null, EventArgs.Empty));

      GameObject.CreateEvent<GamePropertyEventArgs<float>>("PropertyChanged", null, null, null);
      GameObject.CreateEvent("Click", "Interaction", "blah", EventArgs.Empty);

      // Duplicates are ignored
      GameObject.CreateEvent("Click", "-", "-", EventArgs.Empty);
      
      Assert.AreEqual(2, GameObject.GetEventMetadata().Count());

      var m = GameObject.GetEventMetadata<EventArgs>(1);
      var m2 = GameObject.GetEventMetadata<EventArgs>("Click");

      Assert.AreEqual(m, m2);
      Assert.AreEqual(1, m.Id);
      Assert.AreEqual("Click", m.Name);
      Assert.AreEqual("Interaction", m.Category);
      Assert.AreEqual("blah", m.Description);
      Assert.AreEqual(EventArgs.Empty, m.DefaultEventArgs);
    }


    [Test]
    public void EventCollection()
    {
      GameObject.CreateEvent<GamePropertyEventArgs<float>>("PropertyChanged", null, null, null);
      GameObject.CreateEvent("Click", "Interaction", "blah", EventArgs.Empty);
      var c2 = GameObject.CreateEvent("Click2", "Interaction", "blah", EventArgs.Empty);
      GameObject.CreateEvent("Click3", "Interaction", "blah", EventArgs.Empty);

      var a = new GameObject();

      a.Events.Add(c2);
      a.Events.Add<GamePropertyEventArgs<float>>(0);
      a.Events.Add<EventArgs>("Click");

      Assert.AreEqual(3, a.Events.Count());

      Assert.AreEqual("Click", a.Events.Get<EventArgs>(1).Name);
      Assert.AreEqual("Click", a.Events.Get<EventArgs>("Click").Name);
      Assert.AreEqual("Click2", a.Events.Get<EventArgs>(c2).Name);

      a.Events.Remove(1);
      Assert.AreEqual(2, a.Events.Count());

      a.Events.Remove(c2);
      Assert.AreEqual(1, a.Events.Count());

      a.Events.Remove<GamePropertyEventArgs<float>>("PropertyChanged");
      Assert.AreEqual(0, a.Events.Count());
    }


    [Test]
    public void Events()
    {
      GameObject.CreateEvent<GamePropertyEventArgs<float>>("PropertyChanged", null, null, null);
      GameObject.CreateEvent("Click", "Interaction", "blah", EventArgs.Empty);
      var c2 = GameObject.CreateEvent("Click2", "Interaction", "blah", EventArgs.Empty);
      GameObject.CreateEvent("Click3", "Interaction", "blah", EventArgs.Empty);

      var a = new GameObject();

      var e = a.Events.Get<EventArgs>("Click");

      e.Event += OnPropertyChanged;
      e.Event += (s, args) => Assert.AreEqual(a, s);

      _wasCalled = false;
      
      e.Raise();
      Assert.IsTrue(_wasCalled);

      e.Event -= OnPropertyChanged;
      _wasCalled = false;
      e.Raise();
      Assert.IsFalse(_wasCalled);
    }


    [Test]
    public void EventConnection()
    {
      GameObject.CreateProperty("MyBoolean", null, null, false);
      GameObject.CreateEvent("MyEvent", null, null, EventArgs.Empty);

      var source = new GameObject("Source");
      var gameProperty = source.Properties.Get<bool>("MyBoolean");

      var listener = new GameObject("Listener");
      var gameEvent = listener.Events.Get<EventArgs>("MyEvent");

      bool wasCalled = false;
      gameEvent.Event += (s, e) =>
                         {
                           wasCalled = true;
                           Assert.AreEqual(listener, s);
                           Assert.That(e, Is.AssignableTo<EventArgs>());
                         };

      gameProperty.Changed += new EventHandler<GamePropertyEventArgs<bool>>(gameEvent.RaiseOnEvent);
      gameProperty.Value = true;
      Assert.IsTrue(wasCalled);
      
      wasCalled = false;
      gameProperty.Value = false;
      Assert.IsTrue(wasCalled);

      gameProperty.Changed -= new EventHandler<GamePropertyEventArgs<bool>>(gameEvent.RaiseOnEvent);
      wasCalled = false;
      gameProperty.Value = true;
      Assert.IsFalse(wasCalled);
    }
  }
}
