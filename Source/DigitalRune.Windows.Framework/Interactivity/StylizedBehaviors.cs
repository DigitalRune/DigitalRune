// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !SILVERLIGHT && !WINDOWS_PHONE  // Behavior.Clone() is not available.
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Interactivity;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Manages a collection of behaviors. (To be used in a <see cref="Style"/>.)
    /// </summary>
    public class StylizedBehaviorCollection : ObservableCollection<Behavior>
    {
    }


    /// <summary>
    /// Helper class that provides an attached property that allows setting behaviors in a
    /// <see cref="Style"/>.
    /// </summary>
    public static class StylizedBehaviors
    {
        // Based on http://www.livingagile.com/Blog/July-2010/Attaching-Behaviors-from-the-Expression-Blend-SDK-

        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Framework.Interactivity.Behaviors"/>
        /// attached dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a collection of behaviors to be attached to the target of the current
        /// style.
        /// </summary>
        /// <value>
        /// A <see cref="StylizedBehaviorCollection"/> containing the behavior that should be
        /// attached to the target of the current style.
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached(
            "Behaviors",
            typeof(StylizedBehaviorCollection),
            typeof(StylizedBehaviors),
            new PropertyMetadata(null, OnBehaviorsChanged));


        /// <summary>
        /// Gets the value of the
        /// <see cref="P:DigitalRune.Windows.Framework.Interactivity.Behaviors"/> attached property
        /// from a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Framework.Interactivity.Behaviors"/>
        /// attached property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static StylizedBehaviorCollection GetBehaviors(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return (StylizedBehaviorCollection)obj.GetValue(BehaviorsProperty);
        }


        /// <summary>
        /// Sets the value of the
        /// <see cref="P:DigitalRune.Windows.Framework.Interactivity.Behaviors"/> attached property
        /// to a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static void SetBehaviors(DependencyObject obj, StylizedBehaviorCollection value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.SetValue(BehaviorsProperty, value);
        }


        /// <summary>
        /// Called when the <see cref="P:DigitalRune.Windows.Framework.Interactivity.Behaviors"/>
        /// attached property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnBehaviorsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = dependencyObject as UIElement;
            if (element == null)
                return;

            var behaviors = Interaction.GetBehaviors(element);
            behaviors.Clear();

            var newBehaviors = eventArgs.NewValue as StylizedBehaviorCollection;
            if (newBehaviors != null)
            {
                foreach (var behavior in newBehaviors)
                {
                    behaviors.Add((Behavior)behavior.Clone());
                }
            }
        }
    }
}
#endif
