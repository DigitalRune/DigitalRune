// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Chains together a group of post-processors.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Post-processors can be added to a <see cref="PostProcessorChain"/>. A post-processor chain is 
  /// itself a post-processor. When the post-processor chain is executed (by calling 
  /// <see cref="PostProcessor.Process"/>), it will automatically call 
  /// <see cref="PostProcessor.Process"/> of all contained post-processors.
  /// </para>
  /// <para>
  /// By default, a post-processor in a chain reads the output of the previous post-processor and 
  /// writes the result into an intermediate render target. 
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  [DebuggerDisplay("{GetType().Name,nq}(Count = {Count})")]
  [DebuggerTypeProxy(typeof(PostProcessorCollectionView))]
  public class PostProcessorChain : PostProcessor, IList<PostProcessor>
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // This view is used as DebuggerTypeProxy. With this, the debugger will display 
    // a readable list of post-processors for the PostProcessorChain.
    internal sealed class PostProcessorCollectionView
    {
      private readonly ICollection<PostProcessor> _collection;
      public PostProcessorCollectionView(ICollection<PostProcessor> collection)
      {
        _collection = collection;
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public PostProcessor[] PostProcessors
      {
        get { return _collection.ToArray(); }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly List<PostProcessor> _processors;
    private readonly List<PostProcessor> _processorsCopy;
    private bool _processorsCopyDirty = true;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the number of post-processors contained in the post-processor chain.
    /// </summary>
    /// <value>The number of post-processors contained in the post-processor chain.</value>
    public int Count
    {
      get { return _processors.Count; }
    }


    /// <summary>
    /// Gets a value indicating whether this collection is read-only.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this collection is read-only; otherwise, <see langword="false"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool ICollection<PostProcessor>.IsReadOnly
    {
      get { return false; }
    }


    /// <summary>
    /// Gets or sets the post-processor at the specified index.
    /// </summary>
    /// <value>The post-processor at the specified index.</value>
    /// <param name="index">The zero-based index of the post-processor to get or set.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or equal to or greater than <see cref="Count"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>. The <see cref="PostProcessorChain"/> 
    /// does not allow <see langword="null"/> values.
    /// </exception>
    public PostProcessor this[int index]
    {
      get { return _processors[index]; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _processors[index] = value;
        _processorsCopyDirty = true;
      }
    }


    /// <summary>
    /// Gets the <see cref="PostProcessor"/> with the specified name.
    /// </summary>
    /// <param name="name">The name of the post-processor.</param>
    /// <value>
    /// The post-processor with the given name, or <see langword="null"/> if no match was found.
    /// </value>
    public PostProcessor this[string name]
    {
      get
      {
        foreach (var processor in _processors)
          if (processor.Name == name)
            return processor;

        return null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PostProcessorChain"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public PostProcessorChain(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _processors = new List<PostProcessor>();
      _processorsCopy = new List<PostProcessor>();
      _processorsCopyDirty = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- IEnumerable<T> -----

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<PostProcessor> IEnumerable<PostProcessor>.GetEnumerator()
    {
      return _processors.GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the post-processor chain. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for post-processor chain.
    /// </returns>
    public List<PostProcessor>.Enumerator GetEnumerator()
    {
      return _processors.GetEnumerator();
    }
    #endregion


    #region ----- ICollection<T> -----

    /// <summary>
    /// Appends a post-processor to the post-processor chain.
    /// </summary>
    /// <param name="postProcessor">
    /// The post-processor to add to the post-processor chain.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="postProcessor"/> is <see langword="null"/>. The post-processor chain does 
    /// not allow <see langword="null"/> values.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
    public void Add(PostProcessor postProcessor)
    {
      if (postProcessor == null)
        throw new ArgumentNullException("postProcessor");

      _processors.Add(postProcessor);
      _processorsCopyDirty = true;
    }


    /// <summary>
    /// Removes all post-processors from the post-processor chain.
    /// </summary>
    public void Clear()
    {
      _processors.Clear();
      _processorsCopyDirty = true;
    }


    /// <summary>
    /// Determines whether the post-processor chain contains a specific post-processor.
    /// </summary>
    /// <param name="postProcessor">The post-processor to locate in the post-processor chain.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="postProcessor"/> is found in the 
    /// post-processor chain; otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
    public bool Contains(PostProcessor postProcessor)
    {
      return _processors.Contains(postProcessor);
    }


    /// <summary>
    /// Copies the elements of the post-processor chain to an <see cref="Array"/>, starting 
    /// at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// post-processor chain. The <see cref="Array"/> must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="array"/> is multidimensional. Or <paramref name="arrayIndex"/> is equal to 
    /// or greater than the length of <paramref name="array"/>. Or the number of elements in the 
    /// source post-processor chain is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    void ICollection<PostProcessor>.CopyTo(PostProcessor[] array, int arrayIndex)
    {
      _processors.CopyTo(array, arrayIndex);
    }


    /// <summary>
    /// Removes the first occurrence of a specific post-processor from the post-processor chain.
    /// </summary>
    /// <param name="postProcessor">
    /// The post-processor to remove from the post-processor chain.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="postProcessor"/> was successfully removed from the 
    /// post-processor chain; otherwise, <see langword="false"/>. This method also returns 
    /// <see langword="false"/> if <paramref name="postProcessor"/> is not found in the original 
    /// post-processor chain.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
    public bool Remove(PostProcessor postProcessor)
    {
      _processorsCopyDirty = true;
      return _processors.Remove(postProcessor);
    }
    #endregion


    #region ----- IList<T> -----

    /// <summary>
    /// Determines the index of a specific post-processor in the post-processor chain.
    /// </summary>
    /// <param name="postProcessor">The post-processor to locate in the post-processor chain.</param>
    /// <returns>
    /// The index of <paramref name="postProcessor"/> if found in the post-processor chain; 
    /// otherwise, -1.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
    public int IndexOf(PostProcessor postProcessor)
    {
      return _processors.IndexOf(postProcessor);
    }


    /// <summary>
    /// Inserts a post-processor into the post-processor chain at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="postProcessor"/> should be inserted.
    /// </param>
    /// <param name="postProcessor">
    /// The post-processor to insert into the post-processor chain.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the post-processor chain.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="postProcessor"/> is <see langword="null"/>. The post-processor chain does 
    /// not allow <see langword="null"/> values.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
    public void Insert(int index, PostProcessor postProcessor)
    {
      if (postProcessor == null)
        throw new ArgumentNullException("postProcessor");

      _processors.Insert(index, postProcessor);
      _processorsCopyDirty = true;
    }


    /// <summary>
    /// Removes the post-processor at the specified index from the post-processor chain.
    /// </summary>
    /// <param name="index">The zero-based index of the post-processor to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the post-processor chain.
    /// </exception>
    public void RemoveAt(int index)
    {
      _processors.RemoveAt(index);
      _processorsCopyDirty = true;
    }
    #endregion


    /// <overloads>
    /// <summary>
    /// Determines whether the post-processor chain contains a specific post-processor.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the post-processor chain contains a post-processor with the specified
    /// name.
    /// </summary>
    /// <param name="name">
    /// The name of the post-processor to locate in the post-processor chain.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a post-processor with the given name is found in the 
    /// post-processor chain; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(string name)
    {
      foreach (var processor in _processors)
        if (processor.Name == name)
          return true;

      return false;
    }


    /// <overloads>
    /// <summary>
    /// Determines the index of a specific post-processor in the post-processor chain.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines the index of the post-processor with the specified name in the post-processor 
    /// chain.
    /// </summary>
    /// <param name="name">
    /// The name of the post-processor to locate in the post-processor chain.
    /// </param>
    /// <returns>
    /// The index of the post-processor if found in the post-processor chain; otherwise, -1.
    /// </returns>
    public int IndexOf(string name)
    {
      for (int i = 0; i < _processors.Count; i++)
        if (_processors[i].Name == name)
          return i;

      return -1;
    }


    private void UpdateProcessorsCopy()
    {
      if (_processorsCopyDirty)
      {
        _processorsCopy.Clear();
        foreach (var item in _processors)
          _processorsCopy.Add(item);

        _processorsCopyDirty = false;
      }
    }


    /// <inheritdoc/>
    protected override void OnProcess(RenderContext context)
    {
      UpdateProcessorsCopy();
      Process(_processorsCopy, context);

      if (_processorsCopyDirty)
      {
        // Collection is dirty. Maybe an item has been removed. 
        // --> Remove all references to avoid mem-leaks.
        _processorsCopy.Clear();
      }
    }


    /// <summary>
    /// Performs post-processing using the specified collection of processors. 
    /// </summary>
    /// <param name="processors">The post-processors.</param>
    /// <param name="context">The render context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    private static void Process(IList<PostProcessor> processors, RenderContext context)
    {
      Debug.Assert(processors != null);
      Debug.Assert(context != null);
      Debug.Assert(context.SourceTexture != null);

      if (context == null)
        throw new ArgumentNullException("context");

      var graphicsService = context.GraphicsService;
      var renderTargetPool = graphicsService.RenderTargetPool;

      var originalSourceTexture = context.SourceTexture;
      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;

      // Some normal post-processors can be used with any blend state. In a chain
      // alpha blending does not make sense.
      graphicsService.GraphicsDevice.BlendState = BlendState.Opaque;

      // Intermediate render targets for ping-ponging.
      // TODO: Use the originalRenderTarget in the ping-ponging.
      // (Currently, we create up to 2 temp targets. If the originalRenderTarget is not null,
      // and if the viewport is the whole target, then we could use the originalRenderTarget
      // in the ping-ponging. But care must be taken that the originalRenderTarget is never
      // used as the output for the post-processor before the last post-processor...)
      RenderTarget2D tempSource = null;
      RenderTarget2D tempTarget = null;

      // The size and format for intermediate render target is determined by the source image.
      var tempFormat = new RenderTargetFormat(originalSourceTexture)
      {
        Mipmap = false,
        DepthStencilFormat = DepthFormat.None,
      };

      // Remember if any processor has written into target.
      bool targetWritten = false;

      // Execute all processors.
      var numberOfProcessors = processors.Count;
      for (int i = 0; i < numberOfProcessors; i++)
      {
        var processor = processors[i];
        if (!processor.Enabled)
          continue;

        // Find effective output target:
        // If this processor is the last, then we render into the user-defined target. 
        // If this is not the last processor, then we use an intermediate buffer.
        if (IsLastOutputProcessor(processors, i))
        {
          context.RenderTarget = originalRenderTarget;
          context.Viewport = originalViewport;
          targetWritten = true;
        }
        else
        {
          // This is an intermediate post-processor, so we need an intermediate target.
          // If we have one, does it still have the correct format? If not, recycle it.
          if (tempTarget != null && !processor.DefaultTargetFormat.IsCompatibleWith(tempFormat))
          {
            renderTargetPool.Recycle(tempTarget);
            tempTarget = null;
          }

          if (tempTarget == null)
          {
            // Get a new render target. 
            // The format that the processor wants has priority. The current format 
            // is the fallback.
            tempFormat = new RenderTargetFormat(
              processor.DefaultTargetFormat.Width ?? tempFormat.Width,
              processor.DefaultTargetFormat.Height ?? tempFormat.Height,
              processor.DefaultTargetFormat.Mipmap ?? tempFormat.Mipmap,
              processor.DefaultTargetFormat.SurfaceFormat ?? tempFormat.SurfaceFormat,
              processor.DefaultTargetFormat.DepthStencilFormat ?? tempFormat.DepthStencilFormat);
            tempTarget = renderTargetPool.Obtain2D(tempFormat);
          }

          context.RenderTarget = tempTarget;
          context.Viewport = new Viewport(0, 0, tempFormat.Width.Value, tempFormat.Height.Value);
        }

        processor.ProcessInternal(context);

        context.SourceTexture = context.RenderTarget;

        // If we have rendered into tempTarget, then we remember it in tempSource 
        // and reuse the render target in tempSource if any is set.
        if (context.RenderTarget == tempTarget)
          Mathematics.MathHelper.Swap(ref tempSource, ref tempTarget);
      }

      // If there are no processors, or no processor is enabled, then we have to 
      // copy the source to the target manually.
      if (!targetWritten)
        graphicsService.GetCopyFilter().ProcessInternal(context);

      context.SourceTexture = originalSourceTexture;

      // The last processor should have written into the original target.
      Debug.Assert(context.RenderTarget == originalRenderTarget);

      renderTargetPool.Recycle(tempSource);
      renderTargetPool.Recycle(tempTarget);
    }


    /// <summary>
    /// Returns <see langword="true"/> if the given processor is the last processor that renders 
    /// into the back buffer. 
    /// </summary>
    private static bool IsLastOutputProcessor(IList<PostProcessor> processors, int processorIndex)
    {
      // Return false if there is a post-processor after the given index which is enabled.
      int numberOfProcessors = processors.Count;
      for (int i = processorIndex + 1; i < numberOfProcessors; i++)
      {
        var processor = processors[i];
        if (processor.Enabled)
          return false;
      }

      return true;
    }
    #endregion
  }
}
#endif
