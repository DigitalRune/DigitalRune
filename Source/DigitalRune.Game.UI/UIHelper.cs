// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Linq;


namespace DigitalRune.Game.UI
{
  /// <summary>
  /// Provides helper methods for working with <see cref="UIControl"/>s.
  /// </summary>
  public static class UIHelper
  {
    ///// <summary>
    ///// Determines whether the specified control is in a focus scope (e.g. a <see cref="Window"/>)
    ///// which has the focus.
    ///// </summary>
    ///// <param name="control">The control.</param>
    ///// <returns>
    ///// <see langword="true"/> if the specified control has an ancestor which is a focus
    ///// scope (see <see cref="UIControl.IsFocusScope"/>) which has the focus (see 
    ///// <see cref="UIControl.IsFocusWithin"/>. <see langword="false"/> if no parent focus
    ///// scope has the focus or if there is no parent focus scope.
    ///// </returns>
    //internal static bool IsInActiveFocusScope(this UIControl control)
    //{
    //  var parent = control.VisualParent;
    //  while (parent != null)
    //  {
    //    if (parent.IsFocusScope)
    //      return parent.IsFocusWithin;

    //    parent = parent.VisualParent;
    //  }
    //  return true;
    //}


    //--------------------------------------------------------------
    #region LINQ to Visual Tree
    //--------------------------------------------------------------

    private static readonly Func<UIControl, UIControl> GetParent = control => control.VisualParent;
    private static readonly Func<UIControl, IEnumerable<UIControl>> GetChildren = control => control.VisualChildren;


    /// <summary>
    /// Returns the root control of the visual tree.
    /// </summary>
    /// <param name="control">The control where to start the search.</param>
    /// <returns>The root control.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="control"/> is <see langword="null"/>.
    /// </exception>
    public static UIControl GetRoot(this UIControl control)
    {
      if (control == null)
        throw new ArgumentNullException("control");

      while (control.VisualParent != null)
        control = control.VisualParent;

      return control;
    }


    /// <summary>
    /// Gets the ancestors of the control in the visual tree.
    /// </summary>
    /// <param name="control">The control where to start the search.</param>
    /// <returns>The ancestors of <paramref name="control"/> in the visual tree.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="control"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<UIControl> GetAncestors(this UIControl control)
    {
      if (control == null)
        throw new ArgumentNullException("control");

      return TreeHelper.GetAncestors(control, GetParent);
    }


    /// <summary>
    /// Gets the control and its ancestors in the visual tree.
    /// </summary>
    /// <param name="control">The control where to start the search.</param>
    /// <returns>The <paramref name="control"/> and its ancestors in the visual tree.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="control"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<UIControl> GetSelfAndAncestors(this UIControl control)
    {
      if (control == null)
        throw new ArgumentNullException("control");

      return TreeHelper.GetSelfAndAncestors(control, GetParent);
    }


    /// <overloads>
    /// <summary>
    /// Gets the descendants of the control in the visual tree.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the descendants of the control in the visual tree using a depth-first search.
    /// </summary>
    /// <param name="control">The control where to start the search.</param>
    /// <returns>The descendants of <paramref name="control"/> in the visual tree.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="control"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<UIControl> GetDescendants(this UIControl control)
    {
      if (control == null)
        throw new ArgumentNullException("control");

      return TreeHelper.GetDescendants(control, GetChildren, true);
    }


    /// <summary>
    /// Gets the descendants of the control in the visual tree using either a depth-first or a 
    /// breadth-first search.
    /// </summary>
    /// <param name="control">The control where to start the search.</param>
    /// <param name="depthFirst">
    /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
    /// otherwise a breadth-first search will be made.
    /// </param>
    /// <returns>The descendants of <paramref name="control"/> in the visual tree.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="control"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<UIControl> GetDescendants(this UIControl control, bool depthFirst)
    {
      if (control == null)
        throw new ArgumentNullException("control");

      return TreeHelper.GetDescendants(control, GetChildren, depthFirst);
    }


    /// <overloads>
    /// <summary>
    /// Gets the subtree (the given control and all of its descendants in the visual tree).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the subtree (the given control and all of its descendants in the visual tree) using a 
    /// depth-first search.
    /// </summary>
    /// <param name="control">The control where to start the search.</param>
    /// <returns>
    /// The <paramref name="control"/> and all of its descendants in the visual tree.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="control"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<UIControl> GetSubtree(this UIControl control)
    {
      if (control == null)
        throw new ArgumentNullException("control");

      return TreeHelper.GetSubtree(control, GetChildren, true);
    }


    /// <summary>
    /// Gets the subtree (the given control and all of its descendants in the visual tree) using 
    /// either a depth-first or a breadth-first search.
    /// </summary>
    /// <param name="control">The control where to start the search.</param>
    /// <param name="depthFirst">
    /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
    /// otherwise a breadth-first search will be made.
    /// </param>
    /// <returns>
    /// The <paramref name="control"/> and all of its descendants in the visual tree.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="control"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<UIControl> GetSubtree(this UIControl control, bool depthFirst)
    {
      if (control == null)
        throw new ArgumentNullException("control");

      return TreeHelper.GetSubtree(control, GetChildren, depthFirst);
    }


    /// <summary>
    /// Gets the leaves of the control in the visual tree.
    /// </summary>
    /// <param name="control">The control where to start the search.</param>
    /// <returns>
    /// The leaves of <paramref name="control"/> in the visual tree.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="control"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<UIControl> GetLeaves(this UIControl control)
    {
      if (control == null)
        throw new ArgumentNullException("control");

      return TreeHelper.GetLeaves(control, GetChildren);
    }
    #endregion
  }
}
