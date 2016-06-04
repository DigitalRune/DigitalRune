using System;
using System.ComponentModel;
using System.Linq.Expressions;
using NUnit.Framework;


namespace DigitalRune.Windows.Framework.Tests
{
    [TestFixture]
    public class PropertyObserverTest
    {
        private static bool _dummyClassFinalized;


        internal class DummyClass : INotifyPropertyChanged
        {
            public int Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    OnPropertyChanged("Value");
                }
            }
            private int _value;


            public string Text
            {
                get { return _text; }
                set
                {
                    _text = value;
                    OnPropertyChanged("Text");
                }
            }
            private string _text;


            public event PropertyChangedEventHandler PropertyChanged;


            public void RaisePropertyChanged()
            {
                OnPropertyChanged(null);
            }


            void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }


            ~DummyClass()
            {
                _dummyClassFinalized = true;
            }
        }


        [SetUp]
        public void SetUp()
        {
            _dummyClassFinalized = false;
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorShouldThrowWhenArgumentIsNull()
        {
            new PropertyObserver<DummyClass>(null);
        }


        [Test]
        public void ConstructorTest()
        {
            new PropertyObserver<DummyClass>(new DummyClass());
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterShouldThrowWhenPropertyIsNull()
        {
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.RegisterHandler((string)null, src => { });
        }


        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterShouldThrowWhenPropertyIsEmpty()
        {
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.RegisterHandler("", src => { });
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterShouldThrowWhenExpressionIsNull()
        {
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.RegisterHandler((Expression<Func<DummyClass, object>>)null, src => { });
        }


        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterShouldThrowWhenExpressionWrong()
        {
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.RegisterHandler(src => src, src => { });
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterShouldThrowWhenHandlerIsNull()
        {
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.RegisterHandler("Value", null);
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterShouldThrowWhenHandlerIsNull2()
        {
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.RegisterHandler(src => src.Value, null);
        }


#if DEBUG
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterShouldThrowWhenPropertyIsWrong()
        {
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.RegisterHandler("Misspelled Property", src => { });
        }
#endif


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnregisterShouldThrowWhenPropertyIsNull()
        {
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.UnregisterHandler((string)null);
        }


        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void UnregisterShouldThrowWhenPropertyIsEmpty()
        {
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.UnregisterHandler("");
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnregisterShouldThrowWhenExpressionIsNull()
        {
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.UnregisterHandler((Expression<Func<DummyClass, object>>)null);
        }


        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void UnregisterShouldThrowWhenExpressionIsWrong()
        {
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.UnregisterHandler(src => src);
        }


        [Test]
        public void HandlePropertyChangedEvents()
        {
            bool valueChanged = false;
            bool textChanged = false;
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.RegisterHandler("Value", src => valueChanged = true);
            observer.RegisterHandler(src => src.Text, src => textChanged = true);

            Assert.IsFalse(valueChanged);
            Assert.IsFalse(textChanged);

            source.Value = 10;
            Assert.IsTrue(valueChanged);
            Assert.IsFalse(textChanged);
            valueChanged = false;

            source.Text = "abc";
            Assert.IsFalse(valueChanged);
            Assert.IsTrue(textChanged);
            textChanged = false;

            source.RaisePropertyChanged();
            Assert.IsTrue(valueChanged);
            Assert.IsTrue(textChanged);
            valueChanged = false;
            textChanged = false;

            observer.UnregisterHandler("Value");
            source.RaisePropertyChanged();
            Assert.IsFalse(valueChanged);
            Assert.IsTrue(textChanged);
            textChanged = false;

            observer.UnregisterHandler(src => src.Text);
            source.RaisePropertyChanged();
            Assert.IsFalse(valueChanged);
            Assert.IsFalse(textChanged);
        }


        [Test]
        public void SourceIsWeakReference()
        {
            bool valueChanged = false;
            bool textChanged = false;
            DummyClass source = new DummyClass();
            PropertyObserver<DummyClass> observer = new PropertyObserver<DummyClass>(source);
            observer.RegisterHandler("Value", src => valueChanged = true);
            observer.RegisterHandler(src => src.Text, src => textChanged = true);
            source.RaisePropertyChanged();
            Assert.IsTrue(valueChanged);
            Assert.IsTrue(textChanged);

            Assert.IsFalse(_dummyClassFinalized);
            source = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Assert.IsTrue(_dummyClassFinalized);

            observer.UnregisterHandler("Value");
            observer.UnregisterHandler(src => src.Text);
        }
    }
}
