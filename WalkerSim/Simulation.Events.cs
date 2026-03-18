using System;
using System.Collections.Generic;

namespace WalkerSim
{
    public partial class Simulation
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

        const float MergeDistanceThreshold = 25.0f;
        const float MergeRadiusThreshold = 5.0f;

        private void AddEvent(EventData data)
        {
            var events = _state.Events;
            lock (events)
            {
                EventData mergeCandidate = null;

                // Try to find a merge candidate
                foreach (var ev in events)
                {
                    if (ev.Type != data.Type)
                        continue;

                    // Check if the current event fits in the new event and swallow it.
                    var dist = Vector3.Distance(ev.Position, data.Position);
                    if (dist + ev.Radius <= data.Radius)
                    {
                        mergeCandidate = ev;
                        break;
                    }

                    // Check if we should just move the existing event to the new position.
                    var radiusDiff = Math.Abs(ev.Radius - data.Radius);
                    if (dist <= MergeDistanceThreshold &&
                        radiusDiff <= MergeRadiusThreshold)
                    {
                        mergeCandidate = ev;
                        break;
                    }
                }

                if (mergeCandidate != null)
                {
                    mergeCandidate.Position = data.Position;
                    mergeCandidate.Radius = data.Radius;
                    mergeCandidate.Duration = Math.Max(mergeCandidate.Duration, data.Duration);
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
                () => $"Adding sound event at {pos}, radius: {radius}, duration: {duration}");

            if (Config.PauseDuringBloodmoon && _state.IsBloodmoon)
            {
                return;
            }

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
            var events = _state.Events;

            lock (events)
            {
                if (events.Count == 0)
                {
                    _state.EventsTemp.Clear();
                    return;
                }

                var dt = Constants.TickRate;
                for (int i = events.Count - 1; i >= 0; i--)
                {
                    events[i].Duration -= dt;
                    if (events[i].Duration <= 0.0f)
                        events.RemoveAt(i);
                }

                // Copy into the temp list so the simulation can use this without locking for queries.
                _state.EventsTemp.Clear();
                for (int i = 0; i < events.Count; i++)
                    _state.EventsTemp.Add(events[i]);
            }
        }
    }
}
