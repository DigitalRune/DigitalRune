// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;


namespace DigitalRune.ServiceLocation
{
    /// <summary>
    /// Identifies an entry in the <see cref="ServiceContainer"/>.
    /// </summary>
    internal struct ServiceRegistration : IEquatable<ServiceRegistration>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        /// <summary>
        /// The type of the service.
        /// </summary>
        public readonly Type Type;


        /// <summary>
        /// The name under which the object should be registered. Can be <see langword="null"/> or
        /// empty.
        /// </summary>
        public readonly string Key;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistration"/> struct.
        /// </summary>
        /// <param name="type">The type of the service.</param>
        /// <param name="key">
        /// The name under which the object should be registered. Can be <see langword="null"/> or
        /// empty.
        /// </param>
        public ServiceRegistration(Type type, string key)
        {
            Type = type;
            Key = key;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current
        /// <see cref="object"/>.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current
        /// <see cref="object"/>.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="object"/> to compare with the current <see cref="object"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="object"/> is equal to the current
        /// <see cref="object"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is ServiceRegistration && this == (ServiceRegistration)obj;
        }


        /// <summary>
        /// Determines whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the other parameter; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Equals(ServiceRegistration other)
        {
            return this == other;
        }


        /// <summary>
        /// Compares two <see cref="ServiceRegistration"/> objects to determine whether they are the
        /// same.
        /// </summary>
        /// <param name="obj1">The first <see cref="ServiceRegistration"/>.</param>
        /// <param name="obj2">The second <see cref="ServiceRegistration"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="obj1"/> and <paramref name="obj2"/> are
        /// the same; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(ServiceRegistration obj1, ServiceRegistration obj2)
        {
            return obj1.Type == obj2.Type && obj1.Key == obj2.Key;
        }


        /// <summary>
        /// Compares two <see cref="ServiceRegistration"/> objects to determine whether they are
        /// different.
        /// </summary>
        /// <param name="obj1">The first <see cref="ServiceRegistration"/>.</param>
        /// <param name="obj2">The second <see cref="ServiceRegistration"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="obj1"/> and <paramref name="obj2"/> are
        /// different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(ServiceRegistration obj1, ServiceRegistration obj2)
        {
            return obj1.Type != obj2.Type || obj1.Key != obj2.Key;
        }


        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            int hashCode = 0;
            if (Type != null)
                hashCode = Type.GetHashCode();
            if (Key != null)
                hashCode ^= Key.GetHashCode();

            return hashCode;
        }


        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the current <see cref="object"/>.
        /// </returns>
        public override string ToString()
        {
            if (Key == null)
                return string.Format(CultureInfo.InvariantCulture, "({0}; null)", Type.Name);

            return string.Format(CultureInfo.InvariantCulture, "({0}; \"{1}\")", Type.Name, Key);
        }
        #endregion
    }
}
