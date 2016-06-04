// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Selects a technique when rendering a certain effect.
  /// </summary>
  /// <remarks>
  /// <para>
  /// An <see cref="Effect"/> may define several <see cref="EffectTechnique"/>s for rendering 
  /// objects. An <see cref="EffectTechniqueBinding"/> provides the logic that selects the 
  /// appropriate technique when rendering a specific object.
  /// </para>
  /// <para>
  /// This base class always chooses the first technique of an effect and ignores other techniques.
  /// Derived <see cref="EffectTechniqueBinding"/> class should implement more useful strategies.
  /// </para>
  /// <para>
  /// Once a technique is selected, the method <see cref="GetPassBinding"/> can be called. This 
  /// methods returns an <see cref="EffectPassBinding"/>. The <see cref="EffectPassBinding"/> 
  /// can be used to iterate over all effect passes that need to be applied for the current
  /// object.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// <see cref="EffectTechniqueBinding"/>s need to be cloneable. The method 
  /// <see cref="Clone()"/> calls <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/> which
  /// are responsible for creating a clone of the current instance. Classes that derive from 
  /// <see cref="EffectTechniqueBinding"/> need to provide the implementation for 
  /// <see cref="CreateInstanceCore"/> and override <see cref="CloneCore"/> if necessary.
  /// </para>
  /// </remarks>
  public class EffectTechniqueBinding
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The default EffectTechniqueBinding always has the same state (Id is always 0)
    // and can be shared.

    /// <summary>
    /// The default effect technique binding.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly EffectTechniqueBinding Default = new EffectTechniqueBinding();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets (or sets) an ID, which can be used for state-sorting.
    /// </summary>
    /// <value>An ID, which can be used for state-sorting. The allowed range is [0, 127].</value>
    /// <remarks>
    /// The ID must be set in <see cref="OnUpdate"/>. The ID may change from frame to frame.
    /// </remarks>
    public byte Id { get; protected set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="EffectTechniqueBinding"/> that is a clone of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="EffectTechniqueBinding"/> that is a clone of the current instance.
    /// </returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="EffectTechniqueBinding"/> derived class and <see cref="CloneCore"/> to create a 
    /// copy of the current instance. Classes that derive from <see cref="EffectTechniqueBinding"/> 
    /// need to implement <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </remarks>
    public EffectTechniqueBinding Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectTechniqueBinding"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method, 
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="EffectTechniqueBinding"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private EffectTechniqueBinding CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone EffectTechniqueBinding. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="EffectTechniqueBinding"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="EffectTechniqueBinding"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="EffectTechniqueBinding"/> derived 
    /// class must implement this method. A typical implementation is to simply call the default 
    /// constructor and return the result. 
    /// </para>
    /// </remarks>
    protected virtual EffectTechniqueBinding CreateInstanceCore()
    {
      // The default instance is immutable and does not need to be cloned.
      // --> Return same instance to save memory. 
      return this;
    }


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="EffectTechniqueBinding"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="EffectTechniqueBinding"/> derived 
    /// class must implement this method. A typical implementation is to call 
    /// <c>base.CloneCore(this)</c> to copy all properties of the base class and then copy all 
    /// properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(EffectTechniqueBinding source)
    {
      Id = source.Id;
    }
    #endregion


    /// <summary>
    /// Selects a technique for rendering the specified effect and sets the ID.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <para>
    /// The method <see cref="Update"/> is called when an object needs to be rendered using an 
    /// <see cref="Effect"/>. Based on the information given in the render context, the method 
    /// selects the effect technique that should be used for rendering. The selected technique is 
    /// not yet set as the active technique - the information is only stored internally in the 
    /// <see cref="EffectTechniqueBinding"/>. The method also updates the property <see cref="Id"/>,
    /// which can be used for state sorting. 
    /// </para>
    /// <para>
    /// Immediately before rendering the object, the renderer can call <see cref="GetTechnique"/>, which
    /// returns the effect technique that should be used. The renderer can set the returned 
    /// technique as the <see cref="Effect.CurrentTechnique"/> of the <see cref="Effect"/>.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Update"/> calls 
    /// <see cref="OnUpdate"/>, which can be overridden in derived classes. The base implementation 
    /// selects the first technique found in the current <see cref="Effect"/>. <see cref="Id"/> is
    /// always 0.
    /// </para>
    /// </remarks>
    public void Update(RenderContext context)
    {
      OnUpdate(context);
    }


    /// <summary>
    /// Called when the effect technique needs to be selected and <see cref="Id"/> needs to be set.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <para>
    /// <strong>Notes to Inheritors:</strong>
    /// This method can be overridden in derived classes. <see cref="OnUpdate"/> needs to select the 
    /// effect technique that should be used for rendering and internally store the value. In 
    /// addition, the property <see cref="Id"/>, which is used for state sorting, needs to be 
    /// updated.
    /// </para>
    /// <para>
    /// The base implementation always chooses the first technique found in the effect. 
    /// <see cref="Id"/> is set to 0.
    /// </para>
    /// </remarks>
    protected virtual void OnUpdate(RenderContext context)
    {
      Id = 0;
    }


    /// <summary>
    /// Gets the effect technique that should be used for rendering.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="context">The render context.</param>
    /// <returns>The effect technique that should be used for rendering.</returns>
    /// <remarks>
    /// <para>
    /// This method is called immediately before an object is rendered using 
    /// <paramref name="effect"/>. The returned effect technique can set as the as the 
    /// <see cref="Effect.CurrentTechnique"/>.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="GetTechnique"/> calls 
    /// <see cref="OnGetTechnique"/>, which can be overridden in derived classes.
    /// </para>
    /// </remarks>
    public EffectTechnique GetTechnique(Effect effect, RenderContext context)
    {
      return OnGetTechnique(effect, context);
    }


    /// <summary>
    /// Called when the effect technique that should be used for rendering should be returned.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="context">The render context.</param>
    /// <returns>The effect technique that should be used for rendering.</returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong><br/>
    /// This method can be overridden in derived classes. The base implementation always returns the
    /// first technique found in <paramref name="effect"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual EffectTechnique OnGetTechnique(Effect effect, RenderContext context)
    {
      return effect.Techniques[0];
    }


    /// <summary>
    /// Gets the effect pass binding for the specified effect technique.
    /// </summary>
    /// <param name="technique">The effect technique.</param>
    /// <param name="context">The render context.</param>
    /// <returns>The effect pass binding.</returns>
    public EffectPassBinding GetPassBinding(EffectTechnique technique, RenderContext context)
    {
      return new EffectPassBinding(this, technique, context);
    }


    /// <summary>
    /// Called when next effect pass needs to be selected.
    /// </summary>
    /// <param name="technique">The current effect technique.</param>
    /// <param name="context">The render context.</param>
    /// <param name="index">The index of the next effect pass to be applied.</param>
    /// <param name="pass">The effect pass.</param>
    /// <returns>
    /// <see langword="true"/> if the next effect pass has been selected and should be applied; 
    /// otherwise, <see langword="false"/> if the are no more passes.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The method performs the following tasks:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Update the property <see cref="RenderContext.PassIndex"/> in the render context.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Select the next effect pass to be applied and store it in <paramref name="pass"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Increment <paramref name="index"/>.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// <strong>Notes to Inheritors:</strong><br/>
    /// This method can be overridden in derived classes. The base implementation simply iterates 
    /// through all effect passes of the current technique.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected internal virtual bool OnNextPass(EffectTechnique technique, RenderContext context, ref int index, out EffectPass pass)
    {
      Debug.Assert(index >= 0, "Invalid index.");

      int numberOfPasses = technique.Passes.Count;
      if (index >= numberOfPasses)
      {
        // Finished: All effect passes have been applied.
        context.PassIndex = -1;
        pass = null;
        return false;
      }

      context.PassIndex = index;
      pass = technique.Passes[index];
      index++;

      if (index == numberOfPasses - 1
          && string.Equals(pass.Name, "Restore", StringComparison.OrdinalIgnoreCase))
      {
        // A last effect pass may be used to restore the default render states without 
        // drawing anything. The effect pass needs to be called "Restore".
        pass.Apply();

        // Finished: All effect passes have been applied.
        context.PassIndex = -1;
        pass = null;
        return false;
      }

      return true;
    }
    #endregion
  }
}
