// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
using System;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Animation.Content
{
  /// <summary>
  /// Reads a <see cref="KeyFrameCollection{T}"/> from binary format. 
  /// (Only available in the XNA-compatible build.)
  /// </summary>
  /// <typeparam name="T">The type of the animation value.</typeparam>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Animation.dll.
  /// </remarks>
  public class KeyFrameCollectionReader<T> : ContentTypeReader<KeyFrameCollection<T>>
  {
#if !MONOGAME
    /// <summary>
    /// Determines if deserialization into an existing object is possible.
    /// </summary>
    public override bool CanDeserializeIntoExistingObject
    {
      get { return true; }
    }
#endif


      /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    protected override KeyFrameCollection<T> Read(ContentReader input, KeyFrameCollection<T> existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new KeyFrameCollection<T>();
      else
        existingInstance.Clear();

      var count = input.ReadInt32();
      for (int i = 0; i < count; i++)
      {
        TimeSpan time = input.ReadRawObject<TimeSpan>();
        T value = input.ReadRawObject<T>();
        existingInstance.Add(new KeyFrame<T>(time, value));
      }
      
      return existingInstance;
    }
  }
}
