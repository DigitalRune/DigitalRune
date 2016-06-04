using System;
using NUnit.Framework;


namespace DigitalRune.Tests
{
  public class ListenerWithEventArgs
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
  public class WeakEventHandlerGenericTest
  {
    public event EventHandler<EventArgs> Event;

		
		private void RaiseEvent()
		{
			EventHandler<EventArgs> handler = Event;

      if (handler != null)
			  handler(this, EventArgs.Empty);
		}


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ShouldThrowIfSenderIsNull()
    {
      var listener = new ListenerWithEventArgs();
      WeakEventHandler<EventArgs>.Register<WeakEventHandlerGenericTest, ListenerWithEventArgs>(
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
      var listener = new ListenerWithEventArgs();
      WeakEventHandler<EventArgs>.Register(
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
      var listener = new ListenerWithEventArgs();
      WeakEventHandler<EventArgs>.Register(
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
      WeakEventHandler<EventArgs>.Register<WeakEventHandlerGenericTest, ListenerWithEventArgs>(
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
      var listener = new ListenerWithEventArgs();
      WeakEventHandler<EventArgs>.Register(
        this,
        listener,
        (s, h) => s.Event += h, 
        (s, h) => s.Event -= h, 
        null);
    }


    [Test]
    public void HandleEvent()
    {
      var listener = new ListenerWithEventArgs();
      WeakEventHandler<EventArgs>.Register(
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
      var listener = new ListenerWithEventArgs();
      var subscription = WeakEventHandler<EventArgs>.Register(
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
      var listener = new ListenerWithEventArgs();
      WeakEventHandler<EventArgs>.Register(
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
