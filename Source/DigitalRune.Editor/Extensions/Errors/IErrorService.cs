// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


using System.Collections.ObjectModel;


namespace DigitalRune.Editor.Errors
{
    /// <summary>
    /// Represents the Errors window.
    /// </summary>
    public interface IErrorService
    {
        ///// <summary>
        ///// Gets the error view model of the Errors window.
        ///// </summary>
        ///// <value>The view model of the Errors window.</value>
        //EditorDockTabItemViewModel ErrorsViewModel { get; }


        /// <summary>
        /// Gets the error collection.
        /// </summary>
        /// <value>The error collections.</value>
        ObservableCollection<Error> Errors { get; }


        /// <summary>
        /// Opens the Errors window.
        /// </summary>
        void Show();
    }
}
