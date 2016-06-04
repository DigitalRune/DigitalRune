#if !ANDROID
using System;
using System.Threading;
using NUnit.Framework;

#if WINDOWS_PHONE
using Microsoft.Phone.Reactive;
#else
using System.Reactive.Linq;
using System.Reactive.Subjects;
#endif


namespace DigitalRune.Linq.Tests
{
  [TestFixture]
  public class RxTest
  {
    public void DoAsync(object parameter, Action<object> finishedCallback)
    {
      Observable.ToAsync(() =>
      {
        Thread.Sleep(25);
        finishedCallback(parameter);
      })();
    }


    public IObservable<object> DoAsync(object parameter)
    {
      var subject = new AsyncSubject<object>();
      DoAsync(parameter, result => { subject.OnNext(result); subject.OnCompleted(); });
      return subject;
    }


    /// <summary>
    /// Converts an asynchronous method with callback into a method with an 
    /// <see cref="IObservable{T}"/>.
    /// </summary>
    /// <typeparam name="TIn">The type of the first input parameter.</typeparam>
    /// <typeparam name="TOut">The type of the first output parameter.</typeparam>
    /// <param name="method">
    /// The asynchronous method with a signature of
    ///   <c>void DoSomethingAsync(TIn parameter, Action&lt;TOut&gt; callback)</c>.
    /// </param>
    /// <returns>
    /// The asynchronous method converted to a method that returns an <see cref="IObservable{T}"/>
    /// instead of using a callback.
    /// </returns>
    public Func<TIn, IObservable<TOut>> ToObservable<TIn, TOut>(Action<TIn, Action<TOut>> method)
    {
      return input =>
             {
               var subject = new AsyncSubject<TOut>();
               method(input, output => { subject.OnNext(output); subject.OnCompleted(); });
               return subject;          
             };
    }


    [Test]
    public void AsyncMethodAsObservable()
    {
      object result = null;
      DoAsync("Test").Subscribe(v => result = v);
      Assert.IsNull(result);

      // Wait for result.
      Thread.Sleep(50);
      Assert.AreEqual("Test", result);
    }


    [Test]
    public void ConvertAsyncMethodToObservable()
    {
      object result = null;
      var doAsyncRx = ToObservable<object, object>(DoAsync);
      doAsyncRx("Test").Subscribe(v => result = v);
      Assert.IsNull(result);

      // Wait for result.
      Thread.Sleep(50);
      Assert.AreEqual("Test", result);
    }


    [Test]
    public void ConvertAsyncMethodToObservable2()
    {
      object result = null;
      var doAsyncRx = ToObservable<object, object>(DoAsync);
      var observable = doAsyncRx("Test");

      observable.Subscribe(v => result = v);
      Assert.IsNull(result);

      // Wait for result.
      Thread.Sleep(50);
      Assert.AreEqual("Test", result);

      // Late subscription should receive the same result.
      result = null;
      observable.Subscribe(v => result = v);
      Assert.AreEqual("Test", result);
    }
  }
}
#endif
