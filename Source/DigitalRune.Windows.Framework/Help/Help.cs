// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Provides the ability to easily attach Help functionality to elements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The class <see cref="Help"/> provides two attached dependency properties:
    /// <list type="table">
    /// <listheader>
    /// <term>Attached Property</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="P:DigitalRune.Windows.Framework.Help.Url"/></term>
    /// <description>
    /// The address of the Help documentation. The property can be of the form
    /// C:\path\sample.chm or /folder/file.htm.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="P:DigitalRune.Windows.Framework.Help.Keyword"/></term>
    /// <description>The keyword to locate in the Help documentation.</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// The attached properties are automatically inherited by children of a framework element.
    /// Hence, the <see cref="P:DigitalRune.Windows.Framework.Help.Url"/> or a
    /// <see cref="P:DigitalRune.Windows.Framework.Help.Keyword"/> can be specified further up the
    /// logical tree or even on the root node.
    /// </para>
    /// <para>
    /// The class <see cref="Help"/> automatically registers a command binding for the
    /// <see cref="ApplicationCommands.Help"/> command and shows the Help documentation by using the
    /// current <see cref="HelpProvider"/>.
    /// </para>
    /// <example>
    /// In the following example a <see cref="Window"/> provides a Help file. One of the
    /// <see cref="TextBox"/>es is linked to a certain keyword.
    /// <code lang="xaml">
    /// <![CDATA[
    /// <Window x:Class="HelpSample.Window1"
    ///         xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///         xmlns:dr="http://schemas.digitalrune.com/windows"
    ///         Title="Window1" Height="300" Width="300"
    ///         dr:Help.Url="MyHelpfile.chm">
    ///   <Grid>
    ///     <Grid.RowDefinitions>
    ///       <RowDefinition/>
    ///       <RowDefinition/>
    ///     </Grid.RowDefinitions>
    ///     <TextBox dr:Help.Keyword="MyKeyword" Grid.Row="0" Text="Keyword based search"/>
    ///     <TextBox Grid.Row="1" Text="No keyword"/>
    ///   </Grid>
    /// </Window>
    /// ]]>
    /// </code>
    /// </example>
    /// </remarks>
    public static class Help
    {
        // This class is based on the following blog post:
        //
        //   http://peteohanlon.wordpress.com/2009/05/01/easy-help-with-wpf/.
        //
        // I made some improvements (MartinG):
        // - The attached properties are now automatically inherited. No need to recursively visit
        //   all parents.
        // - The Help provider is no longer hardcoded and can be set via a property.

        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the help provider.
        /// </summary>
        /// <value>
        /// The help provider. The default value is <see langword="null"/>.
        /// </value>
        public static IHelpProvider HelpProvider
        {
            get { return _helpProvider; }
            set
            {
                _helpProvider = value;
                CommandManager.InvalidateRequerySuggested();
            }
        }
        private static IHelpProvider _helpProvider; // new FormsHelpProvider();
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Framework.Help.Url"/> attached
        /// dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets the address of the Help documentation.
        /// </summary>
        /// <value>
        /// The address of the Help documentation. Examples: "C:\path\sample.chm",
        /// "/folder/file.htm".
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty UrlProperty = DependencyProperty.RegisterAttached(
            "Url",
            typeof(string),
            typeof(Help),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));


        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Framework.Help.Url"/> attached
        /// property from a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Framework.Help.Url"/> attached
        /// property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public static string GetUrl(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return (string)obj.GetValue(UrlProperty);
        }


        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Framework.Help.Url"/> attached
        /// property to a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static void SetUrl(DependencyObject obj, string value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.SetValue(UrlProperty, value);
        }


        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Framework.Help.Keyword"/> attached
        /// dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets the keyword to locate in the Help documentation.
        /// </summary>
        /// <value> The keyword to locate in the Help documentation.</value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty KeywordProperty = DependencyProperty.RegisterAttached(
          "Keyword",
          typeof(string),
          typeof(Help),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));


        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Framework.Help.Keyword"/>
        /// attached property from a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Framework.Help.Keyword"/> attached
        /// property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static string GetKeyword(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return (string)obj.GetValue(KeywordProperty);
        }


        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Framework.Help.Keyword"/>
        /// attached property to a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static void SetKeyword(DependencyObject obj, string value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.SetValue(KeywordProperty, value);
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="Help"/> class.
        /// </summary>
        static Help()
        {
            // Rather than having to manually associate the Help command, let's take care of this here.
            CommandManager.RegisterClassCommandBinding(
                typeof(FrameworkElement),
                new CommandBinding(ApplicationCommands.Help, Executed, CanExecute));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Determines whether the <see cref="ApplicationCommands.Help"/> command can be executed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="CanExecuteRoutedEventArgs"/> instance containing the event data.
        /// </param>
        private static void CanExecute(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            if (HelpProvider == null)
            {
                eventArgs.CanExecute = false;
                return;
            }

            var dependencyObject = sender as DependencyObject;
            if (dependencyObject != null)
            {
                string url = GetUrl(dependencyObject);
                if (!string.IsNullOrEmpty(url))
                    eventArgs.CanExecute = true;
            }
        }


        /// <summary>
        /// Occurs when the <see cref="ApplicationCommands.Help"/> command is executed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">
        /// The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.
        /// </param>
        private static void Executed(object sender, ExecutedRoutedEventArgs args)
        {
            var dependencyObject = args.OriginalSource as DependencyObject;
            string url = GetUrl(dependencyObject);
            string keyword = GetKeyword(dependencyObject);
            ShowHelp(url, keyword);
        }


        private static void ShowHelp(string url, string keyword)
        {
            if (HelpProvider == null)
                return;

            if (string.IsNullOrEmpty(keyword))
                HelpProvider.ShowHelp(url);
            else
                HelpProvider.ShowHelp(url, keyword);
        }
        #endregion
    }
}
