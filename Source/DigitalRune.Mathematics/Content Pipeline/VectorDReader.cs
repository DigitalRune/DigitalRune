// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Mathematics.Algebra;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Mathematics.Content
{
  /// <summary>
  /// Reads a <see cref="VectorD"/> from binary format. (Only available in the XNA-compatible
  /// build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Mathematics.dll.
  /// </remarks>
  public class VectorDReader : ContentTypeReader<VectorD>
  {
    /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override VectorD Read(ContentReader input, VectorD existingInstance)
    {
      int numberOfElements = input.ReadInt32();

      if (existingInstance == null)
      {
        existingInstance = new VectorD(numberOfElements);
      }
      else
      {
        // Check if we can read the data into the existing object.
        if (existingInstance.NumberOfElements != numberOfElements)
        {
          string message = String.Format(
            CultureInfo.InvariantCulture,
            "Cannot load VectorD with {0} elements into existing VectorD with {1} elements.",
            numberOfElements,
            existingInstance.NumberOfElements);

          throw new ContentLoadException(message);
        }
      }

      for (int index = 0; index < numberOfElements; index++)
          existingInstance[index] = input.ReadDouble();

      return existingInstance;
    }
  }
}
