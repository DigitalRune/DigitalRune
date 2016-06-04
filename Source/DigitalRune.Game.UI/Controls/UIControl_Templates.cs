// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
#if NETFX_CORE || NET45
using System.Reflection;
#endif
using DigitalRune.Game.UI.Rendering;


namespace DigitalRune.Game.UI.Controls
{
  public partial class UIControl
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // Since the GameProperty and GameEvent classes are generic types. It is difficult to handle
    // them in ApplyTemplate because ApplyTemplate needs to know the type of each property/event.
    // We can avoid this if we wrap the properties and let themselves handle the AddToTemplate()
    // job.
    // Additionally, UIProperty stores the overridden default values.

    private interface IUIProperty
    {
      void AddToTemplate(IUIRenderer renderer, string style, GameObject template);
    }


    private sealed class UIProperty<T> : IUIProperty
    {
      public string Name;
      public bool OverridesDefaultValue;
      public T DefaultValue;

      public void AddToTemplate(IUIRenderer renderer, string style, GameObject template)
      {
        // Add this property to the game object. The value is set to:
        // - the value of the Renderer/Theme, or
        // - the overridden default value, or
        // - no value (= the global default value of the GamePropertyMetadata).

        T value;
        if (renderer.GetAttribute(style, Name, out value))
        {
          // Set value from renderer/theme.
          template.SetValue(Name, value);
        }
        else if (OverridesDefaultValue)
        {
          // Set overridden default value.
          template.SetValue(Name, DefaultValue);
        }
        else
        {
          // Use default value. (Add property without local value.)
          template.Properties.Add<T>(Name);
        }
      }
    }


    private interface IUIEvent
    {
      void AddToTemplate(GameObject gameObject);
    }


    private sealed class UIEvent<T> : IUIEvent where T : EventArgs
    {
      public int EventId;

