// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using DigitalRune.Editor.Output;
using NLog;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Compiles a shader effect using the DirectX Effect Compiler (fxc.exe).
    /// </summary>
    internal class Fxc
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
        /// Initializes a new instance of the <see cref="Fxc"/> class.
        /// </summary>
        /// <param name="outputService">The output service.</param>
        public Fxc(IOutputService outputService)
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
        /// Asynchronously builds all the content files which have been added to the project,
        /// dynamically creating .xnb files in the OutputDirectory.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The result contains a
        /// <see cref="bool"/> flag which is <see langword="true"/> if the build was successful, and
        /// a list of strings with the console output.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public Task<Tuple<bool, IList<string>>> BuildAsync(string fileName)
        {
            Logger.Info(CultureInfo.InvariantCulture, "Running MSBuild.exe.");

            return Task<Tuple<bool, IList<string>>>.Factory.StartNew(() =>
            {
                var output = new List<string>();

                string fxcPath = Environment.GetEnvironmentVariable("DXSDK_DIR");
                if (string.IsNullOrEmpty(fxcPath))
                {
                    string message = "Environment variable DXSDK_DIR not found.";
                    Logger.Error(message);
                    _outputService.WriteLine(message);
                    output.Add(message);
                    return new Tuple<bool, IList<string>>(false, output);
                }

                string fxcFileName = Path.Combine(fxcPath, @"Utilities\Bin\x86\fxc.exe");
                if (!File.Exists(fxcFileName))
                {
                    string message = "Microsoft (R) Direct3D Shader Compiler (fxc.exe) not found.";
                    Logger.Error(message);
                    _outputService.WriteLine(message);
                    output.Add(message);
                    return new Tuple<bool, IList<string>>(false, output);
                }

                _outputService.WriteLine($"fxc.exe /T fx_2_0 {Path.GetFileName(fileName)}\n");

                using (var processRunner = new ProcessRunner())
                {
                    processRunner.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            _outputService.WriteLine(e.Data);
                            output.Add(e.Data);
                        }
                    };
                    processRunner.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            _outputService.WriteLine(e.Data);
                            output.Add(e.Data);
                        }
                    };

                    //processRunner.WorkingDirectory = ...
                    processRunner.Start(fxcFileName, $"/T fx_2_0 \"{fileName}\"");
                    processRunner.WaitForExit();
                    if (processRunner.ExitCode == 0)
                    {
                        Logger.Info("Success.");
                        return new Tuple<bool, IList<string>>(true, output);
                    }
                    else
                    {
                        Logger.Info("Failed.");
                        return new Tuple<bool, IList<string>>(false, output);
                    }
                }
            });
        }


        /// <summary>
        /// Compiles the specified file using fxc.exe.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// The file name of the created assembler code if the compilation was successful; otherwise,
        /// <see langword="null"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public string Run(string fileName)
        {
            Logger.Info(CultureInfo.InvariantCulture, "Running Fxc.exe for \"{0}\".", Path.GetFileName(fileName));

            string fxcPath = Environment.GetEnvironmentVariable("DXSDK_DIR");
            if (string.IsNullOrEmpty(fxcPath))
            {
                string message = "Environment variable DXSDK_DIR not found.";
                Logger.Error(message);
                _outputService.WriteLine(message);
                return null;
            }

            string fxcFileName = Path.Combine(fxcPath, @"Utilities\Bin\x86\fxc.exe");
            if (!File.Exists(fxcFileName))
            {
                string message = "Microsoft (R) Direct3D Shader Compiler (fxc.exe) not found.";
                Logger.Error(message);
                _outputService.WriteLine(message);
                return null;
            }

            _outputService.WriteLine($"fxc.exe /T fx_2_0 {Path.GetFileName(fileName)}\n");

            using (var processRunner = new ProcessRunner())
            {
                processRunner.OutputDataReceived += (s, e) => _outputService.WriteLine(e.Data);
                processRunner.ErrorDataReceived += (s, e) => _outputService.WriteLine(e.Data);

                string outputFile = Path.GetTempFileName();
                processRunner.Start(fxcFileName, $"/T fx_2_0 /Fc \"{outputFile}\" \"{fileName}\"");
                processRunner.WaitForExit();
                if (processRunner.ExitCode == 0 && File.Exists(outputFile))
                {
                    Logger.Info("Success.");
                    return outputFile;
                }
                else
                {
                    Logger.Info("Failed.");

                    if (File.Exists(outputFile))
                        File.Delete(outputFile);

                    return null;
                }
            }
        }
        #endregion
    }
}
