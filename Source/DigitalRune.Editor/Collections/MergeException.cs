// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Runtime.Serialization;


namespace DigitalRune.Collections
{
    /// <summary>
    /// The exception that is thrown when two collections of type <see cref="MergeableNode{T}"/>
    /// cannot be merged.
    /// </summary>
    [Serializable]
    public class MergeException : Exception
    {
        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="MergeException"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="MergeException"/> class.
        /// </summary>
        public MergeException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeException"/> class with the given
        /// message.
        /// </summary>
        /// <param name="message">The message.</param>
        public MergeException(string message) : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeException"/> class with the given
        /// message and inner exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public MergeException(string message, Exception inner) : base(message, inner)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The serialization context.</param>
        protected MergeException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}
