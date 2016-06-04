using System;
using DigitalRune.Animation.Traits;


namespace DigitalRune.Animation
{
  // The animation system has been refactored several times. The Helper class provides 
  // extension methods that ensure that the unit tests don't break.
  public static class Helper
  {
    public static T Identity<T>(this IAnimationValueTraits<T> traits)
    {
      T reference = default(T);
      T identity;
      traits.Create(ref reference, out identity);
      traits.SetIdentity(ref identity);
      return identity;
    }


    public static T Inverse<T>(this IAnimationValueTraits<T> traits, T value)
    {
      traits.Invert(ref value, ref value);
      return value;
    }


    public static T Add<T>(this IAnimationValueTraits<T> traits, T value0, T value1)
    {
      traits.Add(ref value0, ref value1, ref value0);
      return value0;
    }


    public static T Multiply<T>(this IAnimationValueTraits<T> traits, T value, int factor)
    {
      traits.Multiply(ref value, factor, ref value);
      return value;
    }


    public static T Interpolate<T>(this IAnimationValueTraits<T> traits, T source, T target, float parameter)
    {
      traits.Interpolate(ref source, ref target, parameter, ref source);
      return source;
    }


    public static T GetValue<T>(this IAnimation<T> animation, TimeSpan time, T defaultSource, T defaultTarget)
    {
      T result = default(T);
      animation.GetValue(time, ref defaultSource, ref defaultTarget, ref result);
      return result;
    }


    public static T GetValue<T>(this AnimationInstance<T> animationInstance, T defaultSource, T defaultTarget)
    {
      T result = default(T);
      animationInstance.GetValue(ref defaultSource, ref defaultTarget, ref result);
      return result;
    }
  }
}
