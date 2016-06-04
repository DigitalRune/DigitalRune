// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Animation.Traits;
using DigitalRune.Linq;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Provides helper methods for working with animations.
  /// </summary>
  public static partial class AnimationHelper
  {
    /// <summary>
    /// Returns the larger of the two durations.
    /// </summary>
    /// <param name="duration1">The first duration to compare.</param>
    /// <param name="duration2">The second duration to compare.</param>
    /// <returns>
    /// Parameter <paramref name="duration1"/> or <paramref name="duration2"/>, whichever is larger.
    /// </returns>
    internal static TimeSpan Max(TimeSpan duration1, TimeSpan duration2)
    {
      return (duration1 >= duration2) ? duration1 : duration2;
    }


    /// <summary>
    /// Gets the state of the animation for the specified time on the timeline. (This helper method
    /// can be used for animations which start at time 0 and run with normal speed.)
    /// </summary>
    /// <param name="timeline">The timeline.</param>
    /// <param name="time">The time on the timeline.</param>
    /// <returns>The animation state.</returns>
    internal static AnimationState GetState(ITimeline timeline, TimeSpan time)
    {
      if (time < TimeSpan.Zero)
      {
        // The animation has not started.
        return AnimationState.Delayed;
      }

      TimeSpan duration = timeline.GetTotalDuration();
      if (time > duration)
      {
        if (timeline.FillBehavior == FillBehavior.Stop)
        {
          // The animation has stopped.
          return AnimationState.Stopped;
        }

        // The animation holds the last value.
        Debug.Assert(timeline.FillBehavior == FillBehavior.Hold);
        return AnimationState.Filling;
      }

      return AnimationState.Playing;
    }


    /// <summary>
    /// Gets the animation time for the specified time value on the timeline. (This helper method
    /// can be used for animations which start at time 0 and run with normal speed.)
    /// </summary>
    /// <param name="timeline">The timeline.</param>
    /// <param name="time">The time on the timeline.</param>
    /// <returns>
    /// The animation time. (Or <see langword="null"/> if the animation is not active at 
    /// <paramref name="time"/>.)
    /// </returns>
    internal static TimeSpan? GetAnimationTime(ITimeline timeline, TimeSpan time)
    {
      if (time < TimeSpan.Zero)
      {
        // The animation has not started.
        return null;
      }

      TimeSpan duration = timeline.GetTotalDuration();
      if (time > duration)
      {
        if (timeline.FillBehavior == FillBehavior.Stop)
        {
          // The animation has stopped.
          return null;
        }

        // The animation holds the final value.
        Debug.Assert(timeline.FillBehavior == FillBehavior.Hold);
        return duration;
      }

      return time;
    }


    /// <summary>
    /// Determines whether the given time value corresponds to a mirrored oscillation loop.
    /// </summary>
    /// <param name="time">The time value.</param>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <param name="loopBehavior">The loop behavior.</param>
    /// <returns>
    /// <see langword="true"/> if the time value is in a mirrored oscillation loop; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    internal static bool IsInMirroredOscillation(TimeSpan time, TimeSpan startTime, TimeSpan endTime, LoopBehavior loopBehavior)
    {
      Debug.Assert(endTime >= startTime, "Invalid start and end time.");
      TimeSpan length = endTime - startTime;

      if (length == TimeSpan.Zero)
        return false;

      if (loopBehavior == LoopBehavior.Oscillate)
      {
        if (time < startTime)
          return ((startTime.Ticks - time.Ticks) / length.Ticks) % 2 == 0;

        if (time > endTime)
          return ((time.Ticks - endTime.Ticks) / length.Ticks) % 2 == 0;
      }

      return false;
    }


    /// <summary>
    /// Handles different loop behaviors by changing the given time value so that it lies between 
    /// start and end time.
    /// </summary>
    /// <param name="time">The time value.</param>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <param name="loopBehavior">The loop behavior.</param>
    /// <param name="loopedTime">The adjusted time value.</param>
    /// <returns>
    /// <see langword="true"/> if the current cycle requires an offset; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    internal static bool LoopParameter(TimeSpan time, TimeSpan startTime, TimeSpan endTime, LoopBehavior loopBehavior, out TimeSpan loopedTime)
    {
      Debug.Assert(endTime >= startTime, "Invalid start and end time.");
      TimeSpan length = endTime - startTime;

      if (length == TimeSpan.Zero)
      {
        loopedTime = startTime;
        return false;
      }

      if (time < startTime)
      {
        #region ----- Pre-loop -----

        // Handle pre-loop. For some loop types we return immediately. For some
        // we adjust the time value.
        if (loopBehavior == LoopBehavior.Constant)
        {
          loopedTime = startTime;
          return false;
        }

        long numberOfPeriods = (endTime.Ticks - time.Ticks) / length.Ticks;
        if (loopBehavior == LoopBehavior.Cycle || loopBehavior == LoopBehavior.CycleOffset)
        {
          loopedTime = new TimeSpan(time.Ticks + length.Ticks * numberOfPeriods);
          return loopBehavior == LoopBehavior.CycleOffset;
        }

        Debug.Assert(loopBehavior == LoopBehavior.Oscillate);

        if (numberOfPeriods % 2 != 0)
        {
          // odd = mirrored
          loopedTime = new TimeSpan(startTime.Ticks + endTime.Ticks - (time.Ticks + length.Ticks * numberOfPeriods)); 
          return false;
        }

        // even = not mirrored
        loopedTime = new TimeSpan(time.Ticks + length.Ticks * numberOfPeriods);
        return false;
        #endregion
      }
      
      if (time > endTime)
      {
        #region ----- Post-loop -----

        // Handle post-loop. For some loop types we return immediately. For some
        // we adjust the time value.
        if (loopBehavior == LoopBehavior.Constant)
        {
          loopedTime = endTime;
          return false;
        }

        long numberOfPeriods = (time.Ticks - startTime.Ticks) / length.Ticks;
        if (loopBehavior == LoopBehavior.Cycle || loopBehavior == LoopBehavior.CycleOffset)
        {
          loopedTime = new TimeSpan(time.Ticks - length.Ticks * numberOfPeriods);
          return loopBehavior == LoopBehavior.CycleOffset;
        }

        Debug.Assert(loopBehavior == LoopBehavior.Oscillate);

        if (numberOfPeriods % 2 != 0)
        {
          // odd = mirrored
          loopedTime = new TimeSpan(endTime.Ticks - (time.Ticks - startTime.Ticks - length.Ticks * numberOfPeriods));
          return false;
        }

        // even = not mirrored
        loopedTime = new TimeSpan(time.Ticks - length.Ticks * numberOfPeriods);
        return false;
        #endregion
      }

      loopedTime = time;
      return false;
    }


    /// <summary>
    /// Gets the cycle offset for a time value.
    /// </summary>
    /// <typeparam name="T">The type of animation value.</typeparam>
    /// <param name="time">The time value.</param>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <param name="startValue">In: The animation value at <paramref name="startTime"/>.</param>
    /// <param name="endValue">In: The animation value at <paramref name="endTime"/>.</param>
    /// <param name="traits">The traits of the animation value.</param>
    /// <param name="loopBehavior">The post-loop behavior.</param>
    /// <param name="cycleOffset">
    /// Out: The cycle offset.
    /// </param>
    /// <remarks>
    /// The cycle offset is <see cref="IAnimationValueTraits{T}.SetIdentity"/> if the 
    /// <see cref="LoopBehavior"/> is unequal to <see cref="LoopBehavior.CycleOffset"/> or if the 
    /// <paramref name="time"/> is in the regular cycle (between the first and the last key frame).
    /// </remarks>
    internal static void GetCycleOffset<T>(TimeSpan time,
                                           TimeSpan startTime, TimeSpan endTime,
                                           ref T startValue, ref T endValue, 
                                           IAnimationValueTraits<T> traits,
                                           LoopBehavior loopBehavior, 
                                           ref T cycleOffset)
    {
      Debug.Assert(endTime > startTime, "Invalid start and end time.");
      TimeSpan length = endTime - startTime;

      // Handle cycle offset.
      if (loopBehavior == LoopBehavior.CycleOffset && length != TimeSpan.Zero)
      {
        traits.Invert(ref startValue, ref startValue);
        traits.Add(ref startValue, ref endValue, ref cycleOffset);

        if (time < startTime)
        {
          long numberOfPeriods = (time.Ticks - endTime.Ticks) / length.Ticks;
          Debug.Assert(numberOfPeriods < 0, "Negative number of periods expected.");
          traits.Multiply(ref cycleOffset, (int)numberOfPeriods, ref cycleOffset);
        }
        else if (time > endTime)
        {
          long numberOfPeriods = (time.Ticks - startTime.Ticks) / length.Ticks;
          Debug.Assert(numberOfPeriods > 0, "Positive number of periods expected.");
          traits.Multiply(ref cycleOffset, (int)numberOfPeriods, ref cycleOffset);
        }
        else
        {
          traits.SetIdentity(ref cycleOffset);
        }
      }
    }


    #region ----- LINQ to Animation Tree -----

    private static readonly Func<AnimationInstance, AnimationInstance> GetParent = animationInstance => animationInstance.Parent;
    private static readonly Func<AnimationInstance, IEnumerable<AnimationInstance>> GetChildren = animationInstance => animationInstance.Children;


    /// <summary>
    /// Returns the root instance of an animation tree.
    /// </summary>
    /// <param name="animationInstance">The animation instance where to start the search.</param>
    /// <returns>The root instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animationInstance"/> is <see langword="null"/>.
    /// </exception>
    public static AnimationInstance GetRoot(this AnimationInstance animationInstance)
    {
      if (animationInstance == null)
        throw new ArgumentNullException("animationInstance");

      while (animationInstance.Parent != null)
        animationInstance = animationInstance.Parent;

      return animationInstance;
    }


    /// <summary>
    /// Gets the ancestors of the <see cref="AnimationInstance"/> in the animation tree.
    /// </summary>
    /// <param name="animationInstance">The animation instance where to start the search.</param>
    /// <returns>
    /// The ancestors of <paramref name="animationInstance"/> in the animation tree.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animationInstance"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<AnimationInstance> GetAncestors(this AnimationInstance animationInstance)
    {
      if (animationInstance == null)
        throw new ArgumentNullException("animationInstance");

      return TreeHelper.GetAncestors(animationInstance, GetParent);
    }


    /// <summary>
    /// Gets the <see cref="AnimationInstance"/> and its ancestors in the animation tree.
    /// </summary>
    /// <param name="animationInstance">The animation instance where to start the search.</param>
    /// <returns>
    /// The <paramref name="animationInstance"/> and its ancestors in the animation tree.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animationInstance"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<AnimationInstance> GetSelfAndAncestors(this AnimationInstance animationInstance)
    {
      if (animationInstance == null)
        throw new ArgumentNullException("animationInstance");

      return TreeHelper.GetSelfAndAncestors(animationInstance, GetParent);
    }


    /// <overloads>
    /// <summary>
    /// Gets the descendants of the <see cref="AnimationInstance"/> in the animation tree.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the descendants of the <see cref="AnimationInstance"/> in the animation tree using a
    /// depth-first search.
    /// </summary>
    /// <param name="animationInstance">The animation instance where to start the search.</param>
    /// <returns>
    /// The descendants of <paramref name="animationInstance"/> in the animation tree.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animationInstance"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<AnimationInstance> GetDescendants(this AnimationInstance animationInstance)
    {
      if (animationInstance == null)
        throw new ArgumentNullException("animationInstance");

      return TreeHelper.GetDescendants(animationInstance, GetChildren, true);
    }


    /// <summary>
    /// Gets the descendants of the <see cref="AnimationInstance"/> in the animation tree 
    /// using either a depth-first or a breadth-first search.
    /// </summary>
    /// <param name="animationInstance">The animation instance where to start the search.</param>
    /// <param name="depthFirst">
    /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
    /// otherwise a breadth-first search will be made.
    /// </param>
    /// <returns>
    /// The descendants of <paramref name="animationInstance"/> in the animation tree.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animationInstance"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<AnimationInstance> GetDescendants(this AnimationInstance animationInstance, bool depthFirst)
    {
      if (animationInstance == null)
        throw new ArgumentNullException("animationInstance");

      return TreeHelper.GetDescendants(animationInstance, GetChildren, depthFirst);
    }


    /// <overloads>
    /// <summary>
    /// Gets the subtree (the given <see cref="AnimationInstance"/> and all of its descendants in the
    /// animation tree).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the subtree (the given <see cref="AnimationInstance"/> and all of its descendants in the
    /// animation tree) using a depth-first search.
    /// </summary>
    /// <param name="animationInstance">The animation instance where to start the search.</param>
    /// <returns>
    /// The <paramref name="animationInstance"/> and all of its descendants in the animation tree.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animationInstance"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<AnimationInstance> GetSubtree(this AnimationInstance animationInstance)
    {
      if (animationInstance == null)
        throw new ArgumentNullException("animationInstance");

      return TreeHelper.GetSubtree(animationInstance, GetChildren, true);
    }


    /// <summary>
    /// Gets the subtree (the given <see cref="AnimationInstance"/> and all of its descendants in the
    /// animation tree) using either a depth-first or a breadth-first search.
    /// </summary>
    /// <param name="animationInstance">The animation instance where to start the search.</param>
    /// <param name="depthFirst">
    /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
    /// otherwise a breadth-first search will be made.
    /// </param>
    /// <returns>
    /// The <paramref name="animationInstance"/> and all of its descendants in the animation tree.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animationInstance"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<AnimationInstance> GetSubtree(this AnimationInstance animationInstance, bool depthFirst)
    {
      if (animationInstance == null)
        throw new ArgumentNullException("animationInstance");

      return TreeHelper.GetSubtree(animationInstance, GetChildren, depthFirst);
    }


    /// <summary>
    /// Gets the leaves of the <see cref="AnimationInstance"/> in the animation tree.
    /// </summary>
    /// <param name="animationInstance">The animation instance where to start the search.</param>
    /// <returns>
    /// The leaves of <paramref name="animationInstance"/> in the animation tree.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animationInstance"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<AnimationInstance> GetLeaves(this AnimationInstance animationInstance)
    {
      if (animationInstance == null)
        throw new ArgumentNullException("animationInstance");

      return TreeHelper.GetLeaves(animationInstance, GetChildren);
    }
    #endregion


    /// <summary>
    /// Computes the linear velocity that moves an object from the current position to a 
    /// target position.
    /// </summary>
    /// <param name="currentPosition">The current position.</param>
    /// <param name="targetPosition">The target position.</param>
    /// <param name="deltaTime">The time over which the movement takes place (in seconds).</param>
    /// <returns>
    /// The linear velocity vector. If an object is moved with this velocity starting at
    /// <paramref name="currentPosition"/>, it will arrive at <paramref name="targetPosition"/>
    /// after <paramref name="deltaTime"/> seconds.
    /// </returns>
    public static Vector3F ComputeLinearVelocity(Vector3F currentPosition, Vector3F targetPosition, float deltaTime)
    {
      if (Numeric.IsZero(deltaTime))
        return Vector3F.Zero;

      return (targetPosition - currentPosition) / deltaTime;
    }


    /// <overloads>
    /// <summary>
    /// Computes the angular velocity that rotates an object from the current orientation to a 
    /// target orientation.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Computes the angular velocity that rotates an object from the current orientation to a 
    /// target orientation.
    /// </summary>
    /// <param name="currentOrientation">The current orientation.</param>
    /// <param name="targetOrientation">The target orientation.</param>
    /// <param name="deltaTime">The time over which the rotation takes place (in seconds).</param>
    /// <returns>
    /// The angular velocity vector. If an object is rotated with this velocity starting at
    /// <paramref name="currentOrientation"/>, it will arrive at <paramref name="targetOrientation"/>
    /// after <paramref name="deltaTime"/> seconds.
    /// </returns>
    public static Vector3F ComputeAngularVelocity(QuaternionF currentOrientation, QuaternionF targetOrientation, float deltaTime)
    {
      if (Numeric.IsZero(deltaTime))
        return Vector3F.Zero;

      // ----- Angular Velocity
      QuaternionF orientationDelta = targetOrientation * currentOrientation.Conjugated;

      // Make sure we move along the shortest arc.
      if (QuaternionF.Dot(currentOrientation, targetOrientation) < 0)
        orientationDelta = -orientationDelta;

      // Determine the angular velocity that rotates the body.
      Vector3F rotationAxis = orientationDelta.Axis;
      if (!rotationAxis.IsNumericallyZero)
      {
        // The angular velocity is computed as rotationAxis * rotationSpeed.
        float rotationSpeed = (orientationDelta.Angle / deltaTime);
        return rotationAxis * rotationSpeed;
      }

      // The axis of rotation is 0. That means the no rotation should be applied.
      return Vector3F.Zero;
    }


    /// <summary>
    /// Computes the angular velocity that rotates an object from the current orientation to a 
    /// target orientation.
    /// </summary>
    /// <param name="currentOrientation">The current orientation.</param>
    /// <param name="targetOrientation">The target orientation.</param>
    /// <param name="deltaTime">The time over which the rotation takes place (in seconds).</param>
    /// <returns>
    /// The angular velocity vector. If an object is rotated with this velocity starting at
    /// <paramref name="currentOrientation"/>, it will arrive at <paramref name="targetOrientation"/>
    /// after <paramref name="deltaTime"/> seconds.
    /// </returns>
    public static Vector3F ComputeAngularVelocity(Matrix33F currentOrientation, Matrix33F targetOrientation, float deltaTime)
    {
      return ComputeAngularVelocity(QuaternionF.CreateRotation(currentOrientation), QuaternionF.CreateRotation(targetOrientation), deltaTime);
    }
  }
}
