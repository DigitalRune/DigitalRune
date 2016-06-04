#region ----- Copyright -----
/*
  The class in this file is based on the WorkOptions from the ParallelTasks library (see 
  http://paralleltasks.codeplex.com/) which is licensed under Ms-PL (see below).


  Microsoft Public License (Ms-PL)

  This license governs use of the accompanying software. If you use the software, you accept this 
  license. If you do not accept the license, do not use the software.

  1. Definitions

  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same 
  meaning here as under U.S. copyright law.

  A "contribution" is the original software, or any additions or changes to the software.

  A "contributor" is any person that distributes its contribution under this license.

  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

  2. Grant of Rights

  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and 
  limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
  copyright license to reproduce its contribution, prepare derivative works of its contribution, and 
  distribute its contribution or any derivative works that you create.

  (B) Patent Grant- Subject to the terms of this license, including the license conditions and 
  limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
  license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or 
  otherwise dispose of its contribution in the software or derivative works of the contribution in 
  the software.

  3. Conditions and Limitations

  (A) No Trademark License- This license does not grant you rights to use any contributors' name, 
  logo, or trademarks.

  (B) If you bring a patent claim against any contributor over patents that you claim are infringed 
  by the software, your patent license from such contributor to the software ends automatically.

  (C) If you distribute any portion of the software, you must retain all copyright, patent, 
  trademark, and attribution notices that are present in the software.

  (D) If you distribute any portion of the software in source code form, you may do so only under 
  this license by including a complete copy of this license with your distribution. If you 
  distribute any portion of the software in compiled or object code form, you may only do so under a 
  license that complies with this license.

  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no 
  express warranties, guarantees or conditions. You may have additional consumer rights under your 
  local laws which this license cannot change. To the extent permitted under your local laws, the 
  contributors exclude the implied warranties of merchantability, fitness for a particular purpose 
  and non-infringement.  
*/
#endregion

using System;


#if !NETFX_CORE && !PORTABLE && !USE_TPL
namespace DigitalRune.Threading
{
  /// <summary>
  /// Defines how an <see cref="IWork"/> instance can be executed.
  /// </summary>
  public struct WorkOptions : IEquatable<WorkOptions>
  {
    /// <summary>
    /// Defines the default options.
    /// </summary>
    /// <remarks>
    /// The default options are:
    /// <list type="bullet">
    /// <listheader>
    /// <term>Property</term>
    /// <description>Default Value</description>
    /// </listheader>
    /// <item>
    /// <term>DetachFromParent</term>
    /// <description>false</description>
    /// </item>
    /// <item>
    /// <term>DetachFromParent</term>
    /// <description>false</description>
    /// </item>
    /// <item>
    /// <term>QueueFIFO</term>
    /// <description>1</description>
    /// </item>
    /// </list>
    /// </remarks>
    public static readonly WorkOptions Default = new WorkOptions { DetachFromParent = false, MaximumThreads = 1, QueueFIFO = false };


    /// <summary>
    /// Gets or sets a value indicating whether the work will be created detached from its parent. 
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the parent task can complete independent from the child task. 
    /// If <see langword="false"/> the parent task will wait for this work to complete before 
    /// completing itself.
    /// </value>
    public bool DetachFromParent { get; set; }


    /// <summary>
    /// Gets or sets the maximum number of threads which can concurrently execute this work.
    /// </summary>
    /// <value>The maximum number of threads which can concurrently execute this work.</value>
    public int MaximumThreads { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this work should be queued in a first-in-first-out 
    /// (FIFO) order.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the task should be queued in FIFO order; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// FIFO order: The task will be started after all currently existing tasks.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FIFO")]
    public bool QueueFIFO { get; set; }


    #region ----- Equality Members -----

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other" />
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(WorkOptions other)
    {
      return DetachFromParent == other.DetachFromParent 
             && MaximumThreads == other.MaximumThreads 
             && QueueFIFO == other.QueueFIFO;
    }


    /// <summary>
    /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
    /// <returns>
    ///   <see langword="true"/> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is WorkOptions && Equals((WorkOptions)obj);
    }


    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures
    /// like a hash table.
    /// </returns>
    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = DetachFromParent.GetHashCode();
        hashCode = (hashCode * 397) ^ MaximumThreads;
        hashCode = (hashCode * 397) ^ QueueFIFO.GetHashCode();
        return hashCode;
      }
    }


    /// <summary>
    /// Compares <see cref="WorkOptions"/> to determine whether they are the same.
    /// </summary>
    /// <param name="left">The first <see cref="WorkOptions"/>.</param>
    /// <param name="right">The second <see cref="WorkOptions"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the 
    /// same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(WorkOptions left, WorkOptions right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares <see cref="WorkOptions"/> to determine whether they are different.
    /// </summary>
    /// <param name="left">The first <see cref="WorkOptions"/>.</param>
    /// <param name="right">The second <see cref="WorkOptions"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are 
    /// different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(WorkOptions left, WorkOptions right)
    {
      return !left.Equals(right);
    }
    #endregion
  }
}
#endif
