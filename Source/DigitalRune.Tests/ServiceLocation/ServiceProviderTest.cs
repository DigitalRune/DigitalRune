//using System;
//using System.Collections;
//using NUnit.Framework;


//namespace DigitalRune.ServiceLocation.Tests
//{
//  [TestFixture]
//  public class ServiceProviderTest
//  {
//    interface IDummyInterface1
//    {
//    }


//    interface IDummyInterface2
//    {
//    }


//    class DummyClass : IDummyInterface1
//    {
//    }


//    class DummySubClass : DummyClass, IDummyInterface2
//    {
//    }


//    [Test]
//    [ExpectedException(typeof(ArgumentNullException))]
//    public void AddServiceException1()
//    {
//      ServiceProvider serviceProvider = new ServiceProvider();
//      serviceProvider.AddService(null, new DummyClass());
//    }


//    [Test]
//    [ExpectedException(typeof(ArgumentNullException))]
//    public void AddServiceException2()
//    {
//      ServiceProvider serviceProvider = new ServiceProvider();
//      serviceProvider.AddService(typeof(DummyClass), null);
//    }


//    [Test]
//    [ExpectedException(typeof(ArgumentException))]
//    public void AddServiceException3()
//    {
//      ServiceProvider serviceProvider = new ServiceProvider();
//      serviceProvider.AddService(typeof(DummyClass), new DummyClass());
//      serviceProvider.AddService(typeof(DummyClass), new DummySubClass());
//    }


//    [Test]
//    [ExpectedException(typeof(ArgumentException))]
//    public void AddServiceException4()
//    {
//      ServiceProvider serviceProvider = new ServiceProvider();
//      serviceProvider.AddService(typeof(DummySubClass), new DummyClass());
//    }


//    [Test]
//    public void AddAndQueryServices()
//    {
//      ServiceProvider serviceProvider = new ServiceProvider();
//      DummyClass service1 = new DummyClass();
//      DummySubClass service2 = new DummySubClass();
//      serviceProvider.AddService(typeof(DummyClass), service1);
//      serviceProvider.AddService(typeof(DummySubClass), service2);

//      object service = serviceProvider.GetService(typeof (object));
//      Assert.IsNull(service);
//      service = serviceProvider.GetService(typeof(DummyClass));
//      Assert.AreSame(service1, service);
//      service = serviceProvider.GetService(typeof(DummySubClass));
//      Assert.AreSame(service2, service);
//    }


//    [Test]
//    [ExpectedException(typeof(ServiceNotFoundException))]
//    public void GetServiceOfTypeException()
//    {
//      ServiceProvider serviceProvider = new ServiceProvider();
//      DummyClass service1 = new DummyClass();
//      DummySubClass service2 = new DummySubClass();
//      serviceProvider.AddService(typeof(DummyClass), service1);
//      serviceProvider.AddService(typeof(DummySubClass), service2);

//      ServiceManager.GetService<object>(serviceProvider);
//    }


//    [Test]
//    public void GetServiceOfType()
//    {
//      ServiceProvider serviceProvider = new ServiceProvider();
//      DummyClass service1 = new DummyClass();
//      DummySubClass service2 = new DummySubClass();
//      serviceProvider.AddService(typeof(DummyClass), service1);
//      serviceProvider.AddService(typeof(DummySubClass), service2);

//      DummyClass dummyClass = ServiceManager.GetService<DummyClass>(serviceProvider);
//      Assert.AreSame(service1, dummyClass);
//      DummySubClass dummySubClass = ServiceManager.GetService<DummySubClass>(serviceProvider);
//      Assert.AreSame(service2, dummySubClass);
//      dummySubClass = ServiceManager.GetService<DummySubClass>(serviceProvider);
//      Assert.AreSame(service2, dummySubClass);
//      dummyClass = ServiceManager.GetService<DummyClass>(serviceProvider);
//      Assert.AreSame(service1, dummyClass);
//    }


