// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.ObjectModel;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a custom property shown in a <see cref="PropertyGrid"/>.
    /// </summary>
    public class PropertySource : ObservableObject, IPropertySource
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name of the property source.
        /// </summary>
        /// <value>The name of the property source.</value>
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        private string _name;


        /// <summary>
        /// Gets or sets the type name of the property source.
        /// </summary>
        /// <value>The type name of the property source.</value>
        public string TypeName
        {
            get { return _typeName; }
            set { SetProperty(ref _typeName, value); }
        }
        private string _typeName;


        /// <inheritdoc />
        public ObservableCollection<IProperty> Properties { get; } = new ObservableCollection<IProperty>();


        /// <summary>
        /// Gets or sets user-defined data.
        /// </summary>
        /// <value>The user-defined data.</value>
        /// <remarks>
        /// This property is not used by the <see cref="PropertyGrid"/>. It can be used by the
        /// application to store custom data.
        /// </remarks>
        public object UserData { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------
        #endregion

    }
}
