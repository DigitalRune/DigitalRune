#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses edge detection to create outlines.",
    "",
    36)]
  public class EdgeFilterSample : PostProcessingSample
  {
    private readonly Vector4F[] _edgeColors =
    {
      new Vector4F(0, 0, 0, 1),          // Black
      new Vector4F(0.5f, 0.5f, 0.5f, 1), // Gray
      new Vector4F(1, 1, 1, 1),          // White
      new Vector4F(1, 0, 0, 1),          // Red
      new Vector4F(0, 1, 0, 1),          // Green
      new Vector4F(0, 0, 1, 1)           // Blue
    };
    private readonly EdgeFilter _edgeFilter;
    private int _silhouetteColorIndex;
    private int _creaseColorIndex;


    public EdgeFilterSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _edgeFilter = new EdgeFilter(GraphicsService)
      {
        SilhouetteColor = new Vector4F(0, 0, 0, 1),
        CreaseColor = new Vector4F(0, 0, 0, 1)
      };
      GraphicsScreen.PostProcessors.Add(_edgeFilter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change edge width.
      if (InputService.IsPressed(Keys.D1, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _edgeFilter.EdgeWidth = _edgeFilter.EdgeWidth + 1;
        else
          _edgeFilter.EdgeWidth = Math.Max(1, _edgeFilter.EdgeWidth - 1);
      }

      // <2> / <Shift> + <2> --> Change silhouette color.
      if (InputService.IsPressed(Keys.D2, true))
      {
        _silhouetteColorIndex = (_silhouetteColorIndex + 1) % _edgeColors.Length;
        _edgeFilter.SilhouetteColor = _edgeColors[_silhouetteColorIndex];
      }

      // <3> / <Shift> + <3> --> Change crease color.
      if (InputService.IsPressed(Keys.D3, true))
      {
        _creaseColorIndex = (_creaseColorIndex + 1) % _edgeColors.Length;
        _edgeFilter.CreaseColor = _edgeColors[_creaseColorIndex];
      }

      // <4> / <Shift> + <4> --> Change depth threshold.
      if (InputService.IsDown(Keys.D4))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _edgeFilter.DepthThreshold = MathHelper.Clamp(_edgeFilter.DepthThreshold * (float)Math.Pow(factor, time * 60), 0.001f, 1);
      }

      // <5> / <Shift> + <5> --> Change depth sensitivity.
      if (InputService.IsDown(Keys.D5))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _edgeFilter.DepthSensitivity = MathHelper.Clamp(_edgeFilter.DepthSensitivity * (float)Math.Pow(factor, time * 60), 0.001f, 1000);
      }

      // <6> / <Shift> + <6> --> Change normal threshold.
      if (InputService.IsDown(Keys.D6))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _edgeFilter.NormalThreshold = MathHelper.Clamp(_edgeFilter.NormalThreshold * (float)Math.Pow(factor, time * 60), 0.001f, 2);
      }

      // <7> / <Shift> + <7> --> Change normal sensitivity.
      if (InputService.IsDown(Keys.D7))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _edgeFilter.NormalSensitivity = MathHelper.Clamp(_edgeFilter.NormalSensitivity * (float)Math.Pow(factor, time * 60), 0.01f, 10);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nPress <1> or <Shift>+<1> to decrease or increase the edge width: " + _edgeFilter.EdgeWidth
        + "\nPress <2> or <Shift>+<2> to change the silhouette color. "
        + "\nPress <3> or <Shift>+<3> to change the crease color. "
        + "\nHold <4> to <Shift>+<4> to decrease or increase the depth threshold: " + _edgeFilter.DepthThreshold
        + "\nHold <5> or <Shift>+<5> to decrease or increase the depth sensitivity: " + _edgeFilter.DepthSensitivity
        + "\nHold <6> or <Shift>+<6> to decrease or increase the normal threshold: " + _edgeFilter.NormalThreshold
        + "\nHold <7> or <Shift>+<7> to decrease or increase the normal sensitivity: " + _edgeFilter.NormalSensitivity);
    }
  }
}
#endif