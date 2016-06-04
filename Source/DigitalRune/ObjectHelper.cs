// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
#if !NETFX_CORE && !PORTABLE
using System.ComponentModel;  // TypeDescriptor
#endif
#if !XBOX
using System.Linq.Expressions;
using System.Reflection;
#endif


namespace DigitalRune
{
  /// <summary>
  /// Provides extension methods applicable to all objects.
  /// </summary>
  public static class ObjectHelper
  {
#if !XBOX && !PORTABLE
    /// <overloads>
    /// <summary>
    /// Retrieves the name of a property identified by a lambda expression.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Retrieves the name of a given object's property identified by a lambda expression.
    /// </summary>
    /// <typeparam name="TObject">The type of object containing the property.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="this">The object containing the property.</param>
    /// <param name="expression">
    /// A lambda expression selecting the property from the containing object.
    /// </param>
    /// <returns>The name of the property accessed by <paramref name="expression"/>.</returns>
    /// <remarks>
    /// It is recommended to use this method to access the name of a property instead of using 
    /// hard-coded string values.
    /// </remarks>
    /// <example>
    /// The following example shows how to retrieve the name of a property as a string.
    /// <code lang="csharp">
    /// <![CDATA[
    /// class MyClass
    /// {
    ///   public int MyProperty { get; set; }
    /// }
    /// 
    /// ...
    /// 
    /// MyClass myClass = new MyClass();
    /// 
    /// // The following line retrieves the property name as a string.
    /// string propertyName = myClass.GetPropertyName(o => o.MyProperty);
    /// ]]>
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="expression"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="expression"/> does not represent an expression accessing a property.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "this")]
    public static string GetPropertyName<TObject, TProperty>(this TObject @this, Expression<Func<TObject, TProperty>> expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      // For reference see:
      //   http://themechanicalbride.blogspot.com/2007/03/symbols-on-steroids-in-c.html
      //   http://www.ingebrigtsen.info/post/2008/12/11/INotifyPropertyChanged-revisited.aspx
      //   http://michaelsync.net/2009/04/09/silverlightwpf-implementing-propertychanged-with-expression-tree
      //   http://joshsmithonwpf.wordpress.com/2009/07/11/one-way-to-avoid-messy-propertychanged-event-handling/

      var memberExpression = expression.Body as MemberExpression;
      if (memberExpression == null)
        throw new ArgumentException("The argument does not represent an expression accessing a field or property.", "expression");

      return memberExpression.Member.Name;
    }


    /// <summary>
    /// Retrieves the name of a property identified by a lambda expression.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="expression">A lambda expression selecting the property.</param>
    /// <returns>The name of the property accessed by <paramref name="expression"/>.</returns>
    /// <remarks>
    /// It is recommended to use this method to access the name of a property instead of using 
    /// hard-coded string values.
    /// </remarks>
    /// <example>
    /// The following example shows how to retrieve the name of a property as a string.
    /// <code lang="csharp">
    /// <![CDATA[
    /// class MyClass
    /// {
    ///   public int MyProperty { get; set; }
    /// }
    /// 
    /// ...
    /// 
    /// MyClass myClass = new MyClass();
    /// 
    /// // The following line retrieves the property name as a string.
    /// string propertyName = ObjectHelper.GetPropertyName(() => myClass.MyProperty);
    /// ]]>
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="expression"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="expression"/> does not represent an expression accessing a property.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static string GetPropertyName<T>(Expression<Func<T>> expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      var memberExpression = expression.Body as MemberExpression;
      if (memberExpression == null)
        throw new ArgumentException("The argument does not represent an expression accessing a field or property.", "expression");

      var property = memberExpression.Member as PropertyInfo;
      if (property == null)
        throw new ArgumentException("The argument does not represent an expression accessing a property.", "expression");

      return memberExpression.Member.Name;
    }
