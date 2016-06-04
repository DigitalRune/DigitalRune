// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Describes a text label of a <see cref="TextScale"/>.
    /// </summary>
    public class TextLabel
    {
        /// <summary>
        /// Gets or sets the detailed description.
        /// </summary>
        /// <value>The detailed description.</value>
        /// <remarks>
        /// This is additional information for the text label which can be shown in a tooltip.
        /// </remarks>
        public string Description { get; set; }


        /// <summary>
        /// Gets or sets the text of the label.
        /// </summary>
        /// <value>The text of the label.</value>
        public string Text { get; set; }


        /// <summary>
        /// Gets or sets the value of the label.
        /// </summary>
        /// <value>The value at which the label is shown.</value>
        public double Value { get; set; }


        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="TextLabel"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="TextLabel"/> class.
        /// </summary>
        public TextLabel()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TextLabel"/> class with the given value and
        /// text.
        /// </summary>
        /// <param name="value">The value of the label.</param>
        /// <param name="text">The text of the label.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="text"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="text"/> is empty.</exception>
        public TextLabel(double value, string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (text.Length == 0)
                throw new ArgumentException("Text must not be empty.", "text");

            Value = value;
            Text = text;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TextLabel"/> class with the given value,
        /// text, and description.
        /// </summary>
        /// <param name="value">The value of the label.</param>
        /// <param name="text">The text of the label.</param>
        /// <param name="description">The description of the label.</param>
        public TextLabel(double value, string text, string description)
            : this(value, text)
        {
            Description = description;
        }


        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current
        /// <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="Object"/> to compare with the current <see cref="Object"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Object"/> is equal to the current
        /// <see cref="Object"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (TextLabel)obj;
            return Value == other.Value && Text == other.Text && Description == other.Description;
        }


        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            int textHash = (Text != null) ? Text.GetHashCode() : 0;
            int descriptionHash = (Description != null) ? Description.GetHashCode() : 0;
            return Value.GetHashCode() ^ textHash ^ descriptionHash;
        }
    }
}
