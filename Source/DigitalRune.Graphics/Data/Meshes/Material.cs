// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines the material (visual properties) of a mesh.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="Mesh"/> represents the geometry and materials of a 3D object that can be 
  /// rendered. A mesh owns a collection of <see cref="Mesh.Materials"/> and is divided into 
  /// <see cref="Submesh"/>es. Each <see cref="Submesh"/> describes a batch of primitives (usually 
  /// triangles) that use one material and can be rendered with a single draw call.
  /// </para>
  /// <para>
  /// An effect binding (see class <see cref="EffectBinding"/>) provides the render states required 
  /// for each draw call: An <seealso cref="Effect"/> that defines the graphics device states. A 
  /// technique binding selects the vertex and pixel shaders for rendering. And parameter bindings
  /// define static properties (color, diffuse texture, gloss map, normal map, etc.) as well as 
  /// dynamic properties (world/view/projection matrices, light properties, etc.). But depending on 
  /// the render pipeline that is used, multiple render passes may be required to draw a certain 
  /// object. Therefore, a <see cref="Material"/> is a dictionary of effect bindings - one effect 
  /// binding per render pass. The dictionary key is the name of the render pass (a case-sensitive 
  /// string such as "Default", "ZPass", "ShadowMap", "GBuffer", "Material", etc.). The dictionary 
  /// value is the <see cref="EffectBinding"/> that contains required settings for this render pass.
  /// The entries in a material depend on the type of renderer that is used.
  /// </para>
  /// <para>
  /// Example: A forward renderer usually only requires a single render pass, in which the mesh is 
  /// rendered into the back buffer. In this case the material contains one entry (Key = "Default", 
  /// Value = <see cref="EffectBinding"/>).
  /// </para>
  /// <para>
  /// Advanced example: A light pre-pass renderer usually requires several render passes per mesh: 
  /// In the "ShadowMap" pass the mesh is rendered into the shadow-map texture, which is used later 
  /// on. Then in the "GBuffer" pass the depth, the normals, and other properties of the mesh are 
  /// rendered into multiple render targets. Next, the renderer computes the lighting information. 
  /// Then in the "Material" pass, the mesh is rendered again - the lighting information is combined 
  /// with the material settings. In this example the material contains 3 entries. The keys are
  /// "GBuffer", "Material", and "ShadowMap".
  /// </para>
  /// <para>
  /// A <see cref="Material"/> is not bound to a certain <see cref="Mesh"/>. It can be shared by 
  /// different <see cref="Mesh"/> objects.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> When an <see cref="EffectBinding"/> is used as part of a 
  /// <see cref="Material"/>, then it can only contain parameter bindings with the sort hint 
  /// <see cref="EffectParameterHint.Material"/> (see <see cref="EffectParameterHint"/>). 
  /// </para>
  /// <para>
  /// <strong>Cloning:<br/>
  /// </strong><see cref="Material"/>s can be cloned. When <see cref="Clone()"/> is called all 
  /// <see cref="EffectBinding"/> are duplicated (deep copy).
  /// </para>
  /// </remarks>
  /// <seealso cref="EffectBinding"/>
  /// <seealso cref="MaterialInstance"/>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public partial class Material : IDictionary<string, EffectBinding>, INamedObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Dictionary<string, EffectBinding> _bindingsPerPass;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the name of the material.
    /// </summary>
    /// <value>The name of the material. The default value is <see langword="null"/>.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets the number of render passes supported by this material.
    /// </summary>
    /// <value>The number of render passes supported by this material.</value>
    public int Count
    {
      get { return _bindingsPerPass.Count; }
    }

    
    /// <summary>
    /// Gets a collection of all the render passes supported by this material.
    /// </summary>
    /// <value>
    /// An <see cref="ICollection{T}"/> containing the render passes supported by this material.
    /// </value>
    /// <remarks>
    /// The order of the render passes in the returned <see cref="ICollection{T}"/> is unspecified, 
    /// but it is guaranteed to be the same order as the corresponding effect bindings in the 
    /// <see cref="ICollection{T}"/> returned by the <see cref="EffectBindings"/> property.
    /// </remarks>
    public Dictionary<string, EffectBinding>.KeyCollection Passes
    {
      get { return _bindingsPerPass.Keys; }
    }


    /// <summary>
    /// Gets a collection of effect bindings used by this material.
    /// </summary>
    /// <value>A collection of effect bindings used by this material.</value>
    /// <remarks>
    /// The order of the effect bindings in the returned <see cref="ICollection{T}"/> is 
    /// unspecified, but it is guaranteed to be the same order as the corresponding render passes in
    /// the <see cref="ICollection{T}"/> returned by the <see cref="EffectBindings"/> property.
    /// </remarks>
    public Dictionary<string, EffectBinding>.ValueCollection EffectBindings
    {
      get { return _bindingsPerPass.Values; }
    }


    /// <summary>
    /// Gets or sets the effect binding for the specified render pass.
    /// </summary>
    /// <param name="pass">The render pass.</param>
    /// <value>
    /// The effect binding, or <see langword="null"/> if this material does not support the 
    /// specified render pass.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pass"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// <paramref name="pass"/> is not found in the material.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
    public EffectBinding this[string pass]
    {
      get { return _bindingsPerPass[pass]; }
      set { _bindingsPerPass[pass] = value; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new binding of the <see cref="Material"/> class.
    /// </summary>
    public Material()
    {
      _bindingsPerPass = new Dictionary<string, EffectBinding>();
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
    /// Removes all render passes and effect bindings from the material.
    /// </summary>
    public void Clear()
    {
      _bindingsPerPass.Clear();
    }


    /// <summary>
    /// Determines whether the material contains an effect binding for the specified render pass.
    /// </summary>
    /// <param name="pass">The render pass.</param>
    /// <returns>
    /// <see langword="true"/> if the material contains an effect binding for the specified render
    /// pass; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pass"/> is <see langword="null"/>.
    /// </exception>
    public bool Contains(string pass)
    {
      return _bindingsPerPass.ContainsKey(pass);
    }


    /// <summary>
    /// Adds an effect binding for the specified render pass to the material.
    /// </summary>
    /// <param name="pass">The render pass.</param>
    /// <param name="effectBinding">The effect binding.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effectBinding"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// There is already an effect binding registered for the same render pass.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
    public void Add(string pass, EffectBinding effectBinding)
    {
      if (effectBinding == null)
        throw new ArgumentNullException("effectBinding");

      _bindingsPerPass.Add(pass, effectBinding);
    }


    /// <summary>
    /// Removes the effect binding for the specified render pass from the material.
    /// </summary>
    /// <param name="pass">The render pass.</param>
    /// <returns>
    /// <see langword="true"/> if effect binding was successfully removed from the material; 
    /// otherwise, <see langword="false"/>. This method also returns <see langword="false"/> if 
    /// <paramref name="pass"/> is not found in the original material.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pass"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
    public bool Remove(string pass)
    {
      return _bindingsPerPass.Remove(pass);
    }


    /// <summary>
    /// Gets the effect binding for the specified render pass
    /// </summary>
    /// <param name="pass">The render pass.</param>
    /// <param name="effectBinding">
    /// When this method returns, the effect binding for the specified render pass; otherwise, the 
    /// <see langword="null"/> if the render pass is not supported by the material. This parameter is 
    /// passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the material supports the specified render pass; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pass"/> is <see langword="null"/>.
    /// </exception>
    public bool TryGet(string pass, out EffectBinding effectBinding)
    {
      return _bindingsPerPass.TryGetValue(pass, out effectBinding);
    }
    #endregion
  }
}
