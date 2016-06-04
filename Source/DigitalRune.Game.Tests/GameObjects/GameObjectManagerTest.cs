using System;
using NUnit.Framework;


namespace DigitalRune.Game.Tests.GameObjects
{
  [TestFixture]
  public class GameObjectManagerTest
  {
    [Test]
    public void Name()
    {
      GameObjectManager m = new GameObjectManager();

      var a = new GameObject();
      // game object always have a default name.
      Assert.IsTrue(!string.IsNullOrEmpty(a.Name));

      a.Name = "A";
      m.Objects.Add(a);
      
      // Cannot change name of a game object that is already in a colleciton.
      Assert.Throws(typeof(InvalidOperationException), () => a.Name = "B");

      // Cannot use "" or null for name.
      var b = new GameObject();
      b.Name = "";
      Assert.Throws(typeof(ArgumentException), () => m.Objects.Add(b));
      b.Name = null;
      Assert.Throws(typeof(ArgumentException), () => m.Objects.Add(b));

      // Cannot use duplicate name.
      var c = new GameObject();
      c.Name = a.Name;
      Assert.Throws(typeof(ArgumentException), () => m.Objects.Add(c));
    }


    [Test]
    public void IsLoaded()
    {
      GameObjectManager m = new GameObjectManager();

      var a = new MyGameObject { Name = "A" };
      var aLoadCount = a.Properties.Get<int>("LoadCount");
      var aUnLoadCount = a.Properties.Get<int>("UnLoadCount");

      Assert.IsFalse(a.IsLoaded);
      Assert.AreEqual(0, aLoadCount.Value);
      Assert.AreEqual(0, aUnLoadCount.Value);

      m.Objects.Add(a);

      Assert.IsTrue(a.IsLoaded);
      Assert.AreEqual(1, aLoadCount.Value);
      Assert.AreEqual(0, aUnLoadCount.Value);

      a.Load();  // Does nothing.
      Assert.AreEqual(1, aLoadCount.Value);
      Assert.AreEqual(0, aUnLoadCount.Value);

      m.Objects.Remove(a);

      Assert.IsFalse(a.IsLoaded);
      Assert.AreEqual(1, aUnLoadCount.Value);

      a.Unload(); // Does nothing.
      Assert.AreEqual(1, aUnLoadCount.Value);
    }


    [Test]
    public void Update()
    {
      GameObjectManager m = new GameObjectManager();

      m.Update(TimeSpan.FromSeconds(-0.1));
      m.Update(TimeSpan.FromSeconds(0.1));

      var a = new MyGameObject { Name = "A" };
      var aUpdateCount = a.Properties.Get<int>("UpdateCount");

      m.Objects.Add(a);
      Assert.AreEqual(0, aUpdateCount.Value);

      m.Update(TimeSpan.FromSeconds(0.1));
      Assert.AreEqual(1, aUpdateCount.Value);

      m.Update(TimeSpan.FromSeconds(0.1));
      Assert.AreEqual(2, aUpdateCount.Value);

      a.Update(TimeSpan.FromSeconds(0.1));  // Is ignored because NewFrame was not called.
      Assert.AreEqual(2, aUpdateCount.Value);
    }


    private class MyGameObject : GameObject
    {
      static MyGameObject()
      {
        CreateProperty("LoadCount", null, null, 0);
        CreateProperty("UnLoadCount", null, null, 0);
        CreateProperty("UpdateCount", null, null, 0);
      }

      protected override void OnLoad()
      {
        var p = Properties.Get<int>("LoadCount");
        p.Value++;
        base.OnLoad();
      }

      protected override void OnUnload()
      {
        var p = Properties.Get<int>("UnLoadCount");
        p.Value++;
        base.OnUnload();
      }

      protected override void OnUpdate(TimeSpan deltaTime)
      {
        var p = Properties.Get<int>("UpdateCount");
        p.Value++;
        base.OnUpdate(deltaTime);
      }
    }
  }
}
