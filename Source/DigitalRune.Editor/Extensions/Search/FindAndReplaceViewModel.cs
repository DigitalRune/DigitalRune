// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Represents the Find and Replace dialog.
    /// </summary>
    internal class FindAndReplaceViewModel : EditorDockTabItemViewModel
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        internal const string DockIdString = "FindAndReplace";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly SearchExtension _searchExtension;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a <see cref="FindAndReplaceViewModel"/> instance that can be used at design-time.
        /// </summary>
        /// <value>
        /// A <see cref="FindAndReplaceViewModel"/> instance that can be used at design-time.
        /// </value>
        internal static FindAndReplaceViewModel DesignInstance
        {
            get
            {
                return new FindAndReplaceViewModel(null)
                {
                    _useRegexOrWildcards = true,
                    _regexOrWildcards = SearchMode.Regex,
                };
            }
        }


        /// <summary>
        /// Gets the recent find patterns.
        /// </summary>
        /// <value>The recent find patterns.</value>
        public IEnumerable<string> RecentFindPatterns
        {
            get { return _searchExtension.RecentFindPatterns; }
        }


        /// <summary>
        /// Gets the recent replacement strings.
        /// </summary>
        /// <value>The recent replacement strings.</value>
        public IEnumerable<string> RecentReplacePatterns
        {
            get { return _searchExtension.RecentReplacePatterns; }
        }


        /// <summary>
        /// Gets the search query.
        /// </summary>
        /// <value>The search query.</value>
        public SearchQuery Query
        {
            get { return _searchExtension.Query; }
        }


        /// <summary>
        /// Gets the search scopes.
        /// </summary>
        /// <value>The search scopes.</value>
        public IList<ISearchScope> SearchScopes
        {
            get { return _searchExtension.SearchScopes; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to use regular expressions or wildcards.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to use regular expressions or wildcards; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        public bool UseRegexOrWildcards
        {
            get { return _useRegexOrWildcards; }
            set
            {
                if (_useRegexOrWildcards == value)
                    return;

                _useRegexOrWildcards = value;

                // UseRegexOrWildcards and RegexOrWildcards need to be synced with the SearchQuery.Mode.
                Query.Mode = value ? RegexOrWildcards : SearchMode.Normal;

                RaisePropertyChanged(() => UseRegexOrWildcards);
            }
        }
        private bool _useRegexOrWildcards;


        /// <summary>
        /// Gets or sets a value indicating whether <see cref="SearchMode.Regex"/> or 
        /// <see cref="SearchMode.Wildcards"/> are selected.
        /// </summary>
        /// <value>
        /// A value indicating whether <see cref="SearchMode.Regex"/> or 
        /// <see cref="SearchMode.Wildcards"/> are selected.
        /// </value>
        public SearchMode RegexOrWildcards
        {
            get { return _regexOrWildcards; }
            set
            {
                if (_regexOrWildcards == value)
                    return;

                _regexOrWildcards = value;

                // UseRegexOrWildcards and RegexOrWildcards need to be synced with the SearchQuery.Mode.
                if (UseRegexOrWildcards)
                    Query.Mode = value;

                RaisePropertyChanged(() => RegexOrWildcards);
            }
        }
        private SearchMode _regexOrWildcards;


        /// <summary>
        /// Gets the 'Find Previous' command.
        /// </summary>
        /// <value>The 'Find Previous' command.</value>
        public ICommand FindPreviousCommand { get; private set; }


        /// <summary>
        /// Gets the 'Find Next' command.
        /// </summary>
        /// <value>The 'Find Next' command.</value>
        public ICommand FindNextCommand { get; private set; }


        /// <summary>
        /// Gets the 'Replace' command.
        /// </summary>
        /// <value>The 'Replace' command.</value>
        public ICommand ReplaceCommand { get; private set; }


        /// <summary>
        /// Gets the 'Replace All' command.
        /// </summary>
        /// <value>The 'Replace All' command.</value>
        public ICommand ReplaceAllCommand { get; private set; }


        /// <summary>
        /// Gets the 'Cancel' command.
        /// </summary>
        /// <value>The 'Cancel' command.</value>
        public DelegateCommand CancelCommand { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="FindAndReplaceViewModel"/> class.
        /// </summary>
        /// <param name="searchExtension">
        /// The search extension. Can be <see langword="null"/> at design-time.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="searchExtension"/> is <see langword="null"/>.
        /// </exception>
        public FindAndReplaceViewModel(SearchExtension searchExtension)
        {
            DisplayName = "Find & Replace";
            DockId = DockIdString;
            IsPersistent = true;
            DockWidth = new GridLength(200);
            DockHeight = new GridLength(1, GridUnitType.Auto);
            //Icon = MultiColorGlyphs.Find;

            if (!WindowsHelper.IsInDesignMode)
            {
                if (searchExtension == null)
                    throw new ArgumentNullException(nameof(searchExtension));

                _searchExtension = searchExtension;

                if (Query.Mode == SearchMode.Normal)
                {
                    _useRegexOrWildcards = false;
                    _regexOrWildcards = SearchMode.Regex;
                }
                else
                {
                    _useRegexOrWildcards = true;
                    _regexOrWildcards = Query.Mode;
                }

                Query.PropertyChanged += OnQueryChanged;

                FindPreviousCommand = ((CommandItem)searchExtension.CommandItems["FindPrevious"]).Command;
                FindNextCommand = ((CommandItem)searchExtension.CommandItems["FindNext"]).Command;
                ReplaceCommand = ((CommandItem)searchExtension.CommandItems["Replace"]).Command;
                ReplaceAllCommand = ((CommandItem)searchExtension.CommandItems["ReplaceAll"]).Command;
                CancelCommand = new DelegateCommand(Cancel);
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnQueryChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            var query = (SearchQuery)sender;
            UseRegexOrWildcards = (query.Mode != SearchMode.Normal);
        }


        private void Cancel()
        {
            Query.FindPattern = null;
        }
        #endregion
    }
}
