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
    /// Represents a control that allows to edit a 2-dimensional vectors.
    /// </summary>
    /// <remarks>
    /// The property <see cref="Value"/> contains the 2-dimensional vector. Supported types are:
    /// <see cref="Vector2"/> (XNA/MonoGame), <see cref="Vector2F"/> (DigitalRune) and 
    /// <see cref="Vector2D"/> (DigitalRune).
    /// </remarks>
    internal partial class Vector2Editor
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
            typeof(Vector2Editor),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, OnIsReadOnlyChanged));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, Boxed.Get(value)); }
        }


        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(object),
            typeof(Vector2Editor),
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
            typeof(Vector2Editor),
            new FrameworkPropertyMetadata(Boxed.DoubleZero, OnXyzChanged));

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }


        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
            "Y",
            typeof(double),
            typeof(Vector2Editor),
            new PropertyMetadata(Boxed.DoubleZero, OnXyzChanged));

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public Vector2Editor()
        {
            InitializeComponent();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnIsReadOnlyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (Vector2Editor)dependencyObject;
            target.Grid.IsEnabled = !target.IsReadOnly;
        }


        private static void OnVectorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (Vector2Editor)dependencyObject;
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
                if (_vectorType == typeof(Vector2))
                {
                    var v = (Vector2)Value;
                    X = Vector3Editor.SafeConvertToDouble(v.X);
                    Y = Vector3Editor.SafeConvertToDouble(v.Y);
                }
                else if (_vectorType == typeof(Vector2F))
                {
                    var v = (Vector2F)Value;
                    X = Vector3Editor.SafeConvertToDouble(v.X);
                    Y = Vector3Editor.SafeConvertToDouble(v.Y);
                }
                else if (_vectorType == typeof(Vector2D))
                {
                    var v = (Vector2D)Value;
                    X = v.X;
                    Y = v.Y;
                }
                else
                {
                    if (Value != null)
                        throw new NotSupportedException("Vector2Editor.Value must be a Vector2, Vector2F or Vector2D.");

                    X = 0;
                    Y = 0;
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }


        private static void OnXyzChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (Vector2Editor)dependencyObject;
            target.OnXyzChanged();
        }


        private void OnXyzChanged()
        {
            if (_isUpdating)
                return;

            _isUpdating = true;
            try
            {
                if (_vectorType == typeof(Vector2))
                    Value = new Vector2((float)X, (float)Y);
                else if (_vectorType == typeof(Vector2F))
                    Value = new Vector2F((float)X, (float)Y);
                else if (_vectorType == typeof(Vector2D))
                    Value = new Vector2D(X, Y);
            }
            finally
            {
                _isUpdating = false;
            }
        }
        #endregion
    }
}
