using System;
using System.Collections.Generic;


namespace DigitalRune.Animation.Tests
{
  internal class AnimatableObject : IAnimatableObject
  {
    public string Name { get; private set; }


    // Note: The AnimatableObject uses a simple Dictionary to store the properties 
    // and therefore cannot have multiple properties with same name but different type.
    public Dictionary<string, IAnimatableProperty> Properties
    {
      get { return _properties; }
    }
    private readonly Dictionary<string, IAnimatableProperty> _properties = new Dictionary<string, IAnimatableProperty>();


    public IEnumerable<IAnimatableProperty> GetAnimatedProperties()
    {
      return _properties.Values;
    }


    public IAnimatableProperty<T> GetAnimatableProperty<T>(string name)
    {
      IAnimatableProperty property;
      Properties.TryGetValue(name, out property);
      return property as IAnimatableProperty<T>;
    }

  
    public AnimatableObject(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");

      if (name.Length == 0)
        throw new ArgumentException("Name must not be empty.", "name");

      Name = name;
    }
  }
}
