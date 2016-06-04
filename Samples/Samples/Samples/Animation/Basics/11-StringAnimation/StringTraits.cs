using System;
using DigitalRune;
using DigitalRune.Animation;
using DigitalRune.Animation.Traits;


namespace Samples.Animation
{
  // AnimationValueTraits for type string.
  // Traits tell the animation system how to create/recycle/add/interpolate/blend the animated
  // value type. Trait classes are usually singletons.
  // Since string instances are not resource-pooled and they cannot be added or interpolated, this
  // implementation is very simple.
  public class StringTraits : Singleton<StringTraits>, IAnimationValueTraits<string>
  {
    // Creates a new string instance and stores it in value.
    public void Create(ref string reference, out string value)
    {
      value = string.Empty;
    }


    // Recycles a string using resource pooling. - Not used.
    public void Recycle(ref string value)
    {
    }


    // Copies source to target.
    public void Copy(ref string source, ref string target)
    {
      target = source;
    }


    // Set an animatable property to the given value.
    public void Set(ref string value, IAnimatableProperty<string> property)
    {
      property.AnimationValue = value;
    }


    // Reset the animation value of the given property.
    public void Reset(IAnimatableProperty<string> property)
    {
    }


    // Following methods are not needed because strings usually cannot be added, 
    // interpolated or blended...

    public void SetIdentity(ref string identity)
    {
      throw new NotSupportedException();
    }


    public void Invert(ref string value, ref string inverse)
    {
      throw new NotSupportedException();
    }


    public void Add(ref string value0, ref string value1, ref string result)
    {
      throw new NotSupportedException();
    }


    public void Multiply(ref string value, int factor, ref string result)
    {
      throw new NotSupportedException();
    }


    public void Interpolate(ref string source, ref string target, float parameter, ref string result)
    {
      throw new NotSupportedException();
    }


    public void BeginBlend(ref string value)
    {
      throw new NotSupportedException();
    }


    public void BlendNext(ref string value, ref string nextValue, float normalizedWeight)
    {
      throw new NotSupportedException();
    }


    public void EndBlend(ref string value)
    {
      throw new NotSupportedException();
    }
  }
}
