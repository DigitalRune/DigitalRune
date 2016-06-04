// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Represents a property that can be animated.
  /// </summary>
  /// <remarks>
  /// <para>
  /// An <see cref="IAnimatableProperty"/> is a property of a certain type that can be animated. It
  /// can have two values: a <i>base value</i> and an <i>animation value</i>.
  /// </para>
  /// <para>
  /// <strong>Base Value:</strong> The base value is the value of the property that is valid when 
  /// no animations are active. The base value is optional - not all properties that implement
  /// <see cref="IAnimatableProperty{T}"/> need to have a base value. The properties 
  /// <see cref="HasBaseValue"/> and <see cref="BaseValue"/> need to be set by the object that
  /// implements the interface. The animation system reads the base value but does not change it.
  /// The base value is used by certain types of animations: For example, additive animations will
  /// add the result of the animations to the base value value. Another example are 
  /// "From-To-Animations": If only the "To" value is defined then the animation will animate from
  /// the base value of the property to the "To" value defined in the animation.
  /// </para>
  /// <para>
  /// <strong>Animation Value:</strong> The animation value of the property is determined by the 
  /// animations that are controlling the property. The properties <see cref="IsAnimated"/> and 
  /// <see cref="AnimationValue"/> are set by the animations system and should be treated as
  /// read-only. <see cref="IsAnimated"/> is <see langword="true"/> when an animation is active; 
  /// <see langword="false"/> indicates that no animations are active. In this case the base value,
  /// if available, should be treated as the effective value of the property.
  /// </para>   
  /// </remarks>
  /// <example>
  /// The following examples shows how an <see cref="IAnimatableProperty{T}"/> could be implemented.
  /// <code lang="csharp">
  /// <![CDATA[
  /// public class AnimatableProperty<T> : IAnimatableProperty<T>
  /// {
  ///   private T _baseValue;
  ///   private T _animationValue;
  ///   private bool _isAnimated;
  /// 
  ///   #region ----- IAnimatableProperty -----
  ///   bool IAnimatableProperty.HasBaseValue
  ///   {
  ///     get { return true; }
  ///   }
  /// 
  ///   object IAnimatableProperty.BaseValue
  ///   {
  ///     get { return _baseValue; }
  ///   }
  /// 
  ///   bool IAnimatableProperty.IsAnimated
  ///   {
  ///     get { return _isAnimated; }
  ///     set { _isAnimated = value; }
  ///   }
  /// 
  ///   object IAnimatableProperty.AnimationValue
  ///   {
  ///     get { return _animationValue; }
  ///   }
  ///   #endregion
  /// 
  ///   #region ----- IAnimatableProperty<T> -----
  ///   T IAnimatableProperty<T>.BaseValue
  ///   {
  ///     get { return _baseValue; }
  ///   }
  /// 
  ///   T IAnimatableProperty<T>.AnimationValue
  ///   {
  ///     get { return _animationValue; }
  ///     set { _animationValue = value; }
  ///   }
  ///   #endregion
  /// 
  ///   public T Value
  ///   {
  ///     get { return _isAnimated ? _animationValue : _baseValue; }
  ///     set { _baseValue = value; }
  ///   }
  /// }
  /// ]]>
  /// </code>
  /// <para>
  /// Here is another example showing how a wrapper for existing properties could look like.
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// public class DelegateAnimatableProperty<T> : IAnimatableProperty<T>
  /// {
  ///   private bool _isAnimated;
  ///   private Func<T> _getter;
  ///   private Action<T> _setter;
  /// 
  ///   #region ----- IAnimatableProperty -----
  /// 
  ///   bool IAnimatableProperty.HasBaseValue
  ///   {
  ///     get { return false; }
  ///   }
  /// 
  ///   object IAnimatableProperty.BaseValue
  ///   {
  ///     get { throw new NotImplementedException(); }
  ///   }
  /// 
  ///   bool IAnimatableProperty.IsAnimated
  ///   {
  ///     get { return _isAnimated; }
  ///     set { _isAnimated = value; }
  ///   }
  /// 
  ///   object IAnimatableProperty.AnimationValue
  ///   {
  ///     get { return (object)_getter(); }
  ///   }
  ///   #endregion
  /// 
  ///   #region ----- IAnimatableProperty<T> -----
  ///   
  ///   T IAnimatableProperty<T>.BaseValue
  ///   {
  ///     get { throw new NotImplementedException(); }
  ///   }
  /// 
  ///   T IAnimatableProperty<T>.AnimationValue
  ///   {
  ///     get { return _getter(); }
  ///     set { _setter(value); }
  ///   }
  ///   #endregion
  /// 
  ///   public DelegateAnimatableProperty(Func<T> getter, Action<T> setter)
  ///   {
  ///     if (getter == null)
  ///       throw new ArgumentNullException("getter");
  ///     if (setter == null)
  ///       throw new ArgumentNullException("setter");
  /// 
  ///     _getter = getter;
  ///     _setter = setter;
  ///   }
  /// }
  /// ]]>
  /// </code>
  /// </example>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Animatable")]
  public interface IAnimatableProperty
  {
    /// <summary>
    /// Gets a value indicating whether this property has a base value.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this property has a base value; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    bool HasBaseValue { get; }


    /// <summary>
    /// Gets the base value.
    /// </summary>
    /// <value>The base value.</value>
    object BaseValue { get; }


    /// <summary>
    /// Gets or sets a value indicating whether this property is animated by the animation system.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this property has an animation value; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is set by the animation system if animations are controlling the property.
    /// The result of the animations are stored in <see cref="AnimationValue"/>.
    /// </para>
    /// <para>
    /// <strong>Notes to Implementors:</strong> The property <see cref="IsAnimated"/> is optional. 
    /// It is not strictly required to store the value in derived types. The getter of the property 
    /// should throw a <see cref="NotImplementedException"/> if it is not implemented and the setter
    /// of the property can be a nop ("no operation").
    /// </para>
    /// </remarks>
    bool IsAnimated { get; set; }


    /// <summary>
    /// Gets the animation value.
    /// </summary>
    /// <value>The animation value.</value>
    /// <remarks>
    /// The value is the result of the animations controlling the property. The property is only 
    /// valid if <see cref="IsAnimated"/> is set.
    /// </remarks>
    object AnimationValue { get; }
  }


  /// <summary>
  /// Represents a property of a certain type that can be animated.
  /// </summary>
  /// <typeparam name="T">The type of the property.</typeparam>
  /// <inheritdoc/>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Animatable")]
  public interface IAnimatableProperty<T> : IAnimatableProperty
  {
    /// <summary>
    /// Gets the base value.
    /// </summary>
    /// <value>The base value.</value>
    new T BaseValue { get; }


    /// <summary>
    /// Gets or sets the animation value.
    /// </summary>
    /// <value>The animation value.</value>
    /// <remarks>
    /// The value is the result of the animations running on the property. The property is only 
    /// valid if <see cref="IAnimatableProperty.IsAnimated"/> is set.
    /// </remarks>
    new T AnimationValue { get; set; }
  }
}
