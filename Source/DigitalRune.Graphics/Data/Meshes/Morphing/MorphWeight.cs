// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Animation;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represent the weight of a morph target that can be animated (no base value).
  /// </summary>
  internal sealed class MorphWeight : IAnimatableProperty<float>
  {
    /// <summary>
    /// Gets or sets the weight of the morph target.
    /// </summary>
    /// <value>The weight of the morph target.</value>
    public float Value { get; set; }


    #region ----- IAnimatableProperty -----

    /// <inheritdoc/>
    bool IAnimatableProperty.HasBaseValue
    {
      get { return false; }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    object IAnimatableProperty.BaseValue
    {
      get
      {
        throw new NotImplementedException(
          "This IAnimatableProperty does not have a BaseValue. "
          + "Check HasBaseValue before accessing BaseValue!");
      }
    }


    /// <inheritdoc/>
    bool IAnimatableProperty.IsAnimated { get; set; }


    /// <inheritdoc/>
    object IAnimatableProperty.AnimationValue
    {
      get { return Value; }
    }
    #endregion


    #region ----- IAnimatableProperty<T> -----

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    float IAnimatableProperty<float>.BaseValue
    {
      get
      {
        throw new NotImplementedException(
          "This IAnimatableProperty does not have a BaseValue. "
          + "Check HasBaseValue before accessing BaseValue!");
      }
    }


    /// <inheritdoc/>
    float IAnimatableProperty<float>.AnimationValue
    {
      get { return Value; }
      set { Value = value; }
    }
    #endregion
  }
}
