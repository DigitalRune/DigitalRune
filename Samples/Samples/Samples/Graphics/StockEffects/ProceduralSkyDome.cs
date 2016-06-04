// This class is a modified version of:
// 
//  SkyProcessor.cs from the AppHub Reach Graphics Demo.
//  Microsoft XNA Community Game Platform
//  Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // Generates skydome mesh.
  public static class ProceduralSkyDome
  {
    // Note: This simple class creates unnecessary duplicate vertices - but showing 
    // how to create an optimized mesh is not the point of this sample. ;-)

    const float CylinderSize = 100;
    const int CylinderSegments = 32;
    const float TexCoordTop = 0.1f;
    const float TexCoordBottom = 0.9f;


    public static Mesh CreateMesh(IGraphicsService graphicsService, Texture2D texture)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");
      if (texture == null)
        throw new ArgumentNullException("texture");

      List<Vector3> positions = new List<Vector3>();
      List<ushort> indices = new List<ushort>();

      // Create two rings of vertices around the top and bottom of the cylinder.
      for (int i = 0; i < CylinderSegments; i++)
      {
        float angle = ConstantsF.TwoPi * i / CylinderSegments;

        float x = (float)Math.Cos(angle) * CylinderSize;
        float z = (float)Math.Sin(angle) * CylinderSize;

        positions.Add(new Vector3(x, CylinderSize * 5 / 12, z));
        positions.Add(new Vector3(x, -CylinderSize * 5 / 12, z));
      }

      // Create two center vertices, used for closing the top and bottom.
      positions.Add(new Vector3(0, CylinderSize, 0));
      positions.Add(new Vector3(0, -CylinderSize, 0));

      // Create the individual triangles that make up our skydome.
      List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();
      ushort index = 0;
      for (int i = 0; i < CylinderSegments; i++)
      {
        int j = (i + 1) % CylinderSegments;

        // Calculate texture coordinates for this segment of the cylinder.
        float u1 = (float)i / (float)CylinderSegments;
        float u2 = (float)(i + 1) / (float)CylinderSegments;

        // Two triangles form a quad, one side segment of the cylinder.
        vertices.Add(new VertexPositionTexture(positions[i * 2], new Vector2(u1, TexCoordTop)));
        indices.Add(index++);
        vertices.Add(new VertexPositionTexture(positions[j * 2], new Vector2(u2, TexCoordTop)));
        indices.Add(index++);
        vertices.Add(new VertexPositionTexture(positions[i * 2 + 1], new Vector2(u1, TexCoordBottom)));
        indices.Add(index++);

        vertices.Add(new VertexPositionTexture(positions[j * 2], new Vector2(u2, TexCoordTop)));
        indices.Add(index++);
        vertices.Add(new VertexPositionTexture(positions[j * 2 + 1], new Vector2(u2, TexCoordBottom)));
        indices.Add(index++);
        vertices.Add(new VertexPositionTexture(positions[i * 2 + 1], new Vector2(u1, TexCoordBottom)));
        indices.Add(index++);

        // Triangle fanning inward to fill the top above this segment.
        vertices.Add(new VertexPositionTexture(positions[CylinderSegments * 2], new Vector2(u1, 0)));
        indices.Add(index++);
        vertices.Add(new VertexPositionTexture(positions[j * 2], new Vector2(u2, TexCoordTop)));
        indices.Add(index++);
        vertices.Add(new VertexPositionTexture(positions[i * 2], new Vector2(u1, TexCoordTop)));
        indices.Add(index++);

        // Triangle fanning inward to fill the bottom below this segment.
        vertices.Add(new VertexPositionTexture(positions[CylinderSegments * 2 + 1], new Vector2(u1, 1)));
        indices.Add(index++);
        vertices.Add(new VertexPositionTexture(positions[i * 2 + 1], new Vector2(u1, TexCoordBottom)));
        indices.Add(index++);
        vertices.Add(new VertexPositionTexture(positions[j * 2 + 1], new Vector2(u2, TexCoordBottom)));
        indices.Add(index++);
      }

      // Create the vertex buffer.
      VertexBuffer vertexBuffer = new VertexBuffer(
        graphicsService.GraphicsDevice,
        VertexPositionTexture.VertexDeclaration,
        vertices.Count, BufferUsage.
        None);
      vertexBuffer.SetData(vertices.ToArray());

      // Create the index buffer.
      IndexBuffer indexBuffer = new IndexBuffer(
        graphicsService.GraphicsDevice,
        IndexElementSize.SixteenBits,
        indices.Count,
        BufferUsage.None);
      indexBuffer.SetData(indices.ToArray());

      // Create a submesh, which is a set of primitives which can be rendered in 
      // one draw call.
      Submesh submesh = new Submesh
      {
        PrimitiveCount = indices.Count / 3,
        PrimitiveType = PrimitiveType.TriangleList,
        StartIndex = 0,
        StartVertex = 0,
        VertexCount = vertices.Count,
        VertexBuffer = vertexBuffer,
        IndexBuffer = indexBuffer,
      };

      // Create a mesh (which is collection of submeshes and materials).
      Mesh mesh = new Mesh
      {
        Name = "Sky",
        BoundingShape = new CylinderShape(CylinderSize, 2 * CylinderSize),
      };
      mesh.Submeshes.Add(submesh);

      // Create a BasicEffectBinding which wraps the XNA BasicEffect.
      // An EffectBinding connects an effect with effect parameter values ("effect 
      // parameter binding"). Some of these parameter values are defined here. Others, 
      // like World matrices, light parameters, etc. are automatically updated by 
      // the graphics engine in each frame.
      BasicEffectBinding effectBinding = new BasicEffectBinding(graphicsService, null)
      {
        LightingEnabled = false,
        TextureEnabled = true,
        VertexColorEnabled = false
      };
      effectBinding.Set("Texture", texture);
      effectBinding.Set("SpecularColor", new Vector3(0, 0, 0));

      // Create a material, which is a collection of effect bindings - one effect 
      // binding for each "render pass". The sky mesh should be rendered in the 
      // "Sky" render pass. This render pass name is an arbitrary string that is 
      // used in SampleGraphicsScreen.cs.
      Material material = new Material();
      material.Add("Sky", effectBinding);

      // Assign the material to the submesh.
      submesh.SetMaterial(material);

      return mesh;
    }
  }
}
