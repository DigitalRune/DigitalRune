// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Windows;
using Microsoft.Xna.Framework;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Represents a control that allows to edit a 4-dimensional vectors.
    /// </summary>
    /// <remarks>
    /// The property <see cref="Value"/> contains the 4-dimensional vector. Supported types are:
    /// <see cref="Vector4"/> (XNA/MonoGame), <see cref="Quaternion"/> (XNA/MonoGame),
    /// <see cref="Vector4F"/> (DigitalRune), <see cref="Vector4D"/> (DigitalRune), 
    /// <see cref="QuaternionF"/> (DigitalRune) and <see cref="QuaternionD"/> (DigitalRune).
    /// </remarks>
    internal partial class Vector4Editor
    {
        // See Vector3Editor for more code comments.

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
            typeof(Vector4Editor),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, OnIsReadOnlyChanged));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, Boxed.Get(value)); }
        }


        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(object),
            typeof(Vector4Editor),
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
            typeof(Vector4Editor),
            new FrameworkPropertyMetadata(Boxed.DoubleZero, OnXyzwChanged));

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }


        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
            "Y",
            typeof(double),
            typeof(Vector4Editor),
            new PropertyMetadata(Boxed.DoubleZero, OnXyzwChanged));

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }


        public static readonly DependencyProperty ZProperty = DependencyProperty.Register(
            "Z",
            typeof(double),
            typeof(Vector4Editor),
            new PropertyMetadata(Boxed.DoubleZero, OnXyzwChanged));

        public double Z
        {
            get { return (double)GetValue(ZProperty); }
            set { SetValue(ZProperty, value); }
        }


        public static readonly DependencyProperty WProperty = DependencyProperty.Register(
            "W",
            typeof(double),
            typeof(Vector4Editor),
            new PropertyMetadata(Boxed.DoubleZero, OnXyzwChanged));

        public double W
        {
            get { return (double)GetValue(WProperty); }
            set { SetValue(WProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public Vector4Editor()
        {
            InitializeComponent();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnIsReadOnlyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (Vector4Editor)dependencyObject;
            target.Grid.IsEnabled = !target.IsReadOnly;
        }


        private static void OnVectorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (Vector4Editor)dependencyObject;
            target.OnValueChanged();
        }


        private void OnValueChanged()
        {
            if (_isUpdating)
                return;
            if (Value == null)
                return;

            _vectorType = Value.GetType();

            _isUpdating = true;
            try
            {
                if (_vectorType == typeof(Vector4))
                {
                    var v = (Vector4)Value;
                    X = Vector3Editor.SafeConvertToDouble(v.X);
                    Y = Vector3Editor.SafeConvertToDouble(v.Y);
                    Z = Vector3Editor.SafeConvertToDouble(v.Z);
                    W = Vector3Editor.SafeConvertToDouble(v.W);
                }
                else if (_vectorType == typeof(Quaternion))
                {
                    var v = (Quaternion)Value;
                    X = Vector3Editor.SafeConvertToDouble(v.X);
                    Y = Vector3Editor.SafeConvertToDouble(v.Y);
                    Z = Vector3Editor.SafeConvertToDouble(v.Z);
                    W = Vector3Editor.SafeConvertToDouble(v.W);
                }
                else if (_vectorType == typeof(Vector4F))
                {
                    var v = (Vector4F)Value;
                    X = Vector3Editor.SafeConvertToDouble(v.X);
                    Y = Vector3Editor.SafeConvertToDouble(v.Y);
                    Z = Vector3Editor.SafeConvertToDouble(v.Z);
                    W = Vector3Editor.SafeConvertToDouble(v.W);
                }
                else if (_vectorType == typeof(Vector4D))
                {
                    var v = (Vector4D)Value;
                    X = v.X;
                    Y = v.Y;
                    Z = v.Z;
                    W = v.W;
                }
                else if (_vectorType == typeof(QuaternionF))
                {
                    var v = (QuaternionF)Value;
                    X = Vector3Editor.SafeConvertToDouble(v.X);
                    Y = Vector3Editor.SafeConvertToDouble(v.Y);
                    Z = Vector3Editor.SafeConvertToDouble(v.Z);
                    W = Vector3Editor.SafeConvertToDouble(v.W);
                }
                else if (_vectorType == typeof(QuaternionD))
                {
                    var v = (QuaternionD)Value;
                    X = v.X;
                    Y = v.Y;
                    Z = v.Z;
                    W = v.W;
                }
                else
                {
                    if (Value != null)
                        throw new NotSupportedException("Vector4Editor.Value must be a Vector4, Quaternion, Vector4F, Vector4D, QuaternionF of QuaternionD.");

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


        private static void OnXyzwChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (Vector4Editor)dependencyObject;
            target.OnXyzwChanged();
        }


        private void OnXyzwChanged()
        {
            if (_isUpdating)
                return;

            _isUpdating = true;
            try
            {
                if (_vectorType == typeof(Vector4))
                    Value = new Vector4((float)X, (float)Y, (float)Z, (float)W);
                else if (_vectorType == typeof(Quaternion))
                    Value = new Quaternion((float)X, (float)Y, (float)Z, (float)W);
                else if (_vectorType == typeof(Vector4F))
                    Value = new Vector4F((float)X, (float)Y, (float)Z, (float)W);
                else if (_vectorType == typeof(Vector4D))
                    Value = new Vector4D(X, Y, Z, W);
                else if (_vectorType == typeof(QuaternionF))
                    Value = new QuaternionF((float)W, (float)X, (float)Y, (float)Z);
                else if (_vectorType == typeof(QuaternionD))
                    Value = new QuaternionD(W, X, Y, Z);
            }
            finally
            {
                _isUpdating = false;
            }
        }
        #endregion
    }
}
