// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;


namespace DigitalRune.Graphics.Effects
{
  partial class EffectBinding
  {
    /// <summary>
    /// Creates a new <see cref="EffectBinding"/> that is a clone of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="EffectBinding"/> that is a clone of the current instance.
    /// </returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="EffectBinding"/> derived class and <see cref="CloneCore"/> to create a copy of 
    /// the current instance. Classes that derive from <see cref="EffectBinding"/> need to 
    /// implement <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </remarks>
    public EffectBinding Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBinding"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method, 
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone EffectBinding. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private EffectBinding CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone EffectBinding. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="EffectBinding"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of
    /// the <see cref="EffectBinding"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="EffectBinding"/> derived class must
    /// implement this method. A typical implementation is to simply call the default constructor
    /// and return the result. 
    /// </para>
    /// </remarks>
    protected virtual EffectBinding CreateInstanceCore()
    {
      return new EffectBinding();
    }


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="EffectBinding"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="EffectBinding"/> derived class must
    /// implement this method. A typical implementation is to call <c>base.CloneCore(this)</c> to
    /// copy all properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(EffectBinding source)
    {
      EffectEx = source.EffectEx;
      MaterialBinding = source.MaterialBinding;
      // Note: MorphWeights need to be set by MeshNode.
      TechniqueBinding = source.TechniqueBinding.Clone();
      OpaqueData = source.OpaqueData;
      UserData = source.UserData;

      // Clone parameter bindings (deep copy).
      ParameterBindings = new EffectParameterBindingCollection(source.Hints);
      foreach (var binding in source.ParameterBindings)
        ParameterBindings.Add(binding.Clone());
    }
  }
}
