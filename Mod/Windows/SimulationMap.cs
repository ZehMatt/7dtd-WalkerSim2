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

            // Clear once at start
            GL.Clear(true, true, UnityEngine.Color.black);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, roadsTexture.width, roadsTexture.height, 0);

            // Create texture with road data
            Texture2D roadTex = new Texture2D(roads.Width, roads.Height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[roads.Width * roads.Height];
            for (int y = 0; y < roads.Height; y++)
            {
                for (int x = 0; x < roads.Width; x++)
                {
                    var roadType = roads.GetRoadType(x, y);
                    int idx = y * roads.Width + x;
                    pixels[idx] = roadType == RoadType.None ? Color.clear :
                                 roadType == RoadType.Asphalt ? new Color(1, 1, 1, 0.4f) :
                                 new Color(1, 1, 1, 0.2f);
                }
            }
            roadTex.SetPixels(pixels);
            roadTex.Apply();

            // Draw the entire texture scaled to fit
            Graphics.DrawTexture(new Rect(0, 0, kWidth, kHeight), roadTex);
            UnityEngine.Object.Destroy(roadTex);

            GL.PopMatrix();
            UnityEngine.RenderTexture.active = oldRT;
            roadsRendered = true;
        }

        private void RenderSimulation()
        {
            var simulation = Simulation.Instance;

            var oldRT = UnityEngine.RenderTexture.active;

            UnityEngine.RenderTexture.active = previewTexturee;

            // Make background transparent.
            GL.Clear(true, true, UnityEngine.Color.clear);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, previewTexturee.width, previewTexturee.height, 0);

            {
                // Render inactive agents.
                {
                    Rendering.Primitives.BeginDrawPixels();

                    foreach (var agent in simulation.Agents)
                    {
                        if (agent.CurrentState != Agent.State.Wandering)
                            continue;

                        var pos = simulation.RemapPosition2D(agent.Position, Vector3.Zero, kCanvas);
                        Rendering.Primitives.DrawPixel(new UnityEngine.Vector2(pos.X, pos.Y), UnityEngine.Color.green);
                    }

                    Rendering.Primitives.EndDrawPixels();
                }

                // Render active agents.
                {
                    Rendering.Primitives.BeginDrawPixels();

                    foreach (var agent in simulation.Agents)
                    {
                        if (agent.CurrentState != Agent.State.Active)
                            continue;

                        var pos = simulation.RemapPosition2D(agent.Position, Vector3.Zero, kCanvas);
                        Rendering.Primitives.DrawPixel(new UnityEngine.Vector2(pos.X, pos.Y), UnityEngine.Color.red);
                    }

                    Rendering.Primitives.EndDrawPixels();
                }

                // Render players.
                {
                    var worldSize = simulation.WorldSize;
                    foreach (var kv in simulation.Players)
                    {
                        var player = kv.Value;

                        var pos = simulation.RemapPosition2D(player.Position, Vector3.Zero, kCanvas);
                        Rendering.Primitives.DrawFilledCircle(new UnityEngine.Vector2(pos.X, pos.Y), 2, UnityEngine.Color.blue);

                        // FIXME: We should maybe draw an ellipse with x and y remapped.
                        var viewRadius = MathEx.Remap(player.ViewRadius, 0, worldSize.X, 0, kWidth);
                        Rendering.Primitives.DrawCircle(new UnityEngine.Vector2(pos.X, pos.Y), viewRadius, UnityEngine.Color.blue);
                    }
                }

                // Render events.
                {
                    var events = simulation.Events;
                    foreach (var ev in events)
                    {
                        var pos = simulation.RemapPosition2D(ev.Position, Vector3.Zero, kCanvas);
                        var radius = MathEx.Remap(ev.Radius, 0, simulation.WorldSize.X, 0, kWidth);

                        Rendering.Primitives.DrawCircle(new UnityEngine.Vector2(pos.X, pos.Y), radius, UnityEngine.Color.red);
                    }
                }

            }

            GL.PopMatrix();

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
