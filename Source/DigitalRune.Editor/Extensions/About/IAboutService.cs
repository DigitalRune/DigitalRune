// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DigitalRune.Windows.Controls;


namespace DigitalRune.Editor.About
{
    /// <summary>
    /// Shows and controls the content of the About dialog.
    /// </summary>
    public interface IAboutService
    {
        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application. The default value is the same as
        /// <see cref="IEditorService.ApplicationName"/>.
        /// </value>
        string ApplicationName { get; set; }


        /// <summary>
        /// Gets or sets the copyright string.
        /// </summary>
        /// <value>
        /// The copyright. Per default, the copyright of the executing assembly is used.
        /// </value>
        string Copyright { get; set; }


        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version. Per default, the version of the executing assembly is used.</value>
        string Version { get; set; }


        /// <summary>
        /// Gets or sets the additional information.
        /// </summary>
        /// <value>Additional information for the About dialog.</value>
        /// <remarks>
        /// <para>
        /// In this property additional information can be set that should also be displayed in the
        /// about dialog, for example: homepage, contact info, support info, etc. The information
        /// can be of any type that can be displayed by a WPF <see cref="ContentPresenter"/>.
        /// </para>
        /// <para>
        /// A text representation of this information should be set in
        /// <see cref="InformationAsString"/>.
        /// </para>
        /// </remarks>
        object Information { get; set; }


        /// <summary>
        /// Gets or sets the additional information (as text string).
        /// </summary>
        /// <value>Additional information (as text string).</value>
        /// <remarks>
        /// This property is not shown in the About dialog. When the about dialog text is copied to
        /// the clipboard, this string is used instead of <see cref="Information"/>. It is not
        /// necessary to set this property if <see cref="Information"/> is already of type
        /// <see cref="string"/>.
        /// </remarks>
        string InformationAsString { get; set; }


        /// <summary>
        /// Gets or sets the image (<see cref="ImageSource"/> or <see cref="MultiColorGlyph"/>) that
        /// should be shown in the About dialog.
        /// </summary>
        /// <value>The image. Can be <see langword="null"/>.</value>
        object Icon { get; set; }


        /// <summary>
        /// Gets the extension descriptions.
        /// </summary>
        /// <value>The extension descriptions.</value>
        /// <remarks>
        /// The default dialog contains a list, where information about extensions are displayed. If
        /// an extension wants to be visible in the About dialog it must add an
        /// <see cref="EditorExtensionDescription"/> item to this collection.
        /// </remarks>
        ICollection<EditorExtensionDescription> ExtensionDescriptions { get; }


        /// <summary>
        /// Shows the About dialog.
        /// </summary>
        /// <remarks>
        /// This method opens the About dialog and returns only when the About dialog is closed.
        /// </remarks>
        void Show();


        /// <summary>
        /// Copies the About information to the clipboard.
        /// </summary>
        /// <returns>
        /// The About information which has been copied into the clipboard.
        /// </returns>
        string CopyInformationToClipboard();
    }
}
