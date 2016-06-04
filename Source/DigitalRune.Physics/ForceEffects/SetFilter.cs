// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

//using System.Collections.Generic;

//#if NETFX_CORE || WP7 || XBOX
//using DigitalRune.Collections;
//#endif


//namespace DigitalRune.Physics
//{
//  /// <summary>
//  /// Manages a <see cref="Set"/> of objects and checks if objects are in the set.
//  /// </summary>
//  /// <typeparam name="T">The type of filtered objects.</typeparam>
//  /// <remarks>
//  /// <see cref="Filter"/> returns <see langword="true"/> or <see langword="false"/> depending on
//  /// whether an object is in the <see cref="Set"/>. The mode can be set to <see cref="IsExcluding"/>:
//  /// in this case <see cref="Filter"/> returns <see langword="false"/> for all set objects and 
//  /// <see langword="true"/> for objects not in the set. ("All set objects are excluded.") 
//  /// The mode can be inverted: If <see cref="IsExcluding"/> is <see langword="false"/>, the 
//  /// <see cref="Filter"/> method returns only <see langword="true"/> if an object is in the set. 
//  /// ("All set objects are included.")
//  /// </remarks>
//  public class SetFilter<T>
//  {
//    /// <summary>
//    /// Gets or sets a value indicating whether <see cref="Filter"/> returns <see langword="true"/>
//    /// for all set objects or for all non-set objects.
//    /// </summary>
//    /// <value>
//    /// <see langword="true"/> if the <see cref="Filter"/> method returns <see langword="false"/> 
//    /// for all objects in the set; otherwise, <see langword="false"/> if the <see cref="Filter"/>
//    /// method returns <see langword="true"/> for all objects in the set.
//    /// </value>
//    public bool IsExcluding { get; set; }


//    /// <summary>
//    /// Gets the set of objects.
//    /// </summary>
//    /// <value>The set.</value>
//    /// <remarks>
//    /// The result of <see cref="Filter"/> depends on whether an object is in this set or not.
//    /// </remarks>
//    public HashSet<T> Set { get; private set; }


//    /// <summary>
//    /// Initializes a new instance of the <see cref="SetFilter&lt;T&gt;"/> class.
//    /// </summary>
//    /// <param name="isExcluding">
//    /// If set to <see langword="true"/> the <see cref="Filter"/> method will return 
//    /// <see langword="false"/> for objects in the <see cref="Set"/>. If set to <see langword="false"/>
//    /// the <see cref="Filter"/> method will return <see langword="true"/> for objects in the
//    /// <see cref="Set"/>.
//    /// </param>
//    public SetFilter(bool isExcluding)
//    {
//      IsExcluding = isExcluding;
//      Set = new HashSet<T>();
//    }


//    /// <summary>
//    /// Returns <see langword="true"/> or <see langword="false"/> for the given object.
//    /// </summary>
//    /// <param name="obj">The object that should be filtered.</param>
//    /// <returns>
//    /// <see langword="true"/> or <see langword="false"/>. The meaning is defined by the owner
//    /// of the filter.
//    /// </returns>
//    /// <remarks>
//    /// This method returns <see langword="true"/> or <see langword="false"/> depending on whether
//    /// the object is in the <see cref="Set"/>. If <see cref="IsExcluding"/> is <see langword="false"/>,
//    /// this method returns <see langword="true"/> for all objects in the <see cref="Set"/>:
//    /// "All set objects are included." If <see cref="IsExcluding"/> is set to <see langword="true"/>,
//    /// this method returns <see langword="false"/> for all objects in the <see cref="Set"/>:
//    /// "All set objects are excluded."
//    /// </remarks>
//    public bool Filter(T obj)
//    {
//      if (IsExcluding)
//        return !Set.Contains(obj);
//      else
//        return Set.Contains(obj);
//    }
//  }
//}
