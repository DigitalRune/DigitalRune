using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using NUnit.Framework;


namespace DigitalRune.ServiceLocation.Tests
{
  [TestFixture]
  public class ServiceContainerTest
  {
    public interface IService1
    {
    }


    public interface IService2
    {
      IService1 Service1 { get; }
    }


    public interface IService3
    {
      IService1 Service1 { get; }
      IService2 Service2 { get; }
    }


    public class Service1 : IService1
    {
    }


    public class Service2 : IService2
    {
      public IService1 Service1 { get; set; }
    }


    public class Service3 : IService3
    {
      public IService1 Service1 { get; private set; }
      public IService2 Service2 { get; private set; }
      public Service3(IService1 service1, IService2 service2)
      {
        Service1 = service1;
        Service2 = service2;
      }
    }


    public class DisposableService : IService1, IDisposable
    {
      public bool Disposed;
      public void Dispose()
      {
        Disposed = true;
      }
    }


    [Test]
    public void DefaultIocContainerShouldBeEmpty()
    {
      var ioc = new ServiceContainer();
      Assert.AreEqual(3, ioc.Cast<object>().Count()); // By default only ServiceContainer is registered.
    }


    [Test]
    public void ShouldReturnNullIfNotRegistered()
    {
      var ioc = new ServiceContainer();
      var instance = ioc.GetInstance<IService1>();
      Assert.IsNull(instance);

      ioc.Register(typeof(IService1), "NamedService1", typeof(Service1));
      instance = ioc.GetInstance<IService1>();
      Assert.IsNull(instance);
    }


