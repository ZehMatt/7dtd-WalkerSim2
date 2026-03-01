using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            _groupIndex = group.Group;
            _speedScale = group.SpeedScale;
            _postSpawnBehavior = group.PostSpawnBehavior;
            _postSpawnWanderSpeed = group.PostSpawnWanderSpeed;
            _color = group.Color ?? string.Empty;
            _name = string.Empty; // display-only; caller sets it after construction

            Processors = new ObservableCollection<MovementProcessorModel>(
                _group.Entries.Select(p => new MovementProcessorModel(p))
            );
        }

        public Config.MovementProcessorGroup Underlying => _group;

        [ObservableProperty]
        private int _groupIndex;

        partial void OnGroupIndexChanged(int value)
        {
            _group.Group = value;
        }

        [ObservableProperty]
        private float _speedScale = 1.0f;

        partial void OnSpeedScaleChanged(float value)
        {
            _group.SpeedScale = value;
        }

        [ObservableProperty]
        private Config.PostSpawnBehavior _postSpawnBehavior = Config.PostSpawnBehavior.Wander;

        partial void OnPostSpawnBehaviorChanged(Config.PostSpawnBehavior value)
        {
            _group.PostSpawnBehavior = value;
        }

        [ObservableProperty]
        private Config.WanderingSpeed _postSpawnWanderSpeed = Config.WanderingSpeed.Walk;

        partial void OnPostSpawnWanderSpeedChanged(Config.WanderingSpeed value)
        {
            _group.PostSpawnWanderSpeed = value;
        }

        [ObservableProperty]
        private string _color = string.Empty;

        partial void OnColorChanged(string value)
        {
            _group.Color = value;
            OnPropertyChanged(nameof(ColorValue));
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

        public ObservableCollection<MovementProcessorModel> Processors { get; }

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
            Processors.Add(new MovementProcessorModel(newProcessor));
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
