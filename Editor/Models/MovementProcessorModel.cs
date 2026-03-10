using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WalkerSim;

namespace Editor.Models
{
    public partial class MovementProcessorModel : ObservableObject
    {
        private readonly Config.MovementProcessor _processor;

        public MovementProcessorModel(Config.MovementProcessor processor)
        {
            _processor = processor;
            _type = processor.Type;
            _distance = processor.Distance;
            _power = processor.Power;
            _param1 = processor.Param1;
            _param2 = processor.Param2;
        }

        public Config.MovementProcessor Underlying => _processor;

        public IRelayCommand? RemoveSelfCommand { get; internal set; }

        // Called when any processor parameter changes (for live config reload)
        public System.Action? ConfigChanged { get; set; }

        [ObservableProperty]
        private Config.MovementProcessorType _type;

        partial void OnTypeChanged(Config.MovementProcessorType value)
        {
            _processor.Type = value;
            var meta = GetMeta();
            // Apply default param values for the new type.
            Param1 = meta.DefaultParam1;
            Param2 = meta.DefaultParam2;
            OnPropertyChanged(nameof(ShowDistance));
            OnPropertyChanged(nameof(ShowPower));
            OnPropertyChanged(nameof(ShowParam1));
            OnPropertyChanged(nameof(ShowParam2));
            OnPropertyChanged(nameof(DistanceLabel));
            OnPropertyChanged(nameof(PowerLabel));
            OnPropertyChanged(nameof(Param1Label));
            OnPropertyChanged(nameof(Param2Label));
            OnPropertyChanged(nameof(Description));
            ConfigChanged?.Invoke();
        }

        [ObservableProperty]
        private float _distance;

        partial void OnDistanceChanged(float value)
        {
            _processor.Distance = value;
            ConfigChanged?.Invoke();
        }

        [ObservableProperty]
        private float _power;

        partial void OnPowerChanged(float value)
        {
            _processor.Power = value;
            ConfigChanged?.Invoke();
        }

        [ObservableProperty]
        private float _param1;

        partial void OnParam1Changed(float value)
        {
            _processor.Param1 = value;
            // Ensure Param2 (max) is not less than Param1 (min).
            if (Param2 < value)
                Param2 = value;
            ConfigChanged?.Invoke();
        }

        [ObservableProperty]
        private float _param2;

        partial void OnParam2Changed(float value)
        {
            // Ensure Param2 (max) is not less than Param1 (min).
            if (value < Param1)
            {
                var min = Param1;
                _processor.Param2 = min;
                // Defer the UI update so the NumericUpDown picks it up after its own update.
                Avalonia.Threading.Dispatcher.UIThread.Post(() => Param2 = min);
                ConfigChanged?.Invoke();
                return;
            }
            _processor.Param2 = value;
            ConfigChanged?.Invoke();
        }

        // Per-type parameter metadata
        private record struct ParamMeta(string? DistanceLabel, string? PowerLabel, string? Param1Label, string? Param2Label, string Description, float DefaultParam1 = 0f, float DefaultParam2 = 0f);

        private static readonly ParamMeta DefaultMeta = new("Distance", "Power", null, null, "Unknown processor type.");

        private static readonly Dictionary<Config.MovementProcessorType, ParamMeta> MetaMap = new()
        {
            { Config.MovementProcessorType.FlockAnyGroup,  new("Distance", "Power", null, null, "Steer towards nearby agents from any group within the given distance.") },
            { Config.MovementProcessorType.AlignAnyGroup,  new("Distance", "Power", null, null, "Align direction with nearby agents from any group within the given distance.") },
            { Config.MovementProcessorType.AvoidAnyGroup,  new("Distance", "Power", null, null, "Steer away from nearby agents from any group within the given distance.") },
            { Config.MovementProcessorType.FlockSameGroup, new("Distance", "Power", null, null, "Steer towards nearby agents belonging to the same group within the given distance.") },
            { Config.MovementProcessorType.AlignSameGroup, new("Distance", "Power", null, null, "Align direction with nearby agents belonging to the same group within the given distance.") },
            { Config.MovementProcessorType.AvoidSameGroup, new("Distance", "Power", null, null, "Steer away from nearby agents belonging to the same group within the given distance.") },
            { Config.MovementProcessorType.FlockOtherGroup,new("Distance", "Power", null, null, "Steer towards nearby agents from other groups within the given distance.") },
            { Config.MovementProcessorType.AlignOtherGroup,new("Distance", "Power", null, null, "Align direction with nearby agents from other groups within the given distance.") },
            { Config.MovementProcessorType.AvoidOtherGroup,new("Distance", "Power", null, null, "Steer away from nearby agents from other groups within the given distance.") },
            { Config.MovementProcessorType.Wind,           new(null, "Power", null, null, "Apply a global wind force that pushes agents in a consistent direction.") },
            { Config.MovementProcessorType.WindInverted,   new(null, "Power", null, null, "Apply a global wind force in the opposite direction of the current wind.") },
            { Config.MovementProcessorType.StickToRoads,   new(null, "Power", null, null, "Agents navigate along roads using a waypoint graph. At intersections they pick a direction, at dead ends they turn around. Power controls attraction strength.") },
            { Config.MovementProcessorType.AvoidRoads,     new(null, "Power", null, null, "Repel agents away from roads, making them prefer off-road paths.") },
            { Config.MovementProcessorType.StickToPOIs,    new(null, "Power", null, null, "Attract agents towards nearby points of interest (POIs/prefabs).") },
            { Config.MovementProcessorType.AvoidPOIs,      new("Distance", "Power", null, null, "Repel agents away from points of interest (POIs/prefabs) within the given distance.") },
            { Config.MovementProcessorType.WorldEvents,    new(null, "Power", null, null, "Attract agents towards active world events such as sounds and explosions.") },
            { Config.MovementProcessorType.PreferCities,   new(null, "Power", null, null, "Attract agents towards city areas, making them gravitate towards urban zones.") },
            { Config.MovementProcessorType.AvoidCities,    new("Distance", "Power", null, null, "Repel agents away from city areas within the given distance.") },
            { Config.MovementProcessorType.CityVisitor,    new(null, "Power", "Min Stay (min)", "Max Stay (min)", "Agents visit cities and stay for a random duration between min and max stay time (in real-time minutes).", 20f, 40f) },
        };

        private ParamMeta GetMeta() => MetaMap.TryGetValue(Type, out var m) ? m : DefaultMeta;

        public bool ShowDistance => GetMeta().DistanceLabel != null;
        public bool ShowPower => GetMeta().PowerLabel != null;
        public bool ShowParam1 => GetMeta().Param1Label != null;
        public bool ShowParam2 => GetMeta().Param2Label != null;

        public string DistanceLabel => GetMeta().DistanceLabel ?? "Distance";
        public string PowerLabel => GetMeta().PowerLabel ?? "Power";
        public string Param1Label => GetMeta().Param1Label ?? "Param1";
        public string Param2Label => GetMeta().Param2Label ?? "Param2";

        public string Description => GetMeta().Description;
    }
}
