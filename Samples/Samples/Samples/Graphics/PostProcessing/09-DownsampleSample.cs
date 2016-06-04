#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses a DownsampleFilter processor to downsample the image to a lower resolution.",
    "",
    39)]
  public class DownsampleSample : PostProcessingSample
  {
    private readonly DownsampleFilter _downsampleFilter;
    private readonly CopyFilter _copyFilter;


    public DownsampleSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Create a DownsampleFilter which downsamples the current screen to a 64x64 pixel buffer.
      _downsampleFilter = new DownsampleFilter(GraphicsService)
      {
        // The PostProcessorChain reads the DefaultTargetFormat property. Here,
        // we can define the downsampled resolution.
        DefaultTargetFormat = new RenderTargetFormat(64, 64, false, SurfaceFormat.Color, DepthFormat.None),
      };
      GraphicsScreen.PostProcessors.Add(_downsampleFilter);

      // This CopyFilter copies the downsampled image to the back buffer.
      _copyFilter = new CopyFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_copyFilter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change downsample factor.
      if (InputService.IsPressed(Keys.D1, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        var currentDownsampledSize = _downsampleFilter.DefaultTargetFormat.Width.Value;
        if (isShiftDown)
          currentDownsampledSize = Math.Min(4096, currentDownsampledSize * 2);
        else
          currentDownsampledSize = Math.Max(1, currentDownsampledSize / 2);

        _downsampleFilter.DefaultTargetFormat = new RenderTargetFormat(
          currentDownsampledSize, currentDownsampledSize, false, SurfaceFormat.Color, DepthFormat.None);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nPress <1> or <Shift>+<1> to decrease or increase the downsample resolution: "
        + _downsampleFilter.DefaultTargetFormat.Width);
    }
  }
}
#endif