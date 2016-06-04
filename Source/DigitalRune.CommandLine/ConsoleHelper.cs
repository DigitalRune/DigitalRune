// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// A disposable object that creates a temporary console if no console is attached. The temporary
    /// console is disposed with this object (after the user presses a key).
    /// </summary>
    internal struct ConsoleRegion : IDisposable
    {
        private readonly bool _hasConsole;


        public ConsoleRegion(bool hasConsole)
        {
            _hasConsole = hasConsole;
        }


        public void Dispose()
        {
            if (!_hasConsole && ConsoleHelper.HasConsole)
            {
                Console.WriteLine("Press any key to continue...");
                try
                {
                    Console.ReadKey();
                }
                catch (InvalidOperationException)
                {
                    // This happens when a temp console was opened a second time!
                    // --> Do not use this more than once in the application lifetime.
                }
                ConsoleHelper.DetachConsole();
            }
        }
    }


    /// <summary>
    /// Provides helper functions for interacting with the console.
    /// </summary>
    public static class ConsoleHelper
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private const int DefaultWindowWidth = 80;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether this application has console.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this application has console; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// Windows Forms or WPF applications usually do not have a console. Console applications
        /// have a console. Use this property to check if you are unsure.
        /// </remarks>
        public static bool HasConsole
        {
            get { return NativeMethods.GetConsoleWindow() != IntPtr.Zero; }
        }


        /// <summary>
        /// Gets the width of the console window in characters.
        /// </summary>
        /// <value>The width of the console window in characters.</value>
        internal static int WindowWidth
        {
            get
            {
                try
                {
                    return Console.WindowWidth;
                }
                catch (IOException)
                {
                    return DefaultWindowWidth;
                }
            }
        }
        #endregion



        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Makes sure a console instance is attached to the process.
        /// </summary>
        /// <returns>
        /// A disposable console object.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If a console is already attached (see <see cref="HasConsole"/>), this method and the 
        /// returned <see cref="IDisposable"/> do nothing. If no console is attached, a new 
        /// console will be attached. The temporary console will be detached when the returned
        /// <see cref="IDisposable"/> is disposed. When the console is being closed, the user must 
        /// press a key ("Press any key to continue...") and the temporary console and the 
        /// application is blocked until the key is pressed.
        /// </para>
        /// <para>
        /// This method is often used in a <see langword="using"/> statement like this:
        /// </para>
        /// <code lang="csharp" title="Example: Using AttachConsole in a using statement.">
        /// <![CDATA[
        /// using (ConsoleHelper.AttachConsole)
        /// {
        ///   Console.WriteLine("Important message...");
        /// }
        /// ]]>
        /// </code>
        /// <para>
        /// If a temporary console has to be attached, it is best to keep this console alive as long
        /// as you need it. Do not call <see cref="AttachConsole"/>/<see cref="DetachConsole"/>
        /// several time in the application lifetime.
        /// </para>
        /// </remarks>
        public static IDisposable AttachConsole()
        {
            var hasConsole = HasConsole;
            var consoleRegion = new ConsoleRegion(hasConsole);

            if (!hasConsole)
            {
                NativeMethods.AllocConsole();

                // Initialize stdout and stderror. Code taken from 
                // http://stackoverflow.com/questions/160587/no-output-to-console-from-a-wpf-application.
                Type type = typeof(Console);
                FieldInfo @out = type.GetField("_out", BindingFlags.Static | BindingFlags.NonPublic);
                FieldInfo error = type.GetField("_error", BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo initializeStdOutError = type.GetMethod("InitializeStdOutError", BindingFlags.Static | BindingFlags.NonPublic);
                Debug.Assert(@out != null);
                Debug.Assert(error != null);
                Debug.Assert(initializeStdOutError != null);
                @out.SetValue(null, null);
                error.SetValue(null, null);
                initializeStdOutError.Invoke(null, new object[] { true });
            }

            return consoleRegion;
        }


        /// <summary>
        /// Detaches the console that is attached to this process.
        /// </summary>
        /// <remarks>
        /// If no console is attached (see <see cref="HasConsole"/>), this method does nothing. After
        /// this method was called, it is still possible to write to the console, but the console and
        /// the output will no longer be visible.
        /// </remarks>
        public static void DetachConsole()
        {
            if (HasConsole)
            {
                // Clean up stdout and stderror.
                Console.SetOut(TextWriter.Null);
                Console.SetError(TextWriter.Null);

                NativeMethods.FreeConsole();
            }
        }


        /// <summary>
        /// Writes the specified text to the text writer centered on the line.
        /// </summary>
        /// <param name="textWriter">The text writer. Can be <see langword="null"/> to write nothing.</param>
        /// <param name="text">The text to be written. Must not be <see langword="null"/>.</param>
        public static void WriteLineCentered(this TextWriter textWriter, string text)
        {
            if (textWriter == null)
                return;
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            int lineWidth = WindowWidth;
            int indent = Math.Max(lineWidth / 2 - text.Length / 2 - 1, 0);
            textWriter.Write(' ', indent);
            textWriter.WriteLine(text);
        }


        // Same as WriteLineCentered, but for StringBuilder.
        internal static void AppendLineCentered(this StringBuilder stringBuilder, string text, int lineWidth)
        {
            if (stringBuilder == null)
                throw new ArgumentNullException(nameof(stringBuilder));
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            int indent = Math.Max(lineWidth / 2 - text.Length / 2 - 1, 0);
            stringBuilder.Append(' ', indent);
            stringBuilder.AppendLine(text);
        }


        /// <summary>
        /// Writes the specified character <i>repeatCount</i> times to the text writer.
        /// </summary>
        /// <param name="textWriter">The text writer. Can be <see langword="null"/> to write nothing.</param>
        /// <param name="character">The character to print.</param>
        /// <param name="repeatCount">The number of times to print <paramref name="character"/>.</param>
        public static void Write(this TextWriter textWriter, char character, int repeatCount)
        {
            if (textWriter == null)
                return;

            for (int i = 0; i < repeatCount; i++)
                textWriter.Write(character);
        }


        /// <summary>
        /// Writes the specified character <i>repeatCount</i> times to the text writer.
        /// </summary>
        /// <param name="textWriter">The text writer. Can be <see langword="null"/> to write nothing.</param>
        /// <param name="text">The text to be written.</param>
        /// <param name="repeatCount">The number of times to print <paramref name="text"/>.</param>
        public static void Write(this TextWriter textWriter, string text, int repeatCount)
        {
            if (textWriter == null)
                return;

            for (int i = 0; i < repeatCount; i++)
                textWriter.Write(text);
        }


        /// <summary>
        /// Writes the specified text to the text writer.
        /// </summary>
        /// <param name="textWriter">The text writer. Can be <see langword="null"/> to write nothing.</param>
        /// <param name="text">The text to be written.</param>
        /// <remarks>
        /// This method is like <see cref="Console.WriteLine()"/> of the <see cref="Console"/>, 
        /// except: At line ends text is wrapped between words and not in the middle of a word
        /// (unless a single word is longer than the line width).
        /// </remarks>
        public static void WriteLineWrapped(this TextWriter textWriter, string text)
        {
            WriteLineIndented(textWriter, text, 0, 0);
        }


        /// <overloads>
        /// <summary>
        /// Writes the specified text to the standard console output stream indented by a certain 
        /// number spaces.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Writes the specified text to the standard console output stream indented by a certain 
        /// number spaces.
        /// </summary>
        /// <param name="textWriter">The text writer. Can be <see langword="null"/> to write nothing.</param>
        /// <param name="text">The text to be written.</param>
        /// <param name="indent">The indentation of the text.</param>
        public static void WriteLineIndented(this TextWriter textWriter, string text, int indent)
        {
            WriteLineIndented(textWriter, text, indent, indent);
        }



        // Same as WriteLineIndented, but for StringBuilder.
        internal static void AppendLineIndented(this StringBuilder stringBuilder, string text,
            int indent, int lineWidth)
        {
            AppendLineIndented(stringBuilder, text, indent, indent, lineWidth);
        }


        /// <summary>
        /// Writes the specified text to the standard console output stream indented by a certain 
        /// number spaces.
        /// (First line can have different indentation.)
        /// </summary>
        /// <param name="textWriter">The text writer. Can be <see langword="null"/> to write nothing.</param>
        /// <param name="text">The text to be written.</param>
        /// <param name="indent">The indentation of the text.</param>
        /// <param name="indentOfFirstLine">The indentation of first line.</param>
        public static void WriteLineIndented(this TextWriter textWriter, string text, int indent, int indentOfFirstLine)
        {
            if (textWriter == null)
                return;
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            int maxWidth = WindowWidth - indent;   // max width of description column (right column)

            // Replace newline characters.
            text = text.Replace("\r\n", "\n");  // Remove '\r' from newline command.
            text = text.Replace("\n", " \n ");  // Add spaces for the following Split().

            // Split description into words.
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();

            // Split words longer than max width
            int i = 0;
            while (i < words.Count)
            {
                if (words[i].Length > maxWidth)
                {
                    string word1 = words[i].Substring(0, maxWidth);
                    string word2 = words[i].Substring(maxWidth);
                    words.RemoveAt(i);
                    words.Insert(i, word1);
                    words.Insert(i + 1, word2);
                }

                i++;
            }

            textWriter.Write(' ', indentOfFirstLine);

            i = 0;
            while (i < words.Count)
            {
                Debug.Assert(words[i].Length <= maxWidth, "At this point all words should be shorter than maxWidth.");

                int remainingWidth = maxWidth;
                while (i < words.Count && remainingWidth >= words[i].Length)
                {
                    if (words[i] == "\n")
                    {
                        i++;
                        break;
                    }

                    textWriter.Write(words[i]);
                    remainingWidth -= words[i].Length;
                    i++;

                    if (remainingWidth > 0)
                    {
                        textWriter.Write(' ');
                        remainingWidth--;
                    }
                }

                textWriter.WriteLine();

                if (i < words.Count)
                    textWriter.Write(' ', indent);
            }
        }


        // Same as WriteLineCentered, but for StringBuilder.
        // Same as WriteLineIndented, but for StringBuilder.
        internal static void AppendLineIndented(this StringBuilder stringBuilder, string text,
            int indent, int indentOfFirstLine, int lineWidth)
        {
            if (stringBuilder == null)
                throw new ArgumentNullException(nameof(stringBuilder));
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            int maxWidth = lineWidth - indent;   // max width of description column (right column)

            // Replace newline characters.
            text = text.Replace("\r\n", "\n");  // Remove '\r' from newline command.
            text = text.Replace("\n", " \n ");  // Add spaces for the following Split().

            // Split description into words.
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();

            // Split words longer than max width
            int i = 0;
            while (i < words.Count)
            {
                if (words[i].Length > maxWidth)
                {
                    string word1 = words[i].Substring(0, maxWidth);
                    string word2 = words[i].Substring(maxWidth);
                    words.RemoveAt(i);
                    words.Insert(i, word1);
                    words.Insert(i + 1, word2);
                }

                i++;
            }

            stringBuilder.Append(' ', indentOfFirstLine);

            i = 0;
            while (i < words.Count)
            {
                Debug.Assert(words[i].Length <= maxWidth, "At this point all words should be shorter than maxWidth.");

                int remainingWidth = maxWidth;
                while (i < words.Count && remainingWidth >= words[i].Length)
                {
                    if (words[i] == "\n")
                    {
                        i++;
                        break;
                    }

                    stringBuilder.Append(words[i]);
                    remainingWidth -= words[i].Length;
                    i++;

                    if (remainingWidth > 0)
                    {
                        stringBuilder.Append(' ');
                        remainingWidth--;
                    }
                }

                stringBuilder.AppendLine();

                if (i < words.Count)
                    stringBuilder.Append(' ', indent);
            }
        }
        #endregion
    }
}
