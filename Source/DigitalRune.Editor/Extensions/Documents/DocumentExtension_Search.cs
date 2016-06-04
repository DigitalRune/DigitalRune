// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Editor.Documents
{
    partial class DocumentExtension
    {
        private CurrentDocumentSearchScope _currentDocumentSearchScope;
        private OpenDocumentsSearchScope _openDocumentsSearchScope;


        /// <summary>
        /// Registers the search scopes provided by the document services.
        /// </summary>
        private void AddSearchScopes()
        {
            if (_searchService == null)
                return;

            _currentDocumentSearchScope = new CurrentDocumentSearchScope(this);
            _searchService.SearchScopes.Add(_currentDocumentSearchScope);

            _openDocumentsSearchScope = new OpenDocumentsSearchScope(Editor, this);
            _searchService.SearchScopes.Add(_openDocumentsSearchScope);

            // Make "Current document" the default search scope.
            _searchService.Query.Scope = _currentDocumentSearchScope;
        }


        /// <summary>
        /// Unregisters the search scopes provided by the document service.
        /// </summary>
        private void RemoveSearchScopes()
        {
            if (_searchService == null)
                return;

            _searchService.SearchScopes.Remove(_currentDocumentSearchScope);
            _currentDocumentSearchScope = null;

            _searchService.SearchScopes.Remove(_openDocumentsSearchScope);
            _openDocumentsSearchScope = null;
        }
    }
}
