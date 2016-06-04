using System;
using System.Windows.Controls;
using System.Windows.Input;
using NUnit.Framework;


namespace DigitalRune.Windows.Framework.Tests
{
    [TestFixture]
    public class DelegateCommandOfTTest
    {
        [Test]
        public void ConstructorException()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateCommand<int>(null));
        }


        [Test]
        public void ConstructorException2()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateCommand<int>(null, null));
        }


        [Test]
        public void Execute()
        {
            int i = 0;
            var execute = new Action<int>(param => i = param);
            var command = new DelegateCommand<int>(execute);

            Assert.IsTrue(((ICommand)command).CanExecute(12));
            ((ICommand)command).Execute(12);
            Assert.AreEqual(12, i);
            Assert.IsTrue(((ICommand)command).CanExecute(15));
            ((ICommand)command).Execute(15);
            Assert.AreEqual(15, i);
            Assert.IsTrue(command.CanExecute(14));
            command.Execute(14);
            Assert.AreEqual(14, i);
            Assert.IsTrue(command.CanExecute(16));
            command.Execute(16);
            Assert.AreEqual(16, i);
        }


        [Test]
        public void CanExecute()
        {
            bool b = false;
            int i = 0;

            Action<int> execute = (param => i = i + param);
            Func<int, bool> predicate = (param => b);
            var command = new DelegateCommand<int>(execute, predicate);

            Assert.IsFalse(((ICommand)command).CanExecute(i));
            Assert.IsFalse(command.CanExecute(i));
            b = true;
            Assert.IsTrue(((ICommand)command).CanExecute(i));
            Assert.IsTrue(command.CanExecute(i));
            b = false;
            Assert.IsFalse(((ICommand)command).CanExecute(i));
            Assert.IsFalse(command.CanExecute(i));
        }


        [Test]
        public void CanExecuteChanged()
        {
            bool b = false;
            int i = 0;
            Action<int> execute = (param => i = i + param);
            Func<int, bool> predicate = (param => b);
            var command = new DelegateCommand<int>(execute, predicate);
            EventHandler eventHandler = (sender, eventArgs) =>
            {
                Assert.AreSame(command, sender);
                Assert.IsNotNull(eventArgs);
                i++;
            };
            command.CanExecuteChanged += eventHandler;

            command.RaiseCanExecuteChanged();
            Assert.AreEqual(1, i);
            command.RaiseCanExecuteChanged();
            command.RaiseCanExecuteChanged();
            Assert.AreEqual(3, i);
            command.CanExecuteChanged -= eventHandler;
            command.RaiseCanExecuteChanged();
            Assert.AreEqual(3, i);
        }


#if !NET45
        [Test]
        public void CanExecuteChangedEventShouldBeWeak()
        {
            bool b = false;
            int i = 0;
            Action<int> execute = (param => i = i + param);
            Func<int, bool> predicate = (param => b);
            var command = new DelegateCommand<int>(execute, predicate);
            command.CanExecuteChanged += new EventConsumer().EventHandler;

            // Garbage collect the EventConsumer.
            GC.Collect();

            EventConsumer.Clear();
            command.RaiseCanExecuteChanged();
            Assert.IsFalse(EventConsumer.EventCalled);
        }


        private class EventConsumer
        {
            public static bool EventCalled;

            public static void Clear()
            {
                EventCalled = false;
            }

            public void EventHandler(object sender, EventArgs eventArgs)
            {
                Assert.AreEqual(EventArgs.Empty, eventArgs);
                EventCalled = true;
            }
        }
#endif
    }
}
