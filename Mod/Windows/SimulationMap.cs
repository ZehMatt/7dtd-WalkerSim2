using System;
using System.Collections.Generic;
using UnityEngine;

namespace WalkerSim
{
    internal class SimulationWindow : GUIWindow
    {
        private static UnityEngine.RenderTexture roadsTexture;
        private static UnityEngine.RenderTexture previewTexturee;

        private static bool roadsRendered = false;

        private const int kWidth = 640;
        private const int kHeight = 640;

        private static readonly Vector3 kCanvas = new Vector3(kWidth, kHeight, 0f);
        private DateTime nextUpdate = DateTime.Now;
        private List<Color> colors = new List<Color>();

        public SimulationWindow(string _id, int _w, int _h, bool _bDrawBackground)
            : base(_id, _w, _h, _bDrawBackground)
        {
        }

        public static void Init()
        {
            var wnd = new SimulationWindow("walkersim", kWidth, kHeight, true);
            LocalPlayerUI.primaryUI.windowManager.Add("walkersim", wnd);

            roadsTexture = new UnityEngine.RenderTexture(kWidth, kHeight, 0);
            roadsTexture.Create();

            previewTexturee = new UnityEngine.RenderTexture(kWidth, kHeight, 0);
            previewTexturee.Create();
        }

        private static Color ColorFromRGB(byte r, byte g, byte b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        private void UpdateColors()
        {
            var simulation = Simulation.Instance;
            if (simulation.GroupCount != colors.Count)
            {
                colors.Clear();
                for (int i = 0; i < simulation.GroupCount; i++)
                {
                    var r = (byte)(i * 10 % 255);
                    var g = (byte)(i * 20 % 255);
                    var b = (byte)(i * 30 % 255);
                    colors.Add(ColorFromRGB(r, g, b));
                }
            }
        }

        private void RenderRoadsToTexture()
        {
            if (roadsRendered)
                return;

            var simulation = Simulation.Instance;
            var mapData = simulation.MapData;
            if (mapData == null)
                return;

            var roads = mapData.Roads;
            if (roads == null)
                return;

            var oldRT = UnityEngine.RenderTexture.active;

            UnityEngine.RenderTexture.active = roadsTexture;

            var colorMainRoad = new UnityEngine.Color(1.0f, 1.0f, 1.0f, 0.4f);
            var colorOffRoad = new UnityEngine.Color(1.0f, 1.0f, 1.0f, 0.2f);

            {
                GL.Clear(true, true, UnityEngine.Color.black);

                GL.PushMatrix();
                GL.LoadPixelMatrix(0, roadsTexture.width, roadsTexture.height, 0);

                {
                    for (int y = 0; y < roads.Height; y++)
                    {
                        for (int x = 0; x < roads.Width; x++)
                        {
                            var roadType = roads.GetRoadType(x, y);
                            if (roadType == RoadType.None)
                                continue;

                            var posX = Math.Remap(x, 0, roads.Width, 0, kWidth);
                            var posY = Math.Remap(y, 0, roads.Height, 0, kHeight);

                            var color = roadType == RoadType.Asphalt ? colorMainRoad : colorOffRoad;
                            Drawing.Primitives.DrawPixel(new UnityEngine.Vector2(posX, posY), color);
                        }
                    }
                }

                GL.PopMatrix();
            }

            UnityEngine.RenderTexture.active = oldRT;

            roadsRendered = true;
        }

        private void RenderSimulation()
        {
            var simulation = Simulation.Instance;

            var oldRT = UnityEngine.RenderTexture.active;

            UnityEngine.RenderTexture.active = previewTexturee;

            {
                // Make background transparent.
                GL.Clear(true, true, UnityEngine.Color.clear);

                GL.PushMatrix();
                GL.LoadPixelMatrix(0, previewTexturee.width, previewTexturee.height, 0);

                // Render agents.
                {
                    foreach (var agent in simulation.Agents)
                    {
                        if (agent.CurrentState == Agent.State.Dead)
                            continue;

                        var pos = simulation.RemapPosition2D(agent.Position, Vector3.Zero, kCanvas);
                        //var color = colors[agent.Group];

                        if (agent.CurrentState == Agent.State.Active)
                        {
                            Drawing.Primitives.DrawFilledCircle(new UnityEngine.Vector2(pos.X, pos.Y), 2, UnityEngine.Color.red);
                        }
                        else
                        {
                            Drawing.Primitives.DrawPixel(new UnityEngine.Vector2(pos.X, pos.Y), UnityEngine.Color.green);
                        }

                    }
                }

                // Render players.
                {
                    var worldSize = simulation.WorldSize;
                    foreach (var kv in simulation.Players)
                    {
                        var player = kv.Value;

                        var pos = simulation.RemapPosition2D(player.Position, Vector3.Zero, kCanvas);
                        Drawing.Primitives.DrawFilledCircle(new UnityEngine.Vector2(pos.X, pos.Y), 2, UnityEngine.Color.blue);

                        // FIXME: We should maybe draw an ellipse with x and y remapped.
                        var viewRadius = Math.Remap(player.ViewRadius, 0, worldSize.X, 0, kWidth);
                        Drawing.Primitives.DrawCircle(new UnityEngine.Vector2(pos.X, pos.Y), viewRadius, UnityEngine.Color.blue);
                    }
                }

                // Render events.
                {
                    var events = simulation.Events;
                    foreach (var ev in events)
                    {
                        var pos = simulation.RemapPosition2D(ev.Position, Vector3.Zero, kCanvas);
                        var radius = Math.Remap(ev.Radius, 0, simulation.WorldSize.X, 0, kWidth);

                        Drawing.Primitives.DrawCircle(new UnityEngine.Vector2(pos.X, pos.Y), radius, UnityEngine.Color.red);
                    }
                }

                GL.PopMatrix();
            }

            UnityEngine.RenderTexture.active = oldRT;
        }

        private void RenderUpdate()
        {
            if (DateTime.Now < nextUpdate)
                return;

            UpdateColors();

            nextUpdate = DateTime.Now;
            nextUpdate = nextUpdate.AddSeconds(1);

            RenderRoadsToTexture();

            RenderSimulation();
        }

        public override void OnGUI(bool _inputActive)
        {
            RenderUpdate();

            base.OnGUI(_inputActive);

            //Log.Out("Rendering UI");
            var rect = new Rect(0, 0, kWidth, kHeight);
            UnityEngine.GUI.DrawTexture(rect, roadsTexture);
            UnityEngine.GUI.DrawTexture(rect, previewTexturee);
        }
    }
}
