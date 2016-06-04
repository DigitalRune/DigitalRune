using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DigitalRune.Particles;


namespace Samples.Particles
{
  // This helper class can check a particle system and determine if there is anything wrong
  // with the configuration. 
  // It checks for following problems:
  // - Is there a particle effector property for a particle parameter that is not set?
  //   (E.g. if there is a SingleLerpEffector with ValueParameter == null, then someone probably
  //   forgot to set this property to a reasonable parameter name.)
  // - Is there a particle parameter that is required by an effector but is missing in 
  //   the particle system?
  //   (E.g. if there is a SingleLerpEffector with ValueParameter == "ParamXY" and the
  //   particle system does not contain a "ParamXY" parameter, then someone probably forgot
  //   to call particleSystem.Parameters.AddUniform/AddVarying("ParamXY").)
  // - Is there a particle parameter that is not initialized by a start value effector?
  //   (For each varying parameter, there should be an effector that initializes this value
  //   for new particles.)
  public static class ParticleSystemValidator
  {
    // Checks for common configuration errors and writes a message.
    [Conditional("DEBUG")]
    public static void Validate(ParticleSystem particleSystem)
    {
      var message1 = CheckForUninitializedEffectorProperties(particleSystem);
      var message2 = CheckForMissingParticleParameters(particleSystem);
      var message3 = CheckForUninitializedParticleParameters(particleSystem);

      if (message1 != null || message2 != null || message3 != null)
      {
        Debug.WriteLine("----- \"{0}\" (type {1}):", particleSystem.Name, particleSystem.GetType().Name);

        if (message1 != null)
          Debug.WriteLine(message1);

        if (message2 != null)
          Debug.WriteLine(message2);

        if (message3 != null)
          Debug.WriteLine(message3);
      }
    }


    // Is there a particle effector property for a particle parameter that is not set?
    public static string CheckForUninitializedEffectorProperties(ParticleSystem particleSystem)
    {
      string message = "Following effector properties are required but not set: ";
      bool uninitializedPropertyFound = false;

      foreach (var effector in particleSystem.Effectors)
      {
        foreach (var propertyInfo in GetProperties(effector))
        {
          // Handle effector properties with a ParticleParameterAttribute.
          var particleParameterAttribute = propertyInfo.GetCustomAttributes(typeof(ParticleParameterAttribute), true).Cast<ParticleParameterAttribute>().FirstOrDefault();
          if (particleParameterAttribute != null && !particleParameterAttribute.Optional)
          {
            var parameterName = propertyInfo.GetValue(effector, null) as string;
            if (string.IsNullOrEmpty(parameterName))
            {
              // The property is a mandatory particle parameter name and is not set.
              uninitializedPropertyFound = true;
              message += string.Format("{0}.{1} ", effector.GetType().Name, propertyInfo.Name);
            }
          }
        }
      }

      return uninitializedPropertyFound ? message : null;
    }


    // Is there a particle parameter that is required by an effector but is missing in the 
    // particle system?
    public static string CheckForMissingParticleParameters(ParticleSystem particleSystem)
    {
      string message = "Parameters missing in particle system: ";
      List<string> missing = new List<string>();

      foreach (var effector in particleSystem.Effectors)
      {
        foreach (var propertyInfo in GetProperties(effector))
        {
          // Handle effector properties with a ParticleParameterAttribute.
          var particleParameterAttribute = propertyInfo.GetCustomAttributes(typeof(ParticleParameterAttribute), true).Cast<ParticleParameterAttribute>().FirstOrDefault();
          if (particleParameterAttribute != null)
          {
            var parameterName = propertyInfo.GetValue(effector, null) as string;

            if (string.IsNullOrEmpty(parameterName))
              continue;                // Property is probably not in use.

            if (particleSystem.Parameters.Contains(parameterName))
              continue;                // Parameter was found in the particle system.

            if (missing.Contains(parameterName))
              continue;                // Parameter was already reported as missing.

            if (particleSystem.Parent != null && particleSystem.Parent.Parameters.Any(p => p.IsUniform && p.Name == parameterName))
              continue;                // Parameter is uniform and was found in the parent particle system.

            // The particle parameter is missing.
            missing.Add(parameterName);
            message += string.Format("\"{0}\" ", parameterName);
          }
        }
      }

      return missing.Count > 0 ? message : null;
    }


    // Is there a particle parameter that is not initialized by a start value effector?
    public static string CheckForUninitializedParticleParameters(ParticleSystem particleSystem)
    {
      // Get the names of all varying particle parameters
      List<string> parameters = particleSystem.Parameters.Where(p => !p.IsUniform).Select(p => p.Name).ToList();

      // "NormalizedAge" is the only parameter handled by the particle system itself.
      parameters.Remove(ParticleParameterNames.NormalizedAge);

      foreach (var effector in particleSystem.Effectors)
      {
        foreach (var propertyInfo in GetProperties(effector))
        {
          // Handle effector properties with a ParticleParameterAttribute.
          var particleParameterAttribute = propertyInfo.GetCustomAttributes(typeof(ParticleParameterAttribute), true).Cast<ParticleParameterAttribute>().FirstOrDefault();
          if (particleParameterAttribute != null)
          {
            var parameterName = propertyInfo.GetValue(effector, null) as string;
            if (!string.IsNullOrEmpty(parameterName) && particleParameterAttribute.Usage == ParticleParameterUsage.Out)
            {
              // The effector initializes this particle parameter.
              parameters.Remove(parameterName);
            }
          }
        }
      }

      if (parameters.Count > 0)
      {
        string message = "Parameters possibly not initialized by any start value effector: ";
        foreach (var parameter in parameters)
          message += string.Format("\"{0}\" ", parameter);
        return message;
      }

      return null;
    }


    private static IEnumerable<PropertyInfo> GetProperties(object obj)
    {
#if NETFX_CORE
      return obj.GetType().GetRuntimeProperties();
#else
      return obj.GetType().GetProperties();
#endif
    }
  }
}
