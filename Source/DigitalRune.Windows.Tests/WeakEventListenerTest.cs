#if !SILVERLIGHT && !WINDOWS_PHONE
using System;
using System.ComponentModel;
using NUnit.Framework;

#pragma warning disable 618       // WeakEventListener is obsolete.


namespace DigitalRune.Windows.Tests
{
    class Dummy : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged()
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(null));
        }
    }


    [TestFixture]
    public class WeakEventListenerTest
    {
        [Test]
        public void ShouldStoreEventHandler()
        {
            bool weakEventReceived = false;

            var listener = new WeakEventListener<PropertyChangedEventArgs>((sender, eventArgs) => weakEventReceived = true);

            // Check whether EventHandler was stored correctly.
            Assert.IsNotNull(listener.EventHandler);
            listener.EventHandler(null, null);
            Assert.IsTrue(weakEventReceived);
            weakEventReceived = false;
        }


        [Test]
        public void HandleWeakEvent()
        {
            Dummy dummy = new Dummy();
            bool weakEventReceived = false;
            var listener = new WeakEventListener<PropertyChangedEventArgs>((sender, eventArgs) => weakEventReceived = true);

            PropertyChangedEventManager.AddListener(dummy, listener, String.Empty);
            dummy.RaisePropertyChanged();
            Assert.IsTrue(weakEventReceived);
            weakEventReceived = false;
        }


        [Test]
        public void AllowsGarbageCollection()
        {
            Dummy dummy = new Dummy();
            bool weakEventReceived = false;
            var listener = new WeakEventListener<PropertyChangedEventArgs>((sender, eventArgs) => weakEventReceived = true);

            PropertyChangedEventManager.AddListener(dummy, listener, String.Empty);

            WeakReference weakReference = new WeakReference(listener);
            listener = null;
            GC.Collect();

            Assert.IsFalse(weakReference.IsAlive);
            Assert.IsFalse(weakEventReceived);
        }
    }
}
#endif
