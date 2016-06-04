// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using DigitalRune.Editor.Search;
using DigitalRune.Editor.Themes;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using ICSharpCode.AvalonEdit.Document;


namespace DigitalRune.Editor.Text
{
    partial class TextDocument
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private IDisposable _searchUpdateSubscription;
        private TextSegmentCollection<SearchResult> _searchResults;

        // ReSharper disable once NotAccessedField.Local
        private IDisposable _themeMessageSubscription; 

        private Brush _searchResultBrush;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        private void InitializeSearch()
        {
            if (_searchService == null)
                return;

            // Automatically preview (highlight) all search results in the text editor.
            // The search result markers need to be updated when the search query is changed.
            var query = _searchService.Query;
            var searchQueryChanges =
              Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => query.PropertyChanged += h, h => query.PropertyChanged -= h)
                        .Select(e => e.EventArgs)
                        .Where(e => string.IsNullOrEmpty(e.PropertyName)
                                    || e.PropertyName == "FindPattern"
                                    || e.PropertyName == "MatchCase"
                                    || e.PropertyName == "MatchWholeWord"
                                    || e.PropertyName == "Mode"
                                    || e.PropertyName == "Scope")
                        .Select(_ => Unit.Default);

            // Additionally, the search result markers need to be updated when the text document changes.
            var documentChanges =
              Observable.FromEventPattern<DocumentChangeEventArgs>(
                            h => AvalonEditDocument.Changed += h,
                            h => AvalonEditDocument.Changed -= h)
                        .Select(_ => Unit.Default);

            // Throttle the event - the update must not block the UI thread.
            _searchUpdateSubscription =
              searchQueryChanges.Merge(documentChanges)
                                .Throttle(TimeSpan.FromSeconds(0.5))
                                .ObserveOnDispatcher()
                                .Subscribe(_ => PreviewSearchResults());

            // The actual search results are stored in a TextSegmentCollection.
            _searchResults = new TextSegmentCollection<SearchResult>(AvalonEditDocument);

            var messageBus = Editor.Services.GetInstance<IMessageBus>();
            if (messageBus != null)
            {
                _themeMessageSubscription = messageBus.Listen<ThemeMessage>().Subscribe(OnThemeMessage);
            }
        }


        private void UninitializeSearch()
        {
            if (_searchService == null)
                return;

            if (_searchUpdateSubscription != null)
            {
                _searchUpdateSubscription.Dispose();
                _searchUpdateSubscription = null;
            }

            _searchResults.Clear();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnThemeMessage(ThemeMessage message)
        {
            _searchResultBrush = null;

            // The layout with the text editors is reloaded after the ThemeMessage. We use priority
            // background to update the markers after layout updates.
            WindowsHelper.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)PreviewSearchResults);
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerable<ISearchResult> ISearchable.Search(SearchQuery query)
        {
            return Search(query, 0, AvalonEditDocument.TextLength);
        }


        internal IEnumerable<ISearchResult> Search(SearchQuery query, int offset, int length)
        {
            // Clear previous search results.
            _searchResults.Clear();

            // Determine the start offset.
            int caretOffset = 0;
            if (query.IsRelative)
            {
                var textEditor = this.GetLastActiveTextEditor();
                if (textEditor != null)
                    caretOffset = textEditor.SelectionStart;  // SelectionStart returns CaretOffsets if nothing is selected.
            }

            if (caretOffset < offset)
                caretOffset = offset;
            else if (caretOffset > offset + length)
                caretOffset = offset + length;

            // Perform search.
            var searchResults = GetSearchResults(query, offset, length);
            if (searchResults == null || searchResults.Count == 0)
                return Enumerable.Empty<ISearchResult>();

            // Save search results in TextSegmentCollection. (The TextSegmentCollection will 
            // automatically update the offsets of the search results when the document changes.)
            foreach (var searchResult in searchResults)
                _searchResults.Add(searchResult);

            // Order search results. (The search query determines the search direction.
            // The search results should start at the caret position.)
            if (query.SearchBackwards)
            {
                int numberOfSearchResults = searchResults.Count;
                int movedSearchResults = 0;
                for (int i = 0; i < numberOfSearchResults; i++)
                {
                    var searchResult = searchResults[i];
                    if (searchResult.EndOffset < caretOffset)
                    {
                        searchResults.Add(searchResult);
                        movedSearchResults++;
                    }
                    else
                    {
                        break;
                    }
                }

                searchResults.RemoveRange(0, movedSearchResults);
                searchResults.Reverse();
            }
            else
            {
                int numberOfSearchResults = searchResults.Count;
                int movedSearchResults = 0;
                for (int i = 0; i < numberOfSearchResults; i++)
                {
                    var searchResult = searchResults[i];
                    if (searchResult.StartOffset < caretOffset)
                    {
                        searchResults.Add(searchResult);
                        movedSearchResults++;
                    }
                    else
                    {
                        break;
                    }
                }

                searchResults.RemoveRange(0, movedSearchResults);
            }

            return searchResults;
        }


        private List<SearchResult> GetSearchResults(SearchQuery query, int offset, int length)
        {
            if (length == 0 || AvalonEditDocument.TextLength == 0)
                return null;

            // Use regular expression.
            var regex = query.AsRegex();
            if (regex == null)
                return null;

            // Match regular expression.
            var matches = regex.Matches(AvalonEditDocument.Text);

            // Return the search results within the specified text segment.
            var searchResults = new List<SearchResult>(matches.Count);
            int endOffset = offset + length;
            foreach (Match match in matches)
            {
                if (offset <= match.Index && match.Index + match.Length <= endOffset)
                    searchResults.Add(new SearchResult(this, match));
            }

            return searchResults;
        }


        private void PreviewSearchResults()
        {
            Debug.Assert(_searchService != null);

            // Remove old search result markers.
            var textEditors = ViewModels.Select(view => ((TextDocumentViewModel)view).TextEditor).ToArray();
            foreach (var textEditor in textEditors)
            {
                if (textEditor == null)
                {
                    // This happens if we documents are loaded from command line and editor window 
                    // is not fully loaded.
                    continue;
                }

                foreach (var marker in SearchMarkers.ToArray())
                {
                    if (marker is SearchResultMarker)
                        SearchMarkers.Remove(marker);
                }
            }

            if (_searchResultBrush == null)
                _searchResultBrush = Application.Current
                                                .FindResource("TextEditor.SearchResultBackground")
                                                as Brush;

            if (_searchResultBrush == null)
                return;

            // Perform text search.
            var searchResults = GetSearchResults(_searchService.Query, 0, AvalonEditDocument.TextLength);
            if (searchResults != null && searchResults.Count > 0)
            {
                // Highlight search results.
                foreach (var searchResult in searchResults)
                {
                    var marker = new SearchResultMarker(_searchResultBrush)
                    {
                        StartOffset = searchResult.StartOffset,
                        Length = searchResult.Length,
                    };
                    SearchMarkers.Add(marker);
                }
            }
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void ISearchable.BeginReplaceAll()
        {
            AvalonEditDocument.BeginUpdate();
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void ISearchable.EndReplaceAll()
        {
            AvalonEditDocument.EndUpdate();
        }
        #endregion
    }
}
