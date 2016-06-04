// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
#if ANIMATION
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
#endif


namespace DigitalRune.Graphics.Content.Pipeline
{
  partial class DRModelProcessor
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    private const int MaxBonesPerVertex = 4;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly int[] _tempIndices = new int[MaxBonesPerVertex];
    private readonly float[] _tempWeights = new float[MaxBonesPerVertex];
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /*
    /// <summary>
    /// Gets or sets a value indicating whether alpha premultiply of vertex color is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if alpha premultiply of vertex colors is enabled; otherwise, <see langword="false"/>.
    /// </value>
    [DefaultValue(true)]
    [DisplayName("Premultiply Vertex Colors")]
    [Description("If enabled, vertex color channels are converted to premultiplied alpha format.")]
    public virtual bool PremultiplyVertexColors
    {
      get { return _premultiplyVertexColors; }
      set { _premultiplyVertexColors = value; }
    }
    private bool _premultiplyVertexColors = true;
    */
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void ProcessVertexChannels(MeshContent mesh)
    {
      foreach (GeometryContent geometry in mesh.Geometry)
      {
        var channels = geometry.Vertices.Channels;
        foreach (var channel in channels.ToArray())
        {
          // Get current index. (ProcessVertexChannel could have modified the vertex
          // channel collection!)
          int channelIndex = channels.IndexOf(channel);
          if (channelIndex < 0)
            continue;

          ProcessVertexChannel(geometry, channelIndex);
        }
      }
    }


    private void ProcessVertexChannel(GeometryContent geometry, int channelIndex)
    {
      // Get the base name of a vertex channel (e.g. "Colors" for "Colors1").
      string baseName = VertexChannelNames.DecodeBaseName(geometry.Vertices.Channels[channelIndex].Name);
      if (baseName != null)
      {
        if (baseName == "Color")
          ProcessColorChannel(geometry, channelIndex);
        else if (baseName == "Weights")
          ProcessWeightsChannel(geometry, channelIndex);
      }
    }


    // Converts color channel from Vector4 to Color and premultiplies with alpha if required.
    private void ProcessColorChannel(GeometryContent geometry, int vertexChannelIndex)
    {
      var channels = geometry.Vertices.Channels;
      try
      {
        channels.ConvertChannelContent<Color>(vertexChannelIndex);
      }
      catch (NotSupportedException exception)
      {
        var channel = channels[vertexChannelIndex];
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Vertex channel \"{0}\" has wrong content type. Actual type: {1}. Expected type: {2}.",
          channel.Name, channel.ElementType, typeof(Vector4));
        throw new InvalidContentException(message, exception);
      }

