#if !MONOGAME && !WP7 && !WP8
// TODO: Add annotation support to MonoGame.

using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // Stores information of a technique which use the RepeatParameter annotation.
  public class RepeatTechniqueDescription : EffectTechniqueDescription
  {
    // An array with one entry per effect pass. The entry is null if the 
    // pass does not need to be repeated. If the pass needs to be repeated, the 
    // entry is the effect parameter that determines repetition count.
    internal readonly EffectParameter[] RepeatParameters;

    public RepeatTechniqueDescription(Effect effect, EffectTechnique technique)
      : base(effect, technique)
    {
      // Store the repeat parameters for each pass in an array.
      RepeatParameters = new EffectParameter[technique.Passes.Count];
      for (int i = 0; i < technique.Passes.Count; i++)
      {
        var pass = technique.Passes[i];
        foreach (var annotation in pass.Annotations)
        {
          if (annotation.Name == "RepeatParameter")
          {
            var repeatParameter = annotation.GetValueString();
            RepeatParameters[i] = effect.Parameters[repeatParameter];
          }
        }
      }
    }
  }
}
#endif