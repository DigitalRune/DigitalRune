using System;
using System.Windows.Input;
using NUnit.Framework;


namespace DigitalRune.Windows.Framework.Tests
{
    [TestFixture]
    public class DelegateCommandTest
    {
        [Test]
        public void ConstructorException()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateCommand(null));
        }


        [Test]
        public void ConstructorException2()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateCommand(null, null));
        }


        [Test]
        public void Execute()
        {
            int i = 0;
            Action execute = (() => i++);
            var command = new DelegateCommand(execute);

            object dummy = new object();
            Assert.IsTrue(((ICommand)command).CanExecute(dummy));
            ((ICommand)command).Execute(dummy);
            Assert.AreEqual(1, i);
            Assert.IsTrue(((ICommand)command).CanExecute(dummy));
            ((ICommand)command).Execute(dummy);
            Assert.AreEqual(2, i);
            Assert.IsTrue(command.CanExecute());
            command.Execute();
            Assert.AreEqual(3, i);
            Assert.IsTrue(command.CanExecute());
            command.Execute();
            Assert.AreEqual(4, i);
        }


        [Test]
        public void CanExecute()
        {
            bool b = false;
            int i = 0;

            Action execute = (() => i++);
            Func<bool> predicate = (() => b);
            var command = new DelegateCommand(execute, predicate);

            object dummy = new object();
            Assert.IsFalse(((ICommand)command).CanExecute(dummy));
            Assert.IsFalse(command.CanExecute());
            b = true;
            Assert.IsTrue(((ICommand)command).CanExecute(dummy));
            Assert.IsTrue(command.CanExecute());
            b = false;
            Assert.IsFalse(((ICommand)command).CanExecute(dummy));
            Assert.IsFalse(command.CanExecute());
        }


        [Test]
        public void CanExecuteChanged()
        {
            bool b = false;
            int i = 0;
            Action execute = (() => { });
            Func<bool> predicate = (() => b);
            var command = new DelegateCommand(execute, predicate);
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
            Action execute = (() => { });
            Func<bool> predicate = (() => b);
            var command = new DelegateCommand(execute, predicate);
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
