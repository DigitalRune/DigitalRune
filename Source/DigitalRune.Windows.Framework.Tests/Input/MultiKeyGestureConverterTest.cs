using System.ComponentModel;
using System.Windows.Input;
using NUnit.Framework;


namespace DigitalRune.Windows.Framework.Tests
{
    [TestFixture]
    public class MultiKeyGestureConverterTest
    {
        [Test]
        public void ShouldConvertGestureToString()
        {
            MultiKeyGesture gesture = new MultiKeyGesture(new[] { Key.A, Key.B, Key.C }, ModifierKeys.Control, "My Display String");
            MultiKeyGestureConverter converter = (MultiKeyGestureConverter)TypeDescriptor.GetConverter(gesture);
            Assert.IsTrue(converter.CanConvertTo(typeof(string)));
            string s = converter.ConvertTo(gesture, typeof(string)) as string;
            Assert.IsNotNull(s);
            Assert.AreEqual("Ctrl+A, Ctrl+B, Ctrl+C:My Display String", s);

            gesture = new MultiKeyGesture(new[] { Key.A, Key.B, Key.C }, ModifierKeys.Control, "");
            s = converter.ConvertTo(gesture, typeof(string)) as string;
            Assert.IsNotNull(s);
            Assert.AreEqual("Ctrl+A, Ctrl+B, Ctrl+C", s);

            gesture = new MultiKeyGesture(new[] { Key.A, Key.B, Key.C }, ModifierKeys.None, "");
            s = converter.ConvertTo(gesture, typeof(string)) as string;
            Assert.IsNotNull(s);
            Assert.AreEqual("A, B, C", s);

            gesture = new MultiKeyGesture(new[] { Key.A }, ModifierKeys.None, "");
            s = converter.ConvertTo(gesture, typeof(string)) as string;
            Assert.IsNotNull(s);
            Assert.AreEqual("A", s);

            gesture = new MultiKeyGesture(new[] { Key.None }, ModifierKeys.None, "");
            s = converter.ConvertTo(gesture, typeof(string)) as string;
            Assert.IsNotNull(s);
            Assert.AreEqual("", s);

            gesture = new MultiKeyGesture(new[] { Key.A, Key.B, Key.C }, ModifierKeys.Control | ModifierKeys.Shift, "");
            s = converter.ConvertTo(gesture, typeof(string)) as string;
            Assert.IsNotNull(s);
            Assert.AreEqual("Ctrl+Shift+A, Ctrl+Shift+B, Ctrl+Shift+C", s);
        }


        [Test]
        public void ShouldConvertGestureFromString()
        {
            MultiKeyGestureConverter converter = (MultiKeyGestureConverter)TypeDescriptor.GetConverter(typeof(MultiKeyGesture));
            string s = "";
            Assert.IsTrue(converter.CanConvertFrom(typeof(string)));
            MultiKeyGesture gesture = converter.ConvertFrom(s) as MultiKeyGesture;
            Assert.IsNotNull(gesture);
            Assert.IsNotNull(gesture.Keys);
            Assert.AreEqual(ModifierKeys.None, gesture.Modifiers);
            Assert.AreEqual(Key.None, gesture.Key);
            Assert.AreEqual(1, gesture.Keys.Count);
            Assert.AreEqual(Key.None, gesture.Keys[0]);
            Assert.AreEqual("", gesture.DisplayString);

            s = "A";
            gesture = converter.ConvertFrom(s) as MultiKeyGesture;
            Assert.IsNotNull(gesture);
            Assert.IsNotNull(gesture.Keys);
            Assert.AreEqual(ModifierKeys.None, gesture.Modifiers);
            Assert.AreEqual(Key.None, gesture.Key);
            Assert.AreEqual(1, gesture.Keys.Count);
            Assert.AreEqual(Key.A, gesture.Keys[0]);
            Assert.AreEqual("A", gesture.DisplayString);

            s = "A,B,C";
            gesture = converter.ConvertFrom(s) as MultiKeyGesture;
            Assert.IsNotNull(gesture);
            Assert.IsNotNull(gesture.Keys);
            Assert.AreEqual(ModifierKeys.None, gesture.Modifiers);
            Assert.AreEqual(Key.None, gesture.Key);
            Assert.AreEqual(3, gesture.Keys.Count);
            Assert.AreEqual(Key.A, gesture.Keys[0]);
            Assert.AreEqual(Key.B, gesture.Keys[1]);
            Assert.AreEqual(Key.C, gesture.Keys[2]);
            Assert.AreEqual("A, B, C", gesture.DisplayString);

            s = "Ctrl+A, Ctrl+B,Ctrl+C";
            gesture = converter.ConvertFrom(s) as MultiKeyGesture;
            Assert.IsNotNull(gesture);
            Assert.IsNotNull(gesture.Keys);
            Assert.AreEqual(ModifierKeys.Control, gesture.Modifiers);
            Assert.AreEqual(Key.None, gesture.Key);
            Assert.AreEqual(3, gesture.Keys.Count);
            Assert.AreEqual(Key.A, gesture.Keys[0]);
            Assert.AreEqual(Key.B, gesture.Keys[1]);
            Assert.AreEqual(Key.C, gesture.Keys[2]);
            Assert.AreEqual("Ctrl+A, Ctrl+B, Ctrl+C", gesture.DisplayString);

            s = "Ctrl+A, Ctrl+B, Ctrl+C:My Display String";
            gesture = converter.ConvertFrom(s) as MultiKeyGesture;
            Assert.IsNotNull(gesture);
            Assert.IsNotNull(gesture.Keys);
            Assert.AreEqual(ModifierKeys.Control, gesture.Modifiers);
            Assert.AreEqual(Key.None, gesture.Key);
            Assert.AreEqual(3, gesture.Keys.Count);
            Assert.AreEqual(Key.A, gesture.Keys[0]);
            Assert.AreEqual(Key.B, gesture.Keys[1]);
            Assert.AreEqual(Key.C, gesture.Keys[2]);
            Assert.AreEqual("My Display String", gesture.DisplayString);

            s = "Ctrl+Shift+A, Ctrl+Shift+B, Ctrl+Shift+C:My Display String";
            gesture = converter.ConvertFrom(s) as MultiKeyGesture;
            Assert.IsNotNull(gesture);
            Assert.IsNotNull(gesture.Keys);
            Assert.AreEqual(ModifierKeys.Control | ModifierKeys.Shift, gesture.Modifiers);
            Assert.AreEqual(Key.None, gesture.Key);
            Assert.AreEqual(3, gesture.Keys.Count);
            Assert.AreEqual(Key.A, gesture.Keys[0]);
            Assert.AreEqual(Key.B, gesture.Keys[1]);
            Assert.AreEqual(Key.C, gesture.Keys[2]);
            Assert.AreEqual("My Display String", gesture.DisplayString);
        }
    }
}
