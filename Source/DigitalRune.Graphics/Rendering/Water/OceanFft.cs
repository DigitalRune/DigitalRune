// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using DigitalRune.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Performs a Fast Fourier Transform (FFT) on the source image.
  /// </summary>
  /// <remarks>
  /// This class performs forward or inverse FFT. This could be reused for general, 
  /// water-independent FFT problems. However, in the last pass it combines the iFFT results as
  /// needed for the ocean waves. This step will be different in other applications of FFT.
  /// </remarks>
  internal class OceanFft : IDisposable
  {
    // Note:
    // - We could turn this class into a general FFT post-processor. However, it is not clear if
    //   a post-processor for a single FFT image is needed. The current class performs up to 4
    //   FFTs simultaneously. In the last pass the result is stored in ocean displacement and
    //   normal map. fft

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly RenderTargetBinding[] _renderTargetBindings = new RenderTargetBinding[2];

    private readonly Effect _effect;
    private readonly EffectParameter _parameterSize;
    private readonly EffectParameter _parameterButterflyIndex;
    //private readonly EffectParameter _parameterIsLastPass;
    //private readonly EffectParameter _parameterLastPassScale;  // Only needed for forward FFT.
    private readonly EffectParameter _parameterChoppiness;
    private readonly EffectParameter _parameterButterflyTexture;
    private readonly EffectParameter _parameterSourceTexture0;
    private readonly EffectParameter _parameterSourceTexture1;
    private readonly EffectPass _passFftHorizontal;
    private readonly EffectPass _passFftVertical;
    private readonly EffectPass _passFftDisplacement;
    private readonly EffectPass _passFftNormal;

    // The butterfly lookup textures for a given number of Butterfly passes.
    private Dictionary<int, Texture2D> _forwardButterflyTextures;
    private Dictionary<int, Texture2D> _inverseButterflyTextures;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="OceanFft"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public OceanFft(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Water/Ocean");
      _parameterSize = _effect.Parameters["Size"];
      _parameterButterflyIndex = _effect.Parameters["ButterflyIndex"];
      //_parameterIsLastPass = _effect.Parameters["IsLastPass"];
      //_parameterLastPassScale = _effect.Parameters["LastPassScale"];
      _parameterChoppiness = _effect.Parameters["Choppiness"];
      _parameterButterflyTexture = _effect.Parameters["ButterflyTexture"];
      _parameterSourceTexture0 = _effect.Parameters["SourceTexture0"];
      _parameterSourceTexture1 = _effect.Parameters["SourceTexture1"];
      _passFftHorizontal = _effect.Techniques[0].Passes["FftHorizontal"];
      _passFftVertical = _effect.Techniques[0].Passes["FftVertical"];
      _passFftDisplacement = _effect.Techniques[0].Passes["FinalFftDisplacementPass"];
      _passFftNormal = _effect.Techniques[0].Passes["FinalFftNormalPass"];
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="OceanFft"/> class.
    /// </summary>
    /// <remarks>
    /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in 
    /// <see langword="true"/>, and then suppresses finalization of the instance.
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="OceanFft"/> class
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Dispose managed resources.
        if (_forwardButterflyTextures != null)
          foreach (var texture in _forwardButterflyTextures.Values)
            texture.SafeDispose();

        _forwardButterflyTextures = null;

        if (_inverseButterflyTextures != null)
          foreach (var texture in _inverseButterflyTextures.Values)
            texture.SafeDispose();

        _inverseButterflyTextures = null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    private Texture2D GetButterflyTexture(bool forward, int numberOfButterflyPasses)
    {
      // Lookup textures are cached in dictionary. Try to get one from cache first:
      var dictionary = forward ? _forwardButterflyTextures : _inverseButterflyTextures;

      if (dictionary == null)
        dictionary = new Dictionary<int, Texture2D>();

      Texture2D texture;
      if (dictionary.TryGetValue(numberOfButterflyPasses, out texture))
        return texture;

      // Nothing in cache. Create the texture.

      // The butterfly lookup texture is a 4 channel texture.
      // rg contain bit-reversal scrambling coordinates.
      // ba contain weights.
      // The texture contains 1 row per butterfly pass.
      int size = 1 << numberOfButterflyPasses;

      // Arrays with [numberOfButterflyPasses][2 * size] entries.
      // 2 * size to store real and imaginary value.
      float[][] indices = new float[numberOfButterflyPasses][];
      float[][] weights = new float[numberOfButterflyPasses][];
      for (int i = 0; i < numberOfButterflyPasses; i++)
      {
        indices[i] = new float[2 * size];
        weights[i] = new float[2 * size];
      }

      ComputeButterflyIndices(indices);
      ComputeButterflyWeights(weights, forward);

      texture = new Texture2D(_effect.GraphicsDevice, size, numberOfButterflyPasses, false, SurfaceFormat.Vector4);
      Vector4[] data = new Vector4[size * numberOfButterflyPasses];

      for (int pass = 0; pass < numberOfButterflyPasses; pass++)
      {
        for (int pixel = 0; pixel < size; pixel++)
        {
          // Note: indices are converted to texture coordinates to sample texel centers.
          data[pass * size + pixel] = new Vector4(indices[pass][2 * pixel] / size + 0.5f / size,
                                                  indices[pass][2 * pixel + 1] / size + 0.5f / size,
                                                  weights[pass][2 * pixel],
                                                  weights[pass][2 * pixel + 1]);
        }
      }
      texture.SetData(data);

      // Add to cache.
      dictionary[numberOfButterflyPasses] = texture;

      return texture;
    }


    private static void ComputeButterflyIndices(float[][] indices)
    {
      int numberOfButterFlies = indices.GetLength(0);
      int n = indices[0].Length / 2;
      int offset = 1;
      for (int i = 0; i < numberOfButterFlies; i++)
      {
        n = n >> 1;
        int step = 2 * offset;
        int end = step;
        int start = 0;
        int p = 0;
        for (int j = 0; j < n; j++)
        {
          for (int k = start, l = 0, v = p; k < end; k += 2, l += 2, v++)
          {
            indices[i][k] = v;
            indices[i][k + 1] = v + offset;
            indices[i][l + end] = v;
            indices[i][l + end + 1] = v + offset;
          }

          start += 2 * step;
          end += 2 * step;
          p += step;
        }
        offset = offset << 1;
      }

      ReverseBits(indices);
    }


    // Reverses the bits for the first butterfly pass.
    private static void ReverseBits(float[][] indices)
    {
      int numberOfButterflyPasses = indices.Length;
      int N = indices[0].Length;    // = 2 * size!

      const uint mask = 0x1;

      for (int j = 0; j < N; j++)
      {
        int index = 0x0;
        int temp = (int)indices[0][j];
        for (int i = 0; i < numberOfButterflyPasses; i++)
        {
          int t = (int)(mask & temp);
          index = (index << 1) | t;
          temp = temp >> 1;
        }

        indices[0][j] = index;
      }
    }


    private static void ComputeButterflyWeights(float[][] weights, bool forward)
    {
      int numberOfButterflyPasses = weights.GetLength(0);
      int size = weights[0].Length / 2;

      float sign = forward ? -1 : 1;

      int numberOfIterations = size / 2;
      int numK = 1;
      for (int i = 0; i < numberOfButterflyPasses; i++)
      {
        int start = 0;
        int end = 2 * numK;

        for (int b = 0; b < numberOfIterations; b++)
        {
          int K = 0;
          for (int k = start; k < end; k += 2)
          {
            var angle = sign * 2.0f * ConstantsF.Pi * K * numberOfIterations / size;
            var cos = (float)Math.Cos(angle);
            var sin = (float)Math.Sin(angle);
            weights[i][k] = cos;
            weights[i][k + 1] = sin;
            weights[i][k + 2 * numK] = -cos;
            weights[i][k + 2 * numK + 1] = -sin;
            K++;
          }

          start += 4 * numK;
          end = start + 2 * numK;
        }

        numberOfIterations = numberOfIterations >> 1;
        numK = numK << 1;
      }
    }


    // Perform FFTs. 
    // 4 complex input images: source0.xy, source0.zw, source1.xy, source1.zw
    // 2 targets: target0 = displacement map, target1 = normal map using Color format.
    public void Process(RenderContext context, bool forward, Texture2D source0, Texture2D source1, RenderTarget2D target0, RenderTarget2D target1, float choppiness)
    {
      if (context == null)
        throw new ArgumentNullException("context");
      if (source0 == null)
        throw new ArgumentNullException("source0");
      if (source1 == null)
        throw new ArgumentNullException("source1");

      if (forward)
      {
        // For forward FFT, uncomment the LastPassScale stuff!
        throw new NotImplementedException("Forward FFT not implemented.");
      }

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var renderTargetPool = graphicsService.RenderTargetPool;

      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      graphicsDevice.BlendState = BlendState.Opaque;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.DepthStencilState = DepthStencilState.None;

      int size = source0.Width;
      _parameterSize.SetValue((float)size);

      _parameterChoppiness.SetValue(choppiness);

      int numberOfButterflyPasses = (int)MathHelper.Log2GreaterOrEqual((uint)source0.Width);
      // ReSharper disable once ConditionIsAlwaysTrueOrFalse
      _parameterButterflyTexture.SetValue(GetButterflyTexture(forward, numberOfButterflyPasses));

      var format = new RenderTargetFormat(size, size, false, source0.Format, DepthFormat.None);
      var tempPing0 = renderTargetPool.Obtain2D(format);
      var tempPing1 = renderTargetPool.Obtain2D(format);
      var tempPong0 = renderTargetPool.Obtain2D(format);
      var tempPong1 = renderTargetPool.Obtain2D(format);

      //_parameterIsLastPass.SetValue(false);

      // Perform horizontal and vertical FFT pass.
      for (int i = 0; i < 2; i++)
      {
        //_parameterLastPassScale.SetValue(1);

        // Perform butterfly passes. We ping-pong between two temp targets.
        for (int pass = 0; pass < numberOfButterflyPasses; pass++)
        {
          _parameterButterflyIndex.SetValue(0.5f / numberOfButterflyPasses + (float)pass / numberOfButterflyPasses);

          if (i == 0 && pass == 0)
          {
            // First pass.
            _renderTargetBindings[0] = new RenderTargetBinding(tempPing0);
            _renderTargetBindings[1] = new RenderTargetBinding(tempPing1);
            graphicsDevice.SetRenderTargets(_renderTargetBindings);
            _parameterSourceTexture0.SetValue(source0);
            _parameterSourceTexture1.SetValue(source1);
          }
          else if (i == 1 && pass == numberOfButterflyPasses - 1)
          {
            // Last pass.
            // We have explicit shader passes for the last FFT pass.
            break;

            //_parameterIsLastPass.SetValue(true);
            //if (forward)
            //  _parameterLastPassScale.SetValue(1.0f / size / size);

            //if (_renderTargetBindings[0].RenderTarget == tempPing0)
            //{
            //  _renderTargetBindings[0] = new RenderTargetBinding(target0);
            //  _renderTargetBindings[1] = new RenderTargetBinding(target1);
            //  graphicsDevice.SetRenderTargets(_renderTargetBindings);
            //  _parameterSourceTexture0.SetValue(tempPing0);
            //  _parameterSourceTexture1.SetValue(tempPing1);
            //}
            //else
            //{
            //  _renderTargetBindings[0] = new RenderTargetBinding(target0);
            //  _renderTargetBindings[1] = new RenderTargetBinding(target1);
            //  graphicsDevice.SetRenderTargets(_renderTargetBindings);
            //  _parameterSourceTexture0.SetValue(tempPong0);
            //  _parameterSourceTexture1.SetValue(tempPong1);
            //}
          }
          else
          {
            // Intermediate pass.
            if (_renderTargetBindings[0].RenderTarget == tempPing0)
            {
              _renderTargetBindings[0] = new RenderTargetBinding(tempPong0);
              _renderTargetBindings[1] = new RenderTargetBinding(tempPong1);
              graphicsDevice.SetRenderTargets(_renderTargetBindings);
              _parameterSourceTexture0.SetValue(tempPing0);
              _parameterSourceTexture1.SetValue(tempPing1);
            }
            else
            {
              _renderTargetBindings[0] = new RenderTargetBinding(tempPing0);
              _renderTargetBindings[1] = new RenderTargetBinding(tempPing1);
              graphicsDevice.SetRenderTargets(_renderTargetBindings);
              _parameterSourceTexture0.SetValue(tempPong0);
              _parameterSourceTexture1.SetValue(tempPong1);
            }
          }

          if (i == 0)
            _passFftHorizontal.Apply();
          else
            _passFftVertical.Apply();

          graphicsDevice.DrawFullScreenQuad();
        }
      }

      // Perform final vertical FFT passes. We have to perform them separately
      // because displacement map and normal map usually have different bit depth.
      // Final pass for displacement.
      graphicsDevice.SetRenderTarget(target0);
      if (_renderTargetBindings[1].RenderTarget == tempPing1)
        _parameterSourceTexture0.SetValue(tempPing0);
      else
        _parameterSourceTexture0.SetValue(tempPong0);

      _passFftDisplacement.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // Final pass for normals. 
      graphicsDevice.SetRenderTarget(target1);
      if (_renderTargetBindings[1].RenderTarget == tempPing1)
        _parameterSourceTexture0.SetValue(tempPing1);
      else
        _parameterSourceTexture0.SetValue(tempPong1);

      _passFftNormal.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // Clean up.
      _renderTargetBindings[0] = default(RenderTargetBinding);
      _renderTargetBindings[1] = default(RenderTargetBinding); 
      _parameterButterflyTexture.SetValue((Texture2D)null);
      _parameterSourceTexture0.SetValue((Texture2D)null);
      _parameterSourceTexture1.SetValue((Texture2D)null);

      renderTargetPool.Recycle(tempPing0);
      renderTargetPool.Recycle(tempPing1);
      renderTargetPool.Recycle(tempPong0);
      renderTargetPool.Recycle(tempPong1);

      savedRenderState.Restore();

      // Reset the texture stages. If a floating point texture is set, we get exceptions
      // when a sampler with bilinear filtering is set.
#if !MONOGAME
      graphicsDevice.ResetTextures();
#endif
    }
    #endregion
  }
}
#endif
