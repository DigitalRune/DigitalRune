// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DigitalRune.Editor.Documents
{
    partial class DocumentExtension
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private const int DefaultNumberOfRecentFiles = 10;
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the number of recent files to remember.
        /// </summary>
        /// <value>The number of recent files to remember.</value>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> is negative or zero.
        /// </exception>
        internal int NumberOfRecentFiles
        {
            get { return _numberOfRecentFiles; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("NumberOfRecentFiles must be greater than 0.", nameof(value));

                _numberOfRecentFiles = value;
            }
        }
        private int _numberOfRecentFiles;


        /// <summary>
        /// Gets the recently used files.
        /// </summary>
        /// <value>The recently used files.</value>
        /// <remarks>
        /// The number of files contained in this collection can be different than
        /// <see cref="NumberOfRecentFiles"/>.
        /// </remarks>
        internal IEnumerable<string> RecentFiles
        {
            get { return _recentFiles; }
        }
        private readonly ObservableCollection<string> _recentFiles = new ObservableCollection<string>();
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Loads the list of recently used files.
        /// </summary>
        private void LoadRecentFiles()
        {
            try
            {
                NumberOfRecentFiles = Properties.Settings.Default.NumberOfRecentFiles;
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Could not load \"Number of recently used files\" from settings file.");
                NumberOfRecentFiles = DefaultNumberOfRecentFiles;
            }

            _recentFiles.Clear();
            try
            {
                _recentFiles.AddRange(Properties.Settings.Default.RecentFiles);
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Could not load \"Recently used files\" from settings file.");
            }
        }


        /// <summary>
        /// Saves the list of recently used files.
        /// </summary>
        private void SaveRecentFiles()
        {
            Properties.Settings.Default.NumberOfRecentFiles = NumberOfRecentFiles;
            while (_recentFiles.Count > NumberOfRecentFiles)
                _recentFiles.RemoveAt(_recentFiles.Count - 1);

            Properties.Settings.Default.RecentFiles = _recentFiles.AsStringCollection();

            try
            {
                Properties.Settings.Default.Save();
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Could not save \"Recently used files\" to settings file.");
            }
        }


        /// <summary>
        /// Adds the specified URI to the list of recently used files.
        /// </summary>
        /// <param name="uri">The URI.</param>
        private void RememberRecentFile(Uri uri)
        {
            if (uri == null)
                return;

            string fileName = uri.LocalPath;
            int index = _recentFiles.IndexOf(fileName);
            if (index >= 0)
                _recentFiles.Move(index, 0);
            else
                _recentFiles.Insert(0, fileName);

            // Update menu and toolbar.
            ((RecentDocumentsItem)CommandItems["RecentFiles"]).Update();
        }
        #endregion
    }
}
