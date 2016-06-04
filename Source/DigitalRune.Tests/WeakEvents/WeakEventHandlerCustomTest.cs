using System;
using System.ComponentModel;
using NUnit.Framework;


namespace DigitalRune.Tests
{
  public class PropertyChangedListener
  {
    public bool EventReceived { get; private set; }

    public void OnEvent(object sender, PropertyChangedEventArgs eventArgs)
    {
      EventReceived = true;
    }

    public void Reset()
    {
      EventReceived = false;
    }
  }


  [TestFixture]
  public class WeakEventHandlerCustomTest : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;


    private void RaiseEvent()
    {
      PropertyChangedEventHandler handler = PropertyChanged;

      if (handler != null)
        handler(this, new PropertyChangedEventArgs(String.Empty));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowIfSenderIsNull()
    {
      var listener = new PropertyChangedListener();
      WeakEventHandler<PropertyChangedEventHandler, PropertyChangedEventArgs>.Register<WeakEventHandlerCustomTest, PropertyChangedListener>(
        null,
        listener,
        handler => new PropertyChangedEventHandler(handler), 
        (s, h) => s.PropertyChanged += h,
        (s, h) => s.PropertyChanged -= h,
        (l, s, e) => l.OnEvent(s, e));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowIfConversionNull()
    {
      var listener = new PropertyChangedListener();
      WeakEventHandler<PropertyChangedEventHandler, PropertyChangedEventArgs>.Register(
        this,
        listener,
        null,
        (s, h) => s.PropertyChanged += h,
        (s, h) => s.PropertyChanged -= h,
        (l, s, e) => l.OnEvent(s, e));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowIfAddHandlerIsNull()
    {
      var listener = new PropertyChangedListener();
      WeakEventHandler<PropertyChangedEventHandler, PropertyChangedEventArgs>.Register(
        this,
        listener,
        handler => new PropertyChangedEventHandler(handler),
        null,
        (s, h) => s.PropertyChanged -= h,
        (l, s, e) => l.OnEvent(s, e));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowIfRemoveHandlerIsNull()
    {
      var listener = new PropertyChangedListener();
      WeakEventHandler<PropertyChangedEventHandler, PropertyChangedEventArgs>.Register(
        this,
        listener,
        handler => new PropertyChangedEventHandler(handler),
        (s, h) => s.PropertyChanged += h,
        null,
        (l, s, e) => l.OnEvent(s, e));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowIfListenerIsNull()
    {
      WeakEventHandler<PropertyChangedEventHandler, PropertyChangedEventArgs>.Register<WeakEventHandlerCustomTest, PropertyChangedListener>(
        this,
        null,
        handler => new PropertyChangedEventHandler(handler),
        (s, h) => s.PropertyChanged += h,
        (s, h) => s.PropertyChanged -= h,
        (l, s, e) => l.OnEvent(s, e));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowIfForwardEventIsNull()
    {
      var listener = new PropertyChangedListener();
      WeakEventHandler<PropertyChangedEventHandler, PropertyChangedEventArgs>.Register(
        this,
        listener,
        handler => new PropertyChangedEventHandler(handler),
        (s, h) => s.PropertyChanged += h,
        (s, h) => s.PropertyChanged -= h,
        null);
    }


    [Test]
    public void HandleEvent()
    {
      var listener = new PropertyChangedListener();
      WeakEventHandler<PropertyChangedEventHandler, PropertyChangedEventArgs>.Register(
        this,
        listener,
        handler => new PropertyChangedEventHandler(handler),
        (s, h) => s.PropertyChanged += h,
        (s, h) => s.PropertyChanged -= h,
        (l, s, e) => l.OnEvent(s, e));

      RaiseEvent();
      Assert.IsTrue(listener.EventReceived);
    }


    [Test]
    public void DetachEventHandler()
    {
      var listener = new PropertyChangedListener();
      var subscription = WeakEventHandler<PropertyChangedEventHandler, PropertyChangedEventArgs>.Register(
        this,
        listener,
        handler => new PropertyChangedEventHandler(handler),
        (s, h) => s.PropertyChanged += h,
        (s, h) => s.PropertyChanged -= h,
        (l, s, e) => l.OnEvent(s, e));

      subscription.Dispose();
      RaiseEvent();
      Assert.IsFalse(listener.EventReceived);
    }


    [Test]
    public void EventHandlerShouldBeWeak()
    {
      bool eventReceived = false;
      var listener = new PropertyChangedListener();
      WeakEventHandler<PropertyChangedEventHandler, PropertyChangedEventArgs>.Register(
        this,
        listener,
        handler => new PropertyChangedEventHandler(handler),
        (s, h) => s.PropertyChanged += h,
        (s, h) => s.PropertyChanged -= h,
        (l, s, e) => { l.OnEvent(s, e); eventReceived = true; });

      RaiseEvent();
      Assert.IsTrue(listener.EventReceived);
      Assert.IsTrue(eventReceived);

      listener.Reset();
      eventReceived = false;
      listener = null;
      GC.Collect();

      RaiseEvent();
      Assert.IsFalse(eventReceived);
    }
  }
}
