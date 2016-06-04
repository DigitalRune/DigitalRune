// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
/*
  The "screen conduction" pattern implemented in DigitalRune.Windows.Framework was 
  inspired by the Caliburn.Micro framework (see http://caliburnmicro.codeplex.com/).
*/
#endregion

using System.Threading.Tasks;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Represents an item which may prevent closing.
    /// </summary>
    public interface IGuardClose
    {
        /// <summary>
        /// Asynchronously determines whether the item can be closed.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The result of the task indicates
        /// whether the item can be closed.
        /// </returns>
        Task<bool> CanCloseAsync();
    }
}
