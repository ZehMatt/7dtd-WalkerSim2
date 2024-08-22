using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace WalkerSim
{
    internal partial class Simulation
    {
        private void SaveState(State state, BinaryWriter writer)
        {
            SaveConfig(state, writer);
            SavePRNG(state, writer);
            SaveAgents(state, writer);
            SaveGrid(state, writer);
            SaveEvents(state, writer);
        }

        private void SaveConfig(State state, BinaryWriter writer)
        {
            var config = state.Config;

            var configSerializer = new XmlSerializer(typeof(Config));

            // We save this as an UTF8 xml string.
            var textWriter = new StringWriter();
            configSerializer.Serialize(textWriter, config);

            Serialization.WriteStringUTF8(writer, textWriter.ToString());
        }

        private void SavePRNG(State state, BinaryWriter writer)
        {
            var prng = state.PRNG;
            Serialization.WriteUInt32(writer, prng.State0);
            Serialization.WriteUInt32(writer, prng.State1);
        }

        private void SaveAgents(State state, BinaryWriter writer)
        {
            var agents = state.Agents;

            Serialization.WriteInt32(writer, agents.Count);
            foreach (var agent in agents)
            {
                Serialization.WriteInt32(writer, agent.Index);
                Serialization.WriteInt32(writer, agent.Group);
                Serialization.WriteVector3(writer, agent.Position);
                Serialization.WriteVector3(writer, agent.Velocity);
                Serialization.WriteInt32(writer, agent.CellIndex);
                Serialization.WriteInt32(writer, agent.EntityId);
                Serialization.WriteInt32(writer, agent.EntityClassId);
                Serialization.WriteInt32(writer, agent.Health);
                Serialization.WriteInt32(writer, (int)agent.CurrentState);
            }
        }

        private void SaveGrid(State state, BinaryWriter writer)
        {
            var grid = state.Grid;

            Serialization.WriteInt32(writer, grid.Length);
            foreach (var cell in grid)
            {
                Serialization.WriteInt32(writer, cell.Count);
                foreach (var entry in cell)
                {
                    Serialization.WriteInt32(writer, entry);
                }
            }
        }

        private void SaveEvents(State state, BinaryWriter writer)
        {
            var events = state.Events;

            Serialization.WriteInt32(writer, events.Count);
            foreach (var ev in events)
            {
                Serialization.WriteInt32(writer, (int)ev.Type);
                Serialization.WriteVector3(writer, ev.Position);
                Serialization.WriteSingle(writer, ev.Radius);
                Serialization.WriteSingle(writer, ev.DecayRate);
            }
        }

        private void LoadState(State state, BinaryReader reader)
        {
            LoadConfig(state, reader);
            LoadPRNG(state, reader);
            LoadAgents(state, reader);
            LoadGrid(state, reader);
            LoadEvents(state, reader);
        }

        private void LoadConfig(State state, BinaryReader reader)
        {
            var configXml = Serialization.ReadStringUTF8(reader);

            var configSerializer = new XmlSerializer(typeof(Config));

            var textReader = new StringReader(configXml);
            var config = (Config)configSerializer.Deserialize(textReader);

            state.Config = config;
        }

        private void LoadPRNG(State State, BinaryReader reader)
        {
            var state0 = Serialization.ReadUInt32(reader);
            var state1 = Serialization.ReadUInt32(reader);
            State.PRNG = new WalkerSim.Random(state0, state1);
        }

        private void LoadAgents(State state, BinaryReader reader)
        {
            var agents = new List<Agent>();
            var active = new Dictionary<int, Agent>();

            var count = Serialization.ReadInt32(reader);
            for (int i = 0; i < count; i++)
            {
                var agent = new Agent();
                agent.Index = Serialization.ReadInt32(reader);
                agent.Group = Serialization.ReadInt32(reader);
                agent.Position = Serialization.ReadVector3(reader);
                agent.Velocity = Serialization.ReadVector3(reader);
                agent.CellIndex = Serialization.ReadInt32(reader);
                agent.EntityId = Serialization.ReadInt32(reader);
                agent.EntityClassId = Serialization.ReadInt32(reader);
                agent.Health = Serialization.ReadInt32(reader);
                agent.CurrentState = (Agent.State)Serialization.ReadInt32(reader);
                agents.Add(agent);

                if (agent.CurrentState == Agent.State.Active)
                {
                    active.Add(agent.EntityId, agent);
                }
            }

            state.Agents = agents;
            state.Active = active;
        }

        private void LoadGrid(State state, BinaryReader reader)
        {
            var cellCount = Serialization.ReadInt32(reader);
            var grid = new List<int>[cellCount];

            for (var cellIdx = 0; cellIdx < cellCount; cellIdx++)
            {
                var cell = new List<int>();
                var entryCount = Serialization.ReadInt32(reader);
                for (var entryIdx = 0; entryIdx < entryCount; entryIdx++)
                {
                    cell.Add(Serialization.ReadInt32(reader));
                }
                grid[cellIdx] = cell;
            }

            state.Grid = grid;
        }

        private void LoadEvents(State state, BinaryReader reader)
        {
            var events = new List<EventData>();

            var count = Serialization.ReadInt32(reader);
            for (int i = 0; i < count; i++)
            {
                var ev = new EventData();
                ev.Type = (EventType)Serialization.ReadInt32(reader);
                ev.Position = Serialization.ReadVector3(reader);
                ev.Radius = Serialization.ReadSingle(reader);
                ev.DecayRate = Serialization.ReadSingle(reader);
                events.Add(ev);
            }

            state.Events = events;
        }

        public bool Save(Stream stream)
        {
            try
            {
                var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true);
                SaveState(_state, writer);
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
                var fs = new FileStream(filePath, FileMode.CreateNew);
                return Save(fs);
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
                LoadState(state, reader);
                _state = state;
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
                var fs = new FileStream(filePath, FileMode.Open);
                return Load(fs);
            }
            catch (Exception ex)
            {
                Logging.Err("Exception trying to load file '{0}', error: {1}", filePath, ex.Message);
                return false;
            }
        }
    }
}