      public void AddToTemplate(GameObject gameObject)
      {
        gameObject.Events.Add<T>(EventId);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Creates a game object event for a <see cref="UIControl"/>. (This method replaces
    /// <see cref="GameObject.CreateEvent{T}"/> of the <see cref="GameObject"/> class.)
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="ownerType">The control type.</param>
    /// <param name="name">The name of the event.</param>
    /// <param name="category">The category of the event.</param>
    /// <param name="description">The description of the event.</param>
    /// <param name="defaultEventArgs">The default event arguments.</param>
    /// <returns>The ID of the created event.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="ownerType"/> is <see langword="null"/>.
    /// </exception>
    public static int CreateEvent<T>(Type ownerType, string name, string category, string description, T defaultEventArgs) where T : EventArgs
    {
      if (ownerType == null)
        throw new ArgumentNullException("ownerType");

      // Create normal GameEvent<T>.
      int eventId = CreateEvent(name, category, description, defaultEventArgs).Id;

      List<IUIEvent> events;
      if (!_eventsPerType.TryGetValue(ownerType, out events))
      {
        // ownerType is not yet registered.
        events = new List<IUIEvent>();
        _eventsPerType.Add(ownerType, events);
      }

      // Add event to list of ownerType's event.
      events.Add(new UIEvent<T> { EventId = eventId });

      return eventId;
    }


    /// <summary>
    /// Creates a game object property for a <see cref="UIControl"/>. (This method replaces
    /// <see cref="GameObject.CreateProperty{T}"/> of the <see cref="GameObject"/> class.)
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="ownerType">The control type.</param>
    /// <param name="name">The name of the property.</param>
    /// <param name="category">The category of the property.</param>
    /// <param name="description">The description of the property.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="options">The <see cref="UIPropertyOptions"/>.</param>
    /// <returns>The ID of the created property</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="ownerType"/> is <see langword="null"/>.
    /// </exception>
    public static int CreateProperty<T>(Type ownerType, string name, string category,
      string description, T defaultValue, UIPropertyOptions options)
    {
      if (ownerType == null)
        throw new ArgumentNullException("ownerType");

      // Create normal GameProperty<T>.
      int propertyId = CreateProperty(name, category, description, defaultValue).Id;

      // Get stored properties (or default value).
      UIPropertyOptions existingOptions = _uiPropertyOptions.Get(propertyId);

      // To keep it simple: We store the "sum" of all options. If ControlA sets "X" to 
      // AffectMeasure, and ControlB sets "X" to AffectArrange, then "X" will be set 
      // AffectMeasure|AffectArrange for all controls.
      _uiPropertyOptions.Set(propertyId, existingOptions | options);

      // Get property list for ownerType.
      List<IUIProperty> properties;
      if (!_propertiesPerType.TryGetValue(ownerType, out properties))
      {
        // ownerType is not yet registered.
        properties = new List<IUIProperty>();
        _propertiesPerType.Add(ownerType, properties);
      }

      // Add property to list of ownerType's properties.
      properties.Add(new UIProperty<T> { Name = name });

      return propertyId;
    }


    /// <summary>
    /// Overrides the default value of a game object property for a specific class.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="ownerType">The class type.</param>
    /// <param name="propertyId">The ID of the game object property.</param>
    /// <param name="defaultValue">The new default value.</param>
    /// <remarks>
    /// These class-specific default values are applied to the template when the control is loaded.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="ownerType"/> is <see langword="null"/>.
    /// </exception>
    public static void OverrideDefaultValue<T>(Type ownerType, int propertyId, T defaultValue)
    {
      // We can only store 1 global DefaultValue for a GameProperty. (The default value is stored
      // globally in the GamePropertyMetadata<T>.) To allow default values per UIControl type, we 
      // store UIProperties per type. This default value is used in ApplyTemplate().

      if (ownerType == null)
        throw new ArgumentNullException("ownerType");

      List<IUIProperty> properties;
      if (!_propertiesPerType.TryGetValue(ownerType, out properties))
      {
        properties = new List<IUIProperty>();
        _propertiesPerType.Add(ownerType, properties);
      }

      var metadata = GetPropertyMetadata<T>(propertyId);
      properties.Add(new UIProperty<T>
      {
        Name = metadata.Name,
        OverridesDefaultValue = true,
        DefaultValue = defaultValue,
      });
    }


    /// <summary>
    /// Gets the <see cref="UIPropertyOptions"/> of game object property.
    /// </summary>
    /// <param name="propertyId">The ID of the game object property.</param>
    /// <returns>The <see cref="UIPropertyOptions"/>.</returns>
    public static UIPropertyOptions GetPropertyOptions(int propertyId)
    {
      return _uiPropertyOptions.Get(propertyId);
    }


    /// <summary>
    /// Sets the <see cref="UIPropertyOptions"/> of a game object property.
    /// </summary>
    /// <param name="propertyId">The ID of the game object property.</param>
    /// <param name="options">The options.</param>
    public static void SetPropertyOptions(int propertyId, UIPropertyOptions options)
    {
      _uiPropertyOptions.Set(propertyId, options);
    }


    /// <summary>
    /// Creates the template game object with default values defined by the renderer/theme.
    /// </summary>
    private void ApplyTemplate()
    {
      if (!IsLoaded)
        return;

      // Remove old template.
      Template = null;

      // No template if Style is no valid string.
      if (string.IsNullOrEmpty(Style))
        return;

      // Get template from renderer and set template.
      var renderer = Screen.Renderer;
      GameObject template;
      if (renderer.Templates.TryGetValue(Style, out template))
      {
        Template = template;
        return;
      }

      // No template for Style found. Create a new template.
      template = new GameObject();

      // Add all properties and events to the template that have been defined for this
      // type and the parent types with CreateProperty/Event. The property values will 
      // be the values from the renderer/theme, the overridden default values, or simply 
      // the GamePropertyMetadata<T>.DefaultValue.
      Type type = GetType();
      do
      {
        // Set game object properties.
        List<IUIProperty> properties;
        if (_propertiesPerType.TryGetValue(type, out properties))
          foreach (var property in properties)
            property.AddToTemplate(Screen.Renderer, Style, template);

        // Set game object events.
        List<IUIEvent> events;
        if (_eventsPerType.TryGetValue(type, out events))
          foreach (var uiEvent in events)
            uiEvent.AddToTemplate(template);

        // Do the same for base class.
#if !NETFX_CORE && !NET45
        type = type.BaseType;
#else
        type = type.GetTypeInfo().BaseType;
#endif
      } while (type != typeof(GameObject) && type != null);

      // Template has been filled with properties and values.

      // Add template.
      Template = template;

      // Remember template for reuse in other controls that have the same style.
      renderer.Templates.Add(Style, template);
    }
    #endregion
  }
}
