// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using DigitalRune.Editor.Output;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using NLog;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// This class wraps the MSBuild functionality needed to build XNA Framework content dynamically
    /// at runtime.
    /// </summary>
    /// <remarks>
    /// It creates a temporary MSBuild project in memory, and adds whatever content files you choose
    /// to this project. It then builds the project, which will create compiled .xnb content files
    /// in a temporary directory. After the build finishes, you can use a regular ContentManager to
    /// load these temporary .xnb files in the usual way.
    /// </remarks>
    internal class XnaContentBuilder : IDisposable
    {
        // Notes:
        // We can use the MSBuild API directly or call MSBuild.exe. At the moment we use MSBuild.exe
        // because using the .NET API directly I could not build an x86 XNA content project when the
        // executing project is x64.

        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        // TODO: Auto-detect MSBuild.exe and let user change path in options page.
        private const string MSBuildPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe";

        // What importers or processors should we load?
        private const string XnaVersion = ", Version=4.0.0.0, PublicKeyToken=842cf8be1de50553";

        private static readonly string[] PipelineAssemblies =
        {
            "Microsoft.Xna.Framework.Content.Pipeline.FBXImporter" + XnaVersion,
            "Microsoft.Xna.Framework.Content.Pipeline.XImporter" + XnaVersion,
            "Microsoft.Xna.Framework.Content.Pipeline.TextureImporter" + XnaVersion,
            "Microsoft.Xna.Framework.Content.Pipeline.EffectImporter" + XnaVersion,

            // If you want to use custom importers or processors from
            // a Content Pipeline Extension Library, add them here.

            // If your extension DLL is installed in the GAC, you should refer to it by assembly
            // name, eg. "MyPipelineExtension, Version=1.0.0.0, PublicKeyToken=1234567812345678".

            // If the extension DLL is not in the GAC, you should refer to it by
            // file path, eg. "c:/MyProject/bin/MyPipelineExtension.dll".
        };
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IOutputService _outputService;
        //private readonly BuildLogger _buildLogger;

        // MSBuild objects used to dynamically build content.
        private Project _buildProject;
        private ProjectRootElement _projectRootElement;
        //private BuildParameters _buildParameters;
        private readonly List<ProjectItem> _projectItems = new List<ProjectItem>();

        private readonly TempDirectoryHelper _tempDirectoryHelper;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether this instance has been disposed of.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance has been disposed of; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        public bool IsDisposed { get; private set; }


        /// <summary>
        /// Gets the output directory, which will contain the generated .xnb files.
        /// </summary>
        public string OutputDirectory
        {
            get { return Path.Combine(_tempDirectoryHelper.TempDirectoryName, "bin/Content"); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Creates a new content builder.
        /// </summary>
        /// <param name="editor">The editor.</param>
        public XnaContentBuilder(IEditorService editor)
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            _outputService = editor.Services.GetInstance<IOutputService>().ThrowIfMissing();

            //_buildLogger = new BuildLogger(outputService);
            _tempDirectoryHelper = new TempDirectoryHelper(editor.ApplicationName, "XnaContentBuilder");
            CreateBuildProject();
        }


        /// <summary>
        /// Releases unmanaged resources before an instance of the <see cref="XnaContentBuilder"/>
        /// class is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// This method releases unmanaged resources by calling the virtual
        /// <see cref="Dispose(bool)"/> method, passing in <see langword="false"/>.
        /// </remarks>
        ~XnaContentBuilder()
        {
            Dispose(false);
        }


        /// <summary>
        /// Releases all resources used by an instance of the <see cref="XnaContentBuilder"/> class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in 
        /// <see langword="true"/>, and then suppresses finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Releases the unmanaged resources used by an instance of the
        /// <see cref="XnaContentBuilder"/> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    _tempDirectoryHelper.Dispose();
                }

                // Release unmanaged resources.

                IsDisposed = true;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Creates a temporary MSBuild content project in memory.
        /// </summary>
        private void CreateBuildProject()
        {
            string projectPath = Path.Combine(_tempDirectoryHelper.TempDirectoryName, "content.contentproj");
            string outputPath = Path.Combine(_tempDirectoryHelper.TempDirectoryName, "bin");

            // Create the build project.
            _projectRootElement = ProjectRootElement.Create(projectPath);

            // Include the standard targets file that defines how to build XNA Framework content.
            _projectRootElement.AddImport("$(MSBuildExtensionsPath)\\Microsoft\\XNA Game Studio\\"
                                        + "v4.0\\Microsoft.Xna.GameStudio.ContentPipeline.targets");

            _buildProject = new Project(_projectRootElement);

            _buildProject.SetProperty("XnaPlatform", "Windows");
            _buildProject.SetProperty("XnaProfile", "HiDef");
            _buildProject.SetProperty("XnaFrameworkVersion", "v4.0");
            _buildProject.SetProperty("Configuration", "Release");
            _buildProject.SetProperty("Platform", "x86");

            _buildProject.SetProperty("OutputPath", outputPath);

            // Register any custom importers or processors.
            foreach (string pipelineAssembly in PipelineAssemblies)
            {
                _buildProject.AddItem("Reference", pipelineAssembly);
            }

            //if (_buildLogger != null)
            //{
            //    _buildParameters = new BuildParameters(ProjectCollection.GlobalProjectCollection)
            //    {
            //        Loggers = new[] { _buildLogger }
            //    };
            //}
        }


        /// <summary>
        /// Adds a new content file to the MSBuild project. The importer and
        /// processor are optional: if you leave the importer null, it will
        /// be autodetected based on the file extension, and if you leave the
        /// processor null, data will be passed through without any processing.
        /// </summary>
        public void Add(string filename, string name, string importer, string processor)
        {
            ProjectItem item = _buildProject.AddItem("Compile", filename)[0];

            item.SetMetadataValue("Link", Path.GetFileName(filename));
            item.SetMetadataValue("Name", name);

            if (!string.IsNullOrEmpty(importer))
                item.SetMetadataValue("Importer", importer);

            if (!string.IsNullOrEmpty(processor))
                item.SetMetadataValue("Processor", processor);

            _projectItems.Add(item);
        }


        /// <summary>
        /// Removes all content files from the MSBuild project.
        /// </summary>
        public void Clear()
        {
            _buildProject.RemoveItems(_projectItems);

            _projectItems.Clear();
        }


        ///// <summary>
        ///// Builds all the content files which have been added to the project,
        ///// dynamically creating .xnb files in the OutputDirectory.
        ///// </summary>
        //public Task<BuildResultCode> BuildAsync()
        //{
        //    // Create and submit a new asynchronous build request.
        //    BuildManager.DefaultBuildManager.BeginBuild(_buildParameters);

        //    BuildRequestData request = new BuildRequestData(_buildProject.CreateProjectInstance(), new string[0]);
        //    BuildSubmission submission = BuildManager.DefaultBuildManager.PendBuildRequest(request);

        //    var tcs = new TaskCompletionSource<BuildResultCode>();
        //    submission.ExecuteAsync(s =>
        //                            {
        //                                tcs.SetResult(s.BuildResult.OverallResult);
        //                                BuildManager.DefaultBuildManager.EndBuild();
        //                            },
        //                            null);

        //    return tcs.Task;
        //}


        /// <summary>
        /// Asynchronously builds all the content files which have been added to the project,
        /// dynamically creating .xnb files in the OutputDirectory.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The result contains a
        /// <see cref="bool"/> flag which is <see langword="true"/> if the build was successful, and
        /// a list of strings with the console output.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public Task<Tuple<bool, IList<string>>> BuildAsync()
        {
            Logger.Info(CultureInfo.InvariantCulture, "Running MSBuild.exe.");

            _buildProject.Save(_projectRootElement.FullPath);

            return Task<Tuple<bool, IList<string>>>.Factory.StartNew(() =>
            {
                var output = new List<string>();

                if (!File.Exists(MSBuildPath))
                {
                    string message = $"Could not find MSBuild.exe. It was expected in: {MSBuildPath}";
                    Logger.Error(message);
                    _outputService.WriteLine(message);
                    output.Add(message);
                    return new Tuple<bool, IList<string>>(false, output);
                }

                var projectDirectory = Path.GetDirectoryName(_projectRootElement.FullPath);
                var projectFileName = Path.GetFileName(_projectRootElement.FullPath);
                var arguments = projectFileName + " /t:Rebuild /v:Minimal";
                _outputService.WriteLine("MSBuild.exe " + arguments);

                using (var processRunner = new ProcessRunner())
                {
                    processRunner.OutputDataReceived += (s, e) =>
                                                        {
                                                            if (e.Data == null)
                                                                return;
                                                            _outputService.WriteLine(e.Data);
                                                            output.Add(e.Data);
                                                        };
                    processRunner.ErrorDataReceived += (s, e) =>
                                                       {
                                                           if (e.Data == null)
                                                               return;
                                                           _outputService.WriteLine(e.Data);
                                                           output.Add(e.Data);
                                                       };

                    processRunner.WorkingDirectory = projectDirectory;
                    processRunner.Start(MSBuildPath, arguments);
                    processRunner.WaitForExit();
                    if (processRunner.ExitCode == 0 && File.Exists(_projectRootElement.FullPath))
                    {
                        Logger.Info("Success.");
                        //_outputService.WriteLine();
                        return new Tuple<bool, IList<string>>(true, output);
                    }
                    else
                    {
                        Logger.Info("Failed.");
                        //_outputService.WriteLine();
                        return new Tuple<bool, IList<string>>(false, output);
                    }
                }
            });
        }
        #endregion
    }
}
