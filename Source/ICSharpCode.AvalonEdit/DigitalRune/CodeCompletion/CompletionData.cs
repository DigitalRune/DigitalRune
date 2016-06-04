using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;


namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Describes an entry in the <see cref="CompletionList"/>. (Default implementation of
    /// <see cref="ICompletionData"/>.)
    /// </summary>
    public class CompletionData : ICompletionData
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
        /// <value>The image. The default value is <see langword="null"/>.</value>
        public object Image { get; set; }


        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text. The default value is <see langword="null"/>.</value>
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
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="CompletionData"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="CompletionData"/> class.
        /// </summary>
        public CompletionData()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CompletionData"/> class.
        /// </summary>
        /// <param name="text">The text/content.</param>
        /// <param name="description">The description.</param>
        /// <param name="image">The image.</param>
        public CompletionData(string text, object description, object image)
        {
            Text = text;
            Content = text;
            Description = description;
            Image = image;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CompletionData"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="description">The description.</param>
        /// <param name="image">The image.</param>
        /// <param name="content">The content.</param>
        public CompletionData(string text, object description, object image, object content)
        {
            Text = text;
            Content = content;
            Description = description;
            Image = image;
        }

        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            if (!textArea.Selection.IsEmpty)
            {
                // ----- PasteMultiple:
                // By default the completion data does not affect the current selection.
                // But in TextEditor.OnPasteMultiple the completion should behave like
                // the Paste command.
                textArea.Document.Remove(completionSegment);
                textArea.ReplaceSelectionWithText(Text);
            }
            else
            {
                textArea.Document.Replace(completionSegment, Text);
            }
        }
        #endregion
    }
}
