using System;
using System.Collections.Generic;
using System.Windows;
using static System.FormattableString;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Provides a <see cref="ResourceDictionary"/> which loads its content only once when several
    /// controls use the same resources.
    /// </summary>
    /// <remarks>
    /// To work in Blend and VS designer, use the property <see cref="SourcePath"/> to set a
    /// relative URI instead of the property <see cref="Source"/>!
    /// </remarks>
    /// <example>
    /// <para>
    /// The following examples shows how to add a shared resource dictionary in XAML.
    /// </para>
    /// <code lang="xaml">
    /// <![CDATA[
    /// <UserControl x:Class="MyApplication.MyView"
    ///              xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///              xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///              xmlns:dr="http://schemas.digitalrune.com/windows">
    ///   <UserControl.Resources>
    ///     <ResourceDictionary>
    ///       <ResourceDictionary.MergedDictionaries>
    ///         <dr:SharedResourceDictionary SourcePath="/MyApplication;component/Resources/Resources.xaml" />
    ///       </ResourceDictionary.MergedDictionaries>
    /// 
    ///       <BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter" />
    ///       ...
    ///     </ResourceDictionary>
    ///   </UserControl.Resources>
    /// 
    ///   ...
    /// 
    /// </UserControl>
    /// ]]>
    /// </code>
    /// </example>
    public class SharedResourceDictionary : ResourceDictionary
    {
        private static readonly Dictionary<Uri, ResourceDictionary> _sharedDictionaries = new Dictionary<Uri, ResourceDictionary>();


        /// <summary>
        /// Gets or sets the relative URI to load resources from.
        /// </summary>
        /// <value>The relative URI to load resources from.</value>
        public string SourcePath
        {
            get { return _sourcePath; }
            set
            {
                _sourcePath = value;
                Source = new Uri(value, UriKind.Relative);
            }
        }
        private string _sourcePath;


        /// <summary>
        /// Gets or sets the uniform resource identifier (URI) to load resources from.
        /// </summary>
        /// <value>The uniform resource identifier (URI) to load resources from.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SourcePath")]
        public new Uri Source
        {
            get { return _source; }
            set
            {
                _source = value;

                if (value == null)
                {
                    base.Source = value;
                    return;
                }

                if (WindowsHelper.IsInDesignMode)
                {
                    if (value.IsAbsoluteUri)
                        throw new NotSupportedException("Absolute source URI are not supported at design-time. Set property SourcePath with a relative URI.");

                    try
                    {
                        MergedDictionaries.Add(Application.LoadComponent(value) as ResourceDictionary);
                        return;
                    }
                    catch (Exception exception)
                    {
                        throw new Exception(Invariant($"Could not load resource: '{value.OriginalString}'"), exception);
                    }
                }

                ResourceDictionary dictionary;
                if (_sharedDictionaries.TryGetValue(value, out dictionary))
                {
                    // Shared instance of dictionary is already loaded.
                    MergedDictionaries.Add(dictionary);
                    return;
                }

                // Let the base class load the dictionary.
                base.Source = value;

                // Remember loaded dictionary.
                _sharedDictionaries.Add(value, this);
            }
        }
        private Uri _source;
    }
}