//    [Test]
//    public void RemoveServices()
//    {
//      ServiceProvider serviceProvider = new ServiceProvider();
//      DummyClass service1 = new DummyClass();
//      DummySubClass service2 = new DummySubClass();
//      serviceProvider.AddService(typeof(DummyClass), service1);
//      serviceProvider.AddService(typeof(DummySubClass), service2);

//      object service = serviceProvider.GetService(typeof(DummyClass));
//      Assert.AreSame(service1, service);
//      service = serviceProvider.GetService(typeof(DummySubClass));
//      Assert.AreSame(service2, service);

//      bool result = serviceProvider.RemoveService(typeof(DummyClass));

//      Assert.IsTrue(result);
//      service = serviceProvider.GetService(typeof(DummyClass));
//      Assert.IsNull(service);
//      service = serviceProvider.GetService(typeof(DummySubClass));
//      Assert.AreSame(service2, service);

//      result = serviceProvider.RemoveService(typeof(DummyClass));

//      Assert.IsFalse(result);

//      result = serviceProvider.RemoveService(typeof(DummySubClass));
//      Assert.IsTrue(result);
//      service = serviceProvider.GetService(typeof(DummySubClass));
//      Assert.IsNull(service);
//    }


//    [Test]
//    public void AddAndQueryServicesByInterface()
//    {
//      ServiceProvider serviceProvider = new ServiceProvider();
//      DummyClass service1 = new DummyClass();
//      DummySubClass service2 = new DummySubClass();
//      serviceProvider.AddService(typeof(IDummyInterface1), service1);
//      serviceProvider.AddService(typeof(IDummyInterface2), service2);

//      object service = serviceProvider.GetService(typeof(object));
//      Assert.IsNull(service);
//      service = serviceProvider.GetService(typeof(IDummyInterface1));
//      Assert.AreSame(service1, service);
//      service = serviceProvider.GetService(typeof(IDummyInterface2));
//      Assert.AreSame(service2, service);
//    }


//    [Test]
//    public void TryGetService()
//    {
//      ServiceProvider serviceProvider = new ServiceProvider();
//      DummyClass service = new DummyClass();
//      serviceProvider.AddService(typeof(IDummyInterface1), service);

//      bool result;
//      IDummyInterface1 queriedService1;
//      IDummyInterface2 queriedService2;
//      result = ServiceManager.TryGetService(serviceProvider, out queriedService1);
//      Assert.IsTrue(result);
//      Assert.AreSame(service, queriedService1);
//      result = ServiceManager.TryGetService(serviceProvider, out queriedService2);
//      Assert.IsFalse(result);
//      Assert.IsNull(queriedService2);
//    }


//    [Test]
//    public void GetEnumerator()
//    {
//      ServiceProvider serviceProvider = new ServiceProvider();
//      DummyClass service1 = new DummyClass();
//      DummySubClass service2 = new DummySubClass();
//      serviceProvider.AddService(typeof(IDummyInterface1), service1);
//      serviceProvider.AddService(typeof(IDummyInterface2), service2);

//      var enumerator1 = ((IEnumerable)serviceProvider).GetEnumerator();
//      enumerator1.MoveNext();
//      Assert.AreEqual(service1, enumerator1.Current);
//      enumerator1.MoveNext();
//      Assert.AreEqual(service2, enumerator1.Current);
//    }


//    [Test]
//    [ExpectedException(typeof(ServiceNotFoundException))]
//    public void ServiceManagerGetGlobalServicesThrows()
//    {
//      ServiceManager.GetGlobalService<IDummyInterface1>();
//    }


//    [Test]
//    public void TestGlobalServices()
//    {
//      ServiceProvider serviceProvider = new ServiceProvider();
//      DummyClass service1 = new DummyClass();
//      DummySubClass service2 = new DummySubClass();
//      serviceProvider.AddService(typeof(IDummyInterface1), service1);
//      serviceProvider.AddService(typeof(IDummyInterface2), service2);

//      ServiceManager.GlobalServices = serviceProvider;
//      Assert.AreEqual(serviceProvider, ServiceManager.GlobalServices);
//      Assert.AreEqual(service2, ServiceManager.GetGlobalService<IDummyInterface2>());
//    }
//  }
//}
