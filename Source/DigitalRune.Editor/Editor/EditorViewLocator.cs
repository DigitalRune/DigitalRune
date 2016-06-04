// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows;
using DigitalRune.Windows.Framework;
using Microsoft.Practices.ServiceLocation;
using NLog;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Resolves views for the view models of the editor and the editor extensions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The view locator searches through the editor's service provider for are a view that matches
    /// the type of the view model (or any of its base classes or interfaces).
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// Views need to be registered in the editor's service provider. A new editor extension usually
    /// registers new views in <strong>OnInitialize</strong>:
    /// </para>
    /// <code lang="csharp">
    /// <![CDATA[public class MyExtension : EditorExtension
    /// {
    ///     protected override void OnInitialize()
    ///     {
    ///         // Register views.
    ///         serviceContainer.RegisterView(typeof(MyViewModel), typeof(MyView));
    ///         serviceContainer.RegisterView(typeof(MyOtherViewModel), typeof(MyOtherView), CreationPolicy.Shared);
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// <para>
    /// The method <see cref="EditorHelper.RegisterView"/> is an extension method provided by the
    /// <see cref="EditorHelper"/>. It is just a convenience method which wraps the following call:
    /// </para>
    /// <code lang="csharp">
    /// <![CDATA[serviceContainer.Register(typeof(FrameworkElement), typeof(MyViewModel).FullName, typeof(MyView), creationPolicy);]]>
    /// </code>
    /// </example>
    internal class EditorViewLocator : IViewLocator
    {
        // Notes:
        // - The view locator searches only the service container specified in the constructor. It 
        //   will not find views which are registered in a another (child) service container.


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IServiceLocator _services;


        /// <summary>
        /// Initializes a new instance of the <see cref="EditorViewLocator"/> class.
        /// </summary>
        /// <param name="services">The service container.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="services"/> is <see langword="null"/>.
        /// </exception>
        public EditorViewLocator(IServiceLocator services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            _services = services;
        }


        /// <inheritdoc cref="IViewLocator.GetView"/>
        public FrameworkElement GetView(object viewModel, DependencyObject parent = null, object context = null)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            Logger.Debug(CultureInfo.InvariantCulture, "Trying to resolve view for {0}.", viewModel.GetType().FullName);

            // Look for a view based on the class.
            var type = viewModel.GetType();
            while (type != null)
            {
                var view = _services.GetInstance<FrameworkElement>(type.FullName);
                if (view != null)
                    return view;

                type = type.BaseType;
            }

            // Look for a view based on the interface.
            type = viewModel.GetType();
            foreach (var @interface in type.GetInterfaces())
            {
                var view = _services.GetInstance<FrameworkElement>(@interface.FullName);
                if (view != null)
                    return view;
            }

            var message = string.Format(CultureInfo.InvariantCulture, "Unable to resolve view for view model of type {0}.", viewModel.GetType());
            throw new EditorException(message);
        }
    }
}
