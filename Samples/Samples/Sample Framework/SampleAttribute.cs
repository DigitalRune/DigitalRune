using System;
using System.Linq;
using System.Reflection;


namespace Samples
{
  /// <summary>
  /// Describes a game component which implements a single sample.
  /// </summary>
  /// <remarks>
  /// In the menu the samples are ordered by category and order. The summary and description
  /// will be printed on the Help screen when the sample is active.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class SampleAttribute : Attribute
  {
    /// <summary>
    /// Gets the category of the sample.
    /// </summary>
    /// <value>The category of the sample.</value>
    public SampleCategory Category { get; private set; }


    /// <summary>
    /// Gets a short summary of the sample.
    /// </summary>
    /// <value>The a short summary sample.</value>
    public string Summary { get; private set; }


    /// <summary>
    /// Gets the detailed description of the sample.
    /// </summary>
    /// <value>The detailed description of the sample.</value>
    public string Description { get; private set; }


    /// <summary>
    /// Gets the order used to sort samples of the same category.
    /// </summary>
    /// <value>The order used to sort samples of the same category.</value>
    public int Order { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="SampleAttribute" /> class.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="summary">The summary.</param>
    /// <param name="description">The description.</param>
    /// <param name="order">The order.</param>
    public SampleAttribute(SampleCategory category, string summary, string description, int order)
    {
      if (summary == null)
        throw new ArgumentNullException("summary");
      if (description == null)
        throw new ArgumentNullException("description");

      Category = category;
      Summary = summary;
      Description = description;
      Order = order;
    }


    /// <summary>
    /// Gets the sample attribute for a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The sample attribute.</returns>
    public static SampleAttribute GetSampleAttribute(Type type)
    {
#if NETFX_CORE
      return type.GetTypeInfo().GetCustomAttribute<SampleAttribute>();
#else
      return type.GetCustomAttributes(false).OfType<SampleAttribute>().FirstOrDefault();
#endif
    }
  }
}
