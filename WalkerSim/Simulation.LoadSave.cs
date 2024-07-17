using System.IO;

namespace WalkerSim
{
    internal partial class Simulation
    {
        private void SaveState(BinaryWriter writer)
        {
            SaveAgents(writer);
            SaveEvents(writer);
        }

        private void SaveAgents(BinaryWriter writer)
        {
            writer.Write(agents.Count);
            foreach (var agent in agents)
            {
                writer.Write(agent.Index);
                writer.Write(agent.Position.X);
                writer.Write(agent.Position.Y);
                writer.Write(agent.Position.Z);
                writer.Write((int)agent.CurrentState);
            }
        }

        private void LoadAgents(BinaryReader reader)
        {
            agents.Clear();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var agent = new Agent();
                agent.Index = reader.ReadInt32();
                agent.Position.X = reader.ReadSingle();
                agent.Position.Y = reader.ReadSingle();
                agent.Position.Z = reader.ReadSingle();
                agent.CurrentState = (Agent.State)reader.ReadInt32();
                agents.Add(agent);
            }
        }

        private void SaveEvents(BinaryWriter writer)
        {
            writer.Write(_events.Count);
            foreach (var ev in _events)
            {
                writer.Write((int)ev.Type);
                writer.Write(ev.Position.X);
                writer.Write(ev.Position.Y);
                writer.Write(ev.Position.Z);
                writer.Write(ev.Radius);
                writer.Write(ev.DecayRate);
            }
        }

        private void LoadEvents(BinaryReader reader)
        {
            _events.Clear();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var ev = new EventData();
                ev.Type = (EventType)reader.ReadInt32();
                ev.Position.X = reader.ReadSingle();
                ev.Position.Y = reader.ReadSingle();
                ev.Position.Z = reader.ReadSingle();
                ev.Radius = reader.ReadSingle();
                ev.DecayRate = reader.ReadSingle();
                _events.Add(ev);
            }
        }

        public bool Save(string filePath)
        {
            return false;
        }

        public bool Load(string filePath)
        {
            return false;
        }
    }
}
