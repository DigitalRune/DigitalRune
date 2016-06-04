using System.Windows.Controls;
using System.Windows.Markup;


namespace SampleApplication
{
    /// <summary>
    /// Describes an example in this sample application.
    /// </summary>
    /// <remarks>
    /// Examples are encapsulated in <see cref="UserControl"/>s.
    /// </remarks>
    [ContentProperty("Sample")]
    public class SampleInfo
    {
        /// <summary>
        /// Gets or sets the title of the example.
        /// </summary>
        /// <value>The title of the example.</value>
        public string Title { get; set; }


        /// <summary>
        /// Gets or sets the description of the example.
        /// </summary>
        /// <value>The description of the example.</value>
        public string Description { get; set; }


        /// <summary>
        /// Gets or sets the <see cref="UserControl"/> that represents the example.
        /// </summary>
        /// <value>The <see cref="UserControl"/> that represents the example.</value>
        public UserControl Sample { get; set; }
    }
}
