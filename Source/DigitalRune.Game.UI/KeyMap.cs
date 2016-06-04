// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
#if !PORTABLE
using System.Xml.Linq;
#endif
using DigitalRune.Game.Input;
using Microsoft.Xna.Framework.Input;
#if SILVERLIGHT
using Keys = System.Windows.Input.Key;
#endif


namespace DigitalRune.Game.UI
{
  /// <summary>
  /// Maps XNA <see cref="Keys"/> (key codes) to characters (keyboard layout).
  /// </summary>
  public class KeyMap
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Note: Enumerations as dictionary keys cause boxing. Instead of 
    //   Dictionary<Keys, Dictionary<ModifierKeys, char>>
    // use
    //   Dictionary<int, Dictionary<int, char>>.
    private readonly Dictionary<int, Dictionary<int, char>> _map = new Dictionary<int, Dictionary<int, char>>();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the <see cref="System.Char"/> with the specified key and modifiers.
    /// </summary>
    /// <remarks>
    /// If an entry is set that does not yet exist, the entry is added to the map. If an entry is
    /// fetched that does not exist, 0 is returned.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
    public char this[Keys key, ModifierKeys modifierKeys]
    {
      get
      {
        // Get variants (different modifiers results) for the specified key.
        Dictionary<int, char> keyVariants;
        bool exists = _map.TryGetValue((int)key, out keyVariants);
        if (!exists)
          return '\0';

        // Get character for the given modifier.
        char c;
        exists = keyVariants.TryGetValue((int)modifierKeys, out c);
        if (!exists)
          return '\0';

        return c;
      }
      set
      {
        Dictionary<int, char> keyVariants;
        bool exists = _map.TryGetValue((int)key, out keyVariants);
        if (!exists)
        {
          // No entries for key exist.
          // Add new dictionary for the key and its variants.
          keyVariants = new Dictionary<int, char>();
          _map.Add((int)key, keyVariants);
        }

        // Add new key variant or change existing one.
        exists = keyVariants.ContainsKey((int)modifierKeys);
        if (!exists)
          keyVariants.Add((int)modifierKeys, value);
        else
          keyVariants[(int)modifierKeys] = value;
      }
    }


    /// <summary>
    /// Gets the an automatic key mapping for the current culture.
    /// </summary>
    /// <value>An automatic key mapping.</value>
    /// <remarks>
    /// <para>
    /// For Windows: Windows OS functions are used to get key mapping for the active keyboard 
    /// layout. 
    /// </para>
    /// <para>
    /// For Xbox 360: An English key mapping is used for USB keyboards. For the ChatPad an English
    /// or German key mapping is used, based on the current culture.
    /// </para>
    /// </remarks>
    public static KeyMap AutoKeyMap
    {
      get
      {
        if (_autoKeyMap != null)
          return _autoKeyMap;

        // Check if CurrentCulture is German.
        string cultureName = CultureInfo.CurrentCulture.EnglishName;
        bool isGerman = cultureName.Contains("German");

#if !WINDOWS && !PORTABLE
        // Use predefined key map.
        if (isGerman)
          _autoKeyMap = EnglishKeyMapGermanChatPad;
        else
          _autoKeyMap = EnglishKeyMap;
        return _autoKeyMap;
#else
        // ----- Automatically generate key map.
        // Start with one of the predefined maps.
        _autoKeyMap = new KeyMap(isGerman ? GermanKeyMap : EnglishKeyMap);

        // FormsHelper provides a method that maps all virtual-key codes and combination
        // of modifier keys to the corresponding Unicode character.
        int[] virtualKeyCodes = Enum.GetValues(typeof(Keys))
                                    .Cast<int>()
                                    .ToArray();
        var keyMappings = PlatformHelper.GetKeyMap(virtualKeyCodes);
        if (keyMappings != null)
          foreach (var keyMapping in keyMappings)
            _autoKeyMap[(Keys)keyMapping.Key, (ModifierKeys)keyMapping.ModifierKeys] = keyMapping.Character;

        return _autoKeyMap;
#endif
      }
    }
    private static KeyMap _autoKeyMap;


