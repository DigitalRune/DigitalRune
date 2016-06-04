#if !MONOGAME && !WP7 && !WP8
// TODO: Add annotation support to MonoGame.

using System;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // Handles the repetition of certain effect passes when objects are rendered.
  public sealed class RepeatTechniqueBinding : EffectTechniqueBinding
  {
    private readonly RepeatTechniqueDescription _description;

    // The index of the current pass.
    private int _passIndex;

    // How often the current pass has been executed.
    private int _actualPassCount;

    // How often the current pass should been executed.
    private int _desiredPassCount;


    public RepeatTechniqueBinding(RepeatTechniqueDescription description)
    {
      _description = description;
    }


    protected override EffectTechniqueBinding CreateInstanceCore()
    {
      return new RepeatTechniqueBinding(_description);
    }


    protected override EffectTechnique OnGetTechnique(Effect effect, RenderContext context)
    {
      _passIndex = 0;
      _actualPassCount = 0;

      // Get the repetition number for the first pass.
      _desiredPassCount = GetPassRepetitionCount(context, 0);

      // We only use the first technique of an effect. Other techniques are ignored.
      return effect.Techniques[0];
    }


    protected override bool OnNextPass(EffectTechnique technique, RenderContext context, ref int index, out EffectPass pass)
    {
      int numberOfPasses = technique.Passes.Count;

      // Progress to next pass when the desired number of repetitions were performed.
      // (Note: We use a while statement and not an if statement, because the desired
      // number of repetitions returned by GetPassRepetitionCount() could theoretically
      // be 0.)
      while (_actualPassCount >= _desiredPassCount)
      {
        // Finished with current pass. Progress to next pass.
        _passIndex++;

        if (_passIndex >= numberOfPasses)
        {
          // Finished: All effect passes have been applied.
          context.PassIndex = -1;
          pass = null;
          return false;
        }

        _actualPassCount = 0;
        _desiredPassCount = GetPassRepetitionCount(context, _passIndex);
      }

      pass = technique.Passes[_passIndex];

      // In the parameter index and context.PassIndex, we store the total number
      // of executed passes.
      context.PassIndex = index;
      index++;

      _actualPassCount++;

      if (index == numberOfPasses - 1 && string.Equals(pass.Name, "Restore", StringComparison.OrdinalIgnoreCase))
      {
        // A last effect pass may be used to restore the default render states without 
        // drawing anything. The effect pass needs to be called "Restore".
        pass.Apply();

        // Finished: All effect passes have been applied.
        context.PassIndex = -1;
        pass = null;
        return false;
      }

      return true;
    }


    private int GetPassRepetitionCount(RenderContext context, int pass)
    {
      var repeatParameter = _description.RepeatParameters[pass];
      if (repeatParameter == null)
        return 1;

      // We assume that the effect parameters which specify the repetition
      // counts are Material parameters.
      // --> We only look for the RepeatParameter in the MaterialBindings.
      // (For Local/PerInstance/PerPass parameters, we would have to check the 
      // MaterialInstanceBindings. For Global parameters, we would have to check 
      // Effect.GetParameterBindings()).
      var parameterBindings = context.MaterialBinding.ParameterBindings;
      return ((EffectParameterBinding<int>)parameterBindings[repeatParameter]).Value;
    }
  }
}
#endif