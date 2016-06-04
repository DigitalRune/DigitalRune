// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using DigitalRune.Windows;
using DigitalRune.Windows.Controls;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Properties
{
    /// <summary>
    /// Represents the Properties window.
    /// </summary>
    internal class PropertiesViewModel : EditorDockTabItemViewModel
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        internal const string DockIdString = "Properties";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly IEditorService _editor;

        // The history of objects that were shown.
        private readonly List<object> _history = new List<object>();
        private object _currentInstance;

        private bool _requiresTimer;
        private DispatcherTimer _timer;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets an instance of the <see cref="PropertiesViewModel"/> that can be used at
        /// design-time.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="PropertiesViewModel"/> that can be used at design-time.
        /// </value>
        internal static PropertiesViewModel DesignInstance
        {
            get
            {
                var vm = new PropertiesViewModel(null);
                vm.PropertySource = PropertyGridHelper.CreatePropertySource(vm);
                return vm;
            }
        }


        /// <summary>
        /// Gets the properties shown in the property grid.
        /// </summary>
        /// <value>The properties shown in the property grid.</value>
        public IPropertySource PropertySource
        {
            get { return _propertySource; }
            private set
            {
                if (SetProperty(ref _propertySource, value))
                {
                    _requiresTimer = RequiresTimer();
                    UpdateTimer();
                }
            }
        }
        private IPropertySource _propertySource;


        /// <summary>
        /// Gets or sets the selected property.
        /// </summary>
        /// <value>The selected property.</value>
        public IProperty SelectedProperty
        {
            get { return _selectedProperty; }
            set { SetProperty(ref _selectedProperty, value); }
        }
        private IProperty _selectedProperty;


        /// <summary>
        /// Gets the command that is invoked to inspect a property value.
        /// </summary>
        /// <value>The command that is invoked to inspect a property value.</value>
        public DelegateCommand<IProperty> InspectCommand { get; }


        /// <summary>
        /// Gets the command that is invoked to navigate back to the previous object.
        /// </summary>
        /// <value>The command that is invoked to navigate back to the previous object.</value>
        public DelegateCommand BackCommand { get; }


        /// <summary>
        /// Gets the command that is invoked to update the property values.
        /// </summary>
        /// <value>The command that is invoked to update the property values.</value>
        public DelegateCommand RefreshCommand { get; }


        /// <summary>
        /// Gets the command that is invoked to copy the value of a property to the clipboard.
        /// </summary>
        /// <value>
        /// The command that is invoked to copy the value of a property to the clipboard.
        /// </value>
        public DelegateCommand CopyValueCommand { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertiesViewModel"/> class.
        /// </summary>
        /// <param name="editor">The editor. Can be <see langword="null"/> at design-time.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> is <see langword="null"/>.
        /// </exception>
        public PropertiesViewModel(IEditorService editor)
        {
            DisplayName = "Properties";
            DockId = DockIdString;
            //Icon = MultiColorGlyphs.Properties;
            IsPersistent = true;
            DockWidth = new GridLength(400);
            DockHeight = new GridLength(600);

            InspectCommand = new DelegateCommand<IProperty>(Inspect);
            BackCommand = new DelegateCommand(GoBack, CanGoBack);
            RefreshCommand = new DelegateCommand(Refresh);
            CopyValueCommand = new DelegateCommand(CopyValue, CanCopyValue);

            if (!WindowsHelper.IsInDesignMode)
            {
                if (editor == null)
                    throw new ArgumentNullException(nameof(editor));

                _editor = editor;
            }

            Show(null, false);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            if (eventArgs.Opened)
            {
                // Initialize timer.
                _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
                _timer.Tick += OnTimerTick;
            }

            UpdateTimer();
            base.OnActivated(eventArgs);
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            UpdateTimer();

            if (eventArgs.Closed)
            {
                //PropertySource = null;
                _timer = null;
            }

            base.OnDeactivated(eventArgs);
        }



        private void UpdateTimer()
        {
            if (_timer == null)
                return;

            _timer.IsEnabled = IsActive && _requiresTimer;
        }


        private static IPropertySource CreatePropertySource(object instance)
        {
            if (instance == null)
                return null;

            return instance as IPropertySource ?? PropertyGridHelper.CreatePropertySource(instance);
        }


        private bool RequiresTimer()
        {
            return PropertySource != null && PropertySource.Properties.OfType<IReflectedProperty>().Any();
        }


        public void Show(object instance, bool keepHistory)
        {
            if (keepHistory)
                _history.Add(_currentInstance);
            else
                _history.Clear();

            _currentInstance = instance;
            PropertySource = CreatePropertySource(instance);
        }


        public void Hide(object instance)
        {
            // Delete ALL instances from history.
            while (_history.Remove(instance)) { }

            if (instance == PropertySource)
            {
                // Instance is currently shown.
                if (CanGoBack())
                    GoBack();
                else
                    Show(null, false);
            }
        }


        internal void Inspect(IProperty property)
        {
            var value = property?.Value;
            if (value != null)
                Show(value, true);
        }


        private bool CanGoBack()
        {
            return _history.Count > 0;
        }


        private void GoBack()
        {
            if (!CanGoBack())
                return;

            // Pop properties source from history stack.
            _currentInstance = _history.Last();
            PropertySource = CreatePropertySource(_currentInstance);
            _history.RemoveAt(_history.Count - 1);
        }


        private void Refresh()
        {
            var source = PropertySource;
            PropertySource = null;
            PropertySource = source;
        }


        private bool CanCopyValue()
        {
            return SelectedProperty?.Value != null;
        }


        private void CopyValue()
        {
            if (!CanCopyValue())
                return;

            Clipboard.SetText(SelectedProperty.Value.ToString());
        }


        /// <inheritdoc/>
        protected override void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            if (InspectCommand != null) // Method is called in constructor before commands are set.
            {
                InspectCommand.RaiseCanExecuteChanged();
                BackCommand.RaiseCanExecuteChanged();
                RefreshCommand.RaiseCanExecuteChanged();
                CopyValueCommand.RaiseCanExecuteChanged();
            }

            base.OnPropertyChanged(eventArgs);
        }


        private void OnTimerTick(object sender, EventArgs eventArgs)
        {
            if (PropertySource == null)
                return;

            Debug.Assert(RequiresTimer(), "Timer should only be enabled when needed.");

            foreach (var property in PropertySource.Properties.OfType<IReflectedProperty>())
                property.Update();
        }
        #endregion
    }
}
