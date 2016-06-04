#if !ANDROID && !IOS   // Cannot read from vertex buffer in MonoGame/OpenGLES.
using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.Materials;
using DigitalRune.Physics.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using Samples.Animation;


namespace Samples.Physics.Specialized
{
  [Sample(SampleCategory.PhysicsSpecialized,
    @"This sample shows how to automatically create collision shapes for an articulated model.",
    @"The vertices of the model are extracted from the vertex buffer. A convex hull shape is
created for each bone.
No ragdoll joints or limits are created.
Important: This is not the recommended method for creating ragdolls. This method creates
too many shapes. There can be gaps at the joints. Manual placed boxes or capsules are
usually faster than convex hulls. Etc.",
    15)]
  [Controls(@"Sample
  Press <Space> to display model in bind pose.
  Press <Enter> to toggle wire frame mode.")]
  public class AutoRagdollShapesSample : CharacterAnimationSample
  {
    private readonly MeshNode _meshNode;
    private readonly Ragdoll _ragdoll;

    private bool _drawWireFrame = true;


    public AutoRagdollShapesSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var modelNode = ContentManager.Load<ModelNode>("Dude/Dude");
      _meshNode = modelNode.GetSubtree().OfType<MeshNode>().First().Clone();
      _meshNode.PoseLocal = new Pose(new Vector3F(0, 0, 0), Matrix33F.CreateRotationY(ConstantsF.Pi));
      SampleHelper.EnablePerPixelLighting(_meshNode);
      GraphicsScreen.Scene.Children.Add(_meshNode);

      var animations = _meshNode.Mesh.Animations;
      var loopingAnimation = new AnimationClip<SkeletonPose>(animations.Values.First())
      {
        LoopBehavior = LoopBehavior.Cycle,
        Duration = TimeSpan.MaxValue,
      };
      var animationController = AnimationService.StartAnimation(loopingAnimation, (IAnimatableProperty)_meshNode.SkeletonPose);
      animationController.AutoRecycle();
      animationController.UpdateAndApply();

      // Create a ragdoll for the Dude model.
      _ragdoll = CreateRagdoll(_meshNode);

      // Set the world space pose of the whole ragdoll. 
      _ragdoll.Pose = _meshNode.PoseWorld;
      // And copy the bone poses of the current skeleton pose.
      _ragdoll.UpdateBodiesFromSkeleton(_meshNode.SkeletonPose);

      foreach (var body in _ragdoll.Bodies)
      {
        if (body != null)
        {
          // Set all bodies to kinematic - they should not be affected by forces.
          body.MotionType = MotionType.Kinematic;

          // Disable collision response.
          body.CollisionResponseEnabled = false;
        }
      }

      // Add ragdoll rigid bodies to the simulation.
      _ragdoll.AddToSimulation(Simulation);
    }


    private Ragdoll CreateRagdoll(MeshNode meshNode)
    {
      var mesh = meshNode.Mesh;
      var skeleton = mesh.Skeleton;

      // Extract the vertices from the mesh sorted per bone.
      var verticesPerBone = new List<Vector3F>[skeleton.NumberOfBones];
      // Also get the AABB of the model.
      Aabb? aabb = null;
      foreach (var submesh in mesh.Submeshes)
      {
        // Get vertex element info.
        var vertexDeclaration = submesh.VertexBuffer.VertexDeclaration;
        var vertexElements = vertexDeclaration.GetVertexElements();

        // Get the vertex positions.
        var positionElement = vertexElements.First(e => e.VertexElementUsage == VertexElementUsage.Position);
        if (positionElement.VertexElementFormat != VertexElementFormat.Vector3)
          throw new NotSupportedException("For vertex positions only VertexElementFormat.Vector3 is supported.");
        var positions = new Vector3[submesh.VertexCount];
        submesh.VertexBuffer.GetData(
          submesh.StartVertex * vertexDeclaration.VertexStride + positionElement.Offset,
          positions,
          0,
          submesh.VertexCount,
          vertexDeclaration.VertexStride);

        // Get the bone indices.
        var boneIndexElement = vertexElements.First(e => e.VertexElementUsage == VertexElementUsage.BlendIndices);
        if (boneIndexElement.VertexElementFormat != VertexElementFormat.Byte4)
          throw new NotSupportedException();
        var boneIndicesArray = new Byte4[submesh.VertexCount];
        submesh.VertexBuffer.GetData(
          submesh.StartVertex * vertexDeclaration.VertexStride + boneIndexElement.Offset,
          boneIndicesArray,
          0,
          submesh.VertexCount,
          vertexDeclaration.VertexStride);

        // Get the bone weights.
        var boneWeightElement = vertexElements.First(e => e.VertexElementUsage == VertexElementUsage.BlendWeight);
        if (boneWeightElement.VertexElementFormat != VertexElementFormat.Vector4)
          throw new NotSupportedException();
        var boneWeightsArray = new Vector4[submesh.VertexCount];
        submesh.VertexBuffer.GetData(
          submesh.StartVertex * vertexDeclaration.VertexStride + boneWeightElement.Offset,
          boneWeightsArray,
          0,
          submesh.VertexCount,
          vertexDeclaration.VertexStride);

        // Sort the vertices per bone. 
        for (int i = 0; i < submesh.VertexCount; i++)
        {
          var vertex = (Vector3F)positions[i];

          // Here, we only check the first bone index. We could also check the
          // bone weights to add the vertex to all bone vertex lists where the 
          // weight is high...
          Vector4 boneIndices = boneIndicesArray[i].ToVector4();
          //Vector4 boneWeights = boneWeightsArray[i];
          int boneIndex = (int)boneIndices.X;
          if (verticesPerBone[boneIndex] == null)
            verticesPerBone[boneIndex] = new List<Vector3F>();
          verticesPerBone[boneIndex].Add(vertex);

          // Add vertex to AABB.
          if (aabb == null)
            aabb = new Aabb(vertex, vertex);
          else
            aabb.Value.Grow(vertex);
        }
      }

      // We create a body for each bone with vertices.
      int numberOfBodies = verticesPerBone.Count(vertices => vertices != null);

      // We use the same mass properties for all bodies. This is not realistic but more stable 
      // because large mass differences or thin bodies (arms!) are less stable.
      // We use the mass properties of sphere proportional to the size of the model.
      const float totalMass = 80;     // The total mass of the ragdoll.
      var massFrame = MassFrame.FromShapeAndMass(new SphereShape(aabb.Value.Extent.Y / 8), Vector3F.One, totalMass / numberOfBodies, 0.1f, 1);

      var material = new UniformMaterial();

      Ragdoll ragdoll = new Ragdoll();
      for (int boneIndex = 0; boneIndex < skeleton.NumberOfBones; boneIndex++)
      {
        var boneVertices = verticesPerBone[boneIndex];
        if (boneVertices != null)
        {
          var bindPoseInverse = (Pose)skeleton.GetBindPoseAbsoluteInverse(boneIndex);

          // Compute bounding capsule.
          //float radius;
          //float height;
          //Pose pose;
          //GeometryHelper.ComputeBoundingCapsule(boneVertices, out radius, out height, out pose);
          //Shape shape = new TransformedShape(new GeometricObject(new CapsuleShape(radius, height), pose));

          // Compute convex hull.
          var points = GeometryHelper.CreateConvexHull(boneVertices, 32, 0).ToTriangleMesh().Vertices;
          Shape shape = new ConvexHullOfPoints(points.Count > 0 ? points : boneVertices);

          ragdoll.Bodies.Add(new RigidBody(shape, massFrame, material));
          ragdoll.BodyOffsets.Add(bindPoseInverse);
        }
        else
        {
          ragdoll.Bodies.Add(null);
          ragdoll.BodyOffsets.Add(Pose.Identity);
        }
      }

      return ragdoll;
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <Space> --> Reset skeleton pose to bind pose.
      if (InputService.IsDown(Keys.Space))
        _meshNode.SkeletonPose.ResetBoneTransforms();

      // <Enter> --> Toggle wireframe drawing.
      if (InputService.IsPressed(Keys.Enter, true))
        _drawWireFrame = !_drawWireFrame;

      // Update the bodies to match the current skeleton pose.
      // This method changes the body poses directly. 
      _ragdoll.UpdateBodiesFromSkeleton(_meshNode.SkeletonPose);

      // Use DebugRenderer to visualize rigid bodies.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      foreach (var body in Simulation.RigidBodies)
      {
        debugRenderer.DrawObject(body, Color.Gray, _drawWireFrame, false);
      }
    }
  }
}
#endif