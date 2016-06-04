// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;
using System.Reflection;
using DigitalRune.Editor.Output;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Content.Pipeline.Builder;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Uses the MonoGame Content Pipeline to process assets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class converts an asset (e.g. a FBX file) to one or more XNB files (e.g. one XNB file
    /// for the model, one XNB file for each texture).
    /// </para>
    /// </remarks>
    [CLSCompliant(false)]
    public sealed class GameContentBuilder
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private static readonly string[] SupportedModelFileExtensions = { ".DAE", ".FBX", ".X" };
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly GameContentBuildLogger _logger;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the folder containing the currently executing assembly.
        /// </summary>
        /// <value>The folder containing the currently executing assembly.</value>
        public string ExecutableFolder { get; }


        /// <summary>
        /// Gets or sets the folder where the MonoGame Content Pipeline caches intermediate files.
        /// </summary>
        /// <value>
        /// The folder where the MonoGame Content Pipeline caches intermediate files.
        ///  If this folder is a relative (non-rooted) path, the folder is relative to the 
        /// <see cref="ExecutableFolder"/>.
        /// </value>
        public string IntermediateFolder { get; set; }


        /// <summary>
        /// Gets or sets the folder where the MonoGame Content Pipeline stores the final XNB files.
        /// </summary>
        /// <value>
        /// The folder where the MonoGame Content Pipeline stores the final XNB files.
        /// If this folder is a relative (non-rooted) path, the folder is relative to the 
        /// <see cref="ExecutableFolder"/>.
        /// </value>
        public string OutputFolder { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="GameContentBuilder" /> class.
        /// </summary>
        /// <param name="services">The service locator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="services"/> is <see langword="null"/>.
        /// </exception>
        public GameContentBuilder(IServiceLocator services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var outputService = services.GetInstance<IOutputService>().ThrowIfMissing();
            _logger = new GameContentBuildLogger(outputService);
            ExecutableFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            IntermediateFolder = "GameContentBuilder/obj";
            OutputFolder = "GameContentBuilder/bin";
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Builds the specified source file.
        /// </summary>
        /// <param name="sourceFile">
        /// The absolute path of the source file. The file format must be a supported 3D model file
        /// format.
        /// </param>
        /// <param name="importerName">The name of the content importer.</param>
        /// <param name="processorName">The name of the content processor.</param>
        /// <param name="processorParameters">The processor parameters.</param>
        /// <param name="errorMessage">The error message in case of failure.</param>
        /// <returns>
        /// <see langword="true"/> if the asset was successfully processed. <see langword="false"/>
        /// if an error has occurred, in which case the output window should contain useful
        /// information.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sourceFile"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="sourceFile"/> is empty or is not an absolute (rooted) path.
        /// </exception>
        public bool Build(string sourceFile, string importerName, string processorName, OpaqueDataDictionary processorParameters, out string errorMessage)
        {
            if (sourceFile == null)
                throw new ArgumentNullException(nameof(sourceFile));
            if (sourceFile.Length == 0)
                throw new ArgumentException("Source file path must not be empty.", nameof(sourceFile));
            if (!Path.IsPathRooted(sourceFile))
                throw new ArgumentException("Source file path must be an absolute (rooted) path.", nameof(sourceFile));

            string executableFolder = ExecutableFolder;

            string outputFolder = OutputFolder;
            if (!Path.IsPathRooted(outputFolder))
                outputFolder = Path.GetFullPath(Path.Combine(executableFolder, outputFolder));

            string intermediateFolder = IntermediateFolder;
            if (!Path.IsPathRooted(intermediateFolder))
                intermediateFolder = PathHelper.Normalize(Path.GetFullPath(Path.Combine(executableFolder, intermediateFolder)));

            // No assets should lie outside the working folder because then some XNB are built
            // into parent folders.
            string workingFolder = Path.GetPathRoot(sourceFile);
            Directory.SetCurrentDirectory(workingFolder);

            var manager = new PipelineManager(workingFolder, outputFolder, intermediateFolder)
            {
                Logger = _logger,
                RethrowExceptions = false,
                CompressContent = false,
                Profile = GraphicsProfile.HiDef,
                Platform = TargetPlatform.Windows
            };

            // Add references to content pipeline assemblies.
            // All assemblies are expected to lie in the executable folder. Therefore, this project
            // references the content pipeline DLLs, so that they are automatically copied to the
            // executable folder.
            // TODO: Make a public property to allow user to set references.
            manager.AddAssembly(Path.GetFullPath(Path.Combine(ExecutableFolder, "DigitalRune.Animation.Content.Pipeline.dll")));
            manager.AddAssembly(Path.GetFullPath(Path.Combine(ExecutableFolder, "DigitalRune.Game.UI.Content.Pipeline.dll")));
            manager.AddAssembly(Path.GetFullPath(Path.Combine(ExecutableFolder, "DigitalRune.Geometry.Content.Pipeline.dll")));
            manager.AddAssembly(Path.GetFullPath(Path.Combine(ExecutableFolder, "DigitalRune.Graphics.Content.Pipeline.dll")));
            manager.AddAssembly(Path.GetFullPath(Path.Combine(ExecutableFolder, "DigitalRune.Mathematics.Content.Pipeline.dll")));

            // Add this assembly because it also contains model processors.
            manager.AddAssembly(Path.GetFullPath(Path.Combine(ExecutableFolder, "DigitalRune.Editor.Game.dll")));

            _logger.LogMessage("----- Begin MonoGame content pipeline -----");

            // Remove any previous build content to force a full rebuild.
            manager.CleanContent(sourceFile);

            try
            {
                manager.BuildContent(sourceFile, null, importerName, processorName, processorParameters);
                errorMessage = null;
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogMessage("Exception in MonoGame content pipeline:");
                _logger.LogWarning(null, null, exception.Message);
                _logger.LogMessage(exception.StackTrace);
                if (exception.Message.StartsWith("Value cannot be null.", StringComparison.OrdinalIgnoreCase))
                    _logger.LogWarning(null, null, "This can happen if the FBX references a texture but the filename is null. Check the referenced texture file paths in a 3D modeling tool.");

                errorMessage = exception.Message;
                return false;
            }
            finally
            {
                _logger.LogMessage("----- End MonoGame content pipeline -----");
            }
        }


        /// <summary>
        /// Determines whether the specified file extension describes a supported 3D model file
        /// format.
        /// </summary>
        /// <param name="extension">The extension, including '.'. For example: ".fbx".</param>
        /// <returns><see langword="true"/> if the file format is supported.</returns>
        public static bool IsSupportedModelFileExtension(string extension)
        {
            foreach (var supportedExtension in SupportedModelFileExtensions)
                if (string.Compare(extension, supportedExtension, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;

            return false;
        }
        #endregion
    }
}