    [Test]
    public void ClearContainer()
    {
      var ioc = new ServiceContainer();
      Assert.AreEqual(3, ioc.Cast<object>().Count()); // By default only ServiceContainer is registered in 3 variants.

      ioc.Clear();
      Assert.AreEqual(0, ioc.Cast<object>().Count());

      ioc.Register(typeof(IService1), null, typeof(Service1));
      Assert.AreEqual(1, ioc.Cast<object>().Count());

      ioc.Clear();
      Assert.AreEqual(0, ioc.Cast<object>().Count());
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RegisterShouldThrowIfInstanceIsNull()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, (Service1)null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RegisterShouldThrowIfServiceTypeIsNull()
    {
      var ioc = new ServiceContainer();
      ioc.Register(null, null, new Service1());
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RegisterShouldThrowIfServiceTypeIsNull2()
    {
      var ioc = new ServiceContainer();
      ioc.Register(null, null, typeof(Service1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RegisterShouldThrowIfServiceTypeIsNull3()
    {
      var ioc = new ServiceContainer();
      ioc.Register(null, null, (Func<ServiceContainer, object>)null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RegisterShouldThrowIfImplementationTypeIsNull()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, (Type)null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RegisterShouldThrowIfHandlerIsNull()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, (Func<ServiceContainer, object>)null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void RegisterShouldThrowIfInstanceIsIncompatible()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, new Service2());
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void RegisterShouldThrowIfTypeIsIncompatible()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, typeof(Service2));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void UnregisterShouldThrowIfServiceTypeIsNull1()
    {
      var ioc = new ServiceContainer();
      ioc.Unregister(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void UnregisterShouldThrowIfServiceTypeIsNull2()
    {
      var ioc = new ServiceContainer();
      ioc.Unregister(null, null);
    }


    [Test]
    public void UnregisterServiceType()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, typeof(Service1));
      ioc.Register(typeof(IService1), "NamedService1", typeof(Service1));
      ioc.Register(typeof(IService2), null, typeof(Service2));
      ioc.Register(typeof(IService2), "NamedService2", typeof(Service2));

      ioc.Unregister(typeof(IService1));
      Assert.IsNull(ioc.GetInstance<IService1>());
      Assert.IsNull(ioc.GetInstance<IService1>("NamedService1"));
      Assert.IsNotNull(ioc.GetInstance<IService2>());
      Assert.IsNotNull(ioc.GetInstance<IService2>("NamedService2"));
    }


    [Test]
    public void UnregisterNamedService()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, typeof(Service1));
      ioc.Register(typeof(IService1), "NamedService1", typeof(Service1));
      ioc.Register(typeof(IService2), null, typeof(Service2));
      ioc.Register(typeof(IService2), "NamedService2", typeof(Service2));

      ioc.Unregister(typeof(IService1), null);
      Assert.IsNull(ioc.GetInstance<IService1>());
      Assert.IsNotNull(ioc.GetInstance<IService1>("NamedService1"));

      ioc.Unregister(typeof(IService2), "NamedService2");
      Assert.IsNotNull(ioc.GetInstance<IService2>());
      Assert.IsNull(ioc.GetInstance<IService2>("NamedService2"));

      ioc.Unregister(typeof(IService1), "NamedService1");
      Assert.IsNull(ioc.GetInstance<IService1>());
      Assert.IsNull(ioc.GetInstance<IService1>("NamedService1"));
    }


    [Test]
    public void RegisterInstance()
    {
      var ioc = new ServiceContainer();
      var instance = new Service1();
      ioc.Register(typeof(IService1), null, instance);
      Assert.AreEqual(instance, ioc.GetInstance<IService1>());
      Assert.AreEqual(instance, ioc.GetInstance<IService1>());
      Assert.AreEqual(instance, ioc.OfType<IService1>().Single());
    }


    [Test]
    public void RegisterInstanceGeneric()
    {
      var ioc = new ServiceContainer();
      var instance = new Service1();
      ioc.Register(typeof(IService1), null, instance);
      Assert.AreEqual(instance, ioc.GetInstance<IService1>());
      Assert.AreEqual(instance, ioc.GetInstance<IService1>());
      Assert.AreEqual(instance, ioc.OfType<IService1>().Single());
    }


    [Test]
    public void RegisterNamedInstance()
    {
      var ioc = new ServiceContainer();
      var instance = new Service1();
      var namedInstance = new Service1();
      ioc.Register(typeof(IService1), null, instance);
      ioc.Register(typeof(IService1), "NamedService1", namedInstance);
      Assert.AreEqual(instance, ioc.GetInstance<IService1>());
      Assert.AreEqual(instance, ioc.GetInstance<IService1>());
      Assert.AreEqual(namedInstance, ioc.GetInstance<IService1>("NamedService1"));
      Assert.AreEqual(namedInstance, ioc.GetInstance<IService1>("NamedService1"));
      Assert.AreEqual(5, ioc.Cast<object>().Count());

      ioc.Register(typeof(IService1), "NamedService1", instance);
      Assert.AreEqual(instance, ioc.GetInstance<IService1>("NamedService1"));
      Assert.AreEqual(5, ioc.Cast<object>().Count());
    }


    [Test]
    public void ReplaceInstance()
    {
      var ioc = new ServiceContainer();
      var instance = new Service1();
      ioc.Register(typeof(IService1), null, instance);
      Assert.AreEqual(instance, ioc.GetInstance<IService1>());
      Assert.AreEqual(4, ioc.Cast<object>().Count());

      instance = new Service1();
      ioc.Register(typeof(IService1), null, instance);
      Assert.AreEqual(instance, ioc.GetInstance<IService1>());
      Assert.AreEqual(4, ioc.Cast<object>().Count());
    }


    [Test]
    public void RegisterType()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, typeof(Service1));
      var instance = ioc.GetInstance<IService1>();
      Assert.IsNotNull(instance);
      Assert.AreEqual(instance, ioc.GetInstance<IService1>());
      Assert.AreEqual(instance, ioc.OfType<IService1>().Single());
    }


    [Test]
    public void RegisterNamedType()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, typeof(Service1));
      ioc.Register(typeof(IService1), "NamedService1", typeof(Service1));
      var instance = ioc.GetInstance<IService1>();
      Assert.IsNotNull(instance);
      Assert.AreEqual(instance, ioc.GetInstance<IService1>());
      var namedInstance = ioc.GetInstance<IService1>("NamedService1");
      Assert.IsNotNull(namedInstance);
      Assert.AreEqual(namedInstance, ioc.GetInstance<IService1>("NamedService1"));
      Assert.AreEqual(5, ioc.Cast<object>().Count());
    }


