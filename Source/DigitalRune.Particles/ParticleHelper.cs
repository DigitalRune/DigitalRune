// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;


namespace DigitalRune.Particles
{
  /// <summary>
  /// Provides helper methods and extension methods for particle systems.
  /// </summary>
  public static class ParticleHelper
  {
    /// <summary>
    /// Gets the value of a uniform or a varying particle parameter.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="parameter">The particle parameter.</param>
    /// <param name="index">
    /// The index of the particle, or -1 to get the <see cref="IParticleParameter{T}.DefaultValue"/>.
    /// </param>
    /// <returns>
    /// The value of the particle at the given index if the particle parameter is varying and the
    /// index is zero or positive; otherwise the <see cref="IParticleParameter{T}.DefaultValue"/>.
    /// </returns>
    public static T GetValue<T>(this IParticleParameter<T> parameter, int index)
    {
      if (parameter == null)
        return default(T);

      T[] values = parameter.Values;
      return (values != null && index >= 0) ? values[index] : parameter.DefaultValue;
    }


    /// <summary>
    /// Sets the value of a uniform or varying particle parameter.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="parameter">The particle parameter.</param>
    /// <param name="index">
    /// The index of the particle, or -1 to set the <see cref="IParticleParameter{T}.DefaultValue"/>.
    /// </param>
    /// <param name="value">The new value.</param>
    /// <remarks>
    /// If the particle parameter is varying and <paramref name="index"/> is zero or positive,
    /// the value of the particle is set. If <paramref name="index"/> is negative, the
    /// <see cref="IParticleParameter{T}.DefaultValue"/> is set. Otherwise, if the particle
    /// parameter is uniform and the <paramref name="index"/> is zero or positive, this method
    /// does nothing.
    /// </remarks>
    public static void SetValue<T>(this IParticleParameter<T> parameter, int index, T value)
    {
      if (parameter == null)
        return;

      T[] values = parameter.Values;
      if (values != null && index >= 0)
        values[index] = value;
      else if (index < 0)
        parameter.DefaultValue = value;
    }


    ///// <summary>
    ///// Gets a list of missing input parameters.
    ///// </summary>
    ///// <param name="particleSystem">The particle system.</param>
    ///// <returns>
    ///// A list of input parameters that are used by particle effectors but cannot be found in the
    ///// particle parameter collection of the particle system. For each missing parameter the list 
    ///// contains a pair: (<see cref="ParticleEffector"/>, parameter name). If there are no missing 
    ///// parameters, an empty list is returned.
    ///// </returns>
    ///// <exception cref="ArgumentNullException">
    ///// <paramref name="particleSystem"/> is <see langword="null"/>.
    ///// </exception>
    //public static List<Pair<ParticleEffector, string>> GetMissingInputParameters(this ParticleSystem particleSystem)
    //{
    //  if (particleSystem == null)
    //    throw new ArgumentNullException("particleSystem");

    //  List<string> parameters = particleSystem.Parameters.Select(p => p.Name).ToList();
    //  parameters.Add(null);
    //  parameters.Add(string.Empty);

    //  List<Pair<ParticleEffector, string>> missingParameters = new List<Pair<ParticleEffector, string>>();

    //  foreach (var effector in particleSystem.Effectors)
    //  {
    //    foreach (var inputParameter in effector.InputParameters)
    //    {
    //      if (!parameters.Contains(inputParameter))
    //        missingParameters.Add(new Pair<ParticleEffector, string>(effector, inputParameter));
    //    }
    //  }
      
    //  return missingParameters;
    //}


    ///// <summary>
    ///// Gets a list of missing output parameters.
    ///// </summary>
    ///// <param name="particleSystem">The particle system.</param>
    ///// <returns>
    ///// A list of output parameters that are used by particle effectors but cannot be found in the
    ///// particle parameter collection of the particle system. For each missing parameter the list 
    ///// contains a pair: (<see cref="ParticleEffector"/>, parameter name). If there are no missing 
    ///// parameters, an empty list is returned.
    ///// </returns>
    ///// <exception cref="ArgumentNullException">
    ///// <paramref name="particleSystem"/> is <see langword="null"/>.
    ///// </exception>
    //public static List<Pair<ParticleEffector, string>> GetMissingOutputParameters(this ParticleSystem particleSystem)
    //{
    //  if (particleSystem == null)
    //    throw new ArgumentNullException("particleSystem");

