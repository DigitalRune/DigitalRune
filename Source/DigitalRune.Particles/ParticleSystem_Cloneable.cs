// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry;


namespace DigitalRune.Particles
{
  partial class ParticleSystem
  {
    /// <inheritdoc/>
    IGeometricObject IGeometricObject.Clone()
    {
      return Clone();
    }


    /// <summary>
    /// Creates a new <see cref="ParticleSystem"/> that is a clone of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="ParticleSystem"/> that is a clone of the current instance.
    /// </returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="ParticleSystem"/> derived class and <see cref="CloneCore"/> to create a copy of 
    /// the current instance. Classes that derive from <see cref="ParticleSystem"/> need to 
    /// implement <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </remarks>
    public ParticleSystem Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystem"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a protected method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method, 
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone particle system. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private ParticleSystem CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone particle system. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="ParticleSystem"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="ParticleSystem"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="ParticleSystem"/> derived class must 
    /// implement this method. A typical implementation is to simply call the default constructor 
    /// and return the result. 
    /// </para>
    /// </remarks>
    protected virtual ParticleSystem CreateInstanceCore()
    {
      return new ParticleSystem();
    }


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="ParticleSystem"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="ParticleEffector"/> derived class 
    /// must implement this method. A typical implementation is to call <c>base.CloneCore(this)</c> 
    /// to copy all properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(ParticleSystem source)
    {
      Name = source.Name;
      Enabled = source.Enabled;
      MaxNumberOfParticles = source.MaxNumberOfParticles;
      InitialDelay = source.InitialDelay;
      PreloadDuration = source.PreloadDuration;
      PreloadDeltaTime = source.PreloadDeltaTime;
      TimeScaling = source.TimeScaling;
      EnableMultithreading = source.EnableMultithreading;
      ReferenceFrame = source.ReferenceFrame;

      // Cloning the particle parameter collection is tricky because the parameters are generics.
      // We use an internal method of the parameter to do the job.
      foreach (var parameter in source.Parameters)
        ((IParticleParameterInternal)parameter).AddCopyToCollection(Parameters);

      foreach (var effector in source.Effectors)
        Effectors.Add(effector.Clone());

      if (source.Children != null)
      {
        Children = new ParticleSystemCollection();
        foreach (var particleSystem in source.Children)
          Children.Add(particleSystem.Clone());
      }

      Pose = source.Pose;
      Shape = source.Shape.Clone();
    }
  }
}
