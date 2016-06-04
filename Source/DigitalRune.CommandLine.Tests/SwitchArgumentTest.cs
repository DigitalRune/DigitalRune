using System;
using NUnit.Framework;


namespace DigitalRune.CommandLine.Tests
{
    [TestFixture]
    public class SwitchArgumentTest
    {
        [Test]
        public void InvalidName1()
        {
            Assert.Throws<ArgumentException>(() => new SwitchArgument("switch ", ""));
        }


        [Test]
        public void InvalidName2()
        {
            Assert.Throws<ArgumentException>(() => new SwitchArgument("s", ""));
        }


        [Test]
        public void InvalidName3()
        {
            Assert.Throws<ArgumentException>(() => new SwitchArgument("switch:x", ""));
        }


        [Test]
        public void InvalidName4()
        {
            Assert.Throws<ArgumentException>(() => new SwitchArgument("switch=", ""));
        }


        [Test]
        public void InvalidName5()
        {
            Assert.Throws<ArgumentException>(() => new SwitchArgument("switch", "", new[] { ":invalid" }, null));
        }


        [Test]
        public void InvalidName6()
        {
            Assert.Throws<ArgumentException>(() => new SwitchArgument("switch", "", new[] { "" }, null));
        }


        [Test]
        public void InvalidShortName1()
        {
            Assert.Throws<ArgumentException>(() => new SwitchArgument("switch", "", null, new[] { ':' }));
        }


        [Test]
        public void InvalidShortName2()
        {
            Assert.Throws<ArgumentException>(() => new SwitchArgument("switch", "", null, new[] { 'S', '-' }));
        }


        [Test]
        public void InvalidShortName3()
        {
            Assert.Throws<ArgumentException>(() => new SwitchArgument("switch", "", null, new[] { '-' }));
        }


        [Test]
        public void ParseLongName1()
        {
            SwitchArgument argument = new SwitchArgument("switch", "");
            Assert.IsFalse(string.IsNullOrEmpty(argument.GetSyntax()));
            Assert.IsTrue(string.IsNullOrEmpty(argument.GetHelp()));

            int i = 1;
            var result = argument.Parse(new[] { "--switch1", "--switch" }, ref i);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, i);
        }


        [Test]
        public void ParseLongName2()
        {
            SwitchArgument argument = new SwitchArgument("switch", "");

            int i = 1;
            var result = argument.Parse(new[] { "--switch1", "--SwItcH" }, ref i);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, i);
        }


        [Test]
        public void ParseInvalidLongName1()
        {
            SwitchArgument argument = new SwitchArgument("switch", "");

            int i = 1;
            var result = argument.Parse(new[] { "--switch1", "-switch" }, ref i);
            Assert.IsNull(result);
            Assert.AreEqual(1, i);
        }


        [Test]
        public void ParseInvalidLongName2()
        {
            SwitchArgument argument = new SwitchArgument("switch", "");

            int i = 1;
            var result = argument.Parse(new[] { "/switch1", "--switch:" }, ref i);
            Assert.IsNull(result);
            Assert.AreEqual(1, i);
        }


        [Test]
        public void ParseLongNameAlias()
        {
            SwitchArgument argument = new SwitchArgument("switch", "", new[] { "Alias" }, null);

            int i = 1;
            var result = argument.Parse(new[] { "--switch1", "--alias" }, ref i);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, i);
        }


        [Test]
        public void ParseShortName1()
        {
            SwitchArgument argument = new SwitchArgument("switch", "", null, new[] { 'S' });

            int i = 1;
            var result = argument.Parse(new[] { "--switch1", "-S" }, ref i);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, i);
        }


        [Test]
        public void ParseShortName2()
        {
            SwitchArgument argument = new SwitchArgument("switch", "", null, new[] { 'S' });

            int i = 1;
            var result = argument.Parse(new[] { "--switch1", "-S" }, ref i);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, i);
        }


        [Test]
        public void ParseInvalidShortName1()
        {
            SwitchArgument argument = new SwitchArgument("switch", "", null, new[] { 'S' });

            int i = 1;
            var result = argument.Parse(new[] { "--switch1", "-s" }, ref i);
            Assert.IsNull(result);
            Assert.AreEqual(1, i);
        }


        [Test]
        public void ParseInvalidShortName3()
        {
            SwitchArgument argument = new SwitchArgument("switch", "", null, new[] { 'S' });

            int i = 1;
            var result = argument.Parse(new[] { "--switch1", "--S" }, ref i);
            Assert.IsNull(result);
            Assert.AreEqual(1, i);
        }


        [Test]
        public void ParseShortNameAlias()
        {
            SwitchArgument argument = new SwitchArgument("switch", "", null, new[] { 'S', 'A' });

            int i = 1;
            var result = argument.Parse(new[] { "--switch1", "-A" }, ref i);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, i);
        }


        [Test]
        public void ParseOther()
        {
            SwitchArgument argument = new SwitchArgument("switch", "", null, new[] { 'S' });

            int i = 0;
            var result = argument.Parse(new[] { "--switch1", "--switch" }, ref i);
            Assert.IsNull(result);
            Assert.AreEqual(0, i);
        }
    }
}
