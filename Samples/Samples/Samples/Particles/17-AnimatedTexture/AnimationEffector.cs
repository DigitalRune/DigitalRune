using System;
using DigitalRune.Particles;


namespace Samples.Particles
{
  // Updates the particle parameter Frame to animate the particle texture.
  public class AnimationEffector : ParticleEffector
  {
    private IParticleParameter<float> _animationTimeParameter;

    public int FramesPerSecond { get; set; }
    public int NumberOfFrames { get; set; }

    [ParticleParameter(ParticleParameterUsage.InOut)]
    public string AnimationTimeParameter { get; set; }


    protected override ParticleEffector CreateInstanceCore()
    {
      return new AnimationEffector();
    }


    protected override void CloneCore(ParticleEffector source)
    {
      base.CloneCore(source);

      var sourceTyped = (AnimationEffector)source;
      AnimationTimeParameter = sourceTyped.AnimationTimeParameter;
    }


    protected override void OnRequeryParameters()
    {
      _animationTimeParameter = ParticleSystem.Parameters.Get<float>(AnimationTimeParameter);
    }


    protected override void OnUninitialize()
    {
      _animationTimeParameter = null;
    }


    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      if (_animationTimeParameter == null)
        return;

      float[] animationTimes = _animationTimeParameter.Values;
      if (animationTimes == null)
      {
        // This effector only works with varying parameters.
        return;
      }

      // The parameter "AnimationTime" stores the normalized animation time where
      // 0 = first animation frame, 1 ... last animation frame.
      float cycleDuration = (float)NumberOfFrames / FramesPerSecond;
      float increment = (float)deltaTime.TotalSeconds / cycleDuration;

      // Update the AnimationTime to cycle through the frames with 
      // a constant frame rate.
      for (int i = startIndex; i < startIndex + count; i++)
        animationTimes[i] = (animationTimes[i] + increment) % 1;
    }
  }
}
