using System;
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

        private const float MaxMergeDistance = 25f;
        private const float MaxAllowedRadius = 500f;

        private void AddEvent(EventData data)
        {
            var events = _state.Events;
            lock (events)
            {
                EventData mergeTarget = null;

                // Try to find a merge candidate
                foreach (var ev in events)
                {
                    if (ev.Type != data.Type)
                        continue;

                    float dist = Vector3.Distance2D(ev.Position, data.Position);

                    if (dist <= MaxMergeDistance)
                    {
                        mergeTarget = ev;
                        break;
                    }

                    if (data.Radius > ev.Radius && dist <= data.Radius)
                    {
                        mergeTarget = ev;
                        break;
                    }
                }

                if (mergeTarget != null)
                {
                    mergeTarget.Duration = Math.Max(mergeTarget.Duration, data.Duration);

                    mergeTarget.Radius = Math.Min(
                        Math.Max(mergeTarget.Radius, data.Radius),
                        MaxAllowedRadius);

                    mergeTarget.Position = Vector3.Lerp(
                        mergeTarget.Position,
                        data.Position,
                        0.2f);
                }
                else
                {
                    events.Add(data);
                }
            }
        }

        public void AddSoundEvent(Vector3 pos, float radius, float duration)
        {
            Logging.CondInfo(Config.LoggingOpts.Events,
                "Adding sound event at {0}, radius: {1}, duration: {2}",
                pos, radius, duration);

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
            var dt = Constants.TickRate;
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
