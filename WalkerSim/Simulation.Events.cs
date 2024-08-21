using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        public enum EventType
        {
            Noise,
        }

        public class EventData
        {
            public EventType Type;
            public Vector3 Position;
            public float Radius;
            public float DecayRate;
        }

        public IReadOnlyList<EventData> Events
        {
            get
            {
                lock (_state)
                {
                    var copy = new List<EventData>(_state.Events);
                    return copy;
                }
            }
        }

        private void AddEvent(EventData data)
        {
            lock (_state)
            {
                // Check if we can merge with an existing event.
                foreach (var ev in _state.Events)
                {
                    if (ev.Type != data.Type)
                    {
                        continue;
                    }

                    var dist = Vector3.Distance(ev.Position, data.Position);

                    // If the new event is within the existing event's radius, no need to expand the radius
                    if (dist <= ev.Radius)
                    {
                        ev.DecayRate = data.DecayRate;
                        return;
                    }

                    // Check if the events are close enough to consider merging
                    if (dist < (ev.Radius + data.Radius))
                    {
                        // Calculate the new radius so that both events are fully encapsulated
                        float newRadius = ((dist / 2) + ev.Radius + data.Radius) / 2;

                        // Only adjust the position if necessary, moving it towards the new event
                        if (dist + data.Radius > ev.Radius)
                        {
                            float t = (newRadius - ev.Radius) / dist; // proportion to move towards the new event
                            ev.Position = Vector3.Lerp(ev.Position, data.Position, t);
                        }

                        ev.Radius = newRadius;
                        ev.DecayRate = data.DecayRate;
                        ev.Position = (ev.Position + data.Position) * 0.5f;

                        return;
                    }
                }

                _state.Events.Add(data);
            }
        }

        public void AddSoundEvent(Vector3 pos, float radius)
        {
            var data = new EventData
            {
                Type = EventType.Noise,
                Position = pos,
                Radius = radius,
                DecayRate = Limits.SoundDecayRate,
            };

            AddEvent(data);
        }

        private void UpdateEvents()
        {
            var dt = TickRate * TimeScale;

            lock (_state)
            {
                foreach (var ev in _state.Events)
                {
                    if (ev.Type == EventType.Noise)
                    {
                        // Shrink the radius.
                        var decay = ev.DecayRate * dt;
                        ev.Radius = System.Math.Max(ev.Radius - decay, 0);
                    }
                }

                // Erase expired events.
                _state.Events.RemoveAll(ev => ev.Radius <= 0);
            }
        }
    }
}
