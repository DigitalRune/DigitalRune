using DigitalRune;
using DigitalRune.Animation;
using DigitalRune.Animation.Traits;


namespace Samples.Animation
{
  // AnimationValueTraits for type double.
  // Traits tell the animation system how to create/recycle/add/interpolate/blend the animated
  // value type. Trait classes are usually singletons.
  public class DoubleTraits : Singleton<DoubleTraits>, IAnimationValueTraits<double>
  {
    public void Create(ref double reference, out double value)
    {
      value = 0;
    }


    public void Recycle(ref double value)
    {
    }


    public void Copy(ref double source, ref double target)
    {
      target = source;
    }


    public void Set(ref double value, IAnimatableProperty<double> property)
    {
      property.AnimationValue = value;
    }


    public void Reset(IAnimatableProperty<double> property)
    {
    }


    public void SetIdentity(ref double identity)
    {
      identity = 0;
    }


    public void Invert(ref double value, ref double inverse)
    {
      inverse = -value;
    }


    public void Add(ref double value0, ref double value1, ref double result)
    {
      result = value0 + value1;
    }


    public void Multiply(ref double value, int factor, ref double result)
    {
      result = value * factor;
    }


    public void Interpolate(ref double source, ref double target, float parameter, ref double result)
    {
      result = source + (target - source) * parameter;
    }


    public void BeginBlend(ref double value)
    {
      value = 0;
    }


    public void BlendNext(ref double value, ref double nextValue, float normalizedWeight)
    {
      value += normalizedWeight * nextValue;
    }


    public void EndBlend(ref double value)
    {
    }
  }
}
