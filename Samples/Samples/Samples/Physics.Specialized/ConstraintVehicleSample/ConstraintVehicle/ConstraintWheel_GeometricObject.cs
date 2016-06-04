// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Specialized
{
  partial class ConstraintWheel
  {
    // Here we implement the IGeometricObject interface.

    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    Aabb IGeometricObject.Aabb
    {
      get
      {
        if (_aabbIsValid == false)
        {
          _aabb = _ray.GetAabb(_rayPose);
          _aabbIsValid = true;
        }
        return _aabb;
      }
    }
    private Aabb _aabb;
    private bool _aabbIsValid;


    Pose IGeometricObject.Pose 
    {
      get { return _rayPose; }
    }    


    Shape IGeometricObject.Shape
    {
      get { return _ray; }
    }


    Vector3F IGeometricObject.Scale
    {
      get { return Vector3F.One; }
    }


    event EventHandler<EventArgs> IGeometricObject.PoseChanged
    {
      add { _poseChanged += value; }
      remove { _poseChanged -= value; }
    }
    private event EventHandler<EventArgs> _poseChanged;


    event EventHandler<ShapeChangedEventArgs> IGeometricObject.ShapeChanged
    {
      add { _shapeChanged += value; }
      remove { _shapeChanged -= value; }
    }
    private event EventHandler<ShapeChangedEventArgs> _shapeChanged;
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    IGeometricObject IGeometricObject.Clone()
    {
      throw new NotImplementedException();
    }


    private void OnPoseChanged(EventArgs eventArgs)
    {
      _aabbIsValid = false;

      var handler = _poseChanged;
      if (handler != null)
        handler(this, eventArgs);
    }


    private void OnShapeChanged(object sender, ShapeChangedEventArgs eventArgs)
    {
      _aabbIsValid = false;

      var handler = _shapeChanged;
      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
