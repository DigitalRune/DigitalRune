// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Organizes a group of animations which can be played simultaneously.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Different animations can be grouped together to be executed simultaneously. The group of 
  /// animations is sometimes called "storyboard".
  /// </para>
  /// <para>
  /// <para>
  /// <strong>Nested Timelines:</strong> Timeline groups can be nested. That means a 
  /// <see cref="TimelineGroup"/> can contain other <see cref="TimelineGroup"/>s.
  /// </para>
  /// <strong>Important:</strong> Animations must not be added or removed while the timeline group 
  /// is playing!
  /// </para>
  /// </remarks>
  public class TimelineGroup : ITimeline, IList<ITimeline>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly List<ITimeline> _timelines;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------


    /// <summary>
    /// Gets or sets a value that specifies how the animation behaves when it reaches the end of its 
    /// duration.
    /// </summary>
    /// <value>
    /// A value that specifies how the animation behaves when it reaches the end of its duration.
    /// The default value is <see cref="Animation.FillBehavior.Hold"/>.
    /// </value>
    /// <inheritdoc cref="ITimeline.FillBehavior"/>
    public FillBehavior FillBehavior { get; set; }


    /// <summary>
    /// Gets or sets the object to which the animation is applied by default.
    /// </summary>
    /// <value>
    /// The object to which the animation is applied by default. The default value is 
    /// <see langword="null"/>.
    /// </value>
    /// <inheritdoc cref="ITimeline.TargetObject"/>
    public string TargetObject { get; set; }


    /// <summary>
    /// Gets the number of timelines contained in the <see cref="TimelineGroup"/>.
    /// </summary>
    /// <value>The number of timelines contained in the <see cref="TimelineGroup"/>.</value>
    public int Count
    {
      get { return _timelines.Count; }
    }


    /// <summary>
    /// Gets a value indicating whether this collection is read-only.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this collection is read-only; otherwise, <see langword="false"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool ICollection<ITimeline>.IsReadOnly
    {
      get { return false; }
    }


    /// <summary>
    /// Gets or sets the timeline at the specified index.
    /// </summary>
    /// <value>The timeline at the specified index.</value>
    /// <param name="index">The zero-based index of the timeline to get or set.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or equal to or greater than <see cref="Count"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>. The <see cref="TimelineGroup"/> does not
    /// allow <see langword="null"/> values.
    /// </exception>
    public ITimeline this[int index]
    {
      get { return _timelines[index]; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _timelines[index] = value;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TimelineGroup"/> class.
    /// </summary>
    public TimelineGroup()
    {
      _timelines = new List<ITimeline>();
      FillBehavior = FillBehavior.Hold;
      TargetObject = null;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public AnimationInstance CreateInstance()
    {
      var animationInstance = AnimationInstance.Create(this);
      foreach (var timeline in _timelines)
        animationInstance.Children.Add(timeline.CreateInstance());

      return animationInstance;
    }


    /// <inheritdoc/>
    public virtual TimeSpan? GetAnimationTime(TimeSpan time)
    {
      return AnimationHelper.GetAnimationTime(this, time);
    }


    /// <inheritdoc/>
    public AnimationState GetState(TimeSpan time)
    {
      return AnimationHelper.GetState(this, time);
    }


    /// <inheritdoc/>
    public TimeSpan GetTotalDuration()
    {
      TimeSpan duration = TimeSpan.Zero;
      foreach (var timeline in _timelines)
        duration = AnimationHelper.Max(duration, timeline.GetTotalDuration());

      return duration;
    }


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
    IEnumerator<ITimeline> IEnumerable<ITimeline>.GetEnumerator()
    {
      return _timelines.GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="TimelineGroup"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="TimelineGroup"/>.
    /// </returns>
    public List<ITimeline>.Enumerator GetEnumerator()
    {
      return _timelines.GetEnumerator();
    }


    /// <summary>
    /// Adds a timeline to the <see cref="TimelineGroup"/>.
    /// </summary>
    /// <param name="timeline">
    /// The timeline to add to the <see cref="TimelineGroup"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="timeline"/> is <see langword="null"/>. The <see cref="TimelineGroup"/> does 
    /// not allow <see langword="null"/> values.
    /// </exception>
    public void Add(ITimeline timeline)
    {
      if (timeline == null)
        throw new ArgumentNullException("timeline");

      _timelines.Add(timeline);
    }


    /// <summary>
    /// Removes all timelines from the <see cref="TimelineGroup"/>.
    /// </summary>
    public void Clear()
    {
      _timelines.Clear();
    }


    /// <summary>
    /// Determines whether the <see cref="TimelineGroup"/> contains a specific timeline.
    /// </summary>
    /// <param name="timeline">The timeline to locate in the <see cref="TimelineGroup"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="timeline"/> is found in the 
    /// <see cref="TimelineGroup"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(ITimeline timeline)
    {
      return _timelines.Contains(timeline);
    }


    /// <summary>
    /// Copies the elements of the <see cref="TimelineGroup"/> to an <see cref="Array"/>, starting 
    /// at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="TimelineGroup"/>. The <see cref="Array"/> must have zero-based indexing.
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
    /// source <see cref="TimelineGroup"/> is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    void ICollection<ITimeline>.CopyTo(ITimeline[] array, int arrayIndex)
    {
      _timelines.CopyTo(array, arrayIndex);
    }


    /// <summary>
    /// Removes the first occurrence of a specific timeline from the <see cref="TimelineGroup"/>.
    /// </summary>
    /// <param name="timeline">The timeline to remove from the <see cref="TimelineGroup"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="timeline"/> was successfully removed from the 
    /// <see cref="TimelineGroup"/>; otherwise, <see langword="false"/>. This method also returns 
    /// <see langword="false"/> if <paramref name="timeline"/> is not found in the original 
    /// <see cref="TimelineGroup"/>.
    /// </returns>
    public bool Remove(ITimeline timeline)
    {
      return _timelines.Remove(timeline);
    }


    /// <summary>
    /// Determines the index of a specific timeline in the <see cref="TimelineGroup"/>.
    /// </summary>
    /// <param name="timeline">The timeline to locate in the <see cref="TimelineGroup"/>.</param>
    /// <returns>
    /// The index of <paramref name="timeline"/> if found in the <see cref="TimelineGroup"/>; 
    /// otherwise, -1.
    /// </returns>
    public int IndexOf(ITimeline timeline)
    {
      return _timelines.IndexOf(timeline);
    }


    /// <summary>
    /// Inserts a timeline into the <see cref="TimelineGroup"/> at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="timeline"/> should be inserted.
    /// </param>
    /// <param name="timeline">
    /// The timeline to insert into the <see cref="TimelineGroup"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="TimelineGroup"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="timeline"/> is <see langword="null"/>. The <see cref="TimelineGroup"/> does 
    /// not allow <see langword="null"/> values.
    /// </exception>
    public void Insert(int index, ITimeline timeline)
    {
      if (timeline == null)
        throw new ArgumentNullException("timeline");

      _timelines.Insert(index, timeline);
    }


    /// <summary>
    /// Removes the timeline at the specified index from the <see cref="TimelineGroup"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the timeline to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="TimelineGroup"/>.
    /// </exception>
    public void RemoveAt(int index)
    {
      _timelines.RemoveAt(index);
    }
    #endregion
  }
}
