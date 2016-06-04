// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Windows;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Represents a control that allows to edit a pose (position + orientation).
    /// </summary>
    /// <remarks>
    /// The property <see cref="Value"/> contains the pose. Supported types are:
    /// <see cref="Pose"/> and <see cref="PoseD"/>.
    /// </remarks>
    internal partial class PoseEditor
    {
        // See Vector3Editor for more code comments.

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private bool _isUpdating;
        private Type _valueType;
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
            typeof(PoseEditor),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, OnIsReadOnlyChanged));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, Boxed.Get(value)); }
        }


        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(object),
            typeof(PoseEditor),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged));

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        public static readonly DependencyProperty XProperty = DependencyProperty.Register(
            "X",
            typeof(double),
            typeof(PoseEditor),
            new FrameworkPropertyMetadata(Boxed.DoubleZero, OnComponentChanged));

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }


        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
            "Y",
            typeof(double),
            typeof(PoseEditor),
            new PropertyMetadata(Boxed.DoubleZero, OnComponentChanged));

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }


        public static readonly DependencyProperty ZProperty = DependencyProperty.Register(
            "Z",
            typeof(double),
            typeof(PoseEditor),
            new PropertyMetadata(Boxed.DoubleZero, OnComponentChanged));

        public double Z
        {
            get { return (double)GetValue(ZProperty); }
            set { SetValue(ZProperty, value); }
        }


        public static readonly DependencyProperty M00Property = DependencyProperty.Register(
            "M00",
            typeof(double),
            typeof(PoseEditor),
            new PropertyMetadata(Boxed.DoubleZero, OnComponentChanged));

        public double M00
        {
            get { return (double)GetValue(M00Property); }
            set { SetValue(M00Property, value); }
        }


        public static readonly DependencyProperty M01Property = DependencyProperty.Register(
            "M01",
            typeof(double),
            typeof(PoseEditor),
            new PropertyMetadata(Boxed.DoubleZero, OnComponentChanged));

        public double M01
        {
            get { return (double)GetValue(M01Property); }
            set { SetValue(M01Property, value); }
        }


        public static readonly DependencyProperty M02Property = DependencyProperty.Register(
            "M02",
            typeof(double),
            typeof(PoseEditor),
            new PropertyMetadata(Boxed.DoubleZero, OnComponentChanged));

        public double M02
        {
            get { return (double)GetValue(M02Property); }
            set { SetValue(M02Property, value); }
        }


        public static readonly DependencyProperty M10Property = DependencyProperty.Register(
            "M10",
            typeof(double),
            typeof(PoseEditor),
            new PropertyMetadata(Boxed.DoubleZero, OnComponentChanged));

        public double M10
        {
            get { return (double)GetValue(M10Property); }
            set { SetValue(M10Property, value); }
        }


        public static readonly DependencyProperty M11Property = DependencyProperty.Register(
            "M11",
            typeof(double),
            typeof(PoseEditor),
            new PropertyMetadata(Boxed.DoubleZero, OnComponentChanged));

        public double M11
        {
            get { return (double)GetValue(M11Property); }
            set { SetValue(M11Property, value); }
        }


        public static readonly DependencyProperty M12Property = DependencyProperty.Register(
            "M12",
            typeof(double),
            typeof(PoseEditor),
            new PropertyMetadata(Boxed.DoubleZero, OnComponentChanged));

        public double M12
        {
            get { return (double)GetValue(M12Property); }
            set { SetValue(M12Property, value); }
        }


        public static readonly DependencyProperty M20Property = DependencyProperty.Register(
            "M20",
            typeof(double),
            typeof(PoseEditor),
            new PropertyMetadata(Boxed.DoubleZero, OnComponentChanged));

        public double M20
        {
            get { return (double)GetValue(M20Property); }
            set { SetValue(M20Property, value); }
        }


        public static readonly DependencyProperty M21Property = DependencyProperty.Register(
            "M21",
            typeof(double),
            typeof(PoseEditor),
            new PropertyMetadata(Boxed.DoubleZero, OnComponentChanged));

        public double M21
        {
            get { return (double)GetValue(M21Property); }
            set { SetValue(M21Property, value); }
        }


        public static readonly DependencyProperty M22Property = DependencyProperty.Register(
            "M22",
            typeof(double),
            typeof(PoseEditor),
            new PropertyMetadata(Boxed.DoubleZero, OnComponentChanged));

        public double M22
        {
            get { return (double)GetValue(M22Property); }
            set { SetValue(M22Property, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public PoseEditor()
        {
            InitializeComponent();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnIsReadOnlyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (PoseEditor)dependencyObject;
            target.Grid.IsEnabled = !target.IsReadOnly;
        }


        private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (PoseEditor)dependencyObject;
            target.OnValueChanged();
        }


        private void OnValueChanged()
        {
            if (_isUpdating)
                return;
            if (Value == null)
                return;

            _valueType = Value.GetType();

            _isUpdating = true;
            try
            {
                if (_valueType == typeof(Pose))
                {
                    var p = (Pose)Value;
                    X = Vector3Editor.SafeConvertToDouble(p.Position.X);
                    Y = Vector3Editor.SafeConvertToDouble(p.Position.Y);
                    Z = Vector3Editor.SafeConvertToDouble(p.Position.Z);
                    M00 = Vector3Editor.SafeConvertToDouble(p.Orientation.M00);
                    M01 = Vector3Editor.SafeConvertToDouble(p.Orientation.M01);
                    M02 = Vector3Editor.SafeConvertToDouble(p.Orientation.M02);
                    M10 = Vector3Editor.SafeConvertToDouble(p.Orientation.M10);
                    M11 = Vector3Editor.SafeConvertToDouble(p.Orientation.M11);
                    M12 = Vector3Editor.SafeConvertToDouble(p.Orientation.M12);
                    M20 = Vector3Editor.SafeConvertToDouble(p.Orientation.M20);
                    M21 = Vector3Editor.SafeConvertToDouble(p.Orientation.M21);
                    M22 = Vector3Editor.SafeConvertToDouble(p.Orientation.M22);
                }
                else if (_valueType == typeof(PoseD))
                {
                    var p = (PoseD)Value;
                    X = p.Position.X;
                    Y = p.Position.Y;
                    Z = p.Position.Z;
                    M00 = p.Orientation.M00;
                    M01 = p.Orientation.M01;
                    M02 = p.Orientation.M02;
                    M10 = p.Orientation.M10;
                    M11 = p.Orientation.M11;
                    M12 = p.Orientation.M12;
                    M20 = p.Orientation.M20;
                    M21 = p.Orientation.M21;
                    M22 = p.Orientation.M22;
                }
                else
                {
                    if (Value != null)
                        throw new NotSupportedException("PoseEditor.Value must be a Pose or a PoseD.");

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


        private static void OnComponentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (PoseEditor)dependencyObject;
            target.OnComponentChanged();
        }


        private void OnComponentChanged()
        {
            if (_isUpdating)
                return;

            _isUpdating = true;
            try
            {
                if (_valueType == typeof(Pose))
                {
                    Value = new Pose(new Vector3F((float)X, (float)Y, (float)Z),
                                     new Matrix33F((float)M00, (float)M01, (float)M02,
                                                   (float)M10, (float)M11, (float)M12,
                                                   (float)M20, (float)M21, (float)M22));
                }
                else if (_valueType == typeof(PoseD))
                {
                    Value = new PoseD(new Vector3D(X, Y, Z),
                                      new Matrix33D(M00, M01, M02,
                                                    M10, M11, M12,
                                                    M20, M21, M22));

                }
            }
            finally
            {
                _isUpdating = false;
            }
        }
        #endregion
    }
}