    /// <summary>
    /// Gets the German key mapping.
    /// </summary>
    /// <value>The German key mapping.</value>
    public static KeyMap GermanKeyMap
    {
      get
      {
        if (_germanKeyMap == null)
        {
          _germanKeyMap = new KeyMap();
          _germanKeyMap.LoadEmbeddedResource("DigitalRune.Game.UI.Resources.GermanKeyMap.xml");
        }
        return _germanKeyMap;
      }
    }
    private static KeyMap _germanKeyMap;


    /// <summary>
    /// Gets the English key mapping.
    /// </summary>
    /// <value>The English key mapping.</value>
    public static KeyMap EnglishKeyMap
    {
      get
      {
        if (_englishKeyMap == null)
        {
          _englishKeyMap = new KeyMap();
          _englishKeyMap.LoadEmbeddedResource("DigitalRune.Game.UI.Resources.EnglishKeyMap.xml");
        }
        return _englishKeyMap;
      }
    }
    private static KeyMap _englishKeyMap;


    /// <summary>
    /// Gets the English key mapping, but with a German key mapping for the ChatPad.
    /// </summary>
    /// <value>The English key mapping, but with a German key mapping for the ChatPad.</value>
    public static KeyMap EnglishKeyMapGermanChatPad
    {
      get
      {
        if (_englishKeyMapGermanChatPad == null)
        {
          _englishKeyMapGermanChatPad = new KeyMap();
          _englishKeyMapGermanChatPad.LoadEmbeddedResource("DigitalRune.Game.UI.Resources.EnglishKeyMapGermanChatPad.xml");
        }
        return _englishKeyMapGermanChatPad;
      }
    }
    private static KeyMap _englishKeyMapGermanChatPad;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new empty instance of the <see cref="KeyMap"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new empty instance of the <see cref="KeyMap"/> class.
    /// </summary>
    public KeyMap()
    {
    }


