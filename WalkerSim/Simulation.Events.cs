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

        private List<EventData> _events = new List<EventData>();

        public IReadOnlyList<EventData> Events
        {
            get
            {
                lock (_events)
                {
                    var copy = new List<EventData>(_events);
                    return copy;
                }
            }
        }

        private void AddEvent(EventData data)
        {
            lock (_events)
            {
                var mergeDistance = 250;
                var mergeDistanceSqr = mergeDistance * mergeDistance;

                // Check if we can merge with an existing event.
                foreach (var ev in _events)
                {
                    if (ev.Type != data.Type)
                    {
                        continue;
                    }

                    var dist = Vector3.DistanceSqr(ev.Position, data.Position);
                    if (dist > mergeDistanceSqr)
                    {
                        continue;
                    }

                    ev.Radius = data.Radius;
                    ev.DecayRate = data.DecayRate;
                    ev.Position = data.Position;

                    return;
                }

                _events.Add(data);
            }
        }

        public void AddNoiseEvent(Vector3 pos, float radius, float decayRate)
        {
            var data = new EventData
            {
                Type = EventType.Noise,
                Position = pos,
                Radius = radius,
                DecayRate = decayRate,
            };

            AddEvent(data);
        }

        private void UpdateEvents()
        {
            var dt = TickRate;

            lock (_events)
            {
                foreach (var ev in _events)
                {
                    if (ev.Type == EventType.Noise)
                    {
                        // Shrink the radius.
                        var decay = ev.DecayRate * dt;
                        ev.Radius = System.Math.Max(ev.Radius - decay, 0);
                    }
                }

                // Erase expired events.
                _events.RemoveAll(ev => ev.Radius <= 0);
            }
        }
    }
}
