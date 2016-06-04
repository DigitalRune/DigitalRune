// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DigitalRune.Collections;


namespace DigitalRune.Particles
{
  /// <summary>
  /// Manages a collection of particle parameters.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This collection manages the particle parameters of a particle system. All parameters must have
  /// a unique name (<see langword="null"/> or empty strings are not allowed).
  /// </para>
  /// <para>
  /// New particle parameters can be added using the methods <see cref="AddUniform{T}"/> and 
  /// <see cref="AddVarying{T}"/>. If a particle parameter with the given name already exists,
  /// <see cref="AddUniform{T}"/> returns the existing parameter and does nothing else - even if the
  /// existing parameter is varying. If a particle parameter with the given name already exists, 
  /// <see cref="AddVarying{T}"/> returns the existing parameter if it is varying. But if the 
  /// existing parameter is uniform, <see cref="AddVarying{T}"/> replaces the existing parameter. - 
  /// This means: Calling <see cref="AddUniform{T}"/> and <see cref="AddVarying{T}"/> multiple times
  /// is safe. Varying parameters have higher priority and replace uniform parameters.
  /// </para>
  /// <para>
  /// The generic methods <see cref="AddUniform{T}"/>, <see cref="AddVarying{T}"/> and 
  /// <see cref="Get{T}(string)"/> throw an <see cref="ParticleSystemException"/> if a particle 
  /// parameter with the given name exists but the particle parameter type is different. The method
  /// <see cref="GetUnchecked{T}(string)"/> is the same as <see cref="Get{T}(string,bool)"/>, except
  /// that it does not throw an exception. It returns <see langword="null"/> if the requested 
  /// particle parameter does not exist or the type does not match.
  /// </para>
  /// <para>
  /// <strong>Parameter Inheritance:</strong><br/>
  /// Per default, the <strong>Get()</strong> methods also search for uniform (but not varying) 
  /// parameters in the particle parameter collections of parent particle systems if a parameter is 
  /// not found in this collection. This means that uniform parameters are automatically inherited 
  /// by child particle systems. Note that enumerating the collection (using 
  /// <see cref="IEnumerable{T}.GetEnumerator"/>) only returns the locally stored particle 
  /// parameters, i.e. inherited parameters are not included in the enumeration.
  /// </para>
  /// <para>
  /// Objects that manipulate particle parameters can hold references to the 
  /// <see cref="IParticleParameter"/> instances. The event <see cref="Changed"/> is raised when the
  /// collection is modified. In this case any references to particle parameters get invalid. - The 
  /// particle parameters could have been replaced or removed. (<see cref="ParticleEffector"/>s do
  /// not need to subscribe to the <see cref="Changed"/> event. They will automatically be updated
  /// by the <see cref="ParticleSystem"/> that owns them.)
  /// </para>
  /// </remarks>
  public class ParticleParameterCollection : IEnumerable<IParticleParameter>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly NamedObjectCollection<IParticleParameter> _collection;
    #endregion
      
      
    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the particle system that owns this particle parameter collection.
    /// </summary>
    /// <value>
    /// The particle system that owns this particle parameter collection. Cannot be 
    /// <see langword="null"/>.
    /// </value>
    public ParticleSystem ParticleSystem { get; private set; }


    /// <summary>
    /// Occurs when the collection was modified.
    /// </summary>
    public event EventHandler<EventArgs> Changed;
    #endregion
      
      
    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleParameterCollection"/> class.
    /// </summary>
    /// <param name="particleSystem">The particle system that owns this collection.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="particleSystem"/> is <see langword="null"/>.
    /// </exception>
    internal ParticleParameterCollection(ParticleSystem particleSystem)
    {
      if (particleSystem == null)
        throw new ArgumentNullException("particleSystem");

      ParticleSystem = particleSystem;

      _collection = new NamedObjectCollection<IParticleParameter>(
        StringComparer.Ordinal,
        8);   // TODO: Choose appropriate threshold for lookup-table.
    }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Removes all particle parameters in this collection.
    /// </summary>
    public void Clear()
    {
      if (_collection.Count > 0)
      {
        _collection.Clear();
        OnChanged(EventArgs.Empty);
      }
    }


    /// <summary>
    /// Determines whether the collection contains a particle parameter with the given name.
    /// </summary>
    /// <param name="name">The name of the parameter (e.g. "Color", "Position", etc.).</param>
    /// <returns>
    /// <see langword="true"/> if a particle parameter with the given name exists; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Contains(string name)
    {
      return _collection.Contains(name);
    }


