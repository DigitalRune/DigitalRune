// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// Stores <see cref="ArgumentResult"/>s.
    /// </summary>
    public class ArgumentResultCollection : ReadOnlyCollection<ArgumentResult>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the <see cref="ArgumentResult"/> for the specified argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The <see cref="ArgumentResult"/> for the specified argument or <see langword="null"/>
        /// if no matching <see cref="ArgumentResult"/> is found.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public ArgumentResult this[Argument argument]
        {
            get
            {
                foreach (var result in Items)
                {
                    if (result.Argument == argument)
                        return result;
                }

                return null;
            }
        }


        /// <summary>
        /// Gets the <see cref="ArgumentResult"/> for the specified argument.
        /// </summary>
        /// <param name="argumentName">The argument name (not an alias!).</param>
        /// <returns>
        /// The <see cref="ArgumentResult"/> for the specified argument name or 
        /// <see langword="null"/> if no matching <see cref="ArgumentResult"/> is found.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.String.Compare(System.String,System.String,System.StringComparison)")]
        public ArgumentResult this[string argumentName]
        {
            get
            {
                foreach (var result in Items)
                {
                    if (string.Compare(result.Argument.Name,
                                       argumentName,
                                       StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        return result;
                    }
                }

                return null;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentResultCollection"/> class.
        /// </summary>
        /// <param name="results">The results.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public ArgumentResultCollection(List<ArgumentResult> results) : base(results)
        {
            // The parameter is List and not IList to be able to allow to cast to 
            // List in GetEnumerator below.
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ArgumentResultCollection"/>. 
        /// </summary>
        /// <returns>
        /// An <see cref="List{T}.Enumerator"/> for <see cref="ArgumentResultCollection"/>.
        /// </returns>
        public new List<ArgumentResult>.Enumerator GetEnumerator()
        {
            return ((List<ArgumentResult>)Items).GetEnumerator();
        }
        #endregion
    }
}