    /// <summary>
    /// Initializes a instance of the <see cref="KeyMap"/> class with the entries
    /// from another <see cref="KeyMap"/>.
    /// </summary>
    /// <param name="source">The source map that will be cloned.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    public KeyMap(KeyMap source)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      // Clone the source KeyMap.
      foreach (KeyValuePair<int, Dictionary<int, char>> entry in source._map)
      {
        var sourceKey = entry.Key;
        var sourceKeyVariants = entry.Value;

        var keyVariants = new Dictionary<int, char>();
        foreach (var keyVariant in sourceKeyVariants)
          keyVariants.Add(keyVariant.Key, keyVariant.Value);

        _map.Add(sourceKey, keyVariants);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
    private void LoadEmbeddedResource(string keyMapName)
    {
      // Get current assembly.
#if !NETFX_CORE && !NET45
      var assembly = Assembly.GetExecutingAssembly();
#else
      Assembly assembly = typeof(KeyMap).GetTypeInfo().Assembly;
#endif
      Stream stream = assembly.GetManifestResourceStream(keyMapName);
      if (stream != null)
      {
        using (XmlReader reader = XmlReader.Create(stream))
          Load(reader);
        //Load(XDocument.Load(reader));

#if NETFX_CORE || PORTABLE
        stream.Dispose();
#else
        stream.Close();
#endif
      }
    }


#if !PORTABLE
    /// <overloads>
    /// <summary>
    /// Loads the key map entries.
    /// </summary>
    /// </overloads>
    /// <summary>
    /// Loads the key map entries.
    /// </summary>
    /// <param name="xDocument">The XML document containing the key map entries.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public void Load(XContainer xDocument)
    {
      _map.Clear();

      if (xDocument == null)
        return;

      var root = xDocument.Descendants("KeyMap").FirstOrDefault();
      if (root == null)
        return;

      foreach (var entry in root.Elements("KeyEntry"))
      {
        XAttribute keyAttribute = entry.Attribute("Key");
        Keys key;
        bool success = EnumHelper.TryParse((string)keyAttribute, true, out key);
        if (!success)
          continue;

        XAttribute modifiersAttribute = entry.Attribute("Modifiers");
        ModifierKeys modifierKeys;
        success = EnumHelper.TryParse((string)modifiersAttribute, true, out modifierKeys);
        if (!success)
          modifierKeys = ModifierKeys.None;

        XAttribute charAttribute = entry.Attribute("Character");
        string c = (string)charAttribute;
        if (c == null || c.Length != 1)
          continue;

        this[key, modifierKeys] = c[0];
      }
    }


    /// <overloads>
    /// <summary>
    /// Saves the key map entries.
    /// </summary>
    /// </overloads>
    /// <summary>
    /// Saves the key map entries.
    /// </summary>
    /// <returns>
    /// An XML document with the serialized key map entries.
    /// </returns>
    public XDocument Save()
    {
      // Example usage: 
      // var xdoc = KeyMap.EnglishKeyMap.Save();
      // xdoc.Save(@"c:\dev\temp\keymap.xml");

      XDocument xDocument = new XDocument();
      var root = new XElement("KeyMap");
      xDocument.Add(root);

      foreach (KeyValuePair<int, Dictionary<int, char>> keyVariants in _map)
      {
        foreach (KeyValuePair<int, char> keyVariant in keyVariants.Value)
        {
          root.Add(new XElement(
            "KeyEntry",
            new XAttribute("Key", (Keys)keyVariants.Key),
            new XAttribute("Modifiers", (ModifierKeys)keyVariant.Key),
            new XAttribute("Character", keyVariant.Value)));
        }
      }
      return xDocument;
    }
#endif


    /// <summary>
    /// Loads the key map entries.
    /// </summary>
    /// <param name="reader">
    /// An XML reader for th XML document containing the key map entries.
    /// </param>
    public void Load(XmlReader reader)
    {
      _map.Clear();

      if (reader == null)
        return;

      if (!reader.ReadToFollowing("KeyMap"))
        return;

      if (!reader.ReadToDescendant("KeyEntry"))
        return;

      do
      {
        var keyAttribute = reader.GetAttribute("Key");
        if (string.IsNullOrEmpty(keyAttribute))
          continue;

        Keys key;
        bool success = EnumHelper.TryParse(keyAttribute, true, out key);
        if (!success)
          continue;

        var modifiersAttribute = reader.GetAttribute("Modifiers");
        if (string.IsNullOrEmpty(modifiersAttribute))
          continue;

        ModifierKeys modifierKeys;
        success = EnumHelper.TryParse(modifiersAttribute, true, out modifierKeys);
        if (!success)
          modifierKeys = ModifierKeys.None;

        var c = reader.GetAttribute("Character");
        if (c == null || c.Length != 1)
          continue;

        this[key, modifierKeys] = c[0];

      } while (reader.ReadToNextSibling("KeyEntry"));
    }


    /// <summary>
    /// Saves the key map entries.
    /// </summary>
    /// <param name="writer"> The XML writer to which the key map is written.</param>
    public void Save(XmlWriter writer)
    {
      // Following code creates identical XML files:
      // var xdoc = KeyMap.EnglishKeyMap.Save();
      // xdoc.Save(@"c:\dev\temp\keymap.xml");
      //   vs.
      // var settings = new XmlWriterSettings { Indent = true, };
      // using (var xmlWriter = XmlWriter.Create(@"c:\dev\temp\keymap.xml", settings))
      // {
      //   xmlWriter.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
      //   KeyMap.EnglishKeyMap.Save(xmlWriter);
      // }

      if (writer == null)
        return;

      writer.WriteStartElement("KeyMap");
      foreach (KeyValuePair<int, Dictionary<int, char>> keyVariants in _map)
      {
        foreach (KeyValuePair<int, char> keyVariant in keyVariants.Value)
        {
          writer.WriteStartElement("KeyEntry");
          writer.WriteAttributeString("Key", ((Keys)keyVariants.Key).ToString());
          writer.WriteAttributeString("Modifiers", ((ModifierKeys)keyVariant.Key).ToString());
          writer.WriteAttributeString("Character", keyVariant.Value.ToString());
          writer.WriteEndElement();
        }
      }
      writer.WriteEndElement();
    }
    #endregion
  }
}
