using System.Net;
using Utf8Json;
using Webserver;
using Webserver.WebAPI;

namespace WalkerSim.WebView
{
    public class Sim : AbsRestApi
    {
        public Sim() : base("WalkerSim2") { }

        public override int DefaultPermissionLevel() => 1000;

        public override int[] DefaultMethodPermissionLevels()
        {
            return new int[5] { -2147483647, 1000, -2147483647, -2147483647, -2147483647 };
        }

        protected override void HandleRestGet(RequestContext ctx)
        {
            switch (ctx.RequestPath)
            {
                case "static":
                    SendStatic(ctx);
                    return;
                case "":
                case "snapshot":
                    SendSnapshot(ctx);
                    return;
            }
            SendEmptyResponse(ctx, HttpStatusCode.NotFound, null, "NOT_FOUND");
        }

        private static void SendStatic(RequestContext ctx)
        {
            var sim = Simulation.Instance;
            var mapData = sim.MapData;

            JsonWriter w;
            PrepareEnvelopedResult(out w);

            w.WriteBeginObject();

            w.WritePropertyName("world");
            if (mapData != null)
            {
                w.WriteBeginObject();
                w.WritePropertyName("minX");
                w.WriteSingle(mapData.WorldMins.X);
                w.WriteValueSeparator();
                w.WritePropertyName("minY");
                w.WriteSingle(mapData.WorldMins.Y);
                w.WriteValueSeparator();
                w.WritePropertyName("maxX");
                w.WriteSingle(mapData.WorldMaxs.X);
                w.WriteValueSeparator();
                w.WritePropertyName("maxY");
                w.WriteSingle(mapData.WorldMaxs.Y);
                w.WriteEndObject();
            }
            else
            {
                w.WriteNull();
            }

            w.WriteValueSeparator();
            w.WritePropertyName("cities");
            w.WriteBeginArray();
            if (mapData?.Cities != null)
            {
                bool first = true;
                foreach (var c in mapData.Cities.CityList)
                {
                    if (!first)
                        w.WriteValueSeparator();
                    first = false;
                    w.WriteBeginObject();
                    w.WritePropertyName("id");
                    w.WriteInt32(c.Id);
                    w.WriteValueSeparator();
                    w.WritePropertyName("x");
                    w.WriteSingle(c.Position.X);
                    w.WriteValueSeparator();
                    w.WritePropertyName("y");
                    w.WriteSingle(c.Position.Y);
                    w.WriteValueSeparator();
                    w.WritePropertyName("bx");
                    w.WriteSingle(c.Bounds.X);
                    w.WriteValueSeparator();
                    w.WritePropertyName("by");
                    w.WriteSingle(c.Bounds.Y);
                    w.WriteEndObject();
                }
            }
            w.WriteEndArray();

            w.WriteValueSeparator();
            w.WritePropertyName("groupColors");
            w.WriteBeginArray();
            int groupCount = sim.GroupCount;
            for (int i = 0; i < groupCount; i++)
            {
                if (i > 0)
                    w.WriteValueSeparator();
                var col = sim.GetGroupColor(i);
                w.WriteString(string.Format("#{0:X2}{1:X2}{2:X2}", col.R, col.G, col.B));
            }
            w.WriteEndArray();

            w.WriteValueSeparator();
            w.WritePropertyName("viewRadius");
            w.WriteInt32(sim.Config?.SpawnActivationRadius ?? 96);

            w.WriteValueSeparator();
            w.WritePropertyName("graph");
            var roads = mapData?.Roads;
            var graph = roads?.Graph;
            if (graph != null && graph.Nodes.Length > 0 && roads.Width > 0 && roads.Height > 0)
            {
                w.WriteBeginObject();

                float wMinX = mapData.WorldMins.X;
                float wMinY = mapData.WorldMins.Y;
                float wRangeX = mapData.WorldMaxs.X - wMinX;
                float wRangeY = mapData.WorldMaxs.Y - wMinY;
                float invW = wRangeX / roads.Width;
                float invH = wRangeY / roads.Height;

                w.WritePropertyName("nodes");
                w.WriteBeginArray();
                for (int i = 0; i < graph.Nodes.Length; i++)
                {
                    if (i > 0)
                        w.WriteValueSeparator();
                    var n = graph.Nodes[i];
                    int connCount = n.Connections != null ? n.Connections.Length : 0;
                    int type = connCount <= 1 ? 1 : (connCount >= 3 ? 2 : 0);
                    w.WriteSingle(wMinX + n.X * invW);
                    w.WriteValueSeparator();
                    w.WriteSingle(wMinY + n.Y * invH);
                    w.WriteValueSeparator();
                    w.WriteInt32(type);
                }
                w.WriteEndArray();

                w.WriteValueSeparator();
                w.WritePropertyName("edges");
                w.WriteBeginArray();
                bool first = true;
                for (int i = 0; i < graph.Nodes.Length; i++)
                {
                    var conns = graph.Nodes[i].Connections;
                    if (conns == null)
                        continue;
                    for (int c = 0; c < conns.Length; c++)
                    {
                        int j = conns[c];
                        if (j <= i)
                            continue;
                        if (!first)
                            w.WriteValueSeparator();
                        first = false;
                        w.WriteInt32(i);
                        w.WriteValueSeparator();
                        w.WriteInt32(j);
                    }
                }
                w.WriteEndArray();

                w.WriteEndObject();
            }
            else
            {
                w.WriteNull();
            }

            w.WriteEndObject();

            SendEnvelopedResult(ctx, ref w, HttpStatusCode.OK);
        }

