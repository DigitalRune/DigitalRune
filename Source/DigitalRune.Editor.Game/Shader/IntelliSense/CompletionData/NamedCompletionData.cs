// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// <see cref="CompletionData"/> that has a name.
    /// </summary>
    /// <remarks>
    /// The name is equal to <see cref="CompletionData.Text"/>.
    /// <see cref="CompletionData.Text"/> must not be changed after this object was created.
    /// </remarks>
    internal class NamedCompletionData : ICompletionData, INamedObject
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets (or sets) the displayed content.
        /// </summary>
        /// <value>
        /// The displayed content. This can be the same as 'Text', or a WPF UIElement if you want to 
        /// display rich content. The default value is <see langword="null"/>.
        /// </value>
        public object Content { get; protected set; }


        /// <summary>
        /// Gets (or sets) the description.
        /// </summary>
        /// <value>The description. The default value is <see langword="null"/>.</value>
        public object Description { get; protected set; }


        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <value>The image.</value>
        public object Image { get; }


        /// <summary>
        /// Gets the name. (Same as <see cref="Text"/>.)
        /// </summary>
        /// <value>
        /// The name of the object. (Same as <see cref="Text"/>.)
        /// </value>
        public string Name
        {
            get { return Text; }
        }


        /// <summary>
        /// Gets the priority. This property is used in the selection logic. You can use it to prefer selecting those items
        /// which the user is accessing most frequently.
        /// </summary>
        /// <value>The priority. The default value is 0.</value>
        public double Priority { get; }


        /// <summary>
        /// Gets (or sets) the text. 
        /// </summary>
        /// <value>The text.</value>
        /// <remarks>
        /// This property is used to filter the list of visible elements.
        /// This property is also the text that will be inserted.
        /// </remarks>
        public string Text { get; protected set; }

        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletionData"/> class.
        /// </summary>
        /// <param name="text">The text (must not be <see langword="null"/> or empty).</param>
        /// <param name="description">The description.</param>
        /// <param name="image">The image.</param>
        public NamedCompletionData(string text, object description, object image)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (text.Length == 0)
                throw new ArgumentException("Parameter text must not be empty.", nameof(text));

            Text = text;
            Content = text;
            Description = description;
            Image = image;
            Priority = 0;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Perform the completion.
        /// </summary>
        /// <param name="textArea">The text area on which the completion is performed.</param>
        /// <param name="completionSegment">
        /// The text segment that was used by the completion window if the user types (segment between
        /// CompletionWindow.StartOffset and CompletionWindow.EndOffset).
        /// </param>
        /// <param name="insertionRequestEventArgs">
        /// The EventArgs used for the insertion request. These can be TextCompositionEventArgs,
        /// KeyEventArgs, MouseEventArgs, depending on how the insertion was triggered.
        /// </param>
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            // Insert Text.
            textArea.Document.Replace(completionSegment, Text);
        }
        #endregion
    }
}
