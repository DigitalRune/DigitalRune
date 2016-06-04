// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Editor.Output
{
    /// <summary>
    /// Provides the Output window which is used to show display messages or errors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Messages can be written into different buffers, called <i>views</i>. The user can switch the
    /// currently visible view in the Output window. New views can be added on-the-fly simply by
    /// clearing a new view or writing a message into a new view.
    /// </para>
    /// <para>
    /// The name of the view can be <see langword="null"/>, which indicates the default buffer.
    /// </para>
    /// <para>
    /// <strong>Thread-Safety:</strong> It is safe to call the output service from a background
    /// thread.
    /// </para>
    /// </remarks>
    public interface IOutputService
    {
        /// <summary>
        /// Clears the specified view.
        /// </summary>
        /// <param name="view">The output view. Can be <see langword="null"/> or empty.</param>
        /// <remarks>
        /// This method does not automatically show the Output window. Call <see cref="Show"/> to
        /// ensure that the Output window is visible.
        /// </remarks>
        void Clear(string view = null);


        /// <summary>
        /// Writes the specified text and adds a newline afterwards.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="view">The output view. Can be <see langword="null"/> or empty.</param>
        /// <remarks>
        /// This method does not automatically show the Output window. Call <see cref="Show"/> to
        /// ensure that the Output window is visible.
        /// </remarks>
        void WriteLine(string message, string view = null);


        /// <summary>
        /// Opens the Output window and shows the specified view.
        /// </summary>
        /// <param name="view">The output view. Can be <see langword="null"/> or empty.</param>
        void Show(string view = null);
    }
}
