// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Runs an external process, allows to write to its standard input stream and reads the
    /// standard output and standard error streams.
    /// </summary>
    /// <example>
    /// The following example code calls a command to sort 3 lines of text.
    /// <code lang="csharp">
    /// <![CDATA[var processRunner = new ProcessRunner();
    /// processRunner.Start(@"C:\Windows\System32\sort.exe");
    /// processRunner.StandardInput.WriteLine("C");
    /// processRunner.StandardInput.WriteLine("A");
    /// processRunner.StandardInput.WriteLine("B");
    /// processRunner.StandardInput.Close();
    /// processRunner.WaitForExit();
    /// 
    /// Console.Out.WriteLine(processRunner.StandardOutput);
    /// processRunner.Dispose();
    /// ]]>
    /// </code>
    /// </example>
    public class ProcessRunner : IDisposable
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private Process _process;
        private readonly ManualResetEvent _errorComplete = new ManualResetEvent(true);
        private readonly ManualResetEvent _outputComplete = new ManualResetEvent(true);
        private readonly StringBuilder _standardOutput = new StringBuilder();
        private readonly StringBuilder _standardError = new StringBuilder();
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
        /// Gets search paths for files, directories for temporary files, application-specific options, 
        /// and other similar information.
        /// </summary>
        /// <remarks>
        /// A dictionary that provides environment variables that apply to the process and child 
        /// processes.
        /// </remarks>
        public Dictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        /// <value>The working directory.</value>
        public string WorkingDirectory { get; set; }


        /// <summary>
        /// Gets the standard input of the associated process.
        /// </summary>
        /// <value>
        /// The standard input of the associated process. Returns <see langword="null"/> if no process
        /// is running.
        /// </value>
        public TextWriter StandardInput
        {
            get
            {
                ThrowIfDisposed();
                return _process?.StandardInput;
            }
        }


        /// <summary>
        /// Gets the error output of the associated process.
        /// </summary>
        public string StandardError
        {
            get
            {
                ThrowIfDisposed();
                lock (_standardError)
                  return _standardError.ToString();
            }
        }


        /// <summary>
        /// Gets the output of the associated process.
        /// </summary>
        public string StandardOutput
        {
            get
            {
                ThrowIfDisposed();
                lock (_standardOutput)
                  return _standardOutput.ToString();
            }
        }


        /// <summary>
        /// Gets the value that the associated process specified when it terminated.
        /// </summary>
        /// <value>
        /// The exit value set by the associated process when it terminated. (Returns 0 if no process
        /// has been started, or if the process has been killed.)
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// The process is still running.
        /// </exception>
        public int ExitCode
        {
            get
            {
                ThrowIfDisposed();
                return _process?.ExitCode ?? 0;
            }
        }


        /// <summary>
        /// Gets a value indicating whether the associated process is running.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the associated process is running; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        public bool IsRunning
        {
            get
            {
                ThrowIfDisposed();
                return (_process != null) && !_process.HasExited;
            }
        }


        /// <summary>
        /// Occurs when the associated process writes a line to the standard error stream.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> ErrorDataReceived;


        /// <summary>
        /// Occurs when the associated process writes a line to the standard output stream.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> OutputDataReceived;


        /// <summary>
        /// Occurs when the associated process exited.
        /// </summary>
        public event EventHandler<EventArgs> ProcessExited;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRunner"/> class.
        /// </summary>
        public ProcessRunner()
        {
            WorkingDirectory = Environment.CurrentDirectory;
        }


        /// <summary>
        /// Releases unmanaged resources before an instance of the <see cref="ProcessRunner"/> class
        /// is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// This method releases unmanaged resources by calling the virtual
        /// <see cref="Dispose(bool)"/> method, passing in <see langword="false"/>.
        /// </remarks>
        ~ProcessRunner()
        {
            Dispose(false);
        }


        /// <summary>
        /// Releases all resources used by an instance of the <see cref="ProcessRunner"/> class.
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
        /// Releases the unmanaged resources used by an instance of the <see cref="ProcessRunner"/>
        /// class and optionally releases the managed resources.
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
                    if (_process != null)
                    {
                        _process.ErrorDataReceived -= OnErrorDataReceived;
                        _process.OutputDataReceived -= OnOutputDataReceived;
                        _process.Exited -= OnProcessExited;
                        _process.Dispose();
                        _process = null;
                    }

                    _errorComplete.Dispose();
                    _outputComplete.Dispose();
                }

                IsDisposed = true;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }


        /// <summary>
        /// Starts the specified process.
        /// </summary>
        /// <param name="fileName">The name of an application file to run in the process.</param>
        /// <param name="arguments">Command-line arguments to pass when starting the process.</param>
        /// <exception cref="InvalidOperationException">
        /// The process is already running.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void Start(string fileName, string arguments = "")
        {
            ThrowIfDisposed();
            if (_process != null)
            {
                if (!_process.HasExited)
                    throw new InvalidOperationException("Unable to start process because process is already running.");

                _process.ErrorDataReceived -= OnErrorDataReceived;
                _process.OutputDataReceived -= OnOutputDataReceived;
                _process.Exited -= OnProcessExited;
                _process.Dispose();
            }

            lock(_standardError)
                _standardError.Clear();
            lock(_standardOutput)
                _standardOutput.Clear();

            _errorComplete.Reset();
            _outputComplete.Reset();

            _process = new Process
            {
                StartInfo =
                {
                    Arguments = arguments,
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    FileName = fileName,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = WorkingDirectory
                }
            };
            foreach (var variable in EnvironmentVariables)
                _process.StartInfo.EnvironmentVariables.Add(variable.Key, variable.Value);

            // Event handlers.
            _process.EnableRaisingEvents = true;
            _process.ErrorDataReceived += OnErrorDataReceived;
            _process.OutputDataReceived += OnOutputDataReceived;
            _process.Exited += OnProcessExited;

            // Start process.
            try
            {
                _process.Start();
            }
            catch (Exception)
            {
                _process.ErrorDataReceived -= OnErrorDataReceived;
                _process.OutputDataReceived -= OnOutputDataReceived;
                _process.Exited -= OnProcessExited;
                _process.Dispose();
                _process = null;

                throw;
            }

            // Asynchronously read standard error and output.
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();
        }


        /// <summary>
        /// Immediately stops the associated process.
        /// </summary>
        public void Kill()
        {
            ThrowIfDisposed();
            if (_process != null)
            {
                if (_process.HasExited)
                {
                    // Process has already terminated.
                    _process = null;
                }
                else
                {
                    // Kill process.
                    _process.Kill();
                    _process.Dispose();
                    _process = null;

                    _errorComplete.WaitOne();
                    _outputComplete.WaitOne();
                }
            }
        }


        /// <summary>
        /// Waits indefinitely for the associated process to exit.
        /// </summary>
        /// <remarks>
        /// Return immediately if no process is currently running. The method automatically closes the
        /// standard input stream.
        /// </remarks>
        public void WaitForExit()
        {
            ThrowIfDisposed();
            if (_process == null)
                return;

            _process.StandardInput.Close();
            _process.WaitForExit();
            _errorComplete.WaitOne();
            _outputComplete.WaitOne();
        }


        /// <summary>
        /// Waits the specified number of milliseconds for the associated process to exit.
        /// </summary>
        /// <param name="milliseconds">The milliseconds.</param>
        /// <inheritdoc cref="WaitForExit()"/>
        public void WaitForExit(int milliseconds)
        {
            ThrowIfDisposed();
            if (_process == null)
                return;

            _process.StandardInput.Close();
            bool processExited = _process.WaitForExit(milliseconds);
            if (processExited)
            {
                _errorComplete.WaitOne(milliseconds);
                _outputComplete.WaitOne(milliseconds);
            }
        }


        private void OnErrorDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            if (eventArgs.Data == null)
            {
                // Error output of process is complete.
                _errorComplete.Set();
            }

            lock (_standardError)
              _standardError.AppendLine(eventArgs.Data);

            // Raise ErrorDataReceived event.
            OnErrorDataReceived(eventArgs);
        }


        private void OnOutputDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            if (eventArgs.Data == null)
            {
                // Output of process is complete.
                _outputComplete.Set();
            }

            lock (_standardOutput)
              _standardOutput.AppendLine(eventArgs.Data);

            // Raise OutputDataReceived event.
            OnOutputDataReceived(eventArgs);
        }


        private void OnProcessExited(object sender, EventArgs eventArgs)
        {
            // Wait for output to complete.
            _errorComplete.WaitOne();
            _outputComplete.WaitOne();

            // Raise ProcessExited event.
            OnProcessExited(eventArgs);
        }


        /// <summary>
        /// Raises the <see cref="ErrorDataReceived"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> <br/>
        /// When overriding <see cref="OnErrorDataReceived(DataReceivedEventArgs)"/> in a derived class,
        /// be sure to call the base class's <see cref="OnErrorDataReceived(DataReceivedEventArgs)"/> 
        /// method so that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnErrorDataReceived(DataReceivedEventArgs eventArgs)
        {
            ErrorDataReceived?.Invoke(this, eventArgs);
        }


        /// <summary>
        /// Raises the <see cref="OutputDataReceived"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> <br/>
        /// When overriding <see cref="OnOutputDataReceived(DataReceivedEventArgs)"/> in a derived class, 
        /// be sure to call the base class's <see cref="OnOutputDataReceived(DataReceivedEventArgs)"/> 
        /// method so that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnOutputDataReceived(DataReceivedEventArgs eventArgs)
        {
            OutputDataReceived?.Invoke(this, eventArgs);
        }


        /// <summary>
        /// Raises the <see cref="ProcessExited"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> <br/>
        /// When overriding <see cref="OnProcessExited(EventArgs)"/> in a derived class, be sure to call
        /// the base class's <see cref="OnProcessExited(EventArgs)"/> method so that registered 
        /// delegates receive the event.
        /// </remarks>
        protected virtual void OnProcessExited(EventArgs eventArgs)
        {
            ProcessExited?.Invoke(this, eventArgs);
        }
        #endregion
    }
}