#endif


    /// <summary>
    /// Parses the specified value (using the invariant culture).
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    /// <para>
    /// This method can parse values of types that
    /// <list type="bullet">
    /// <item>implement <strong>System.IConvertible</strong>,</item>
    /// <item>have a static method <strong>Parse(string)</strong> or <strong>Parse(string, IFormatProvider)</strong>, or</item>
    /// <item>have a <strong>TypeConverter</strong>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// A <see cref="NotSupportedException"/> is thrown for other types that do not have an 
    /// automatic mechanism for value conversion from string. Further, an exception can be thrown
    /// if the given string is invalid.
    /// </para>
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// Cannot convert string to target type.
    /// </exception>
    public static T Parse<T>(string value)
    {
      return (T)Parse(typeof(T), value);
    }


    /// <summary>
    /// Parses a string and converts it to the specified type (using the invariant culture).
    /// </summary>
    /// <param name="type">The target type.</param>
    /// <param name="value">The value.</param>
    /// <returns>An instance of the given type that represents the string value.</returns>
    /// <remarks>
    /// <para>
    /// This method can parse values of types that
    /// <list type="bullet">
    /// <item>implement <strong>System.IConvertible</strong>,</item>
    /// <item>have a static method <strong>Parse(string)</strong> or <strong>Parse(string, IFormatProvider)</strong>, or</item>
    /// <item>have a <strong>TypeConverter</strong>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// A <see cref="NotSupportedException"/> is thrown for other types that do not have an 
    /// automatic mechanism for value conversion from string. Further, an exception can be thrown
    /// if the given string is invalid.
    /// </para>
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// Cannot convert string to target type.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TypeConverter")]
    public static object Parse(Type type, string value)
    {
      if (type == null)
        throw new ArgumentNullException("type");

      if (type == typeof(string))
        return value;

      // Enumerations implement IConvertible - but do not support conversion from string! Therefore,
      // we use class Enum. This class supports [Flags] enums too: e.g. "EnumVal0, EnumVal1".
#if !NETFX_CORE && !NET45
      if (type.IsEnum)
        return Enum.Parse(type, value, true);
#else
      var typeInfo = type.GetTypeInfo();
      if (typeInfo.IsEnum)
        return Enum.Parse(type, value, true);
#endif

      if (IsConvertible(type))
        return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);

#if !NETFX_CORE && !NET45
      var parseMethod = type.GetMethod("Parse", new[] { typeof(string), typeof(IFormatProvider) });
      if (parseMethod != null && parseMethod.IsStatic)
        return parseMethod.Invoke(null, new object[] { value, CultureInfo.InvariantCulture });

      parseMethod = type.GetMethod("Parse", new[] { typeof(string) });
      if (parseMethod != null && parseMethod.IsStatic)
        return parseMethod.Invoke(null, new object[] { value });
#else
      var methods = typeInfo.GetDeclaredMethods("Parse");
      MethodInfo parseString = null;
      MethodInfo parseStringFormat = null;
      foreach (var method in methods)
      {
        var parameters = method.GetParameters();
        if (parseString == null && parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
        {
          parseString = method;
        }
        else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType == typeof(IFormatProvider))
        {
          parseStringFormat = method;
        }

        if (parseStringFormat != null)
          break;
      }

      if (parseStringFormat != null)
        return parseStringFormat.Invoke(null, new object[] { value, CultureInfo.InvariantCulture });

      if (parseString != null)
        return parseString.Invoke(null, new object[] { value });
#endif

#if !NETFX_CORE && !PORTABLE
      var converter = GetTypeConverter(type);
      if (converter != null)
      {
        // ReSharper disable AssignNullToNotNullAttribute
        return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
        // ReSharper restore AssignNullToNotNullAttribute
      }

      throw new NotSupportedException("Cannot convert string to type '{0}'. No Parse(string) method or TypeConverter found.");

#else
      throw new NotSupportedException("Cannot convert string to type '{0}'. No Parse(string) method found.");
#endif
    }


    /// <summary>
    /// Determines the <see cref="ObjectHelper"/> <strong>Parse</strong> methods can parse the 
    /// given type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>
    /// <see langword="true"/> if the type can be parsed; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    public static bool CanParse(Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");

      if (type == typeof(string))
        return true;

#if !NETFX_CORE && !NET45
      if (type.IsEnum)
        return true;
#else
      if (type.GetTypeInfo().IsEnum)
        return true;
#endif

      if (IsConvertible(type))
        return true;

#if !NETFX_CORE && !PORTABLE
      var converter = GetTypeConverter(type);
      if (converter != null && converter.CanConvertFrom(typeof(string)))
        return true;
#endif

      return false;
    }


