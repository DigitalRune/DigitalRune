// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry
{  
  public static partial class GeometryHelper
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// A sensor consists of sampling direction and the index of the point that was sampled.
    /// </summary>
    private sealed class Sensor
    {
      public readonly Vector3F Direction;
      public int IndexOfPoint = -1;

      public Sensor(Vector3F direction)
      {
        Direction = direction;
      }
    }


    /// <summary>
    /// A sensor triangle consists of three sensors.
    /// </summary>
    private sealed class SensorTriangle
    {
      // The sensors:
      public int IndexOfSensor0;
      public int IndexOfSensor1;
      public int IndexOfSensor2;

      // The iteration in which this triangle was created. -1 if the triangle should
      // not be refined any further.
      public int Iteration;

      public SensorTriangle(int indexOfSensor0, int indexOfSensor1, int indexOfSensor2, int iteration)
      {
        IndexOfSensor0 = indexOfSensor0;
        IndexOfSensor1 = indexOfSensor1;
        IndexOfSensor2 = indexOfSensor2;
        Iteration = iteration;
      }
    }


    /// <summary>
    /// A list of points.
    /// </summary>
    private sealed class PointCollector
    {
      public readonly List<Vector3F> Points = new List<Vector3F>();
      public float DistanceThreshold;

      // The index of a point or -1 if the point is not contained.
      public int GetIndex(Vector3F point)
      {
        int numberOfPoints = Points.Count;
        for (int i = 0; i < numberOfPoints; i++)
        {
          Vector3F p = Points[i];
          if (DistanceThreshold > 0 && Vector3F.AreNumericallyEqual(p, point, DistanceThreshold)
              || DistanceThreshold == 0 && Vector3F.AreNumericallyEqual(p, point))
            return i;
        }

        return -1;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Creates a list of vertices of the convex hull by sampling the support mapping of a 
    /// <see cref="ConvexShape"/>.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <param name="distanceThreshold">
    /// The distance threshold; must be greater than or equal to 0.
    /// </param>
    /// <param name="iterationLimit">
    /// The iteration limit; must be greater than or equal to 1.
    /// </param>
    /// <returns>The points of the convex hull.</returns>
    /// <remarks>
    /// <para>
    /// This method refines the convex hull in several iterations. The convex hull will be refined 
    /// as long as the new vertices are significantly away from the previously computed hull (using 
    /// <paramref name="distanceThreshold"/> as the limit). The method will also terminate if more 
    /// than <paramref name="iterationLimit"/> iterations have been performed.
    /// </para>
    /// <para>
    /// Use this method only for 3-dimensional and finite shapes (e.g. not for 
    /// <see cref="PointShape"/>, <see cref="LineSegment"/>, etc.).
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shape"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="distanceThreshold"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="iterationLimit"/> is less than 1.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    internal static IList<Vector3F> SampleConvexShape(ConvexShape shape, float distanceThreshold, int iterationLimit)
    {
      if (shape == null)
        throw new ArgumentNullException("shape");
      if (distanceThreshold < 0)
        throw new ArgumentOutOfRangeException("distanceThreshold", "distanceThreshold must be greater than or equal to 0.");
      if (iterationLimit < 1)
        throw new ArgumentOutOfRangeException("iterationLimit", "iterationLimit must be greater than or equal to 1.");

      distanceThreshold = Math.Max(distanceThreshold, Numeric.EpsilonF);
      PointCollector points = new PointCollector { DistanceThreshold = distanceThreshold };

      // Start with 6 sensors and 8 sensor triangles (like a double pyramid).
      List<Sensor> sensors = new List<Sensor>
      {
        new Sensor(new Vector3F(-1, 0, 0)),
        new Sensor(new Vector3F(1, 0, 0)),
        new Sensor(new Vector3F(0, -1, 0)),
        new Sensor(new Vector3F(0, 1, 0)),
        new Sensor(new Vector3F(0, 0, -1)),
        new Sensor(new Vector3F(0, 0, 1))
      };

      List<SensorTriangle> triangles = new List<SensorTriangle>
      {
        new SensorTriangle(5, 1, 3, 1),   // SensorTriangle.Iteration is set to 1.
        new SensorTriangle(5, 2, 1, 1),
        new SensorTriangle(1, 2, 4, 1),
        new SensorTriangle(4, 2, 0, 1),
        new SensorTriangle(0, 2, 5, 1),
        new SensorTriangle(1, 4, 3, 1),
        new SensorTriangle(3, 4, 0, 1),
        new SensorTriangle(0, 5, 3, 1)
      };

      // Sample the first 6 sensors.
      int numberOfSensors = sensors.Count;
      for (int i = 0; i < numberOfSensors; i++)
      {
        Sensor sensor = sensors[i];

        Vector3F sample = shape.GetSupportPoint(sensor.Direction);
        int index = points.GetIndex(sample);
        if (index == -1)
        {
          // Add new point.
          index = points.Points.Count;
          points.Points.Add(sample);
        }

        sensor.IndexOfPoint = index;
      }

      // Collector for new points.
      PointCollector newPoints = new PointCollector();

      bool finished = false;
      while (!finished)
      {
        // Assume that we cannot refine. If we can refine, finished must be set to false.
        finished = true;

        // Reset collector for new points.
        newPoints.Points.Clear();

        int numberOfPoints = points.Points.Count;
        int numberOfTriangles = triangles.Count;
        for (int currentTriangle = 0; currentTriangle < numberOfTriangles; currentTriangle++)
        {
          // In each iteration refine the existing triangles by breaking up edges.
          SensorTriangle triangle = triangles[currentTriangle];

          int iteration = triangle.Iteration;
          if (iteration >= 0 && iteration < iterationLimit)
          {
            // Sensor indices.
            int indexOfSensor0 = triangle.IndexOfSensor0;
            int indexOfSensor1 = triangle.IndexOfSensor1;
            int indexOfSensor2 = triangle.IndexOfSensor2;

            // Sensors.
            Sensor sensor0 = sensors[indexOfSensor0];
            Sensor sensor1 = sensors[indexOfSensor1];
            Sensor sensor2 = sensors[indexOfSensor2];

            // New sensors.
            Sensor sensor01 = new Sensor((sensor0.Direction + sensor1.Direction).Normalized);
            Sensor sensor12 = new Sensor((sensor1.Direction + sensor2.Direction).Normalized);
            Sensor sensor20 = new Sensor((sensor2.Direction + sensor0.Direction).Normalized);

            // New Support Points.
            Vector3F v01 = shape.GetSupportPoint(sensor01.Direction);
            Vector3F v12 = shape.GetSupportPoint(sensor12.Direction);
            Vector3F v20 = shape.GetSupportPoint(sensor20.Direction);

            // See if the points are new.
            sensor01.IndexOfPoint = points.GetIndex(v01);
            sensor12.IndexOfPoint = points.GetIndex(v12);
            sensor20.IndexOfPoint = points.GetIndex(v20);

            // Check if at least one point is new, and if we have improved more than the distance threshold.
            // (The PointCollector.GetIndex() automatically takes care of the distance threshold.)
            if (sensor01.IndexOfPoint == -1 || sensor12.IndexOfPoint == -1 || sensor20.IndexOfPoint == -1)
            {
              // We refine the sensor triangle into 4 triangles.
              finished = false;

              // Indices of the new sensors.
              int indexOfSensor01 = sensors.Count;
              int indexOfSensor12 = indexOfSensor01 + 1;
              int indexOfSensor20 = indexOfSensor12 + 1;

              // Add sensors.
              sensors.Add(sensor01);
              sensors.Add(sensor12);
              sensors.Add(sensor20);

              // Use the instance of the old triangle for the first new triangle.
              triangle.IndexOfSensor0 = indexOfSensor01;
              triangle.IndexOfSensor1 = indexOfSensor12;
              triangle.IndexOfSensor2 = indexOfSensor20;
              iteration++;
              triangle.Iteration = iteration;

              // Add other triangles, but only if they contain a new point.
              if (sensor01.IndexOfPoint == -1 || sensor12.IndexOfPoint == -1)
                triangles.Add(new SensorTriangle(indexOfSensor01, indexOfSensor1, indexOfSensor12, iteration));
              if (sensor12.IndexOfPoint == -1 || sensor20.IndexOfPoint == -1)
                triangles.Add(new SensorTriangle(indexOfSensor12, indexOfSensor2, indexOfSensor20, iteration));
              if (sensor01.IndexOfPoint == -1 || sensor20.IndexOfPoint == -1)
                triangles.Add(new SensorTriangle(indexOfSensor0, indexOfSensor01, indexOfSensor20, iteration));

              // Add new points
              if (sensor01.IndexOfPoint == -1)
              {
                int newPointsIndex = newPoints.GetIndex(v01);
                if (newPointsIndex == -1)
                {
                  newPointsIndex = newPoints.Points.Count;
                  newPoints.Points.Add(v01);
                }

                sensor01.IndexOfPoint = numberOfPoints + newPointsIndex;
              }

              if (sensor12.IndexOfPoint == -1)
              {
                int newPointsIndex = newPoints.GetIndex(v12);
                if (newPointsIndex == -1)
                {
                  newPointsIndex = newPoints.Points.Count;
                  newPoints.Points.Add(v12);
                }

                sensor12.IndexOfPoint = numberOfPoints + newPointsIndex;
              }

              if (sensor20.IndexOfPoint == -1)
              {
                int newPointsIndex = newPoints.GetIndex(v20);
                if (newPointsIndex == -1)
                {
                  newPointsIndex = newPoints.Points.Count;
                  newPoints.Points.Add(v20);
                }

                sensor20.IndexOfPoint = numberOfPoints + newPointsIndex;
              }
            }
            else
            {
              // Fine enough.
              triangle.Iteration = -1;
            }
          }
        }

        // Copy new points to point collector.
        int numberOfNewPoints = newPoints.Points.Count;
        for (int i = 0; i < numberOfNewPoints; i++)
          points.Points.Add(newPoints.Points[i]);
      }

      return points.Points;
    }
    #endregion
  }
}
