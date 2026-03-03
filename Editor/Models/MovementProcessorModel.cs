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
    }
}
