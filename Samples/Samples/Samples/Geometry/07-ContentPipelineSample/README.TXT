In almost all 3D games, collisions between complex shaped objects must be detected. Creating 
collision shapes at load-time can be time consuming. This example demonstrates various ways how to 
create collision shapes using the XNA content pipeline and load the pre-built collision shapes at 
runtime.

The Samples.Content.Pipeline project contains a custom content processor 
"ModelWithCollisionMeshProcessor" that is derived from the DigitalRune DRModelProcessor. This 
content processor takes a model asset and builds a DigitalRune Graphics ModelNodel. In addition to 
that it creates a TriangleMeshShape that can be used for collision detection. The TriangleMeshShape 
is stored in the UserData property and exported together with the model.

Using triangle meshes for collision detection is slower than using simple shapes, such as boxes, 
spheres or convex shapes. Triangle meshes should be avoided whenever possible. One solution is to 
build a collision model that consists only of simple shapes that approximiate the original model. 
At runtime the detailed model is used for rendering and the collision model is used for collision 
detection.
The content pipeline project contains a second content processor called "CollisionShapeProcessor". 
This content processor takes a model asset and creates simple collision shapes. A special naming 
convention is used to identify boxes, spheres, or convex polyhedra whithin the model. (See below.)


----------------------------------------------
Description:
----------------------------------------------

Content
-------
The content project "Content" contains a several 3D models.
- Saucer/Saucer.fbx: A model of a flying saucer. This asset is processed using the new 
  ModelWithCollisionMeshProcessor from the project Samples.Content.Pipeline. The content processor 
  creates a DigitalRune ModelNode together with a TriangleMeshShape for collision detection. At 
  runtime the Model will be used for rendering and the TriangleMeshShape will be used for collision 
  detection.
- Ship/Ship.fbx: A model of a space ship. This asset is processed using the DigitalRune 
  DRModelProcessor to build a DigitalRune ModelNode. The ModelNode is used for rendering at runtime.
- Ship_CollisionModel.fbx: This model contains the approximated collision shapes (boxes, spheres,
  convex polyhedra) for the ship. This asset is processed using the new CollisionShapeProcessor from 
  the project Samples.Content.Pipeline. The content processor creates a Shape that is used for 
  collision detection at runtime.
  This file was created in Blender. It contains several boxes, spheres and convex meshes that
  approximate the shape of the ship. 
  Following naming convention is used:
  - The mesh of a box ends with "Box". 
  - The mesh of a sphere ends with "Sphere". 
  - The mesh of a convex mesh ends with "Convex".
  Please examine this model in an 3D modelling tool of your choice. (The original Blender file 
  "Ship.blend" is also included.)

Please note that the Content project references the DigitalRune content pipeline assemblies
(e.g. DigitalRune.Mathematics.Content.Pipeline.dll, DigitalRune.Geometry.Content.Pipeline.dll, ...) 
and Samples.Content.Pipeline.dll.
