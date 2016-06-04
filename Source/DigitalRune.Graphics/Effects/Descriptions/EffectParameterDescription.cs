// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Describes a parameter of an effect.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="EffectParameterDescription"/>s provide additional information for effect 
  /// parameters. This information is used by the graphics engine to understand effects and apply 
  /// them properly during rendering.
  /// </para>
  /// <para>
  /// The standard semantic (see <see cref="Semantic"/>) is a unique, case-sensitive string (such as 
  /// "World", "Diffuse", "PointLightPosition", etc.) that defines how the parameter should be 
  /// interpreted and used by the engine. User-defined strings can be used, as long as they do not 
  /// conflict with any of the existing semantics. Existing semantics are defined by the following
  /// types: <see cref="DefaultEffectParameterSemantics"/>, 
  /// <see cref="SceneEffectParameterSemantics"/>.
  /// </para>
  /// <para>
  /// The index (see <see cref="Index"/>) defines the object to which the parameter needs to be 
  /// bound if multiple objects of the same type exist. The description (<see cref="Semantic"/> = 
  /// "PointLightPosition", <see cref="Index"/> = 3, <see cref="Hint"/> = 
  /// <see cref="EffectParameterHint.Local"/>) means that the parameter stores the position of 
  /// 4<sup>th</sup> point light near the object that is being rendered.
  /// </para>
  /// <para>
  /// Additionally, the sort hint (see <see cref="Hint"/>) indicates how the parameter should be 
  /// treated during state sorting.
  /// </para>
  /// <para>
  /// The descriptions are created automatically by effect interpreters (see 
  /// <see cref="IEffectInterpreter"/>) when an effect is initialized. The graphics service manages 
  /// a list of effect interpreters. Custom interpreters can be added to the graphics service to 
  /// support new types of effects.
  /// </para>
  /// </remarks>
  /// <seealso cref="DefaultEffectParameterSemantics"/>
  /// <seealso cref="SceneEffectParameterSemantics"/>
  [DebuggerDisplay("{GetType().Name,nq}(Parameter = {Parameter.Name}, Semantic = {Semantic}, Index = {Index}, Hint = {Hint})")]
  public class EffectParameterDescription
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the standard semantic (case-sensitive).
    /// </summary>
    /// <value>
    /// The standard semantic (case-sensitive). The default value is <see langword="null"/>, which 
    /// means that the meaning of the effect parameter is unknown.
    /// </value>
    /// <remarks>
    /// The standard semantic is a case-sensitive string that defines the purpose of the effect 
    /// parameter: Examples are "World", "Diffuse", "PointLightPosition", etc. 
    /// </remarks>
    /// <seealso cref="DefaultEffectParameterSemantics"/>
    /// <seealso cref="SceneEffectParameterSemantics"/>
    public string Semantic { get; internal set; }


    /// <summary>
    /// Gets the effect parameter.
    /// </summary>
    /// <value>The effect parameter.</value>
    public EffectParameter Parameter { get; private set; }


    /// <summary>
    /// Gets the zero-based index.
    /// </summary>
    /// <value>
    /// The zero-based index. The default value is 0. (Internal: The value may be -1 during 
    /// initialization. -1 indicates that the index is unknown and should be set automatically. When
    /// the index is -1 at runtime, this usually indicates an error and parameter bindings will not 
    /// be applied correctly.)
    /// </value>
    /// <remarks>
    /// The index defines the object to which the parameter is bound if multiple objects of the same
    /// type exist. Example: The description (<see cref="Semantic"/> = "PointLightPosition", 
    /// <see cref="Index"/> = 3, <see cref="Hint"/> = <see cref="EffectParameterHint.Local"/>) means
    /// that the parameter stores the position of 4<sup>th</sup> point light near the object that is 
    /// being rendered.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is out of range. Allowed values are -1, 0, and positive numbers.
    /// </exception>
    public int Index
    {
      get { return _index; }
      internal set
      {
        if (value < -1)
          throw new ArgumentOutOfRangeException("value", "Usage index of effect parameter must be -1, 0, or a positive number.");

        _index = value;
      }
    }
    private int _index;


    /// <summary>
    /// Gets a value indicating how the effect parameter should be treated during state sorting.
    /// </summary>
    /// <value>
    /// A value indicating how the effect parameter should be treated during state sorting.
    /// </value>
    public EffectParameterHint Hint { get; internal set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterDescription"/> class.
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="semantic">The semantic.</param>
    /// <param name="index">The index.</param>
    /// <param name="hint">
    /// A value indicating how the effect parameter should be treated during state sorting.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Invalid <paramref name="index"/>. Allowed values are -1, 0, and positive numbers.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="hint"/> is invalid.
    /// </exception>
    public EffectParameterDescription(EffectParameter parameter, string semantic, int index, EffectParameterHint hint)
    {
      if (parameter == null)
        throw new ArgumentNullException("parameter");
      if (index < -1)
        throw new ArgumentOutOfRangeException("index", "The index of the effect parameter must be -1, 0, or positive.");

      ValidateHint(hint);

      Semantic = semantic;
      Parameter = parameter;
      Index = index;
      Hint = hint;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    [Conditional("DEBUG")]
    private static void ValidateHint(EffectParameterHint hint)
    {
      switch (hint)
      {
        case EffectParameterHint.Global:
        case EffectParameterHint.Material:
        case EffectParameterHint.PerInstance:
        case EffectParameterHint.Local:
        case EffectParameterHint.PerPass:
          return;

        default:
          throw new ArgumentException("Invalid effect parameter hint. Allowed values are Global, Material, Local, PerInstance, and PerPass.", "hint");
      }
    }
    #endregion
  }
}
