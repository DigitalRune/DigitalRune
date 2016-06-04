using System;
using System.Reactive.Subjects;
using NUnit.Framework;


namespace DigitalRune.Windows.Framework.Tests
{
    [TestFixture]
    public class MessageBusTest
    {
        [Test]
        public void SubscribeToMessageBus()
        {
            var messageBus = new MessageBus();
            messageBus.Listen<string>().Subscribe(_ => Assert.Fail(), e => Assert.Fail(), Assert.Fail);
        }


        [Test]
        public void ListenToMessageWithoutToken()
        {
            const string message1 = "Test Message #1";
            const string message2 = "Test Message #2";
            string receivedMessage = null;

            var messageBus = new MessageBus();
            messageBus.Listen<string>().Subscribe(m => receivedMessage = m, e => Assert.Fail(), Assert.Fail);

            Assert.IsNull(receivedMessage);

            messageBus.Publish(message1);
            Assert.AreEqual(message1, receivedMessage);

            messageBus.Publish(message2);
            Assert.AreEqual(message2, receivedMessage);
        }


        [Test]
        public void UnsubscribeListener()
        {
            const string message1 = "Test Message #1";
            const string message2 = "Test Message #2";
            string receivedMessage = null;

            var messageBus = new MessageBus();
            var subscription = messageBus.Listen<string>().Subscribe(m => receivedMessage = m, e => Assert.Fail(), Assert.Fail);

            Assert.IsNull(receivedMessage);

            messageBus.Publish(message1);
            Assert.AreEqual(message1, receivedMessage);

            subscription.Dispose();
            messageBus.Publish(message2);
            Assert.AreEqual(message1, receivedMessage);
        }


        [Test]
        public void ListenToMessageWithToken()
        {
            const string message1 = "Test Message #1";
            const string message2 = "Test Message #2";
            string token = Guid.NewGuid().ToString();
            string receivedMessage = null;

            var messageBus = new MessageBus();
            messageBus.Listen<string>(token).Subscribe(m => receivedMessage = m, e => Assert.Fail(), Assert.Fail);

            Assert.IsNull(receivedMessage);

            messageBus.Publish(message1, token);
            Assert.AreEqual(message1, receivedMessage);

            messageBus.Publish(message2, token);
            Assert.AreEqual(message2, receivedMessage);
        }


        [Test]
        public void ShouldNotReceiveMessageWithToken()
        {
            const string message1 = "Test Message #1";
            string token = Guid.NewGuid().ToString();
            string receivedMessage = null;

            var messageBus = new MessageBus();
            messageBus.Listen<string>().Subscribe(m => receivedMessage = m, e => Assert.Fail(), Assert.Fail);

            Assert.IsNull(receivedMessage);

            messageBus.Publish(message1, token);
            Assert.IsNull(receivedMessage);
        }


        [Test]
        public void ShouldNotReceiveMessageWithoutToken()
        {
            const string message1 = "Test Message #1";
            string token = Guid.NewGuid().ToString();
            string receivedMessage = null;

            var messageBus = new MessageBus();
            messageBus.Listen<string>(token).Subscribe(m => receivedMessage = m, e => Assert.Fail(), Assert.Fail);

            Assert.IsNull(receivedMessage);

            messageBus.Publish(message1);
            Assert.IsNull(receivedMessage);
        }


        [Test]
        public void SuscribersShouldBeWeak()
        {
            const string message1 = "Test Message #1";
            const string message2 = "Test Message #2";
            string token = Guid.NewGuid().ToString();
            string receivedMessage = null;

            var messageBus = new MessageBus();
            messageBus.Listen<string>(token).Subscribe(m => receivedMessage = m, e => Assert.Fail(), Assert.Fail);

            Assert.IsNull(receivedMessage);

            messageBus.Publish(message1, token);
            Assert.AreEqual(message1, receivedMessage);

            GC.Collect();

            messageBus.Publish(message2);
            Assert.AreEqual(message1, receivedMessage);
        }


        [Test]
        public void RegisterPublisher()
        {
            const string message1 = "Test Message #1";
            const string message2 = "Test Message #2";
            const string message3 = "Test Message #3";
            string receivedMessage = null;

            var publisher = new Subject<string>();
            var messageBus = new MessageBus();
            messageBus.RegisterPublisher(publisher);
            publisher.OnNext(message1);

            messageBus.Listen<string>().Subscribe(m => receivedMessage = m, e => Assert.Fail(), Assert.Fail);
            Assert.IsNull(receivedMessage);

            publisher.OnNext(message2);
            Assert.AreEqual(message2, receivedMessage);

            publisher.OnNext(message3);
            Assert.AreEqual(message3, receivedMessage);
        }


        [Test]
        public void ShouldThrowIfPublisherIsNull()
        {
            var messageBus = new MessageBus();
            Assert.Throws<ArgumentNullException>(() => messageBus.RegisterPublisher<string>(null));
        }


        [Test]
        public void UnregisterPublisher()
        {
            const string message1 = "Test Message #1";
            const string message2 = "Test Message #2";
            string receivedMessage = null;

            var publisher = new Subject<string>();
            var messageBus = new MessageBus();
            var subscription = messageBus.RegisterPublisher(publisher);

            messageBus.Listen<string>().Subscribe(m => receivedMessage = m, e => Assert.Fail(), Assert.Fail);
            Assert.IsNull(receivedMessage);

            publisher.OnNext(message1);
            Assert.AreEqual(message1, receivedMessage);

            subscription.Dispose();

            publisher.OnNext(message2);
            Assert.AreEqual(message1, receivedMessage);
        }


        [Test]
        public void MultiplePublishers()
        {
            const string message1 = "Test Message #1";
            const string message2 = "Test Message #2";
            const string message3 = "Test Message #3";
            const string message4 = "Test Message #4";
            string receivedMessage = null;


            var messageBus = new MessageBus();
            var publisher1 = new Subject<string>();
            var publisher2 = new Subject<string>();
            var subscription1 = messageBus.RegisterPublisher(publisher1);
            messageBus.RegisterPublisher(publisher2);

            messageBus.Listen<string>().Subscribe(m => receivedMessage = m, e => Assert.Fail(), Assert.Fail);
            Assert.IsNull(receivedMessage);

            messageBus.Publish(message1);
            Assert.AreEqual(message1, receivedMessage);

            publisher1.OnNext(message2);
            Assert.AreEqual(message2, receivedMessage);

            publisher2.OnNext(message3);
            Assert.AreEqual(message3, receivedMessage);

            subscription1.Dispose();
            publisher1.OnNext(message4);
            Assert.AreEqual(message3, receivedMessage);

            publisher2.OnNext(message4);
            Assert.AreEqual(message4, receivedMessage);
        }


        [Test]
        public void MultipleSubscribers()
        {
            const string message1 = "Test Message #1";
            const string message2 = "Test Message #2";
            string receivedMessage1 = null;
            string receivedMessage2 = null;

            var messageBus = new MessageBus();

            messageBus.Listen<string>().Subscribe(m => receivedMessage1 = m, e => Assert.Fail(), Assert.Fail);
            Assert.IsNull(receivedMessage1);

            messageBus.Publish(message1);
            Assert.AreEqual(message1, receivedMessage1);

            messageBus.Listen<string>().Subscribe(m => receivedMessage2 = m, e => Assert.Fail(), Assert.Fail);
            Assert.IsNull(receivedMessage2);

            messageBus.Publish(message2);
            Assert.AreEqual(message2, receivedMessage1);
            Assert.AreEqual(message2, receivedMessage2);
        }
    }
}
