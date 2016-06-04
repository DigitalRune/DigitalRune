// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;
using NLog;
using static System.FormattableString;


namespace DigitalRune.Editor.Options
{
    /// <summary>
    /// Merges collections of options nodes.
    /// </summary>
    internal class OptionsMergeAlgorithm : MergeAlgorithm<OptionsPageViewModel>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsMergeAlgorithm"/> class.
        /// </summary>
        public OptionsMergeAlgorithm()
        {
            CloneNodesOnMerge = true;
        }


        /// <summary>
        /// Called when a node should be merged with an existing node.
        /// </summary>
        /// <param name="existingNode">
        /// The existing node to which <paramref name="node"/> shall be merged.
        /// </param>
        /// <param name="node">
        /// The additional node which is about to be merged to <paramref name="existingNode"/>.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method merges two nodes if they contain a <see cref="OptionsGroupViewModel"/>. Nodes
        /// with other content cannot be merged.
        /// </para>
        /// </remarks>
        /// <exception cref="MergeException">
        /// Options cannot be merged. Only <see cref="OptionsGroupViewModel"/>s can be merged.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        protected override void OnMerge(MergeableNode<OptionsPageViewModel> existingNode, MergeableNode<OptionsPageViewModel> node)
        {
            if (!(existingNode.Content is OptionsGroupViewModel) || !(node.Content is OptionsGroupViewModel))
            {
                // Nodes cannot be merged.
                string message = Invariant(
                    $"Cannot merge options \"{existingNode.Content.DisplayName}\" - only nodes containing OptionsPageGroup objects can be merged.");

                Logger.Error(message);
                throw new MergeException(message);
            }

            // Call base method to merge children.
            base.OnMerge(existingNode, node);
        }
    }
}
