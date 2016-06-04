// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Windows;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Describes a text search query.
    /// </summary>
    [Serializable]
    public class SearchQuery : ObservableObject
    {
        /// <summary>
        /// Gets or sets the find pattern.
        /// </summary>
        /// <value>The find pattern. The default value is <see langword="null"/>.</value>
        public string FindPattern
        {
            get { return _findPattern; }
            set
            {
                if (_findPattern == value)
                    return;

                SetProperty(ref _findPattern, TrimNewLine(value));
            }
        }
        private string _findPattern;


        /// <summary>
        /// Gets or sets the replace pattern.
        /// </summary>
        /// <value>The replace pattern. The default value is <see langword="null"/>.</value>
        public string ReplacePattern
        {
            get { return _replacePattern; }
            set
            {
                if (_replacePattern == value)
                    return;

                SetProperty(ref _replacePattern, TrimNewLine(value));
            }
        }
        private string _replacePattern;


        /// <summary>
        /// Gets or sets a value indicating whether the search is case-sensitive.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the search is case-sensitive; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        public bool MatchCase
        {
            get { return _matchCase; }
            set { SetProperty(ref _matchCase, value); }
        }
        private bool _matchCase;


        /// <summary>
        /// Gets or sets a value indicating whether the whole word must match when comparing the
        /// content with the search pattern.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the whole word must match; otherwise, <see langword="false"/>.
        /// The default value is <see langword="false"/>.
        /// </value>
        public bool MatchWholeWord
        {
            get { return _matchWholeWorld; }
            set { SetProperty(ref _matchWholeWorld, value); }
        }
        private bool _matchWholeWorld;


        /// <summary>
        /// Gets or sets the search mode.
        /// </summary>
        /// <value>The search mode.</value>
        public SearchMode Mode
        {
            get { return _mode; }
            set { SetProperty(ref _mode, value); }
        }
        private SearchMode _mode;


        /// <summary>
        /// Gets or sets the search scope.
        /// </summary>
        /// <value>The search scope. The default value is <see langword="null"/>.</value>
        public ISearchScope Scope
        {
            get { return _scope; }
            set { SetProperty(ref _scope, value); }
        }
        private ISearchScope _scope;


        /// <summary>
        /// Gets or sets a value indicating whether the search direction is forward/down or
        /// backward/up.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the search direction is backwards/up; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        public bool SearchBackwards
        {
            get { return _searchBackwards; }
            set { SetProperty(ref _searchBackwards, value); }
        }
        private bool _searchBackwards;


        /// <summary>
        /// Gets or sets a value indicating whether to start the search relative to the local
        /// position in the document.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the search starts at the local position; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        public bool IsRelative
        {
            get { return _isRelative; }
            set { SetProperty(ref _isRelative, value); }
        }
        private bool _isRelative = true;


        /// <summary>
        /// Cuts the specified text off at the first newline character.
        /// </summary>
        /// <param name="text">The text to trim.</param>
        /// <returns>The trimmed text.</returns>
        private static string TrimNewLine(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                int endOfLine = text.IndexOfAny(new[] { '\r', '\n' });
                if (endOfLine >= 0)
                    text = text.Substring(0, endOfLine);
            }

            return text;
        }


        /// <summary>
        /// Converts this search query into a regular expression.
        /// </summary>
        /// <returns>
        /// The regular expression, or <see langword="null"/> if the current query cannot be
        /// converted to a regular expression.
        /// </returns>
        public Regex AsRegex()
        {
            if (string.IsNullOrEmpty(FindPattern))
                return null;

            var pattern = FindPattern;
            if (Mode == SearchMode.Normal)
                pattern = Regex.Escape(pattern);
            else if (Mode == SearchMode.Wildcards)
                pattern = ConvertWildcardsToRegex(pattern);

            if (MatchWholeWord)
                pattern = @"\b" + pattern + @"\b";  // \b matches word boundary.

            var options = RegexOptions.Compiled | RegexOptions.Multiline;
            if (!MatchCase)
                options |= RegexOptions.IgnoreCase;

            try
            {
                return new Regex(pattern, options);
            }
            catch (Exception)
            {
                return null;
            }
        }


        private static string ConvertWildcardsToRegex(string findPattern)
        {
            var stringBuilder = new StringBuilder();
            foreach (var c in findPattern)
            {
                if (c == '?')
                    stringBuilder.Append(".");
                else if (c == '*')
                    stringBuilder.Append(".*");
                else
                    stringBuilder.Append(Regex.Escape(c.ToString(CultureInfo.InvariantCulture)));
            }

            return stringBuilder.ToString();
        }
    }
}