// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;


namespace DigitalRune.Editor.Search
{
    partial class SearchExtension
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private const int MaxNumberOfCachedEntries = 20;
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private IEnumerator<ISearchable> _searchableIterator;
        private IEnumerator<ISearchResult> _searchResultIterator;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the recent find patterns.
        /// </summary>
        /// <value>The recent find patterns.</value>
        internal IEnumerable<string> RecentFindPatterns
        {
            get { return _recentFindPatterns; }
        }
        private ObservableCollection<string> _recentFindPatterns;


        /// <summary>
        /// Gets the recent replacement strings.
        /// </summary>
        /// <value>The recent replacement strings.</value>
        internal IEnumerable<string> RecentReplacePatterns
        {
            get { return _recentReplacePatterns; }
        }
        private ObservableCollection<string> _recentReplacePatterns;


        /// <inheritdoc/>
        public SearchQuery Query { get; private set; }


        /// <inheritdoc/>
        public IList<ISearchScope> SearchScopes { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        private void InitializeSearchService()
        {
            _recentFindPatterns = new ObservableCollection<string>();
            _recentReplacePatterns = new ObservableCollection<string>();

            Query = new SearchQuery();
            Query.PropertyChanged += OnQueryChanged;

            SearchScopes = new SearchScopeCollection(this);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void Reset<T>(ref IEnumerator<T> enumerator)
        {
            enumerator?.Dispose();
            enumerator = null;
        }


        /// <summary>
        /// Determines whether the Find Next/Previous actions can be executed.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the Find Next/Previous actions can be executed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        internal bool CanFind()
        {
            // CanFind returns true if we have a search patterns and any searchables.

            if (string.IsNullOrEmpty(Query.FindPattern))
                return false;

            // Try to initialize Query.Scope with a default scope.
            // We use the first available SearchScope.
            if (Query.Scope == null)
                Query.Scope = SearchScopes.FirstOrDefault();

            return Query.Scope?.Searchables != null && Query.Scope.Searchables.Any();
        }


        /// <summary>
        /// Jumps to the previous search result.
        /// </summary>
        internal void FindPrevious()
        {
            Find(true);
        }


        /// <summary>
        /// Jumps to the next search result.
        /// </summary>
        internal void FindNext()
        {
            Find(false);
        }


        private void Find(bool searchBackwards)
        {
            Debug.Assert(Query != null);
            Debug.Assert(SearchScopes != null);

            if (SearchScopes.Count == 0)
            {
                Logger.Warn("No search scopes are registered. Cannot perform search.");
                return;
            }

            if (Query.Scope == null)
            {
                Logger.Warn("No search scope selected in query. Cannot perform search.");
                return;
            }

            // Store recently used patterns.
            AddRecentEntry(_recentFindPatterns, Query.FindPattern);

            // Update search direction. This will automatically restart the search if the 
            // search direction has changed.
            Query.SearchBackwards = searchBackwards;

            // If we have a result from an FindNext but the selection was moved, we have to 
            // restart.
            if (_searchResultIterator != null)
            {
                if (!_searchResultIterator.Current.IsSelected)
                {
                    // The user has interacted and changed the selection. We have to start a new search.
                    Reset(ref _searchableIterator);
                    Reset(ref _searchResultIterator);
                }
            }

            // Initialize document iterator.
            if (_searchableIterator == null)
            {
                _searchableIterator = Query.Scope.Searchables.GetEnumerator();

                // Move iterator to first document.
                if (!_searchableIterator.MoveNext())
                {
                    // Abort if no documents are found.
                    PromptEndOfScopeReached();
                    Reset(ref _searchableIterator);
                    Reset(ref _searchResultIterator);
                    return;
                }
            }

            // Initialize result iterator.
            if (_searchResultIterator == null)
                _searchResultIterator = _searchableIterator.Current.Search(Query).GetEnumerator();

            // Get next search result.
            while (!_searchResultIterator.MoveNext())
            {
                // No more search results in current document.
                // Move iterator to next document.
                if (!_searchableIterator.MoveNext())
                {
                    // Abort if no documents are found.
                    PromptEndOfScopeReached();
                    Reset(ref _searchableIterator);
                    Reset(ref _searchResultIterator);
                    return;
                }

                _searchResultIterator = _searchableIterator.Current.Search(Query).GetEnumerator();
            }

            // We have a new search result.
            _searchResultIterator.Current.IsSelected = true;
        }


        /// <summary>
        /// Replaces the current find result.
        /// </summary>
        /// <remarks>
        /// If no find result is currently selected, only <see cref="FindNext"/> is executed.
        /// </remarks>
        internal void Replace()
        {
            if (_searchResultIterator == null || !_searchResultIterator.Current.IsSelected)
            {
                FindNext();
                return;
            }

            // Store recently used patterns.
            AddRecentEntry(_recentFindPatterns, Query.FindPattern);
            AddRecentEntry(_recentReplacePatterns, Query.ReplacePattern);

            // Replace current search result.
            _searchResultIterator.Current.Replace(Query.ReplacePattern ?? string.Empty);

            Reset(ref _searchableIterator);
            Reset(ref _searchResultIterator);

            FindNext();
        }


        /// <summary>
        /// Replaces all find results.
        /// </summary>
        internal void ReplaceAll()
        {
            // Store recently used patterns.
            AddRecentEntry(_recentFindPatterns, Query.FindPattern);
            AddRecentEntry(_recentReplacePatterns, Query.ReplacePattern);

            // Loop through all documents.
            // Search and replace each search result.
            foreach (var searchable in Query.Scope.Searchables)
            {
                searchable.BeginReplaceAll();
                try
                {
                    foreach (var result in searchable.Search(Query))
                        result.Replace(Query.ReplacePattern);
                }
                finally
                {
                    searchable.EndReplaceAll();
                }
            }
        }


        private static void AddRecentEntry(IList<string> list, string entry)
        {
            if (string.IsNullOrEmpty(entry))
                return;

            // Insert entry at beginning of list and then remove duplicate entry. 
            // (Note: Do not remove the entry at the old index before the new entry 
            // is added. This would clear the combo box!)
            int index = list.IndexOf(entry);
            if (index != 0)
                list.Insert(0, entry);

            // Remove duplicate entry.
            if (index > 0)
                list.RemoveAt(index + 1);

            // If there are too many items in RecentFindPatterns, remove the last items.
            while (list.Count > MaxNumberOfCachedEntries)
                list.RemoveAt(list.Count - 1);
        }


        private void PromptEndOfScopeReached()
        {
            string title = Editor.ApplicationName;
            if (string.IsNullOrEmpty(title))
                title = "Find";

            MessageBox.Show(
                "Find reached the starting point of the search. No more occurrences found.",
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }


        /// <inheritdoc/>
        public void ShowQuickFind()
        {
            Editor.Focus(_quickFindCommandItem.CreateToolBarItem());
        }


        /// <inheritdoc/>
        public void ShowFindAndReplace()
        {
            Editor.ActivateItem(FindAndReplaceViewModel);
        }


        private void OnQueryChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            // A new query affords a new search.
            Reset(ref _searchableIterator);
            Reset(ref _searchResultIterator);

            InvalidateCommands();
        }


        private void InvalidateCommands()
        {
            foreach (var item in CommandItems.OfType<DelegateCommandItem>())
                item.Command.RaiseCanExecuteChanged();
        }
        #endregion
    }
}
