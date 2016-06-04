// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using DigitalRune.Mathematics;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using NLog;


namespace DigitalRune.Editor.Status
{
    /// <summary>
    /// Represents an object that tracks the status of an operation and can be displayed the status
    /// service.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="StatusViewModel"/> can be used to show a status message or the progress of an
    /// operation. If a <see cref="CancellationTokenSource"/> is set, the view will show a
    /// <strong>Cancel</strong> button.
    /// </para>
    /// <para>
    /// <see cref="Track(Task,string,string,string,TimeSpan)"/> can be called to monitor a given
    /// task. The status information will be automatically closed once the task completes.
    /// </para>
    /// <para>
    /// A progress bar is shown if <see cref="ShowProgress"/> is set. The <see cref="Progress"/>
    /// needs to be value in the range [0, 1]. The value <see cref="double.NaN"/> can be set to show
    /// an indeterminate progress bar.
    /// </para>
    /// <para>
    /// This class also implements <see cref="IProgress{T}"/>:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Interface</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>
    /// <c>IProgress&lt;int&gt;</c>
    /// </term>
    /// <description>
    /// The range [0, 100] indicates the progress. -1 indicates that the progress is indeterminate.
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// <c>IProgress&lt;double&gt;</c>
    /// </term>
    /// <description>
    /// The range [0, 1] indicates the progress. -1 or NaN indicate that the progress is
    /// indeterminate.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// The <see cref="IStatusService"/> shows a status view model until it is closed. The 
    /// <see cref="StatusViewModel"/> is closed automatically when 
    /// <see cref="Track(Task,string,string,string,TimeSpan)"/>, 
    /// <see cref="CloseAfterAsync"/> or <see cref="CloseAfterDefaultDurationAsync"/> are used.
    /// In all other cases the view model has to be closed manually using <see cref="CloseAsync"/>.
    /// </para>
    /// <para>
    /// <strong>Thread-Safety:</strong><br/> It is safe to report the progress via
    /// <see cref="IProgress{T}"/> from a background thread. All other methods and properties may
    /// only be accessed on the UI thread!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public sealed class StatusViewModel : Screen, IProgress<double>, IProgress<int>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(3);

        private readonly Subject<double> _subject;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a status message that describes the state of the current operation.
        /// </summary>
        /// <value>The status message describing the status of the current operation.</value>
        public string Message
        {
            get { return _message; }
            set
            {
                CheckAccess();
                SetProperty(ref _message, value);
            }
        }
        private string _message;


        /// <summary>
        /// Gets or sets a value indicating whether a progress indicator should be displayed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to display a progress indicator; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        public bool ShowProgress
        {
            get { return _showProgress; }
            set
            {
                CheckAccess();
                if (SetProperty(ref _showProgress, value))
                    UpdateTaskbar();
            }
        }
        private bool _showProgress;


        /// <summary>
        /// Gets or sets a value indicating the progress of the current operation.
        /// </summary>
        /// <value>
        /// A value in the range [0, 1], which indicates the current progress. The value can be
        /// <see cref="double.NaN"/> to indicate that the progress is indeterminate. The default
        /// value is 0.
        /// </value>
        public double Progress
        {
            get { return _progress; }
            set
            {
                CheckAccess();
                if (SetProperty(ref _progress, value) && IsActive)
                    UpdateTaskbar();
            }
        }
        private double _progress;


        /// <summary>
        /// Gets or sets the <see cref="CancellationTokenSource"/> for canceling the current
        /// operation.
        /// </summary>
        /// <value>
        /// The <see cref="CancellationTokenSource"/> for canceling the current operation.
        /// </value>
        public CancellationTokenSource CancellationTokenSource
        {
            get { return _cancellationTokenSource; }
            set
            {
                CheckAccess();
                if (SetProperty(ref _cancellationTokenSource, value))
                {
                    RaisePropertyChanged(() => CanBeCanceled);
                    UpdateCommands();
                }
            }
        }
        private CancellationTokenSource _cancellationTokenSource;


