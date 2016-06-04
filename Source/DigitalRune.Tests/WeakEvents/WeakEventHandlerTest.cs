using System;
using NUnit.Framework;


namespace DigitalRune.Tests
{
  public class Listener
  {
    public bool EventReceived { get; private set; }

    public void OnEvent(object sender, EventArgs eventArgs)
    {
      EventReceived = true;
    }

    public void Reset()
    {
      EventReceived = false;
    }
  }


  [TestFixture]
  public class WeakEventHandlerTest
  {
    public event EventHandler Event;


    private void RaiseEvent()
    {
      EventHandler handler = Event;

      if (handler != null)
        handler(this, EventArgs.Empty);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowIfSenderIsNull()
    {
      var listener = new Listener();
      WeakEventHandler.Register<WeakEventHandlerTest, Listener>(
        null,
        listener,
        (s, h) => s.Event += h, 
        (s, h) => s.Event -= h, 
        (l, s, e) => l.OnEvent(s, e));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowIfAddHandlerIsNull()
    {
      var listener = new Listener();
      WeakEventHandler.Register(
        this,
        listener,
        null, 
        (s, h) => s.Event -= h, 
        (l, s, e) => l.OnEvent(s, e));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowIfRemoveHandlerIsNull()
    {
      var listener = new Listener();
      WeakEventHandler.Register(
        this,
        listener,
        (s, h) => s.Event += h, 
        null, 
        (l, s, e) => l.OnEvent(s, e));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowIfListenerIsNull()
    {
      WeakEventHandler.Register<WeakEventHandlerTest, Listener>(
        this,
        null,
        (s, h) => s.Event += h, 
        (s, h) => s.Event -= h, 
        (l, s, e) => l.OnEvent(s, e));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowIfForwardEventIsNull()
    {
      var listener = new Listener();
      WeakEventHandler.Register(
        this,
        listener,
        (s, h) => s.Event += h, 
        (s, h) => s.Event -= h, 
        null);
    }


    [Test]
    public void HandleEvent()
    {
      var listener = new Listener();
      WeakEventHandler.Register(
        this,
        listener,
        (s, h) => s.Event += h, 
        (s, h) => s.Event -= h, 
        (l, s, e) => l.OnEvent(s, e));

      RaiseEvent();
      Assert.IsTrue(listener.EventReceived);
    }


    [Test]
    public void DetachEventHandler()
    {
      var listener = new Listener();
      var subscription = WeakEventHandler.Register(
        this,
        listener,
        (s, h) => s.Event += h, 
        (s, h) => s.Event -= h, 
        (l, s, e) => l.OnEvent(s, e));

      subscription.Dispose();
      RaiseEvent();
      Assert.IsFalse(listener.EventReceived);
    }


    [Test]
    public void EventHandlerShouldBeWeak()
    {
      bool eventReceived = false;
      var listener = new Listener();
      WeakEventHandler.Register(
        this,
        listener,
        (s, h) => s.Event += h, 
        (s, h) => s.Event -= h, 
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
