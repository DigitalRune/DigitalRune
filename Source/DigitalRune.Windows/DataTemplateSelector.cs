// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace DigitalRune.Windows
{
  /// <summary>
  /// Automatically applies a <see cref="DataTemplate"/> to the content that matches the type of the
  /// <see cref="FrameworkElement.DataContext"/>. (Replacement for implicit data templates which are
  /// available in WPF, but not in Silverlight.)
  /// </summary>
  /// <remarks>
  /// A data template must be registered in a resource dictionary using the full type name (e.g. 
  /// "MyApplication.Views.MyView") as its key.
  /// </remarks>
  public class DataTemplateSelector : ContentControl
  {
    /// <summary>
    /// Called when the value of the <see cref="ContentControl.Content"/> property changes.
    /// </summary>
    /// <param name="oldContent">The old value of the <see cref="ContentControl.Content"/> property.</param>
    /// <param name="newContent">The new value of the <see cref="ContentControl.Content"/> property.</param>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
      if (newContent != null)
        ContentTemplate = FindDataTemplate(newContent.GetType().FullName);
      else
        ContentTemplate = null;
    }


    private DataTemplate FindDataTemplate(string key)
    {
      // Check resource dictionaries of visual ancestors and self.
      DependencyObject dependencyObject = this;
      while (dependencyObject != null)
      {
        var element = dependencyObject as FrameworkElement;
        if (element != null)
        {
          var dataTemplate = element.Resources[key] as DataTemplate;
          if (dataTemplate != null)
            return dataTemplate;
        }

        dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
      }

      // Check resource dictionary of application.
      return Application.Current.Resources[key] as DataTemplate;
    }
  }
}
#endif
