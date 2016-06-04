using DigitalRune.Animation;
using DigitalRune.Animation.Traits;


namespace Samples.Animation
{
  // A key-frame animation for strings.
  // This class implements the Traits property and disables key-frame interpolation. 
  // The rest is done by the base class.
  public class StringKeyFrameAnimation : KeyFrameAnimation<string>
  {
    public override IAnimationValueTraits<string> Traits
    {
      get { return StringTraits.Instance; }
    }


    public StringKeyFrameAnimation()
    {
      EnableInterpolation = false;
    }
  }
}
