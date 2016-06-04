// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
/*
  The "screen conduction" pattern implemented in DigitalRune.Windows.Framework was 
  inspired by the Caliburn.Micro framework (see http://caliburnmicro.codeplex.com/).
*/
#endregion

using System;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Provides extensions methods for screens and conductors.
    /// </summary>
    public static class ScreenHelper
    {
        ///<summary>
        /// Activates and deactivates a child whenever the specified parent is activated or 
        /// deactivated.
        ///</summary>
        /// <typeparam name="TChild">The type of the child.</typeparam>
        /// <typeparam name="TParent">The type of the parent.</typeparam>
        ///<param name="child">The child to activate and deactivate.</param>
        ///<param name="parent">
        /// The parent whose activation/deactivation triggers the child's activation/deactivation.
        /// </param>
        /// <remarks>
        /// When the parent is closed, the child is closed too and the link between the parent and 
        /// the child ends; i.e. when the parent is reopened, the child is not reopened.
        /// </remarks>
        public static void ConductWith<TChild, TParent>(this TChild child, TParent parent)
            where TChild : IActivatable
            where TParent : IActivatable
        {
            // Set an event handler to activate the child whenever the parent screen is activated.
            EventHandler<ActivationEventArgs> activationHandler = (s, e) => child.OnActivate();
            parent.Activated += activationHandler;

            // Set an event handler that deactivates the child whenever the parent is 
            // deactivated. (Unregister the event handler if the parent is closed.)
            EventHandler<DeactivationEventArgs> deactivationHandler = null;
            deactivationHandler = (s, e) =>
            {
                child.OnDeactivate(e.Closed);
                if (e.Closed)
                {
                    parent.Activated -= activationHandler;
                    parent.Deactivated -= deactivationHandler;
                }
            };
            parent.Deactivated += deactivationHandler;
        }
    }
}
