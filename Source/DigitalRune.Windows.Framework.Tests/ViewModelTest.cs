/*
using System;
using NUnit.Framework;


namespace DigitalRune.Windows.Framework.Tests
{
  [TestFixture]
  public class ViewModelTest
  {
    private class MyViewModel : ViewModel<MyViewModel>
    {
      public int Value1
      {
        get { return _value1; }
        set
        {
          _value1 = value;
          RaisePropertyChanged(x => x.Value1);
        }
      }
      private int _value1;


      public int Value2
      {
        get { return _value2; }
        set
        {
          _value2 = value;
          RaisePropertyChanged(x => x.Value2);
        }
      }
      private int _value2;


      public int InvalidValue
      {
        get { return _invalidValue; }
        set
        {
          _invalidValue = value;
          RaisePropertyChanged<object>(null);
        }
      }
      private int _invalidValue;


      public bool DisposeWasCalled { get; private set; }


      public MyViewModel()
      {
        DisposeWasCalled = false;
        DisplayName = "Name of MyViewModel";
      }


      protected override void Dispose(bool disposing)
      {
        DisposeWasCalled = true;
        base.Dispose(disposing);
      }

      public void InvalidateAllProperties()
      {
        RaisePropertyChanged();
      }
    }


    [Test]
    public void DisposeTest()
    {
      var vm = new MyViewModel();
      Assert.IsFalse(vm.IsDisposed);
      Assert.IsFalse(vm.DisposeWasCalled);
      vm.Dispose();
      Assert.IsTrue(vm.IsDisposed);
      Assert.IsTrue(vm.DisposeWasCalled);
    }


    [Test]
    public void DisplayNameTest()
    {
      var vm = new MyViewModel();
      Assert.AreEqual("Name of MyViewModel", vm.DisplayName);
    }


    [Test]
    public void PropertyChangedTest()
    {
      bool allPropertiesChanged = false;
      bool value1Changed = false;
      bool value2Changed = false;

      var vm = new MyViewModel();
      vm.PropertyChanged += ((sender, eventArgs) => allPropertiesChanged = String.IsNullOrEmpty(eventArgs.PropertyName));
      vm.PropertyChanged += ((sender, eventArgs) => value1Changed = eventArgs.PropertyName == "Value1");
      vm.PropertyChanged += ((sender, eventArgs) => value2Changed = eventArgs.PropertyName == "Value2");

      Assert.IsFalse(allPropertiesChanged);
      Assert.IsFalse(value1Changed);
      Assert.IsFalse(value2Changed);

      vm.Value1 = 10;
      Assert.AreEqual(10, vm.Value1);
      Assert.IsFalse(allPropertiesChanged);
      Assert.IsTrue(value1Changed);
      Assert.IsFalse(value2Changed);
      value1Changed = false;

      vm.Value2 = 11;
      Assert.AreEqual(11, vm.Value2);
      Assert.IsFalse(allPropertiesChanged);
      Assert.IsFalse(value1Changed);
      Assert.IsTrue(value2Changed);
      value2Changed = false;

      vm.InvalidateAllProperties();
      Assert.IsTrue(allPropertiesChanged);
      Assert.IsFalse(value1Changed);
      Assert.IsFalse(value2Changed);
      value2Changed = false;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RaisePropertyChangedShouldThrowWhenNull()
    {
      var vm = new MyViewModel { InvalidValue = 10 };
    }
  }
}
*/
