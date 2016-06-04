// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Wraps an existing field or property and makes it animatable.
  /// </summary>
  /// <typeparam name="T">The type of the property.</typeparam>
  /// <remarks>
  /// <para>
  /// The animation system can only animate objects of type <see cref="IAnimatableProperty{T}"/>.
  /// Normal CLR field or properties do not implement this interface and can therefore not be 
  /// animated directly. The <see cref="DelegateAnimatableProperty{T}"/> wraps an existing field or
  /// property and makes it accessible for the animation system.
  /// </para>
  /// <para>
  /// The <see cref="DelegateAnimatableProperty{T}"/> requires two callbacks: One that reads the
  /// value (see <see cref="GetValue"/>) and one that writes the value (<see cref="SetValue"/>). The
  /// callbacks can read/write any existing field or property.
  /// </para>
  /// <para>
  /// This type of <see cref="IAnimatableProperty"/> does not have a
  /// <see cref="IAnimatableProperty{T}.BaseValue"/>, which means that it does not support additive
  /// animations or certain types of from/to/by-animations. Use <see cref="AnimatableProperty{T}"/>
  /// instead of <see cref="DelegateAnimatableProperty{T}"/> if this functionality is required.
  /// </para>
  /// <para>
  /// Note: The animation system does not keep animatable properties alive. It automatically 
  /// removes animations if the <see cref="IAnimatableProperty{T}"/> is no longer referenced by 
  /// another object. This means, the created <see cref="IAnimatableProperty{T}"/> needs to be kept
  /// alive as long as the property should be animated.
  /// </para>
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(IsAnimated = {_isAnimated}, Value = {GetValue()})")]
  public class DelegateAnimatableProperty<T> : IAnimatableProperty<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _isAnimated;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the callback that reads the property value.
    /// </summary>
    /// <value>The callback that reads the property value.</value>
    public Func<T> GetValue { get; set; }


    /// <summary>
    /// Gets or sets the callback that writes the property value.
    /// </summary>
    /// <value>The callback that writes the property value.</value>
    public Action<T> SetValue { get; set; }


    #region ----- IAnimatableProperty -----

    /// <summary>
    /// Gets a value indicating whether this property has a base value.
    /// </summary>
    /// <value>Returns always <see langword="false"/>.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IAnimatableProperty.HasBaseValue
    {
      get { return false; }
    }


    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Always throws <see cref="NotImplementedException"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    object IAnimatableProperty.BaseValue
    {
      get
      {
        throw new NotImplementedException(
          "The current IAnimatableProperty does not have a BaseValue. "
          + "Check HasBaseValue before accessing BaseValue!");
      }
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
      get
      {
        return (GetValue != null) ? (object)GetValue() : null;
      }
    }
    #endregion


    #region ----- IAnimatableProperty<T> -----

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Always throws <see cref="NotImplementedException"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    T IAnimatableProperty<T>.BaseValue
    {
      get
      {
        throw new NotImplementedException(
          "The current IAnimatableProperty<T> does not have a BaseValue. "
          + "Check HasBaseValue before accessing BaseValue!");
      }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    T IAnimatableProperty<T>.AnimationValue
    {
      get
      {
        return (GetValue != null) ? GetValue() : default(T);
      }
      set
      {
        if (SetValue != null)
          SetValue(value);
      }
    }
    #endregion

    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateAnimatableProperty{T}"/> class.
    /// </summary>
    /// <param name="getter">A callback that reads the property value.</param>
    /// <param name="setter">A callback that writes the property value.</param>
    public DelegateAnimatableProperty(Func<T> getter, Action<T> setter)
    {
      GetValue = getter;
      SetValue = setter;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
