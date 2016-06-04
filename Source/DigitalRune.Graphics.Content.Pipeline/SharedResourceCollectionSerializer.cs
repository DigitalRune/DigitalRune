// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

//using System;
//using System.Collections.Generic;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;


//namespace DigitalRune.Graphics.Content.Pipeline
//{
//  /// <summary>
//  /// Serializes a collection of shared resources.
//  /// </summary>
//  /// <typeparam name="TCollection">The type of the collection.</typeparam>
//  /// <typeparam name="TElement">The element type of the collection.</typeparam>
//  /// <remarks>
//  /// See http://blogs.msdn.com/shawnhar/archive/2008/11/20/serializing-collections-of-shared-resources.aspx.
//  /// </remarks>
//  internal abstract class SharedResourceCollectionSerializer<TCollection, TElement> : ContentTypeSerializer<TCollection>
//    where TCollection : IEnumerable<TElement>
//    where TElement : class
//  {
//    private readonly ContentSerializerAttribute _elementFormat;


//    /// <summary>
//    /// Initializes a new instance of the 
//    /// <see cref="SharedResourceCollectionSerializer{TCollection, TElement}"/> class.
//    /// </summary>
//    /// <param name="elementName">The name of the element.</param>
//    protected SharedResourceCollectionSerializer(string elementName)
//    {
//      _elementFormat = new ContentSerializerAttribute { ElementName = elementName };
//    }


//    /// <summary>
//    /// Serializes an object to intermediate XML format.
//    /// </summary>
//    /// <param name="output">
//    /// Specifies the intermediate XML location, and provides various serialization helpers.
//    /// </param>
//    /// <param name="collection">The strongly typed object to be serialized.</param>
//    /// <param name="format">Specifies the content format for this object.</param>
//    protected override void Serialize(IntermediateWriter output, TCollection collection, ContentSerializerAttribute format)
//    {
//      if (output == null)
//        throw new ArgumentNullException("output");

//      foreach (TElement element in collection)
//        output.WriteSharedResource(element, _elementFormat);
//    }


//    /// <summary>
//    /// Deserializes a collection of shared resources.
//    /// </summary>
//    /// <param name="input">Intermediate XML file.</param>
//    /// <param name="collection">The collection.</param>
//    protected void Deserialize(IntermediateReader input, IList<TElement> collection)
//    {
//      if (input == null)
//        throw new ArgumentNullException("input");

//      if (collection == null)
//        throw new ArgumentNullException("collection");

//      while (input.MoveToElement(_elementFormat.ElementName))
//      {
//        int insertionIndex = collection.Count;

//        // Add a dummy item 
//        TElement item = default(TElement);
//        collection.Add(item);

//        // Replace the dummy element with the deserialized element.
//        input.ReadSharedResource(_elementFormat, (TElement element) => collection[insertionIndex] = element);
//      }
//    }
//  }
//}
