using System.Collections.Generic;
using System.Threading.Tasks;

namespace WalkerSim
{
    public enum RoadType : byte
    {
        None = 0,
        Asphalt,
        Offroad,
    }

    public class RoadGraph
    {
        public const int NodeSpacing = 8;
        private const int MinRoadPixels = 1;

        public struct RoadNode
        {
            public float X; // bitmap coordinates (centroid)
            public float Y;
            public RoadType Type;
            public int[] Connections;
        }

        public RoadNode[] Nodes = System.Array.Empty<RoadNode>();
        int[] _nodeGrid = System.Array.Empty<int>(); // grid cell → node index (-1 if none)
        int _gridColumns;
        int _gridRows;

        public int FindNearestNode(float bitmapX, float bitmapY, int searchRadius = 4)
        {
            int gridX = (int)(bitmapX / NodeSpacing);
            int gridY = (int)(bitmapY / NodeSpacing);

            int bestNode = -1;
            float bestDist = float.MaxValue;

            for (int dy = -searchRadius; dy <= searchRadius; dy++)
            {
                for (int dx = -searchRadius; dx <= searchRadius; dx++)
                {
                    int cx = gridX + dx;
                    int cy = gridY + dy;
                    if (cx < 0 || cx >= _gridColumns || cy < 0 || cy >= _gridRows)
                        continue;

                    int nodeIdx = _nodeGrid[cy * _gridColumns + cx];
                    if (nodeIdx == -1) continue;

                    var node = Nodes[nodeIdx];
                    float distX = node.X - bitmapX;
                    float distY = node.Y - bitmapY;
                    float dist = distX * distX + distY * distY;

                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestNode = nodeIdx;
                    }
                }
            }

            return bestNode;
        }

        /// <summary>
        /// Check if two adjacent grid cells have road pixels near their shared
        /// boundary. This handles curved roads correctly because it looks at where
        /// the road actually crosses between cells, not a straight centroid line.
        /// </summary>
        /// <param name="gxA">Grid X of cell A.</param>
        /// <param name="gyA">Grid Y of cell A.</param>
        /// <param name="gxB">Grid X of cell B (must be adjacent).</param>
        /// <param name="gyB">Grid Y of cell B (must be adjacent).</param>
        private static bool HasSharedBorderRoad(RoadType[,] data, int width, int height,
            int gxA, int gyA, int gxB, int gyB)
        {
            int dx = gxB - gxA; // -1, 0, or 1
            int dy = gyB - gyA;

            // For each direction, check a strip of pixels near the shared edge.
            // "margin" pixels inside each cell near the boundary.
            const int margin = 2;

            if (dx != 0 && dy == 0)
            {
                // Horizontal neighbor. Shared edge is a vertical line.
                // Check the rightmost columns of the left cell and leftmost of right.
                int leftGx = dx > 0 ? gxA : gxB;
                int rightGx = dx > 0 ? gxB : gxA;
                int edgeX = rightGx * NodeSpacing; // first pixel column of right cell

                int yStart = gyA * NodeSpacing;
                int yEnd = System.Math.Min(yStart + NodeSpacing, height);

                bool foundLeft = false, foundRight = false;
                for (int y = yStart; y < yEnd && (!foundLeft || !foundRight); y++)
                {
                    for (int m = 0; m < margin; m++)
                    {
                        int lx = edgeX - 1 - m;
                        if (lx >= 0 && lx < width && data[lx, y] != RoadType.None)
                            foundLeft = true;
                        int rx = edgeX + m;
                        if (rx >= 0 && rx < width && data[rx, y] != RoadType.None)
                            foundRight = true;
                    }
                }
                return foundLeft && foundRight;
            }
            else if (dx == 0 && dy != 0)
            {
                // Vertical neighbor. Shared edge is a horizontal line.
                int topGy = dy > 0 ? gyA : gyB;
                int bottomGy = dy > 0 ? gyB : gyA;
                int edgeY = bottomGy * NodeSpacing;

                int xStart = gxA * NodeSpacing;
                int xEnd = System.Math.Min(xStart + NodeSpacing, width);

                bool foundTop = false, foundBottom = false;
                for (int x = xStart; x < xEnd && (!foundTop || !foundBottom); x++)
                {
                    for (int m = 0; m < margin; m++)
                    {
                        int ty = edgeY - 1 - m;
                        if (ty >= 0 && ty < height && data[x, ty] != RoadType.None)
                            foundTop = true;
                        int by = edgeY + m;
                        if (by >= 0 && by < height && data[x, by] != RoadType.None)
                            foundBottom = true;
                    }
                }
                return foundTop && foundBottom;
            }
            else
            {
                // Diagonal neighbor. Check the corner area (margin x margin) of
                // each cell nearest to the shared corner point.
                int cornerX = (dx > 0 ? gxB : gxA) * NodeSpacing;
                int cornerY = (dy > 0 ? gyB : gyA) * NodeSpacing;

                bool foundA = false, foundB = false;

                // Cell A side of the corner.
                int axStart = dx > 0 ? cornerX - margin : cornerX;
                int ayStart = dy > 0 ? cornerY - margin : cornerY;
                int axEnd = System.Math.Min(axStart + margin, width);
                int ayEnd = System.Math.Min(ayStart + margin, height);
                for (int y = System.Math.Max(ayStart, 0); y < ayEnd && !foundA; y++)
                    for (int x = System.Math.Max(axStart, 0); x < axEnd && !foundA; x++)
                        if (data[x, y] != RoadType.None)
                            foundA = true;

                // Cell B side of the corner.
                int bxStart = dx > 0 ? cornerX : cornerX - margin;
                int byStart = dy > 0 ? cornerY : cornerY - margin;
                int bxEnd = System.Math.Min(bxStart + margin, width);
                int byEnd = System.Math.Min(byStart + margin, height);
                for (int y = System.Math.Max(byStart, 0); y < byEnd && !foundB; y++)
                    for (int x = System.Math.Max(bxStart, 0); x < bxEnd && !foundB; x++)
                        if (data[x, y] != RoadType.None)
                            foundB = true;

                return foundA && foundB;
            }
        }

