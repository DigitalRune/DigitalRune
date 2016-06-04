// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Selects a <see cref="DataTemplate"/> for a <see cref="IProperty"/> in a
    /// <see cref="PropertyGrid"/>.
    /// </summary>
    public class PropertyGridTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// When overridden in a derived class, returns a <see cref="DataTemplate"/> based on custom
        /// logic.
        /// </summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>
        /// Returns a <see cref="DataTemplate"/> or <see langword="null"/>. The default value is
        /// <see langword="null"/>.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // We expect that the item is an IProperty and the container is a FrameworkElement.
            // We need a FrameworkElement to call TryFindResource().
            var property = item as IProperty;
            if (property == null)
                return base.SelectTemplate(item, container);

            var containerElement = container as FrameworkElement;
            if (containerElement == null)
                return base.SelectTemplate(item, container);

            // First, try to find data template using the explicit resource key.
            DataTemplate dataTemplate = null;
            if (property.DataTemplateKey != null)
                dataTemplate = containerElement.TryFindResource(property.DataTemplateKey) as DataTemplate;

            // Next, search for a data template using the type of the property.
            // If we do not find a type, we continue the search using the base type.
            var type = property.PropertyType;
            while (dataTemplate == null && type != null)
            {
                // Try using a ComponentResourceKey with typeof(PropertyGrid) and the property type.
                dataTemplate = containerElement.TryFindResource(new ComponentResourceKey(typeof(PropertyGrid), type)) as DataTemplate;

                // Continue with base type.
                type = type.BaseType;
            }

            // If the type is a interface, the above loop does not find a template, but we can
            // use the fallback template for "Object".
            if (dataTemplate == null && property.PropertyType.IsInterface)
                dataTemplate = containerElement.TryFindResource(new ComponentResourceKey(typeof(PropertyGrid), typeof(object))) as DataTemplate;

            // We haven't found anything so far. Maybe we can use a String type converter to display
            // the property in an editable text box.
            if (dataTemplate == null)
            {
                var converter = TypeDescriptor.GetConverter(property.PropertyType);
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                    dataTemplate = containerElement.TryFindResource(new ComponentResourceKey(typeof(PropertyGrid), typeof(string))) as DataTemplate;
            }

            // If we still haven't found anything, we use the fallback data template.
            if (dataTemplate == null)
                dataTemplate = containerElement.TryFindResource(new ComponentResourceKey(typeof(PropertyGrid), "default")) as DataTemplate;

            return dataTemplate;
        }
    }
}
