using System;
using NUnit.Framework;


namespace DigitalRune.Windows.Framework.Tests.MVVM
{
    [TestFixture]
    public class CompositeCommandTest
    {
        [Test]
        public void ExecuteMultipleCommands()
        {
            bool command1Called = false;
            bool command2Called = false;
            var command1 = new DelegateCommand(
              () => command1Called = true,
              () => true);
            var command2 = new DelegateCommand<int>(
              i =>
              {
                  Assert.AreEqual(123, i);
                  command2Called = true;
              },
              i =>
              {
                  Assert.AreEqual(123, i);
                  return true;
              });

            var compositeCommand = new CompositeCommand();
            compositeCommand.RegisterCommand(command1);
            compositeCommand.RegisterCommand(command2);

            compositeCommand.Execute(123);
            Assert.IsTrue(command1Called);
            Assert.IsTrue(command2Called);

            command1Called = false;
            command2Called = false;
            compositeCommand.UnregisterCommand(command1);

            compositeCommand.Execute(123);
            Assert.IsFalse(command1Called);
            Assert.IsTrue(command2Called);
        }


        [Test]
        public void CanExecuteWithMultipleCommands()
        {
            bool canExecuteCommand1 = false;
            bool canExecuteCommand2 = false;
            var command1 = new DelegateCommand(
              () => { },
              () => canExecuteCommand1);
            var command2 = new DelegateCommand<int>(
              i => Assert.AreEqual(123, i),
              i =>
              {
                  Assert.AreEqual(123, i);
                  return canExecuteCommand2;
              });

            var compositeCommand = new CompositeCommand();
            Assert.IsFalse(compositeCommand.CanExecute(123));

            compositeCommand.RegisterCommand(command1);
            compositeCommand.RegisterCommand(command2);
            Assert.IsFalse(compositeCommand.CanExecute(123));

            canExecuteCommand1 = true;
            Assert.IsFalse(compositeCommand.CanExecute(123));

            canExecuteCommand2 = true;
            Assert.IsTrue(compositeCommand.CanExecute(123));

            canExecuteCommand1 = false;
            compositeCommand.UnregisterCommand(command1);
            Assert.IsTrue(compositeCommand.CanExecute(123));

            canExecuteCommand2 = false;
            Assert.IsFalse(compositeCommand.CanExecute(123));


            canExecuteCommand1 = true;
            canExecuteCommand2 = true;
            compositeCommand.UnregisterCommand(command2);
            Assert.IsFalse(compositeCommand.CanExecute(123));
        }


        [Test]
        public void CanExecuteChangedWithMultipleCommands()
        {
            var command1 = new DelegateCommand(() => { }, () => true);
            var command2 = new DelegateCommand<int>(i => { }, _ => true);

            var compositeCommand = new CompositeCommand();
            bool canExecuteEventFired = false;
            EventHandler eventHandler = (sender, eventArgs) =>
                                        {
                                            Assert.AreSame(compositeCommand, sender);
                                            Assert.IsNotNull(eventArgs);
                                            canExecuteEventFired = true;
                                        };
            compositeCommand.CanExecuteChanged += eventHandler;
            compositeCommand.RegisterCommand(command1);
            compositeCommand.RegisterCommand(command2);

            Assert.IsFalse(EventConsumer.EventCalled);

            command1.RaiseCanExecuteChanged();
            Assert.IsTrue(canExecuteEventFired);

            canExecuteEventFired = false;
            command2.RaiseCanExecuteChanged();
            Assert.IsTrue(canExecuteEventFired);

            canExecuteEventFired = false;
            compositeCommand.UnregisterCommand(command1);
            Assert.IsTrue(canExecuteEventFired);  // Unregister also raises CanExecuteChanged.

            canExecuteEventFired = false;
            command1.RaiseCanExecuteChanged();
            Assert.IsFalse(canExecuteEventFired);
            command2.RaiseCanExecuteChanged();
            Assert.IsTrue(canExecuteEventFired);

            canExecuteEventFired = false;
            compositeCommand.UnregisterCommand(command2);
            Assert.IsTrue(canExecuteEventFired);  // Unregister also raises CanExecuteChanged.

            canExecuteEventFired = false;
            command1.RaiseCanExecuteChanged();
            command2.RaiseCanExecuteChanged();
            Assert.IsFalse(canExecuteEventFired);
        }


#if !NET45
        [Test]
        public void CanExecuteChangedEventShouldBeWeak()
        {
            var command1 = new DelegateCommand(() => { }, null);
            var command2 = new DelegateCommand<int>(i => { }, null);

            var compositeCommand = new CompositeCommand();
            compositeCommand.RegisterCommand(command1);
            compositeCommand.RegisterCommand(command2);
            compositeCommand.CanExecuteChanged += new EventConsumer().EventHandler;

            // Garbage collect the EventConsumer.
            GC.Collect();

            EventConsumer.Clear();
            command1.RaiseCanExecuteChanged();
            command2.RaiseCanExecuteChanged();
            Assert.IsFalse(EventConsumer.EventCalled);
        }
#endif


        internal class EventConsumer
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
    }
}
