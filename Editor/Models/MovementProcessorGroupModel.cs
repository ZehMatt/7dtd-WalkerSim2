using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WalkerSim;

namespace Editor.Models
{
    public partial class MovementProcessorGroupModel : ObservableObject
    {
        private readonly Config.MovementProcessorGroup _group;

        public struct GroupIndexOption
        {
            public int Value;
            public string Text;
            public override string ToString() => Text;
        }

        public MovementProcessorGroupModel(Config.MovementProcessorGroup group)
        {
            _group = group;

            // Initialize observable backing fields from underlying data.
            _groupIndex = group.Group;
            _speedScale = group.SpeedScale;
            _postSpawnBehavior = group.PostSpawnBehavior;
            _postSpawnWanderSpeed = group.PostSpawnWanderSpeed;
            _color = group.Color ?? string.Empty;
            _name = group.Name ?? string.Empty;

            Processors = new ObservableCollection<MovementProcessorModel>(
                _group.Entries.Select(p =>
                {
                    var m = new MovementProcessorModel(p);
                    m.RemoveSelfCommand = new RelayCommand(() => RemoveProcessor(m));
                    return m;
                })
            );
        }

        public Config.MovementProcessorGroup Underlying => _group;

        // Called when any group parameter changes (for live config reload)
        public System.Action? ConfigChanged { get; set; }

        [ObservableProperty]
        private int _groupIndex;

        partial void OnGroupIndexChanged(int value)
        {
            _group.Group = value;
            ConfigChanged?.Invoke();
        }

        [ObservableProperty]
        private float _speedScale = 1.0f;

        partial void OnSpeedScaleChanged(float value)
        {
            _group.SpeedScale = value;
            ConfigChanged?.Invoke();
        }

        [ObservableProperty]
        private Config.PostSpawnBehavior _postSpawnBehavior = Config.PostSpawnBehavior.Wander;

        partial void OnPostSpawnBehaviorChanged(Config.PostSpawnBehavior value)
        {
            _group.PostSpawnBehavior = value;
            ConfigChanged?.Invoke();
        }

        [ObservableProperty]
        private Config.WanderingSpeed _postSpawnWanderSpeed = Config.WanderingSpeed.Walk;

        partial void OnPostSpawnWanderSpeedChanged(Config.WanderingSpeed value)
        {
            _group.PostSpawnWanderSpeed = value;
            ConfigChanged?.Invoke();
        }

        [ObservableProperty]
        private string _color = string.Empty;

        partial void OnColorChanged(string value)
        {
            _group.Color = value;
            OnPropertyChanged(nameof(ColorValue));
            ConfigChanged?.Invoke();
        }

        public Avalonia.Media.Color ColorValue
        {
            get
            {
                if (!string.IsNullOrEmpty(Color))
                {
                    try { return Avalonia.Media.Color.Parse(Color); }
                    catch { }
                }
                return Avalonia.Media.Colors.Gray;
            }
        }

        [ObservableProperty]
        private string _name = string.Empty;

        partial void OnNameChanged(string value)
        {
            _group.Name = value;
            OnPropertyChanged(nameof(DisplayName));
        }

        // 1-based position in the MovementSystems list, set externally.
        private int _systemIndex = 0;
        public int SystemIndex
        {
            get => _systemIndex;
            set { _systemIndex = value; OnPropertyChanged(nameof(DisplayName)); }
        }

        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? $"System {_systemIndex}" : Name;

        public ObservableCollection<MovementProcessorModel> Processors { get; }

        // ── Group index dropdown ─────────────────────────────────────────────
        private bool _suppressGroupOptionWrite = false;

        private List<GroupIndexOption> _groupIndexOptions = new List<GroupIndexOption>();
        public List<GroupIndexOption> GroupIndexOptions
        {
            get => _groupIndexOptions;
            private set { _groupIndexOptions = value; OnPropertyChanged(); }
        }

        private GroupIndexOption? _selectedGroupOption;
        public GroupIndexOption? SelectedGroupOption
        {
            get => _selectedGroupOption;
            set
            {
                _selectedGroupOption = value;
                OnPropertyChanged();
                if (!_suppressGroupOptionWrite && value != null)
                    GroupIndex = value.Value.Value;
            }
        }

        public void RefreshGroupOptions(IEnumerable<MovementProcessorGroupModel> peerSystems, int totalGroups)
        {
            var options = new List<GroupIndexOption>();
            options.Add(new GroupIndexOption { Value = -1, Text = "Any" });

            var usedIndices = new HashSet<int>(
                peerSystems.Where(s => s != this && s.GroupIndex >= 0).Select(s => s.GroupIndex));

            for (int i = 0; i < totalGroups; i++)
            {
                if (usedIndices.Contains(i)) continue;
                options.Add(new GroupIndexOption { Value = i, Text = i.ToString() });
            }

            if (GroupIndex >= 0 && !options.Any(o => o.Value == GroupIndex))
                options.Add(new GroupIndexOption { Value = GroupIndex, Text = GroupIndex.ToString() });

            _suppressGroupOptionWrite = true;
            GroupIndexOptions = options;
            _selectedGroupOption = options.Find(o => o.Value == GroupIndex) is { } found ? found : (GroupIndexOption?)null;
            _suppressGroupOptionWrite = false;
            OnPropertyChanged(nameof(SelectedGroupOption));
        }

        [RelayCommand]
        public void AddProcessor()
        {
            var newProcessor = new Config.MovementProcessor
            {
                Type = Config.MovementProcessorType.FlockAnyGroup,
                Distance = 50f,
                Power = 0.0001f
            };
            _group.Entries.Add(newProcessor);
            var m = new MovementProcessorModel(newProcessor);
            m.RemoveSelfCommand = new RelayCommand(() => RemoveProcessor(m));
            m.ConfigChanged = ConfigChanged;
            Processors.Add(m);
        }

        [RelayCommand]
        public void RemoveProcessor(MovementProcessorModel processor)
        {
            if (processor != null)
            {
                _group.Entries.Remove(processor.Underlying);
                Processors.Remove(processor);
            }
        }
    }
}