        public static RoadGraph Build(RoadType[,] data, int width, int height)
        {
            var graph = new RoadGraph();
            graph._gridColumns = (width + NodeSpacing - 1) / NodeSpacing;
            graph._gridRows = (height + NodeSpacing - 1) / NodeSpacing;
            graph._nodeGrid = new int[graph._gridColumns * graph._gridRows];

            for (int i = 0; i < graph._nodeGrid.Length; i++)
                graph._nodeGrid[i] = -1;

            var nodes = new List<RoadNode>();

            // Pass 1: Create nodes at road pixel centroids per grid cell.
            for (int gy = 0; gy < graph._gridRows; gy++)
            {
                for (int gx = 0; gx < graph._gridColumns; gx++)
                {
                    float sumX = 0, sumY = 0;
                    int count = 0;
                    int asphaltCount = 0;

                    int startX = gx * NodeSpacing;
                    int startY = gy * NodeSpacing;
                    int endX = System.Math.Min(startX + NodeSpacing, width);
                    int endY = System.Math.Min(startY + NodeSpacing, height);

                    for (int y = startY; y < endY; y++)
                    {
                        for (int x = startX; x < endX; x++)
                        {
                            var roadType = data[x, y];
                            if (roadType != RoadType.None)
                            {
                                sumX += x;
                                sumY += y;
                                count++;
                                if (roadType == RoadType.Asphalt)
                                    asphaltCount++;
                            }
                        }
                    }

                    if (count >= MinRoadPixels)
                    {
                        var node = new RoadNode
                        {
                            X = sumX / count,
                            Y = sumY / count,
                            Type = asphaltCount > count / 2 ? RoadType.Asphalt : RoadType.Offroad,
                        };

                        graph._nodeGrid[gy * graph._gridColumns + gx] = nodes.Count;
                        nodes.Add(node);
                    }
                }
            }

            // Pass 2: Connect nodes in adjacent cells only if road pixels
            // form a continuous path between them.
            var connectionLists = new List<int>[nodes.Count];
            for (int i = 0; i < connectionLists.Length; i++)
                connectionLists[i] = new List<int>(4);

            for (int gy = 0; gy < graph._gridRows; gy++)
            {
                for (int gx = 0; gx < graph._gridColumns; gx++)
                {
                    int nodeIdx = graph._nodeGrid[gy * graph._gridColumns + gx];
                    if (nodeIdx == -1) continue;

                    var nodeA = nodes[nodeIdx];

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;

                            int nx = gx + dx;
                            int ny = gy + dy;
                            if (nx < 0 || nx >= graph._gridColumns || ny < 0 || ny >= graph._gridRows)
                                continue;

                            int neighborIdx = graph._nodeGrid[ny * graph._gridColumns + nx];
                            if (neighborIdx == -1) continue;

                            // For diagonal connections, skip if both intermediate
                            // cardinal cells have nodes — the path already exists
                            // through them and adding the diagonal creates a triangle.
                            if (dx != 0 && dy != 0)
                            {
                                int cardA = graph._nodeGrid[gy * graph._gridColumns + nx]; // (nx, gy)
                                int cardB = graph._nodeGrid[ny * graph._gridColumns + gx]; // (gx, ny)
                                if (cardA != -1 && cardB != -1)
                                    continue;
                            }

                            // Check that road pixels exist on both sides of the
                            // shared cell boundary (handles curves correctly).
                            if (HasSharedBorderRoad(data, width, height, gx, gy, nx, ny))
                                connectionLists[nodeIdx].Add(neighborIdx);
                        }
                    }
                }
            }

            // Finalize: convert connection lists to arrays.
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                node.Connections = connectionLists[i].ToArray();
                nodes[i] = node;
            }

            graph.Nodes = nodes.ToArray();

            int deadEnds = 0;
            int intersections = 0;
            for (int i = 0; i < graph.Nodes.Length; i++)
            {
                if (graph.Nodes[i].Connections.Length == 1) deadEnds++;
                else if (graph.Nodes[i].Connections.Length >= 3) intersections++;
            }

            Logging.Info("Road graph: {0} nodes ({1} dead ends, {2} intersections), grid {3}x{4}",
                graph.Nodes.Length, deadEnds, intersections, graph._gridColumns, graph._gridRows);

            return graph;
        }
    }

    public class Roads
    {
        // Downscaled to 768x768
        const int ScaledWidth = 768;
        const int ScaledHeight = 768;

        public const int CellSize = 32;

        RoadType[,] _data = new RoadType[0, 0];
        int _width = 0;
        int _height = 0;
        string _name = string.Empty;

        public string Name
        {
            get => _name;
        }

        public struct RoadPoint
        {
            public static RoadPoint Invalid = new RoadPoint()
            {
                Type = RoadType.None,
                X = -1,
                Y = -1,
            };

            public RoadType Type;
            public int X;
            public int Y;
        }

        public class Cell
        {
            public List<RoadPoint> Points;
        }

        Cell[] _roadGrid = new Cell[0];
        int _columnCount = 0;
        int _rowCount = 0;

        public int Width
        {
            get => _width;
        }

        public int Height
        {
            get => _height;
        }

        public int ColumnCount
        {
            get => _columnCount;
        }

        public int RowCount
        {
            get => _rowCount;
        }

        public RoadGraph Graph { get; private set; }

        public static Roads LoadFromFile(string splatPath)
        {
            Logging.Info("Loading roads from file: " + splatPath);

            using (var img = WalkerSim.Drawing.LoadFromFile(splatPath))
            {
                img.RemoveTransparency();

                return Roads.LoadFromBitmap(img, splatPath);
            }
        }

        private static Roads LoadFromBitmap(Drawing.IBitmap img, string name)
        {
            // Resize the image to 712x712
            var scaled = Drawing.Create(img, ScaledWidth, ScaledHeight);

            var height = scaled.Height;
            var width = scaled.Width;

            var data = new RoadType[width, height];

            scaled.LockPixels();
            Parallel.For(0, height * width, index =>
            {
                int y = index / width;
                int x = index % width;
                var pixel = scaled.GetPixel(x, y);
                if (pixel.R != 0)
                {
                    data[x, y] = RoadType.Asphalt;
                }
                else if (pixel.G != 0)
                {
                    data[x, y] = RoadType.Offroad;
                }
            });
            scaled.UnlockPixels();

            var res = new Roads();
            res._name = name;
            res._width = width;
            res._height = height;
            res._data = data;

            // Create the grid.
            res._columnCount = (int)MathEx.Ceiling((float)width / CellSize);
            res._rowCount = (int)MathEx.Ceiling((float)height / CellSize);
            res._roadGrid = new Cell[res._columnCount * res._rowCount];
            for (int i = 0; i < res._roadGrid.Length; i++)
            {
                res._roadGrid[i] = new Cell()
                {
                    Points = new List<RoadPoint>(),
                };
            }

            // Extract all the data and store it in the cells.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var roadType = data[x, y];
                    if (roadType == RoadType.None)
                        continue;

                    var cellX = x / CellSize;
                    var cellY = y / CellSize;
                    var cellIndex = cellY * res._columnCount + cellX;
                    var cell = res._roadGrid[cellIndex];

                    var point = new RoadPoint()
                    {
                        Type = roadType,
                        X = x,
                        Y = y,
                    };
                    cell.Points.Add(point);
                }
            }

            // Minimize the amount of points, we can delete every second point.
            for (int i = 0; i < res._roadGrid.Length; i++)
            {
                var cell = res._roadGrid[i];
                if (cell.Points.Count == 0)
                {
                    continue;
                }

                var newPoints = new List<RoadPoint>();
                for (int j = 0; j < cell.Points.Count; j++)
                {
                    if (j % 2 == 0)
                    {
                        newPoints.Add(cell.Points[j]);
                    }
                }

                cell.Points = newPoints;
            }

            // Build the road graph for node-to-node traversal.
            res.Graph = RoadGraph.Build(data, width, height);

            return res;
        }

        // Gets the pixel information for the given bitmap coordinates in the scaled image.
        public RoadType GetRoadType(int x, int y)
        {
            int index = y * _width + x;
            if (index < 0 || index >= _data.Length)
                return RoadType.None;

            return _data[x, y];
        }


        public Cell GetCell(int cellX, int cellY)
        {
            if (cellX < 0 || cellX >= _columnCount || cellY < 0 || cellY >= _rowCount)
            {
                return null;
            }

            var index = cellY * _columnCount + cellX;
            return _roadGrid[index];
        }

        // Get the closest road point to the given bitmap coordinates, the search distance is limited to
        // 3x3 grid of cells.
        public RoadPoint GetClosestRoad(int x, int y, int dirX, int dirY)
        {
            var bestChoice = RoadPoint.Invalid;
            var closest = RoadPoint.Invalid;

            var cellX = x / CellSize;
            var cellY = y / CellSize;
            float closestDist = float.MaxValue;
            // Define a view cone angle (e.g., 90 degrees total, 45 degrees each side)
            const float viewConeCosThreshold = 0.7071f; // cos(45°)

            // Normalize direction vector
            float dirLength = (float)System.Math.Sqrt(dirX * dirX + dirY * dirY);
            float normDirX = dirLength > 0 ? dirX / dirLength : 0;
            float normDirY = dirLength > 0 ? dirY / dirLength : 0;

            // Iterate through neighboring cells (3x3 grid)
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    var neighborCellX = cellX + offsetX;
                    var neighborCellY = cellY + offsetY;

                    // Ensure the neighboring cell is within grid bounds
                    if (neighborCellX >= 0 && neighborCellX < _columnCount &&
                        neighborCellY >= 0 && neighborCellY < _rowCount)
                    {
                        var cell = GetCell(neighborCellX, neighborCellY);

                        // Check each point in the neighboring cell
                        for (int i = 0; i < cell.Points.Count; i++)
                        {
                            var point = cell.Points[i];
                            if (point.X == x && point.Y == y)
                            {
                                continue;
                            }

                            // Vector from current position to road point
                            float toPointX = point.X - x;
                            float toPointY = point.Y - y;
                            float toPointLength = (float)System.Math.Sqrt(toPointX * toPointX + toPointY * toPointY);
                            if (toPointLength == 0)
                                continue;

                            var dist = toPointLength * toPointLength; // Use squared distance
                            if (dist < closestDist)
                            {
                                closestDist = dist;
                                closest = point;

                                // Normalize vector to road point
                                float normToPointX = toPointX / toPointLength;
                                float normToPointY = toPointY / toPointLength;

                                // Calculate dot product to check if point is within view cone
                                float dot = normDirX * normToPointX + normDirY * normToPointY;
                                if (dot >= viewConeCosThreshold) // Point is within view cone
                                {
                                    bestChoice = point;
                                }
                            }

                        }
                    }
                }
            }

            if (bestChoice.Type != RoadType.None)
            {
                // If we found a point within the view cone, return it
                return bestChoice;
            }

            return closest;
        }
    }
}
