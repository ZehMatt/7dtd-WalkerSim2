using CommunityToolkit.Mvvm.ComponentModel;
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

        [ObservableProperty]
        private Config.MovementProcessorType _type;

        partial void OnTypeChanged(Config.MovementProcessorType value)
        {
            _processor.Type = value;
        }

        [ObservableProperty]
        private float _distance;

        partial void OnDistanceChanged(float value)
        {
            _processor.Distance = value;
        }

        [ObservableProperty]
        private float _power;

        partial void OnPowerChanged(float value)
        {
            _processor.Power = value;
        }
    }
}
