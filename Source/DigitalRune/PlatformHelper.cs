// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;
#if WINDOWS
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
#elif NETFX_CORE
using Windows.ApplicationModel.DataTransfer;
#elif WINDOWS_PHONE || SILVERLIGHT
using System.Windows;
#endif


namespace DigitalRune
{
  /// <summary>
  /// Provides helper methods for platform-specific tasks. (For internal use by other DigitalRune
  /// libraries.)
  /// </summary>
  /// <remarks>
  /// Most DigitalRune libraries are platform-independent, portable class libraries. These libraries
  /// cannot access platform-specific functions directly. The <see cref="PlatformHelper"/> provides
  /// access to platform-specific functions as needed.
  /// </remarks>
  public static class PlatformHelper
  {
#if WINDOWS
    private static bool User32DllNotFound;
#endif


#if WINDOWS
    /// <summary>
    /// Determines whether [is running on mac].
    /// </summary>
    /// <returns><see langword="true" /> if [is running on mac]; otherwise, <see langword="false" />.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    internal static bool IsRunningOnMac()
    {
      // This code is from 
      //  https://github.com/jpobst/Pinta/blob/master/Pinta.Core/Managers/SystemManager.cs
      //  by Jonathan Pobst. (MIT license).
      IntPtr buf = IntPtr.Zero;
      try
      {
        buf = Marshal.AllocHGlobal(8192);
        // Hack: Get sysname.
        if (NativeMethods.uname(buf) == 0)
        {
          string os = Marshal.PtrToStringAnsi(buf);
          if (os == "Darwin")
            return true;
        }
      }
      catch
      {
      }
      finally
      {
        if (buf != IntPtr.Zero)
          Marshal.FreeHGlobal(buf);
      }
      return false;
    }
#endif


    /// <summary>
    /// Gets the default cursor (<strong>System.Windows.Forms.Form</strong>).
    /// </summary>
    /// <value>
    /// The default cursor (<strong>System.Windows.Forms.Cursors.Arrow</strong>) or 
    /// <see langword="null"/> if <strong>System.Windows.Forms</strong> is not available on the 
    /// target platform.
    /// </value>
    public static object DefaultCursor
    {
      get
      {
#if WINDOWS
        return Cursors.Arrow;
#else
        return null;
#endif
      }
    }


    /// <overloads>
    /// <summary>
    /// Creates a <strong>System.Windows.Forms.Cursor</strong> instance.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates a <strong>System.Windows.Forms.Cursor</strong> instance.
    /// </summary>
    /// <param name="fileName">
    /// The file path of the cursor file (usually .cur or .ani).
    /// </param>
    /// <returns>
    /// The <strong>System.Windows.Forms.Cursor</strong> object or <see langword="null"/> if
    /// <strong>System.Windows.Forms</strong> is not available on the target platform.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for destroying Cursor.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "fileName")]
    public static object CreateCursor(string fileName)
    {
#if !WINDOWS
      return null;
#else
      if (string.IsNullOrEmpty(fileName))
        return null;

      if (!File.Exists(fileName))
        return null;

      if (User32DllNotFound)
        return new Cursor(fileName);

      try
      {
        // We need to use the native LoadImage method. Cursor.ctor() does not 
        // support animated cursors! 
        // On non-windows platforms, this will throw a DllNotFoundException.
        IntPtr handle = NativeMethods.LoadImage(IntPtr.Zero, fileName, 2, 0, 0, 0x0010);
        return (handle != IntPtr.Zero) ? new Cursor(handle) : null;
      }
      catch (EntryPointNotFoundException)
      {
        // User32.dll is not available on the current platform (e.g. Linux, MacOS).
        User32DllNotFound = true;
      }
      catch (DllNotFoundException)
      {
        // User32.dll is not available on the current platform (e.g. Linux, MacOS).
        User32DllNotFound = true;
      }

      // Fall back to Cursor constructor which does not support animated cursors.
      return new Cursor(fileName);
#endif
    }


