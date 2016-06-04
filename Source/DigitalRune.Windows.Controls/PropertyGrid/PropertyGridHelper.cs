// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DigitalRune.Collections;
using static System.FormattableString;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Provides helper methods for the <see cref="PropertyGrid"/> control.
    /// </summary>
    public static class PropertyGridHelper
    {
        /// <summary>
        /// Compares property category and names. Properties are sorted by category, then by name.
        /// The category "Common" (or "Common Properties") is sorted to the beginning. The default 
        /// category ("Misc" or "Miscellaneous") is sorted to the back.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        private class PropertyComparer : Singleton<PropertyComparer>, IComparer<IProperty>
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
            public int Compare(IProperty x, IProperty y)
            {
                bool xIsDefault = string.CompareOrdinal(x.Category, Categories.Default) == 0;
                bool yIsDefault = string.CompareOrdinal(y.Category, Categories.Default) == 0;
                if (xIsDefault)
                {
                    if (yIsDefault)
                        return string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);

                    return 1;
                }

                if (yIsDefault)
                    return -1;

                xIsDefault = string.CompareOrdinal(x.Category, "Miscellaneous") == 0;
                yIsDefault = string.CompareOrdinal(y.Category, "Miscellaneous") == 0;
                if (xIsDefault)
                {
                    if (yIsDefault)
                        return string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);

                    return 1;
                }

                if (yIsDefault)
                    return -1;

                bool xIsCommon = string.CompareOrdinal(x.Category, "Common") == 0;
                bool yIsCommon = string.CompareOrdinal(y.Category, "Common") == 0;
                if (xIsCommon)
                {
                    if (yIsCommon)
                        return string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);

                    return -1;
                }

                if (yIsCommon)
                    return 1;

                xIsCommon = string.CompareOrdinal(x.Category, "Common Properties") == 0;
                yIsCommon = string.CompareOrdinal(y.Category, "Common Properties") == 0;
                if (xIsCommon)
                {
                    if (yIsCommon)
                        return string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);

                    return -1;
                }

                if (yIsCommon)
                    return 1;

                var categoryResult = string.Compare(x.Category, y.Category, StringComparison.CurrentCultureIgnoreCase);
                if (categoryResult != 0)
                    return categoryResult;

                return string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Converts the specified value to the target type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">The target type.</param>
        /// <returns>
        /// The <paramref name="value"/> converted to <paramref name="targetType"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The specified value is not assignable to the property.
        /// </exception>
        internal static object Convert(object value, Type targetType)
        {
            if (value == null && !targetType.IsClass
                || value != null && !targetType.IsInstanceOfType(value))
            {
                // Not directly assignable. Try to use TypeConverter.
                var converter = TypeDescriptor.GetConverter(targetType);
                if (converter == null)
                    throw new ArgumentException("The specified value is not assignable to the property.", nameof(value));

                value = converter.ConvertFrom(value);
            }

            return value;
        }


        /// <summary>
        /// Creates a property source for the specified CLR object using reflection.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name for the property source.</param>
        /// <param name="typeName">The type name of the property source.</param>
        /// <returns>The property source.</returns>
        /// <remarks>
        /// <para>
        /// This method can be used for static types and dynamic types that implement
        /// <see cref="ICustomTypeDescriptor"/>. The property source lists instance properties.
        /// Properties where the <see cref="BrowsableAttribute"/> is set to <see langword="false"/>
        /// are ignored.
        /// </para>
        /// <para>
        /// To find the name of the property source the method uses (in this order):
        /// </para>
        /// <list type="number">
        /// <item>The <paramref name="name"/> parameter.</item>
        /// <item>The <see cref="INamedObject"/> interface.</item>
        /// <item>A string property called "Name".</item>
        /// </list>
        /// <para>
        /// To find the type name of the property source the method uses (in this order): 
        /// </para>
        /// <list type="number">
        /// <item>The <paramref name="typeName"/> parameter.</item>
        /// <item>The CLR type name.</item>
        /// </list>
        /// <para>
        /// The <paramref name="instance"/> is stored in the <see cref="PropertySource.UserData"/>
        /// of the <see cref="PropertySource"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="instance"/> is <see langword="null"/>.
        /// </exception>
        public static PropertySource CreatePropertySource(object instance, string name = null, string typeName = null)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var propertySource = new PropertySource { UserData = instance };

            // Get property descriptors.
            var typeConverter = TypeDescriptor.GetConverter(instance);
            PropertyDescriptorCollection propertyDescriptors = null;
            if (typeConverter != null && typeConverter.GetPropertiesSupported())
            {
                // Try to get properties from type converter.
                propertyDescriptors = typeConverter.GetProperties(instance);
            }

            if (propertyDescriptors == null && instance is ICustomTypeDescriptor)
            {
                // Instance is a dynamic object.
                propertyDescriptors = ((ICustomTypeDescriptor)instance).GetProperties();
            }

            bool isBasicObject = false;
            if (propertyDescriptors == null)
            {
                // Instance is a regular CLR object.
                isBasicObject = true;
                propertyDescriptors = TypeDescriptor.GetProperties(instance.GetType());
            }

            // Create a list of all properties.
            propertySource.Properties.AddRange(
                propertyDescriptors.OfType<PropertyDescriptor>()
                                   .Where(descriptor => descriptor.IsBrowsable)
                                   .Select(descriptor => new ReflectedProperty(instance, descriptor))
                                   .OrderBy(p => p, PropertyComparer.Instance));

            // Add public fields of regular CLR objects (very important for structs like Vector3F).
            if (isBasicObject)
            {
                foreach (var field in instance.GetType().GetFields())
                    if (!field.IsLiteral && !field.IsStatic)
                        propertySource.Properties.Add(new ReflectedField(instance, field));
            }

            // Add items of an IEnumerable.
            var enumerable = instance as IEnumerable;
            if (enumerable != null)
            {
                int index = 0;
                foreach (var item in enumerable)
                {
                    string itemName = "Item[" + index + "]";
                    var namedItem = item as INamedObject;
                    if (namedItem != null)
                        itemName = Invariant($"{itemName}: {namedItem.Name}");

                    propertySource.Properties.Add(new CustomProperty(itemName, item)
                    {
                        Category = "Items",
                        IsReadOnly = true
                    });
                    index++;
                }
            }

            // ----- Set name:
            // 1. Try name parameter.
            propertySource.Name = name;

            // 2. Try INamedObject interface.
            if (propertySource.Name == null)
            {
                var namedObject = instance as INamedObject;
                if (namedObject != null)
                    propertySource.Name = namedObject.Name;
            }

            // 3. Try "Name" property.
            if (propertySource.Name == null)
            {
                var namePropertyDescriptor =
                    propertyDescriptors.OfType<PropertyDescriptor>()
                                       .FirstOrDefault(d => d.Name == "Name" && d.PropertyType == typeof(string));

                if (namePropertyDescriptor != null)
                    propertySource.Name = (string)namePropertyDescriptor.GetValue(instance);
            }

            // Fallback: Empty string.
            if (propertySource.Name == null)
                propertySource.Name = string.Empty;

            // Set type name.
            propertySource.TypeName = typeName;
            if (propertySource.TypeName == null)
                propertySource.TypeName = instance.GetType().Name;

            return propertySource;
        }
    }
}