#if !NETFX_CORE && !PORTABLE
    /// <summary>
    /// Gets the type converter for the given type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>
    /// A new type converter instance for the given type; or <see langword="null"/> if no type
    /// converter was found.
    /// </returns>
    /// <remarks>
    /// This method is similar to <strong>TypeDescriptor.GetConverter(Type)</strong> - but it is 
    /// also available in Silverlight and on the Xbox 360.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    public static TypeConverter GetTypeConverter(Type type)
    {
#if !SILVERLIGHT && !WP7 && !WP8 && !XBOX
      return TypeDescriptor.GetConverter(type);
#else
      TypeConverterAttribute attribute = (TypeConverterAttribute)Attribute.GetCustomAttribute(type, typeof(TypeConverterAttribute), false); 
      if (attribute != null)
      {
        try
        {
          var converterType = Type.GetType(attribute.ConverterTypeName, false); 
          if (converterType != null)
          {
            return (Activator.CreateInstance(converterType) as TypeConverter);
          }
        } 
        // ReSharper disable once EmptyGeneralCatchClause
        catch
        {
        }
      }
      return null;
#endif
    }
#endif


    /// <overloads>
    /// <summary>
    /// Determines whether the specified object/type is a base data type or implements the
    /// <see cref="IConvertible"/> interface.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified object is a base data type or implements the
    /// <see cref="IConvertible"/> interface.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is convertible; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
    public static bool IsConvertible(object obj)
    {
#if NETFX_CORE || NET45
      return obj != null && IsConvertible(obj.GetType());
#else
      return obj is IConvertible;
#endif
    }


    /// <summary>
    /// Determines whether the specified type is a base data type or implements the 
    /// <see cref="IConvertible"/> interface.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="type"/> is a convertible; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsConvertible(Type type)
    {
#if NETFX_CORE || NET45
      return type == typeof(bool)
             || type == typeof(int) || type == typeof(byte) || type == typeof(short) || type == typeof(long)
             || type == typeof(float) || type == typeof(double) || type == typeof(decimal)
             || type == typeof(char) || type == typeof(string) || type == typeof(DateTime)
             || type == typeof(uint) || type == typeof(sbyte) || type == typeof(ushort) || type == typeof(ulong)
             || type.GetTypeInfo().IsEnum;
#else
      return typeof(IConvertible).IsAssignableFrom(type);
#endif
    }


    /// <overloads>
    /// <summary>
    /// Converts the object to the specified target type.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts the object to the specified target type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>A value of type <typeparamref name="T"/>.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="value"/> is not in a format for <typeparamref name="T"/> recognized by the
    /// current format provider.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// This conversion is not supported.<br/>
    /// Or, <paramref name="value"/> is <see langword="null"/> and <typeparamref name="T"/> is a
    /// value type. Or, <paramref name="value"/> does not implement the <see cref="IConvertible"/>
    /// interface.
    /// </exception>
    /// <exception cref="OverflowException">
    /// <paramref name="value"/> represents a number that is out of the range of 
    /// <typeparamref name="T"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
    public static T ConvertTo<T>(object value)
    {
      // TODO: For more advanced conversions take a look at Json.NET (Newtonsoft.Json\Src\Newtonsoft.Json\Utilities\ConvertUtils.cs).

      if (value is T)
        return (T)value;

#if SILVERLIGHT || WP7 || XBOX || PORTABLE
      return ConvertTo<T>(value, CultureInfo.CurrentCulture);
#else
      return (T)Convert.ChangeType(value, typeof(T));
#endif
    }


    /// <summary>
    /// Converts the object to the specified target type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// </param>
    /// <returns>A value of type <typeparamref name="T"/>.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="value"/> is not in a format for <typeparamref name="T"/> recognized by 
    /// <paramref name="provider"/>.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// This conversion is not supported.<br/>
    /// Or, <paramref name="value"/> is <see langword="null"/> and <typeparamref name="T"/> is a
    /// value type. Or, <paramref name="value"/> does not implement the <see cref="IConvertible"/>
    /// interface.
    /// </exception>
    /// <exception cref="OverflowException">
    /// <paramref name="value"/> represents a number that is out of the range of 
    /// <typeparamref name="T"/>.
    /// </exception>
    public static T ConvertTo<T>(object value, IFormatProvider provider)
    {
      // TODO: For more advanced conversions take a look at Json.NET (Newtonsoft.Json\Src\Newtonsoft.Json\Utilities\ConvertUtils.cs).

      if (value is T)
        return (T)value;

      return (T)Convert.ChangeType(value, typeof(T), provider);
    }


    /// <summary>
    /// Safely disposes the object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to dispose. Can be <see langword="null"/>.</param>
    /// <remarks>
    /// The method calls <see cref="IDisposable.Dispose"/> if the <paramref name="obj"/> is not null
    /// and implements the interface <see cref="IDisposable"/>.
    /// </remarks>
    public static void SafeDispose<T>(this T obj) where T : class
    {
      var disposable = obj as IDisposable;
      if (disposable != null)
        disposable.Dispose();
    }
  }
}
