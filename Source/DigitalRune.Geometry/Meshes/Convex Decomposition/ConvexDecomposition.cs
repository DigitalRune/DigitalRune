// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !NETFX_CORE && !WP7 && !XBOX
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DigitalRune.Threading;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Meshes
{
  /// <summary>
  /// Performs an approximate convex decomposition of an input mesh. (Not available on these
  /// platforms: Silverlight, Windows Phone 7, Xbox 360)
  /// </summary>
  /// <remarks>
  /// <para>
  /// An approximate convex decomposition takes a mesh and computes a composite shape of convex
  /// polyhedra (see <see cref="Decomposition"/>) that resembles the original mesh.
  /// </para>
  /// <para>
  /// To use this process: Create an instance of this class. Set decomposition parameters (like 
  /// <see cref="AllowedConcavity"/>, <see cref="VertexLimit"/>, etc.). Then start the process with 
  /// <see cref="Decompose"/> or <see cref="DecomposeAsync"/>. After the decomposition has finished
  /// the result is available in the property <see cref="Decomposition"/>.
  /// </para>
  /// <para>
  /// This class is not available in the Silverlight and the Xbox 360 compatible build of
  /// DigitalRune.Geometry.dll.
  /// </para>
  /// </remarks>
  public class ConvexDecomposition
  {
    // Notes:
    // This class implements Convex Decomposition as described in 
    // Game Programming Gems 8 with minor modifications.
    //
    // Other implementations of CD:
    // - John Ratcliff's CodeSuppository
    //   This recursively cuts the mesh in the middle of the longest AABB
    //   axis. The algorithm is faster but less optimal. 


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private ITriangleMesh _mesh;
    private bool _cancel;
    private List<CDIsland> _islands;
    private List<CDIslandLink> _links;

    private readonly object _syncRoot = new object();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the allowed concavity.
    /// </summary>
    /// <value>The allowed concavity; must be greater than 0.</value>
    /// <remarks>
    /// The default value is 0.1 which means the normal surface distance from a concave part to the 
    /// convex hull should not be larger than 0.1 units.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public float AllowedConcavity
    {
      get { return _allowedConcavity; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "AllowedConcavity must be greater than 0.");
        _allowedConcavity = value;
      }
    }
    private float _allowedConcavity;


    /// <summary>
    /// Gets or sets the small island boost factor.
    /// </summary>
    /// <value>The small island boost factor. The default value is 0.01.</value>
    /// <remarks>
    /// If this value is larger the merging of small island pairs is preferred. Making this value
    /// lower reduces the speed of the convex decomposition. 
    /// </remarks>
    public float SmallIslandBoost { get; set; }


    /// <summary>
    /// Gets or sets the maximal number of vertices per convex part in the final result.
    /// </summary>
    /// <value>
    /// The maximal number of vertices per convex part in the final result. The default value is 32.
    /// </value>
    /// <remarks>
    /// <see cref="VertexLimit"/> defines the max number of vertices per convex part of the final
    /// decomposition. <see cref="IntermediateVertexLimit"/> defines the max number of vertices
    /// during the decomposition process. 
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is less than 4.
    /// </exception>
    public int VertexLimit
    {
      get { return _vertexLimit; }
      set
      {
        if (value < 4)
          throw new ArgumentOutOfRangeException("value", "VertexLimit should be greater than 3.");

        _vertexLimit = value;
      }
    }
    private int _vertexLimit;


    /// <summary>
    /// Gets or sets the maximal of number vertices per convex part during the decomposition 
    /// process.
    /// </summary>
    /// <value>
    /// The maximal number of vertices per convex part during the decomposition process. The default 
    /// value is 32.
    /// </value>
    /// <remarks>
    /// <see cref="VertexLimit"/> defines the max number of vertices per convex part of the final
    /// decomposition. <see cref="IntermediateVertexLimit"/> defines the max number of vertices
    /// during the decomposition process. 
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is less than 4.
    /// </exception>
    public int IntermediateVertexLimit
    {
      get { return _intermediateVertexLimit; }
      set
      {
        if (value < 4)
          throw new ArgumentOutOfRangeException("value", "IntermediateVertexLimit should be greater than 3.");

        _intermediateVertexLimit = value;
      }
    }
    private int _intermediateVertexLimit;


    /// <summary>
    /// Gets or sets the width of the skin of each convex part.
    /// </summary>
    /// <value>The width of the skin of each convex part. The default value is 0.</value>
    /// <remarks>
    /// If this value is positive, the convex parts are extruded by this value. If this value is 
    /// negative, the convex parts are shrunk by this value.
    /// </remarks>
    public float SkinWidth { get; set; }


    /// <summary>
    /// Gets the convex decomposition of the mesh.
    /// </summary>
    /// <value>
    /// A composite shape where each child is a <see cref="ConvexPolyhedron"/>.
    /// </value>
    public CompositeShape Decomposition
    {
      get
      {
        lock (_syncRoot)
        {
          if (_decomposition == null)
            CreateCompositeShape();

          return _decomposition;
        }
      }
    }
    private CompositeShape _decomposition;


    /// <summary>
    /// Gets or sets a value indicating whether an asynchronous decomposition is in progress.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if an asynchronous decomposition is in progress; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsBusy { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether triangle vertices are used for concavity
    /// computation.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if triangle vertices are used for concavity computation; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    public bool SampleTriangleVertices { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether triangle centers are used for concavity computation.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if triangle centers are used for concavity computation; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    public bool SampleTriangleCenters { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether multithreading is enabled. (Experimental!)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if multithreading is enabled; otherwise, <see langword="false"/>. The
    /// default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Multithreading support is currently experimental. If multithreading is enabled,
    /// the result might be non-deterministic. (The result can vary between two calls with the 
    /// same settings!)
    /// </remarks>
    public bool EnableMultithreading { get; set; }
    

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event ProgressChangedEventHandler ProgressChanged;


    /// <summary>
    /// Occurs when an asynchronous decomposition (see <see cref="DecomposeAsync"/>) has completed.
    /// </summary>
    public event EventHandler<AsyncCompletedEventArgs> Completed;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvexDecomposition"/> class.
    /// </summary>
    public ConvexDecomposition()
    {
      AllowedConcavity = 0.1f;
      SmallIslandBoost = 0.01f;
      VertexLimit = 32;
      IntermediateVertexLimit = 32;
      SkinWidth = 0;
      SampleTriangleVertices = true;
      SampleTriangleCenters = false;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Decomposes the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <remarks>
    /// This method blocks until the decomposition has finished. The result is available
    /// in the property <see cref="Decomposition"/>.
    /// </remarks>
    public void Decompose(ITriangleMesh mesh)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      _mesh = mesh;
      DoWork();
    }


    /// <summary>
    /// Decomposes the specified mesh (asynchronously).
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <remarks>
    /// <para>
    /// This method does not block. The flag <see cref="IsBusy"/> is set until the decomposition has
    /// finished. The event <see cref="ProgressChanged"/> informs you on the current progress. The
    /// event <see cref="Completed"/> is raised when the decomposition is finished. The current
    /// intermediate decomposition result is available in <see cref="Decomposition"/>. Retrieving
    /// the result while the decomposition is running is possible but will temporarily block the
    /// decomposition process.
    /// </para>
    /// <para>
    /// <strong>Thread-Safety:</strong><br/>
    /// The <paramref name="mesh"/> must not be modified while the decomposition is in progress.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Convex decomposition is already in progress.
    /// </exception>
    public void DecomposeAsync(ITriangleMesh mesh)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");
      if (IsBusy)
        throw new InvalidOperationException("Convex decomposition is already in progress.");

      _mesh = mesh;
      _cancel = false;
      IsBusy = true;

      Parallel.Start(DoWork);
    }


    /// <summary>
    /// Cancels the current asynchronous decomposition process.
    /// </summary>
    public void CancelAsync()
    {
      _cancel = true;
    }


    private void OnProgressChanged(int progress)
    {
      var handler = ProgressChanged;

      if (handler != null)
        handler(this, new ProgressChangedEventArgs(progress, null));
    }


    private void DoWork()
    {
      // The locks are released regularly, so that the Decomposition property can be 
      // accessed.

      Exception exception = null;
      try
      {
        if ((GlobalSettings.ValidationLevelInternal & GlobalSettings.ValidationLevelUserHighExpensive) != 0)
          ValidateInput();

        lock (_syncRoot)
        {
          _decomposition = null;

          // Create the Dual graph.
          CreateDualGraph();
        }

        // Partitioning process: 
        MergeIslands(); // Each internal loop is locked.

        lock (_syncRoot)
        {
          if (!_cancel)
            CreateCompositeShape();
        }

        if (!_cancel)
          OnProgressChanged(100);
      }
      catch (Exception e)
      {
        exception = e;

        if (!IsBusy)  // Throw only when in synchronous decomposition.
          throw;
      }
      finally
      {
        _mesh = null;

        if (IsBusy) // IsBusy is only set for async operations.
        {
          IsBusy = false;

          // Raise completed event.
          var handler = Completed;
          if (handler != null)
            handler(this, new AsyncCompletedEventArgs(exception, _cancel, null));
        }
      }
    }


    private void ValidateInput()
    {
      int numberOfTriangles = _mesh.NumberOfTriangles;
      for (int i = 0; i < numberOfTriangles; i++)
      {
        var triangle = _mesh.GetTriangle(i);

        // Check for NaN or infinity. If we sum up all values, we only have to make one check.
        float value = triangle.Vertex0.X + triangle.Vertex0.Y + triangle.Vertex0.Z
                      + triangle.Vertex1.X + triangle.Vertex1.Y + triangle.Vertex1.Z
                      + triangle.Vertex2.X + triangle.Vertex2.Y + triangle.Vertex2.Z;
        if (!Numeric.IsFinite(value))
          throw new GeometryException("Cannot compute convex decomposition because the vertex positions are invalid (e.g. NaN or infinity).");
      }
    }


    private void CreateDualGraph()
    {
      var triangles = new List<CDTriangle>();

      // Convert to TriangleMesh.
      var triangleMesh = _mesh as TriangleMesh;
      if (triangleMesh == null)
      {
        triangleMesh = new TriangleMesh();
        triangleMesh.Add(_mesh, false);
        triangleMesh.WeldVertices();
      }

      // Initialize vertex normals.
      var normals = new Vector3F[triangleMesh.Vertices.Count];    // Vertex normals.
      var neighborCounts = new int[triangleMesh.Vertices.Count];  // Numbers of triangles that touch each vertex.
      for (int i = 0; i < triangleMesh.Vertices.Count; i++)
      {
        normals[i] = Vector3F.Zero;
        neighborCounts[i] = 0;
      }

      // Go through all triangles. Add the normal to normals and increase the neighborCounts
      for (int i = 0; i < triangleMesh.NumberOfTriangles; i++)
      {
        Triangle triangle = triangleMesh.GetTriangle(i);
        var normal = triangle.Normal;

        for (int j = 0; j < 3; j++)
        {
          var vertexIndex = triangleMesh.Indices[(i * 3) + j];
          normals[vertexIndex] = normals[vertexIndex] + normal;
          neighborCounts[vertexIndex] = neighborCounts[vertexIndex] + 1;
        }
      }

      // Create triangles.
      for (int i = 0; i < triangleMesh.NumberOfTriangles; i++)
      {
        Triangle triangle = triangleMesh.GetTriangle(i);
        var cdTriangle = new CDTriangle
        {
          Id = i,
          Vertices = new[] { triangle.Vertex0, triangle.Vertex1, triangle.Vertex2 },
          Normal = triangle.Normal,   // TODO: Special care for degenerate triangles needed?
        };

        for (int j = 0; j < 3; j++)
        {
          var vertexIndex = triangleMesh.Indices[(i * 3) + j];
          var normalSum = normals[vertexIndex];
          var neighborCount = neighborCounts[vertexIndex];
          if (neighborCount > 0)
          {
            var normal = normalSum / neighborCount;
            normal.TryNormalize();
            cdTriangle.VertexNormals[j] = normal;
          }
        }

        triangles.Add(cdTriangle);
      }

      // Create an island for each triangle.
      _islands = new List<CDIsland>(triangles.Count);
      for (int i = 0; i < triangles.Count; i++)
      {
        var triangle = triangles[i];

        var island = new CDIsland();
        island.Id = i;
        island.Triangles = new[] { triangle };
        island.Vertices = triangle.Vertices;

        island.Aabb = new Aabb(triangle.Vertices[0], triangle.Vertices[0]);
        island.Aabb.Grow(triangle.Vertices[1]);
        island.Aabb.Grow(triangle.Vertices[2]);

        triangle.Island = island;

        _islands.Add(island);
      }

      // Find connectivity (= add neighbor links).
      for (int i = 0; i < triangles.Count; i++)
      {
        var a = triangles[i];
        for (int j = i + 1; j < triangles.Count; j++)
        {
          var b = triangles[j];
          CDTriangle.FindNeighbors(a, b);
        }
      }

      // Create links.
      _links = new List<CDIslandLink>();
      for (int i = 0; i < _islands.Count; i++)
      {
        var island = _islands[i];
        var triangle = island.Triangles[0];

        // Go through all neighbors. 
        // If there is a neighbor, create a link.
        // To avoid two links per triangle, we create the link only if the id of this triangle
        // is less than the other island id.
        for (int j = 0; j < 3; j++)
        {
          CDTriangle neighborTriangle = triangle.Neighbors[j];
          if (neighborTriangle != null && neighborTriangle.Island.Id > i)
          {
            var link = new CDIslandLink(island, neighborTriangle.Island, AllowedConcavity, SmallIslandBoost, IntermediateVertexLimit, SampleTriangleVertices, SampleTriangleCenters);
            _links.Add(link);
          }
        }
      }

      // Now, we have a lot of islands with 1 triangle each.
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private void MergeIslands()
    {
      List<CDIslandLink> newLinks = new List<CDIslandLink>();
      List<CDIslandLink> obsoleteLinks = (EnableMultithreading) ? new List<CDIslandLink>() : null;

      while (_links.Count > 0 && !_cancel)
      {
        lock (_syncRoot)
        {
          // Find link with lowest decimation cost.
          CDIslandLink bestLink = _links[0];
          int bestLinkIndex = 0;
          for (int i = 0; i < _links.Count; i++)
          {
            var link = _links[i];
            if (link.DecimationCost < bestLink.DecimationCost)
            {
              bestLink = link;
              bestLinkIndex = i;
            }
          }

          // Remove the found link.
          _links.RemoveAt(bestLinkIndex);

          // Ignore links that have exceeded the concavity limit.
          if (bestLink.Concavity > AllowedConcavity)
            continue;

          // The created composite shape is now invalid again. 
          _decomposition = null;

          // Remove island B
          _islands.Remove(bestLink.IslandB);

          // Merge the islands of the best link into island A.
          foreach (var triangle in bestLink.IslandB.Triangles)
            triangle.Island = bestLink.IslandA;
          bestLink.IslandA.Triangles = bestLink.IslandA.Triangles.Union(bestLink.IslandB.Triangles).ToArray();
          bestLink.IslandA.Aabb = bestLink.Aabb;
          bestLink.IslandA.Vertices = bestLink.Vertices;
          bestLink.IslandA.ConvexHullBuilder = bestLink.ConvexHullBuilder;

          // Remove old links where A and B are involved and add new
          // links with A.
          if (!EnableMultithreading)
          {
            for (int i = _links.Count - 1; i >= 0; i--)
            {
              var link = _links[i];
              CDIsland otherIsland = null;
              if (link.IslandA == bestLink.IslandA || link.IslandA == bestLink.IslandB)
                otherIsland = link.IslandB;
              else if (link.IslandB == bestLink.IslandA || link.IslandB == bestLink.IslandB)
                otherIsland = link.IslandA;

              // This link does not link to the merged islands.
              if (otherIsland == null)
                continue;

              // Remove link.
              _links.RemoveAt(i);

              // If _newLinks already contains a link with otherIsland we are done.
              bool linkExists = false;
              foreach (var newLink in newLinks)
              {
                if (newLink.IslandA == otherIsland || newLink.IslandB == otherIsland)
                {
                  linkExists = true;
                  break;
                }
              }

              if (linkExists)
                continue;

              // Create link between otherIsland and bestLink.IslandA.
              link = new CDIslandLink(otherIsland, bestLink.IslandA, AllowedConcavity, SmallIslandBoost,
                                      IntermediateVertexLimit, SampleTriangleVertices, SampleTriangleCenters);
              newLinks.Add(link);
            }
          }
          else
          {
            // Experimental multithreading hack.
            // Note: When multithreading is enabled the result is non-deterministic 
            // because the order of the links in the _links list change...
            Parallel.ForEach(_links, link =>
            {
              CDIsland otherIsland = null;
              if (link.IslandA == bestLink.IslandA || link.IslandA == bestLink.IslandB)
                otherIsland = link.IslandB;
              else if (link.IslandB == bestLink.IslandA || link.IslandB == bestLink.IslandB)
                otherIsland = link.IslandA;

              // This link does not link to the merged islands.
              if (otherIsland == null)
                return;

              // Remove link.
              lock (obsoleteLinks)
                obsoleteLinks.Add(link);

              // If _newLinks already contains a link with otherIsland we are done.
              lock (newLinks)
              {
                foreach (var newLink in newLinks)
                  if (newLink.IslandA == otherIsland || newLink.IslandB == otherIsland)
                    return;
              }

              // Create link between otherIsland and bestLink.IslandA.
              link = new CDIslandLink(otherIsland, bestLink.IslandA, AllowedConcavity, SmallIslandBoost,
                                      IntermediateVertexLimit, SampleTriangleVertices, SampleTriangleCenters);

              // Add link but only if another thread did not add a similar link.
              // TODO: Can this happen or can we remove this check. 
              lock (newLinks)
              {
                foreach (var newLink in newLinks)
                  if (newLink.IslandA == otherIsland || newLink.IslandB == otherIsland)
                    return;

                newLinks.Add(link);
              }
            });
            
            foreach (var link in obsoleteLinks)
              _links.Remove(link);

            obsoleteLinks.Clear();
          }

          // Add new links.
          _links.AddRange(newLinks);
          newLinks.Clear();

          OnProgressChanged((_mesh.NumberOfTriangles - _islands.Count) * 100 / _mesh.NumberOfTriangles);
        }
      }
    }


    /// <summary>
    /// Creates the composite shape and saves it in <see cref="_decomposition"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private void CreateCompositeShape()
    {
      // Convert islands into CompositeShape with convex children.
      _decomposition = new CompositeShape();

      if (_islands == null)
        return;

      foreach (var island in _islands)
      {
        if (island.Vertices.Length <= 0)
          continue;

        // ReSharper disable EmptyGeneralCatchClause
        try
        {
          // ----- Get convex hull mesh.
          DcelMesh convexHullMesh;
          if (island.ConvexHullBuilder == null)
          {
            // Create convex hull from scratch.

            // Get all vertices of all island triangles.
            var points = island.Triangles.SelectMany(t => t.Vertices);

            // Create convex hull. 
            convexHullMesh = GeometryHelper.CreateConvexHull(points, VertexLimit, SkinWidth);
          }
          else
          {
            // Use existing convex hull.
            convexHullMesh = island.ConvexHullBuilder.Mesh;
            if (convexHullMesh.Vertices.Count > VertexLimit || SkinWidth != 0)
              convexHullMesh.ModifyConvex(VertexLimit, SkinWidth);
          }

          // ----- Add a ConvexPolyhedron to CompositeShape.
          if (convexHullMesh.Vertices.Count > 0)
          {
            var convexHullPoints = convexHullMesh.Vertices.Select(v => v.Position);
            var convexPolyhedron = new ConvexPolyhedron(convexHullPoints);
            var geometricObject = new GeometricObject(convexPolyhedron);
            _decomposition.Children.Add(geometricObject);
          }
        }
        catch
        {
          // Could not generate convex hull. Ignore object.
        }
        // ReSharper restore EmptyGeneralCatchClause
      }
    }
    #endregion
  }
}
#endif
