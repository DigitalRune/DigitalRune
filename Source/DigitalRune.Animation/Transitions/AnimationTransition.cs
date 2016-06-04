// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Animation.Easing;


namespace DigitalRune.Animation.Transitions
{
  /// <summary>
  /// Controls how animations interact with any existing ones as they are added
  /// to or removed from the animation system.
  /// </summary>
  public abstract class AnimationTransition
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The default easing function used by fade-in/out transitions.
    /// </summary>
    internal static readonly IEasingFunction DefaultEase = new HermiteEase { Mode = EasingMode.EaseInOut };
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the animation instance that is controlled by this animation transition.
    /// </summary>
    /// <value>The animation instance that is controlled by this animation transition.</value>
    protected internal AnimationInstance AnimationInstance { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes the animation transition.
    /// </summary>
    /// <param name="animationManager">The <see cref="AnimationManager"/>.</param>
    /// <remarks>
    /// <see cref="Initialize"/> is called when the transition is added to the animation system.
    /// </remarks>
    internal void Initialize(AnimationManager animationManager)
    {
      OnInitialize(animationManager);
    }


    /// <summary>
    /// Updates the animation transition.
    /// </summary>
    /// <param name="animationManager">The <see cref="AnimationManager"/>.</param>
    internal void Update(AnimationManager animationManager)
    {
      OnUpdate(animationManager);
    }


    /// <summary>
    /// Un-initializes the animation transition.
    /// </summary>
    /// <param name="animationManager">The <see cref="AnimationManager"/>.</param>
    /// <remarks>
    /// <see cref="Initialize"/> is called when the transition is removed from the animation system.
    /// </remarks>
    internal void Uninitialize(AnimationManager animationManager)
    {
      OnUninitialize(animationManager);
    }


    /// <summary>
    /// Called when the animation transition is started.
    /// </summary>
    /// <param name="animationManager">The <see cref="AnimationManager"/>.</param>
    protected virtual void OnInitialize(AnimationManager animationManager)
    {
    }


    /// <summary>
    /// Called when the animation transition is updated.
    /// </summary>
    /// <param name="animationManager">The <see cref="AnimationManager"/>.</param>
    protected virtual void OnUpdate(AnimationManager animationManager)
    {
    }


    /// <summary>
    /// Called when the animation transition is removed.
    /// </summary>
    /// <param name="animationManager">The <see cref="AnimationManager"/>.</param>
    protected virtual void OnUninitialize(AnimationManager animationManager)
    {
    }
    #endregion
  }
}
