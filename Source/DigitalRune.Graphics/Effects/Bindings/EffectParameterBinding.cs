// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Binds a parameter of an effect to a certain value.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class is used to bind an <see cref="EffectParameter"/> of an <see cref="Effect"/> to a 
  /// certain value. When the effect parameter represents a single value (such as 
  /// <see cref="float"/>, <see cref="Vector3"/>, <see cref="Matrix"/>, etc.) an 
  /// <see cref="EffectParameterBinding{T}"/> should be used to bind the value. When the effect 
  /// parameter represents an array of values an <see cref="EffectParameterArrayBinding{T}"/> needs 
  /// to be used.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// <see cref="EffectParameterBinding"/>s need to be cloneable. The method 
  /// <see cref="Clone()"/> calls <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/> which
  /// are responsible for creating a clone of the current instance. Classes that derive from 
  /// <see cref="EffectParameterBinding"/> need to provide the implementation for 
  /// <see cref="CreateInstanceCore"/> and override <see cref="CloneCore"/> if necessary.
  /// </para>
  /// </remarks>
  /// <seealso cref="EffectParameterBinding{T}"/>
  /// <seealso cref="EffectParameterArrayBinding{T}"/>
  public abstract partial class EffectParameterBinding
  {
    ///// <para>
    ///// <strong>Notes for users of DigitalRune Mathematics:</strong><br/> 
    ///// The DigitalRune vector types (see <see cref="Vector2F"/>, <see cref="Vector3F"/>, 
    ///// <see cref="Vector4F"/>) are column-vectors, whereas vectors in DirectX and XNA are 
    ///// row-vectors. When the DigitalRune vector types are set using an 
    ///// <see cref="EffectParameterBinding{T}"/> they are automatically converted to the corresponding 
    ///// XNA vector types and then applied using the right 
    ///// <strong>EffectParameter.SetValue()</strong>-method. Similarly, when a DigitalRune matrix type 
    ///// (see <see cref="Matrix44F"/>) is set with an <see cref="EffectParameterBinding{T}"/> it is 
    ///// automatically transposed and converted to an XNA matrix type. 
    ///// </para>

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the effect parameter.
    /// </summary>
    /// <value>The effect parameter.</value>
    public EffectParameter Parameter
    {
      get { return Description.Parameter; }
    }


    /// <summary>
    /// Gets the description of the effect parameter.
    /// </summary>
    /// <value>The description of the effect parameter.</value>
    public EffectParameterDescription Description { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterBinding"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterBinding"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or
    /// other special cases!)
    /// </summary>
    protected EffectParameterBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterBinding"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    protected EffectParameterBinding(Effect effect, EffectParameter parameter)
    {
      if (effect == null)
        throw new ArgumentNullException("effect");
      if (parameter == null)
        throw new ArgumentNullException("parameter");

      try
      {
        Description = effect.GetParameterDescriptions()[parameter];
      }
      catch (KeyNotFoundException)
      {
        throw new GraphicsException("Missing effect parameter description.\n\n" 
          + "Cause:\nThis can happen if an effect parameter uses a struct type, "
          + "and the parameter name or semantic is known by an effect interpreter, but "
          + "no binding is provided by any effect binder.\n\n"
          + "Solution:\nAn effect binder must create a binding for this parameter.");
      }

      if ((GlobalSettings.ValidationLevelInternal & GlobalSettings.ValidationLevelDevBasic) != 0)
        VerifyEffectParameter(effect);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="EffectParameterBinding"/> that is a clone of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="EffectParameterBinding"/> that is a clone of the current instance.
    /// </returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="EffectParameterBinding"/> derived class and <see cref="CloneCore"/> to create a 
    /// copy of the current instance. Classes that derive from <see cref="EffectParameterBinding"/> 
    /// need to implement <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </remarks>
    public EffectParameterBinding Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterBinding"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method, 
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="EffectParameterBinding"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private EffectParameterBinding CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone EffectParameterBinding. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="EffectParameterBinding"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="EffectParameterBinding"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="EffectParameterBinding"/> derived 
    /// class must implement this method. A typical implementation is to simply call the default 
    /// constructor and return the result. 
    /// </para>
    /// </remarks>
    protected abstract EffectParameterBinding CreateInstanceCore();


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified 
    /// <see cref="EffectParameterBinding"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="EffectParameterBinding"/> derived 
    /// class must implement this method. A typical implementation is to call 
    /// <c>base.CloneCore(this)</c> to copy all properties of the base class and then copy all 
    /// properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(EffectParameterBinding source)
    {
      Description = source.Description;
    }
    #endregion


    /// <summary>
    /// Called when binding is created to verify that <see cref="Parameter"/> is a valid parameter.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <exception cref="EffectBindingException">
    /// <see cref="Parameter"/> is not a valid effect parameter of the current effect.
    /// </exception>
    private void VerifyEffectParameter(Effect effect)
    {
      // We use 3 foreach-loops to search for parameter.
      // We could move all checks into a single loop, but in most cases the
      // first loop is sufficient. Hence, the current approach is faster.

      // Compare Parameter with all top-level parameters.
      foreach (var parameter in effect.Parameters)
        if (parameter == Parameter)
          return;

      // Compare Parameter recursively with all array elements.
      foreach (var parameter in effect.Parameters)
      {
        if (parameter.Elements.Count > 0)
        {
          if (parameter.ParameterClass == EffectParameterClass.Struct // parameter is an array of structs. --> A field of the structs might contain the wanted parameter.
              || parameter.ParameterClass == Parameter.ParameterClass // Element type matches parameter. --> Check elements. (e.g. parameter is array of LightDirections and Parameter is LightDirection3).
                 && parameter.ParameterType == Parameter.ParameterType)
          {
            // Recursively check element in array.
            bool dummy;
            if (ContainsParameter(parameter.Elements, out dummy))
              return;
          }
        }
      }

      // Compare Parameter recursively with all struct members.
      foreach (var parameter in effect.Parameters)
      {
        if (parameter.ParameterClass == EffectParameterClass.Struct)
        {
          // Recursively check members of struct.
          bool dummy;
          if (ContainsParameter(parameter.StructureMembers, out dummy))
            return;
        }
      }

      string message = String.Format(
        CultureInfo.InvariantCulture,
        "Invalid effect parameter binding for parameter \"{0}\". Parameter is not part of the current effect.",
        Parameter.Name);
      throw new EffectBindingException(message, effect, Parameter);
    }


    /// <summary>
    /// Recursively determines whether the specified collection of effect parameters contains 
    /// <see cref="Parameter"/>.
    /// </summary>
    /// <param name="parameterCollection">The effect parameter collection.</param>
    /// <param name="isField">
    /// <see langword="true"/> if <see cref="Parameter"/> is the member of a struct; otherwise
    /// <see langword="false"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="parameterCollection"/> contains 
    /// <see cref="Parameter"/>; otherwise, <see langword="false"/>.
    /// </returns>
    private bool ContainsParameter(EffectParameterCollection parameterCollection, out bool isField)
    {
      // Compare with parameters in parameterCollection.
      foreach (EffectParameter parameter in parameterCollection)
      {
        if (parameter == Parameter)
        {
          // Wanted parameter is an item of parameterCollection.
          isField = false;
          return true;
        }
      }

      // Recursively compare with all array elements.
      foreach (EffectParameter parameter in parameterCollection)
      {
        if (parameter.Elements.Count > 0)
        {
          // Current parameter in parameterCollection is an array.
          // -> Recursively check elements of array.
          if (ContainsParameter(parameter.Elements, out isField))
          {
            // Wanted parameter is contained in array.
            return true;
          }
        }
      }

      // Recursively compare with all struct members.
      foreach (EffectParameter parameter in parameterCollection)
      {
        if (parameter.ParameterClass == EffectParameterClass.Struct)
        {
          // Current parameter in parameterCollection is a struct.
          // -> Recursively check members of struct.
          bool dummy;
          if (ContainsParameter(parameter.StructureMembers, out dummy))
          {
            // Wanted parameter is a member of the struct.
            isField = true;
            return true;
          }
        }
      }

      isField = false;
      return false;
    }


    /// <summary>
    /// Updates the value of the binding.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <para>
    /// This method is called usually once per frame to update the parameter value. 
    /// <see cref="Update"/> calls <see cref="OnUpdate"/> which needs to be implemented in derived 
    /// classes. When the effect parameter is dynamic and needs to be recalculated each frame before
    /// rendering, <see cref="OnUpdate"/> can be used to compute the new parameter value. When the 
    /// effect parameter is static then <see cref="OnUpdate"/> can be empty.
    /// </para>
    /// <para>
    /// The method <see cref="Update"/> only calculates the new value and stores it internally. 
    /// The <see cref="Apply"/> then sets the value in the target parameter (see 
    /// <see cref="Parameter"/>).
    /// </para>
    /// <para>
    /// <see cref="Update"/> needs to be called before <see cref="Apply"/>.
    /// </para>
    /// </remarks>
    public void Update(RenderContext context)
    {
      OnUpdate(context);
    }


    /// <summary>
    /// Called when the effect parameter value needs to be updated.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// This method needs to be implemented in derived classes.
    /// </remarks>
    protected abstract void OnUpdate(RenderContext context);


    /// <summary>
    /// Applies the value to the effect parameter.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <para>
    /// This method sets the shader constant to the value that was calculated in 
    /// <see cref="Update"/>. <see cref="Apply"/> needs to be called after <see cref="Update"/> 
    /// before the object that uses the <see cref="Effect"/> is rendered.
    /// </para>
    /// <para>
    /// This method calls <see cref="OnApply"/> which needs to be implemented in derived classes and
    /// is responsible for assigning the value that was calculated in <see cref="Update"/> to 
    /// <see cref="Parameter"/> by using the appropriate <strong>EffectParameter.SetValue()</strong>
    /// method.
    /// </para>
    /// </remarks>
    /// <exception cref="EffectBindingException">
    /// Unable to apply effect parameter.
    /// </exception>
    public void Apply(RenderContext context)
    {
      try
      {
        OnApply(context);
      }
      catch (Exception exception)
      {
        throw new EffectBindingException("Could not set parameter " + Parameter.Name + ": " + exception.Message, null, Parameter, exception);
      }
    }


    /// <summary>
    /// Called when the effect parameter value needs to be applied.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// This method needs to be implemented in derived classes.
    /// </remarks>
    protected abstract void OnApply(RenderContext context);
    // Note: context parameter is not needed by most bindings, but it could be used to check 
    // what technique is used and to ignore parameters that are not used in the technique, 
    // or for other optimizations.
    #endregion
  }
}
