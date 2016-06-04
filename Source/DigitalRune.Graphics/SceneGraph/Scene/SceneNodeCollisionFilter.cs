// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Collections;
using DigitalRune.Geometry.Partitioning;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Filters collisions between scene nodes using their group IDs.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This filter uses the scene node groups (see <see cref="SceneGraph.Scene.SetGroup"/> and 
  /// <see cref="SceneGraph.Scene.GetGroup"/>) to decide if a pair of scene nodes can "collide".
  /// Per default all collisions are enabled. <see cref="Set"/> can be used to disable collisions
  /// between two groups.
  /// </para>
  /// </remarks>
  public class SceneNodeCollisionFilter : IPairFilter<SceneNode>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Use jagged array [i][j] for lower triangular matrix where i ≥ j.
    private readonly bool[][] _groupPairFlags;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the scene.
    /// </summary>
    /// <value>The scene.</value>
    public Scene Scene { get; private set; }


    /// <summary>
    /// The maximum number of supported scene node groups.
    /// </summary>
    /// <remarks>
    /// Scene node group numbers must be in the range 0 - (<see cref="MaxNumberOfGroups"/> - 1).
    /// This limit can be changed in the constructor.
    /// </remarks>
    public int MaxNumberOfGroups
    {
      get { return _groupPairFlags.Length; }
    }


    /// <summary>
    /// Occurs when the filter rules were changed.
    /// </summary>
    public event EventHandler<EventArgs> Changed;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneNodeCollisionFilter"/> class.
    /// </summary>
    /// <param name="scene">The scene.</param>
    public SceneNodeCollisionFilter(Scene scene)
      : this(scene, 10)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SceneNodeCollisionFilter"/> class.
    /// </summary>
    /// <param name="scene">The scene.</param>
    /// <param name="maxNumberOfGroups">
    /// The maximum number of groups (see <see cref="MaxNumberOfGroups"/>).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="scene"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxNumberOfGroups"/> is negative.
    /// </exception>
    public SceneNodeCollisionFilter(Scene scene, int maxNumberOfGroups)
    {
      if (scene == null)
        throw new ArgumentNullException("scene");
      if (maxNumberOfGroups < 0)
        throw new ArgumentOutOfRangeException("maxNumberOfGroups", "The max number of collision groups must be greater than or equal to 0.");

      Scene = scene;
      _groupPairFlags = new bool[maxNumberOfGroups][];
      for (int i = 0; i < maxNumberOfGroups; i++)
        _groupPairFlags[i] = new bool[i + 1];

      // Per default all collisions are enabled.
      ResetInternal();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Resets this filter. All collisions will be enabled.
    /// </summary>
    public virtual void Reset()
    {
      ResetInternal();
      OnChanged(EventArgs.Empty);
    }


    private void ResetInternal()
    {
      for (int i = 0; i < _groupPairFlags.Length; i++)
      {
        var row = _groupPairFlags[i];
        for (int j = 0; j < row.Length; j++)
          row[j] = true;
      }
    }


    /// <summary>
    /// Enables or disables collisions between a pair of scene node groups.
    /// </summary>
    /// <param name="groupA">The first group.</param>
    /// <param name="groupB">The second group.</param>
    /// <param name="collisionsEnabled">
    /// If set to <see langword="true"/> collisions between scene nodes in <paramref name="groupA"/> 
    /// and scene nodes in <paramref name="groupB"/> are enabled. 
    /// Use <see langword="false"/> to disable collisions.
    /// </param>
    /// <remarks>
    /// To disable collisions for objects within one group, this method can be called with 
    /// <paramref name="groupA"/> == <paramref name="groupB"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="groupA"/> is out of range.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="groupB"/> is out of range.
    /// </exception>
    public void Set(int groupA, int groupB, bool collisionsEnabled)
    {
      if (groupA < 0 || groupA > MaxNumberOfGroups - 1)
        throw new ArgumentOutOfRangeException("groupA", String.Format(CultureInfo.InvariantCulture, "The number of the scene node group must be a value from 0 to {0}.", MaxNumberOfGroups - 1));
      if (groupB < 0 || groupB > MaxNumberOfGroups - 1)
        throw new ArgumentOutOfRangeException("groupB", String.Format(CultureInfo.InvariantCulture, "The number of the scene node group must be a value from 0 to {0}.", MaxNumberOfGroups - 1));

      if (groupA >= groupB)
        _groupPairFlags[groupA][groupB] = collisionsEnabled;
      else
        _groupPairFlags[groupB][groupA] = collisionsEnabled;

      OnChanged(EventArgs.Empty);
    }


    /// <summary>
    /// Returns <see langword="true"/> if collisions between two scene node groups are enabled.
    /// </summary>
    /// <param name="groupA">The first scene node group.</param>
    /// <param name="groupB">The second scene node group.</param>
    /// <returns>
    /// <see langword="true"/> if collisions with the between <paramref name="groupA"/> and 
    /// <paramref name="groupB"/> are enabled; otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="groupA"/> or <paramref name="groupB"/> is out of range.
    /// </exception>
    public bool Get(int groupA, int groupB)
    {
      if (groupA >= groupB)
        return _groupPairFlags[groupA][groupB];
      else
        return _groupPairFlags[groupB][groupA];
    }


    /// <summary>
    /// Determines whether the given <see cref="SceneNode"/>s can collide.
    /// </summary>
    /// <param name="pair">The pair of collision objects.</param>
    /// <returns>
    /// <see langword="true"/> if the pair of collision objects can collide; otherwise, 
    /// <see langword="false"/> if the objects cannot collide.
    /// </returns>
    public virtual bool Filter(Pair<SceneNode> pair)
    {
      int groupA = Scene.GetGroup(pair.First);
      int groupB = Scene.GetGroup(pair.Second);

      return Get(groupA, groupB);
    }


    /// <summary>
    /// Raises the <see cref="Changed"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnChanged"/> in a derived
    /// class, be sure to call the base class's <see cref="OnChanged"/> method so that registered
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnChanged(EventArgs eventArgs)
    {
      var handler = Changed;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
