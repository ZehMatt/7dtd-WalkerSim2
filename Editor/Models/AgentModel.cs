using CommunityToolkit.Mvvm.ComponentModel;
using WalkerSim;

namespace Editor.Models
{
    /// <summary>
    /// Observable wrapper around a live <see cref="Agent"/> instance.
    /// Writes go directly back to the underlying agent fields.
    /// </summary>
    public partial class AgentModel : ObservableObject
    {
        private readonly Agent _agent;

        public AgentModel(Agent agent)
        {
            _agent = agent;
            Pull();
        }

        public Agent Underlying => _agent;

        // ── Display label ─────────────────────────────────────────────────────
        public string Label => $"Agent {_agent.Index}  [{_agent.CurrentState}]";

        // ── Pulled / pushed fields ────────────────────────────────────────────
        [ObservableProperty] private int _index;
        [ObservableProperty] private int _group;
        [ObservableProperty] private Agent.State _currentState;
        [ObservableProperty] private Agent.SubState _currentSubState;
        [ObservableProperty] private Agent.TravelState _currentTravelState;
        [ObservableProperty] private Agent.MoveType _walkType;
        [ObservableProperty] private int _entityId;
        [ObservableProperty] private int _entityClassId;
        [ObservableProperty] private float _health;
        [ObservableProperty] private float _maxHealth;
        [ObservableProperty] private float _originalMaxHealth;
        [ObservableProperty] private float _positionX;
        [ObservableProperty] private float _positionY;
        [ObservableProperty] private float _positionZ;
        [ObservableProperty] private float _velocityX;
        [ObservableProperty] private float _velocityY;
        [ObservableProperty] private int _targetCityIndex;
        [ObservableProperty] private int _roadNodeTarget;
        [ObservableProperty] private string _roadNodeHistory = "";
        [ObservableProperty] private uint _lastUpdateTick;
        [ObservableProperty] private uint _lastSpawnTick;

        /// <summary>Copy current agent state into observable properties.</summary>
        public void Pull()
        {
#pragma warning disable MVVMTK0034
            _index              = _agent.Index;
            _group              = _agent.Group;
            _currentState       = _agent.CurrentState;
            _currentSubState    = _agent.CurrentSubState;
            _currentTravelState = _agent.CurrentTravelState;
            _walkType           = _agent.WalkType;
            _entityId           = _agent.EntityId;
            _entityClassId      = _agent.EntityClassId;
            _health             = _agent.Health;
            _maxHealth          = _agent.MaxHealth;
            _originalMaxHealth  = _agent.OriginalMaxHealth;
            _positionX          = _agent.Position.X;
            _positionY          = _agent.Position.Y;
            _positionZ          = _agent.Position.Z;
            _velocityX          = _agent.Velocity.X;
            _velocityY          = _agent.Velocity.Y;
            _targetCityIndex    = _agent.TargetCityIndex;
            _roadNodeTarget     = _agent.RoadNodeTarget;
            _roadNodeHistory    = FormatRoadHistory(_agent);
            _lastUpdateTick     = _agent.LastUpdateTick;
            _lastSpawnTick      = _agent.LastSpawnTick;
#pragma warning restore MVVMTK0034

            // Raise all at once to avoid many individual events
            OnPropertyChanged(string.Empty);
        }

        // ── Write-back on change ──────────────────────────────────────────────
        partial void OnGroupChanged(int value)                      => _agent.Group = value;
        partial void OnCurrentStateChanged(Agent.State value)       => _agent.CurrentState = value;
        partial void OnCurrentSubStateChanged(Agent.SubState value) => _agent.CurrentSubState = value;
        partial void OnCurrentTravelStateChanged(Agent.TravelState value) => _agent.CurrentTravelState = value;
        partial void OnWalkTypeChanged(Agent.MoveType value)        => _agent.WalkType = value;
        partial void OnHealthChanged(float value)                   => _agent.Health = value;
        partial void OnMaxHealthChanged(float value)                => _agent.MaxHealth = value;
        partial void OnTargetCityIndexChanged(int value)            => _agent.TargetCityIndex = value;
        partial void OnRoadNodeTargetChanged(int value)              => _agent.RoadNodeTarget = value;
        partial void OnPositionXChanged(float value) => _agent.Position = new Vector3(value, _agent.Position.Y, _agent.Position.Z);
        partial void OnPositionYChanged(float value) => _agent.Position = new Vector3(_agent.Position.X, value, _agent.Position.Z);
        partial void OnPositionZChanged(float value) => _agent.Position = new Vector3(_agent.Position.X, _agent.Position.Y, value);

        partial void OnVelocityXChanged(float value) => _agent.Velocity = new Vector3(value, _agent.Velocity.Y, _agent.Velocity.Z);
        partial void OnVelocityYChanged(float value) => _agent.Velocity = new Vector3(_agent.Velocity.X, value, _agent.Velocity.Z);

        private static string FormatRoadHistory(Agent agent)
        {
            if (agent.RoadNodeHistoryCount == 0)
                return "(empty)";

            var sb = new System.Text.StringBuilder();
            // Show oldest to newest.
            int start = agent.RoadNodeHistoryCount < Agent.RoadNodeHistorySize
                ? 0
                : agent.RoadNodeHistoryPos;

            for (int i = 0; i < agent.RoadNodeHistoryCount; i++)
            {
                int idx = (start + i) % Agent.RoadNodeHistorySize;
                if (i > 0) sb.Append(" → ");
                sb.Append(agent.RoadNodeHistory[idx]);
            }
            return sb.ToString();
        }
    }
}