    [Test]
    public void ReplaceType()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, typeof(Service1));
      var instance = ioc.GetInstance<IService1>();
      Assert.IsNotNull(instance);
      Assert.AreEqual(4, ioc.Cast<object>().Count());

      ioc.Register(typeof(IService1), null, typeof(Service1));
      var newInstance = ioc.GetInstance<IService1>();
      Assert.IsNotNull(newInstance);
      Assert.AreNotEqual(newInstance, instance);
      Assert.AreEqual(4, ioc.Cast<object>().Count());
    }


    [Test]
    public void RegisterNonSharedType()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, typeof(Service1), CreationPolicy.NonShared);
      var firstInstance = ioc.GetInstance<IService1>();
      var secondInstance = ioc.GetInstance<IService1>();
      Assert.IsNotNull(firstInstance);
      Assert.IsNotNull(secondInstance);
      Assert.AreNotSame(firstInstance, secondInstance);
    }


    [Test]
    public void PropertyInjectionTest()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService2), null, typeof(Service2));
      ioc.Register(typeof(IService1), null, typeof(Service1));

      var service2 = ioc.GetInstance<IService2>();
      Assert.IsNull(service2.Service1);

      ioc.ResolveProperties(service2);
      Assert.IsNotNull(service2.Service1);
    }


    [Test]
    public void ConstructorInjectionTest()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, typeof(Service1));
      ioc.Register(typeof(IService2), null, typeof(Service2));
      ioc.Register(typeof(IService3), null, typeof(Service3));
      var instance = ioc.GetInstance<IService3>();

      Assert.IsNotNull(instance);
      Assert.IsNotNull(instance.Service1);
      Assert.IsNotNull(instance.Service2);
    }


    [Test]
    [ExpectedException(typeof(ActivationException))]
    public void ShouldRaiseActivationExceptionIfSomethingGoesWrong()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, c => { throw new Exception("Fail"); });
      ioc.GetInstance(typeof(IService1));
    }


    [Test]
    [ExpectedException(typeof(ActivationException))]
    public void ShouldRaiseActivationExceptionIfSomethingGoesWrong2()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), "NamedService1", c => { throw new Exception("Fail"); });
      ioc.GetAllInstances<IService1>();
    }


    [Test]
    public void GetAllInstances()
    {
      var ioc = new ServiceContainer();
      Assert.AreEqual(0, ioc.GetAllInstances<IService1>().Count());

      ioc.Register(typeof(IService1), null, typeof(Service1));
      ioc.Register(typeof(IService1), "NamedService1", typeof(Service1));
      ioc.Register(typeof(IService1), "NamedService2", typeof(Service1));
      Assert.AreEqual(2, ioc.GetAllInstances<IService1>().Count());
      Assert.IsNotNull(ioc.GetAllInstances<IService1>().ToArray()[0]);
      Assert.IsNotNull(ioc.GetAllInstances<IService1>().ToArray()[1]);
    }


    [Test]
    public void ServiceProviderInterface()
    {
      var ioc = new ServiceContainer();
      ioc.Register(typeof(IService1), null, typeof(Service1));
      Assert.IsNotNull(((IServiceProvider)ioc).GetService(typeof(IService1)));
    }


    [Test]
    public void ResolveFactoryMethod()
    {
      var ioc = new ServiceContainer();
      var namedService1 = new Service1();
      ioc.Register(typeof(IService1), "NamedService1", namedService1);
      ioc.Register(typeof(IService1), null, typeof(Service1));

      // Default service.
      var createService = ioc.GetInstance<Func<IService1>>();
      Assert.NotNull(createService);
      var service = createService();
      Assert.NotNull(service);
      Assert.AreNotEqual(namedService1, service);
      Assert.IsTrue(service is Service1);

      // Named service.
      createService = ioc.GetInstance<Func<IService1>>("NamedService1");
      Assert.NotNull(createService);
      service = createService();
      Assert.AreEqual(namedService1, service);
      Assert.IsTrue(service is Service1);
    }


    [Test]
    public void ResolveList()
    {
      var ioc = new ServiceContainer();

      // Empty list.
      var services = ioc.GetInstance<IEnumerable<IService1>>();
      Assert.NotNull(services);
      Assert.AreEqual(0, services.Count());

      // Unnamed service --> Still empty list.
      ioc.Register(typeof(IService1), null, typeof(Service1));
      services = ioc.GetInstance<IEnumerable<IService1>>();
      Assert.NotNull(services);
      Assert.AreEqual(0, services.Count());

      // List with one entry.
      ioc.Register(typeof(IService1), "NamedService1", typeof(Service1));
      services = ioc.GetInstance<IEnumerable<IService1>>();
      Assert.NotNull(services);

      var array = services.ToArray();
      Assert.AreEqual(1, array.Length);
      Assert.NotNull(array[0]);
      Assert.IsTrue(array[0] is Service1);

      // List with two entries.
      ioc.Register(typeof(IService1), "NamedService2", typeof(Service1));
      services = ioc.GetInstance<IEnumerable<IService1>>();
      Assert.NotNull(services);

      array = services.ToArray();
      Assert.AreEqual(2, array.Length);
      Assert.NotNull(array[0]);
      Assert.IsTrue(array[0] is Service1);
      Assert.NotNull(array[1]);
      Assert.IsTrue(array[1] is Service1);
      Assert.AreNotSame(array[0], array[1]);
    }


    [Test]
    public void ShouldContainSelf()
    {
      var ioc = new ServiceContainer();
      Assert.AreSame(ioc, ioc.Cast<object>().First());
    }


    [Test]
    [ExpectedException(typeof(ObjectDisposedException))]
    public void ShouldThrowIfDisposed()
    {
      var ioc = new ServiceContainer();
      ioc.Dispose();
      ioc.Clear();
    }


    [Test]
    public void CreateChildContainer()
    {
      var ioc = new ServiceContainer();
      Assert.AreSame(ioc, ioc.Cast<object>().First());

      var childContainer = ioc.CreateChildContainer();
      Assert.AreSame(childContainer, childContainer.Cast<object>().First());
    }


    [Test]
    public void ChildContainerShouldBeDisposed()
    {
      var ioc = new ServiceContainer();
      var childContainer = ioc.CreateChildContainer();
      ioc.Dispose();
      Assert.That(() => childContainer.Clear(), Throws.TypeOf(typeof(ObjectDisposedException)));
    }


    [Test]
    public void ComplexHierarchy()
    {
      // Build hierarchy.
      var root = new ServiceContainer();
      var child1 = root.CreateChildContainer();
      child1.Register(typeof(IService1), "token1", typeof(Service1), CreationPolicy.Shared);

      // Register root services after child1 is created to see whether new services
      // are properly inherited.
      root.Register(typeof(IService1), null, typeof(Service1), CreationPolicy.Shared);
      root.Register(typeof(IService2), null, typeof(Service2), CreationPolicy.LocalShared);
      root.Register(typeof(IService3), null, typeof(Service3), CreationPolicy.NonShared);

      var child2 = child1.CreateChildContainer();
      child2.Register(typeof(IService1), null, typeof(Service1), CreationPolicy.Shared);
      child2.Register(typeof(IService1), "token2", typeof(DisposableService), CreationPolicy.Shared);

      // Resolve services.
      var service1InChild2 = child2.GetInstance<IService1>();
      var service2InChild2 = child2.GetInstance<IService2>();
      var service3InChild2 = child2.GetInstance<IService3>();
      var disposableServiceInChild2 = child2.GetInstance<IService1>("token2");

      var service3InChild1 = child1.GetInstance<IService3>();
      var service2InChild1 = child1.GetInstance<IService2>();
      var service1InChild1 = child1.GetInstance<IService1>();
      var namedService1InChild1 = child1.GetInstance<IService1>("token1");
      var namedService1InChild2 = child2.GetInstance<IService1>("token1");

      var service1InRoot = root.GetInstance<IService1>();
      var service3InRoot = root.GetInstance<IService3>();
      var service2InRoot = root.GetInstance<IService2>();

      // Check root services.
      Assert.IsNotNull(service1InRoot);
      Assert.IsNotNull(service2InRoot);
      Assert.IsNotNull(service3InRoot);
      Assert.IsNull(root.GetInstance<IService1>("token1"));
      Assert.AreSame(root, root.GetInstance<ServiceContainer>());
      Assert.AreSame(service1InRoot, root.GetInstance<IService1>());
      Assert.AreSame(service2InRoot, root.GetInstance<IService2>());
      Assert.AreNotSame(service3InRoot, root.GetInstance<IService3>());
      Assert.AreSame(service1InRoot, service3InRoot.Service1);
      Assert.AreSame(service2InRoot, service3InRoot.Service2);

      Assert.IsNull(service2InRoot.Service1);
      root.ResolveProperties(service2InRoot);
      Assert.AreSame(service1InRoot, service2InRoot.Service1);

      // Check child1 services.
      Assert.IsNotNull(service1InChild1);
      Assert.IsNotNull(service2InChild1);
      Assert.IsNotNull(service3InChild1);
      Assert.IsNotNull(namedService1InChild1);
      Assert.AreSame(child1, child1.GetInstance<ServiceContainer>());
      Assert.AreSame(service1InChild1, child1.GetInstance<IService1>());
      Assert.AreSame(service2InChild1, child1.GetInstance<IService2>());
      Assert.AreNotSame(service3InChild1, child1.GetInstance<IService3>());
      Assert.AreSame(service1InChild1, service3InChild1.Service1);
      Assert.AreSame(service2InChild1, service3InChild1.Service2);

      Assert.IsNull(service2InChild1.Service1);
      child1.ResolveProperties(service2InChild1);
      Assert.AreSame(service1InChild1, service2InChild1.Service1);

      Assert.AreSame(service1InRoot, service1InChild1);
      Assert.AreNotSame(service2InRoot, service2InChild1);

      // Check child2 services.
      Assert.IsNotNull(service1InChild2);
      Assert.IsNotNull(service2InChild2);
      Assert.IsNotNull(service3InChild2);
      Assert.IsNotNull(namedService1InChild2);
      Assert.IsNotNull(disposableServiceInChild2);
      Assert.AreSame(child2, child2.GetInstance<ServiceContainer>());
      Assert.AreSame(service1InChild2, child2.GetInstance<IService1>());
      Assert.AreSame(service2InChild2, child2.GetInstance<IService2>());
      Assert.AreNotSame(service3InChild2, child2.GetInstance<IService3>());
      Assert.AreSame(service1InChild2, service3InChild2.Service1);
      Assert.AreSame(service2InChild2, service3InChild2.Service2);

      Assert.IsNull(service2InChild2.Service1);
      child2.ResolveProperties(service2InChild2);
      Assert.AreSame(service1InChild2, service2InChild2.Service1);

      Assert.AreNotSame(service1InRoot, service1InChild2);
      Assert.AreNotSame(service1InChild1, service1InChild2);
      Assert.AreNotSame(service2InRoot, service2InChild2);
      Assert.AreNotSame(service2InChild1, service2InChild2);

      Assert.AreSame(child1.GetInstance<IService1>("token1"), child2.GetInstance<IService1>("token1"));
      Assert.AreSame(disposableServiceInChild2, child2.GetInstance<IService1>("token2"));

      var service1InstancesInChild2 = child2.GetAllInstances<IService1>().ToArray();
      Assert.AreEqual(2, service1InstancesInChild2.Length);
      Assert.IsTrue(service1InstancesInChild2.Contains(namedService1InChild1));
      Assert.IsTrue(service1InstancesInChild2.Contains(disposableServiceInChild2));

      // Unregister local entry.
      child2.Unregister(typeof(IService1));
      Assert.AreSame(service1InRoot, child2.GetInstance<IService1>());

      // Dispose hierarchy.
      root.Dispose();
      Assert.IsTrue(((DisposableService)disposableServiceInChild2).Disposed);
    }


    [Test]
    public void InstanceShouldNotBeDisposed()
    {
      var ioc = new ServiceContainer();

      // DisposalPolicy.Manual
      var disposableService = new DisposableService();
      ioc.Register(typeof(IService1), "Token", disposableService);

      ioc.Dispose();
      Assert.IsFalse(disposableService.Disposed);
    }


    [Test]
    public void TypeShouldBeDisposed()
    {
      var ioc = new ServiceContainer();

      // DisposalPolicy.Automatic
      ioc.Register(typeof(IService1), null, typeof(DisposableService));
      var service = ioc.GetInstance<IService1>();

      ioc.Dispose();
      Assert.IsTrue(((DisposableService)service).Disposed);
    }


    [Test]
    public void DisposalPolicies()
    {
      var ioc = new ServiceContainer();

      // Not disposable.
      ioc.Register(typeof(IService1), null, new Service1());

      // DisposalPolicy.Automatic
      ioc.Register(typeof(IService1), null, typeof(DisposableService));
      var service = ioc.GetInstance<IService1>();

      // DisposalPolicy.Manual
      var disposableService = new DisposableService();
      ioc.Register(typeof(IService1), "Token", disposableService);

      ioc.Dispose();
      Assert.IsFalse(disposableService.Disposed);
      Assert.IsTrue(((DisposableService)service).Disposed);
    }


    [Test]
    public void ShouldNotBeDisposedOnUnregister()
    {
      var ioc = new ServiceContainer();

      ioc.Register(typeof(IService1), null, new DisposableService()); // DisposalPolicy = Manual

      var disposableService = ioc.GetInstance<IService1>() as DisposableService;
      Assert.IsNotNull(disposableService);
      Assert.IsFalse(disposableService.Disposed);

      ioc.Unregister(typeof(IService1));
      Assert.IsFalse(disposableService.Disposed);

      ioc.Register(typeof(IService1), null, typeof(DisposableService), CreationPolicy.NonShared, DisposalPolicy.Manual);

      disposableService = ioc.GetInstance<IService1>() as DisposableService;
      Assert.IsNotNull(disposableService);
      Assert.IsFalse(disposableService.Disposed);

      ioc.Unregister(typeof(IService1));
      Assert.IsFalse(disposableService.Disposed);
    }


    [Test]
    public void ShouldNotBeDisposedOnClear()
    {
      var ioc = new ServiceContainer();

      ioc.Register(typeof(IService1), null, new DisposableService()); // DisposalPolicy = Manual

      var disposableService = ioc.GetInstance<IService1>() as DisposableService;
      Assert.IsNotNull(disposableService);
      Assert.IsFalse(disposableService.Disposed);

      ioc.Clear();
      Assert.IsFalse(disposableService.Disposed);

      ioc.Register(typeof(IService1), null, typeof(DisposableService), CreationPolicy.NonShared, DisposalPolicy.Manual);

      disposableService = ioc.GetInstance<IService1>() as DisposableService;
      Assert.IsNotNull(disposableService);
      Assert.IsFalse(disposableService.Disposed);

      ioc.Clear();
      Assert.IsFalse(disposableService.Disposed);
    }


    [Test]
    public void ShouldBeDisposedOnUnregister()
    {
      var ioc = new ServiceContainer();

      // Shared service.
      ioc.Register(typeof(IService1), null, typeof(DisposableService), CreationPolicy.Shared, DisposalPolicy.Automatic);

      var disposableService = ioc.GetInstance<IService1>() as DisposableService;
      Assert.IsNotNull(disposableService);
      Assert.IsFalse(disposableService.Disposed);

      ioc.Unregister(typeof(IService1));
      Assert.IsTrue(disposableService.Disposed);
      Assert.IsNull(ioc.GetInstance<IService1>());

      // Single non-shared service.
      ioc.Register(typeof(IService1), null, typeof(DisposableService), CreationPolicy.NonShared, DisposalPolicy.Automatic);

      disposableService = ioc.GetInstance<IService1>() as DisposableService;
      Assert.IsNotNull(disposableService);
      Assert.IsFalse(disposableService.Disposed);

      ioc.Unregister(typeof(IService1));
      Assert.IsTrue(disposableService.Disposed);
      Assert.IsNull(ioc.GetInstance<IService1>());

      // Multiple non-shared service.
      ioc.Register(typeof(IService1), null, typeof(DisposableService), CreationPolicy.NonShared, DisposalPolicy.Automatic);

      var disposableService0 = ioc.GetInstance<IService1>() as DisposableService;
      var disposableService1 = ioc.GetInstance<IService1>() as DisposableService;
      var disposableService2 = ioc.GetInstance<IService1>() as DisposableService;
      Assert.IsNotNull(disposableService0);
      Assert.IsNotNull(disposableService1);
      Assert.IsNotNull(disposableService2);
      Assert.AreNotSame(disposableService0, disposableService1);
      Assert.AreNotSame(disposableService0, disposableService2);
      Assert.IsFalse(disposableService0.Disposed);
      Assert.IsFalse(disposableService1.Disposed);
      Assert.IsFalse(disposableService2.Disposed);

      ioc.Unregister(typeof(IService1));
      Assert.IsTrue(disposableService0.Disposed);
      Assert.IsNull(ioc.GetInstance<IService1>());
    }


    [Test]
    public void ShouldBeDisposedOnClear()
    {
      var ioc = new ServiceContainer();

      // Shared service.
      ioc.Register(typeof(IService1), null, typeof(DisposableService), CreationPolicy.Shared, DisposalPolicy.Automatic);

      var disposableService = ioc.GetInstance<IService1>() as DisposableService;
      Assert.IsNotNull(disposableService);
      Assert.IsFalse(disposableService.Disposed);

      ioc.Clear();
      Assert.IsTrue(disposableService.Disposed);
      Assert.IsNull(ioc.GetInstance<IService1>());

      // Single non-shared service.
      ioc.Register(typeof(IService1), null, typeof(DisposableService), CreationPolicy.NonShared, DisposalPolicy.Automatic);

      disposableService = ioc.GetInstance<IService1>() as DisposableService;
      Assert.IsNotNull(disposableService);
      Assert.IsFalse(disposableService.Disposed);

      ioc.Clear();
      Assert.IsTrue(disposableService.Disposed);
      Assert.IsNull(ioc.GetInstance<IService1>());

      // Multiple non-shared service.
      ioc.Register(typeof(IService1), null, typeof(DisposableService), CreationPolicy.NonShared, DisposalPolicy.Automatic);

      var disposableService0 = ioc.GetInstance<IService1>() as DisposableService;
      var disposableService1 = ioc.GetInstance<IService1>() as DisposableService;
      var disposableService2 = ioc.GetInstance<IService1>() as DisposableService;
      Assert.IsNotNull(disposableService0);
      Assert.IsNotNull(disposableService1);
      Assert.IsNotNull(disposableService2);
      Assert.AreNotSame(disposableService0, disposableService1);
      Assert.AreNotSame(disposableService0, disposableService2);
      Assert.IsFalse(disposableService0.Disposed);
      Assert.IsFalse(disposableService1.Disposed);
      Assert.IsFalse(disposableService2.Disposed);

      ioc.Clear();
      Assert.IsTrue(disposableService0.Disposed);
      Assert.IsNull(ioc.GetInstance<IService1>());
    }
  }
}
