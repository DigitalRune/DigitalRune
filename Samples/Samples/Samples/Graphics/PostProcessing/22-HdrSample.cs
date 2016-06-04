#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample uses the HdrFilter to create bloom and dynamic luminance adaption.",
    @"Please note that the render pipeline implemented by SceneGraphicsScreen does not produce HDR 
color values - therefore it does not make much sense to use the HdrFilter and the image quality 
does not look good. To see the HdrFilter with a HDR render pipeline have a look at the 
DeferredLightingSample.",
    52)]
  public class HdrSample : PostProcessingSample
  {
    private readonly HdrFilter _hdrFilter;


    public HdrSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // HDR scenes usually use floating-point render targets. Here, we simply render the current
      // LDR scene into a floating-point render target and scale all color values.
      var graphicsDevice = GraphicsService.GraphicsDevice;
      int width = graphicsDevice.PresentationParameters.BackBufferWidth;
      int height = graphicsDevice.PresentationParameters.BackBufferHeight;

      // The MadFilter can be used to scale the current color values.
      var madProcessor = new MadFilter(GraphicsService)
      {
        Scale = new Vector3F(4),
        DefaultTargetFormat = new RenderTargetFormat(width, height, false, SurfaceFormat.HdrBlendable, DepthFormat.None),
      };
      GraphicsScreen.PostProcessors.Add(madProcessor);

      // The HdrFilter processes the floating-point render target.
      _hdrFilter = new HdrFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_hdrFilter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change downsample factor.
      if (InputService.IsPressed(Keys.D1, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _hdrFilter.DownsampleFactor++;
        else
          _hdrFilter.DownsampleFactor = Math.Max(1, _hdrFilter.DownsampleFactor - 1);
      }

      // <2> / <Shift> + <2> --> Change bloom intensity.
      if (InputService.IsDown(Keys.D2))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _hdrFilter.BloomIntensity *= (float)Math.Pow(factor, time * 60);
      }

      // <3> / <Shift> + <3> --> Change bloom threshold.
      if (InputService.IsDown(Keys.D3))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _hdrFilter.BloomThreshold *= (float)Math.Pow(factor, time * 60);
      }

      // <4> --> Toggle geometric mean.
      if (InputService.IsPressed(Keys.D4, false))
        _hdrFilter.UseGeometricMean = !_hdrFilter.UseGeometricMean;

      // <5> --> Toggle adaption.
      if (InputService.IsPressed(Keys.D5, false))
        _hdrFilter.UseAdaption = !_hdrFilter.UseAdaption;

      // <6> / <Shift> + <6> --> Change adaption speed.
      if (InputService.IsDown(Keys.D6))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _hdrFilter.AdaptionSpeed *= (float)Math.Pow(factor, time * 60);
      }

      // <7> / <Shift> + <7> --> Change middle gray value.
      if (InputService.IsDown(Keys.D7))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _hdrFilter.MiddleGray *= (float)Math.Pow(factor, time * 60);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nPress <1> or <Shift>+<1> to decrease or increase the downsample factor: " + _hdrFilter.DownsampleFactor
        + "\nHold <2> or <Shift>+<2> to decrease or increase the bloom intensity: " + _hdrFilter.BloomIntensity
        + "\nHold <3> or <Shift>+<3> to decrease or increase the bloom threshold: " + _hdrFilter.BloomThreshold
        + "\nPress <4> to toggle geometric mean for luminance computation: " + _hdrFilter.UseGeometricMean
        + "\nPress <5> to toggle dynamic eye adaption: " + _hdrFilter.UseAdaption
        + "\nHold <6> or <Shift>+<6> to decrease or increase adaption speed: " + _hdrFilter.AdaptionSpeed
        + "\nHold <7> or <Shift>+<7> to decrease or increase the middle gray value: " + _hdrFilter.MiddleGray);
    }
  }
}
#endif