// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Animation.Content
{
  /// <summary>
  /// Reads a <see cref="TimelineGroup"/> from binary format. 
  /// (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Animation.dll.
  /// </remarks>
  public class TimelineGroupReader : ContentTypeReader<TimelineGroup>
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
    protected override TimelineGroup Read(ContentReader input, TimelineGroup existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new TimelineGroup();
      else
        existingInstance.Clear();

      existingInstance.FillBehavior = input.ReadRawObject<FillBehavior>();
      if (input.ReadBoolean())
        existingInstance.TargetObject = input.ReadString();

      var count = input.ReadInt32();

      for (int i = 0; i < count; i++)
      {
        existingInstance.Add(DummyTimeline.Instance);
        int index = i;
        input.ReadSharedResource<ITimeline>(t => existingInstance[index] = t);
      }

      return existingInstance;
    }
  }
}
