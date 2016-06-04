using System.ComponentModel;
using System.Windows;


namespace WpfInteropSample2
{
  /// <summary>
  /// Provides helper methods for WPF.
  /// </summary>
  public static class WindowsHelper
  {
    /// <summary>
    /// Gets a value indicating whether the controls runs in the context of a designer (e.g.
    /// Visual Studio Designer or Expression Blend).
    /// </summary>
    /// <value>
    /// <see langword="true" /> if controls run in design mode; otherwise, 
    /// <see langword="false" />.
    /// </value>
    public static bool IsInDesignMode
    {
      get
      {
        if (!_isInDesignMode.HasValue)
          _isInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement)).Metadata.DefaultValue;

        return _isInDesignMode.Value;
      }
    }

    private static bool? _isInDesignMode;
  }
}