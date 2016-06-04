// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Windows;
using Microsoft.Xna.Framework;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Represents a control that allows to edit a 3-dimensional vectors.
    /// </summary>
    /// <remarks>
    /// The property <see cref="Value"/> contains the 3-dimensional vector. Supported types are:
    /// <see cref="Vector3"/> (XNA/MonoGame), <see cref="Vector3F"/> (DigitalRune) and 
    /// <see cref="Vector3D"/> (DigitalRune).
    /// </remarks>
    internal partial class Vector3Editor
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private bool _isUpdating;
        private Type _vectorType;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly",
            typeof(bool),
            typeof(Vector3Editor),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, OnIsReadOnlyChanged));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, Boxed.Get(value)); }
        }


        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(object),
            typeof(Vector3Editor),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnVectorChanged));

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        public static readonly DependencyProperty XProperty = DependencyProperty.Register(
            "X",
            typeof(double),
            typeof(Vector3Editor),
            new FrameworkPropertyMetadata(Boxed.DoubleZero, OnXyzChanged));

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }


        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
            "Y",
            typeof(double),
            typeof(Vector3Editor),
            new PropertyMetadata(Boxed.DoubleZero, OnXyzChanged));

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }


        public static readonly DependencyProperty ZProperty = DependencyProperty.Register(
            "Z",
            typeof(double),
            typeof(Vector3Editor),
            new PropertyMetadata(Boxed.DoubleZero, OnXyzChanged));

        public double Z
        {
            get { return (double)GetValue(ZProperty); }
            set { SetValue(ZProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public Vector3Editor()
        {
            InitializeComponent();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnIsReadOnlyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (Vector3Editor)dependencyObject;
            target.Grid.IsEnabled = !target.IsReadOnly;
        }


        private static void OnVectorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (Vector3Editor)dependencyObject;
            target.OnValueChanged();
        }


        private void OnValueChanged()
        {
            if (_isUpdating)
                return;
            if (Value == null)
                return;

            _vectorType = Value.GetType();

            // We have to disable OnXyzChanged() while we set X, Y and Z; otherwise, X = ... will
            // call OnXyzChanged and override Value.Y and Value.Z!
            _isUpdating = true;
            try
            {
                if (_vectorType == typeof(Vector3))
                {
                    var v = (Vector3)Value;
                    X = SafeConvertToDouble(v.X);
                    Y = SafeConvertToDouble(v.Y);
                    Z = SafeConvertToDouble(v.Z);
                }
                else if (_vectorType == typeof(Vector3F))
                {
                    var v = (Vector3F)Value;
                    X = SafeConvertToDouble(v.X);
                    Y = SafeConvertToDouble(v.Y);
                    Z = SafeConvertToDouble(v.Z);
                }
                else if (_vectorType == typeof(Vector3D))
                {
                    var v = (Vector3D)Value;
                    X = v.X;
                    Y = v.Y;
                    Z = v.Z;
                }
                else
                {
                    if (Value != null)
                        throw new NotSupportedException("Vector3Editor.Value must be a Vector3, Vector3F or Vector3D.");

                    X = 0;
                    Y = 0;
                    Z = 0;
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }


        private static void OnXyzChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (Vector3Editor)dependencyObject;
            target.OnXyzChanged();
        }


        private void OnXyzChanged()
        {
            if (_isUpdating)
                return;

            _isUpdating = true;
            try
            {
                if (_vectorType == typeof(Vector3))
                    Value = new Vector3((float)X, (float)Y, (float)Z);
                else if (_vectorType == typeof(Vector3F))
                    Value = new Vector3F((float)X, (float)Y, (float)Z);
                else if (_vectorType == typeof(Vector3D))
                    Value = new Vector3D(X, Y, Z);
            }
            finally
            {
                _isUpdating = false;
            }
        }


        internal static double SafeConvertToDouble(float value)
        {
            // When casting from float to double we use 
            //   var d = (double)(decimal)f;
            // If we cast directly from float to double, we get garbage decimal digits, e.g.
            //   0.3f --> 0.30000001192092896

            if (value < (float)decimal.MinValue || value > (float)decimal.MaxValue || !Numeric.IsFinite(value))
            {
                // Cannot use the trick. Decimal has smaller range than float and double! Casting to
                // decimal would throw exception.
                return value;
            }

            return (double)(decimal)value;
        }
        #endregion
    }
}
