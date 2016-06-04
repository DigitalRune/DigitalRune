using DigitalRune.Animation;
using DigitalRune.Animation.Traits;


namespace Samples.Animation
{
  // A from/to/by animation for double values.
  // This class implements the Traits property. The rest is done by the base class.
  public class DoubleFromToByAnimation : FromToByAnimation<double>
  {
    public override IAnimationValueTraits<double> Traits
    {
      get { return DoubleTraits.Instance; }
    }
  }
}