    //  List<string> parameters = particleSystem.Parameters.Select(p => p.Name).ToList();
    //  parameters.Add(null);
    //  parameters.Add(string.Empty);

    //  List<Pair<ParticleEffector, string>> missingParameters = new List<Pair<ParticleEffector, string>>();

    //  foreach (var effector in particleSystem.Effectors)
    //  {
    //    foreach (var outputParameter in effector.OutputParameters)
    //    {
    //      if (!parameters.Contains(outputParameter))
    //        missingParameters.Add(new Pair<ParticleEffector, string>(effector, outputParameter));
    //    }
    //  }

    //  return missingParameters;
    //}


    ///// <summary>
    ///// Gets a list of uninitialized parameters.
    ///// </summary>
    ///// <param name="particleSystem">The particle system.</param>
    ///// <returns>
    ///// A list of parameters that are not initialized by any particle effector. For each 
    ///// uninitialized parameter the list contains the parameter name. If there are no uninitialized 
    ///// parameters, an empty list is returned. (Uniform particle parameters where the 
    ///// <see cref="IParticleParameter{T}.DefaultValue"/> has been changed count as initialized.)
    ///// </returns>
    ///// <exception cref="ArgumentNullException">
    ///// <paramref name="particleSystem"/> is <see langword="null"/>.
    ///// </exception>
    //public static List<string> GetUninitializedParameters(this ParticleSystem particleSystem)
    //{
    //  if (particleSystem == null)
    //    throw new ArgumentNullException("particleSystem");

    //  List<string> parameters = particleSystem.Parameters.Select(p => p.Name).ToList();

    //  // Normalized age is handled by the particle system itself.
    //  parameters.Remove(ParticleParameterNames.NormalizedAge); 

    //  foreach (var effector in particleSystem.Effectors)
    //  {
    //    // An effector initializes a parameter if the parameter is in its OutputParameter list
    //    // and not in its InputParameter list. 
    //    foreach (var initializedParameter in effector.OutputParameters.Except(effector.InputParameters))
    //    {
    //      parameters.Remove(initializedParameter);
    //      if (parameters.Count == 0)
    //        return parameters;
    //    }
    //  }

    //  // Remove all uniform parameter where the DefaultValue is not null or default(T).
    //  foreach (var parameter in particleSystem.Parameters.OfType<IParticleParameterInternal>())
    //  {
    //    if (parameter.IsUniform && parameter.IsInitialized)
    //      parameters.Remove(((IParticleParameter)parameter).Name);
    //  }

    //  return parameters;
    //}


    /// <summary>
    /// Gets the world space pose of the particle system, which is obtained by concatenating the 
    /// poses of the given and all parent particle systems.
    /// </summary>
    /// <param name="particleSystem">The particle system.</param>
    /// <returns>The world space pose of the particle system.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="particleSystem"/> is <see langword="null"/>.
    /// </exception>
    public static Pose GetPoseWorld(this ParticleSystem particleSystem)
    {
      if (particleSystem == null)
        throw new ArgumentNullException("particleSystem");

      var pose = particleSystem.Pose;

      var parent = particleSystem.Parent;
      while (parent != null)
      {
        pose = parent.Pose * pose;
        parent = parent.Parent;
      }

      return pose;
    }


    /// <summary>
    /// Determines whether the specified particle system is alive.
    /// </summary>
    /// <param name="particleSystem">The particle system.</param>
    /// <returns>
    /// <see langword="true"/> if the specified particle system is alive; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method checks if the <see cref="ParticleSystem.NumberOfLivingParticles"/> of the
    /// specified particle system or any of its child particle systems is greater than 0. If the 
    /// particle system and all children do not have any "living" particles, this method returns 
    /// <see langword="false"/>.
    /// </para>
    /// <para>
    /// Important note: This method returns <see langword="true"/> if there are temporarily no 
    /// living particles in the particle system. It can still happen that the particle system 
    /// contains effectors that will add new particles in the future. 
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="particleSystem"/> is <see langword="null"/>.
    /// </exception>
    public static bool IsAlive(this ParticleSystem particleSystem)
    {
      if (particleSystem == null)
        throw new ArgumentNullException("particleSystem");

      if (particleSystem.NumberOfLivingParticles > 0)
        return true;

      if (particleSystem.Children != null)
      {
        foreach (var child in particleSystem.Children)
        {
          if (child.IsAlive())
            return true;
        }
      }

      return false;
    }
  }
}
