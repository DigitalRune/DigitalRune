using DigitalRune.Particles;


namespace Samples.Particles
{
  public static class ParticleHelper
  {
    public static int CountNumberOfParticles(ParticleSystem particleSystem)
    {
      int count = 0;
      count += particleSystem.NumberOfLivingParticles;
      if (particleSystem.Children != null)
        count += CountNumberOfParticles(particleSystem.Children);

      return count;
    }


    public static int CountNumberOfParticles(ParticleSystemCollection particleSystems)
    {
      int count = 0;
      foreach (var particleSystem in particleSystems)
      {
        count += particleSystem.NumberOfLivingParticles;

        if (particleSystem.Children != null)
          count += CountNumberOfParticles(particleSystem.Children);
      }
      return count;
    }
  }
}