    /// <summary>
    /// Creates a <strong>System.Windows.Forms.Cursor</strong> instance.
    /// </summary>
    /// <param name="stream">
    /// A stream for reading the cursor file (usually .cur or .ani).
    /// </param>
    /// <returns>
    /// The <strong>System.Windows.Forms.Cursor</strong> object or <see langword="null"/> if
    /// <strong>System.Windows.Forms</strong> is not available on the target platform.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for destroying Cursor.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stream")]
    public static object CreateCursor(Stream stream)
    {
#if !WINDOWS
      return null;
#else
      if (stream == null)
        return null;

      if (User32DllNotFound)
        return new Cursor(stream);

      // Read cursor data.
      int length = (int)stream.Length;
      var data = new Byte[length];
      stream.Read(data, 0, length);

      // Create temporary file for native LoadImage function.
      string tempFilename = null;
      try
      {
        tempFilename = Path.GetTempFileName();
        using (var tempStream = new FileStream(tempFilename, FileMode.Open, FileAccess.Write, FileShare.None))
          tempStream.Write(data, 0, length);

        IntPtr handle = NativeMethods.LoadImage(IntPtr.Zero, tempFilename, 2, 0, 0, 0x0010);
        return (handle != IntPtr.Zero) ? new Cursor(handle) : null;
      }
      catch (IOException)
      {
        // Problems with temporary file. Ignore and use Cursor.ctor instead of LoadImage.
      }
      catch (DllNotFoundException)
      {
        // User32.dll is not available on the current platform (e.g. Linux, MacOS).
        User32DllNotFound = true;
      }
      finally
      {
        // Delete temp file.
        try
        {
          if (tempFilename != null)
            File.Delete(tempFilename);
        }
        // ReSharper disable once EmptyGeneralCatchClause
        catch
        {
          // Ignore.
        }
      }

      stream.Seek(0, SeekOrigin.Begin);
      return new Cursor(stream);
#endif
    }


    /// <summary>
    /// Destroys a cursor that was created using (<see cref="CreateCursor(string)"/>).
    /// </summary>
    /// <param name="cursor">The <strong>System.Windows.Forms.Cursor</strong>.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cursor")]
    public static void DestroyCursor(object cursor)
    {
#if WINDOWS
      var cursorTyped = cursor as Cursor;
      if (cursorTyped == null)
        throw new ArgumentException("Parameter cursor must be of type System.Windows.Forms.Cursor");

      if (User32DllNotFound)
        return;

      try
      {
        NativeMethods.DestroyCursor(cursorTyped.Handle);
      }
      catch (DllNotFoundException)
      {
        // User32.dll is not available on the current platform (e.g. Linux, MacOS).
        User32DllNotFound = true;
      }
#endif
    }


    /// <summary>
    /// Gets the <strong>System.Windows.Forms.Form</strong> associated with the given handle.
    /// </summary>
    /// <param name="handle">
    /// An <see cref="IntPtr"/> that represents the Windows handle of a form.
    /// </param>
    /// <returns>
    /// The <strong>System.Windows.Forms.Form</strong> object or <see langword="null"/> if 
    /// <strong>System.Windows.Forms</strong> is not available on the target platform.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "handle")]
    public static object GetForm(IntPtr handle)
    {
#if WINDOWS
      return Control.FromHandle(handle) as Form;
#else
      return null;
#endif
    }


    /// <summary>
    /// Determines whether the specified form is visible.
    /// </summary>
    /// <param name="form">The <strong>System.Windows.Forms.Form</strong>.</param>
    /// <returns>
    /// <see langword="true"/> if the form is visible; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="form"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <strong>System.Windows.Forms</strong> is not available on the target platform.
    /// </exception>
    public static bool IsFormVisible(object form)
    {
      if (form == null)
        throw new ArgumentNullException("form");

#if PORTABLE
      throw Portable.NotImplementedException;
#elif WINDOWS
      var formTyped = ((Form)form);
      return formTyped.Visible;
#else
      throw new NotSupportedException();
#endif
    }


