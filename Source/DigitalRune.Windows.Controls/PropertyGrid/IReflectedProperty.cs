// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Describes a single field/property that was created using reflection.
    /// </summary>
    public interface IReflectedProperty : IProperty
    {
        /// <summary>
        /// Checks whether the <see cref="ReflectedField.Value"/> has changed and triggers the
        /// <see cref="ReflectedField.PropertyChanged"/> event if necessary.
        /// </summary>
        /// <remarks>
        /// This method is not necessary if the property value is changed via the
        /// <see cref="PropertyDescriptor"/>'s <see cref="PropertyDescriptor.SetValue"/> method or
        /// if the <see cref="ReflectedField.Instance"/> that owns this property implements
        /// <see cref="INotifyPropertyChanged"/> or has a <i>PropertyName</i> Changed event.
        /// </remarks>
        void Update();
    }
}
