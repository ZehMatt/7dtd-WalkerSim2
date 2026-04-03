using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace WalkerSim
{
    public partial class Simulation
    {
        private DateTime _nextAutoSave = DateTime.MaxValue;
        private string _autoSaveFile;
        private float _autoSaveInterval = -1;

        private bool SerializeState(State state, SerializationContext ctx)
        {
            if (!SerializeHeader(state, ctx))
            {
                return false;
            }
            SerializeInfo(state, ctx);
            SerializeStats(state, ctx);
            SerializeConfig(state, ctx);
            SerializePRNG(state, ctx);
            SerializeAgents(state, ctx);
            SerializeEvents(state, ctx);

            return true;
        }

        private bool SerializeHeader(State state, SerializationContext ctx)
        {
            uint magic = ctx.IsWriting ? Constants.SaveMagic : 0;
            uint version = ctx.IsWriting ? Constants.SaveVersion : 0;

            ctx.Serialize(ref magic, false);
            ctx.Serialize(ref version, false);

            if (ctx.IsReading)
            {
                if (magic != Constants.SaveMagic)
                {
                    throw new Exception("Invalid magic value, file is probably corrupted.");
                }

                state.Version = version;
                if (state.Version != Constants.SaveVersion)
                {
                    Logging.Info("Saved state is using a different version, skipping load.");
                    return false;
                }
            }

            return true;
        }

        private bool SerializeInfo(State state, SerializationContext ctx)
        {
            ctx.Serialize(ref state.WorldMins);
            ctx.Serialize(ref state.WorldMaxs);
            ctx.Serialize(ref state.WorldName);
            ctx.Serialize(ref state.SlowIterator);
            ctx.Serialize(ref state.WindDir);
            ctx.Serialize(ref state.WindDirTarget);
            ctx.Serialize(ref state.WindTime);
            ctx.Serialize(ref state.Ticks);
            ctx.Serialize(ref state.UnscaledTicks);
            ctx.Serialize(ref state.TickNextWindChange);
            ctx.Serialize(ref state.GroupCount);
            ctx.Serialize(ref state.MaxNeighbourDistance);
            ctx.Serialize(ref state.GameTime);

            return true;
        }

        private bool SerializeStats(State state, SerializationContext ctx)
        {
            ctx.Serialize(ref state.SuccessfulSpawns);
            ctx.Serialize(ref state.FailedSpawns);
            ctx.Serialize(ref state.TotalDespawns);

            return true;
        }

        private bool SerializeConfig(State state, SerializationContext ctx)
        {
            if (ctx.IsWriting)
            {
                var textWriter = new StringWriter();
                state.Config.Export(textWriter);
                var configXml = textWriter.ToString();
                ctx.Serialize(ref configXml);
            }
            else
            {
                string configXml = null;
                ctx.Serialize(ref configXml);
                state.Config = Config.LoadFromText(configXml);
            }

            return true;
        }

        private bool SerializePRNG(State state, SerializationContext ctx)
        {
            uint state0 = ctx.IsWriting ? state.PRNG.State0 : 0;
            uint state1 = ctx.IsWriting ? state.PRNG.State1 : 0;

            ctx.Serialize(ref state0);
            ctx.Serialize(ref state1);

            if (ctx.IsReading)
            {
                state.PRNG = new WalkerSim.Random(state0, state1);
            }

            return true;
        }

        private bool SerializeAgents(State state, SerializationContext ctx)
        {
            int count = ctx.IsWriting ? state.Agents.Count : 0;
            ctx.Serialize(ref count);

            if (ctx.IsReading)
            {
                state.Agents = new List<Agent>(count);
                state.Spawned = new ConcurrentDictionary<int, Agent>();
            }

            for (int i = 0; i < count; i++)
            {
                Agent agent;
                if (ctx.IsWriting)
                {
                    agent = state.Agents[i];
                }
                else
                {
                    agent = new Agent();
                }

                ctx.Serialize(ref agent.Index);
                ctx.Serialize(ref agent.Group);
                ctx.Serialize(ref agent.Position);
                ctx.Serialize(ref agent.Velocity);
                ctx.Serialize(ref agent.CellIndex);
                ctx.Serialize(ref agent.EntityId);
                ctx.Serialize(ref agent.EntityClassId);
                ctx.Serialize(ref agent.Health);
                ctx.Serialize(ref agent.MaxHealth);
                ctx.Serialize(ref agent.OriginalMaxHealth);
                ctx.SerializeEnum(ref agent.Dismemberment);
                ctx.SerializeEnum(ref agent.WalkType);

                // NOTE: Ensure the state isn't some intermediate one when saving.
                if (ctx.IsWriting)
                {
                    var agentState = agent.CurrentState;
                    if (agentState == Agent.State.PendingSpawn)
                    {
                        agentState = Agent.State.Wandering;
                    }
                    ctx.SerializeEnum(ref agentState);
                }
                else
                {
                    ctx.SerializeEnum(ref agent.CurrentState);
                }

                ctx.Serialize(ref agent.LastUpdateTick);
                ctx.Serialize(ref agent.LastSpawnTick);
                ctx.SerializeEnumByte(ref agent.CurrentSubState);
                ctx.Serialize(ref agent.AlertedTick);
                ctx.Serialize(ref agent.AlertPosition);
                ctx.Serialize(ref agent.TimeToDie);
                ctx.Serialize(ref agent.TargetCityIndex);
                ctx.Serialize(ref agent.CityTime);
                ctx.SerializeEnumByte(ref agent.CurrentTravelState);
                ctx.Serialize(ref agent.RoadNodeTarget);
                ctx.Serialize(ref agent.RoadNodeHistoryCount);
                ctx.Serialize(ref agent.RoadNodeHistoryPos);
                for (int h = 0; h < Agent.RoadNodeHistorySize; h++)
                    ctx.Serialize(ref agent.RoadNodeHistory[h]);

                if (ctx.IsReading)
                {
                    state.Agents.Add(agent);
                    if (agent.CurrentState == Agent.State.Spawned)
                    {
                        state.Spawned.TryAdd(agent.EntityId, agent);
                    }
                }
            }

            return true;
        }

        private bool SerializeEvents(State state, SerializationContext ctx)
        {
            int count = ctx.IsWriting ? state.Events.Count : 0;
            ctx.Serialize(ref count);

            if (ctx.IsReading)
            {
                state.Events = new List<EventData>(count);
            }

            for (int i = 0; i < count; i++)
            {
                EventData ev;
                if (ctx.IsWriting)
                {
                    ev = state.Events[i];
                }
                else
                {
                    ev = new EventData();
                }

                ctx.SerializeEnum(ref ev.Type);
                ctx.Serialize(ref ev.Position);
                ctx.Serialize(ref ev.Radius);
                ctx.Serialize(ref ev.Duration);

                if (ctx.IsReading)
                {
                    state.Events.Add(ev);
                }
            }

            return true;
        }

        public bool Save(Stream stream)
        {
            try
            {
                var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true);
                var ctx = SerializationContext.CreateWriter(writer);
                lock (_state)
                {
                    if (!SerializeState(_state, ctx))
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Err("Failed to serialize state, error: {1}", ex.Message);
                return false;
            }

            return true;
        }

        public bool Save(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    return Save(fs);
                }
            }
            catch (Exception ex)
            {
                Logging.Err("Exception trying to save file '{0}', error: {1}", filePath, ex.Message);
                return false;
            }
        }

        public bool Load(Stream stream)
        {
            try
            {
                var state = new State();
                var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true);
                var ctx = SerializationContext.CreateReader(reader);

                if (!SerializeState(state, ctx))
                {
                    return false;
                }

                state.MapData = _state.MapData;
                _state = state;

                // Rebuild the grid from agent positions.
                RebuildGrid();

                BuildGroupStarts();
                SetupProcessors();
            }
            catch (Exception ex)
            {
                Logging.Err("Failed to deserializing state, error: {1}", ex.Message);
                return false;
            }
            return true;
        }

        public bool Load(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    return Load(fs);
                }
            }
            catch (Exception ex)
            {
                Logging.Err("Exception trying to load file '{0}', error: {1}", filePath, ex.Message);
                return false;
            }
        }

        public void EnableAutoSave(string file, float interval)
        {
            _autoSaveFile = file;
            _nextAutoSave = DateTime.Now.AddSeconds(interval);
            _autoSaveInterval = interval;

            Logging.Out("Enabled auto-save, interval: {0}s, file: '{1}'.", interval, file);
        }

        public void AutoSave()
        {
            if (_autoSaveFile == null)
                return;

            var elapsedMs = Utils.Measure(() =>
            {
                Save(_autoSaveFile);
            });

            Logging.Out("Saved simulation in {0}.", elapsedMs);
        }

        private void CheckAutoSave()
        {
            var now = DateTime.Now;
            if (now < _nextAutoSave)
                return;

            AutoSave();

            _nextAutoSave = now.AddSeconds(_autoSaveInterval);
        }
    }
}
