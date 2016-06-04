using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Traits;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace Samples.Animation
{
  // A custom animation derived from Animation<T> base class. This animation animates
  // a Vector2F value and creates a circle movement.
  public class MyCircleAnimation : Animation<Vector2F>
  {
    // Traits tell the animation system how to create/recycle/add/blend/etc. the animated
    // value type. Trait classes are usually singletons.
    public override IAnimationValueTraits<Vector2F> Traits
    {
      get { return Vector2FTraits.Instance; }
    }


    // This animation goes on forever.
    public override TimeSpan GetTotalDuration()
    {
      return TimeSpan.MaxValue;
    }


    // Compute the animation value for the given time and stores it in result. 
    // This animation does not need defaultSource and defaultTarget parameters.
    protected override void GetValueCore(TimeSpan time, ref Vector2F defaultSource, ref Vector2F defaultTarget, ref Vector2F result)
    {
      const float circlePeriod = 4.0f;
      float angle = (float)time.TotalSeconds / circlePeriod * ConstantsF.TwoPi;

      Matrix22F rotation = Matrix22F.CreateRotation(angle);
      Vector2F offset = new Vector2F(200, 0);
      Vector2F rotatedOffset = rotation * offset;

      result.X = 500 + rotatedOffset.X;
      result.Y = 350 + rotatedOffset.Y;
    }
  }
}