    /// <summary>
    /// Sets the cursor of the specified form.
    /// </summary>
    /// <param name="form">The <strong>System.Windows.Forms.Form</strong>.</param>
    /// <param name="cursor">The <strong>System.Windows.Forms.Cursor</strong>.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cursor")]
    public static void SetCursor(object form, object cursor)
    {
      if (form == null)
        return;

#if PORTABLE
      throw Portable.NotImplementedException;
#elif WINDOWS
      var formTyped = ((Form)form);
      var cursorTyped = (Cursor)cursor;

      // If the Cursor does not change we must not change the property - because even when 
      // we set Cursor to the SAME value the animation of an animated cursor is reset.
      // ReSharper disable once RedundantCheckBeforeAssignment
      if (formTyped.Cursor != cursorTyped)
        formTyped.Cursor = cursorTyped;
#endif
    }


    //--------------------------------------------------------------
    #region Clipboard
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the <strong>System.Windows.Forms.Clipboard</strong> is
    /// supported on the current platform.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the clipboard is supported on the current platform; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public static bool IsClipboardSupported
    {
      get
      {
//#if WINDOWS || NETFX_CORE || WINDOWS_PHONE || SILVERLIGHT
#if WINDOWS || WINDOWS_PHONE || SILVERLIGHT     // TODO: Clipboard is not yet available in Windows Store Universal apps.
        return true;
#else
        return false;
#endif
      }
    }


