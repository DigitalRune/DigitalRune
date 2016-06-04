// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Represents a property that can be animated. (Default implementation.)
  /// </summary>
  /// <typeparam name="T">The type of the property.</typeparam>
  /// <remarks>
  /// <para>
  /// An <see cref="AnimatableProperty{T}"/> represents a standalone value, which can be animated by
  /// the animation system. (Use <see cref="DelegateAnimatableProperty{T}"/> if you want to animate
  /// an existing field or property instead of creating a standalone value.)
  /// </para>
  /// <para>
  /// <see cref="AnimatableProperty{T}"/> provides a default implementation of the interface 
  /// <see cref="IAnimatableProperty{T}"/> which is required by the animation system. It internally 
  /// stores two values: a <i>base value</i> and an <i>animation value</i>.
  /// </para>
  /// <para>
  /// <strong>Base Value:</strong> The base value is the value of the property that is valid when 
  /// no animations are active. The animation system reads the base value but does not change it.
  /// The base value is used by certain types of animations: For example, additive animations will
  /// add the result of the animations to the base value value. Another example are 
  /// "From-To-Animations": If only the "To" value is defined then the animation will animate from
  /// the base value of the property to the "To" value defined in the animation.
  /// </para>
  /// <para>
  /// <strong>Animation Value:</strong> The animation value of the property is determined by the 
  /// animations that are controlling the property. The properties 
  /// <see cref="IAnimatableProperty.IsAnimated"/> and 
  /// <see cref="IAnimatableProperty{T}.AnimationValue"/> are set by the animations system and 
  /// should be treated as read-only. <see cref="IAnimatableProperty.IsAnimated"/> is 
  /// <see langword="true"/> when an animation is active; <see langword="false"/> indicates that no 
  /// animations are active. In this case the base value is the effective value of the property.
  /// </para>   
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(IsAnimated = {_isAnimated}, Value = {Value})")]
  public class AnimatableProperty<T> : IAnimatableProperty<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private T _baseValue;
    private T _animationValue;
    private bool _isAnimated;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    #region ----- IAnimatableProperty -----

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IAnimatableProperty.HasBaseValue
    {
      get { return true; }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    object IAnimatableProperty.BaseValue
    {
      get { return _baseValue; }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IAnimatableProperty.IsAnimated
    {
      get { return _isAnimated; }
      set { _isAnimated = value; }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    object IAnimatableProperty.AnimationValue
    {
      get { return _animationValue; }
    }
    #endregion


    #region ----- IAnimatableProperty<T> -----

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    T IAnimatableProperty<T>.BaseValue
    {
      get { return _baseValue; }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    T IAnimatableProperty<T>.AnimationValue
    {
      get { return _animationValue; }
      set { _animationValue = value; }
    }
    #endregion


    /// <summary>
    /// Gets or sets the value of the property.
    /// </summary>
    /// <value>The value of the property.</value>
    /// <remarks>
    /// Writing <see cref="Value"/> changes the base value of the property. Reading 
    /// <see cref="Value"/> returns the effective value of the property. (If the property is 
    /// animated, then the animation value is effective value. If no animations are active, then the
    /// base value is the effective value.) 
    /// </remarks>
    public T Value
    {
      get { return _isAnimated ? _animationValue : _baseValue; }
      set { _baseValue = value; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
