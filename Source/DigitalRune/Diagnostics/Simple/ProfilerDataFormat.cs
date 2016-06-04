// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Diagnostics
{
  /// <summary>
  /// Provides formatting data for <see cref="ProfilerData"/>.
  /// </summary>
  internal sealed class ProfilerDataFormat
  {
    /// <summary>
    /// The default data format that can be used if the user has not specified a custom format.
    /// </summary>
    internal static readonly ProfilerDataFormat Default = new ProfilerDataFormat();


    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    public string Description { get; set; }


    /// <summary>
    /// Gets or sets the scale.
    /// </summary>
    /// <value>The scale.</value>
    public double Scale { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ProfilerDataFormat"/> class.
    /// </summary>
    public ProfilerDataFormat()
    {
      Scale = 1;
      Description = string.Empty;
    }
  }
}
