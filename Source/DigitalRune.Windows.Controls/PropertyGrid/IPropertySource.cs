// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.ObjectModel;
using System.ComponentModel;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Describes the properties which should be displayed in a <see cref="PropertyGrid"/>.
    /// </summary>
    public interface IPropertySource : INamedObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the type name of the property source.
        /// </summary>
        /// <value>The type name of the property source.</value>
        string TypeName { get; }


        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>A collection of properties.</value>
        ObservableCollection<IProperty> Properties { get; }
    }
}
