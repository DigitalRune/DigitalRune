#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses a SaoFilter to add ambient occlusion.",
    @"The SaoFilter implements 'Scalable Ambient Obscurance' a SSAO method with higher quality than
the SSAO algorithm used in the SsaoFilter.",
    58)]
  public class SaoSample : PostProcessingSample
  {
    private readonly SaoFilter _saoFilter;


    public SaoSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _saoFilter = new SaoFilter(GraphicsService)
      {
        MaxOcclusion = 0.5f,
        Radius = 0.2f,
        Bias = 0.0005f,
        CombineWithSource = false,
      };
      GraphicsScreen.PostProcessors.Add(_saoFilter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change strength.
      if (InputService.IsDown(Keys.D1))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _saoFilter.Strength *= (float)Math.Pow(factor, time * 60);
      }

      // <2> / <Shift> + <2> --> Change max occlusion.
      if (InputService.IsDown(Keys.D2))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _saoFilter.MaxOcclusion *= (float)Math.Pow(factor, time * 60);
      }

      // <3> / <Shift> + <3> --> Change radius.
      if (InputService.IsDown(Keys.D3))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _saoFilter.Radius *= (float)Math.Pow(factor, time * 60);
      }

      // <4> / <Shift> + <4> --> Change min bias.
      if (InputService.IsDown(Keys.D4))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _saoFilter.MinBias *= (float)Math.Pow(factor, time * 60);
      }

      // <5> / <Shift> + <5> --> Change depth-dependent bias.
      if (InputService.IsDown(Keys.D5))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _saoFilter.Bias *= (float)Math.Pow(factor, time * 60);
      }

      // <6> / <Shift> + <6> --> Change number of samples.
      if (InputService.IsPressed(Keys.D6, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _saoFilter.NumberOfSamples++;
        else
          _saoFilter.NumberOfSamples = Math.Max(1, _saoFilter.NumberOfSamples - 1);
      }

      // <7> / <Shift> + <7> --> Change blur scale.
      if (InputService.IsDown(Keys.D7))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _saoFilter.BlurScale *= (float)Math.Pow(factor, time * 60);
      }

      // <8> / <Shift> + <8> --> Change edge softness.
      if (InputService.IsDown(Keys.D8))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _saoFilter.EdgeSoftness *= (float)Math.Pow(factor, time * 60);
      }

      // <9> / <Shift> + <9> --> Change max distances.
      if (InputService.IsPressed(Keys.D9, false))
        _saoFilter.CombineWithSource = !_saoFilter.CombineWithSource;

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the AO strength: " + _saoFilter.Strength
        + "\nHold <2> or <Shift>+<2> to decrease or increase the max occlusion: " + _saoFilter.MaxOcclusion
        + "\nHold <3> or <Shift>+<3> to decrease or increase the radius: " + _saoFilter.Radius
        + "\nHold <4> or <Shift>+<4> to decrease or increase the minimal bias: " + _saoFilter.MinBias
        + "\nHold <5> or <Shift>+<5> to decrease or increase the depth-dependent bias: " + _saoFilter.Bias
        + "\nPress <6> or <Shift>+<6> to decrease or increase the number of samples: " + _saoFilter.NumberOfSamples
        + "\nHold <7> or <Shift>+<7> to decrease or increase the blur scale: " + _saoFilter.BlurScale
        + "\nHold <8> or <Shift>+<8> to decrease or increase the edge softness: " + _saoFilter.EdgeSoftness
        + "\nPress <9> to toggle rendering of the AO buffer: " + _saoFilter.CombineWithSource);
    }
  }
}
#endif