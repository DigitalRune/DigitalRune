// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Xml.Linq;
using DigitalRune.Windows;


namespace DigitalRune.Editor.Layout
{
    /// <summary>
    /// Represents a window layout.
    /// </summary>
    internal class WindowLayout : ObservableObject
    {
        /// <summary>
        /// Gets the name of the window layout.
        /// </summary>
        /// <value>The name of the window layout.</value>
        /// <remarks>
        /// The layout name must be a valid file name (without extension). It must not contain
        /// invalid characters!
        /// </remarks>
        public string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                    RaisePropertyChanged(nameof(Description));
            }
        }
        private string _name;


        /// <summary>
        /// Gets the entry to show in the "Manage Layouts" dialog.
        /// </summary>
        /// <value>The entry to show in the "Manage Layouts" dialog.</value>
        public string Description
        {
            get
            {
                if (IsFactoryPreset && IsActive)
                    return $"{Name} (factory preset, active)";

                if (IsFactoryPreset)
                    return $"{Name} (factory preset)";

                if (IsActive)
                    return $"{Name} (active)";

                return Name;
            }
        }


        /// <summary>
        /// Gets or sets the serialized window layout.
        /// </summary>
        /// <value>The serialized window layout.</value>
        public XElement SerializedLayout
        {
            get { return _serializedLayout; }
            set { SetProperty(ref _serializedLayout, value); }
        }
        private XElement _serializedLayout;


        /// <summary>
        /// Gets a value indicating whether this window layout is a factory preset.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the window layout is a factory preset; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>Factory presets cannot be renamed or deleted.</remarks>
        public bool IsFactoryPreset { get; }


        /// <summary>
        /// Gets a value indicating whether this window layout is active.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the window layout is active; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (SetProperty(ref _isActive, value))
                    RaisePropertyChanged(nameof(Description));
            }
        }
        private bool _isActive;


        /// <summary>
        /// Gets a value indicating whether the layout is dirty and needs to be saved on exit.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the layout is dirty and needs to be saved on exit; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsDirty
        {
            get { return _isDirty; }
            set { SetProperty(ref _isDirty, value); }
        }
        private bool _isDirty;


        /// <summary>
        /// Initializes a new instance of the <see cref="WindowLayout"/> class.
        /// </summary>
        /// <param name="name">The layout name.</param>
        /// <param name="isFactoryPreset">
        /// <see langword="true"/> if the window layout is a factory preset; otherwise,
        /// <see langword="false"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="name"/> is empty.
        /// </exception>
        public WindowLayout(string name, bool isFactoryPreset)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name.Length == 0)
                throw new ArgumentException("Argument name must not be an empty string.", nameof(name));

            _name = name;
            IsFactoryPreset = isFactoryPreset;
        }
    }
}