        /// <summary>
        /// Gets a value indicating whether the current operation can be canceled.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the operation can be canceled; otherwise, 
        /// <see langword="false" />.
        /// </value>
        public bool CanBeCanceled
        {
            get
            {
                // Determines whether the Cancel button is visible.
                CheckAccess();
                return CancellationTokenSource != null
                       && CancellationTokenSource.Token.CanBeCanceled
                       && !IsCompleted;
            }
        }


        /// <summary>
        /// Gets the command that cancels the current operation.
        /// </summary>
        /// <value>The Cancel command.</value>
        public DelegateCommand CancelCommand { get; }


        /// <summary>
        /// Gets or sets a value indicating whether the current operation has completed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the operation has completed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                CheckAccess();
                if (SetProperty(ref _isCompleted, value))
                {
                    RaisePropertyChanged(() => CanBeCanceled);
                    UpdateCommands();
                }
            }
        }
        private bool _isCompleted;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusViewModel"/> class.
        /// </summary>
        public StatusViewModel()
        {
            CancelCommand = new DelegateCommand(Cancel, CanCancel);

            if (WindowsHelper.IsInDesignMode)
            {
                Message = "Status message.";
                Progress = 0.33;
                CancellationTokenSource = new CancellationTokenSource();
            }
            else
            {
                // Use an observable to report progress on background thread and observe progress on
                // dispatcher.
                _subject = new Subject<double>();
                _subject.Throttle(TimeSpan.FromTicks(166666))   // Ignore events within the same frame.
                        .ObserveOnDispatcher()
                        .Subscribe(Report);

                Message = "TODO: Update status message.";
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        [Conditional("DEBUG")]
        private static void CheckAccess()
        {
            if (!WindowsHelper.CheckAccess())
                throw new InvalidOperationException("View model properties and methods must be accessed on UI thread.");
        }


        /// <inheritdoc/>
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            CheckAccess();
            base.OnActivated(eventArgs);
            UpdateTaskbar();
        }


        /// <inheritdoc/>
        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            CheckAccess();
            ResetTaskbar();
            base.OnDeactivated(eventArgs);
        }


        private static void ResetTaskbar()
        {
            var taskbarInfoItem = Application.Current?.MainWindow?.TaskbarItemInfo;
            if (taskbarInfoItem == null)
                return;

            taskbarInfoItem.ProgressState = TaskbarItemProgressState.None;
            taskbarInfoItem.ProgressValue = 0;
        }


        private void UpdateTaskbar()
        {
            var window = Application.Current?.MainWindow;
            if (window == null)
                return;

            var taskbarInfoItem = window.TaskbarItemInfo;
            if (taskbarInfoItem == null)
            {
                taskbarInfoItem = new TaskbarItemInfo();
                window.TaskbarItemInfo = taskbarInfoItem;
            }

            if (ShowProgress)
            {
                if (Numeric.IsNaN(Progress))
                {
                    taskbarInfoItem.ProgressState = TaskbarItemProgressState.Indeterminate;
                    taskbarInfoItem.ProgressValue = Progress;
                }
                else
                {
                    taskbarInfoItem.ProgressState = TaskbarItemProgressState.Normal;
                    taskbarInfoItem.ProgressValue = Progress;
                }
            }
            else
            {
                taskbarInfoItem.ProgressState = TaskbarItemProgressState.None;
                taskbarInfoItem.ProgressValue = 0;
            }
        }


        private void UpdateCommands()
        {
            CancelCommand.RaiseCanExecuteChanged();
        }


        /// <overloads>
        /// <summary>
        /// Tracks the completion of the specified operation.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Tracks the completion of the specified operation.
        /// </summary>
        /// <param name="task">The operation to track.</param>
        /// <param name="completionMessage">
        /// The message shown when the operation completes successfully.
        /// </param>
        /// <param name="errorMessage">
        /// The message shown when the operation completes due to an unhandled exception.
        /// </param>
        /// <param name="cancellationMessage">
        /// The message shown when the operation completes due to being canceled.
        /// </param>
        /// <returns>
        /// A task that completes when this status view model was closed.
        /// </returns>
        public Task Track(Task task, string completionMessage = null, string errorMessage = null, string cancellationMessage = null)
        {
            CheckAccess();
            return Track(task, completionMessage, errorMessage, cancellationMessage, DefaultDuration);
        }


        /// <summary>
        /// Tracks the completion of the specified operation.
        /// </summary>
        /// <param name="task">The operation to track.</param>
        /// <param name="completionMessage">
        /// The message shown when the operation completes successfully.
        /// </param>
        /// <param name="errorMessage">
        /// The message shown when the operation completes due to an unhandled exception.
        /// </param>
        /// <param name="cancellationMessage">
        /// The message shown when the operation completes due to being canceled.
        /// </param>
        /// <param name="duration">
        /// The duration for which the status message remains visible after the operation has
        /// completed.
        /// </param>
        /// <returns>
        /// A task that completes when this status view model was closed.
        /// </returns>
        public async Task Track(Task task, string completionMessage, string errorMessage, string cancellationMessage, TimeSpan duration)
        {
            CheckAccess();
            try
            {
                await task;

                if (completionMessage != null)
                    Message = completionMessage;
            }
            catch (OperationCanceledException)
            {
                if (cancellationMessage != null)
                    Message = cancellationMessage;

                ShowProgress = false;
            }
            catch (Exception)
            {
                if (errorMessage != null)
                    Message = errorMessage;

                ShowProgress = false;
            }

            Progress = 100;

            IsCompleted = true;
            await CloseAfterAsync(duration);
        }


        private bool CanCancel()
        {
            // Disable CancelCommand if cancellation is already in progress.
            return CanBeCanceled && !CancellationTokenSource.Token.IsCancellationRequested;
        }


        /// <summary>
        /// Requests cancellation of the running operation.
        /// </summary>
        /// <remarks>
        /// The associated <see cref="CancellationToken"/> will be notified of the cancellation and
        /// will transition to a state where <see cref="CancellationToken.IsCancellationRequested"/>
        /// returns <see langword="true"/>.
        /// </remarks>
        private void Cancel()
        {
            if (CanCancel())
            {
                try
                {
                    CancellationTokenSource.Cancel();

                    // ReSharper disable once ExplicitCallerInfoArgument
                    RaisePropertyChanged(nameof(CanBeCanceled));

                    UpdateCommands();
                }
                catch (AggregateException exception)
                {
                    // Unexpected - cancellation callbacks should not throw exceptions.
                    Logger.Error(exception, "Error while canceling the operation.");
                }
            }
        }


        /// <summary>
        /// Removes the status information immediately.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The result of the task indicates
        /// whether the screen was deactivated/closed successfully.
        /// </returns>
        public Task<bool> CloseAsync()
        {
            return Conductor.DeactivateItemAsync(this, true);
        }


        /// <summary>
        /// Removes the after status information after a default duration.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The result of the task indicates
        /// whether the screen was deactivated/closed successfully.
        /// </returns>
        public Task<bool> CloseAfterDefaultDurationAsync()
        {
            return CloseAfterAsync(DefaultDuration);
        }


        /// <summary>
        /// Removes the after status information after the specified duration.
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The result of the task indicates
        /// whether the screen was deactivated/closed successfully.
        /// </returns>
        public async Task<bool> CloseAfterAsync(TimeSpan duration)
        {
            CheckAccess();
            await Task.Delay(duration);
            return await Conductor.DeactivateItemAsync(this, true);
        }


        #region ----- IProgress<T> -----

        /// <summary>
        /// Reports a progress update.
        /// </summary>
        /// <param name="value">The value of the updated progress.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IProgress<double>.Report(double value)
        {
            _subject.OnNext(value);
        }


        /// <summary>
        /// Reports a progress update.
        /// </summary>
        /// <param name="value">The value of the updated progress.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IProgress<int>.Report(int value)
        {
            _subject.OnNext(value / 100.0);
        }


        private void Report(double value)
        {
            CheckAccess();
            if (value >= 0)
            {
                ShowProgress = true;
                Progress = value;
            }
            else
            {
                ShowProgress = true;
                Progress = double.NaN;
            }
        }
        #endregion

        #endregion
    }
}
