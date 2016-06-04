using System;
using System.Windows.Input;
using NUnit.Framework;


namespace DigitalRune.Windows.Framework.Tests
{
    [TestFixture]
    public class CommandReferenceTest
    {
        [Test]
        public void ShouldExecuteReferencedCommand()
        {
            bool canExecuteChangedCalled = false;
            TestCommand testCommand = new TestCommand();
            CommandReference commandReference = new CommandReference { Command = testCommand };
            commandReference.CanExecuteChanged += ((sender, eventArgs) => canExecuteChangedCalled = true);

            object canExecuteParameter = new object();
            bool canExecute = commandReference.CanExecute(canExecuteParameter);
            Assert.IsTrue(canExecute);
            Assert.AreSame(canExecuteParameter, testCommand.CanExecuteParameter);

            object executeParameter = new object();
            commandReference.Execute(executeParameter);
            Assert.AreSame(executeParameter, testCommand.ExecuteParameter);

            Assert.IsFalse(canExecuteChangedCalled);
        }


        [Test]
        public void CanExecuteShouldReturnFalseWhenEmpty()
        {
            CommandReference commandReference = new CommandReference();

            bool canExecute = commandReference.CanExecute(null);
            Assert.IsFalse(canExecute);
        }


        [Test]
        public void ForwardCanExecuteChangedEvent()
        {
            bool canExecuteChangedCalled = false;
            TestCommand testCommand = new TestCommand();
            CommandReference commandReference = new CommandReference { Command = testCommand };
            commandReference.CanExecuteChanged += ((sender, eventArgs) => canExecuteChangedCalled = true);

            object canExecuteParameter = new object();
            bool canExecute = commandReference.CanExecute(canExecuteParameter);
            Assert.IsTrue(canExecute);
            Assert.AreSame(canExecuteParameter, testCommand.CanExecuteParameter);

            testCommand.CanExecuteValue = false;
            testCommand.FireCanExecuteChanged();

            Assert.IsTrue(canExecuteChangedCalled);

            canExecuteParameter = new object();
            canExecute = commandReference.CanExecute(canExecuteParameter);
            Assert.IsFalse(canExecute);
            Assert.AreSame(canExecuteParameter, testCommand.CanExecuteParameter);

            commandReference.Command = null;
        }


        internal class TestCommand : ICommand
        {
            public object CanExecuteParameter { get; set; }
            public bool CanExecuteValue { get; set; }
            public bool CanExecuteCalled { get; set; }
            public object ExecuteParameter { get; set; }
            public bool ExecuteCalled { get; set; }
            public int ExecuteCallCount { get; set; }
            public event EventHandler CanExecuteChanged;

            public TestCommand()
            {
                CanExecuteValue = true;
            }

            public void FireCanExecuteChanged()
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }

            public bool CanExecute(object parameter)
            {
                CanExecuteParameter = parameter;
                CanExecuteCalled = true;
                return CanExecuteValue;
            }

            public void Execute(object parameter)
            {
                ExecuteParameter = parameter;
                ExecuteCalled = true;
                ExecuteCallCount += 1;
            }
        }
    }
}
