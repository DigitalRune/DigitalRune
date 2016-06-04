using System;
using NUnit.Framework;


namespace DigitalRune.Tests
{
  [TestFixture]
  public class WeakDelegateTest
  {
    [Test]
    [ExpectedException(typeof(TypeInitializationException))]
    public void StaticConstructorShouldThrowExceptionOnNonDelegate()
    {
      WeakDelegate<object> weakDelegate = new WeakDelegate<object>(new object());
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorShouldThrowExceptionOnNull()
    {
      WeakDelegate<Action> weakDelegate = new WeakDelegate<Action>(null);
    }


    //[Test]
    //[ExpectedException(typeof(ArgumentException))]
    //public void ConstructorShouldThrowExceptionOnClosure()
    //{
    //  int localVariable = 0;
    //  WeakDelegate<Action> weakDelegate = new WeakDelegate<Action>(() => Console.WriteLine(localVariable));
    //}


    [Test]
    public void TargetReference()
    {
      MockDelegateTarget mockDelegateTarget = new MockDelegateTarget();
      var weakDelegate = new WeakDelegate<Action<object>>(mockDelegateTarget.DelegateMethod);
      Assert.AreEqual(mockDelegateTarget, weakDelegate.TargetReference.Target);

      mockDelegateTarget = null;
      GC.Collect();
      Assert.IsNull(weakDelegate.TargetReference.Target);
    }


    [Test]
    public void MethodInfo()
    {
      MockDelegateTarget mockDelegateTarget = new MockDelegateTarget();
      var weakDelegate = new WeakDelegate<Action<object>>(mockDelegateTarget.DelegateMethod);
      Assert.AreEqual("DelegateMethod", weakDelegate.MethodInfo.Name);
    }


    [Test]
    public void IsValidAsLongAsTargetIsAlive()
    {
      MockDelegateTarget mockDelegateTarget = new MockDelegateTarget();
      var weakDelegate = new WeakDelegate<Action<object>>(mockDelegateTarget.DelegateMethod);

      GC.Collect();
      Assert.IsTrue(weakDelegate.IsAlive);

      object dummyParameter = new object();
      weakDelegate.Invoke(dummyParameter);
      Assert.IsTrue(mockDelegateTarget.DelegateCalled);
      Assert.AreSame(dummyParameter, mockDelegateTarget.DelegateParameter);
    }


    [Test]
    public void IsInvalidWhenTargetIsCollected()
    {
      MockDelegateTarget mockDelegateTarget = new MockDelegateTarget();
      var weakDelegate = new WeakDelegate<Action<object>>(mockDelegateTarget.DelegateMethod);

      mockDelegateTarget = null;
      GC.Collect();

      Assert.IsFalse(weakDelegate.IsAlive);
    }


    [Test]
    public void IsAlwaysValidForStaticMethods()
    {
      var weakDelegate = new WeakDelegate<Action<object>>(MockStaticDelegateTarget.DelegateMethod);

      GC.Collect();
      Assert.IsTrue(weakDelegate.IsAlive);

      MockStaticDelegateTarget.Clear();
      object dummyParameter = new object();
      weakDelegate.Invoke(dummyParameter);
      Assert.IsTrue(MockStaticDelegateTarget.DelegateCalled);
      Assert.AreSame(dummyParameter, MockStaticDelegateTarget.DelegateParameter);
    }


    [Test]
    public void RetrieveDelegateOfObjectMethod()
    {
      MockDelegateTarget mockDelegateTarget = new MockDelegateTarget();
      var weakDelegate = new WeakDelegate<Action<object>>(mockDelegateTarget.DelegateMethod);

      object dummyParameter = new object();
      Delegate d = weakDelegate.Delegate;      
      d.DynamicInvoke(dummyParameter);
      Assert.IsTrue(mockDelegateTarget.DelegateCalled);
      Assert.AreSame(dummyParameter, mockDelegateTarget.DelegateParameter);
    }


    [Test]
    public void RetrieveDelegateTypeOfObjectMethod()
    {
      MockDelegateTarget mockDelegateTarget = new MockDelegateTarget();
      var weakDelegate = new WeakDelegate<Action<object>>(mockDelegateTarget.DelegateMethod);

      Assert.AreEqual(typeof(Action<object>), weakDelegate.DelegateType);
    }


    [Test]
    public void RetrieveDelegateOfStaticMethod()
    {
      var weakDelegate = new WeakDelegate<Action<object>>(MockStaticDelegateTarget.DelegateMethod);

      MockStaticDelegateTarget.Clear();
      object dummyParameter = new object();
      Delegate d = weakDelegate.Delegate;
      d.DynamicInvoke(dummyParameter);
      Assert.IsTrue(MockStaticDelegateTarget.DelegateCalled);
      Assert.AreSame(dummyParameter, MockStaticDelegateTarget.DelegateParameter);
    }


    [Test]
    public void RetrieveDelegateOfGarbageCollectedObject()
    {
      MockDelegateTarget mockDelegateTarget = new MockDelegateTarget();
      var weakDelegate = new WeakDelegate<Action<object>>(mockDelegateTarget.DelegateMethod);

      mockDelegateTarget = null;
      GC.Collect();

      Assert.IsNull(weakDelegate.Delegate);
    }


    public class MockDelegateTarget
    {
      public bool DelegateCalled;
      public object DelegateParameter;

      public void DelegateMethod(object dummy)
      {
        DelegateParameter = dummy;
        DelegateCalled = true;
      }
    }


    public static class MockStaticDelegateTarget
    {
      public static bool DelegateCalled;
      public static object DelegateParameter;

      public static void Clear()
      {
        DelegateCalled = false;
        DelegateParameter = null;
      }

      public static void DelegateMethod(object dummy)
      {
        DelegateParameter = dummy;
        DelegateCalled = true;
      }
    }
  }
}
