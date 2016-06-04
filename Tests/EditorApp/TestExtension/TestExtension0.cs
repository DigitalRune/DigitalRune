using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DigitalRune.Collections;
using DigitalRune.Windows.Framework;
using DigitalRune.Editor;
using DigitalRune.Editor.About;
using DigitalRune.Editor.Options;
using DigitalRune.Editor.Status;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Themes;


namespace EditorApp
{
    public sealed class TestExtension0 : EditorExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private ResourceDictionary _resourceDictionary;
        private MergeableNodeCollection<ICommandItem> _toolBarNodes;            // Toolbar items
        private MergeableNodeCollection<OptionsPageViewModel> _optionsNodes;    // Option pages
        private EditorExtensionDescription _extensionDescription;               // About information
        private bool _isTestItemChecked;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        protected override void OnInitialize()
        {
        }


        protected override void OnStartup()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/TestExtension/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
            EditorHelper.RegisterResources(_resourceDictionary);

            AddCommandItems();
            AddToolBarNodes();

            AddOptionsNodes();

            _extensionDescription = new EditorExtensionDescription
            {
                Name = "TestExtension0",
                Description = "This is a simple test extension.",
                Icon = MultiColorGlyphs.Plugin,
                Version = "1.0.0.0",
            };
            Editor.Services.GetInstance<IAboutService>()?.ExtensionDescriptions.Add(_extensionDescription);
        }

        protected override void OnShutdown()
        {
            Editor.Services.GetInstance<IAboutService>()?.ExtensionDescriptions.Remove(_extensionDescription);

            Editor.Services.GetInstance<IOptionsService>()?.OptionsNodeCollections.Remove(_optionsNodes);
            _optionsNodes = null;

            Editor.ToolBarNodeCollections.Remove(_toolBarNodes);
            _toolBarNodes = null;

            CommandItems.Clear();

            EditorHelper.UnregisterResources(_resourceDictionary);
        }


        protected override void OnUninitialize()
        {
        }


        private void AddCommandItems()
        {
            var testCommand = new DelegateCommand(OnTestItemExecute);
            CommandItems.Add(new DelegateCommandItem("Test", testCommand)
            {
                AlwaysShowText = false,
                Category = "Misc",
                InputGestures = new InputGestureCollection(new[] { new MultiKeyGesture(new[] { Key.A, Key.S }, ModifierKeys.Control) }),
                Icon = MultiColorGlyphs.SplitWindow,
                IsCheckable = true,
                IsChecked = false,
                Text = "Test Item",
                ToolTip = "This issues the Test command.",
            });

            var visibilityCommand = new DelegateCommand(() => ((DelegateCommandItem)CommandItems[0]).IsVisible = !CommandItems[0].IsVisible);
            CommandItems.Add(new DelegateCommandItem("Visibility", visibilityCommand)
            {
                Category = "Misc",
                InputGestures = new InputGestureCollection(new[] { new KeyGesture(Key.V, ModifierKeys.Alt) }),
                IsCheckable = true,
                Text = "_Toggle Test Item Visibility",
                ToolTip = "Toggle Test Item Visibility",
            });

            CommandItems.Add(new DelegateCommandItem("TestWindow", new DelegateCommand(OpenTestWindow))
            {
                Text = "Test",
                ToolTip = "Show the test window",
            });

            CommandItems.Add(new DelegateCommandItem("GC", new DelegateCommand(CollectGarbage))
            {
                Text = "GC",
                ToolTip = "Full garbage collection",
            });

            CommandItems.Add(new DelegateCommandItem("Throw", new DelegateCommand(Throw))
            {
                Text = "Throw",
                ToolTip = "Throw an exception",
            });

            CommandItems.Add(new DelegateCommandItem("Status1", new DelegateCommand(TestStatus1))
            {
                Text = "Status #1",
                ToolTip = "Tests the status service.",
            });

            CommandItems.Add(new DelegateCommandItem("Status2", new DelegateCommand(TestStatus2))
            {
                Text = "Status #2",
                ToolTip = "Tests the status service.",
            });
        }


        private void AddToolBarNodes()
        {
            _toolBarNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("TestGroup", "_Test") { AlwaysShowText = true },
                    new MergeableNode<ICommandItem>(CommandItems["TestWindow"]),
                    new MergeableNode<ICommandItem>(CommandItems["GC"]),
                    new MergeableNode<ICommandItem>(CommandItems["Throw"]),
                    new MergeableNode<ICommandItem>(CommandItems["Status1"]),
                    new MergeableNode<ICommandItem>(CommandItems["Status2"])),
            };

            Editor.ToolBarNodeCollections.Add(_toolBarNodes);
        }


        private void AddOptionsNodes()
        {
            _optionsNodes = new MergeableNodeCollection<OptionsPageViewModel>
            {
                new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Group0"),
                    new MergeableNode<OptionsPageViewModel>(new TestOptionsPageViewModel("Options 0")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup0")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup1")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup2")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup3")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup4"))),
                new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Group1"),
                    new MergeableNode<OptionsPageViewModel>(new TestOptionsPageViewModel("Options 1")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup0")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup1")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup2")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup3")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup4"))),
                new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Group2"),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup0")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup1")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup2")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup3")),
                    new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("Subgroup4")),
                    new MergeableNode<OptionsPageViewModel>(new TestOptionsPageViewModel("Options 2"))),
            };

            var optionsService = Editor.Services.GetInstance<IOptionsService>().WarnIfMissing();
            optionsService?.OptionsNodeCollections.Add(_optionsNodes);
        }


        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            if (dockId == TestViewModel.DockIdString)
                return new TestViewModel(Editor);

            return null;
        }




        private void OnTestItemExecute()
        {
            // Note: We need to store the state in a custom variable.
            // (ICommandItem.IsChecked is bound to a menu item and a toolbar button.
            // Menu items and toolbar buttons bind "IsChecked" two way by default, so they would 
            // automatically toggle the state. However, we have also set up a key binding. The key 
            // binding needs to be handled explicitly.
            // To not get confused with the different ways IsChecked might change, we store the state
            // of the button explicitly in the variable _isTestItemChecked.
            _isTestItemChecked = !_isTestItemChecked;
            ((DelegateCommandItem)CommandItems[0]).IsChecked = _isTestItemChecked;
        }


        private void OpenTestWindow()
        {
            Editor.ActivateItem(new TestViewModel(Editor));
        }


        private static void CollectGarbage()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }


        private static void Throw()
        {
            throw new Exception("This is a test exception.\nThe quick brown fox jumps over the lazy dog.");
        }


        private void TestStatus1()
        {
            // Simulate a short operation.
            var statusService = Editor.Services.GetInstance<IStatusService>();
            var status = new StatusViewModel
            {
                Message = "Operation in progress...",
                ShowProgress = true,
                Progress = 0
            };
            statusService.Show(status);

            Observable.Interval(TimeSpan.FromMilliseconds(25))
                      .Take(50)
                      .ObserveOnDispatcher()    // StatusViewModel properties and methods need to be accessed on UI thread.
                      .Subscribe(_ =>
                      {
                          status.Progress += 0.02;
                      },
                      () =>
                      {
                          status.Message = "Operation completed successfully.";
                          status.CloseAfterDefaultDurationAsync();
                      });
        }


        private void TestStatus2()
        {
            var statusService = Editor.Services.GetInstance<IStatusService>();

            // Simulate a short operation.
            var status = new StatusViewModel
            {
                Message = "Operation in progress...",
                CancellationTokenSource = new CancellationTokenSource(),
            };

            var task = DummyOperationAsync(status.CancellationTokenSource.Token, status);
            statusService.Show(status);
            status.Track(task, "Operation completed successfully.", "Operation failed.", "Operation canceled.");
        }


        private static async Task DummyOperationAsync(CancellationToken token, IProgress<int> progress)
        {
            int value = 0;
            while (value < 100)
            {
                token.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromMilliseconds(25), token).ConfigureAwait(false);
                value += 2;
                progress.Report(value); // IProgress<T>.Report can be called from background thread.
            }
        }
        #endregion
    }
}
