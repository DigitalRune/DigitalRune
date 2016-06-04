namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Describes a single overload for the <see cref="OverloadProvider"/>.
    /// </summary>
    public class OverloadDescription
    {
        /// <summary>
        /// Gets or sets the text 'SelectedIndex of Count'.
        /// </summary>
        /// <value>
        /// The text 'SelectedIndex of Count'. The default value is <see langword="null"/>.
        /// </value>
        public string IndexText { get; set; }


        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>The header. The default value is <see langword="null"/>.</value>
        public object Header { get; set; }


        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content. The default value is <see langword="null"/>.</value>
        public object Content { get; set; }


        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="OverloadDescription"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="OverloadDescription"/> class.
        /// </summary>
        public OverloadDescription()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="OverloadDescription"/> class.
        /// </summary>
        /// <param name="indexText">The index text.</param>
        /// <param name="header">The header.</param>
        /// <param name="content">The content.</param>
        public OverloadDescription(string indexText, object header, object content)
        {
            IndexText = indexText;
            Header = header;
            Content = content;
        }
    }
}
