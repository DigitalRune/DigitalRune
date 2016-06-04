// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Reactive.Linq;
using DigitalRune.Windows;
using static System.FormattableString;


namespace DigitalRune.Editor.Status
{
    /// <summary>
    /// Shows the total managed memory at regular intervals.
    /// </summary>
    internal class TotalMemoryViewModel : ObservableObject, IDisposable
    {
        private readonly IDisposable _subscription;


        /// <summary>
        /// Gets a value indicating whether this instance has been disposed of.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance has been disposed of; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsDisposed { get; private set; }


        /// <summary>
        /// Gets or sets the message showing the total managed memory.
        /// </summary>
        /// <value>The message showing the total managed memory.</value>
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        private string _message;


        /// <summary>
        /// Gets or sets the tool tip.
        /// </summary>
        /// <value>The tool tip.</value>
        public string ToolTip
        {
            get { return _toolTip; }
            set { SetProperty(ref _toolTip, value); }
        }
        private string _toolTip;


        /// <summary>
        /// Initializes a new instance of the <see cref="TotalMemoryViewModel"/> class.
        /// </summary>
        public TotalMemoryViewModel()
        {
            SetTotalMemory(GC.GetTotalMemory(false));

            if (!WindowsHelper.IsInDesignMode)
            {
                _subscription = Observable.Interval(TimeSpan.FromSeconds(1))
                                          .Select(_ => GC.GetTotalMemory(false))
                                          .ObserveOnDispatcher()
                                          .Subscribe(SetTotalMemory);
            }
        }


        /// <summary>
        /// Releases all resources used by an instance of the <see cref="TotalMemoryViewModel"/>
        /// class.
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
        /// <see cref="TotalMemoryViewModel"/> class and optionally releases the managed resources.
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
                    _subscription?.Dispose();
                }

                IsDisposed = true;
            }
        }


        private void SetTotalMemory(long bytes)
        {
            int mb = (int)(bytes / 1024.0 / 1024.0 + 0.5);
            Message = Invariant($"{mb:N0} MiB");
            ToolTip = Invariant($"Managed memory: {mb:N0} MiB ({bytes:N0} bytes)");
        }
    }
}
