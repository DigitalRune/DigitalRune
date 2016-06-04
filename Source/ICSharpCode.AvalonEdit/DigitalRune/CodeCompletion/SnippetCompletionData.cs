using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Snippets;


namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// <see cref="ICompletionData"/> for a text snippet.
    /// </summary>
    /// <remarks>
    /// If the <see cref="Snippet"/> contains a '|' character, then the cursor is placed at this
    /// position after the snippet was inserted. The '|' is not inserted.
    /// </remarks>
    public class SnippetCompletionData : ICompletionData
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        public object Image { get; set; }


        /// <summary>
        /// Gets or sets the text. 
        /// </summary>
        /// <value>The text.</value>
        /// <remarks>
        /// This property is used to filter the list of visible elements.
        /// This property is also the text that will be inserted.
        /// </remarks>
        public string Text { get; set; }


        /// <summary>
        /// Gets or sets the displayed content.
        /// </summary>
        /// <value>
        /// The displayed content. This can be the same as 'Text', or a WPF UIElement if you want to 
        /// display rich content. The default value is <see langword="null"/>.
        /// </value>
        public object Content { get; set; }


        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description. The default value is <see langword="null"/>.</value>
        public object Description { get; set; }


        /// <summary>
        /// Gets or sets the priority. This property is used in the selection logic. You can use it
        /// to prefer selecting those items which the user is accessing most frequently.
        /// </summary>
        /// <value>The priority. The default value is 0.</value>
        public double Priority { get; set; }


        /// <summary>
        /// Gets or sets the code snippet.
        /// </summary>
        /// <value>The code snippet.</value>
        public Snippet Snippet { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="SnippetCompletionData"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="SnippetCompletionData"/> class.
        /// </summary>
        public SnippetCompletionData()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SnippetCompletionData"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="image">The image.</param>
        /// <param name="snippet">The snippet.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="snippet"/> is <see langword="null"/>.
        /// </exception>
        public SnippetCompletionData(string text, object description, object image, Snippet snippet)
        {
            if (snippet == null)
                throw new ArgumentNullException(nameof(snippet));

            Text = text;
            Content = text;
            Description = description;
            Image = image;
            Snippet = snippet;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            // Remove completion segment.
            textArea.Document.Remove(completionSegment);

            // Insert snippet.
            Snippet.Insert(textArea);
        }
        #endregion
    }
}
