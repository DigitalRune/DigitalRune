// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a height field which can be used for simple terrains.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A height field is defined by a 1-dimensional array (<see cref="Samples"/>) which contains
  /// <see cref="NumberOfSamplesX"/> x <see cref="NumberOfSamplesZ"/> height values.
  /// This array is triangulated. Each array element defines a triangle vertex.
  /// The height samples are addressed like this: 
  /// </para>
  /// <para>
  /// <c>height(indexX, indexZ) = Samples[indexZ * NumberOfSamplesX + indexX]</c>
  /// </para>
  /// <para>
  /// The height field is placed in the local x/z plane where the up direction is the positive
  /// y-axis. The top-left height field corner (<c>Samples[0]</c>) is positioned at
  /// x = <see cref="OriginX"/> and z = <see cref="OriginZ"/>.
  /// </para>
  /// <para>
  /// The height field is stretched to fill a rectangle <see cref="WidthX"/> x <see cref="WidthZ"/>.
  /// </para>
  /// <para>
  /// Holes in the height field can be created by setting a height value in the <see cref="Samples"/>
  /// to <see cref="float.NaN"/>. All triangles touching the "hole" vertex will be removed. This
  /// also means that the shape of the hole depends on the current tessellation pattern.
  /// If all elements in the <see cref="Samples"/> are <see cref="float.NaN"/>, then the behavior
  /// of the height field is undefined.
  /// </para>
  /// <para>
  /// Limitations of holes and collision detection:
  /// Please note that computation of <see cref="CollisionQueryType.ClosestPoints"/>, might not work
  /// as expected if an object moves through a hole and under the height field. Also computation
  /// of contacts might not work as expected on the border triangles near the hole. For good
  /// collision detection results it is recommended to avoid closest points queries for objects
  /// under the height field and to always surround a hole with other collision objects.
  /// For instance if there is a cave in a game terrain, make the hole larger than the cave entrance
  /// and surround it with additional rock meshes.
  /// </para>
  /// <para>
  /// <strong>Shape Features:</strong> If a <see cref="HeightField"/> is involved in a
  /// <see cref="Contact"/> the shape feature property (<see cref="Contact.FeatureA"/> or
  /// <see cref="Contact.FeatureB"/>) contains the index of the triangle that creates the
  /// <see cref="Contact"/>: Each cell consists of two triangles. The first cell contains the
  /// triangles 0 and 1. The next cell in positive x direction contains the triangles 2 and 3. And
  /// so on. To compute an index all cells of a row in positive x direction are enumerated then the
  /// next x-row is enumerated and so on.
  /// </para>
  /// </remarks>
  /// <example>
  /// <para>
  /// Here is an example (pseudo-code):
  /// </para>
  /// <para>
  /// <c>HeightField { Samples=float[200], OriginX=3000, OriginY =4000, WidthX=100, WidthZ=200 }</c>
  /// </para>
  /// <para>
  /// This creates a height field in the x/z plane. The field is 100 units wide in the x-axis and
  /// 200 units wide in the z-axis. The array element [0] defines the height field height at
  /// x = 3000, z = 4000. The array element [199] defines the height field height at
  /// x = 3100, z = 4200
  /// </para>
  /// <para>
  /// The <see cref="HeightField"/> must contain at least 2 x 2 elements.
  /// </para>
  /// <para>
  /// For best performance, <see cref="IGeometricObject"/>s that have a <see cref="HeightField"/>
  /// shape should use only 90 degree rotations in their <see cref="Pose"/>. The local x/z plane of
  /// the height field should be normal to a world space axis plane.
  /// </para>
  /// <para>
  /// For a height field the collision behavior is only defined in the x/z range of the height
  /// field. Beyond the height field limits the collision behavior is undefined. For example, if a
  /// box is left of the height field and moves sideways into the height field, the closest-point
  /// and contact information might not be intuitive and such cases should be avoided.
  /// </para>
  /// </example>
  public class HeightField : Shape
  {
    // Notes:
    // Holes are represented using NaN in the Array. Calculations must handle
    // NaN values correctly.

    // TODO:
    // - Allow different height field array variants (float, [][], int, IEnumerable, ...)
    // - Allow different triangulation variants (left diagonal, right diagonal, diamond)
    // - We could add a "thickness" to the AABB to catch all deep interpenetrations with AABB tests.
    // - Add properties BaseIndex and RowStride (StrideX, Stride) to allow to use a
    //   region of a larger 1-dimensional array.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The cached difference of the highest and lowest height field point.
    private float _minHeight = float.NaN;
    private float _maxHeight = float.NaN;

    // Array with normals for the height entries. - Used when UseFastCollisionApproximation is set.
    //internal Vector3F[,] NormalArray = null;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the height field.
    /// </summary>
    /// <value>
    /// The height field. The default height field is 2 x 2 height field where the heights are 0.
    /// </value>
    /// <remarks>
    /// <para>
    /// This 1-dimensional array contains the height samples of the rectangular height field.
    /// The array must contain at least <c>NumberOfSamplesX * NumberSamplesZ</c> elements.
    /// The elements are addressed like this: <c>height(indexX, indexZ) = Samples[indexZ * NumberOfSamplesX + indexX]</c>.
    /// </para>
    /// <para>
    /// Use <see cref="SetSamples"/> to change this property.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public float[] Samples
    {
      get { return _samples; }
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
    private float[] _samples;


    /// <summary>
    /// Gets the number of samples per row (= along the x-axis).
    /// </summary>
    /// <value>The number of samples per row (= along the x-axis).</value>
    /// <remarks>
    /// Use <see cref="SetSamples"/> to change this property.
    /// </remarks>
    public int NumberOfSamplesX
    {
      get { return _numberOfSamplesX; }
    }
    private int _numberOfSamplesX;


    /// <summary>
    /// Gets the number of samples per column (= along the z-axis).
    /// </summary>
    /// <value>The number of samples per column (= along the z-axis).</value>
    /// <remarks>
    /// Use <see cref="SetSamples"/> to change this property.
    /// </remarks>
    public int NumberOfSamplesZ
    {
      get { return _numberOfSamplesZ; }
    }
    private int _numberOfSamplesZ;


    /// <summary>
    /// Gets or sets the height field.
    /// </summary>
    /// <value>
    /// The height field. The default height field is 2 x 2 height field where the heights are 0.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> contains less than 2 x 2 elements.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    [Obsolete("Property Array is obsolete. Use the properties Samples, NumberOfSamplesX and NumberOfSamplesZ instead.")]
    public float[,] Array
    {
      get { return _array; }
      set
      {
        if (value == null)
        {
          // Delete _array. _samples is kept because we do not support empty height fields.
          _array = null;
          return;
        }

        if (value.GetLength(0) < 2 || value.GetLength(1) < 2)
          throw new ArgumentException("The HeightField array must contain at least 2 x 2 elements.");

        if (_array != value)
        {
          _array = value;
          CopyArrayToSamples();
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
    private float[,] _array;


    /// <summary>
    /// Gets or sets the depth of the height field.
    /// </summary>
    /// <value>The depth. The default value is 100.</value>
    /// <remarks>
    /// <para>
    /// This value defines the depth of each height field cell. This value is relevant for detecting
    /// collisions which happen below the height field surface. Penetrations with the height field
    /// deeper than this value are ignored.
    /// </para>
    /// </remarks>
    public float Depth
    {
      get { return _depth; }
      set
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_depth != value)
        {
          _depth = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _depth = 100;


    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>An inner point.</value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space).
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get
      {
        // Return a point in the center below the surface.
        float centerX = _originX + _widthX / 2;
        float centerZ = _originZ + _widthZ / 2;
        Vector3F p = new Vector3F(centerX, GetHeight(centerX, centerZ) - _depth / 2, centerZ);

        if (!Numeric.IsNaN(p.Y))
          return p;

        // The chosen point is a hole. --> Choose any other non-hole point instead.
        for (int z = 0; z < _numberOfSamplesZ; z++)
        {
          for (int x = 0; x < _numberOfSamplesX; x++)
          {
            float height = _samples[z * _numberOfSamplesX + x];
            if (Numeric.IsNaN(height))
              continue;

            return new Vector3F(
              _originX + x * _widthX / (_numberOfSamplesX - 1),
              height - _depth / 2,
              _originZ + z * _widthZ / (_numberOfSamplesZ - 1));
          }
        }

        // There are no non-hole vertices? Return dummy value.
        return Vector3F.Zero;
      }
    }


    /// <summary>
    /// Gets or sets a value indicating whether the collision detection should use a fast, less
    /// accurate method.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the collision detection should use a fast, less accurate method; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If this flag is set, the collision detection uses a new, very fast algorithm for computing
    /// collisions between the height field and other objects. This algorithm should only be used
    /// if the height field is smooth.
    /// </para>
    /// </remarks>
    public bool UseFastCollisionApproximation { get; set; }


    /// <summary>
    /// Gets or sets height field origin along the x-axis.
    /// </summary>
    /// <value>The height field origin along the x-axis. The default value is 0.</value>
    /// <remarks>
    /// The height field origin determines the position of the top-left height field corner
    /// (<c>Samples[0]</c>).
    /// </remarks>
    public float OriginX
    {
      get { return _originX; }
      set
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_originX != value)
        {
          _originX = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _originX;


    /// <summary>
    /// Gets or sets height field origin along the z-axis.
    /// </summary>
    /// <value>The height field origin along the z-axis. The default value is 0.</value>
    /// <inheritdoc cref="OriginX"/>
    public float OriginZ
    {
      get { return _originZ; }
      set
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_originZ != value)
        {
          _originZ = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _originZ;


    /// <summary>
    /// Gets or sets the width along the local x-axis.
    /// </summary>
    /// <value>The first width. The default value is 1000.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public float WidthX
    {
      get { return _widthX; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "The Width must be greater than zero.");

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_widthX != value)
        {
          _widthX = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _widthX = 1000;


    /// <summary>
    /// Gets or sets the width along the local z-axis.
    /// </summary>
    /// <value>The second width. The default value is 1000.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public float WidthZ
    {
      get { return _widthZ; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "The Width must be greater than zero.");

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_widthZ != value)
        {
          _widthZ = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _widthZ = 1000;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="HeightField"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="HeightField"/> class.
    /// </summary>
    public HeightField()
    {
      _samples = new float[] { 0, 0, 0, 0 };
      _numberOfSamplesX = 2;
      _numberOfSamplesZ = 2;
      _depth = 100;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="HeightField"/> class from the given array.
    /// </summary>
    /// <param name="widthX">The width along the x-axis.</param>
    /// <param name="widthZ">The width along the z-axis.</param>
    /// <param name="array">The array.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="array"/> contains less than 2 x 2 elements.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="widthX"/> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="widthZ"/> is negative or 0.
    /// </exception>
    [Obsolete("Setting the height field with a 2-dimensional array is obsolete. Use a 1-dimensional array instead.")]
    public HeightField(float widthX, float widthZ, float[,] array)
    {
      if (widthX <= 0)
        throw new ArgumentOutOfRangeException("widthX", "The Width must be greater than zero.");
      if (widthZ <= 0)
        throw new ArgumentOutOfRangeException("widthZ", "The Width must be greater than zero.");
      if (array == null)
        throw new ArgumentNullException("array");
      if (array.GetLength(0) < 2 || array.GetLength(1) < 2)
        throw new ArgumentException("The HeightField array must contain at least 2 x 2 elements.");

      _widthX = widthX;
      _widthZ = widthZ;
      _array = array;
      CopyArrayToSamples();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="HeightField" /> class from the given array.
    /// </summary>
    /// <param name="originX">The origin along the x-axis.</param>
    /// <param name="originZ">The origin along the z-axis.</param>
    /// <param name="widthX">The width along the x-axis.</param>
    /// <param name="widthZ">The width along the z-axis.</param>
    /// <param name="samples">The height samples (see <see cref="Samples" />).</param>
    /// <param name="numberOfSamplesX">
    /// The number of samples along the x-axis. (Must be at least 2.)
    /// </param>
    /// <param name="numberOfSamplesZ">
    /// The number of samples along the z-axis. (Must be at least 2.)
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="widthX" /> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="widthZ" /> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="samples" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfSamplesX" /> or <paramref name="numberOfSamplesZ"/> is less than 2.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The height samples array must contain at least 
    /// <paramref name="numberOfSamplesX"/> x <paramref name="numberOfSamplesZ"/> elements.
    /// </exception>
    public HeightField(float originX, float originZ, float widthX, float widthZ, float[] samples, int numberOfSamplesX, int numberOfSamplesZ)
    {
      if (widthX <= 0)
        throw new ArgumentOutOfRangeException("widthX", "The Width must be greater than zero.");
      if (widthZ <= 0)
        throw new ArgumentOutOfRangeException("widthZ", "The Width must be greater than zero.");
      if (samples == null)
        throw new ArgumentNullException("samples");
      if (numberOfSamplesX < 2)
        throw new ArgumentOutOfRangeException("numberOfSamplesX", "The number of samples in each direction must be 2 or greater.");
      if (numberOfSamplesZ < 2)
        throw new ArgumentOutOfRangeException("numberOfSamplesZ", "The number of samples in each direction must be 2 or greater.");
      if (samples.Length < numberOfSamplesX * numberOfSamplesZ)
        throw new ArgumentException("The height samples array must contain at least numberOfSamplesX * numberOfSamplesZ elements.");

      _originX = originX;
      _originZ = originZ;
      _widthX = widthX;
      _widthZ = widthZ;
      _samples = samples;
      _numberOfSamplesX = numberOfSamplesX;
      _numberOfSamplesZ = numberOfSamplesZ;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new HeightField();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (HeightField)sourceShape;

      _samples = (float[])source.Samples.Clone();
      if (source._array != null)
        _array = (float[,])source._array.Clone();
      _numberOfSamplesX = source._numberOfSamplesX;
      _numberOfSamplesZ = source._numberOfSamplesZ;
      _depth = source.Depth;
      _originX = source.OriginX;
      _originZ = source.OriginZ;
      _widthX = source.WidthX;
      _widthZ = source.WidthZ;
    }
    #endregion


    /// <summary>
    /// Sets the array of height samples.
    /// </summary>
    /// <param name="samples">The height samples (see <see cref="Samples"/>).</param>
    /// <param name="numberOfSamplesX">
    /// The number of samples along the x-axis. (Must be at least 2.)
    /// </param>
    /// <param name="numberOfSamplesZ">
    /// The number of samples along the z-axis. (Must be at least 2.)
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="samples" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfSamplesX" /> or <paramref name="numberOfSamplesZ"/> is less than 2.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The height samples array must contain at least
    /// <paramref name="numberOfSamplesX"/> x <paramref name="numberOfSamplesZ"/> elements.
    /// </exception>
    public void SetSamples(float[] samples, int numberOfSamplesX, int numberOfSamplesZ)
    {
      if (samples == null)
        throw new ArgumentNullException("samples");
      if (numberOfSamplesX < 2)
        throw new ArgumentOutOfRangeException("numberOfSamplesX", "The number of samples in each direction must be 2 or greater.");
      if (numberOfSamplesZ < 2)
        throw new ArgumentOutOfRangeException("numberOfSamplesZ", "The number of samples in each direction must be 2 or greater.");
      if (samples.Length < numberOfSamplesX * numberOfSamplesZ)
        throw new ArgumentException("The height samples array must contain at least numberOfSamplesX * numberOfSamplesZ elements.");

      _samples = samples;
      _numberOfSamplesX = numberOfSamplesX;
      _numberOfSamplesZ = numberOfSamplesZ;

      OnChanged(ShapeChangedEventArgs.Empty);
    }


    private void CopyArrayToSamples()
    {
      _numberOfSamplesX = _array.GetLength(0);
      _numberOfSamplesZ = _array.GetLength(1);

      int length = _numberOfSamplesX * _numberOfSamplesZ;
      if (_samples == null || _samples.Length != length)
        _samples = new float[length];

      for (int z = 0; z < _numberOfSamplesZ; z++)
        for (int x = 0; x < _numberOfSamplesX; x++)
          _samples[z * _numberOfSamplesX + x] = _array[x, z];
    }



    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      // Recompute local cached AABB if it is invalid.
      if (Numeric.IsNaN(_minHeight))
      {
        // Find min and max height. 
        // TODO: We could cache that beforehand.
        _minHeight = float.PositiveInfinity;
        _maxHeight = float.NegativeInfinity;
        foreach (float height in _samples)
        {
          if (height < _minHeight)
            _minHeight = height;
          if (height > _maxHeight)
            _maxHeight = height;
        }
      }

      Vector3F minimum = new Vector3F(_originX, _minHeight, _originZ);
      Vector3F maximum = new Vector3F(_originX + _widthX, _maxHeight, _originZ + _widthZ);

      // Apply scale.
      var scaledLocalAabb = new Aabb(minimum, maximum);
      scaledLocalAabb.Scale(scale);

      // Add depth after scaling because scaleY = 0 makes sense to flatten the height field
      // but the bounding box should have a height > 0 to avoid tunneling.
      scaledLocalAabb.Minimum.Y = Math.Min(scaledLocalAabb.Minimum.Y - _depth, -_depth);

      // Apply pose.
      return scaledLocalAabb.GetAabb(pose);
    }


    /// <summary>
    /// Gets the height for a height field coordinate.
    /// </summary>
    /// <param name="x">
    /// The x coordinate that lies in the interval [0, <see cref="WidthX"/>].
    /// </param>
    /// <param name="z">
    /// The z coordinate that lies in the interval [0, <see cref="WidthZ"/>].
    /// </param>
    /// <returns>The interpolated height for the given coordinates.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float GetHeight(float x, float z)
    {
      // Subtract origin.
      var xo = x - _originX;
      var zo = z - _originZ;

      if (xo < 0 || xo > _widthX || zo < 0 || zo > _widthZ)
        return float.NaN;

      // Compute cell indices.
      float cellWidthX = _widthX / (_numberOfSamplesX - 1);
      float cellWidthZ = _widthZ / (_numberOfSamplesZ - 1);
      int indexX = Math.Min((int)(xo / cellWidthX), _numberOfSamplesX - 2);
      int indexZ = Math.Min((int)(zo / cellWidthZ), _numberOfSamplesZ - 2);

      // Determine which triangle we need.
      float xRelative = xo / cellWidthX - indexX;
      float zRelative = zo / cellWidthZ - indexZ;
      Debug.Assert(Numeric.IsGreaterOrEqual(xRelative, 0) && Numeric.IsLessOrEqual(xRelative, 1));
      Debug.Assert(Numeric.IsGreaterOrEqual(zRelative, 0) && Numeric.IsLessOrEqual(zRelative, 1));
      bool useSecondTriangle = (xRelative + zRelative) > 1;   // The diagonal is where xRel + zRel == 1.

      // Get correct triangle for point.
      Triangle triangle = GetTriangle(indexX, indexZ, useSecondTriangle);

      // Store heights of the triangle vertices.
      float height0 = triangle.Vertex0.Y;
      float height1 = triangle.Vertex1.Y;
      float height2 = triangle.Vertex2.Y;

      // Get barycentric coordinates (relative to triangle in xz plane).
      float u, v, w;

      // Project triangle into xz plane.
      triangle.Vertex0.Y = 0;
      triangle.Vertex1.Y = 0;
      triangle.Vertex2.Y = 0;
      GeometryHelper.GetBarycentricFromPoint(triangle, new Vector3F(x, 0, z), out u, out v, out w);

      Debug.Assert((Numeric.IsGreaterOrEqual(u, 0) && Numeric.IsGreaterOrEqual(v, 0)) && Numeric.IsLessOrEqual(u + v, 1));

      // Return height (computed with barycentric coordinates).
      return u * height0 + v * height1 + w * height2;
    }


    /// <summary>
    /// Gets a triangle representing a part of a height field cell.
    /// </summary>
    /// <param name="indexX">The cell index along <see cref="HeightField.WidthX"/>.</param>
    /// <param name="indexZ">The cell index along <see cref="HeightField.WidthZ"/>.</param>
    /// <param name="second">
    /// If set to <see langword="true"/> the triangle for the second cell half is returned; 
    /// otherwise the triangle for the first cell half is returned.
    /// </param>
    /// <returns>A triangle.</returns>
    /// <remarks>
    /// <para>
    /// Each cell of the height field array is approximated with 2 triangles. 
    /// <paramref name="second"/> decides which triangle is returned. 
    /// </para>
    /// <para>
    /// The cell indices start at (0, 0) for the first cell. The last cell has the indices 
    /// (m-2, n-2) when the height field array is a float[m, n] array.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="indexX"/> is out of range.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="indexZ"/> is out of range.
    /// </exception>
    public Triangle GetTriangle(int indexX, int indexZ, bool second)
    {
      if (indexX < 0 || indexX + 1 >= _numberOfSamplesX)
        throw new ArgumentOutOfRangeException("indexX");
      if (indexZ < 0 || indexZ + 1 >= _numberOfSamplesZ)
        throw new ArgumentOutOfRangeException("indexZ");

      Triangle triangle = new Triangle();

      float cellWidthX = _widthX / (_numberOfSamplesX - 1);
      float cellWidthZ = _widthZ / (_numberOfSamplesZ - 1);

      // Upper left coordinate in 2D height field.
      float x = _originX + cellWidthX * indexX;
      float z = _originZ + cellWidthZ * indexZ;

      if (second == false)
      {
        // ----- Build first triangle. 

        // Get 3 heights.
        float height0 = _samples[indexZ * _numberOfSamplesX + indexX];
        float height1 = _samples[indexZ * _numberOfSamplesX + (indexX + 1)];
        float height2 = _samples[(indexZ + 1) * _numberOfSamplesX + indexX];

        // Build vertices.
        // Note: If this order is changed, methods here and in HeightFieldAlgorithm (neighbor selection) must be updated.
        triangle.Vertex0 = new Vector3F(x, height0, z);
        triangle.Vertex2 = new Vector3F(x + cellWidthX, height1, z);  // Vertex 1 and 2 swapped for correct counter clockwise order!
        triangle.Vertex1 = new Vector3F(x, height2, z + cellWidthZ);
      }
      else
      {
        // ----- Build second triangle. 

        // Get 3 heights.
        float height0 = _samples[(indexZ + 1) * _numberOfSamplesX + indexX];
        float height1 = _samples[indexZ * _numberOfSamplesX + (indexX + 1)];
        float height2 = _samples[(indexZ + 1) * _numberOfSamplesX + (indexX + 1)];

        // Build vertices.
        // Note: If this order is changed, methods here and in HeightFieldAlgorithm (neighbor selection) 
        // and RayHeightFieldAlgorithm must be updated.
        triangle.Vertex0 = new Vector3F(x, height0, z + cellWidthZ);
        triangle.Vertex2 = new Vector3F(x + cellWidthX, height1, z); // Vertex 1 and 2 swapped for correct counter clockwise order!
        triangle.Vertex1 = new Vector3F(x + cellWidthX, height2, z + cellWidthZ);
      }
      return triangle;
    }


    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used</param>
    /// <returns>Positive infinity (<see cref="float.PositiveInfinity"/>)</returns>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      return float.PositiveInfinity;
    }


    /* Not used - see HeightFieldAlgorithm_Fast.cs.
    /// <summary>
    /// Initializes the normal vector array.
    /// </summary>
    internal void InitializeNormalArray()
    {
      int arrayLengthX = Array.GetLength(0);
      int arrayLengthZ = Array.GetLength(1);

      NormalArray = new Vector3F[arrayLengthX, arrayLengthZ];
      for (int x = 0; x < arrayLengthX; x++)
      {
        for (int z = 0; z < arrayLengthZ; z++)
        {
          // Normal computation as described and derived in 
          // Game Programming Gems 3 - Fast Heightfield normal calculation.

          // Get four neighbors.
          float h1 = Array[Math.Min(x + 1, arrayLengthX - 1), z];
          float h2 = Array[x, Math.Min(z + 1, arrayLengthZ - 1)];
          float h3 = Array[Math.Max(x - 1, 0), z];
          float h4 = Array[x, Math.Max(z - 1, 0)];

          // This could be used for very smooth surfaces.
          //NormalArray[x, z] = new Vector3F(
          //  h3 - h1,
          //  2,
          //  h4 - h2).Normalized;

          // This is more accurate.
          //NormalArray[x, z] = (new Vector3F(-h1, 1, -h2).Normalized
          //                     + new Vector3F(h3, 1, -h2).Normalized
          //                     + new Vector3F(h3, 1, h4).Normalized
          //                     + new Vector3F(-h1, 1, h4).Normalized
          //                    ).Normalized;

          // Or, use the neighbor triangles to compute the normals - 
          // the advantage of this method is that the normals fit the visible
          // triangle mesh.
          var normal = Vector3F.Zero;
          if (x - 1 >= 0)
          {
            if (z - 1 >= 0)
            {
              normal += GetTriangle(x - 1, z - 1, true).Normal;
            }
            if (z + 1 < arrayLengthZ)
            {
              normal += (GetTriangle(x - 1, z, false).Normal 
                         + GetTriangle(x - 1, z, true).Normal).Normalized;
            }
          }

          if (x + 1 < arrayLengthX)
          {
            if (z - 1 >= 0)
            {
              normal += (GetTriangle(x, z - 1, false).Normal
                         + GetTriangle(x, z - 1, true).Normal).Normalized;
            }
            if (z + 1 < arrayLengthZ)
            {
              normal += GetTriangle(x, z, false).Normal;
            }
          }
          normal.Normalize();
          NormalArray[x, z] = normal;
        }
      }
    }*/


    /// <summary>
    /// Invalidates this height field.
    /// </summary>
    /// <remarks>
    /// This method must be called if the content of <see cref="Samples"/> was changed. 
    /// This method calls <see cref="OnChanged"/>.
    /// </remarks>
    public void Invalidate()
    {
      if (_array != null)
        CopyArrayToSamples();

      OnChanged(ShapeChangedEventArgs.Empty);
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      TriangleMesh mesh = new TriangleMesh();

      int numberOfCellsInX = _numberOfSamplesX - 1;
      int numberOfCellsInZ = _numberOfSamplesZ - 1;

      // Add vertex positions.
      for (int z = 0; z < _numberOfSamplesZ; z++)
      {
        for (int x = 0; x < _numberOfSamplesX; x++)
        {
          mesh.Vertices.Add(
            new Vector3F(
              _originX + (float)x / numberOfCellsInX * WidthX,
              _samples[z * NumberOfSamplesX + x],
              _originZ + (float)z / numberOfCellsInZ * WidthZ));
        }
      }

      // Add triangle indices.
      for (int z = 0; z < numberOfCellsInZ; z++)
      {
        for (int x = 0; x < numberOfCellsInX; x++)
        {
          // Check for holes.
          // Shared vertices of both triangles.
          var height = _samples[(z + 1) * _numberOfSamplesX + x] * _samples[z * _numberOfSamplesX + (x + 1)];
          if (!Numeric.IsFinite(height))
            continue;

          // First triangle.
          if (Numeric.IsFinite(_samples[z * _numberOfSamplesX + x]))
          {
            mesh.Indices.Add(z * _numberOfSamplesX + x);
            mesh.Indices.Add((z + 1) * _numberOfSamplesX + x);
            mesh.Indices.Add(z * _numberOfSamplesX + x + 1);
          }

          // Second triangle.
          if (Numeric.IsFinite(_samples[(z + 1) * _numberOfSamplesX + (x + 1)]))
          {
            mesh.Indices.Add(z * _numberOfSamplesX + x + 1);
            mesh.Indices.Add((z + 1) * _numberOfSamplesX + x);
            mesh.Indices.Add((z + 1) * _numberOfSamplesX + x + 1);
          }
        }
      }
      return mesh;
    }


    /// <inheritdoc/>
    protected override void OnChanged(ShapeChangedEventArgs eventArgs)
    {
      // Set cached AABB to "invalid".
      _minHeight = float.NaN;
      _maxHeight = float.NaN;

      // Cached normals are also invalid.
      //NormalArray = null;

      base.OnChanged(eventArgs);
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(
        CultureInfo.InvariantCulture,
        "HeightField {{ OriginX = {0}, OriginZ = {1}, WidthX = {2}, WidthZ = {3} }}",
        _originX,
        _originZ,
        _widthX,
        _widthZ);
    }
    #endregion
  }
}
