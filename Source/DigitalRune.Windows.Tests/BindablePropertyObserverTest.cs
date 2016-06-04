using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Controls;
using NUnit.Framework;


namespace DigitalRune.Windows.Tests
{
    [TestFixture]
    public class BindablePropertyObserverTest
    {
        class Foo : INotifyPropertyChanged
        {
            private int _value;

            public int Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }


            public void OnObservedValueChanged(object sender, EventArgs args)
            {
                Value++;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void GarbageCollection()
        {
            var source = new Slider();
            //var source = new Foo();
            var sourceWeak = new WeakReference(source);

            var observer = new Foo();
            var observerWeak = new WeakReference(observer);

            var bpo = new BindablePropertyObserver(source, "Value");
            bpo.ValueChanged += observer.OnObservedValueChanged;
            //var pd = TypeDescriptor.GetProperties(typeof(Foo)).OfType<PropertyDescriptor>().First(d => d.Name == "Value");
            //pd.AddValueChanged(source, observer.OnObservedValueChanged);

            Assert.That(observer.Value, Is.EqualTo(0));

            source.Value = 1;
            Assert.That(observer.Value, Is.EqualTo(1));

            source.Value = 2;
            Assert.That(observer.Value, Is.EqualTo(2));

            Assert.IsTrue(sourceWeak.IsAlive);
            Assert.IsTrue(observerWeak.IsAlive);

            source = null;

            GC.Collect();

            Assert.IsFalse(sourceWeak.IsAlive);     // This works for DependencyObjects but not for normal CLR objects with INotifyPropertyChanged;
            Assert.IsTrue(observerWeak.IsAlive);

            observer = null;
            bpo = null;

            GC.Collect();

            Assert.IsFalse(sourceWeak.IsAlive);
            Assert.IsFalse(observerWeak.IsAlive);
        }
    }
}
