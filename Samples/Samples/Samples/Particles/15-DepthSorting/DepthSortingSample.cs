using DigitalRune.Diagnostics;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Particles
{
  [Sample(SampleCategory.Particles,
    @"This sample demonstrates per-particle depth-sorting between several particle systems.",
    @"A smoke effect on a ring is created. Inside this ring is another smoke effect. If 
depth-sorting is disabled, the back particles can be rendered in front of other particles 
and this does not look good.",
    15)]
  [Controls(@"Sample
  Hold <Left Mouse> or <Right Trigger> to disable depth-sorting.")]
  public class DepthSortingSample : ParticleSample
  {
    private readonly BrownOut _brownOut;
    private readonly ParticleSystemNode _particleSystemNode;


    public DepthSortingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _brownOut = new BrownOut(ContentManager);
      ParticleSystemService.ParticleSystems.Add(_brownOut);

      _particleSystemNode = new ParticleSystemNode(_brownOut);
      GraphicsScreen.Scene.Children.Add(_particleSystemNode);
    }


    public override void Update(GameTime gameTime)
    {
      bool wasDepthSorted = _brownOut.IsDepthSorted;
      _brownOut.IsDepthSorted = !(InputService.IsDown(MouseButtons.Left) || InputService.IsDown(Buttons.RightTrigger, LogicalPlayerIndex.One));
      if (wasDepthSorted != _brownOut.IsDepthSorted)
      {
        // DigitalRune Graphics caches states like IsDepthSorted. To delete the cached data,
        // we can delete the current ParticleSystem.RenderData.
        _brownOut.RenderData = null;
        foreach (var child in _brownOut.Children)
          child.RenderData = null;
      }

      // Synchronize particles <-> graphics.
      _particleSystemNode.Synchronize(GraphicsService);

      Profiler.AddValue("ParticleCount", ParticleHelper.CountNumberOfParticles(ParticleSystemService.ParticleSystems));
    }
  }
}
