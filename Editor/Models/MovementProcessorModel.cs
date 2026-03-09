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
            OnPropertyChanged(nameof(ShowDistance));
            OnPropertyChanged(nameof(ShowPower));
            OnPropertyChanged(nameof(ShowParam1));
            OnPropertyChanged(nameof(ShowParam2));
            OnPropertyChanged(nameof(DistanceLabel));
            OnPropertyChanged(nameof(PowerLabel));
            OnPropertyChanged(nameof(Param1Label));
            OnPropertyChanged(nameof(Param2Label));
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
            ConfigChanged?.Invoke();
        }

        [ObservableProperty]
        private float _param2;

        partial void OnParam2Changed(float value)
        {
            _processor.Param2 = value;
            ConfigChanged?.Invoke();
        }

        // Per-type parameter metadata
        private record struct ParamMeta(string? DistanceLabel, string? PowerLabel, string? Param1Label, string? Param2Label);

        private static readonly ParamMeta DefaultMeta = new("Distance", "Power", null, null);

        private static readonly Dictionary<Config.MovementProcessorType, ParamMeta> MetaMap = new()
        {
            { Config.MovementProcessorType.FlockAnyGroup,  new("Distance", "Power", null, null) },
            { Config.MovementProcessorType.AlignAnyGroup,  new("Distance", "Power", null, null) },
            { Config.MovementProcessorType.AvoidAnyGroup,  new("Distance", "Power", null, null) },
            { Config.MovementProcessorType.FlockSameGroup, new("Distance", "Power", null, null) },
            { Config.MovementProcessorType.AlignSameGroup, new("Distance", "Power", null, null) },
            { Config.MovementProcessorType.AvoidSameGroup, new("Distance", "Power", null, null) },
            { Config.MovementProcessorType.FlockOtherGroup,new("Distance", "Power", null, null) },
            { Config.MovementProcessorType.AlignOtherGroup,new("Distance", "Power", null, null) },
            { Config.MovementProcessorType.AvoidOtherGroup,new("Distance", "Power", null, null) },
            { Config.MovementProcessorType.Wind,           new(null, "Power", null, null) },
            { Config.MovementProcessorType.WindInverted,   new(null, "Power", null, null) },
            { Config.MovementProcessorType.StickToRoads,   new(null, "Power", null, null) },
            { Config.MovementProcessorType.AvoidRoads,     new(null, "Power", null, null) },
            { Config.MovementProcessorType.StickToPOIs,    new(null, "Power", null, null) },
            { Config.MovementProcessorType.AvoidPOIs,      new("Distance", "Power", null, null) },
            { Config.MovementProcessorType.WorldEvents,    new(null, "Power", null, null) },
            { Config.MovementProcessorType.PreferCities,   new(null, "Power", null, null) },
            { Config.MovementProcessorType.AvoidCities,    new("Distance", "Power", null, null) },
            { Config.MovementProcessorType.CityVisitor,    new(null, "Power", "Min Stay (min)", "Max Stay (min)") },
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
    }
}
