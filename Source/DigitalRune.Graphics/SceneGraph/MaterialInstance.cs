// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Graphics.Effects;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents an instance of a specific material.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Each mesh has a set of materials (see class <see cref="Material"/>). A material defines the 
  /// effect bindings (see class <see cref="EffectBinding"/>) for all required render passes. 
  /// </para>
  /// <para>
  /// The effect parameters basically fall into two categories:
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// <i>Non-shared parameters</i>, which are unique for each mesh node. Examples are world 
  /// matrices, light properties, etc. These effect parameters depend on the actual mesh instance 
  /// that is rendered.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <i>Shared parameters</i>, which are shared by mesh nodes with the same base mesh. Examples are
  /// view/projection matrices, most material properties, etc.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// When a new mesh node (see class <see cref="MeshNode"/>) is created for a mesh (see class 
  /// <see cref="Mesh"/>), all materials are instanced. That means, a new object of type
  /// <see cref="MaterialInstance"/> is created for each material. A material instance references
  /// the base material from which it was created (see property <see cref="Material"/>).
  /// </para>
  /// <para>
  /// Both, the base material and the material instance are dictionaries of effect bindings: They 
  /// contain one effect binding for each render pass that is required to render the mesh. The base 
  /// material contains the bindings for all shared parameters. The material instance contains the 
  /// bindings for non-shared parameters. Materials can be shared between different meshes - they 
  /// are not bound to a specific mesh, but material instances belong to a certain mesh node!
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// <see cref="MaterialInstance"/>s can be cloned. When <see cref="Clone()"/> is called all 
  /// <see cref="EffectBindings"/> are duplicated (deep copy). The base <see cref="Material"/> is 
  /// copied by reference (shallow copy).
  /// </para>
  /// </remarks>
  /// <seealso cref="EffectBinding"/>
  /// <seealso cref="Material"/>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Material.Name})")]
  public partial class MaterialInstance : IDictionary<string, EffectBinding>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Dictionary<string, EffectBinding> _bindingsPerPass;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the material.
    /// </summary>
    /// <value>The material.</value>
    public Material Material { get; private set; }


    /// <summary>
    /// Gets the number of render passes supported by this material instance.
    /// </summary>
    /// <value>The number of render passes supported by this material instance.</value>
    public int Count
    {
      get { return _bindingsPerPass.Count; }
    }


    /// <summary>
    /// Gets a read-only collection of all the render passes supported by this material instance.
    /// </summary>
    /// <value>
    /// A read-only <see cref="ICollection{T}"/> containing the render passes supported by this 
    /// material instance.
    /// </value>
    /// <remarks>
    /// The order of the render passes in the returned <see cref="ICollection{T}"/> is unspecified, 
    /// but it is guaranteed to be the same order as the corresponding effect bindings in the 
    /// <see cref="ICollection{T}"/> returned by the <see cref="EffectBindings"/> property.
    /// </remarks>
    public ICollection<string> Passes
    {
      get { return _bindingsPerPass.Keys; }
    }


    /// <summary>
    /// Gets a read-only collection of effect bindings used by this material instance.
    /// </summary>
    /// <value>A read-only collection of effect bindings used by this material instance.</value>
    /// <remarks>
    /// The order of the effect bindings in the returned <see cref="ICollection{T}"/> is 
    /// unspecified, but it is guaranteed to be the same order as the corresponding render passes in
    /// the <see cref="ICollection{T}"/> returned by the <see cref="EffectBindings"/> property.
    /// </remarks>
    public ICollection<EffectBinding> EffectBindings
    {
      get { return _bindingsPerPass.Values; }
    }


    /// <summary>
    /// Gets or sets the effect parameter bindings for the specified render pass.
    /// </summary>
    /// <param name="pass">The render pass.</param>
    /// <value>
    /// The effect parameter bindings, or <see langword="null"/> if this material instance does not 
    /// support the specified render pass.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pass"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// <paramref name="pass"/> is not found in the material.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
    public EffectBinding this[string pass]
    {
      get { return _bindingsPerPass[pass]; }
      set { throw new NotSupportedException("The MaterialInstance is a read-only collection."); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialInstance"/> class.
    /// (This constructor creates an uninitialized instance. Use this constructor only for 
    /// cloning or other special cases!)
    /// </summary>
    protected MaterialInstance()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialInstance"/> class.
    /// </summary>
    /// <param name="material">The material.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="material"/> is <see langword="null"/>.
    /// </exception>
    public MaterialInstance(Material material)
    {
      if (material == null)
        throw new ArgumentNullException("material");

      Material = material;

      // Create local effect parameter bindings for all render passes.
      _bindingsPerPass = new Dictionary<string, EffectBinding>(material.Count);
      foreach (var effectBindingPerPass in material)
      {
        string pass = effectBindingPerPass.Key;
        var effectBinding = effectBindingPerPass.Value;
        
        _bindingsPerPass.Add(pass, effectBinding.CreateMaterialInstance());
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through a collection. 
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{T}"/> object that can be used to iterate through the collection.
    /// </returns>
    public Dictionary<string, EffectBinding>.Enumerator GetEnumerator()
    {
      return _bindingsPerPass.GetEnumerator();
    }


    /// <summary>
    /// Determines whether the material instance contains effect parameter bindings for the 
    /// specified render pass.
    /// </summary>
    /// <param name="pass">The render pass.</param>
    /// <returns>
    /// <see langword="true"/> if the material instance contains effect parameter bindings for the 
    /// specified render pass; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pass"/> is <see langword="null"/>.
    /// </exception>
    public bool Contains(string pass)
    {
      return _bindingsPerPass.ContainsKey(pass);
    }


    /// <summary>
    /// Gets the effect parameter bindings for the specified render pass.
    /// </summary>
    /// <param name="pass">The render pass.</param>
    /// <param name="bindings">
    /// When this method returns, the effect parameter bindings for the specified render pass, if 
    /// the render pass is supported by the material; otherwise, the <see langword="null"/>. This 
    /// parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the material supports the specified render pass; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pass"/> is <see langword="null"/>.
    /// </exception>
    public bool TryGet(string pass, out EffectBinding bindings)
    {
      return _bindingsPerPass.TryGetValue(pass, out bindings);
    }
    #endregion
  }
}
