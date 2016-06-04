// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;

#if !POOL_ENUMERABLES
using System.Linq;
#endif


namespace DigitalRune.Geometry.Partitioning
{
  partial class DualPartition<T>
  {
    /// <inheritdoc/>
    public override IEnumerable<T> GetOverlaps(Aabb aabb)
    {
      UpdateInternal();
      var overlapsStatic = StaticPartition.GetOverlaps(aabb);
      var overlapsDynamic = DynamicPartition.GetOverlaps(aabb);

#if !POOL_ENUMERABLES
      return overlapsStatic.Concat(overlapsDynamic);
#else
      return ConcatWork<T>.Create(overlapsStatic, overlapsDynamic);
#endif
    }


    /// <inheritdoc/>
    public override IEnumerable<T> GetOverlaps(Ray ray)
    {
      UpdateInternal();
      var overlapsStatic = StaticPartition.GetOverlaps(ray);
      var overlapsDynamic = DynamicPartition.GetOverlaps(ray);

#if !POOL_ENUMERABLES
      return overlapsStatic.Concat(overlapsDynamic);
#else
      return ConcatWork<T>.Create(overlapsStatic, overlapsDynamic);
#endif
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public IEnumerable<T> GetOverlaps(IList<Plane> planes)
    {
      UpdateInternal();

      var staticPartition = StaticPartition as ISupportFrustumCulling<T>;
      var dynamicPartition = DynamicPartition as ISupportFrustumCulling<T>;
      if (staticPartition == null || dynamicPartition == null)
        throw new NotSupportedException("Both the static partition and the dynamic partition need to implement ISuppportFrustumCulling<T>.");

      var overlapsStatic = staticPartition.GetOverlaps(planes);
      var overlapsDynamic = dynamicPartition.GetOverlaps(planes);

#if !POOL_ENUMERABLES
      return overlapsStatic.Concat(overlapsDynamic);
#else
      return ConcatWork<T>.Create(overlapsStatic, overlapsDynamic);
#endif
    }
  }
}
