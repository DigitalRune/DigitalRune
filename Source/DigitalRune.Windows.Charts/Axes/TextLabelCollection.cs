// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// A collection of <see cref="TextLabel"/> objects.
    /// </summary>
    public class TextLabelCollection : ObservableCollection<TextLabel>
    {
        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="TextLabelCollection"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="TextLabelCollection"/> class.
        /// </summary>
        public TextLabelCollection()
        {
        }


#if WINDOWS_PHONE
        /// <summary>
        /// Initializes a new instance of the <see cref="TextLabelCollection"/> class with the given
        /// text labels.
        /// </summary>
        /// <param name="textLabels">The text labels.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="textLabels"/> is <see langword="null"/>.
        /// </exception>
        public TextLabelCollection(IEnumerable<TextLabel> textLabels)
        {
            if (textLabels == null)
                throw new ArgumentNullException("textLabels");

            foreach (TextLabel textLabel in textLabels)
                Add(textLabel);
        }
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="TextLabelCollection"/> class with the given
        /// text labels.
        /// </summary>
        /// <param name="textLabels">The text labels.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="textLabels"/> is <see langword="null"/>.
        /// </exception>
        public TextLabelCollection(IEnumerable<TextLabel> textLabels)
            : base(textLabels)
        {
        }
#endif


#if WINDOWS_PHONE
        /// <summary>
        /// Initializes a new instance of the <see cref="TextLabelCollection"/> class with the given
        /// list of text labels.
        /// </summary>
        /// <param name="textLabels">The text labels.</param>
        public TextLabelCollection(List<TextLabel> textLabels)
        {
            if (textLabels == null)
                throw new ArgumentNullException("textLabels");

            foreach (TextLabel textLabel in textLabels)
                Add(textLabel);
        }
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="TextLabelCollection"/> class with the given
        /// list of text labels.
        /// </summary>
        /// <param name="textLabels">The text labels.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public TextLabelCollection(List<TextLabel> textLabels)
            : base(textLabels)
        {
        }
#endif
    }
}
