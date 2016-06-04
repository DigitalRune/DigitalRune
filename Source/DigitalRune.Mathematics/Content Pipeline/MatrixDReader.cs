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
  /// Reads a <see cref="MatrixD"/> from binary format. (Only available in the XNA-compatible
  /// build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Mathematics.dll.
  /// </remarks>
  public class MatrixDReader : ContentTypeReader<MatrixD>
  {
    /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override MatrixD Read(ContentReader input, MatrixD existingInstance)
    {
      int numberOfRows = input.ReadInt32();
      int numberOfColumns = input.ReadInt32();

      if (existingInstance == null)
      {
        existingInstance = new MatrixD(numberOfRows, numberOfColumns);
      }
      else
      {
        // Check if we can read the data into the existing object.
        if (existingInstance.NumberOfRows != numberOfRows || existingInstance.NumberOfColumns != numberOfColumns)
        {
          string message = String.Format(
            CultureInfo.InvariantCulture,
            "Cannot load {0}x{1} MatrixD into existing {2}x{3} MatrixD.",
            numberOfRows,
            numberOfColumns,
            existingInstance.NumberOfRows,
            existingInstance.NumberOfColumns);

          throw new ContentLoadException(message);
        }
      }

      for (int row = 0; row < numberOfRows; row++)
        for (int column = 0; column < numberOfColumns; column++)
          existingInstance[row, column] = input.ReadDouble();

      return existingInstance;
    }
  }
}
