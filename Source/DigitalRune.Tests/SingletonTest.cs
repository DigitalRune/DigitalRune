using System;
using NUnit.Framework;


namespace DigitalRune.Tests
{
  [TestFixture]
  public class SingletonTest
  {
    public class LogA : Singleton<LogA>
    {      
    }


    public class LogB : Singleton<LogB>
    {
    }


    public class LogC : Singleton<LogC>
    {
    }


    public class InvalidType : Singleton<LogA>
    {      
    }


    [Test]
    public void ShouldAutomaticallyCreateInstance()
    {
      LogA log1 = LogA.Instance;
      LogA log2 = LogA.Instance;

      Assert.IsNotNull(log1);
      Assert.IsNotNull(log2);
      Assert.AreSame(log1, log2);
    }


    [Test]
    public void ConstructorShouldSetInstance()
    {
      LogB log = new LogB();
      Assert.AreSame(LogB.Instance, log);
      Assert.AreSame(Singleton<LogB>.Instance, log);
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ShouldThrowWhenTypeInstantiatedTwice()
    {
      LogC log1 = new LogC();
      LogC log2 = new LogC();
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ShouldThrowWhenTypeIsWrong()
    {
      InvalidType invalidType = new InvalidType();
    }
  }
}
