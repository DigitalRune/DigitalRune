// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune
{
  /// <summary>
  /// Represents an object that supports resource pooling and can be recycled.
  /// </summary>
  public interface IRecyclable
  {
    /// <summary>
    /// Recycles this instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    void Recycle();
  }
}
