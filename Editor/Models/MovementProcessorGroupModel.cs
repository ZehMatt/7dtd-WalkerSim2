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

        public MovementProcessorGroupModel(Config.MovementProcessorGroup group)
        {
            _group = group;

            // Initialize observable backing fields from underlying data.
            _weight = group.Weight;
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
        private float _weight = 1.0f;

        partial void OnWeightChanged(float value)
        {
            _group.Weight = value;
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


        private static readonly Config.MovementProcessorType[] AllProcessorTypes =
            System.Enum.GetValues<Config.MovementProcessorType>()
            .Where(t => t != Config.MovementProcessorType.Invalid).ToArray();

        public MovementProcessorModel AddProcessor()
        {
            // Pick the first type not already used in this system.
            var usedTypes = new HashSet<Config.MovementProcessorType>(
                Processors.Select(p => p.Type));
            var available = AllProcessorTypes.Where(t => !usedTypes.Contains(t)).ToArray();
            if (available.Length == 0)
                return null; // All types used.

            var newProcessor = new Config.MovementProcessor
            {
                Type = available[0],
                Distance = 50f,
                Power = 0.10f
            };
            _group.Entries.Add(newProcessor);
            var m = new MovementProcessorModel(newProcessor);
            m.RemoveSelfCommand = new RelayCommand(() => RemoveProcessor(m));
            m.ConfigChanged = ConfigChanged;
            Processors.Add(m);
            ConfigChanged?.Invoke();
            return m;
        }

        [RelayCommand]
        public void RemoveProcessor(MovementProcessorModel processor)
        {
            if (processor != null)
            {
                _group.Entries.Remove(processor.Underlying);
                Processors.Remove(processor);
                ConfigChanged?.Invoke();
            }
        }
    }
}