        private static void SendSnapshot(RequestContext ctx)
        {
            var sim = Simulation.Instance;

            JsonWriter w;
            PrepareEnvelopedResult(out w);

            w.WriteBeginObject();
            w.WritePropertyName("t");
            w.WriteUInt32(sim.Ticks);

            w.WriteValueSeparator();
            w.WritePropertyName("a");
            w.WriteBeginArray();
            var agents = sim.Agents;
            if (agents != null)
            {
                bool first = true;
                int count = agents.Count;
                for (int i = 0; i < count; i++)
                {
                    var a = agents[i];
                    if (a.CurrentState != Agent.State.Wandering)
                        continue;
                    if (!first)
                        w.WriteValueSeparator();
                    first = false;
                    w.WriteInt32((int)a.Position.X);
                    w.WriteValueSeparator();
                    w.WriteInt32((int)a.Position.Y);
                    w.WriteValueSeparator();
                    w.WriteInt32(a.Group);
                }
            }
            w.WriteEndArray();

            w.WriteValueSeparator();
            w.WritePropertyName("s");
            w.WriteBeginArray();
            var spawned = sim.Active;
            if (spawned != null)
            {
                bool first = true;
                foreach (var kv in spawned)
                {
                    var a = kv.Value;
                    if (a.CurrentState != Agent.State.Spawned)
                        continue;
                    if (!first)
                        w.WriteValueSeparator();
                    first = false;
                    w.WriteInt32((int)a.Position.X);
                    w.WriteValueSeparator();
                    w.WriteInt32((int)a.Position.Y);
                    w.WriteValueSeparator();
                    w.WriteInt32(a.Group);
                }
            }
            w.WriteEndArray();

            w.WriteValueSeparator();
            w.WritePropertyName("p");
            w.WriteBeginArray();
            bool firstP = true;
            foreach (var kv in sim.Players)
            {
                if (!firstP)
                    w.WriteValueSeparator();
                firstP = false;
                w.WriteInt32((int)kv.Value.Position.X);
                w.WriteValueSeparator();
                w.WriteInt32((int)kv.Value.Position.Y);
            }
            w.WriteEndArray();

            w.WriteValueSeparator();
            w.WritePropertyName("e");
            w.WriteBeginArray();
            var events = sim.Events;
            if (events != null)
            {
                bool firstE = true;
                for (int i = 0; i < events.Count; i++)
                {
                    var ev = events[i];
                    if (!firstE)
                        w.WriteValueSeparator();
                    firstE = false;
                    w.WriteSingle(ev.Position.X);
                    w.WriteValueSeparator();
                    w.WriteSingle(ev.Position.Y);
                    w.WriteValueSeparator();
                    w.WriteSingle(ev.Radius);
                }
            }
            w.WriteEndArray();

            w.WriteEndObject();

            SendEnvelopedResult(ctx, ref w, HttpStatusCode.OK);
        }
    }
}
