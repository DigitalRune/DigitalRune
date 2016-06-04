// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Provides statistics about the occlusion culling process.
  /// </summary>
  public class OcclusionCullingStatistics
  {
    /// <summary>
    /// Gets the number of occluders.
    /// </summary>
    /// <value>The number of occluders.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public int Occluders { get; internal set; }


    /// <summary>
    /// Gets the total number of objects that were tested in the last query.
    /// </summary>
    /// <value>The total number of objects that were tested in the last query.</value>
    public int ObjectsTotal { get; internal set; }


    /// <summary>
    /// Gets the number of objects that were visible in the last query.
    /// </summary>
    /// <value>The number of objects that were visible in the last query.</value>
    public int ObjectsVisible
    {
      get { return ObjectsTotal - ObjectsCulled; }
    }


    /// <summary>
    /// Gets the number of objects that were occluded in the last query.
    /// </summary>
    /// <value>The number of objects that were occluded in the last query.</value>
    public int ObjectsCulled { get; internal set; }


    /// <summary>
    /// Gets the total number of shadow casters that were tested in the last query.
    /// </summary>
    /// <value>The total number of shadow casters that were tested in the last query.</value>
    public int ShadowCastersTotal { get; internal set; }


    /// <summary>
    /// Gets the total number of shadow casters that were visible in the last query.
    /// </summary>
    /// <value>The total number of shadow casters that were visible in the last query.</value>
    public int ShadowCastersVisible
    {
      get { return ShadowCastersTotal - ShadowCastersCulled; }
    }


    /// <summary>
    /// Gets the number of shadow casters that were culled in the last query.
    /// </summary>
    /// <value>The number of shadow casters that were culled in the last query.</value>
    public int ShadowCastersCulled { get; internal set; }
  }
}
