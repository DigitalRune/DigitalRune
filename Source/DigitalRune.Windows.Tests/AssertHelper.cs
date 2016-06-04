using System;
#if NETFX_CORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using NUnit.Framework;
#endif


namespace DigitalRune.Windows.Tests
{
    public static class AssertHelper
    {
        public static void Throws<T>(Action action) where T : Exception
        {
#if WINDOWS_PHONE
            Assert.ThrowsException<T>(action);
#else
            Assert.Throws(typeof(T), () => action());
#endif
        }
    }
}
