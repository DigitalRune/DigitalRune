// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry
{
  /// <summary>
  /// A lightweight <see cref="IGeometricObject"/> implementation without events. (For internal use 
  /// only.)
  /// </summary>
  /// <remarks>
  /// This <see cref="IGeometricObject"/> implementation is used by collision algorithms to get a 
  /// temporary <see cref="IGeometricObject"/> instance for tests. Since, the events are disabled, 
  /// this class cannot be used for normal <see cref="CollisionObject"/>s. Not using events improves 
  /// performance drastically when geometric objects are exchanged.
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Shape = {Shape})")]
  internal sealed class TestGeometricObject : IGeometricObject, IRecyclable
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static readonly ResourcePool<TestGeometricObject> Pool =
      new ResourcePool<TestGeometricObject>(
        () => new TestGeometricObject(),
        null,
        null);
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    public Aabb Aabb
    {
      get
      {
        if (_aabbIsValid == false)
        {
          _aabb = Shape.GetAabb(Scale, Pose);
          _aabbIsValid = true;
        }

        return _aabb;
      }
    }
    private Aabb _aabb;
    private bool _aabbIsValid;
    

    public Pose Pose
    {
      get { return _pose; }
      set 
      { 
        _pose = value; 
        _aabbIsValid = false; 
      }
    }
    private Pose _pose = Pose.Identity;
    

    public Shape Shape
    {
      get { return _shape; }
      set 
      { 
        _shape = value; 
        _aabbIsValid = false; 
      }
    }
    private Shape _shape;


    public Vector3F Scale
    {
      get { return _scale; }
      set
      {
        _scale = value;
        _aabbIsValid = false;
      }
    }
    private Vector3F _scale = new Vector3F(1, 1, 1);


    // Events are not implemented.
    public event EventHandler<EventArgs> PoseChanged { add { } remove { } }
    public event EventHandler<ShapeChangedEventArgs> ShapeChanged { add { } remove { } }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    private TestGeometricObject()
    {
    }

    
    public static TestGeometricObject Create()
    {
      return Pool.Obtain();
    }


    public void Recycle()
    {
      Shape = null;
      Scale = Vector3F.One;
      Pose = Pose.Identity;
      Pool.Recycle(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    IGeometricObject IGeometricObject.Clone()
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
