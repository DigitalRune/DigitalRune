using System;
using NUnit.Framework;


namespace DigitalRune.Tests
{
  [TestFixture]
  public class WeakDelegatesCollectionTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddDelegateShouldThrowExceptionWhenNull()
    {
      var weakMulticastDelegate = new WeakMulticastDelegate<Action>();
      weakMulticastDelegate.Add(null);
    }


    [Test]
    public void InvokesRegisteredDelegates()
    {
      MockDelegateTarget mockDelegateTarget1 = new MockDelegateTarget();
      MockDelegateTarget mockDelegateTarget2 = new MockDelegateTarget();

      var weakMulticastDelegate = new WeakMulticastDelegate<Action<object>>();
      weakMulticastDelegate.Add(mockDelegateTarget1.DelegateMethod);
      weakMulticastDelegate.Add(mockDelegateTarget2.DelegateMethod);
      weakMulticastDelegate.Add(mockDelegateTarget1.DelegateMethod2);
      weakMulticastDelegate.Add(mockDelegateTarget2.DelegateMethod2);
      weakMulticastDelegate.Add(MockStaticDelegateTarget.DelegateMethod);

      MockStaticDelegateTarget.Clear();
      object parameter = new object();
      weakMulticastDelegate.Invoke(parameter);

      Assert.IsTrue(mockDelegateTarget1.DelegateCalled);
      Assert.IsTrue(mockDelegateTarget2.DelegateCalled);
      Assert.IsTrue(mockDelegateTarget1.DelegateCalled2);
      Assert.IsTrue(mockDelegateTarget2.DelegateCalled2);
      Assert.IsTrue(MockStaticDelegateTarget.DelegateCalled);
      Assert.AreSame(parameter, mockDelegateTarget1.DelegateParameter);
      Assert.AreSame(parameter, mockDelegateTarget2.DelegateParameter);
      Assert.AreSame(parameter, mockDelegateTarget1.DelegateParameter2);
      Assert.AreSame(parameter, mockDelegateTarget2.DelegateParameter2);
      Assert.AreSame(parameter, MockStaticDelegateTarget.DelegateParameter);
    }


    [Test]
    public void RemoveRegisteredDelegates()
    {
      MockDelegateTarget mockDelegateTarget1 = new MockDelegateTarget();
      MockDelegateTarget mockDelegateTarget2 = new MockDelegateTarget();

      var weakMulticastDelegate = new WeakMulticastDelegate<Action<object>>();
      weakMulticastDelegate.Add(mockDelegateTarget1.DelegateMethod);
      weakMulticastDelegate.Add(mockDelegateTarget2.DelegateMethod);
      weakMulticastDelegate.Add(mockDelegateTarget1.DelegateMethod2);
      weakMulticastDelegate.Add(mockDelegateTarget2.DelegateMethod2);
      weakMulticastDelegate.Add(MockStaticDelegateTarget.DelegateMethod);

      weakMulticastDelegate.Remove(mockDelegateTarget1.DelegateMethod);
      weakMulticastDelegate.Remove(mockDelegateTarget1.DelegateMethod);  // Remove twice
      weakMulticastDelegate.Remove(mockDelegateTarget2.DelegateMethod);
      weakMulticastDelegate.Remove(mockDelegateTarget2.DelegateMethod2);
      weakMulticastDelegate.Remove(MockStaticDelegateTarget.DelegateMethod);

      MockStaticDelegateTarget.Clear();
      object parameter = new object();
      weakMulticastDelegate.Invoke(parameter);

      Assert.IsFalse(mockDelegateTarget1.DelegateCalled);
      Assert.IsFalse(mockDelegateTarget2.DelegateCalled);
      Assert.IsTrue(mockDelegateTarget1.DelegateCalled2);
      Assert.IsFalse(mockDelegateTarget2.DelegateCalled2);
      Assert.IsFalse(MockStaticDelegateTarget.DelegateCalled);
      Assert.AreSame(parameter, mockDelegateTarget1.DelegateParameter2);
    }


    [Test]
    public void GCRemovesDelegates()
    {
      MockDelegateTarget mockDelegateTarget1 = new MockDelegateTarget();
      MockDelegateTarget mockDelegateTarget2 = new MockDelegateTarget();

      var weakMulticastDelegate = new WeakMulticastDelegate<Action<object>>();
      weakMulticastDelegate.Add(mockDelegateTarget1.DelegateMethod);
      weakMulticastDelegate.Add(mockDelegateTarget2.DelegateMethod);
      weakMulticastDelegate.Add(mockDelegateTarget1.DelegateMethod2);
      weakMulticastDelegate.Add(mockDelegateTarget2.DelegateMethod2);

      Assert.AreEqual(4, weakMulticastDelegate.Count);

      mockDelegateTarget2 = null;
      GC.Collect();
      object parameter = new object();
      weakMulticastDelegate.Invoke(parameter);
      Assert.AreEqual(2, weakMulticastDelegate.Count);
      Assert.IsTrue(mockDelegateTarget1.DelegateCalled);
      Assert.IsTrue(mockDelegateTarget1.DelegateCalled2);
      Assert.AreSame(parameter, mockDelegateTarget1.DelegateParameter);
      Assert.AreSame(parameter, mockDelegateTarget1.DelegateParameter2);

      mockDelegateTarget1 = null;
      GC.Collect();
      weakMulticastDelegate.Remove(null);
      Assert.AreEqual(0, weakMulticastDelegate.Count);

      weakMulticastDelegate.Invoke(parameter); // Dummy invoke on empty WeakDelegateManager.
    }


    public class MockDelegateTarget
    {
      public bool DelegateCalled;
      public object DelegateParameter;
      public bool DelegateCalled2;
      public object DelegateParameter2;

      public void DelegateMethod(object dummy)
      {
        DelegateParameter = dummy;
        DelegateCalled = true;
      }

      public void DelegateMethod2(object dummy)
      {
        DelegateParameter2 = dummy;
        DelegateCalled2 = true;
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