    /// <summary>
    /// Adds a varying particle parameter.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="name">The name of the parameter (e.g. "Color", "Position", etc.).</param>
    /// <returns>The existing or newly created particle parameter.</returns>
    /// <remarks>
    /// If the collection does not contain a particle parameter with the given 
    /// <paramref name="name"/>, a new varying particle parameter is added. If the collection 
    /// contains a varying particle parameter with the given <paramref name="name"/>, then only the
    /// existing particle parameter is returned. If the collection contains a uniform particle 
    /// parameter with the given <paramref name="name"/>, then the existing particle parameter is 
    /// removed and replaced by a new varying particle parameter.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty.
    /// </exception>
    /// <exception cref="ParticleSystemException">
    /// A particle parameter with the given name was found but the parameter type cannot be cast to 
    /// type <typeparamref name="T"/>.
    /// </exception>
    public IParticleParameter<T> AddVarying<T>(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("The particle parameter name must not be empty.", "name");

      IParticleParameter<T> parameter = Get<T>(name, true);
      T defaultValue = default(T);
      if (parameter is UniformParticleParameter<T>)
      {
        // Remove uniform parameter, but keep default value.
        defaultValue = parameter.DefaultValue;
        _collection.Remove(name);
      }
      else if (parameter != null)
      {
        return parameter;
      }
      
      parameter = new VaryingParticleParameter<T>(name, ParticleSystem.MaxNumberOfParticles);
      parameter.DefaultValue = defaultValue;
      _collection.Add(parameter);
      OnChanged(EventArgs.Empty);
      
      return parameter;
    }



    /// <summary>
    /// Adds a uniform particle parameter.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="name">The name of the parameter (e.g. "Color", "Position", etc.).</param>
    /// <returns>The existing or newly created particle parameter.</returns>
    /// <remarks>
    /// If the collection does not contain a particle parameter with the given 
    /// <paramref name="name"/>, a new uniform particle parameter is added. If the collection
    /// contains a particle parameter with the given <paramref name="name"/>, then only the
    /// existing particle parameter is returned - even if the parameter is varying!
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty.
    /// </exception>
    /// <exception cref="ParticleSystemException">
    /// A particle parameter with the given name was found but the parameter type cannot be cast to 
    /// type <typeparamref name="T"/>.
    /// </exception>
    public IParticleParameter<T> AddUniform<T>(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("The particle parameter name must not be empty.", "name");

      IParticleParameter<T> parameter = Get<T>(name, true);
      if (parameter != null)
        return parameter;

      parameter = new UniformParticleParameter<T>(name);
      _collection.Add(parameter);
      OnChanged(EventArgs.Empty);

      return parameter;
    }
   

    /// <overloads>
    /// <summary>
    /// Gets a particle parameter with the specified type. (If the type is wrong an 
    /// exception is thrown.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets a particle parameter with the specified type and name. (If the type is wrong an 
    /// exception is thrown.)
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="name">The name of the parameter (e.g. "Color", "Position", etc.).</param>
    /// <returns>
    /// The particle parameter, or <see langword="null"/> if no particle parameter with the given 
    /// <paramref name="name"/> was found.
    /// </returns>
    /// <remarks>
    /// If the particle parameter is not found in this <see cref="ParticleParameterCollection"/>,
    /// then the parameter collections of any parent particle systems are searched as well (only 
    /// for uniform parameters).
    /// </remarks>
    /// <exception cref="ParticleSystemException">
    /// A particle parameter with the given name was found, but the parameter type cannot be cast to 
    /// type <typeparamref name="T"/>.
    /// </exception>
    public IParticleParameter<T> Get<T>(string name)
    {
      return Get<T>(name, false);
    }


    /// <summary>
    /// Gets a particle parameter with the specified type and name. (If the type is wrong an
    /// exception is thrown.)
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="name">The name of the parameter (e.g. "Color", "Position", etc.).</param>
    /// <param name="excludeInherited">
    /// If set to <see langword="true"/> only parameters of this 
    /// <see cref="ParticleParameterCollection"/> are returned. If set to <see langword="false"/>
    /// the parent particle systems are also scanned for uniform particle parameters with the
    /// specified name.
    /// </param>
    /// <returns>
    /// The particle parameter, or <see langword="null"/> if no particle parameter with the given
    /// <paramref name="name"/> was found.
    /// </returns>
    /// <exception cref="ParticleSystemException">
    /// A particle parameter with the given name was found, but the parameter type cannot be cast to
    /// type <typeparamref name="T"/>.
    /// </exception>
    public IParticleParameter<T> Get<T>(string name, bool excludeInherited)
    {
      // Do not throw exception. Simply return null. This simplifies ParticleEffectors.
      if (string.IsNullOrEmpty(name))
        return null;

      IParticleParameter parameter;
      _collection.TryGet(name, out parameter);

      if (parameter == null)
      {
        if (!excludeInherited)
        {
          if (ParticleSystem.Parent != null)
          {
            // Search the parent particle systems - but only accept uniform parameters.
            var parameterTyped = ParticleSystem.Parent.Parameters.Get<T>(name, false);
            if (parameterTyped != null && parameterTyped.Values == null)
              return parameterTyped;
          }
        }
        return null;
      }
      else
      {
        var parameterTyped = parameter as IParticleParameter<T>;
        if (parameterTyped != null)
          return parameterTyped;

        // Invalid type.
        string message = string.Format(CultureInfo.InvariantCulture, "The particle parameter '{0}' cannot be cast to '{1}'.", name, typeof(T).Name);
        throw new ParticleSystemException(message)
        {
          ParticleSystem = ParticleSystem,
          ParticleParameter = name,
        };
      }
    }


