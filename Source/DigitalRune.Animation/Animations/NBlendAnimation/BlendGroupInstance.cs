// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Represents an instance of a <see cref="BlendGroup"/>.
  /// </summary>
  internal sealed class BlendGroupInstance : AnimationInstance
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static readonly ResourcePool<BlendGroupInstance> Pool = new ResourcePool<BlendGroupInstance>(
      () => new BlendGroupInstance(),   // Create
      null,                             // Initialize
      null);                            // Uninitialize

    // The previous synchronized duration of the BlendGroup.
    // (TimeSpan.Zero if the animations are not synchronized.)
    private TimeSpan _duration;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Prevents a default instance of the <see cref="BlendGroupInstance"/> class from being 
    /// created.
    /// </summary>
    private BlendGroupInstance()
    {
    }


    /// <summary>
    /// Creates an instance of the <see cref="BlendGroupInstance"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <param name="blendGroup">The <see cref="BlendGroup"/> that should be played back.</param>
    /// <returns>
    /// A new or reusable instance of the <see cref="BlendGroupInstance"/> class.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method tries to obtain a previously recycled instance from a resource pool if resource
    /// pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>). If no
    /// object is available, a new instance is automatically allocated on the heap. 
    /// </para>
    /// <para>
    /// The owner of the object should call <see cref="Recycle"/> when the instance is no longer 
    /// needed.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="blendGroup"/> is <see langword="null"/>.
    /// </exception>
    public static BlendGroupInstance Create(BlendGroup blendGroup)
    {
      var blendGroupInstance = Pool.Obtain();
      blendGroupInstance.Initialize(blendGroup);
      return blendGroupInstance;
    }


    /// <summary>
    /// Recycles this instance of the <see cref="AnimationInstance"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    public override void Recycle()
    {
      Reset();
      _duration = TimeSpan.Zero;

      if (RunCount < int.MaxValue)
        Pool.Recycle(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    internal override void SetTime(TimeSpan? time)
    {
      if (time.HasValue && Time.HasValue)
      {
        TimeSpan previousTime = Time.Value;
        TimeSpan deltaTime = time.Value - previousTime;
        time = ((BlendGroup)Animation).AdjustTimeline(previousTime, ref _duration);
        time += deltaTime;
      }
      else
      {
        _duration = TimeSpan.Zero;
      }

      base.SetTime(time);
    }
    #endregion
  }
}
