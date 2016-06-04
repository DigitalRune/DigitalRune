// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides the logic for rendering a specific 
  /// <see cref="Microsoft.Xna.Framework.Graphics.Effect"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// An <see cref="Microsoft.Xna.Framework.Graphics.Effect"/> defines the render states required to
  /// render an object. An <see cref="EffectBinding"/> provides the logic that is required for using
  /// an effect at runtime.
  /// </para>
  /// <para>
  /// Multiple <see cref="EffectBinding"/>s can share the same <see cref="Effect"/>. An 
  /// <see cref="EffectBinding"/> can in theory be shared by different graphics objects. But in 
  /// most cases an <see cref="EffectBinding"/> belongs to a single object, such as a 
  /// <see cref="Mesh"/>.
  /// </para>
  /// <para>
  /// <strong>Technique and Parameter Bindings:</strong><br/>
  /// An effect may define one or more effect techniques. When rendering a certain object the 
  /// correct technique needs to be chosen. In addition, an effect defines a set of effect 
  /// parameters. An effect file (.fx) can define default values for effect parameters. However, 
  /// most effect parameters need to be set at runtime. Static parameters (such as colors, textures,
  /// etc.) can be set when assets are loaded. Dynamic parameters (such as world matrix, view 
  /// matrix, projection matrix, etc.) need to be updated, typically once per frame, when the 
  /// associated objects are rendered.
  /// </para>
  /// <para>
  /// Effect parameters belong to different categories, defined by 
  /// <see cref="EffectParameterHint"/>. An <see cref="EffectBinding"/> can be used to manage all
  /// kinds of effect parameters, or only parameters of a certain type, e.g. only 
  /// <see cref="EffectParameterHint.Material"/> parameters. See also <see cref="Hints"/>.
  /// </para>
  /// <para>
  /// DigitalRune Graphics introduces the concept of <i>effect technique bindings</i> and <i>effect 
  /// parameter bindings</i>: The technique binding (see property <see cref="TechniqueBinding"/>) 
  /// provides the logic for selecting a technique at runtime. Parameter bindings (stored in 
  /// <see cref="ParameterBindings"/>) links a effect parameters to a certain values. By using a 
  /// <see cref="ConstParameterBinding{T}"/> a parameter can be bound to a static value. By using a 
  /// <see cref="DelegateParameterBinding{T}"/> a parameter can be dynamically updated when needed. 
  /// Effect technique and parameter bindings are evaluated when the associated object (e.g. a mesh)
  /// needs to be rendered. Evaluation consists of two phases:
  /// <list type="number">
  /// <item>
  /// <description>
  /// <strong>Update:</strong> The effect technique is selected; the new value of an effect 
  /// parameter is calculated.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <strong>Apply:</strong> An effect technique is selected for rendering; the new value is 
  /// applied to the effect parameter in <see cref="Effect"/>.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// <strong>Initialization:</strong> When an effect binding is created all technique and parameter
  /// bindings are created automatically. This initialization involves two steps:
  /// <list type="number">
  /// <item>
  /// <description>
  /// <strong>Interpretation: </strong> <see cref="IEffectInterpreter"/>s are used to interpret the
  /// meaning of effect techniques and parameters. An <see cref="IEffectInterpreter"/> returns 
  /// <see cref="EffectTechniqueDescription"/>s and <see cref="EffectParameterDescription"/>s which
  /// indicate how the effect should be used at runtime. This information is stored per effect and 
  /// can also be queried using the methods <see cref="EffectHelper.GetTechniqueDescriptions"/> and 
  /// <see cref="EffectHelper.GetParameterDescriptions"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <strong>Binding of Effect Parameters: </strong> <see cref="IEffectBinder"/>s read the
  /// information provided in the previous step and create an <see cref="EffectTechniqueBinding"/>
  /// for the effect and <see cref="EffectParameterBinding"/>s for all effect parameters.
  /// </description>
  /// </item>
  /// </list>
  /// The <see cref="IEffectInterpreter"/>s and the <see cref="IEffectBinder"/>s are stored in the
  /// <see cref="IGraphicsService"/>. Custom interpreters/binders can be added to support new types
  /// of effects.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// <see cref="EffectBinding"/>s need to be cloneable. The method <see cref="Clone()"/> calls 
  /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/> which are responsible for 
  /// creating a clone of the current instance. Classes that derive from <see cref="EffectBinding"/>
  /// need to provide the implementation for <see cref="CreateInstanceCore"/> and override 
  /// <see cref="CloneCore"/> if necessary.
  /// </para>
  /// <para>
  /// By default, when an <see cref="EffectBinding"/> is cloned all technique and parameter bindings
  /// are duplicated (deep copy).  Any optional object stored in <see cref="UserData"/> is copied 
  /// per reference (shallow copy).
  /// </para>
  /// </remarks>
  public partial class EffectBinding
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>Temporary ID set during rendering.</summary>
    internal uint Id;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the effect.
    /// </summary>
    /// <value>The effect.</value>
    public Effect Effect
    {
      get { return EffectEx.Resource; }
    }


    /// <summary>
    /// Gets the <see cref="EffectEx"/>.
    /// </summary>
    /// <value>The <see cref="EffectEx"/>.</value>
    internal EffectEx EffectEx { get; private set; }


    /// <summary>
    /// Gets the material binding. (Only valid if this is a material instance binding.)
    /// </summary>
    /// <value>The material binding.</value>
    internal EffectBinding MaterialBinding { get; private set; }


    /// <summary>
    /// Gets or sets the weights of the morph targets. (Only valid if this is a material instance
    /// binding and the submeshes have morph targets.)
    /// </summary>
    /// <value>The weights of the morph targets.</value>
    internal MorphWeightCollection MorphWeights { get; set; }
    // The MorphWeightCollection is needed by the MeshRenderer.
    // Alternatively, we could remove EffectBinding.MorphWeights and add it to MeshRenderer.Job.
    // But this could slow down the draw job sorting.


    /// <summary>
    /// Gets or sets the binding that resolves the effect technique.
    /// </summary>
    /// <value>The binding that resolves the effect technique.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public EffectTechniqueBinding TechniqueBinding
    {
      get { return _techniqueBinding; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _techniqueBinding = value;
      }
    }
    private EffectTechniqueBinding _techniqueBinding;


    /// <summary>
    /// Gets the bindings that resolve effect parameters.
    /// </summary>
    /// <value>The bindings that resolve effect parameters.</value>
    public EffectParameterBindingCollection ParameterBindings { get; private set; }


    /// <summary>
    /// Gets a value indicating which effect parameters are handled by this effect binding.
    /// </summary>
    /// <value>
    /// A bitwise combination of <see cref="EffectParameterHint"/> values. The value defines which
    /// parameter bindings are handled by this effect binding.
    /// </value>
    public EffectParameterHint Hints
    {
      get { return ParameterBindings.Hints; }
    }


    /// <summary>
    /// Gets or sets a value indicating whether <see cref="OpaqueData"/> should be kept for 
    /// debugging.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if <see cref="OpaqueData"/> should be kept for debugging; otherwise, 
    /// <see langword="false"/> if opaque data will be deleted once the effect binding is 
    /// initialized. The default value is <see langword="false"/>.
    /// </value>
    public static bool KeepOpaqueData { get; set; }


    /// <summary>
    /// Gets the opaque data (only used for debugging, only set if <see cref="KeepOpaqueData"/> is 
    /// <see langword="true"/>).
    /// </summary>
    /// <value>The opaque data.</value>
    public IDictionary<string, object> OpaqueData { get; private set; }


    /// <summary>
    /// Gets or sets user-defined data.
    /// </summary>
    /// <value>User-defined data.</value>
    public object UserData { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBinding"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBinding"/> class. (This constructor
    /// creates an uninitialized instance. Use this constructor only for cloning or other special
    /// cases!)
    /// </summary>
    protected EffectBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBinding"/> class which can store
    /// all kinds of effect parameters.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="effect">The effect.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> or <paramref name="effect"/> is <see langword="null"/>.
    /// </exception>
    public EffectBinding(IGraphicsService graphicsService, Effect effect)
      : this(graphicsService, effect, null, EffectParameterHint.Any)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBinding"/> class which can be used in a 
    /// <see cref="Material"/> (only storing bindings for 
    /// <see cref="EffectParameterHint.Material"/> parameters).
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="effect">The effect.</param>
    /// <param name="opaqueData">The opaque data. Can be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> or <paramref name="effect"/> is <see langword="null"/>.
    /// </exception>
    public EffectBinding(IGraphicsService graphicsService, Effect effect, IDictionary<string, object> opaqueData)
      : this(graphicsService, effect, opaqueData, EffectParameterHint.Material)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBinding"/> class with the given settings.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="effect">The effect.</param>
    /// <param name="opaqueData">The opaque data. Can be <see langword="null"/>.</param>
    /// <param name="hints">
    /// A bitwise combination of <see cref="EffectParameterHint"/> values. The value defines which
    /// parameter bindings can be added to the effect binding.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> or <paramref name="effect"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="effect"/> is an XNA stock effect. The effect binding cannot be used with XNA 
    /// stock effects.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Effect should be initialized in constructor. Alternative designs complicate API.")]
    public EffectBinding(IGraphicsService graphicsService, Effect effect, IDictionary<string, object> opaqueData, EffectParameterHint hints)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");
      if (effect == null)
        throw new ArgumentNullException("effect");

      if (effect is AlphaTestEffect && !(effect is WrappedAlphaTestEffect)
          || effect is BasicEffect && !(effect is WrappedBasicEffect)
          || effect is DualTextureEffect && !(effect is WrappedDualTextureEffect)
          || effect is EnvironmentMapEffect && !(effect is WrappedEnvironmentMapEffect)
          || effect is SkinnedEffect && !(effect is WrappedSkinnedEffect))
      {
        throw new ArgumentException("The EffectBinding class cannot be used with XNA stock effects (e.g. BasicEffect). Use a derived effect binding instead (e.g. BasicEffectBinding).");
      }

      // Initialize additional information, if not already initialized.
      EffectEx = EffectEx.From(effect, graphicsService);

      if (KeepOpaqueData)
        OpaqueData = opaqueData;

      // Initialize effect bindings.
      ParameterBindings = new EffectParameterBindingCollection(hints);
      InitializeBindings(graphicsService, opaqueData);
    }


    /// <summary>
    /// Creates a new instance of the <see cref="EffectBinding"/> class for a material instance.
    /// </summary>
    internal EffectBinding CreateMaterialInstance()
    {
      Debug.Assert(Hints == EffectParameterHint.Material, "EffectBinding for material expected.");

      // Note: The EffectBinding used in a Material can be a special binding, such as
      // BasicEffectBinding. The EffectBinding used in the MaterialInstance is not a
      // clone of the BasicEffectBinding, instead it is a normal EffectBinding.

      // A material instance may contain all parameters, except global and material settings.
      const EffectParameterHint hints = EffectParameterHint.Local
                                        | EffectParameterHint.PerInstance
                                        | EffectParameterHint.PerPass;

      var effectBinding = new EffectBinding
      {
        EffectEx = EffectEx,
        MaterialBinding = this,
        TechniqueBinding = TechniqueBinding.Clone(),
        ParameterBindings = new EffectParameterBindingCollection(hints),
        OpaqueData = OpaqueData,
      };

      // Copy all local, per-instance and per-pass bindings.
      foreach (var binding in EffectEx.ParameterBindings)
        if ((binding.Description.Hint & hints) != 0)
          effectBinding.ParameterBindings.Add(binding.Clone());

      return effectBinding;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes the effect technique and parameter bindings.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="opaqueData">The opaque data.</param>
    private void InitializeBindings(IGraphicsService graphicsService, IDictionary<string, object> opaqueData)
    {
      OnInitializeBindings(graphicsService, opaqueData);
    }


    /// <summary>
    /// Called when the effect technique and parameter bindings should be initialized.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="opaqueData">The opaque data. Can be <see langword="null"/>.</param>
    /// <remarks>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Derived classes can override this method to create
    /// custom parameter bindings. If the derived class does not initialize all parameter bindings
    /// then it should call the base implementation of <see cref="OnInitializeBindings"/> to
    /// initialize the remaining bindings.
    /// </para>
    /// <para>
    /// The method is called by the constructor of the base class. This means that derived classes
    /// may not be initialized yet!
    /// </para>
    /// </remarks>
    protected virtual void OnInitializeBindings(IGraphicsService graphicsService, IDictionary<string, object> opaqueData)
    {
      if (TechniqueBinding == null)
        TechniqueBinding = EffectEx.TechniqueBinding.Clone();

      EffectHelper.InitializeParameterBindings(graphicsService, EffectEx, opaqueData, ParameterBindings);
    }
    #endregion
  }
}