    /// <summary>
    /// Gets the clipboard text.
    /// </summary>
    /// <returns>The text from the clipboard.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public static string GetClipboardText()
    {
      string data = null;
#if WINDOWS
      Thread thread = new Thread(() =>
      {
        data = Clipboard.GetText();
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      thread.Join();

      data = data.Replace(Environment.NewLine, "\n");
//#elif NETFX_CORE              // TODO: Clipboard is not yet available in Windows Store Universal apps.
//      var dataPackageView = Clipboard.GetContent();
//      if (dataPackageView.Contains(StandardDataFormats.Text))
//        data = dataPackageView.GetTextAsync().GetResults();
#elif WINDOWS_PHONE
      // Clipboard text can only be set in Windows Phone 7. Clipboard.GetText()
      // causes a SecurityException.
#elif SILVERLIGHT
      data = Clipboard.GetText();
#endif
      return data;
    }


    /// <summary>
    /// Sets the clipboard text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="text"/> is <see langword="null"/>.
    /// </exception>
    public static void SetClipboardText(string text)
    {
      if (text == null)
        throw new ArgumentNullException("text");

#if WINDOWS
      text = text.Replace("\n", Environment.NewLine);
      Thread thread = new Thread(() =>
      {
        Clipboard.Clear();
        Clipboard.SetText(text);
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      thread.Join();
//#elif NETFX_CORE             // TODO: Clipboard is not yet available in Windows Store Universal apps.
//      var dataPackage = new DataPackage();
//      dataPackage.SetText(text);
//      Clipboard.SetContent(dataPackage);
#elif WINDOWS_PHONE || SILVERLIGHT
      Clipboard.SetText(text);
#endif
    }
    #endregion


    //--------------------------------------------------------------
    #region SystemInformation
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the amount of the delta value of a single mouse wheel rotation increment.
    /// </summary>
    /// <value>
    /// The amount of the delta value of a single mouse wheel rotation increment.
    /// </value>
    public static int MouseWheelScrollDelta
    {
      get
      {
#if WINDOWS
        return SystemInformation.MouseWheelScrollDelta;
#else
        return 120;
#endif
      }
    }


    /// <summary>
    /// Gets the number of lines to scroll when the mouse wheel is rotated.
    /// </summary>
    /// <value>
    /// The number of lines to scroll on a mouse wheel rotation, or -1 if the "One screen at a time" 
    /// mouse option is selected. 
    /// </value>
    public static int MouseWheelScrollLines
    {
      get
      {
#if WINDOWS
        return SystemInformation.MouseWheelScrollLines;
#else
        return 1;
#endif
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region KeyMap
    //--------------------------------------------------------------

#if WINDOWS
    [Flags]
    internal enum ModifierKeys
    {
      None = 0,
      Alt = 1,
      Control = 2,
      Shift = 4,
      ControlAlt = Control | Alt,
    }
#endif


    /// <summary>
    /// Maps a virtual-key code and combination of modifier keys to the corresponding Unicode
    /// character.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Equality operators are not needed.")]
    public struct KeyMapping
    {
      /// <summary>The virtual-key code.</summary>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
      public int Key;

      /// <summary>The modifier keys (Alt = 1, Control = 2, Shift = 4).</summary>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
      public int ModifierKeys;

      /// <summary>The character value.</summary>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
      public char Character;
    }


    /// <summary>
    /// Gets the key map for the specified virtual-key codes.
    /// </summary>
    /// <param name="virtualKeyCodes">The virtual key codes.</param>
    /// <returns>
    /// The key map, which maps each virtual-key code and combination of modifier keys to the
    /// corresponding Unicode character.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="virtualKeyCodes"/> is <see langword="null"/>.
    /// </exception>
#if !WINDOWS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
#endif
    public static KeyMapping[] GetKeyMap(int[] virtualKeyCodes)
    {
#if WINDOWS
      if (User32DllNotFound)
        return null;

      try
      {
        if (virtualKeyCodes == null)
          throw new ArgumentNullException("virtualKeyCodes");

        var keyMap = new List<KeyMapping>();

        // Get keyboard layout of current thread.
        IntPtr layout = NativeMethods.GetKeyboardLayout(0);

        // We need an array where the index is a virtual-key code.
        // Relevant entries are 0x10 - 0x12 (Shift, Control and Alt).
        byte[] keyStates = new byte[256];

        // Get the character for all virtual-keys and modifier key combos.
        foreach (int virtualKeyCode in virtualKeyCodes)
        {
          // Get scancode.
          uint scanCode = NativeMethods.MapVirtualKeyEx((uint)virtualKeyCode, 0, layout);

          keyStates[0x10] = 0x00; // No Shift.
          keyStates[0x11] = 0x00; // No Control.
          keyStates[0x12] = 0x00; // No Alt.
          AddKeyMapEntry(keyMap, virtualKeyCode, scanCode, keyStates, ModifierKeys.None, layout);

          keyStates[0x10] = 0x80; // Only Shift pressed.
          keyStates[0x11] = 0x00;
          keyStates[0x12] = 0x00;
          AddKeyMapEntry(keyMap, virtualKeyCode, scanCode, keyStates, ModifierKeys.Shift, layout);

          keyStates[0x10] = 0x00;
          keyStates[0x11] = 0x80; // Control pressed.
          keyStates[0x12] = 0x80; // Alt pressed.
          AddKeyMapEntry(keyMap, virtualKeyCode, scanCode, keyStates, ModifierKeys.ControlAlt, layout);
        }

        return keyMap.ToArray();
      }
      catch (EntryPointNotFoundException)
      {
        // User32.dll is not available on the current platform (e.g. Linux, MacOS).
        User32DllNotFound = true;
      }
      catch (DllNotFoundException)
      {
        // User32.dll is not available on the current platform (e.g. Linux, MacOS).
        User32DllNotFound = true;
      }
#endif

      return null;
    }

#if WINDOWS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
    private static void AddKeyMapEntry(List<KeyMapping> keyMap, int virtualKeyCode, uint scanCode, byte[] keyStates, ModifierKeys modifierKeys, IntPtr layout)
    {
      char c;
      
      // Call ToUnicode twice because if the last ToUnicode call was done for a dead key (e.g. ´)
      // then this call creates the result of the dead key and the current key (e.g. ´ + a = á).
      NativeMethods.ToUnicodeEx((uint)virtualKeyCode, scanCode, keyStates, out c, 1, 0, layout);
      int result = NativeMethods.ToUnicodeEx((uint)virtualKeyCode, scanCode, keyStates, out c, 1, 0, layout);
      if (result == 1)
      {
        keyMap.Add(new KeyMapping
        {
          Key = virtualKeyCode,
          ModifierKeys = (int)modifierKeys,
          Character = c
        });
      }
    }
#endif
#endregion
  }
}
