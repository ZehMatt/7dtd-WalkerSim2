using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WalkerSim;

namespace Editor.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        private readonly Simulation _simulation = Simulation.Instance;
        private readonly System.Random _prng = new System.Random((int)DateTime.Now.Ticks);

        private static readonly float WorldSizeX = 6000;
        private static readonly float WorldSizeY = 6000;
        private static readonly Vector3 WorldMins = new Vector3(-(WorldSizeX * 0.5f), -(WorldSizeY * 0.5f), 0);
        private static readonly Vector3 WorldMaxs = new Vector3(WorldSizeX * 0.5f, WorldSizeY * 0.5f, 256);

        [ObservableProperty]
        private Config _config = Config.GetDefault();

        [ObservableProperty]
        private Models.MovementProcessorGroupModel? _selectedSystem;

        partial void OnSelectedSystemChanged(Models.MovementProcessorGroupModel? value)
        {
            OnPropertyChanged(nameof(HasSelectedSystem));
            SelectedProcessor = null;
            RefreshGroupIndexOptions();
        }

        public bool HasSelectedSystem => SelectedSystem != null;

        // Group Index options for the dropdown
        public struct GroupIndexOption
        {
            public int Value;
            public string Text;
            public override string ToString() => Text;
        }

        private List<GroupIndexOption> _availableGroupIndexOptions = new List<GroupIndexOption>();
        public List<GroupIndexOption> AvailableGroupIndexOptions
        {
            get => _availableGroupIndexOptions;
            private set { _availableGroupIndexOptions = value; OnPropertyChanged(); }
        }

        private bool _suppressGroupIndexWrite = false;
        private GroupIndexOption? _selectedGroupIndexOption;
        public GroupIndexOption? SelectedGroupIndexOption
        {
            get => _selectedGroupIndexOption;
            set
            {
                _selectedGroupIndexOption = value;
                OnPropertyChanged();
                if (!_suppressGroupIndexWrite && SelectedSystem != null && value != null)
                    SelectedSystem.GroupIndex = value.Value.Value;
            }
        }

        private void RefreshGroupIndexOptions()
        {
            var options = new List<GroupIndexOption>();
            options.Add(new GroupIndexOption { Value = -1, Text = "Any" });

            var totalGroups = Config.GroupSize > 0
                ? (Config.PopulationDensity + Config.GroupSize - 1) / Config.GroupSize
                : 0;

            var currentGroupIndex = SelectedSystem?.GroupIndex ?? -1;
            var usedIndices = new HashSet<int>(
                MovementSystems
                    .Where(s => s != SelectedSystem && s.GroupIndex >= 0)
                    .Select(s => s.GroupIndex));

            for (int i = 0; i < totalGroups; i++)
            {
                if (usedIndices.Contains(i)) continue;
                options.Add(new GroupIndexOption { Value = i, Text = i.ToString() });
            }

            // Always include current value even if out of range
            if (currentGroupIndex >= 0 && !options.Any(o => o.Value == currentGroupIndex))
                options.Add(new GroupIndexOption { Value = currentGroupIndex, Text = currentGroupIndex.ToString() });

            // Suppress write-back while ItemsSource changes; the ComboBox may fire the
            // binding with a stale or null value as it resets, which would corrupt the model.
            _suppressGroupIndexWrite = true;
            AvailableGroupIndexOptions = options;
            _selectedGroupIndexOption = options.Find(o => o.Value == currentGroupIndex) is { } found ? found : (GroupIndexOption?)null;
            _suppressGroupIndexWrite = false;
            OnPropertyChanged(nameof(SelectedGroupIndexOption));
        }

        [ObservableProperty]
        private Models.MovementProcessorModel? _selectedProcessor;

        partial void OnSelectedProcessorChanged(Models.MovementProcessorModel? value)
        {
            OnPropertyChanged(nameof(HasSelectedProcessor));
        }

        public bool HasSelectedProcessor => SelectedProcessor != null;

        // Wrapper properties for Config fields (Config uses fields, not properties, so we need wrappers for binding)
        public int PopulationDensity
        {
            get => Config.PopulationDensity;
            set { Config.PopulationDensity = value; OnPropertyChanged(); if (!_suppressReset) ResetSimulation(); }
        }

        public int GroupSize
        {
            get => Config.GroupSize;
            set { Config.GroupSize = value; OnPropertyChanged(); if (!_suppressReset) ResetSimulation(); }
        }

        public int RandomSeed
        {
            get => Config.RandomSeed;
            set { Config.RandomSeed = value; OnPropertyChanged(); }
        }

        public int SpawnActivationRadius
        {
            get => Config.SpawnActivationRadius;
            set { Config.SpawnActivationRadius = value; OnPropertyChanged(); }
        }

        public Config.WorldLocation StartPosition
        {
            get => Config.StartPosition;
            set { Config.StartPosition = value; OnPropertyChanged(); if (!_suppressReset) ResetSimulation(); }
        }

        public Config.WorldLocation RespawnPosition
        {
            get => Config.RespawnPosition;
            set { Config.RespawnPosition = value; OnPropertyChanged(); if (!_suppressReset) ResetSimulation(); }
        }

        public bool StartAgentsGrouped
        {
            get => Config.StartAgentsGrouped;
            set { Config.StartAgentsGrouped = value; OnPropertyChanged(); if (!_suppressReset) ResetSimulation(); }
        }

        public bool EnhancedSoundAwareness
        {
            get => Config.EnhancedSoundAwareness;
            set { Config.EnhancedSoundAwareness = value; OnPropertyChanged(); }
        }

        public float SoundDistanceScale
        {
            get => Config.SoundDistanceScale;
            set { Config.SoundDistanceScale = value; OnPropertyChanged(); }
        }

        public bool FastForwardAtStart
        {
            get => Config.FastForwardAtStart;
            set { Config.FastForwardAtStart = value; OnPropertyChanged(); }
        }

        public bool PauseDuringBloodmoon
        {
            get => Config.PauseDuringBloodmoon;
            set { Config.PauseDuringBloodmoon = value; OnPropertyChanged(); }
        }

        public uint SpawnProtectionTime
        {
            get => Config.SpawnProtectionTime;
            set { Config.SpawnProtectionTime = value; OnPropertyChanged(); }
        }

        [ObservableProperty]
        private bool _isSimulationRunning = false;

        partial void OnIsSimulationRunningChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNotRunning));
        }

        public bool IsNotRunning => !IsSimulationRunning;

        [ObservableProperty]
        private bool _isSimulationPaused = false;

        [ObservableProperty]
        private int _totalAgents = 0;

        [ObservableProperty]
        private int _activeAgents = 0;

        [ObservableProperty]
        private int _inactiveAgents = 0;

        [ObservableProperty]
        private int _groupCount = 0;

        [ObservableProperty]
        private ulong _simulationTicks = 0;

        [ObservableProperty]
        private string _simulationTime = "00:00:00.000";

        [ObservableProperty]
        private double _averageSimTime = 0;

        [ObservableProperty]
        private double _averageUpdateTime = 0;

        public ObservableCollection<Models.MovementProcessorGroupModel> MovementSystems { get; }

        public System.Collections.ObjectModel.ObservableCollection<Models.LogEntry> LogEntries { get; } = new System.Collections.ObjectModel.ObservableCollection<Models.LogEntry>();

        public Config.WorldLocation[] StartPositionOptions { get; } = (Config.WorldLocation[])Enum.GetValues(typeof(Config.WorldLocation));

        public Config.WorldLocation[] RespawnPositionOptions { get; } = (Config.WorldLocation[])Enum.GetValues(typeof(Config.WorldLocation));

        public Config.PostSpawnBehavior[] PostSpawnBehaviorOptions { get; } = (Config.PostSpawnBehavior[])Enum.GetValues(typeof(Config.PostSpawnBehavior));

        public Config.WanderingSpeed[] WanderingSpeedOptions { get; } = (Config.WanderingSpeed[])Enum.GetValues(typeof(Config.WanderingSpeed));

        public Config.MovementProcessorType[] MovementProcessorTypeOptions { get; } = (Config.MovementProcessorType[])Enum.GetValues(typeof(Config.MovementProcessorType));

        // Suppress simulation reset during bulk config updates (e.g. import, OnConfigChanged)
        private bool _suppressReset = false;

        // World selection
        private List<string> _worldFolders = new List<string>();

        public Action WorldLoaded;

        [ObservableProperty]
        private ObservableCollection<string> _worldNames = new ObservableCollection<string>();

        [ObservableProperty]
        private string _selectedWorldName = null;

        public IAsyncRelayCommand ImportConfigurationCommand => new AsyncRelayCommand(ImportConfiguration);

        public IAsyncRelayCommand ExportConfigurationCommand => new AsyncRelayCommand(ExportConfiguration);

        public EditorViewModel()
        {
            // Initialize movement systems from config
            MovementSystems = new ObservableCollection<Models.MovementProcessorGroupModel>(
                Config.Processors.Select(p => new Models.MovementProcessorGroupModel(p))
            );

            // Setup logging
            Logging.AddSink(this);

            // Initialize simulation in editor mode
            _simulation.EditorMode = true;
            _simulation.SetWorldSize(WorldMins, WorldMaxs);
            ResetSimulation();

            // Discover worlds asynchronously so the UI isn't blocked
            Task.Run(LoadWorldList);
        }

        private void LoadWorldList()
        {
            var folders = WorldLocator.FindWorldFolders();
            Dispatcher.UIThread.Post(() =>
            {
                _worldFolders = folders;
                WorldNames.Clear();
                foreach (var f in folders)
                    WorldNames.Add(Path.GetFileName(f));

                Logging.Info($"Found {folders.Count} world(s).");
            });
        }

        partial void OnSelectedWorldNameChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            if (WorldLocator.TryGetWorldPath(_worldFolders, value, out var worldPath))
            {
                _simulation.LoadMapData(worldPath, value);
                ResetSimulation();
                Logging.Info($"Loaded world: {value}");
                WorldLoaded?.Invoke();
            }
        }

        // Called after Config property changes
        partial void OnConfigChanged(Config value)
        {
            _suppressReset = true;
            // Notify all wrapper properties to refresh their values from the new Config
            OnPropertyChanged(nameof(PopulationDensity));
            OnPropertyChanged(nameof(GroupSize));
            OnPropertyChanged(nameof(RandomSeed));
            OnPropertyChanged(nameof(SpawnActivationRadius));
            OnPropertyChanged(nameof(StartPosition));
            OnPropertyChanged(nameof(RespawnPosition));
            OnPropertyChanged(nameof(StartAgentsGrouped));
            OnPropertyChanged(nameof(EnhancedSoundAwareness));
            OnPropertyChanged(nameof(SoundDistanceScale));
            OnPropertyChanged(nameof(FastForwardAtStart));
            OnPropertyChanged(nameof(PauseDuringBloodmoon));
            OnPropertyChanged(nameof(SpawnProtectionTime));
            _suppressReset = false;
        }

        [RelayCommand]
        public void AddMovementSystem()
        {
            var newGroup = new Config.MovementProcessorGroup
            {
                Group = MovementSystems.Count == 0 ? -1 : 0,
                SpeedScale = 1.0f,
                PostSpawnBehavior = Config.PostSpawnBehavior.Wander
            };

            Config.Processors.Add(newGroup);
            var model = new Models.MovementProcessorGroupModel(newGroup);
            model.Name = $"System {MovementSystems.Count + 1}";
            MovementSystems.Add(model);
            SelectedSystem = model;
        }

        [RelayCommand]
        public void StartSimulation()
        {
            if (IsSimulationRunning) return;

            _simulation.Start();
            IsSimulationRunning = true;
            IsSimulationPaused = false;
            Logging.Info("Simulation started");
        }

        [RelayCommand]
        public void StopSimulation()
        {
            if (!IsSimulationRunning) return;

            _simulation.Stop();
            IsSimulationRunning = false;
            IsSimulationPaused = false;
            Logging.Info("Simulation stopped");
        }

        [RelayCommand]
        public void PauseSimulation()
        {
            if (!IsSimulationRunning || IsSimulationPaused) return;

            _simulation.SetPaused(true);
            IsSimulationPaused = true;
            Logging.Info("Simulation paused");
        }

        [RelayCommand]
        public void ResumeSimulation()
        {
            if (!IsSimulationRunning || !IsSimulationPaused) return;

            _simulation.SetPaused(false);
            IsSimulationPaused = false;
            Logging.Info("Simulation resumed");
        }

        [RelayCommand]
        public void ResetSimulation()
        {
            bool wasRunning = IsSimulationRunning;
            if (wasRunning)
            {
                StopSimulation();
            }

            _simulation.Reset(Config);
            UpdateSimulationStats();
            Logging.Info("Simulation reset");

            if (wasRunning)
            {
                StartSimulation();
            }
        }

        [RelayCommand]
        public void AdvanceOneTick()
        {
            if (IsSimulationRunning && !IsSimulationPaused) return;

            _simulation.Advance(1);
            UpdateSimulationStats();
        }

        [RelayCommand]
        public void SetSimulationSpeed(int speed)
        {
            _simulation.TimeScale = speed;
            Logging.Info($"Simulation speed set to {speed}x");
        }

        public void UpdateSimulationStats()
        {
            TotalAgents = _simulation.AgentCount;
            ActiveAgents = _simulation.ActiveCount;
            InactiveAgents = TotalAgents - ActiveAgents;
            GroupCount = _simulation.GroupCount;
            SimulationTicks = _simulation.Ticks;

            var secsElapsed = _simulation.GetSimulationTimeSeconds();
            var timeSpan = TimeSpan.FromSeconds(secsElapsed);
            SimulationTime = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", 
                timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

            AverageSimTime = _simulation.AverageSimTime * 1000.0; // Convert to ms
            AverageUpdateTime = _simulation.AverageUpdateTime * 1000.0; // Convert to ms
        }

        [RelayCommand]
        public void DuplicateMovementSystem()
        {
            if (SelectedSystem == null) return;

            var source = SelectedSystem.Underlying;
            var newGroup = new Config.MovementProcessorGroup
            {
                Group = source.Group,
                SpeedScale = source.SpeedScale,
                PostSpawnBehavior = source.PostSpawnBehavior,
                PostSpawnWanderSpeed = source.PostSpawnWanderSpeed,
                Color = source.Color
            };

            // Deep copy processors
            foreach (var proc in source.Entries)
            {
                newGroup.Entries.Add(new Config.MovementProcessor
                {
                    Type = proc.Type,
                    Distance = proc.Distance,
                    Power = proc.Power
                });
            }

            Config.Processors.Add(newGroup);
            var model = new Models.MovementProcessorGroupModel(newGroup);
            model.Name = $"{SelectedSystem.Name} (Copy)";
            MovementSystems.Add(model);
            SelectedSystem = model;
        }

        [RelayCommand]
        public void RemoveMovementSystem()
        {
            if (SelectedSystem == null) return;

            Config.Processors.Remove(SelectedSystem.Underlying);
            MovementSystems.Remove(SelectedSystem);
            SelectedSystem = MovementSystems.FirstOrDefault();
        }

        [RelayCommand]
        public void ClearLog()
        {
            LogEntries.Clear();
        }

        private async Task ImportConfiguration()
        {
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null) return;

            try
            {
                var files = await mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Load Configuration",
                    FileTypeFilter = new[] { new FilePickerFileType("XML Files") { Patterns = new[] { "*.xml" } } },
                    AllowMultiple = false
                });

                if (files != null && files.Count > 0 && files[0] != null)
                {
                    var file = files[0];
                    var filePath = file.TryGetLocalPath();
                    if (filePath == null)
                    {
                        Logging.Err("Could not get file path");
                        return;
                    }

                    var loadedConfig = WalkerSim.Config.LoadFromFile(filePath);

                    if (loadedConfig == null)
                    {
                        Logging.Err("Failed to load configuration file");
                        return;
                    }

                    Config = loadedConfig;
                    Logging.Info($"Configuration loaded from {System.IO.Path.GetFileName(filePath)}");

                    // Re-initialize movement systems from new config
                    _suppressReset = true;
                    MovementSystems.Clear();
                    int sysIdx = 0;
                    foreach (var proc in Config.Processors)
                    {
                        var m = new Models.MovementProcessorGroupModel(proc);
                        m.Name = $"System {++sysIdx}";
                        MovementSystems.Add(m);
                    }
                    SelectedSystem = MovementSystems.FirstOrDefault();
                    _suppressReset = false;

                    // Reset simulation with new config (OnConfigChanged will refresh all UI bindings)
                    ResetSimulation();
                }
            }
            catch (System.Exception ex)
            {
                Logging.Exception(ex);
                Logging.Err($"Failed to import configuration: {ex.Message}");
            }
        }

        private async Task ExportConfiguration()
        {
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null) return;

            try
            {
                var file = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Export Configuration",
                    DefaultExtension = "xml",
                    FileTypeChoices = new[] { new FilePickerFileType("XML Files") { Patterns = new[] { "*.xml" } } },
                    SuggestedFileName = "WalkerSim.xml"
                });

                if (file != null)
                {
                    var path = file.TryGetLocalPath();
                    if (path == null)
                    {
                        Logging.Err("Could not get file path");
                        return;
                    }

                    using (var writer = new System.IO.StreamWriter(path))
                    {
                        Config.Export(writer);
                    }

                    Logging.Info($"Configuration exported to {System.IO.Path.GetFileName(path)}");
                }
            }
            catch (System.Exception ex)
            {
                Logging.Exception(ex);
                Logging.Err($"Failed to export configuration: {ex.Message}");
            }
        }    }

    // Implement Logging ISink to receive log messages
    public partial class EditorViewModel : Logging.ISink
    {
        public Action<Models.LogEntry> LogEntryAdded;

        public void Message(Logging.Level level, string message)
        {
            // The simulation runs on a background thread — always dispatch to UI thread
            if (Dispatcher.UIThread.CheckAccess())
            {
                var entry = new Models.LogEntry(level, message);
                LogEntries.Add(entry);
                LogEntryAdded?.Invoke(entry);
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var entry = new Models.LogEntry(level, message);
                    LogEntries.Add(entry);
                    LogEntryAdded?.Invoke(entry);
                });
            }
        }
    }
}
