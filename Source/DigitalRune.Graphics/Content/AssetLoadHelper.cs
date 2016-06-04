// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Helps to determine when an asset with shared resources is fully loaded.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Shared resources (see <see cref="ContentReader.ReadSharedResource{T}"/>) are loaded deferred.
  /// When the <see cref="ContentTypeReader.Read"/> method returns, shared resources might not yet
  /// be loaded. An <see cref="AssetLoadHelper"/> keeps track of the loading of shared resources for
  /// a specific asset. An <see cref="AssetLoadHelper"/> raises an event 
  /// <see cref="AssetLoadHelper.AssetLoaded"/> when all fix-up actions have been executed and all 
  /// shared resources have been loaded. This event can be used to initialize an asset as soon as it 
  /// is loaded including all shared resources.
  /// </para>
  /// <para>
  /// Each asset has a certain <see cref="AssetLoadHelper"/> which can be retrieved by calling 
  /// <see cref="Get"/>. Any fixup actions of the asset must be wrapped using <see cref="Fixup{T}"/>
  /// (see example below).
  /// </para>
  /// <para>
  /// <strong>WARNING:</strong> The <see cref="AssetLoadHelper"/> can only be used to track shared 
  /// resources when it is guaranteed that they have a value that is not <see langword="null"/>.
  /// This is a limitation by the XNA Framework. (When a shared resource is loaded by calling 
  /// <see cref="ContentReader.ReadSharedResource{T}"/> the fix-up action only gets executed, when 
  /// it has a value other than <see langword="null"/>. When the value is <see langword="null"/> the 
  /// fix-up action will never get executed and the <see cref="AssetLoadHelper"/> will never realize 
  /// that the loading of the asset has finished.)
  /// </para>
  /// </remarks>
  /// <example>
  /// In the <see cref="ContentTypeReader{T}"/> of a certain resource, the following code is called
  /// to load the shared resource. The method <c>myAsset.OnAssetLoaded</c> is called automatically
  /// once all shared resource have been loaded. <c>myAsset.OnAssetLoaded</c> can be used to 
  /// initialize the asset.
  /// <code lang="csharp">
  /// <![CDATA[
  /// internal class MyAssetReader : ContentTypeReader<MyAsset>
  /// {
  ///   protected override MyAsset Read(ContentReader input, MyAsset existingInstance)
  ///   {
  ///     if (existingInstance == null)
  ///       existingInstance = new MyAsset();
  /// 
  ///     ... load properties of MyAsset ...
  ///     
  ///     // Load shared resources:
  ///     // Use AssetLoadHelper to receive an event when the asset (including 
  ///     // all shared resources) is loaded.
  ///     using (var helper = AssetLoadHelper.Get(input.AssetName))
  ///     {
  ///       // When loading the shared resource, use AssetLoadHelper.Fixup() to 
  ///       // wrap the fix-up action.
  ///       input.ReadSharedResource(helper.Fixup<MyResource>(res => existingInstance.Resource = res));
  /// 
  ///       helper.AssetLoaded += existingInstance.OnAssetLoaded;
  ///     }
  /// 
  ///     return existingInstance;
  ///   }
  /// }
  /// ]]>
  /// </code>
  /// </example>
  public sealed class AssetLoadHelper : IDisposable
  {
    // Current uses of the AssetLoadHelper: 
    // - MeshNode: When a MeshNode is loaded, the MeshNode creates a MaterialInstance 
    //   for each Material in the mesh. But without the AssetLoadHelper.AssetLoaded 
    //   event the MeshNode does not know when the Materials in the Mesh are loaded. 
    //   MeshNode.Mesh is set BEFORE the fixups of the Mesh are all handled.
    // - LodGroupNode: When all LOD nodes are loaded, the bounding shape is updated.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // Wraps a fixup action and calls AssetLoadHelper.OnFixupExecuted when the 
    // fixup action was executed.
    internal sealed class FixupWrapper<T>
    {
      private readonly AssetLoadHelper _helper;
      private readonly Action<T> _fixup;

      public FixupWrapper(AssetLoadHelper helper, Action<T> fixup)
      {
        Debug.Assert(helper != null, "The AssetLoadHelper must not be null.");
        Debug.Assert(fixup != null, "The fixup action must not be null.");

        _helper = helper;
        _fixup = fixup;
      }

      public void Fixup(T resource)
      {
        _fixup(resource);
        _helper.OnFixupExecuted(this);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // A static register for (asset name, AssetLoadHelper) pairs.
    private static readonly Dictionary<string, AssetLoadHelper> AssetLoadHelpers = new Dictionary<string, AssetLoadHelper>();

    // Reference counting is used to determine whether the AssetLoadHelper is in use.
    private int _referenceCount;

    // A list of all fixups for the current asset. When a fixup is executed, the 
    // entry is replaced with null.
    private readonly List<object> _fixups = new List<object>();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the asset.
    /// </summary>
    /// <value>The name of the asset. (See <see cref="ContentReader.AssetName"/>.)</value>
    public string AssetName { get; private set; }


    /// <summary>
    /// Occurs after all fix-up actions were executed and the asset is fully loaded.
    /// </summary>
    public event EventHandler<EventArgs> AssetLoaded;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Prevents a default instance of the <see cref="AssetLoadHelper"/> class from being created.
    /// </summary>
    /// <param name="assetName">The name of the asset.</param>
    private AssetLoadHelper(string assetName)
    {
      AssetName = assetName;
    }


    /// <summary>
    /// Releases the <see cref="AssetLoadHelper"/>.
    /// </summary>
    public void Dispose()
    {
      RemoveReference();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the <see cref="AssetLoadHelper"/> for a specific asset.
    /// </summary>
    /// <param name="assetName">
    /// The name of the asset. (See <see cref="ContentReader.AssetName"/>.)
    /// </param>
    /// <returns>The <see cref="AssetLoadHelper"/> for the given asset.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="assetName"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="assetName"/> is empty.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static AssetLoadHelper Get(string assetName)
    {
      if (assetName == null)
        throw new ArgumentNullException("assetName");
      if (assetName.Length == 0)
        throw new ArgumentException("Asset name must not be empty.", "assetName");

      lock (AssetLoadHelpers)
      {
        AssetLoadHelper assetLoadHelper;
        if (!AssetLoadHelpers.TryGetValue(assetName, out assetLoadHelper))
        {
          // Create and register new helper.
          assetLoadHelper = new AssetLoadHelper(assetName);
          AssetLoadHelpers[assetName] = assetLoadHelper;
        }

        assetLoadHelper.AddReference();
        return assetLoadHelper;
      }
    }


    private void AddReference()
    {
      Interlocked.Increment(ref _referenceCount);
    }


    private void RemoveReference()
    {
      if (Interlocked.Decrement(ref _referenceCount) == 0)
        Release();
    }


    private void Release()
    {
      lock (AssetLoadHelpers)
      {
        if (_referenceCount == 0 && _fixups.Count == 0)
        {
          // Unregister this fixup helper and raise AssetLoaded event.
          if (AssetLoadHelpers.Remove(AssetName))
            OnAssetLoaded(EventArgs.Empty);
        }
      }
    }


    /// <summary>
    /// Wraps and registers a fix-up action.
    /// </summary>
    /// <typeparam name="T">The type of the shared resource.</typeparam>
    /// <param name="fixup">The fix-up action.</param>
    /// <returns>A new action that will call the fix-up action.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#")]
    public Action<T> Fixup<T>(Action<T> fixup)
    {
      Debug.Assert(_referenceCount > 0, "Fixups should only be added while AssetLoadHelper is referenced.");

      var wrapper = new FixupWrapper<T>(this, fixup);

      lock (_fixups)
        _fixups.Add(wrapper);

      return wrapper.Fixup;
    }


    private void OnFixupExecuted(object fixup)
    {
      // Remove completed fixup.
      int remaining;
      lock (_fixups)
      {
        _fixups.Remove(fixup);
        remaining = _fixups.Count;
      }

      // Release the AssetLoadHelper when all fixups have completed.
      // (Note: While AssetLoadHelper is referenced, new fixups might still be added.)
      if (remaining == 0 && _referenceCount == 0)
        Release();
    }


    /// <summary>
    /// Raises the <see cref="AssetLoaded"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnAssetLoaded"/> in a 
    /// derived class, be sure to call the base class's <see cref="OnAssetLoaded"/> method so that 
    /// registered delegates receive the event.
    /// </remarks>
    private void OnAssetLoaded(EventArgs eventArgs)
    {
      var handler = AssetLoaded;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
