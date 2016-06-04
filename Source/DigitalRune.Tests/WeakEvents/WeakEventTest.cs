using System;
using NUnit.Framework;


namespace DigitalRune.Tests
{
  internal class MockEventSource
  {
    private readonly WeakEvent<EventHandler<EventArgs>> _myEvent = new WeakEvent<EventHandler<EventArgs>>();

    public event EventHandler<EventArgs> MyEvent
    {
      add { _myEvent.Add(value); }
      remove { _myEvent.Remove(value); }
    }

    protected virtual void OnMyEvent(EventArgs eventArgs)
    {
      _myEvent.Invoke(this, eventArgs);
    }  

    public virtual void RaiseMyEvent(EventArgs eventArgs)
    {
      OnMyEvent(eventArgs);
    }
  }


  // The dummy event handler needs to be a public method of a public type to work in Silverlight.
  public class MockEventHandler
  {
    public Action<object, EventArgs> Action;

    public void OnEvent(object sender, EventArgs eventArgs)
    {
      Action(sender, eventArgs);
    }
  }


  // The following event handlers does not work in Silverlight because the class is internal.
  internal class MockInternalEventHandler
  {
    public Action<object, EventArgs> Action;

    public void OnEvent(object sender, EventArgs eventArgs)
    {
      Action(sender, eventArgs);
    }
  }


  [TestFixture]
  public class WeakEventTest
  {
    [Test]
    [ExpectedException(typeof(TypeInitializationException))]
    public void StaticConstructorShouldThrowOnInvalidType()
    {
      var weakEvent = new WeakEvent<object>();
    }


    [Test]
    [ExpectedException(typeof(TypeInitializationException))]
    public void StaticConstructorShouldThrowOnInvalidEventHandler()
    {
      var weakEvent = new WeakEvent<Action<int, EventArgs>>();
    }


    [Test]
    [ExpectedException(typeof(TypeInitializationException))]
    public void StaticConstructorShouldThrowOnInvalidEventHandler2()
    {
      var weakEvent = new WeakEvent<Action<object, object>>();
    }


    [Test]
    [ExpectedException(typeof(TypeInitializationException))]
    public void StaticConstructorShouldThrowOnInvalidEventHandler3()
    {
      var weakEvent = new WeakEvent<Func<object, EventArgs, int>>();
    }


    [Test]
    public void AllowTypesCompatibleWithEventHandler()
    {
      var weakEvent = new WeakEvent<Action<object, EventArgs>>();
    }


    [Test]
    public void SingleEventHandler()
    {
      var eventSource = new MockEventSource();
      var eventHandler = new MockEventHandler();
      var eventArgs = new EventArgs();
      bool eventRaised = false;

      eventHandler.Action = (s, e) =>
      {
        Assert.AreSame(eventSource, s);
        Assert.AreSame(eventArgs, e);
        eventRaised = true;
      };

      eventSource.MyEvent += eventHandler.OnEvent;
      eventSource.RaiseMyEvent(eventArgs);
      Assert.IsTrue(eventRaised);

      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();

      eventRaised = false;
      eventSource.RaiseMyEvent(eventArgs);
      Assert.IsTrue(eventRaised);

      eventRaised = false;
      eventSource.MyEvent -= eventHandler.OnEvent;
      eventSource.RaiseMyEvent(eventArgs);
      Assert.IsFalse(eventRaised);
    }


    [Test]
    public void MultipleEventHandlers()
    {
      var eventSource = new MockEventSource();
      var eventHandler1 = new MockEventHandler();
      var eventHandler2 = new MockEventHandler();
      var eventArgs = new EventArgs();
      bool eventRaised1 = false;
      bool eventRaised2 = false;

      eventHandler1.Action = (s, e) =>
      {
        Assert.AreSame(eventSource, s);
        Assert.AreSame(eventArgs, e);
        eventRaised1 = true;
      };

      eventHandler2.Action = (s, e) =>
      {
        Assert.AreSame(eventSource, s);
        Assert.AreSame(eventArgs, e);
        eventRaised2 = true;
      };

      eventSource.MyEvent += eventHandler1.OnEvent;
      eventSource.MyEvent += eventHandler2.OnEvent;
      eventSource.RaiseMyEvent(eventArgs);
      Assert.IsTrue(eventRaised1);
      Assert.IsTrue(eventRaised2);

      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();

      eventRaised1 = false;
      eventRaised2 = false;
      eventSource.RaiseMyEvent(eventArgs);
      Assert.IsTrue(eventRaised1);
      Assert.IsTrue(eventRaised2);

      eventSource.MyEvent -= eventHandler1.OnEvent;
      eventRaised1 = false;
      eventRaised2 = false;
      eventSource.RaiseMyEvent(eventArgs);
      Assert.IsFalse(eventRaised1);
      Assert.IsTrue(eventRaised2);

      eventSource.MyEvent -= eventHandler2.OnEvent;
      eventRaised1 = false;
      eventRaised2 = false;
      eventSource.RaiseMyEvent(eventArgs);
      Assert.IsFalse(eventRaised1);
      Assert.IsFalse(eventRaised2);
    }


    [Test]
    public void RegisterAndUnregisterEventHandler()
    {      
      var eventSource = new MockEventSource();
      var eventHandler = new MockEventHandler();
      var eventArgs = new EventArgs();
      bool eventRaised = false;

      eventHandler.Action = (s, e) =>
      {
        Assert.AreSame(eventSource, s);
        Assert.AreSame(eventArgs, e);
        eventRaised = true;
      };

      eventSource.MyEvent += eventHandler.OnEvent;      
      eventSource.MyEvent -= eventHandler.OnEvent;
      eventSource.RaiseMyEvent(eventArgs);
      Assert.IsFalse(eventRaised);
    }


    [Test]
    public void EventShouldBeWeak()
    {
      var eventSource = new MockEventSource();
      var eventHandler = new MockEventHandler();
      var eventArgs = new EventArgs();
      bool eventRaised = false;

      eventHandler.Action = (s, e) =>
      {
        Assert.AreSame(eventSource, s);
        Assert.AreSame(eventArgs, e);
        eventRaised = true;
      };

      eventSource.MyEvent += eventHandler.OnEvent;
      eventSource.RaiseMyEvent(eventArgs);
      Assert.IsTrue(eventRaised);

      eventHandler = null;
      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();

      eventRaised = false;
      eventSource.RaiseMyEvent(eventArgs);
      Assert.IsFalse(eventRaised);
    }


    // The following test will raise a MethodAccessException in Silverlight because
    // the event handler needs to be public.
    [Test]
#if SILVERLIGHT
    [ExpectedException(typeof(MethodAccessException))]
#endif
    public void EventWithInternalEventHandler()
    {
      var eventSource = new MockEventSource();
      var eventHandler = new MockInternalEventHandler();
      var eventArgs = new EventArgs();
      bool eventRaised = false;

      eventHandler.Action = (s, e) =>
      {
        Assert.AreSame(eventSource, s);
        Assert.AreSame(eventArgs, e);
        eventRaised = true;
      };

      eventSource.MyEvent += eventHandler.OnEvent;
      eventSource.RaiseMyEvent(eventArgs);
      Assert.IsTrue(eventRaised);
    }
  }
}
