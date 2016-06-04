// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using DigitalRune.Editor.Output;
using DigitalRune.Editor.Text;
using NLog;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Analyzes a shader effect using NVShaderPerf.
    /// </summary>
    internal class ShaderPerf
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IOutputService _outputService;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderPerf"/> class.
        /// </summary>
        /// <param name="outputService">The output service.</param>
        public ShaderPerf(IOutputService outputService)
        {
            if (outputService == null)
                throw new ArgumentNullException(nameof(outputService));

            _outputService = outputService;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Runs NVShaderPerf for the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// <see langword="true"/> if the run was successful; otherwise, <see langword="false"/> if
        /// NVShaderPerf was not found or if there was an error.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public bool Run(TextDocument document)
        {
            Debug.Assert(document.Uri != null, "The document needs to be saved before NVShaderPerf can be run.");

            Logger.Info(CultureInfo.InvariantCulture, "Running NVShaderPerf.exe for \"{0}\".", Path.GetFileName(document.Uri.LocalPath));

            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string shaderPerfFileName = Path.Combine(programFilesPath, @"NVIDIA Corporation\NVIDIA ShaderPerf\NVShaderPerf.exe");
            if (!File.Exists(shaderPerfFileName))
            {
                string message = "NVIDIA ShaderPerf (NVShaderPerf.exe) not found.";
                Logger.Error(message);
                _outputService.WriteLine(message);
                return false;
            }

            using (var processRunner = new ProcessRunner())
            {
                processRunner.OutputDataReceived += (s, e) => _outputService.WriteLine(e.Data);
                processRunner.ErrorDataReceived += (s, e) => _outputService.WriteLine(e.Data);

                // Analyze the passes in the effect.
                var shaderParser = new ShaderParser(new HlslIntelliSense());
                var techniquesAndPasses = shaderParser.GetTechniquesAndPasses(document.AvalonEditDocument.CreateSnapshot());
                foreach (var techniqueAndPasses in techniquesAndPasses)
                {
                    string technique = techniqueAndPasses.Item1;
                    foreach (string pass in techniqueAndPasses.Item2)
                    {
                        _outputService.WriteLine(string.Format(CultureInfo.InvariantCulture, "NVShaderPerf.exe -g G80 -t {0} -p {1} {2}\n", technique, pass, Path.GetFileName(document.Uri.LocalPath)));

                        processRunner.Start(shaderPerfFileName, string.Format(CultureInfo.InvariantCulture, "-g G80 -t {0} -p {1} -include \"{2}\" \"{3}\"", technique, pass, Path.GetDirectoryName(document.Uri.LocalPath), document.Uri.LocalPath));
                        processRunner.WaitForExit();

                        if (processRunner.ExitCode != 0)
                        {
                            // Break on error.
                            Logger.Info("Failed.");
                            //_outputService.WriteLine();
                            return false;
                        }
                    }
                }

                Logger.Info("Success.");
                //_outputService.WriteLine();
                return true;
            }
        }
        #endregion
    }
}
