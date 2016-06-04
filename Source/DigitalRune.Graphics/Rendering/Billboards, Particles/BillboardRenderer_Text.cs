// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Text;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  partial class BillboardRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The sprite batch and effect are created on demand.
    private SpriteBatch _spriteBatch;
    private BasicEffect _textEffect;

    // Default font used if no other font is specified.
    private SpriteFont _defaultFont;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    private void InitializeText(SpriteFont spriteFont)
    {
      _defaultFont = spriteFont;
    }


    private void DisposeText()
    {
      if (_textEffect != null)
        _textEffect.Dispose();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    private void DrawText(int index, int endIndex, RenderContext context)
    {
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderStates = new RenderStateSnapshot(graphicsDevice);

      // The sprite batch and text effect are only created when needed.
      if (_spriteBatch == null)
      {
        _spriteBatch = context.GraphicsService.GetSpriteBatch();
        _textEffect = new BasicEffect(graphicsDevice)
        {
          TextureEnabled = true,
          VertexColorEnabled = true,
        };
      }

      _textEffect.View = (Matrix)context.CameraNode.View;
      _textEffect.Projection = context.CameraNode.Camera.Projection;

      var jobs = _jobs.Array;
      while (index < endIndex)
      {
        var node = jobs[index++].Node as BillboardNode;
        if (node == null)
          continue;
        
        var billboard = node.Billboard as TextBillboard;
        if (billboard == null)
          continue;
        
        var font = billboard.Font ?? _defaultFont;
        if (font == null)
          continue;
        
        var text = billboard.Text as string;
        var stringBuilder = billboard.Text as StringBuilder;
        if (string.IsNullOrEmpty(text) && (stringBuilder == null || stringBuilder.Length == 0))
          continue;

        Vector3F position = node.PoseWorld.Position;
        var orientation = billboard.Orientation;

        #region ----- Billboarding -----

        // (Code copied from BillboardBatchReach.)

        // Normal
        Vector3F normal;
        if (orientation.Normal == BillboardNormal.ViewPlaneAligned)
        {
          normal = _defaultNormal;
        }
        else if (orientation.Normal == BillboardNormal.ViewpointOriented)
        {
          Vector3F n = _cameraPose.Position - position;
          normal = n.TryNormalize() ? n : _defaultNormal;
        }
        else
        {
          normal = node.Normal;
        }

        // Axis = up vector
        Vector3F axis = node.Axis;
        if (orientation.IsAxisInViewSpace)
          axis = _cameraPose.ToWorldDirection(axis);

        if (1 - Vector3F.Dot(normal, axis) < Numeric.EpsilonF)
        {
          // Normal and axis are parallel.
          // --> Bend normal by adding a fraction of the camera down vector.
          Vector3F cameraDown = -_cameraPose.Orientation.GetColumn(1);
          normal += cameraDown * 0.001f;
          normal.Normalize();
        }

        // Compute right.
        //Vector3F right = Vector3F.Cross(axis, normal);
        // Inlined:
        Vector3F right;
        right.X = axis.Y * normal.Z - axis.Z * normal.Y;
        right.Y = axis.Z * normal.X - axis.X * normal.Z;
        right.Z = axis.X * normal.Y - axis.Y * normal.X;
        if (!right.TryNormalize())
          right = normal.Orthonormal1;   // Normal and axis are parallel --> Choose random perpendicular vector.

        if (orientation.IsAxisFixed)
        {
          // Make sure normal is perpendicular to right and up.
          //normal = Vector3F.Cross(right, axis);
          // Inlined:
          normal.X = right.Y * axis.Z - right.Z * axis.Y;
          normal.Y = right.Z * axis.X - right.X * axis.Z;
          normal.Z = right.X * axis.Y - right.Y * axis.X;

          // No need to normalize because right and up are normalized and perpendicular.
        }
        else
        {
          // Make sure axis is perpendicular to normal and right.
          //axis = Vector3F.Cross(normal, right);
          // Inlined:
          axis.X = normal.Y * right.Z - normal.Z * right.Y;
          axis.Y = normal.Z * right.X - normal.X * right.Z;
          axis.Z = normal.X * right.Y - normal.Y * right.X;

          // No need to normalize because normal and right are normalized and perpendicular.
        }
        #endregion

        _textEffect.World = new Matrix(right.X, right.Y, right.Z, 0,
                                       -axis.X, -axis.Y, -axis.Z, 0,
                                       normal.X, normal.Y, normal.Z, 0,
                                       position.X, position.Y, position.Z, 1);

        Vector3F color3F = node.Color * billboard.Color;
        float alpha = node.Alpha * billboard.Alpha;
        Color color = new Color(color3F.X * alpha,
                                color3F.Y * alpha,
                                color3F.Z * alpha,
                                alpha);

        Vector2 size = (text != null) ? font.MeasureString(text) : font.MeasureString(stringBuilder);
        Vector2 origin = size / 2;
        float scale = node.ScaleWorld.Y; // Assume uniform scale.

        _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, graphicsDevice.DepthStencilState, RasterizerState.CullNone, _textEffect);
        if (text != null)
          _spriteBatch.DrawString(font, text, Vector2.Zero, color, 0, origin, scale, SpriteEffects.None, 0);
        else
          _spriteBatch.DrawString(font, stringBuilder, Vector2.Zero, color, 0, origin, scale, SpriteEffects.None, 0);

        _spriteBatch.End();
      }

      savedRenderStates.Restore();
    }
    #endregion
  }
}