      if (_modelDescription == null || _modelDescription.PremultiplyVertexColors)
      {
        var channel = channels.Get<Color>(vertexChannelIndex);
        for (int i = 0; i < channel.Count; i++)
        {
          Color color = channel[i];
          channel[i] = Color.FromNonPremultiplied(color.R, color.G, color.B, color.A);
        }
      }
    }


    // Converts a channel of type BoneWeightCollection to two new channels:
    // Byte4 indices + Vector4 weights
    private void ProcessWeightsChannel(GeometryContent geometry, int vertexChannelIndex)
    {
#if ANIMATION
      if (_skeleton == null)
      {
        // No skeleton? Remove BoneWeightCollection.
        geometry.Vertices.Channels.RemoveAt(vertexChannelIndex);
        return;
      }

      if (_skeleton.NumberOfBones > 255)
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Too many bones in skeleton. Actual number of bones: {0}. Allowed number of bones: {1}.",
          _skeleton.NumberOfBones, 255);
        throw new InvalidContentException(message, _rootBone.Identity);
      }

      var channels = geometry.Vertices.Channels;
      var channel = channels[vertexChannelIndex];
      var boneWeightChannel = channel as VertexChannel<BoneWeightCollection>;
      if (boneWeightChannel == null)
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Vertex channel \"{0}\" has wrong content type. Actual type: {1}. Expected type: {2}.",
          channel.Name, channel.ElementType, typeof(BoneWeightCollection));
        throw new InvalidContentException(message, geometry.Parent.Identity);
      }

      // Create two channels (Byte4 indices + Vector4 weights) from a BoneWeight channel.
      Byte4[] boneIndices = new Byte4[boneWeightChannel.Count];
      Vector4[] boneWeights = new Vector4[boneWeightChannel.Count];
      for (int i = 0; i < boneWeightChannel.Count; i++)
      {
        // Convert bone weights for vertex i.
        var boneWeightCollection = boneWeightChannel[i];
        if (boneWeightCollection == null)
        {
          string message = String.Format(
            CultureInfo.InvariantCulture,
            "NULL entry found in channel \"{0}\". Expected element type: {1}.",
            boneWeightChannel.Name, typeof(BoneWeightCollection));
          throw new InvalidContentException(message, geometry.Parent.Identity);
        }

        ConvertBoneWeights(boneWeightCollection, boneIndices, boneWeights, i, geometry);
      }

      // The current channel has the name "WeightsN", where N is the usage index.
      // Get the usage index.
      int usageIndex = VertexChannelNames.DecodeUsageIndex(boneWeightChannel.Name);

      // Store the converted bone information in two new channels called "BlendIndicesN"
      // and "BlendWeightsN".
      string blendIndices = VertexChannelNames.EncodeName(VertexElementUsage.BlendIndices, usageIndex);
      if (channels.Contains(blendIndices))
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot store converted blend indices for vertex channel \"{0}\", because a vertex channel called \"{1}\" already exists.",
          boneWeightChannel.Name, blendIndices);
        throw new InvalidContentException(message, geometry.Parent.Identity);
      }

      string blendWeights = VertexChannelNames.EncodeName(VertexElementUsage.BlendWeight, usageIndex);
      if (channels.Contains(blendWeights))
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot store converted blend weights for vertex channel \"{0}\", because a vertex channel called \"{1}\" already exists.",
          boneWeightChannel.Name, blendWeights);
        throw new InvalidContentException(message, geometry.Parent.Identity);
      }

      // Insert the new channels after "WeightsN" and remove "WeightsN".
      channels.Insert(vertexChannelIndex + 1, blendIndices, boneIndices);
      channels.Insert(vertexChannelIndex + 2, blendWeights, boneWeights);
      channels.RemoveAt(vertexChannelIndex);
#endif
    }


#if ANIMATION
    // Convert BoneWeightCollection to Byte4 (bone indices) and Vector4 (bone weights).
    private void ConvertBoneWeights(BoneWeightCollection boneWeightCollection, Byte4[] boneIndices, Vector4[] boneWeights, int vertexIndex, GeometryContent geometry)
    {
      // Normalize weights. (Number of weights should be MaxBonesPerVertex. Sum should be 1.)
      boneWeightCollection.NormalizeWeights(MaxBonesPerVertex);

      // Convert BoneWeights object to bone indices and bone weights.
      for (int i = 0; i < boneWeightCollection.Count; i++)
      {
        BoneWeight boneWeight = boneWeightCollection[i];
        int boneIndex = _skeleton.GetIndex(boneWeight.BoneName);
        if (boneIndex == -1)
        {
          string message = String.Format(
            CultureInfo.InvariantCulture,
            "Vertex references unknown bone name \"{0}\".",
            boneWeight.BoneName);
          throw new InvalidContentException(message, geometry.Parent.Identity);
        }

        _tempIndices[i] = boneIndex;
        _tempWeights[i] = boneWeight.Weight;
      }

      // Clear unused indices/weights.
      for (int i = boneWeightCollection.Count; i < MaxBonesPerVertex; i++)
      {
        _tempIndices[i] = 0;
        _tempWeights[i] = 0f;
      }

      boneIndices[vertexIndex] = new Byte4(_tempIndices[0], _tempIndices[1], _tempIndices[2], _tempIndices[3]);
      boneWeights[vertexIndex] = new Vector4(_tempWeights[0], _tempWeights[1], _tempWeights[2], _tempWeights[3]);
    }
#endif
    #endregion
  }
}
