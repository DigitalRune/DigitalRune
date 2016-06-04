// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Markup;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Binds a <see cref="MultiKeyGesture"/> to an <see cref="ICommand"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public class MultiKeyBinding : InputBinding
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="MultiKeyGesture"/>.
        /// </summary>
        /// <value>The <see cref="MultiKeyGesture"/>.</value>
        /// <exception cref="ArgumentNullException">
        /// The value is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The value is not a <see cref="MultiKeyGesture"/>.
        /// </exception>
        [TypeConverter(typeof(MultiKeyGestureConverter))]
        [ValueSerializer(typeof(MultiKeyGestureValueSerializer))]
        public override InputGesture Gesture
        {
            get
            {
                if (base.Gesture == null)
                    base.Gesture = new MultiKeyGesture(new[] { Key.None }, ModifierKeys.None);

                return base.Gesture as MultiKeyGesture;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (!(value is MultiKeyGesture))
                    throw new ArgumentException("Gesture is not a MultiKeyGesture.");

                base.Gesture = value;
            }
        }


        /// <summary>
        /// Gets or sets the keys.
        /// </summary>
        public IEnumerable<Key> Keys
        {
            get { return ((MultiKeyGesture)Gesture).Keys; }
            set { Gesture = new MultiKeyGesture(value, ((MultiKeyGesture)Gesture).Modifiers); }
        }


        /// <summary>
        /// Gets or sets the modifier keys.
        /// </summary> 
        public ModifierKeys Modifiers
        {
            get { return ((KeyGesture)Gesture).Modifiers; }
            set
            {
                Gesture = new MultiKeyGesture(((MultiKeyGesture)Gesture).Keys, value);
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiKeyBinding"/> class.
        /// </summary>
        public MultiKeyBinding()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MultiKeyBinding"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="gesture">The multi-key gesture.</param>
        public MultiKeyBinding(ICommand command, MultiKeyGesture gesture)
            : base(command, gesture)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MultiKeyBinding"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="keys">The keys.</param>
        /// <param name="modifiers">The modifier keys.</param>
        public MultiKeyBinding(ICommand command, IEnumerable<Key> keys, ModifierKeys modifiers)
            : base(command, new MultiKeyGesture(keys, modifiers))
        {
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------
        #endregion
    }
}
