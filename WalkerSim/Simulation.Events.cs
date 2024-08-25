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
            public float Duration;
        }

        public IReadOnlyList<EventData> Events
        {
            get
            {
                lock (_state.Events)
                {
                    var copy = new List<EventData>(_state.Events);
                    return copy;
                }
            }
        }

        private void AddEvent(EventData data)
        {
            var events = _state.Events;
            lock (events)
            {
                // Check if we can merge with an existing event.
                foreach (var ev in _state.Events)
                {
                    if (ev.Type != data.Type)
                    {
                        continue;
                    }

                    var dist = Vector3.Distance(ev.Position, data.Position);

                    // If the new event is within the existing event's radius we add to the duration.
                    if (dist <= ev.Radius)
                    {
                        ev.Duration = System.Math.Max(ev.Duration, data.Duration);
                        return;
                    }

                }

                events.Add(data);
            }
        }

        public void AddSoundEvent(Vector3 pos, float radius, float duration)
        {
            var data = new EventData
            {
                Type = EventType.Noise,
                Position = pos,
                Radius = radius,
                Duration = duration,
            };

            AddEvent(data);
        }

        private void UpdateEvents()
        {
            var dt = TickRate * TimeScale;

            var events = _state.Events;

            lock (events)
            {
                foreach (var ev in events)
                {
                    ev.Duration -= dt;
                }

                // Erase expired events.
                events.RemoveAll(ev => ev.Duration <= 0.0f);

                // Make a copy after the update so the simulation can use this without locking for queries.
                _state.EventsTemp = new List<EventData>(events);
            }
        }
    }
}
