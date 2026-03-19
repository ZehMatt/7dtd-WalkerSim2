using Avalonia;
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
        private bool _agentsDirty = false;
        private readonly System.Random _prng = new System.Random((int)DateTime.Now.Ticks);

        [ObservableProperty]
        private bool _hasUnsavedChanges = false;

        private static readonly float WorldSizeX = 6000;
        private static readonly float WorldSizeY = 6000;
        private static readonly Vector3 WorldMins = new Vector3(-(WorldSizeX * 0.5f), -(WorldSizeY * 0.5f), 0);
        private static readonly Vector3 WorldMaxs = new Vector3(WorldSizeX * 0.5f, WorldSizeY * 0.5f, 256);

        [ObservableProperty]
        private Config _config = LoadDefaultConfig();

        private static Config LoadDefaultConfig()
        {
            try
            {
                var uri = new Uri("avares://Editor/Assets/WalkerSim.xml");
                using var stream = Avalonia.Platform.AssetLoader.Open(uri);
                using var reader = new System.IO.StreamReader(stream);
                var cfg = Config.LoadFromStream(reader);
                if (cfg != null)
                    return cfg;
            }
            catch { }
            return Config.GetDefault();
        }

        [ObservableProperty]
        private Models.MovementProcessorGroupModel? _selectedSystem;

        partial void OnSelectedSystemChanged(Models.MovementProcessorGroupModel? value)
        {
            SelectedProcessor = null;
            OnPropertyChanged(nameof(HasSelectedSystem));
            OnPropertyChanged(nameof(HasSelectedSystemOnly));
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(HasNoSelection));
        }

        [ObservableProperty]
        private Models.MovementProcessorModel? _selectedProcessor;

        partial void OnSelectedProcessorChanged(Models.MovementProcessorModel? value)
        {
            OnPropertyChanged(nameof(HasSelectedSystem));
            OnPropertyChanged(nameof(HasSelectedProcessor));
            OnPropertyChanged(nameof(HasSelectedSystemOnly));
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(HasNoSelection));
            OnPropertyChanged(nameof(MovementProcessorTypeOptions));
        }

        // HasSelectedSystem = a system is selected (regardless of whether a processor is also selected).
        // Used to gate "+ Processor" availability.
        public bool HasSelectedSystem => SelectedSystem != null;
        public bool HasSelectedProcessor => SelectedProcessor != null;
        public bool HasSelection => SelectedSystem != null || SelectedProcessor != null;
        public bool HasSelectedSystemOnly => SelectedSystem != null && SelectedProcessor == null;
        public bool HasNoSelection => !HasSelection;

        // TreeView two-way binding target.  Setting it updates SelectedSystem / SelectedProcessor.
        [ObservableProperty]
        private object? _treeSelectedItem;

        partial void OnTreeSelectedItemChanged(object? value)
        {
            if (value is Models.MovementProcessorGroupModel sys)
            {
                // Explicitly clear processor — OnSelectedSystemChanged won't fire
                // if SelectedSystem is already this system (e.g. clicking the system
                // node after having one of its processors selected).
                SelectedProcessor = null;
                SelectedSystem = sys;
            }
            else if (value is Models.MovementProcessorModel proc)
            {
                // Set SelectedSystem first so its changed handler clears
                // SelectedProcessor, then set SelectedProcessor after.
                SelectedSystem = MovementSystems.FirstOrDefault(s => s.Processors.Contains(proc));
                SelectedProcessor = proc;
            }
            else
            {
                SelectedSystem = null;
                SelectedProcessor = null;
            }
        }

        private void MarkDirty()
        {
            if (!_suppressReset)
                HasUnsavedChanges = true;
        }

        // Wrapper properties for Config fields (Config uses fields, not properties, so we need wrappers for binding)
        public int PopulationDensity
        {
            get => Config.PopulationDensity;
            set { Config.PopulationDensity = value; OnPropertyChanged(); MarkDirty(); RefreshAllSystemIndices(); if (!_suppressReset) ResetSimulation(); }
        }

        public int GroupSize
        {
            get => Config.GroupSize;
            set { Config.GroupSize = value; OnPropertyChanged(); MarkDirty(); RefreshAllSystemIndices(); if (!_suppressReset) ResetSimulation(); }
        }

        public int RandomSeed
        {
            get => Config.RandomSeed;
            set { Config.RandomSeed = value; OnPropertyChanged(); MarkDirty(); }
        }

        public int SpawnActivationRadius
        {
            get => Config.SpawnActivationRadius;
            set { Config.SpawnActivationRadius = value; OnPropertyChanged(); MarkDirty(); }
        }

        public Config.WorldLocation StartPosition
        {
            get => Config.StartPosition;
            set { Config.StartPosition = value; OnPropertyChanged(); MarkDirty(); if (!_suppressReset) ResetSimulation(); }
        }

        public Config.WorldLocation RespawnPosition
        {
            get => Config.RespawnPosition;
            set { Config.RespawnPosition = value; OnPropertyChanged(); MarkDirty(); if (!_suppressReset) ResetSimulation(); }
        }

        public bool StartAgentsGrouped
        {
            get => Config.StartAgentsGrouped;
            set { Config.StartAgentsGrouped = value; OnPropertyChanged(); MarkDirty(); if (!_suppressReset) ResetSimulation(); }
        }

        public bool EnhancedSoundAwareness
        {
            get => Config.EnhancedSoundAwareness;
            set { Config.EnhancedSoundAwareness = value; OnPropertyChanged(); MarkDirty(); }
        }

        public float SoundDistanceScale
        {
            get => Config.SoundDistanceScale;
            set { Config.SoundDistanceScale = value; OnPropertyChanged(); MarkDirty(); }
        }

        public bool PauseDuringBloodmoon
        {
            get => Config.PauseDuringBloodmoon;
            set { Config.PauseDuringBloodmoon = value; OnPropertyChanged(); MarkDirty(); }
        }

        public uint SpawnProtectionTime
        {
            get => Config.SpawnProtectionTime;
            set { Config.SpawnProtectionTime = value; OnPropertyChanged(); MarkDirty(); if (!_suppressReset) ResetSimulation(); }
        }

        public bool InfiniteZombieLifetime
        {
            get => Config.InfiniteZombieLifetime;
            set { Config.InfiniteZombieLifetime = value; OnPropertyChanged(); MarkDirty(); }
        }

        public float PopulationStartPercent
        {
            get => Config.PopulationStartPercent;
            set { Config.PopulationStartPercent = value; OnPropertyChanged(); MarkDirty(); if (!_suppressReset) ResetSimulation(); }
        }

        public int FullPopulationAtDay
        {
            get => Config.FullPopulationAtDay;
            set { Config.FullPopulationAtDay = value; OnPropertyChanged(); MarkDirty(); if (!_suppressReset) ResetSimulation(); }
        }

        // Wrapper properties for movement processor parameters to support live editing
        private Models.MovementProcessorModel? _selectedMovementProcessor;

        public Models.MovementProcessorModel SelectedMovementProcessor
        {
            get => _selectedMovementProcessor ??= MovementSystems.FirstOrDefault(s => s.Processors.Count > 0).Processors[0];
            set { _selectedMovementProcessor = value; OnPropertyChanged(); }
        }

        public float MovementProcessorDistance
        {
            get => SelectedMovementProcessor?.Underlying.Distance ?? 30.0f;
            set
            {
                var proc = SelectedMovementProcessor?.Underlying;
                if (proc != null)
                {
                    proc.Distance = value;
                    OnPropertyChanged();
                    if (!_suppressReset)
                        _simulation.ReloadConfig(Config);
                }
            }
        }

        public float MovementProcessorPower
        {
            get => SelectedMovementProcessor?.Underlying.Power ?? 0.5f;
            set
            {
                var proc = SelectedMovementProcessor?.Underlying;
                if (proc != null)
                {
                    proc.Power = value;
                    OnPropertyChanged();
                    if (!_suppressReset)
                        _simulation.ReloadConfig(Config);
                }
            }
        }

        [ObservableProperty]
        private bool _isSimulationRunning = false;

        partial void OnIsSimulationRunningChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNotRunning));
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanStop));
            OnPropertyChanged(nameof(CanPause));
            OnPropertyChanged(nameof(CanResume));
            OnPropertyChanged(nameof(CanAdvance));
            OnPropertyChanged(nameof(CanSetSpeed));
        }

        public bool IsNotRunning => !IsSimulationRunning;
        public bool CanStart => !IsSimulationRunning;
        public bool CanStop => IsSimulationRunning;
        public bool CanPause => IsSimulationRunning && !IsSimulationPaused;
        public bool CanResume => IsSimulationRunning && IsSimulationPaused;
        public bool CanAdvance => !IsSimulationRunning || IsSimulationPaused;
        public bool CanSetSpeed => IsSimulationRunning;

        [ObservableProperty]
        private bool _isSimulationPaused = false;

        partial void OnIsSimulationPausedChanged(bool value)
        {
            OnPropertyChanged(nameof(CanPause));
            OnPropertyChanged(nameof(CanResume));
            OnPropertyChanged(nameof(CanAdvance));
        }

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
        private string _tickTime = "0.00000 ms.";

        [ObservableProperty]
        private string _updateTime = "0.00000 ms.";

        [ObservableProperty]
        private string _windDirection = "";

        [ObservableProperty]
        private string _windDirectionTarget = "";

        [ObservableProperty]
        private uint _tickNextWindChange = 0;

        public ObservableCollection<Models.MovementProcessorGroupModel> MovementSystems { get; }

        public ObservableCollection<object> AgentListItems { get; } = new ObservableCollection<object>();

        [ObservableProperty]
        private Models.AgentModel? _selectedAgent;

        partial void OnSelectedAgentChanged(Models.AgentModel? value)
        {
            OnPropertyChanged(nameof(HasSelectedAgent));
            // If tracking, switch to the newly selected agent automatically.
            if (IsTrackingAgent && value != null)
                TrackAgentRequested?.Invoke(value.Underlying);
            else if (IsTrackingAgent && value == null)
            {
                IsTrackingAgent = false;
                StopTrackingRequested?.Invoke();
            }
        }

        public bool HasSelectedAgent => SelectedAgent != null;

        [ObservableProperty]
        private bool _isTrackingAgent;

        partial void OnIsTrackingAgentChanged(bool value) => OnPropertyChanged(nameof(IsNotTrackingAgent));
        public bool IsNotTrackingAgent => !IsTrackingAgent;

        // ── Tool state ────────────────────────────────────────────────────────────

        public enum EditorTool { None, EmitSound, Kill, AddPlayer, SetPlayerPosition }

        [ObservableProperty]
        private EditorTool _activeTool = EditorTool.None;

        partial void OnActiveToolChanged(EditorTool value)
        {
            OnPropertyChanged(nameof(IsToolActive));
            OnPropertyChanged(nameof(ActiveToolPreviewRadius));
        }

        [ObservableProperty]
        private float _soundRadius = 90.0f;

        [ObservableProperty]
        private float _killRadius = 650.0f;

        public bool IsToolActive => ActiveTool != EditorTool.None;

        /// <summary>World-space preview circle radius for the active tool, or NaN for point tools.</summary>
        public float ActiveToolPreviewRadius => ActiveTool switch
        {
            EditorTool.EmitSound => SoundRadius,
            EditorTool.Kill => KillRadius,
            _ => float.NaN,
        };

        /// <summary>Called by SimulationCanvas when the user clicks the world.</summary>
        public void HandleCanvasClick(Vector3 position)
        {
            switch (ActiveTool)
            {
                case EditorTool.EmitSound:
                    _simulation.AddSoundEvent(position, SoundRadius, 20.0f);
                    Logging.Info($"Emitted sound at {position}, radius: {SoundRadius}");
                    break;

                case EditorTool.Kill:
                    int killed = _simulation.KillAgentsInRadius(position, KillRadius);
                    Logging.Info($"Killed {killed} agent(s) at {position}");
                    break;

                case EditorTool.AddPlayer:
                    _simulation.AddPlayer(0, position, 0);
                    Logging.Info($"Added player at {position}");
                    ActiveTool = EditorTool.None;
                    break;

                case EditorTool.SetPlayerPosition:
                    _simulation.UpdatePlayer(0, position, true);
                    Logging.Info($"Set player position to {position}");
                    ActiveTool = EditorTool.None;
                    break;
            }
        }

        [RelayCommand] public void ActivateEmitSound() => ActiveTool = EditorTool.EmitSound;
        [RelayCommand] public void ActivateKill() => ActiveTool = EditorTool.Kill;
        [RelayCommand] public void ActivateAddPlayer() => ActiveTool = EditorTool.AddPlayer;
        [RelayCommand] public void ActivateSetPlayerPosition() => ActiveTool = EditorTool.SetPlayerPosition;
        [RelayCommand] public void CancelTool() => ActiveTool = EditorTool.None;

        // Wired up by MainWindow.axaml.cs to the SimulationCanvas.
        public Action<WalkerSim.Agent>? NavigateToAgentRequested;
        public Action<WalkerSim.Agent>? TrackAgentRequested;
        public Action? StopTrackingRequested;
        public Action? GroupColorsChanged;

        [RelayCommand]
        public void GoToAgent()
        {
            if (SelectedAgent != null)
                NavigateToAgentRequested?.Invoke(SelectedAgent.Underlying);
        }

        [RelayCommand]
        public void TrackSelectedAgent()
        {
            if (SelectedAgent == null)
                return;
            IsTrackingAgent = true;
            TrackAgentRequested?.Invoke(SelectedAgent.Underlying);
        }

        [RelayCommand]
        public void StopTrackingAgent()
        {
            IsTrackingAgent = false;
            StopTrackingRequested?.Invoke();
        }

        private object? _selectedAgentListItem;
        public object? SelectedAgentListItem
        {
            get => _selectedAgentListItem;
            set
            {
                // Group headers are not selectable — ignore and clear
                if (value is Models.AgentGroupHeader)
                {
                    _selectedAgentListItem = null;
                    SelectedAgent = null;
                    OnPropertyChanged();
                    return;
                }
                _selectedAgentListItem = value;
                OnPropertyChanged();
                SelectedAgent = value as Models.AgentModel;
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<Models.LogEntry> LogEntries { get; } = new System.Collections.ObjectModel.ObservableCollection<Models.LogEntry>();

        public Config.WorldLocation[] StartPositionOptions { get; } = Enum.GetValues<Config.WorldLocation>();

        public Config.WorldLocation[] RespawnPositionOptions { get; } = Enum.GetValues<Config.WorldLocation>();

        public Config.PostSpawnBehavior[] PostSpawnBehaviorOptions { get; } = Enum.GetValues<Config.PostSpawnBehavior>();

        public Config.WanderingSpeed[] WanderingSpeedOptions { get; } = Enum.GetValues<Config.WanderingSpeed>();

        private static readonly Config.MovementProcessorType[] AllProcessorTypes =
            Enum.GetValues<Config.MovementProcessorType>()
            .Where(t => t != Config.MovementProcessorType.Invalid).ToArray();

        /// <summary>
        /// Returns processor types available for the currently selected processor.
        /// Excludes types already used by sibling processors in the same system,
        /// but always includes the selected processor's own current type.
        /// </summary>
        public Config.MovementProcessorType[] MovementProcessorTypeOptions
        {
            get
            {
                var sys = SelectedSystem;
                var proc = SelectedProcessor;
                if (sys == null)
                    return AllProcessorTypes;

                var usedTypes = new System.Collections.Generic.HashSet<Config.MovementProcessorType>();
                foreach (var p in sys.Processors)
                {
                    if (p != proc) // Don't exclude the current processor's own type.
                        usedTypes.Add(p.Type);
                }

                return AllProcessorTypes.Where(t => !usedTypes.Contains(t)).ToArray();
            }
        }

        public WalkerSim.Agent.State[] AgentStateOptions { get; } = Enum.GetValues<WalkerSim.Agent.State>();
        public WalkerSim.Agent.SubState[] AgentSubStateOptions { get; } = Enum.GetValues<WalkerSim.Agent.SubState>();
        public WalkerSim.Agent.TravelState[] AgentTravelStateOptions { get; } = Enum.GetValues<WalkerSim.Agent.TravelState>();
        public WalkerSim.Agent.MoveType[] AgentMoveTypeOptions { get; } = Enum.GetValues<WalkerSim.Agent.MoveType>();

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

        public IAsyncRelayCommand LoadStateCommand => new AsyncRelayCommand(LoadState);

        public IAsyncRelayCommand SaveStateCommand => new AsyncRelayCommand(SaveState);

        public EditorViewModel()
        {
            Logging.Info("Creating view model...");
            using (Logging.Scope())
            {
                // Initialize movement systems from config
                Logging.Info("Initializing movement systems...");
                MovementSystems = new ObservableCollection<Models.MovementProcessorGroupModel>(
                    Config.Processors.Select(p => CreateMovementSystemModel(p))
                );
                RefreshAllSystemIndices();

                // Setup logging
                Logging.AddSink(this);

                // Initialize simulation in editor mode
                Logging.Info("Initializing simulation...");
                _simulation.EditorMode = true;
                _simulation.SetWorldSize(WorldMins, WorldMaxs);
                ResetSimulation();

                // Discover worlds asynchronously so the UI isn't blocked
                Logging.Info("Starting world discovery...");
                Task.Run(LoadWorldList);
            }
        }

        private void LoadWorldList()
        {
            Logging.Info("Discovering worlds...");
            List<string> folders;
            try
            {
                folders = WorldLocator.FindWorldFolders();
            }
            catch (Exception ex)
            {
                Logging.Err("Failed to discover worlds: {0}", ex.Message);
                folders = new List<string>();
            }

            Logging.Info("Found {0} world folder(s), posting to UI thread...", folders.Count);
            Dispatcher.UIThread.Post(() =>
            {
                Logging.Info("Populating world list...");
                _worldFolders = folders;
                WorldNames.Clear();
                foreach (var f in folders)
                    WorldNames.Add(Path.GetFileName(f));

                Logging.Info($"Found {folders.Count} world(s).");

                // Default to Navezgane if present
                if (SelectedWorldName == null)
                {
                    var navezgane = WorldNames.FirstOrDefault(n =>
                        n.Equals("Navezgane", StringComparison.OrdinalIgnoreCase));
                    if (navezgane != null)
                    {
                        Logging.Info("Auto-selecting world: {0}", navezgane);
                        SelectedWorldName = navezgane;
                    }
                }
            });
        }

        public void ReloadWorldList()
        {
            Task.Run(LoadWorldList);
        }

        partial void OnSelectedWorldNameChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            if (WorldLocator.TryGetWorldPath(_worldFolders, value, out var worldPath))
            {
                var worldName = value;
                Logging.Info("Loading world '{0}' from: {1}", worldName, worldPath);
                Task.Run(() =>
                {
                    Logging.Info("Loading map data for '{0}'...", worldName);
                    var result = _simulation.LoadMapData(worldPath, worldName);
                    Logging.Info("Map data load {0} for '{1}'.", result ? "succeeded" : "failed", worldName);
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (result)
                        {
                            Logging.Info("Resetting simulation for '{0}'...", worldName);
                            ResetSimulation();
                            Logging.Info($"Loaded world: {worldName}");
                            WorldLoaded?.Invoke();
                        }
                        else
                        {
                            Logging.Err($"Failed to load world: {worldName}");
                        }
                    });
                });
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
            OnPropertyChanged(nameof(PauseDuringBloodmoon));
            OnPropertyChanged(nameof(SpawnProtectionTime));
            OnPropertyChanged(nameof(InfiniteZombieLifetime));
            OnPropertyChanged(nameof(PopulationStartPercent));
            OnPropertyChanged(nameof(FullPopulationAtDay));
            _suppressReset = false;
        }

        [RelayCommand]
        public void AddProcessorToSelectedSystem()
        {
            var proc = SelectedSystem?.AddProcessor();
            if (proc != null)
                TreeSelectedItem = proc;
        }

        [RelayCommand]
        public void DuplicateSelected()
        {
            // Only systems can be duplicated, not individual processors.
            var sys = SelectedSystem ?? (SelectedProcessor != null
                ? MovementSystems.FirstOrDefault(s => s.Processors.Contains(SelectedProcessor))
                : null);
            if (sys != null)
                DuplicateMovementSystem(sys);
        }

        [RelayCommand]
        public void RemoveSelected()
        {
            // Capture locals — TreeView selection changes during Remove can
            // null out SelectedProcessor/SelectedSystem mid-method.
            var proc = SelectedProcessor;
            var sys = SelectedSystem;

            if (proc != null && sys != null)
            {
                var procs = sys.Processors;
                int idx = procs.IndexOf(proc);
                sys.RemoveProcessor(proc);
                SelectedProcessor = null;
                if (procs.Count > 0)
                    TreeSelectedItem = procs[idx < procs.Count ? idx : procs.Count - 1];
                else
                    TreeSelectedItem = sys;
            }
            else if (sys != null)
            {
                int idx = MovementSystems.IndexOf(sys);
                RemoveMovementSystem(sys);
                if (MovementSystems.Count > 0)
                    TreeSelectedItem = MovementSystems[idx < MovementSystems.Count ? idx : MovementSystems.Count - 1];
                else
                    TreeSelectedItem = null;
            }
        }

        [RelayCommand]
        public void AddMovementSystem()
        {
            var idx = Config.Processors.Count;
            var c = WalkerSim.Drawing.ColorTable.GetColorForIndex(idx);
            var newGroup = new Config.MovementProcessorGroup
            {
                Weight = 1.0f,
                SpeedScale = 1.0f,
                PostSpawnBehavior = Config.PostSpawnBehavior.Wander,
                Color = $"#{c.R:X2}{c.G:X2}{c.B:X2}"
            };

            Config.Processors.Add(newGroup);
            var model = CreateMovementSystemModel(newGroup);
            model.Name = $"System {MovementSystems.Count + 1}";
            MovementSystems.Add(model);
            RefreshAllSystemIndices();
            ReloadConfigLive();
        }

        [RelayCommand]
        public void StartSimulation()
        {
            if (IsSimulationRunning)
                return;

            _simulation.Reset(Config);
            _simulation.Start();
            IsSimulationRunning = true;
            IsSimulationPaused = false;
        }

        [RelayCommand]
        public void StopSimulation()
        {
            if (!IsSimulationRunning)
                return;

            _simulation.Stop();
            IsSimulationRunning = false;
            IsSimulationPaused = false;
        }

        [RelayCommand]
        public void PauseSimulation()
        {
            if (!IsSimulationRunning || IsSimulationPaused)
                return;

            _simulation.Stop();
            IsSimulationPaused = true;
        }

        [RelayCommand]
        public void ResumeSimulation()
        {
            if (!IsSimulationRunning || !IsSimulationPaused)
                return;

            _simulation.Start();
            IsSimulationPaused = false;
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
            _agentsDirty = true;
            GroupColorsChanged?.Invoke();
            UpdateSimulationStats();

            if (wasRunning)
            {
                StartSimulation();
            }
        }

        [RelayCommand]
        public void AdvanceOneTick()
        {
            if (IsSimulationRunning && !IsSimulationPaused)
                return;

            _simulation.Advance(1);
            UpdateSimulationStats();
        }

        [RelayCommand]
        public void SetSimulationSpeed(string speedStr)
        {
            if (int.TryParse(speedStr, out var speed))
            {
                _simulation.TimeScale = speed;
                CurrentSpeed = speed;
                Logging.Info($"Simulation speed set to {speed}x");
            }
        }

        [ObservableProperty]
        private int _currentSpeed = 1;

        public void RefreshAgentModels()
        {
            var agents = _simulation.Agents;

            // Count current agent models in the list
            int currentAgentCount = 0;
            foreach (var item in AgentListItems)
                if (item is Models.AgentModel)
                    currentAgentCount++;

            if (currentAgentCount == agents.Count && !_agentsDirty)
            {
                // Structure unchanged: just pull live data into the selected agent
                SelectedAgent?.Pull();
                return;
            }

            _agentsDirty = false;

            // Structure changed (e.g. after a reset): rebuild grouped list
            var prevUnderlying = SelectedAgent?.Underlying;
            AgentListItems.Clear();

            var grouped = System.Linq.Enumerable.OrderBy(
                System.Linq.Enumerable.GroupBy(agents, a => a.Group),
                g => g.Key);

            // Build group-to-system name mapping.
            var groupToSystem = _simulation.GroupToSystemIndex;

            Models.AgentModel? newSelected = null;
            foreach (var group in grouped)
            {
                var systemName = "";
                if (groupToSystem != null && group.Key >= 0 && group.Key < groupToSystem.Length)
                {
                    var sysIdx = groupToSystem[group.Key];
                    if (sysIdx >= 0 && sysIdx < MovementSystems.Count)
                        systemName = MovementSystems[sysIdx].DisplayName;
                }
                AgentListItems.Add(new Models.AgentGroupHeader(group.Key, systemName));
                foreach (var agent in group)
                {
                    var model = new Models.AgentModel(agent);
                    if (agent == prevUnderlying)
                        newSelected = model;
                    AgentListItems.Add(model);
                }
            }

            // Restore selection
            _selectedAgentListItem = newSelected;
            SelectedAgent = newSelected;
            OnPropertyChanged(nameof(SelectedAgentListItem));
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

            var simTimeMs = _simulation.AverageSimTime * 1000.0;
            var updateTimeMs = _simulation.AverageUpdateTime * 1000.0;
            TickTime = string.Format("{0:0.00000} ms. ({1:0.000}/s)",
                simTimeMs, simTimeMs > 0 ? 1000.0 / simTimeMs : 0);
            UpdateTime = string.Format("{0:0.00000} ms. ({1:0.000}/s)",
                updateTimeMs, updateTimeMs > 0 ? 1000.0 / updateTimeMs : 0);

            var wd = _simulation.WindDirection;
            WindDirection = string.Format("{0:0.00}, {1:0.00}", wd.X, wd.Y);
            var wdt = _simulation.WindDirectionTarget;
            WindDirectionTarget = string.Format("{0:0.00}, {1:0.00}", wdt.X, wdt.Y);
            TickNextWindChange = _simulation.TickNextWindChange;
        }

        [RelayCommand]
        public void DuplicateMovementSystem(Models.MovementProcessorGroupModel? system)
        {
            if (system == null)
                return;

            var source = system.Underlying;
            var newGroup = new Config.MovementProcessorGroup
            {
                Weight = source.Weight,
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
                    Power = proc.Power,
                    Param1 = proc.Param1,
                    Param2 = proc.Param2
                });
            }

            Config.Processors.Add(newGroup);
            var model = CreateMovementSystemModel(newGroup);
            model.Name = $"{system.DisplayName} (Copy)";
            MovementSystems.Add(model);
            RefreshAllSystemIndices();
            ReloadConfigLive();
        }

        [RelayCommand]
        public void RemoveMovementSystem(Models.MovementProcessorGroupModel? system)
        {
            if (system == null)
                return;

            Config.Processors.Remove(system.Underlying);
            MovementSystems.Remove(system);
            RefreshAllSystemIndices();
            ReloadConfigLive();
        }

        private void RefreshAllSystemIndices()
        {
            for (int i = 0; i < MovementSystems.Count; i++)
            {
                MovementSystems[i].SystemIndex = i + 1;
            }
        }

        private Models.MovementProcessorGroupModel CreateMovementSystemModel(Config.MovementProcessorGroup group)
        {
            var model = new Models.MovementProcessorGroupModel(group);
            model.ConfigChanged = ReloadConfigLive;

            // Wire up ConfigChanged for all existing processors
            foreach (var proc in model.Processors)
            {
                proc.ConfigChanged = ReloadConfigLive;
            }

            return model;
        }

        private void ReloadConfigLive()
        {
            if (!_suppressReset)
            {
                HasUnsavedChanges = true;
                _simulation.ReloadConfig(Config);
                GroupColorsChanged?.Invoke();
            }
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

            if (mainWindow == null)
                return;

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
                        var m = CreateMovementSystemModel(proc);
                        if (string.IsNullOrEmpty(m.Name))
                            m.Name = $"System {++sysIdx}";
                        else
                            sysIdx++;
                        MovementSystems.Add(m);
                    }
                    RefreshAllSystemIndices();
                    _suppressReset = false;

                    // Reset simulation with new config (OnConfigChanged will refresh all UI bindings)
                    ResetSimulation();
                    HasUnsavedChanges = false;
                }
            }
            catch (System.Exception ex)
            {
                Logging.Exception(ex);
                Logging.Err($"Failed to load configuration: {ex.Message}");
            }
        }

        private async Task ExportConfiguration()
        {
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null)
                return;

            try
            {
                var file = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Configuration",
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

                    HasUnsavedChanges = false;
                    Logging.Info($"Configuration saved to {System.IO.Path.GetFileName(path)}");
                }
            }
            catch (System.Exception ex)
            {
                Logging.Exception(ex);
                Logging.Err($"Failed to save configuration: {ex.Message}");
            }
        }

        private async Task LoadState()
        {
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null)
                return;

            try
            {
                var files = await mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Load State Save",
                    FileTypeFilter = new[] { new FilePickerFileType("State Files") { Patterns = new[] { "*.bin" } } },
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

                    // Stop simulation if running
                    if (IsSimulationRunning)
                    {
                        StopSimulation();
                    }

                    if (!_simulation.Load(filePath))
                    {
                        Logging.Err("Failed to load state save file");
                        return;
                    }

                    // Update config from loaded state
                    Config = _simulation.Config;

                    // Update world selection if loaded world is available
                    var loadedWorldName = _simulation.WorldName;
                    if (!string.IsNullOrEmpty(loadedWorldName))
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            SelectedWorldName = loadedWorldName;
                        });
                    }

                    // Re-initialize movement systems from loaded config
                    _suppressReset = true;
                    MovementSystems.Clear();
                    int sysIdx = 0;
                    foreach (var proc in Config.Processors)
                    {
                        var m = CreateMovementSystemModel(proc);
                        if (string.IsNullOrEmpty(m.Name))
                            m.Name = $"System {++sysIdx}";
                        else
                            sysIdx++;
                        MovementSystems.Add(m);
                    }
                    RefreshAllSystemIndices();
                    _suppressReset = false;

                    // Update stats
                    UpdateSimulationStats();

                    // Count dead agents
                    int numDead = 0;
                    foreach (var agent in _simulation.Agents)
                    {
                        if (agent.CurrentState == WalkerSim.Agent.State.Dead)
                            numDead++;
                    }

                    Logging.Info($"Loaded state save with {_simulation.AgentCount} agents, {numDead} dead.");
                }
            }
            catch (System.Exception ex)
            {
                Logging.Exception(ex);
                Logging.Err($"Failed to load state: {ex.Message}");
            }
        }

        private async Task SaveState()
        {
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null)
                return;

            try
            {
                var file = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save State",
                    DefaultExtension = "bin",
                    FileTypeChoices = new[] { new FilePickerFileType("State Files") { Patterns = new[] { "*.bin" } } },
                    SuggestedFileName = "WalkerSim.bin"
                });

                if (file != null)
                {
                    var path = file.TryGetLocalPath();
                    if (path == null)
                    {
                        Logging.Err("Could not get file path");
                        return;
                    }

                    if (!_simulation.Save(path))
                    {
                        Logging.Err("Failed to save state");
                        return;
                    }

                    Logging.Info($"State saved to {System.IO.Path.GetFileName(path)}");
                }
            }
            catch (System.Exception ex)
            {
                Logging.Exception(ex);
                Logging.Err($"Failed to save state: {ex.Message}");
            }
        }
    }

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
