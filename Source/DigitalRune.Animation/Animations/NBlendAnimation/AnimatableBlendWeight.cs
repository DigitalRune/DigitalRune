// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Represents a blend weight of an animation in a <see cref="BlendGroup"/>.
  /// </summary>
  internal sealed class AnimatableBlendWeight : IAnimatableProperty<float>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly BlendGroup _blendGroup;
    private float _baseValue;
    private float _animationValue;
    private bool _isAnimated;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    #region ----- IAnimatableProperty -----

    /// <inheritdoc/>
    bool IAnimatableProperty.HasBaseValue
    {
      get { return true; }
    }


    /// <inheritdoc/>
    object IAnimatableProperty.BaseValue
    {
      get { return _baseValue; }
    }


    /// <inheritdoc/>
    bool IAnimatableProperty.IsAnimated
    {
      get { return _isAnimated; }
      set
      {
        if (_isAnimated == value)
          return;

        _isAnimated = value;
        _blendGroup.OnWeightChanged();
      }
    }


    /// <inheritdoc/>
    object IAnimatableProperty.AnimationValue
    {
      get { return _animationValue; }
    }
    #endregion


    #region ----- IAnimatableProperty<T> -----

    /// <inheritdoc/>
    float IAnimatableProperty<float>.BaseValue
    {
      get { return _baseValue; }
    }


    /// <inheritdoc/>
    float IAnimatableProperty<float>.AnimationValue
    {
      get { return _animationValue; }
      set
      {
        if (_animationValue == value)
          return;

        _animationValue = value;
        if (_isAnimated)
        {
          _blendGroup.OnWeightChanged();
        }
      }
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
    public float Value
    {
      get { return _isAnimated ? _animationValue : _baseValue; }
      set
      {
        if (_baseValue == value)
          return;

        _baseValue = value;
        if (!_isAnimated)
        {
          _blendGroup.OnWeightChanged();
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatableBlendWeight"/> class.
    /// </summary>
    /// <param name="blendGroup">The blend group.</param>
    /// <param name="weight">The blend weight.</param>
    public AnimatableBlendWeight(BlendGroup blendGroup, float weight)
    {
      Debug.Assert(blendGroup != null, "BlendGroup is null.");
      _blendGroup = blendGroup;
      _baseValue = weight;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
