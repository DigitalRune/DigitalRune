using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Graphics.SceneGraph.Tests
{
  [TestFixture]
  public class SceneTest
  {
    private class TestSceneNode : SceneNode
    {
      public new Shape Shape
      {
        get { return base.Shape; }
        set { base.Shape = value; }
      }
    }


    [Test]
    public void RandomizedMassSceneUpdate()
    {
      const int NumberOfSceneNodes = 6;
      const int NumberOfSteps = 10000;
      const float Saturation = 0.7f;  // Percent of scene nodes which should be in the scene.

      var random = new Random(1234567);
      var scene = new Scene();

      int numberOfNodesInScene = 0;

      var nodes = new TestSceneNode[NumberOfSceneNodes];

      // Create random nodes.
      for (int i = 0; i < NumberOfSceneNodes; i++)
      {
        var node = new TestSceneNode();
        nodes[i] = node;

        var position = random.NextVector3F(-1000, 1000);
        var orientation = random.NextQuaternionF();
        node.PoseLocal = new Pose(position, orientation);

        float p = random.NextFloat(0, 1);
        if (p < 0.1f)
          node.Shape = Shape.Empty;
        else if (p < 0.2f)
          node.Shape = Shape.Infinite;
        //else if (p < 0.21f)
        //{
        //  node.Shape = new BoxShape(float.PositiveInfinity, 1, 1);

        //  // Remove orientation - otherwise we get infinite AABB.
        //  node.PoseLocal = new Pose(node.PoseLocal.Position);

        //  var aabb = node.Aabb;
        //  var isValid = node.Aabb.Extent.IsNaN;
        //}
        else
          node.Shape = new SphereShape(random.NextFloat(0, 10));
      }

      for (int updateIndex = 0; updateIndex < NumberOfSteps; updateIndex++)
      {
        for (int i = 0; i < NumberOfSceneNodes; i++)
        {
          var node = nodes[i];

          // Add
          if (node.Parent == null)
          {
            if (random.NextFloat(0, 1) < 0.1f) // 10 percent change to add.
            {
              numberOfNodesInScene++;
              scene.Children.Add(node);
            }
          }

          // Remove
          if (node.Parent != null && ((float)numberOfNodesInScene) / (float)NumberOfSceneNodes > Saturation) 
          {
            if (random.NextFloat(0, 1) < 0.1f) // 10 percent change to remove.
            {
              numberOfNodesInScene--;
              node.Parent.Children.Remove(node);
            }
          }

          // Move
          //if (node.IsInScene)
          {
            if (random.NextFloat(0, 1) < 0.5f) // 50% change to move
            {
              var pose = node.PoseWorld;
              pose.Position += random.NextVector3F(0, 10);
              node.PoseWorld = pose;
            }
            if (random.NextFloat(0, 1) < 0.1f) // 50% change to scale
            {
              node.ScaleLocal = random.NextVector3F(0.5f, 1.5f);
            }
          }
        }

        scene.Update(TimeSpan.FromSeconds(0.016666666f));
      }
    }


    [Test]
    public void RandomizedMassSceneUpdate2()
    {
      const int NumberOfSceneNodes = 10;
      const int NumberOfSteps = 10000;
      const float WorldSize = 1000;

      var random = new Random(123457);
      var scene = new Scene();
      scene.EnableMultithreading = false;

      var nodes = new TestSceneNode[NumberOfSceneNodes];

      // Create random nodes.
      for (int i = 0; i < NumberOfSceneNodes; i++)
      {
        var node = new TestSceneNode();
        nodes[i] = node;

        var position = random.NextVector3F(0, WorldSize);
        var orientation = random.NextQuaternionF();
        node.PoseLocal = new Pose(position, orientation);

        float p = random.NextFloat(0, 100);
        if (p < 0.1f)
          node.Shape = Shape.Empty;
        else if (p < 0.2f)
          node.Shape = Shape.Infinite;
        else if (p < 0.6f)
          node.Shape = new BoxShape(random.NextVector3F(0, WorldSize));
        else 
          node.Shape = new SphereShape(random.NextFloat(0, WorldSize));
      }

      var projection = new PerspectiveProjection();
      projection.SetFieldOfView(0.8f, 1, WorldSize / 10000, WorldSize);
      var camera = new Camera(projection);
      var cameraNode = new CameraNode(camera);

      for (int updateIndex = 0; updateIndex < NumberOfSteps; updateIndex++)
      {
        int actionsPerFrame = random.NextInteger(0, 100);
        for (int i = 0; i < actionsPerFrame; i++)
        {
          var node = nodes[random.Next(0, NumberOfSceneNodes)];

          const int numberOfActions = 100;
          int action = random.Next(0, numberOfActions);

          //scene.Validate();

          if (action == 0)
          {
            // Add
            if (node.Parent == null)
            {
              scene.Children.Add(node);
              //scene.Validate();
            }
          }
          else if (action == 1)
          {
            // Remove
            if (node.Parent != null)
            {
              node.Parent.Children.Remove(node);
              //scene.Validate();
            }
          }
          else if (action == 2)
          {
            // Move
            var pose = node.PoseWorld;
            pose.Position = random.NextVector3F(0, WorldSize);
            node.PoseWorld = pose;
            //scene.Validate();
          }
          else if (action == 3)
          {
            // Very small Move
            var pose = node.PoseWorld;
            const float maxDistance = WorldSize / 10000;
            pose.Position += random.NextVector3F(-maxDistance, maxDistance);
            node.PoseWorld = pose;
            //scene.Validate();
          }
          else if (action == 4)
          {
            // Small Move
            var pose = node.PoseWorld;
            const float maxDistance = WorldSize / 100;
            pose.Position += random.NextVector3F(-maxDistance, maxDistance);
            node.PoseWorld = pose;
            //scene.Validate();
          }
          else if (action == 5)
          {
            // Scale
            node.ScaleLocal = random.NextVector3F(0.0f, 10f);
            //scene.Validate();
          }
          else if (action == 6)
          {
            // Rotate
            node.PoseWorld = new Pose(node.PoseWorld.Position, random.NextQuaternionF());
            //scene.Validate();
          }
          else if (action == 7)
          {
            // Query
            var query = scene.Query<CameraFrustumQuery>(cameraNode, null);
            //Debug.WriteLine("Camera queried nodes: " + query.SceneNodes.Count);
            //scene.Validate();
          }
          else if (action == 8)
          {
            // Move camera.
            cameraNode.PoseWorld = new Pose(random.NextVector3F(0, WorldSize), random.NextQuaternionF());
            //scene.Validate();
          }
          else if (action == 9)
          {
            // Change shape.
            int type = random.NextInteger(0, 5);
            if (type == 0)
              node.Shape = new BoxShape(random.NextVector3F(0, WorldSize));
            else if (type == 1)
              node.Shape = new SphereShape(random.NextFloat(0, WorldSize));
            else if (type == 2)
              node.Shape = new BoxShape(new Vector3F(Single.MaxValue));
            else if (type == 3)
              node.Shape = new BoxShape(new Vector3F(0));
            else if (type == 4)
              node.Shape = Shape.Empty;
            //scene.Validate();
          }
          else if (action == 10)
          {
            // Add to random parent.
            if (node.Parent == null)
            {
              var randomParent = nodes[random.NextInteger(0, NumberOfSceneNodes - 1)];

              // Avoid loops:
              bool isLoop = node.GetSubtree().Contains(randomParent);
              if (!isLoop)
              {
                if (randomParent.Children == null)
                  randomParent.Children = new SceneNodeCollection();

                randomParent.Children.Add(node);
              }
            }
          }
          else if (action == 11)
          {
            if (node.Parent != null && node.Parent != scene)
              node.Parent.Children.Remove(node);
          }
          else if (action == 13)
          {
            if (random.NextInteger(0, 100) < 5)
            scene.Children.Clear();
          }
          else if (action == 14)
          {
            if (random.NextInteger(0, 100) < 5)
              scene.Children = new SceneNodeCollection();
          }
        }

        //Debug.WriteLine("Number of nodes in scene: " + scene.GetDescendants().Count());

        //scene.Validate();
        scene.Update(TimeSpan.FromSeconds(0.016666666f));
        //scene.Validate();
      }
    }


    [Test]
    public void Validate0()
    {
      // Disabled validation.
      GlobalSettings.ValidationLevel = 0;
      var scene = new Scene();
      scene.Children.Add(new TestSceneNode { PoseLocal = new Pose(new Vector3F(float.NaN))});

      // Enabled validation.
      GlobalSettings.ValidationLevel = 0xff;
      scene = new Scene();
      // NaN pose.
      Assert.Throws<GraphicsException>(()=> scene.Children.Add(new TestSceneNode { Name = "xyz", PoseLocal = new Pose(new Vector3F(float.NaN)) }));
    }


    [Test]
    public void Validate1()
    {
      // Enabled validation.
      GlobalSettings.ValidationLevel = 0xff;
      var scene = new Scene();
      // Invalid orientation.
      Assert.Throws<GraphicsException>(() => scene.Children.Add(new TestSceneNode { PoseLocal = new Pose() }));
    }


    [Test]
    public void Validate2()
    {
      // Enabled validation.
      GlobalSettings.ValidationLevel = 0xff;
      var scene = new Scene();
      // Invalid orientation.
      var matrix = Matrix33F.Identity;
      matrix.M20 = float.PositiveInfinity;
      Assert.Throws<GraphicsException>(() => scene.Children.Add(new TestSceneNode { PoseLocal = new Pose(matrix) }));
    }


    [Test]
    public void Validate3()
    {
      // Enabled validation.
      GlobalSettings.ValidationLevel = 0xff;
      var scene = new Scene();
      // Invalid shape.
      Assert.Throws<GraphicsException>(() => scene.Children.Add(new TestSceneNode { Shape = new SphereShape(float.PositiveInfinity) }));
    }


    [Test]
    public void Validate4()
    {
      // Enabled validation.
      GlobalSettings.ValidationLevel = 0xff;
      var scene = new Scene();
      // Invalid scale.
      Assert.Throws<GraphicsException>(() => scene.Children.Add(new TestSceneNode { ScaleLocal = new Vector3F(float.PositiveInfinity, 1, 1) }));
    }


    [Test]
    public void Validate5()
    {
      // Enabled validation.
      GlobalSettings.ValidationLevel = 0xff;
      var scene = new Scene();

      // This is allowed.
      var n = new TestSceneNode { Shape = Shape.Infinite };
      scene.Children.Add(n);

      // Invalid changes of already added node:
      Assert.Throws<GraphicsException>(() => n.ScaleLocal = new Vector3F(1, 1, float.NaN));
    }


    [Test]
    public void Validate6()
    {
      GlobalSettings.ValidationLevel = 0xff;
      var scene = new Scene();
      // This is allowed.
      var n = new TestSceneNode { Shape = Shape.Empty };
      scene.Children.Add(n);

      // Invalid changes of already added node:
      var mesh = new TriangleMesh();
      mesh.Add(new Triangle(new Vector3F(1), new Vector3F(2), new Vector3F(3)));
      mesh.Add(new Triangle(new Vector3F(4), new Vector3F(float.NaN, 5, 5), new Vector3F(6)));
      var meshShape = new TriangleMeshShape(mesh);
      Assert.Throws<GraphicsException>(() => n.Shape = meshShape);
    }

    
    [Test]
    public void Validate7()
    {
      GlobalSettings.ValidationLevel = 0xff;
      var scene = new Scene();
      // This is allowed.
      var n = new TestSceneNode { Shape = Shape.Empty };
      scene.Children.Add(n);

      // Invalid changes of already added node:
      Assert.Throws<GraphicsException>(() => n.PoseLocal = new Pose(new Vector3F(float.NaN)));
    }
  }
}