    /// <overloads>
    /// <summary>
    /// Gets a particle parameter with the specified type. (If the type is wrong 
    /// <see langword="null"/> is returned.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets a particle parameter with the specified type and name. (If the type is wrong 
    /// <see langword="null"/> is returned.)
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="name">The name of the parameter (e.g. "Color", "Position", etc.).</param>
    /// <returns>
    /// The particle parameter, or <see langword="null"/> if no particle parameter with the given 
    /// <paramref name="name"/> and type <typeparamref name="T"/> was found.
    /// </returns>
    /// <remarks>
    /// If the particle parameter is not found in this <see cref="ParticleParameterCollection"/>,
    /// then the parameter collections of any parent particle systems are searched as well (only 
    /// for uniform parameters).
    /// </remarks>
    public IParticleParameter<T> GetUnchecked<T>(string name)
    {
      return GetUnchecked<T>(name, false);
    }


    /// <summary>
    /// Gets a particle parameter with the specified type and name. (If the type is wrong 
    /// <see langword="null"/> is returned.)
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="name">The name of the parameter (e.g. "Color", "Position", etc.).</param>
    /// <param name="excludeInherited">
    /// If set to <see langword="true"/> only parameters of this 
    /// <see cref="ParticleParameterCollection"/> are returned. If set to <see langword="false"/>
    /// the parent particle systems are also scanned for uniform particle parameters with the
    /// specified name.
    /// </param>
    /// <returns>
    /// The particle parameter, or <see langword="null"/> if no particle parameter with the given
    /// <paramref name="name"/> and type <typeparamref name="T"/> was found.
    /// </returns>
    public IParticleParameter<T> GetUnchecked<T>(string name, bool excludeInherited)
    {
      // Do not throw exception. Simply return null. This simplifies ParticleEffectors.
      if (string.IsNullOrEmpty(name))
        return null;

      IParticleParameter parameter;
      _collection.TryGet(name, out parameter);

      if (parameter == null)
      {
        if (!excludeInherited)
        {
          if (ParticleSystem.Parent != null)
          {
            // Search the parent particle systems - but only accept uniform parameters.
            var parameterTyped = ParticleSystem.Parent.Parameters.GetUnchecked<T>(name, false);
            if (parameterTyped != null && parameterTyped.Values == null)
              return parameterTyped;
          }
        }
        return null;
      }
      else
      {
        return parameter as IParticleParameter<T>;
      }
    }


    /// <summary>
    /// Removes a particle parameter.
    /// </summary>
    /// <param name="name">The name of the parameter (e.g. "Color", "Position", etc.).</param>
    /// <returns>
    /// <see langword="true"/> if a particle parameter was found and removed; otherwise,
    /// <see langword="false"/> if no particle parameter with the given name was found.
    /// </returns>
    public bool Remove(string name)
    {
      if (string.IsNullOrEmpty(name))
        return false;

      var result = _collection.Remove(name);

      if (result)
        OnChanged(EventArgs.Empty);

      return result;
    }
    

    internal void UpdateArrayLength()
    {
      foreach (var parameter in _collection.OfType<IParticleParameterInternal>())
        parameter.UpdateArrayLength(ParticleSystem.MaxNumberOfParticles);
    }

    
    #region ----- IEnumerable -----

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ParticleParameterCollection"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="ParticleParameterCollection"/>.
    /// </returns>
    public List<IParticleParameter>.Enumerator GetEnumerator()
    {
      return _collection.GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ParticleParameterCollection"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="ParticleParameterCollection"/>.
    /// </returns>
    IEnumerator<IParticleParameter> IEnumerable<IParticleParameter>.GetEnumerator()
    {
      return _collection.GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ParticleParameterCollection"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="ParticleParameterCollection"/>.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return _collection.GetEnumerator();
    }
    #endregion


    /// <summary>
    /// Raises the <see cref="Changed"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnChanged"/> in a derived 
    /// class, be sure to call the base class's <see cref="OnChanged"/> method so that registered 
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnChanged(EventArgs eventArgs)
    {
      var handler = Changed;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
